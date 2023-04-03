namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Parser;
    using System;
    using System.IO;
    using System.Text;

    public class EsfValue : IEsfValue
    {
        private string desc;
        private int isDeleted;
        private int isNew;
        private uint offset;
        private object originalValue;
        private IEsfNode parent;
        private EsfParser parser;
        private EsfValueType type;
        private object value;

        public EsfValue()
        {
            this.desc = string.Empty;
        }

        public EsfValue(EsfParser outer)
        {
            this.desc = string.Empty;
            this.parser = outer;
        }

        public EsfValue(IEsfValue esfValue, IEsfNode parent, EsfParser parser)
        {
            this.desc = string.Empty;
            this.Offset = esfValue.Offset;
            this.Type = esfValue.Type;
            this.Value = esfValue.Value;
            this.IsNew = esfValue.IsNew;
            this.IsDeleted = esfValue.IsDeleted;
            this.OriginalValue = esfValue.OriginalValue;
            this.Description = esfValue.Description;
            this.Parent = parent;
            this.Parser = parser;
        }

        public void ChangeValue(string oldValue)
        {
            if (this.originalValue == null)
            {
                this.originalValue = oldValue;
                IEsfNode parent = this.parent;
                parent.ContainsChanges++;
                while (parent != null)
                {
                    parent.TreeContainsChanges++;
                    parent = parent.Parent;
                }
            }
        }

        public virtual void QuickSave()
        {
            if (this.originalValue != null)
            {
                this.originalValue = null;
                this.parser.writer.BaseStream.Seek((long) (this.offset + 1), SeekOrigin.Begin);
                switch (this.type)
                {
                    case EsfValueType.Short:
                        this.parser.writer.Write(short.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.Boolean:
                        this.parser.writer.Write(bool.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.Int:
                        this.parser.writer.Write(int.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.Byte:
                        this.parser.writer.Write(byte.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.UInt16:
                        this.parser.writer.Write(ushort.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.UInt:
                        this.parser.writer.Write(uint.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.Float:
                        this.parser.writer.Write(float.Parse(this.value.ToString()));
                        return;

                    case EsfValueType.FloatPoint:
                        this.parser.writer.Write(EsfFloatPoint.Parse(this.value.ToString()).x);
                        this.parser.writer.Write(EsfFloatPoint.Parse(this.value.ToString()).y);
                        return;

                    case EsfValueType.FloatPoint3D:
                        this.parser.writer.Write(EsfFloatPoint3D.Parse(this.value.ToString()).x);
                        this.parser.writer.Write(EsfFloatPoint3D.Parse(this.value.ToString()).y);
                        this.parser.writer.Write(EsfFloatPoint3D.Parse(this.value.ToString()).z);
                        return;

                    case EsfValueType.UTF16:
                        this.parser.writer.Seek(2, SeekOrigin.Current);
                        this.parser.writer.Write(Encoding.Unicode.GetBytes(this.value.ToString()));
                        return;

                    case EsfValueType.Ascii:
                        this.parser.writer.Seek(2, SeekOrigin.Current);
                        this.parser.writer.Write(Encoding.ASCII.GetBytes(this.value.ToString()));
                        return;

                    case EsfValueType.UShort:
                        this.parser.writer.Write(ushort.Parse(this.value.ToString()));
                        return;

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
                        this.parser.writer.Seek(4, SeekOrigin.Current);
                        this.parser.writer.Write((byte[]) this.value);
                        return;

                    case EsfValueType.SingleNode:
                        throw new Exception("Shouldn't find a singlenode here");

                    case EsfValueType.PolyNode:
                        throw new Exception("Shouldn't find a multinode here");
                }
                throw new Exception("Found an unexpected type");
            }
        }

        public virtual void SlowSave()
        {
            this.originalValue = null;
            this.parser.writer.Write((byte) this.type);
            switch (this.type)
            {
                case EsfValueType.Short:
                    this.parser.writer.Write(short.Parse(this.value.ToString()));
                    return;

                case EsfValueType.Boolean:
                    this.parser.writer.Write(bool.Parse(this.value.ToString()));
                    return;

                case EsfValueType.Int:
                    this.parser.writer.Write(int.Parse(this.value.ToString()));
                    return;

                case EsfValueType.Byte:
                    this.parser.writer.Write(byte.Parse(this.value.ToString()));
                    return;

                case EsfValueType.UInt16:
                    this.parser.writer.Write(ushort.Parse(this.value.ToString()));
                    return;

                case EsfValueType.UInt:
                    this.parser.writer.Write(uint.Parse(this.value.ToString()));
                    return;

                case EsfValueType.Float:
                    this.parser.writer.Write(float.Parse(this.value.ToString()));
                    return;

                case EsfValueType.FloatPoint:
                    this.parser.writer.Write(EsfFloatPoint.Parse(this.value.ToString()).x);
                    this.parser.writer.Write(EsfFloatPoint.Parse(this.value.ToString()).y);
                    return;

                case EsfValueType.FloatPoint3D:
                    this.parser.writer.Write(EsfFloatPoint3D.Parse(this.value.ToString()).x);
                    this.parser.writer.Write(EsfFloatPoint3D.Parse(this.value.ToString()).y);
                    this.parser.writer.Write(EsfFloatPoint3D.Parse(this.value.ToString()).y);
                    return;

                case EsfValueType.UTF16:
                {
                    int length = ((string) this.value).Length;
                    this.parser.writer.Write((ushort) length);
                    this.parser.writer.Write(Encoding.Unicode.GetBytes(this.value.ToString()));
                    return;
                }
                case EsfValueType.Ascii:
                {
                    int num = ((string) this.value).Length;
                    this.parser.writer.Write((ushort) num);
                    this.parser.writer.Write(Encoding.ASCII.GetBytes(this.value.ToString()));
                    return;
                }
                case EsfValueType.UShort:
                    this.parser.writer.Write(ushort.Parse(this.value.ToString()));
                    return;

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
                    long position = this.parser.writer.BaseStream.Position;
                    int num4 = ((byte[]) this.value).Length;
                    this.parser.writer.Write((uint) ((position + 4L) + num4));
                    this.parser.writer.Write((byte[]) this.value);
                    return;
                }

                case EsfValueType.SingleNode:
                    throw new Exception("Shouldn't find a singlenode here");

                case EsfValueType.PolyNode:
                  throw new Exception("Shouldn't find a multinode here");

                //case EsfValueType.SingleNode:
                //{
                //    this.parser.writer.Write(this.reader.ReadBytes(3));
                //    uint num6 = this.reader.ReadUInt32();
                //    this.parser.writer.Write((uint)(num6 + ((uint)num2)));
                //    this.SaveUnparsedPart(num6);
                //    continue;
                //}
                //case EsfValueType.PolyNode:
                //{
                //    this.parser.writer.Write(this.reader.ReadBytes(3));
                //    uint num7 = this.reader.ReadUInt32();
                //    this.parser.writer.Write((uint)(num7 + ((uint)num2)));
                //    uint num8 = this.reader.ReadUInt32();
                //    this.parser.writer.Write(num8);
                //    var num10 = 0;
                //    goto Label_0326;
                //}
                default:
                        throw new Exception("Found an unexpected type");
            }
        //Label_02FA:
        //    num9 = this.reader.ReadUInt32();
        //    this.writer.Write((uint)(num9 + ((uint)num2)));
        //    this.SaveUnparsedPart(num9);
        //    num10++;
        //Label_0326:
        //    if (num10 < num8)
        //    {
        //        goto Label_02FA;
        //    }            
        //    catch (Exception)
        //    {
        //    }
        }

        public void UndoChangeValue()
        {
            if (this.originalValue != null)
            {
                this.value = this.originalValue;
                this.originalValue = null;
                this.parent.ContainsChanges--;
                while (this.parent != null)
                {
                    this.parent.TreeContainsChanges--;
                    this.parent = this.parent.Parent;
                }
            }
        }

        public bool ValidateNewValue(string newValue)
        {
            bool flag;
            try
            {
                switch (this.type)
                {
                    case EsfValueType.Short:
                        this.value = short.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.Boolean:
                        this.value = bool.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.Int:
                        this.value = int.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.Byte:
                        this.value = byte.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.UInt16:
                        this.value = ushort.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.UInt:
                        this.value = uint.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.Float:
                        this.value = float.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.FloatPoint:
                        this.value = EsfFloatPoint.Parse(newValue);
                        goto Label_01DC;

                    case EsfValueType.UTF16:
                        this.value = newValue;
                        goto Label_01DC;

                    case EsfValueType.Ascii:
                        if (Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(newValue)) != newValue)
                        {
                            throw new FormatException("ASCII string can't contain extended characters");
                        }
                        break;

                    case EsfValueType.UShort:
                        this.value = ushort.Parse(newValue);
                        goto Label_01DC;

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
                        throw new FormatException("Can't edit binary data");

                    case EsfValueType.SingleNode:
                        throw new FormatException("Can't edit nodes");

                    case EsfValueType.PolyNode:
                        throw new FormatException("Can't edit nodes");

                    default:
                        throw new Exception("Unsupported value type: " + this.type.ToString());
                }
                this.value = newValue;
            Label_01DC:
                flag = true;
            }
            catch (ArgumentNullException)
            {
                flag = false;
            }
            catch (FormatException)
            {
                flag = false;
            }
            catch (OverflowException)
            {
                flag = false;
            }
            return flag;
        }

        public string Description
        {
            get
            {
                return this.desc;
            }
            set
            {
                this.desc = value;
            }
        }

        public bool IsBinaryType
        {
            get
            {
                switch (this.type)
                {
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
                        return true;
                }
                return false;
            }
        }

        public int IsDeleted
        {
            get
            {
                return this.isDeleted;
            }
            set
            {
                this.isDeleted = value;
            }
        }

        public int IsNew
        {
            get
            {
                return this.isNew;
            }
            set
            {
                this.isNew = value;
            }
        }

        public bool IsValueType
        {
            get
            {
                return ((this.type != EsfValueType.SingleNode) && (this.type != EsfValueType.PolyNode));
            }
        }

        public uint Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                this.offset = value;
            }
        }

        public object OriginalValue
        {
            get
            {
                return this.originalValue;
            }
            set
            {
                this.originalValue = value;
            }
        }

        public IEsfNode Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public EsfParser Parser
        {
            get
            {
                return this.parser;
            }
            set
            {
                this.parser = value;
            }
        }

        public EsfValueType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

