namespace EsfEditor.EditorObjects
{
    using EsfEditor.Core.EsfObjects;
    using System;

    public class ExportObject
    {
        public string fileName;
        public IEsfNode node;

        public ExportObject(string fileName, IEsfNode node)
        {
            this.fileName = fileName;
            this.node = node;
        }
    }
}

