namespace EsfEditor.EditorObjects
{
    using EsfEditor.Core.EsfObjects;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class OpenFiles : IDisposable
    {
        private readonly List<EsfFile> esfFiles = new List<EsfFile>();
        private EsfFile fileToReload;

        public void Dispose()
        {
            this.UnregisterAllFile();
        }

        public EsfFile GetFile(string filePath)
        {
            foreach (EsfFile file in this.esfFiles)
            {
                if (file.FilePath == filePath)
                {
                    return file;
                }
            }
            return null;
        }

        public bool IsOpened(string fileName)
        {
            foreach (EsfFile file in this.esfFiles)
            {
                if (file.FilePath == fileName)
                {
                    return true;
                }
            }
            return false;
        }

        public void RegisterFile(EsfFile file)
        {
            this.esfFiles.Add(file);
        }

        public void ReloadFile()
        {
            if (this.fileToReload != null)
            {
                this.fileToReload.Reload();
            }
        }

        public void SaveAllFiles(BackgroundWorker worker)
        {
            foreach (EsfFile file in this.esfFiles)
            {
                file.Parser.Save(worker);
            }
        }

        public void SaveFile(EsfFile file, BackgroundWorker worker)
        {
            file.Parser.Save(worker);
        }

        public void UnregisterAllFile()
        {
            foreach (EsfFile file in this.esfFiles)
            {
                file.Dispose();
            }
            this.esfFiles.Clear();
        }

        public void UnregisterFile(EsfFile file)
        {
            this.esfFiles.Remove(file);
            file.Dispose();
        }

        public void UnregisterFile(string fileName)
        {
            EsfFile item = null;
            foreach (EsfFile file2 in this.esfFiles)
            {
                if (file2.FileName == fileName)
                {
                    item = file2;
                }
            }
            this.esfFiles.Remove(item);
            item.Dispose();
        }

        public List<EsfFile> EsfFiles
        {
            get
            {
                return this.esfFiles;
            }
        }

        public EsfFile FileToReload
        {
            get
            {
                return this.fileToReload;
            }
            set
            {
                this.fileToReload = value;
            }
        }
    }
}

