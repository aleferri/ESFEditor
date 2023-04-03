namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Core.Enums;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct EsfHeader
    {
        public EsfType magic;
        public uint unknown1;
        public uint unknown2;
        public uint offsetNodeNames;
    }
}

