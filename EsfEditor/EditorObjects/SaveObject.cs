namespace EsfEditor.EditorObjects
{
    using EsfEditor.Core.EsfObjects;
    using System;

    public class SaveObject
    {
        public EsfFile File;
        public string SaveAsFilePath = "";

        public SaveObject(EsfFile file, string saveAsFilePath)
        {
            this.File = file;
            this.SaveAsFilePath = saveAsFilePath;
        }
    }
}

