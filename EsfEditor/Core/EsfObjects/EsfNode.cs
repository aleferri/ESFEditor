namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Parser;
    using System;
    using System.Collections.Generic;

    public class EsfNode : EsfValue
    {
        private bool beenParsed;
        private int containsChanges;
        private string name;
        private uint offsetEnd;
        private int treeContainsChanges;
        private byte unknown;
        private string title;
        private List<IEsfValue> values;

        public EsfNode()
        {
            this.values = new List<IEsfValue>();
            this.name = string.Empty;
            this.title = string.Empty;
        }

        public EsfNode(IEsfNode esfNode, IEsfNode parent, EsfParser parser)
            : base(esfNode, parent, parser)
        {
            this.values = new List<IEsfValue>();
            this.name = string.Empty;
            this.title = string.Empty;
            foreach (IEsfValue value2 in esfNode.Values)
            {
                if (value2.Type == EsfValueType.SingleNode)
                {
                    this.Values.Add(new EsfSingleNode((IEsfNode)value2, parent, parser));
                }
                else
                {
                    if (value2.Type == EsfValueType.PolyNode)
                    {
                        this.Values.Add(new EsfPolyNode((IEsfNode)value2, parent, parser));
                        continue;
                    }
                    this.Values.Add(new EsfValue(value2, parent, parser));
                }
            }
   //         TitleParser vP = new TitleParser();
            this.unknown = esfNode.Unknown;
            this.offsetEnd = esfNode.OffsetEnd;
            this.beenParsed = esfNode.BeenParsed;
            this.name = esfNode.Name;
            this.title = esfNode.Title;
            this.containsChanges = esfNode.ContainsChanges;
            this.treeContainsChanges = esfNode.TreeContainsChanges;
        }

        public virtual IEsfNode CopyTo(IEsfNode destination, int index)
        {
            throw new NotImplementedException("Copying not supported for this type node");
        }

        public virtual void Delete()
        {
            this.Delete(true);
        }

        public virtual void Delete(bool updateParents)
        {
            if (updateParents)
            {
                for (IEsfNode node = base.Parent; node != null; node = node.Parent)
                {
                    node.TreeContainsChanges++;
                }
            }
            base.IsDeleted++;
        }

        public virtual List<IEsfValue> GetValues()
        {
            List<IEsfValue> list = new List<IEsfValue>();
            foreach (IEsfValue value2 in this.values)
            {
                if (value2.IsValueType)
                {
                    list.Add(value2);
                }
            }
            return list;
        }

        public virtual void UnDelete()
        {
            this.UnDelete(true);
        }

        public virtual void UnDelete(bool updateParents)
        {
            if (updateParents)
            {
                for (IEsfNode node = base.Parent; node != null; node = node.Parent)
                {
                    node.TreeContainsChanges--;
                }
            }
            base.IsDeleted--;
        }

        public virtual void UndoCopy()
        {
            throw new NotImplementedException("Undoing copying not supported for this type node");
        }

        public bool BeenParsed
        {
            get
            {
                return this.beenParsed;
            }
            set
            {
                this.beenParsed = value;
            }
        }

        public int ContainsChanges
        {
            get
            {
                return this.containsChanges;
            }
            set
            {
                this.containsChanges = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public uint OffsetEnd
        {
            get
            {
                return this.offsetEnd;
            }
            set
            {
                this.offsetEnd = value;
            }
        }

        public int TreeContainsChanges
        {
            get
            {
                return this.treeContainsChanges;
            }
            set
            {
                this.treeContainsChanges = value;
            }
        }

        public byte Unknown
        {
            get
            {
                return this.unknown;
            }
            set
            {
                this.unknown = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public List<IEsfValue> Values
        {
            get
            {
                return this.values;
            }
            set
            {
                this.values = value;
            }
        }
    }
}

