namespace EsfEditor.Utils
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class FilesPaths
    {
        public static string Application
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public static string ImportExport
        {
            get
            {
                Assembly.GetExecutingAssembly();
                return Path.Combine(Application, "ImportExport");
            }
        }

        public static string Xml
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}

