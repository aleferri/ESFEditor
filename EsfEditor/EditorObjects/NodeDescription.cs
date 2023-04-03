namespace EsfEditor.EditorObjects
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class NodeDescription
    {
        private string name;
        private List<string> valuesDescription;

        public NodeDescription()
        {
        }

        public NodeDescription(string name, List<string> valuesDescription)
        {
            this.name = name;
            this.valuesDescription = valuesDescription;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public List<string> ValuesDescription
        {
            get
            {
                return this.valuesDescription;
            }
            set
            {
                this.valuesDescription = value;
            }
        }
    }
}

