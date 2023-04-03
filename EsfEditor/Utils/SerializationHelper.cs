namespace EsfEditor.Utils
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public static class SerializationHelper
    {
        private static object BinaryDeserialize(Stream stream, Type type)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        private static void BinarySerialize(Stream stream, object o)
        {
            new BinaryFormatter().Serialize(stream, o);
        }

        public static object DeserializeFromFile(string fileName, SerializationType type, Type objectType)
        {
            FileStream stream = null;
            try
            {
                string str;
                if (type == SerializationType.Xml)
                {
                    str = Path.Combine(FilesPaths.Xml, fileName);
                }
                else
                {
                    str = Path.Combine(FilesPaths.Application, fileName);
                }
                using (stream = new FileStream(str, FileMode.Open))
                {
                    if (type == SerializationType.Xml)
                    {
                        return XmlDeserialize(stream, objectType);
                    }
                    return BinaryDeserialize(stream, objectType);
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static void SerializeToFile(string fileName, object o, SerializationType type)
        {
            FileStream stream = null;
            try
            {
                string str;
                if (type == SerializationType.Xml)
                {
                    str = Path.Combine(FilesPaths.Xml, fileName);
                }
                else
                {
                    str = Path.Combine(FilesPaths.Application, fileName);
                }
                using (stream = new FileStream(str, FileMode.Create))
                {
                    if (type == SerializationType.Xml)
                    {
                        XmlSerialize(stream, o);
                    }
                    else
                    {
                        BinarySerialize(stream, o);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static object XmlDeserialize(Stream stream, Type type)
        {
            XmlTextReader xmlReader = new XmlTextReader(stream);
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(xmlReader);
        }

        private static void XmlSerialize(Stream stream, object o)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            new XmlSerializer(o.GetType()).Serialize((XmlWriter) writer, o);
        }
    }
}

