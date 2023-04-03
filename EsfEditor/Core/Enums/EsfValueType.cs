namespace EsfEditor.Core.Enums
{
    using System;

    public enum EsfValueType : byte
    {
        Ascii = 0x0f,
        Binary41 = 0x41,
        Binary42 = 0x42,
        Binary43 = 0x43,
        Binary44 = 0x44,
        Binary45 = 0x45,
        Binary46 = 0x46,
        Binary47 = 0x47,
        Binary48 = 0x48,
        Binary49 = 0x49,
        Binary4A = 0x4a,
        Binary4B = 0x4b,
        Binary4C = 0x4c,
        Binary4D = 0x4d,
        Boolean = 1,
        Byte = 6,
        Float = 0X0a,
        FloatPoint = 0X0c,
        FloatPoint3D = 0x0d,
        Int = 4,
        PolyNode = 0x81,
        Short = 0,
        SingleNode = 0x80,
        UInt = 8,
        UInt16 = 7,
        UShort = 0x10,
        UTF16 = 0x0e
    }
}
