using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using miranda.ui;

namespace miranda
{
    public static class AppSettingsManager
    {
        static AppSettingsManager()
        {
            Dir = Application.StartupPath + @"\";
        }
        //public static AppSettings Settings { get; private set; }
        public static string Dir { get; set; }

        public static AppSettings Load(string key)
        {
            var ser = new XmlSerializer(typeof (AppSettings));
            var file = Dir + key + ".xml";

            if (!File.Exists(file))
            {
                var s = new AppSettings();
                s.Key = key;
                return s;
            }
            var fs = new FileStream(file, FileMode.Open);
            
            var settings = (AppSettings)ser.Deserialize(fs);

            fs.Close();

            settings.Key = key;
            return settings;
        }

        public static void Save(AppSettings settings)
        {
            var ser = new XmlSerializer(typeof(AppSettings));
            var file = Dir + settings.Key + ".xml";
            var fs = new FileStream(file, FileMode.Create);

            ser.Serialize(fs, settings);

            fs.Close();
        }
    }
}