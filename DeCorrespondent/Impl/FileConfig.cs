using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DeCorrespondent.Impl
{
    public class FileConfig : IKindleEmailSenderConfig, IEmailSummarySenderConfig, IWebReaderConfig, IArticleRendererConfig
    {
        public static FileConfig Load(string path)
        {
            return DeserializeXml(File.ReadAllText(path ?? @"D:\Applications\DeCorrespondent\config.xml"));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, SerializeXml(this));
        }

        [XmlIgnore]
        public IKindleEmailSenderConfig KindleEmailSenderConfig { get { return this; } }
        [XmlIgnore]
        public IEmailSummarySenderConfig EmailSummarySenderConfig { get { return this; } }
        [XmlIgnore]
        public IWebReaderConfig CorrespondentCredentails { get { return this; } }
        [XmlIgnore]
        public IArticleRendererConfig ArticleRendererConfig { get { return this; } }
        public string Username { get; set; }
        public string Password { get; set; }
        public string KindleEmail { get; set; }
        public string SummaryEmail { get; set; }
        public string LicenseKey { get; set; }

        private static string SerializeXml(FileConfig model)
        {
            var serializer = new XmlSerializer(typeof(FileConfig));
            var sb = new StringBuilder(2048);
            serializer.Serialize(new StringWriter(sb), model);
            return sb.ToString();
        }

        private static FileConfig DeserializeXml(string xml)
        {
            if (xml == null) return (FileConfig)Activator.CreateInstance(typeof(FileConfig));
            var serializer = new XmlSerializer(typeof(FileConfig));
            return (FileConfig)serializer.Deserialize(new StringReader(xml));
        }

    }
}
