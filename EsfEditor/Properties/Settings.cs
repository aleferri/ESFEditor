namespace EsfEditor.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    [CompilerGenerated, GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings) SettingsBase.Synchronized(new Settings()));

        [DebuggerNonUserCode, UserScopedSetting]
        public StringCollection DataGridViewFormColumns
        {
            get
            {
                return (StringCollection) this["DataGridViewFormColumns"];
            }
            set
            {
                this["DataGridViewFormColumns"] = value;
            }
        }

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting, DebuggerNonUserCode, DefaultSettingValue("100, 100")]
        public Point FormLocation
        {
            get
            {
                return (Point) this["FormLocation"];
            }
            set
            {
                this["FormLocation"] = value;
            }
        }

        [DefaultSettingValue("550, 400"), DebuggerNonUserCode, UserScopedSetting]
        public Size FormSize
        {
            get
            {
                return (Size) this["FormSize"];
            }
            set
            {
                this["FormSize"] = value;
            }
        }

        [DefaultSettingValue("Normal"), UserScopedSetting, DebuggerNonUserCode]
        public FormWindowState FormState
        {
            get
            {
                return (FormWindowState) this["FormState"];
            }
            set
            {
                this["FormState"] = value;
            }
        }
    }
}

