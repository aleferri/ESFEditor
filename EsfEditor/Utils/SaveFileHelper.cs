namespace EsfEditor.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Xml;

    public static class SaveFileHelper
    {
        public static void SaveAsXml(string fileName, StringCollection strings, string documentRoot)
        {
            FileStream w = null;
            try
            {
                using (w = new FileStream(Path.Combine(FilesPaths.Xml, fileName), FileMode.Create))
                {
                    new XmlTextWriter(w, Encoding.UTF8);
                    XmlUtils.MakeXmlDocument(documentRoot, strings).Save(w);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SaveBinaryFile(string fileName, byte[] binaryData)
        {
            FileStream output = null;
            try
            {
                using (output = new FileStream(fileName, FileMode.Create))
                {
                    new BinaryWriter(output).Write(binaryData);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SaveBinaryFile(string fileName, List<byte[]> binaryData, BackgroundWorker worker)
        {
            FileStream output = null;
            try
            {
                using (output = new FileStream(fileName, FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(output);
                    int num = 1;
                    foreach (byte[] buffer in binaryData)
                    {
                        writer.Write(buffer);
                        worker.ReportProgress((num * 100) / binaryData.Count);
                        num++;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SaveXml(string fileName, XmlDocument doc)
        {
            FileStream w = null;
            try
            {
                using (w = new FileStream(Path.Combine(FilesPaths.Xml, fileName), FileMode.Create))
                {
                    new XmlTextWriter(w, Encoding.UTF8);
                    doc.Save(w);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

