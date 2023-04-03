namespace EsfEditor.Utils
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    public static class XmlUtils
    {
        public static XmlDocument LoadXmlFromFile(string fileName)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(Path.Combine(FilesPaths.Xml, fileName));
            }
            catch (Exception)
            {
            }
            return document;
        }

        public static XmlDocument LoadXmlFromResources(string fileName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            XmlDocument document = new XmlDocument();
            try
            {
                Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + "." + fileName);
                document.Load(manifestResourceStream);
            }
            catch (Exception)
            {
            }
            return document;
        }

        public static XmlDocument MakeXmlDocument(string documentRoot, StringCollection strings)
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "", ""));
            XmlNode newChild = document.CreateNode(XmlNodeType.Element, documentRoot, "");
            document.AppendChild(newChild);
            try
            {
                //using (StringEnumerator enumerator = strings.GetEnumerator())
                //{
                StringEnumerator enumerator = strings.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string name = enumerator.Current.Replace(":", "").Replace(" ", "");
                        XmlNode node = document.CreateNode(XmlNodeType.Element, name, "");
                        newChild.AppendChild(node);
                    }
                    return document;
                //}
            }
            catch (Exception)
            {
            }
            return document;
        }
    }
}

