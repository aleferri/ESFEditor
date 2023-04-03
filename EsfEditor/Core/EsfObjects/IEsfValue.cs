namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Core.Enums;
    using EsfEditor.Parser;
    using System;

    public interface IEsfValue
    {
        void ChangeValue(string oldValue);
        void QuickSave();
        void SlowSave();
        void UndoChangeValue();
        bool ValidateNewValue(string newValue);

        string Description { get; set; }

        bool IsBinaryType { get; }

        int IsDeleted { get; set; }

        int IsNew { get; set; }

        bool IsValueType { get; }

        uint Offset { get; set; }

        object OriginalValue { get; set; }

        IEsfNode Parent { get; set; }

        EsfParser Parser { get; set; }

        EsfValueType Type { get; set; }

        object Value { get; set; }
    }
}

