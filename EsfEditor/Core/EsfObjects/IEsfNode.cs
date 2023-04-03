namespace EsfEditor.Core.EsfObjects
{
    using System;
    using System.Collections.Generic;

    public interface IEsfNode : IEsfValue
    {
        void AddChildren(int index, IEsfNode newNode);
        IEsfNode CopyTo(IEsfNode destination, int index);
        void Delete();
        void Delete(bool updateParents);
        List<IEsfNode> GetChildren();
        List<IEsfValue> GetValues();
        bool HasSizeChanged();
        void Parse();
        void ParseDeep();
        void UnDelete();
        void UnDelete(bool updateParents);
        void UndoCopy();

        bool BeenParsed { get; set; }

        int ContainsChanges { get; set; }

        string Name { get; set; }

        uint OffsetEnd { get; set; }

        int TreeContainsChanges { get; set; }

        byte Unknown { get; set; }

        string Title { get; set; }

        List<IEsfValue> Values { get; set; }
    }
}

