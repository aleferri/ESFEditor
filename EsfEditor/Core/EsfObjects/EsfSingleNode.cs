namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Parser;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class EsfSingleNode : EsfNode, IEsfNode, IEsfValue
    {
        public EsfSingleNode(EsfParser outer)
        {
            base.Parser = outer;
            base.Values = new List<IEsfValue>();
        }

        public EsfSingleNode(IEsfNode esfNode, IEsfNode parent, EsfParser parser) : base(esfNode, parent, parser)
        {
            base.ContainsChanges = parent.ContainsChanges;
            base.TreeContainsChanges = parent.TreeContainsChanges;
            base.IsNew++;
        }

        public void AddChildren(int index, IEsfNode newNode)
        {
            base.Values.Insert(index, newNode);
        }

        public override IEsfNode CopyTo(IEsfNode destination, int index)
        {
            IEsfNode parent = destination;
            destination.ContainsChanges++;
            while (parent != null)
            {
                parent.TreeContainsChanges++;
                parent = parent.Parent;
            }
            IEsfNode newNode = new EsfSingleNode(this, destination, destination.Parser) {
                IsNew = destination.IsNew + 1
            };
            destination.AddChildren(index, newNode);
            return newNode;
        }

        public override void Delete(bool updateParents)
        {
            base.Delete(updateParents);
            foreach (IEsfValue value2 in base.Values)
            {
                if (!value2.IsBinaryType && !value2.IsValueType)
                {
                    ((IEsfNode) value2).Delete(false);
                }
            }
        }

        public List<IEsfNode> GetChildren()
        {
            if (!base.BeenParsed)
            {
                this.Parse();
            }
            List<IEsfNode> list = new List<IEsfNode>();
            foreach (EsfValue value2 in base.Values)
            {
                if ((value2.Type == EsfValueType.SingleNode) || (value2.Type == EsfValueType.PolyNode))
                {
                    list.Add((IEsfNode) value2);
                }
            }
            return list;
        }

        public bool HasSizeChanged()
        {
            if ((base.ContainsChanges != 0) || (base.TreeContainsChanges != 0))
            {
                foreach (EsfValue value2 in base.Values)
                {
                    if (value2.IsDeleted > 0)
                    {
                        return true;
                    }
                    if (value2.IsNew > 0)
                    {
                        return true;
                    }
                    switch (value2.Type)
                    {
                        case EsfValueType.UTF16:
                        case EsfValueType.Ascii:
                            if ((value2.OriginalValue == null) || (((string) value2.Value).Length == ((string) value2.OriginalValue).Length))
                            {
                                continue;
                            }
                            return true;

                        case EsfValueType.SingleNode:
                            if (!((EsfSingleNode) value2).HasSizeChanged())
                            {
                                continue;
                            }
                            return true;

                        case EsfValueType.PolyNode:
                            if (!((EsfPolyNode) value2).HasSizeChanged())
                            {
                                continue;
                            }
                            return true;
                    }
                }
            }
            return false;
        }

        public void Parse()
        {
            base.Parser.reader.BaseStream.Seek((long) base.Offset, SeekOrigin.Begin);
            base.Parser.reader.BaseStream.Seek(4L, SeekOrigin.Current);
            base.OffsetEnd = base.Parser.reader.ReadUInt32();
            while (base.Parser.reader.BaseStream.Position < base.OffsetEnd)
            {
                base.Values.Add(base.Parser.ValueParser(this));
            }
            base.BeenParsed = true;
        }

        public void ParseDeep()
        {
            foreach (IEsfNode node in this.GetChildren())
            {
                node.ParseDeep();
            }
        }

        public override void QuickSave()
        {
            if ((base.TreeContainsChanges != 0) || (base.ContainsChanges != 0))
            {
                base.TreeContainsChanges = 0;
                base.ContainsChanges = 0;
                foreach (EsfValue value2 in base.Values)
                {
                    value2.QuickSave();
                }
            }
        }

        public override void SlowSave()
        {
            if (base.IsDeleted <= 0)
            {
                uint position = (uint) base.Parser.writer.BaseStream.Position;
                uint offset = base.Offset;
                base.Parser.reader.BaseStream.Seek((long) base.Offset, SeekOrigin.Begin);
                long num3 = position - offset;
                if (!base.BeenParsed)
                {
                    base.Parser.writer.Write(base.Parser.reader.ReadUInt32());
                    uint endOffsetSource = base.Parser.reader.ReadUInt32();
                    base.Parser.writer.Write((uint) (endOffsetSource + ((uint) num3)));
                    base.Parser.SaveUnparsedPart(endOffsetSource);
                }
                else
                {
                    base.Parser.writer.Write((byte) base.Type);
                    ushort nodeNameIndex = base.Parser.GetNodeNameIndex(base.Name);
                    base.Parser.writer.Write(nodeNameIndex);
                    base.Parser.writer.Write(base.Unknown);
                    base.Parser.reader.BaseStream.Seek(8L, SeekOrigin.Current);
                    base.Parser.writer.BaseStream.Seek(4L, SeekOrigin.Current);
                    foreach (EsfValue value2 in base.Values)
                    {
                        value2.SlowSave();
                    }
                    uint num6 = (uint) base.Parser.writer.BaseStream.Position;
                    base.Parser.writer.BaseStream.Seek((long) (position + 4), SeekOrigin.Begin);
                    base.Parser.writer.Write(num6);
                    base.Parser.writer.BaseStream.Seek((long) num6, SeekOrigin.Begin);
                }
                base.IsNew = 0;
                base.IsDeleted = 0;
                base.ContainsChanges = 0;
                base.TreeContainsChanges = 0;
            }
        }

        public override void UnDelete(bool updateParents)
        {
            base.UnDelete(updateParents);
            foreach (IEsfValue value2 in base.Values)
            {
                if (!value2.IsBinaryType && !value2.IsValueType)
                {
                    ((IEsfNode) value2).UnDelete(false);
                }
            }
        }

        public override void UndoCopy()
        {
            for (IEsfNode node = base.Parent; node != null; node = node.Parent)
            {
                node.TreeContainsChanges -= base.TreeContainsChanges;
                node.TreeContainsChanges--;
            }
            base.Parent.GetChildren().Remove(this);
        }
    }
}

