namespace EsfEditor.EditorObjects
{
    using EsfEditor.Core.EsfObjects;
    using EsfEditor.Utils;
    using System;
    using System.Collections.Generic;

    public static class NodesStructure
    {
        private static bool needSave;
        private static List<NodeDescription> nodesDescriptions;

        public static NodeDescription GetNodeDescriptions(string nodeName)
        {
            foreach (NodeDescription description in nodesDescriptions)
            {
                if (description.Name == nodeName)
                { 
                    return description;
                }
            }
            return null;
        }

        public static void LoadNodesDescription(SerializationType type)
        {
            string fileName = (type == SerializationType.Xml) ? "NodesDescriptions.xml" : "NodesDescriptions.bin";
            nodesDescriptions = (List<NodeDescription>)SerializationHelper.DeserializeFromFile(fileName, type, typeof(List<NodeDescription>));
            string fileDump = "NodesDescriptionsDump.xml";
            SerializationHelper.SerializeToFile(fileDump, nodesDescriptions, SerializationType.Xml);
        }

        public static void Save(SerializationType type)
        {
            string fileName = "NodesDescriptions.xml";
            if (type == SerializationType.Binary)
            {
                fileName = "NodesDescriptions.bin";
            }
            SerializationHelper.SerializeToFile(fileName, nodesDescriptions, type);
            needSave = false;
        }

        public static List<IEsfValue> SetDescriptions(IEsfNode node)
        {
            List<IEsfValue> values = node.GetValues();
            NodeDescription nodeDescriptions = GetNodeDescriptions(node.Name);
            try
            {
                if ((nodeDescriptions == null) || (nodeDescriptions.ValuesDescription.Count <= 0))
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        values[i].Description = "Node not found: " + node.Name;
                    }
                    return values;
                }
                int num = Math.Min(nodeDescriptions.ValuesDescription.Count, values.Count);
                for (int i = 0; i < num; i++)
                {
                    values[i].Description = nodeDescriptions.ValuesDescription[i];
                }
            }
            catch (Exception)
            {
            }
            return values;
        }

        public static List<IEsfValue> SetDescriptions(ref List<IEsfValue> values, string nodeName)
        {
            NodeDescription nodeDescriptions = GetNodeDescriptions(nodeName);
            try
            {
                if ((nodeDescriptions == null) || (nodeDescriptions.ValuesDescription.Count <= 0))
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        values[i].Description = "Node Name not found " + nodeName;
                    }
                    return values;
                }
                int num = Math.Min(nodeDescriptions.ValuesDescription.Count, values.Count);
                for (int i = 0; i < num; i++)
                {
                    values[i].Description = nodeDescriptions.ValuesDescription[i];
                }
            }
            catch (Exception)
            {
            }
            return values;
        }

        public static void SetNodeDescriptions(IEsfNode node)
        {
            NodeDescription nodeDescriptions = GetNodeDescriptions(node.Name);
            if (nodeDescriptions == null)
            {
                List<string> list = new List<string>();
                NodeDescription nD = new NodeDescription(node.Name, list);
                //list.Add((string)nD.ToString());
                list.Add(nD.ToString());
                needSave = true;
            }
            List<IEsfValue> values = node.GetValues();
            nodeDescriptions.ValuesDescription.Clear();
            foreach (IEsfValue value2 in values)
            {
                nodeDescriptions.ValuesDescription.Add(value2.Description);
            }
            needSave = true;
        }

        public static bool NeedSave
        {
            get
            {
                return needSave;
            }
            set
            {
                needSave = value;
            }
        }

        public static List<NodeDescription> NodesDescriptions
        {
            get
            {
                return nodesDescriptions;
            }
            set
            {
                nodesDescriptions = value;
            }
        }
    }
}

