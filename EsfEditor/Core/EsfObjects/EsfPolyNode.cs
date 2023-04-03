namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Parser;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class EsfPolyNode : EsfNode, IEsfNode, IEsfValue
    {
        public List<EsfMultiNode> nodes;

        public EsfPolyNode(EsfParser outer)
        {
            this.nodes = new List<EsfMultiNode>();
            this.nodes = new List<EsfMultiNode>();
            base.Parser = outer;
        }

        public EsfPolyNode(IEsfNode esfNode, IEsfNode parent, EsfParser parser) : base(esfNode, parent, parser)
        {
            this.nodes = new List<EsfMultiNode>();
            EsfPolyNode nodes = (EsfPolyNode) esfNode;
            base.ContainsChanges = parent.ContainsChanges;
            base.TreeContainsChanges = parent.TreeContainsChanges;
            base.IsNew++;
            foreach (EsfMultiNode node in nodes.nodes)
            {
                this.nodes.Add(new EsfMultiNode(node, this, parser));
            }
        }

        public void AddChildren(int index, IEsfNode newNode)
        {
            if (newNode is EsfMultiNode)
            {
                this.nodes.Insert(index, (EsfMultiNode) newNode);
            }
            else
            {
                base.Values.Insert(index, newNode);
            }
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
            IEsfNode newNode = new EsfPolyNode(this, destination, destination.Parser) {
                IsNew = destination.IsNew + 1
            };
            destination.AddChildren(index, newNode);
            return newNode;
        }

        public override void Delete(bool updateParents)
        {
            base.Delete(updateParents);
            foreach (EsfMultiNode node in this.nodes)
            {
                node.Delete(false);
            }
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
            foreach (EsfMultiNode node in this.nodes)
            {
                list.Add(node);
            }
            return list;
        }

        public override List<IEsfValue> GetValues()
        {
            return new List<IEsfValue>();
        }

        public bool HasSizeChanged()
        {
            if ((base.ContainsChanges != 0) || (base.TreeContainsChanges != 0))
            {
                foreach (EsfMultiNode node in this.nodes)
                {
                    if (node.IsDeleted > 0)
                    {
                        return true;
                    }
                    if (node.IsNew > 0)
                    {
                        return true;
                    }
                    if (node.HasSizeChanged())
                    {
                        return true;
                    }
                }
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
            for (uint i = base.Parser.reader.ReadUInt32(); base.Parser.reader.BaseStream.Position < base.OffsetEnd; i--)
            {
    //            TitleParser vP = new TitleParser();
                EsfMultiNode item = new EsfMultiNode(base.Parser) {
                    Offset = (uint) base.Parser.reader.BaseStream.Position,
                    Parent = this,
                    Name = base.Name,
        //            Title =base.Title,
                    IsDeleted = base.IsDeleted,
                    IsNew = base.IsNew
                };
                this.nodes.Add(item);
                base.Parser.reader.BaseStream.Seek((long) base.Parser.reader.ReadUInt32(), SeekOrigin.Begin);
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
                foreach (EsfValue value2 in this.nodes)
                {
                    value2.QuickSave();
                }
            }
        }

        public override void SlowSave()
        {
            if (base.IsDeleted <= 0)
            {
                if (!base.BeenParsed)
                {
                    uint offset = base.Offset;
                    base.Parser.reader.BaseStream.Seek((long) (base.Offset + 4), SeekOrigin.Begin);
                    uint endOffsetSource = base.Parser.reader.ReadUInt32();
                    base.Parser.reader.BaseStream.Seek(-8L, SeekOrigin.Current);
                    base.Parser.SaveUnparsedPart(endOffsetSource);
                }
                else
                {
                    base.Parser.writer.Write((byte) base.Type);
                    ushort nodeNameIndex = base.Parser.GetNodeNameIndex(base.Name);
                    base.Parser.writer.Write(nodeNameIndex);
                    base.Parser.writer.Write(base.Unknown);
                    uint position = (uint) base.Parser.writer.BaseStream.Position;
                    base.Parser.writer.BaseStream.Seek(8L, SeekOrigin.Current);
                    uint num4 = 0;
                    foreach (EsfMultiNode node in this.nodes)
                    {
                        if (node.IsDeleted == 0)
                        {
                            num4++;
                        }
                        node.SlowSave();
                    }
                    uint num5 = (uint) base.Parser.writer.BaseStream.Position;
                    base.Parser.writer.BaseStream.Seek((long) position, SeekOrigin.Begin);
                    base.Parser.writer.Write(num5);
                    base.Parser.writer.Write(num4);
                    base.Parser.writer.BaseStream.Seek((long) num5, SeekOrigin.Begin);
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
            foreach (EsfMultiNode node in this.nodes)
            {
                node.UnDelete(false);
            }
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

