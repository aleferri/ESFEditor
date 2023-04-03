namespace EsfEditor.Core.EsfObjects
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct EsfFloatPoint3D
    {
        public float x;
        public float y;
        public float z;
        public override string ToString()
        {
            return (this.x + ";" + this.y + ";" + this.z);
        }

        public static EsfFloatPoint3D Parse(string value)
        {
            EsfFloatPoint3D point = new EsfFloatPoint3D();
            string[] strArray = value.Split(new char[] { ';' });
            if (strArray.Length != 3)
            {
                throw new FormatException();
            }
            point.x = float.Parse(strArray[0]);
            point.y = float.Parse(strArray[1]);
            point.z = float.Parse(strArray[2]);
            return point;
        }
    }
}