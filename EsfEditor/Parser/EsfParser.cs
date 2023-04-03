namespace EsfEditor.Parser
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Core.EsfObjects;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    public class EsfParser : IDisposable
    {
        private BackgroundWorker backgroundWorker;
        private readonly string filename;
        private FileStream fs;
        private EsfHeader header;

        public StringCollection nodeNames;
        public BinaryReader reader;
        public bool readOnly;
        public IEsfNode root;
        public BinaryWriter writer;

        private int percentageDone;

        private EsfParser()
        {
        }

        public EsfParser(string filename)
        {
            this.filename = filename;
            try
            {
                this.fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                this.readOnly = false;
            }
            catch (UnauthorizedAccessException)
            {
                this.fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                this.readOnly = true;
            }
            this.reader = new BinaryReader(this.fs);
            this.header = new EsfHeader();
            this.header.magic = (EsfType)this.reader.ReadUInt32();
            switch (this.header.magic)
            {
                case EsfType.ABCD:
                    break;

                case EsfType.ABCE:
                    this.header.unknown1 = this.reader.ReadUInt32();
                    this.header.unknown2 = this.reader.ReadUInt32();
                    break;

                default:
                    throw new Exception("Unsupported ESF type");
            }
            this.header.offsetNodeNames = this.reader.ReadUInt32();
            long position = this.reader.BaseStream.Position;
            this.reader.BaseStream.Seek((long)this.header.offsetNodeNames, SeekOrigin.Begin);
            this.nodeNames = this.ParseNodeNames();
            this.reader.BaseStream.Seek(position, SeekOrigin.Begin);
            this.root = (IEsfNode)this.ValueParser(null);
        }

        public void Dispose()
        {
            this.reader?.Close();
            this.writer?.Close();
            this.fs?.Close();
        }

        public List<byte[]> GetBinaryData(IEsfNode node)
        {
            List<byte[]> list = new List<byte[]>();
            if (!node.BeenParsed)
            {
                node.Parse();
            }
            this.reader.BaseStream.Seek((long)node.Offset, SeekOrigin.Begin);
            try
            {
                byte[] buffer;
                int count;
                if ((node.OffsetEnd - node.Offset) > 0x7fffffff)
                {
                    uint num2 = node.OffsetEnd - node.Offset;
                    while (num2 > 0)
                    {
                        if (num2 > 0x7fffffff)
                        {
                            count = 0x7fffffff;
                            num2 -= 0x7fffffff;
                        }
                        else
                        {
                            count = (int)num2;
                            num2 = 0;
                        }
                        buffer = new byte[count];
                        this.reader.BaseStream.Read(buffer, 0, count);
                        list.Add(buffer);
                    }
                    return list;
                }
                count = (int)(node.OffsetEnd - node.Offset);
                buffer = new byte[count];
                this.reader.BaseStream.Read(buffer, 0, count);
                list.Add(buffer);
            }
            catch (Exception)
            {
            }
            return list;
        }

        public ushort GetNodeNameIndex(string nodeName)
        {
            int index = this.nodeNames.IndexOf(nodeName);
            if (index == -1)
            {
                index = this.nodeNames.Add(nodeName);
            }
            return (ushort)index;
        }

        public void ParseAll()
        {
        }

        private StringCollection ParseNodeNames()
        {
            ushort num = this.reader.ReadUInt16();
            StringCollection strings = new StringCollection();
            for (ushort i = 0; i < num; i = (ushort) (i + 1))
            {
                ushort count = this.reader.ReadUInt16();
                strings.Add(Encoding.ASCII.GetString(this.reader.ReadBytes(count)));
            }
            return strings;
        }


        public void Save(BackgroundWorker bw)
        {
            this.backgroundWorker = bw;
            this.percentageDone = 0;
            if (!this.root.HasSizeChanged())
            {
                this.writer = new BinaryWriter(this.fs);
                this.root.QuickSave();
            }
            else
            {
                string tempFileName = Path.GetTempFileName();
                this.SaveAs(tempFileName, bw);
                this.reader.Close();
                File.Delete(this.filename);
                File.Move(tempFileName, this.filename);
                try
                {
                    this.fs = File.Open(this.filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    this.readOnly = false;
                }
                catch (UnauthorizedAccessException)
                {
                    this.fs = File.Open(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    this.readOnly = true;
                }
                this.reader = new BinaryReader(this.fs);
            }
        }

        public void SaveAs(string newFilename, BackgroundWorker bw)
        {
            this.backgroundWorker = bw;
            this.percentageDone = 0;
            if (!this.root.HasSizeChanged())
            {
                File.Copy(this.filename, newFilename, true);
                this.fs = File.Open(newFilename, FileMode.Open, FileAccess.Write, FileShare.None);
                this.writer = new BinaryWriter(this.fs);
                this.root.QuickSave();
                this.writer.Close();
            }
            else
            {
                this.fs = File.Open(newFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                this.writer = new BinaryWriter(this.fs);
                this.writer.Write((uint)this.header.magic);
                if (this.header.magic == EsfType.ABCE)
                {
                    this.writer.Write(this.header.unknown1);
                    this.writer.Write(this.header.unknown2);
                }
                this.writer.BaseStream.Seek(4L, SeekOrigin.Current);
                this.root.SlowSave();
                long position = this.writer.BaseStream.Position;
                if (this.header.magic == EsfType.ABCE)
                {
                    this.writer.BaseStream.Seek(12L, SeekOrigin.Begin);
                }
                else
                {
                    this.writer.BaseStream.Seek(4L, SeekOrigin.Begin);
                }
                this.writer.Write((uint)position);
                this.writer.BaseStream.Seek(position, SeekOrigin.Begin);
                this.writer.Write((ushort)this.nodeNames.Count);
                foreach (string str in this.nodeNames)
                {
                    this.writer.Write((ushort)str.Length);
                    this.writer.Write(Encoding.ASCII.GetBytes(str));
                }
                this.writer.Close();
            }
        }

        public void SaveExport(string newFilename, IEsfNode node, BackgroundWorker bw)
        {
            this.backgroundWorker = bw;
            this.fs = File.Open(newFilename, FileMode.Create, FileAccess.Write, FileShare.None);
            this.writer = new BinaryWriter(this.fs);
            this.writer.Write((uint)this.header.magic);
            if (this.header.magic == EsfType.ABCE)
            {
                this.writer.Write(this.header.unknown1);
                this.writer.Write(this.header.unknown2);
            }
            this.writer.BaseStream.Seek(4L, SeekOrigin.Current);
            node.SlowSave();
            long position = this.writer.BaseStream.Position;
            if (this.header.magic == EsfType.ABCE)
            {
                this.writer.BaseStream.Seek(12L, SeekOrigin.Begin);
            }
            else
            {
                this.writer.BaseStream.Seek(4L, SeekOrigin.Begin);
            }
            this.writer.Write((uint)position);
            this.writer.BaseStream.Seek(position, SeekOrigin.Begin);
            this.writer.Write((ushort)this.nodeNames.Count);
            foreach (string str in this.nodeNames)
            {
                this.writer.Write((ushort)str.Length);
                this.writer.Write(Encoding.ASCII.GetBytes(str));
            }
            this.writer.Close();
            this.fs.Close();
            this.fs = (FileStream)this.reader.BaseStream;
        }

        public void SaveUnparsedPart(uint endOffsetSource)
        {
            int num = (int)((this.reader.BaseStream.Position * 100L) / this.reader.BaseStream.Length);
            if (num > this.percentageDone)
            {
                this.percentageDone = num;
                if (this.backgroundWorker.WorkerReportsProgress)
                {
                    this.backgroundWorker.ReportProgress(this.percentageDone);
                }
            }
            try
            {
                while (this.reader.BaseStream.Position < endOffsetSource)
                {
                    uint num8;
                    uint num9;
                    int num10;
                    long num2 = this.writer.BaseStream.Position - this.reader.BaseStream.Position;
                    EsfValueType type = (EsfValueType)this.reader.ReadByte();
                    this.writer.Write((byte)type);
                    switch (type)
                    {
                        case EsfValueType.Short:
                        case EsfValueType.UInt16:
                        case EsfValueType.UShort:
                            {
                                this.writer.Write(this.reader.ReadBytes(2));
                                continue;
                            }
                        case EsfValueType.Boolean:
                        case EsfValueType.Byte:
                            {
                                this.writer.Write(this.reader.ReadByte());
                                continue;
                            }
                        case EsfValueType.Int:
                        case EsfValueType.UInt:
                        case EsfValueType.Float:
                            {
                                this.writer.Write(this.reader.ReadBytes(4));
                                continue;
                            }
                        case EsfValueType.FloatPoint:
                            {
                                this.writer.Write(this.reader.ReadBytes(8));
                                continue;
                            }
                        case EsfValueType.FloatPoint3D:
                            {
                                this.writer.Write(this.reader.ReadBytes(8));
                                continue;
                            }
                        case EsfValueType.UTF16:
                            {
                                ushort num5 = this.reader.ReadUInt16();
                                this.writer.Write(num5);
                                this.writer.Write(this.reader.ReadBytes(num5 * 2));
                                continue;
                            }
                        case EsfValueType.Ascii:
                            {
                                ushort num4 = this.reader.ReadUInt16();
                                this.writer.Write(num4);
                                this.writer.Write(this.reader.ReadBytes(num4));
                                continue;
                            }
                        case EsfValueType.Binary41:
                        case EsfValueType.Binary42:
                        case EsfValueType.Binary43:
                        case EsfValueType.Binary44:
                        case EsfValueType.Binary45:
                        case EsfValueType.Binary46:
                        case EsfValueType.Binary47:
                        case EsfValueType.Binary48:
                        case EsfValueType.Binary49:
                        case EsfValueType.Binary4A:
                        case EsfValueType.Binary4B:
                        case EsfValueType.Binary4C:
                        case EsfValueType.Binary4D:
                            {
                                uint num3 = this.reader.ReadUInt32();
                                this.writer.Write((uint)(num3 + ((uint)num2)));
                                this.writer.Write(this.reader.ReadBytes((int)(num3 - this.reader.BaseStream.Position)));
                                continue;
                            }
                        case EsfValueType.SingleNode:
                            {
                                this.writer.Write(this.reader.ReadBytes(3));
                                uint num6 = this.reader.ReadUInt32();
                                this.writer.Write((uint)(num6 + ((uint)num2)));
                                this.SaveUnparsedPart(num6);
                                continue;
                            }
                        case EsfValueType.PolyNode:
                            {
                                this.writer.Write(this.reader.ReadBytes(3));
                                uint num7 = this.reader.ReadUInt32();
                                this.writer.Write((uint)(num7 + ((uint)num2)));
                                num8 = this.reader.ReadUInt32();
                                this.writer.Write(num8);
                                num10 = 0;
                                goto Label_0326;
                            }
                        default:
                            throw new Exception("Encountered unknown value type at 0x" + this.reader.BaseStream.Position.ToString("x8"));
                    }
                Label_02FA:
                    num9 = this.reader.ReadUInt32();
                    this.writer.Write((uint)(num9 + ((uint)num2)));
                    this.SaveUnparsedPart(num9);
                    num10++;
                Label_0326:
                    if (num10 < num8)
                    {
                        goto Label_02FA;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public IEsfValue ValueParser(IEsfNode parent)
        {
            IEsfValue value2 = new EsfValue(this);
            EsfValueType type = (EsfValueType)this.reader.ReadByte();
            uint num = ((uint)this.reader.BaseStream.Position) - 1;
            switch (type)
            {
                case EsfValueType.Short:
                    value2.Value = this.reader.ReadInt16();
                    break;

                case EsfValueType.Boolean:
                    value2.Value = this.reader.ReadBoolean();
                    break;

                case EsfValueType.Int:
                    value2.Value = this.reader.ReadInt32();
                    break;

                case EsfValueType.Byte:
                    value2.Value = this.reader.ReadByte();
                    break;

                case EsfValueType.UInt16:
                    value2.Value = this.reader.ReadUInt16();
                    break;

                case EsfValueType.UInt:
                    value2.Value = this.reader.ReadUInt32();
                    break;

                case EsfValueType.Float:
                    value2.Value = this.reader.ReadSingle();
                    break;

                case EsfValueType.FloatPoint:
                    {
                        EsfFloatPoint point = new EsfFloatPoint
                        {
                            x = this.reader.ReadSingle(),
                            y = this.reader.ReadSingle()
                        };
                        value2.Value = point;
                        break;
                    }

                case EsfValueType.FloatPoint3D:
                    {
                        EsfFloatPoint3D point = new EsfFloatPoint3D();
                        point.x = this.reader.ReadSingle();
                        point.y = this.reader.ReadSingle();
                        point.z = this.reader.ReadSingle();
                        value2.Value = point;
                        break;
                    }

                case EsfValueType.UTF16:
                    {
                        ushort num4 = this.reader.ReadUInt16();
                        value2.Value = Encoding.Unicode.GetString(this.reader.ReadBytes(num4 * 2));
                        break;
                    }
                case EsfValueType.Ascii:
                    {
                        ushort count = this.reader.ReadUInt16();
                        value2.Value = Encoding.ASCII.GetString(this.reader.ReadBytes(count));
                        break;
                    }
                case EsfValueType.UShort:
                    value2.Value = this.reader.ReadUInt16();
                    break;

                case EsfValueType.Binary41:
                case EsfValueType.Binary42:
                case EsfValueType.Binary43:
                case EsfValueType.Binary44:
                case EsfValueType.Binary45:
                case EsfValueType.Binary46:
                case EsfValueType.Binary47:
                case EsfValueType.Binary48:
                case EsfValueType.Binary49:
                case EsfValueType.Binary4A:
                case EsfValueType.Binary4B:
                case EsfValueType.Binary4C:
                case EsfValueType.Binary4D:
                    {
                        int num2 = (int)(this.reader.ReadUInt32() - ((uint)this.reader.BaseStream.Position));
                        byte[] buffer = this.reader.ReadBytes(num2);
                        value2.Value = buffer;
                        break;
                    }
                case EsfValueType.SingleNode:
                    {
                        value2 = new EsfSingleNode(this);
                        ((EsfNode)value2).Name = this.nodeNames[this.reader.ReadUInt16()];
                        ((EsfNode)value2).Unknown = this.reader.ReadByte();
                        this.reader.BaseStream.Seek((long)this.reader.ReadUInt32(), SeekOrigin.Begin);
                        if (parent != null)
                        {
                            value2.IsDeleted = parent.IsDeleted;
                            value2.IsNew = parent.IsNew;
                        }
                        break;
                    }

                case EsfValueType.PolyNode:
                    {
                        value2 = new EsfPolyNode(this);
                        ((EsfNode)value2).Name = this.nodeNames[this.reader.ReadUInt16()];
                        ((EsfNode)value2).Unknown = this.reader.ReadByte();
                        this.reader.BaseStream.Seek((long)this.reader.ReadUInt32(), SeekOrigin.Begin);
                        if (parent != null)
                        {
                            value2.IsDeleted = parent.IsDeleted;
                            value2.IsNew = parent.IsNew;
                        }
                        break;
                    }

                default:
                    throw new Exception("Unsupported value type: " + ((byte)type).ToString() + " at: " + Convert.ToString((long)value2.Offset, 0x10));
            }
            value2.Parent = parent;
            value2.Offset = num;
            value2.Type = type;
            return value2;
        }
    }
}

