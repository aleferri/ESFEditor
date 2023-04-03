namespace EsfEditor.Core.EsfObjects
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct EsfFloatPoint
    {
        public float x;
        public float y;
        public override string ToString()
        {
            return (this.x + ";" + this.y);
        }

        public static EsfFloatPoint Parse(string value)
        {
            EsfFloatPoint point = new EsfFloatPoint();
            string[] strArray = value.Split(new char[] { ';' });
            if (strArray.Length != 2)
            {
                throw new FormatException();
            }
            point.x = float.Parse(strArray[0]);
            point.y = float.Parse(strArray[1]);
            return point;
        }
    }
}

