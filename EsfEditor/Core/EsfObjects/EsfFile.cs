namespace EsfEditor.Core.EsfObjects
{
    using EsfEditor.Parser;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class EsfFile : IDisposable
    {
        private string filePath;
        private EsfParser parser;
        private List<IEsfNode> rootNodes = new List<IEsfNode>();

        public EsfFile(string filePath)
        {
            this.filePath = filePath;
            this.parser = new EsfParser(filePath);
            this.rootNodes.Add(this.parser.root);
        }

        public void Dispose()
        {
            this.parser.Dispose();
        }

        public bool HasChanges()
        {
            if ((this.parser.root.TreeContainsChanges <= 0) && (this.parser.root.ContainsChanges <= 0))
            {
                return false;
            }
            return true;
        }

        public void Reload()
        {
            this.parser.Dispose();
            this.rootNodes.Clear();
            this.parser = new EsfParser(this.filePath);
            this.rootNodes.Add(this.parser.root);
        }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.filePath);
            }
        }

        public string FilePath
        {
            get
            {
                return this.filePath;
            }
            set
            {
                this.filePath = value;
            }
        }

        public EsfParser Parser
        {
            get
            {
                return this.parser;
            }
            set
            {
                this.parser = value;
            }
        }

        public List<IEsfNode> RootNodes
        {
            get
            {
                return this.rootNodes;
            }
            set
            {
                this.rootNodes = value;
            }
        }
    }
}

