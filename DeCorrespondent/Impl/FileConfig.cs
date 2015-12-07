using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace DeCorrespondent.Impl
{
    public class FileConfig : IArticleRendererConfig
    {
        public static FileConfig Load(string path)
        {
            if (!File.Exists(path ?? @"config.xml") )
                new FileConfig().Save(path ?? @"config.xml");
            return DeserializeXml(File.ReadAllText(path ?? @"config.xml"));
        }

        public void Save(string path)
        {
            File.WriteAllText(path ?? @"config.xml", SerializeXml(this));
        }

        public FileConfig()
        {
            SmtpMailConfig = new SmtpMailConfig();
            DeCorrespondentReaderConfig = new DeCorrespondentReaderConfig();
            KindleEmailSenderConfig = new KindleEmailSenderConfig();
            EmailNotificationSenderConfig = new EmailNotificationSenderConfig();
            ArticleRendererConfig = new ArticleRendererConfig();
            MaxAantalArticles = 20;
        }

        [XmlIgnore]
        public SmtpMailConfig SmtpMailConfig { get; private set; }
        [XmlIgnore]
        public DeCorrespondentReaderConfig DeCorrespondentReaderConfig { get; private set; }
        [XmlIgnore]
        public KindleEmailSenderConfig KindleEmailSenderConfig { get; private set; }
        [XmlIgnore]
        public EmailNotificationSenderConfig EmailNotificationSenderConfig { get; private set; }
        [XmlIgnore]
        public ArticleRendererConfig ArticleRendererConfig { get; private set; }

        //DeCorrespondent settings
        [ConfigurableViaCommandLine("Gebruikersnaam van je DeCorrespondent account", false)]
        public string DeCorrespondentUsername { get { return DeCorrespondentReaderConfig.Username; } set { DeCorrespondentReaderConfig.Username = value; } }
        [ConfigurableViaCommandLine("Wachtwoord van je DeCorrespondent account (wordt encrypted opgeslagen)", true)]
        public string DeCorrespondentPassword { get { return Encryptor.EncryptAES(DeCorrespondentReaderConfig.Password); } set { DeCorrespondentReaderConfig.Password = Encryptor.DecryptAES(value); } }
        
        //Kindle settings
        [ConfigurableViaCommandLine("Email adres van je kindle", false)]
        public string KindleEmail { get { return KindleEmailSenderConfig.KindleEmail; } set { KindleEmailSenderConfig.KindleEmail = value; } }

        //Notificatie settings
        [ConfigurableViaCommandLine("Email adres waar notificaties naartoe moeten worden gestuurd", false)]
        public string NotificationEmail { get { return EmailNotificationSenderConfig.NotificationEmail; } set { EmailNotificationSenderConfig.NotificationEmail = value; } }

        //Email settings
        [ConfigurableViaCommandLine("Gebruikersnaam van je mail account", false)]
        public string SmtpUsername { get { return SmtpMailConfig.Username; } set { SmtpMailConfig.Username = value; } }
        [ConfigurableViaCommandLine("Wachtwoord van je mail account (wordt encrypted opgeslagen)", true)]
        public string SmtpPassword { get { return Encryptor.EncryptAES(SmtpMailConfig.Password); } set { SmtpMailConfig.Password = Encryptor.DecryptAES(value); } }
        [ConfigurableViaCommandLine("Smtp server naam", false)]
        public string SmtpServer { get { return SmtpMailConfig.Server; } set { SmtpMailConfig.Server = value; } }
        [ConfigurableViaCommandLine("Smtp server port", false)]
        public string SmtpPortNumber { get { return ""+SmtpMailConfig.Port; } set { SmtpMailConfig.Port = int.Parse(value); } }
        [ConfigurableViaCommandLine("Smtp ssl enabled", false)]
        public string SmtpEnableSsl { get { return "" + SmtpMailConfig.EnableSsl; } set { SmtpMailConfig.EnableSsl = bool.Parse(value); } }

        //render settins:
        public string EvoPdfLicenseKey { get { return ArticleRendererConfig.EvoPdfLicenseKey; } set { ArticleRendererConfig.EvoPdfLicenseKey = value; } }
        public bool DisplayInfocards { get { return ArticleRendererConfig.DisplayInfocards; } set { ArticleRendererConfig.DisplayInfocards = value; } }
        public bool DisplayPublicationLinks { get { return ArticleRendererConfig.DisplayPublicationLinks; } set { ArticleRendererConfig.DisplayPublicationLinks = value; } }
        public bool DisplayBlockquotes { get { return ArticleRendererConfig.DisplayBlockquotes; } set { ArticleRendererConfig.DisplayBlockquotes = value; } }
        public int MaxAantalArticles { get; set; }

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

        public class ConfigurableViaCommandLine : Attribute
        {

            public ConfigurableViaCommandLine(string description, bool isPassword)
            {
                Description = description;
                IsPassword = isPassword;
            }

            public string Display(string val)
            {
                if (string.IsNullOrEmpty(val))
                    return "";
                if (IsPassword)
                    return string.Join("", Encryptor.DecryptAES(val).ToCharArray().Select(v => "*"));
                return val;
            }
            public string Description { get; private set; }
            public bool IsPassword { get; private set; }
        }
    }

    public class SmtpMailConfig : ISmtpMailConfig
    {
        internal SmtpMailConfig()
        {
            Username = "";
            Password = "";
            Server = "smtp.gmail.com";
            Port = 587;
            EnableSsl = true;
        }
        public string Username { get; internal set; }
        public string Password { get; internal set; }
        public string Server { get; internal set; }
        public int Port { get; internal set; }
        public bool EnableSsl { get; internal set; }
    }

    public class DeCorrespondentReaderConfig : IDeCorrespondentReaderConfig
    {
        internal DeCorrespondentReaderConfig()
        {
            Username = "";
            Password = "";
        }
        public string Username { get; internal set; }
        public string Password { get; internal set; }
    }

    public class KindleEmailSenderConfig : IKindleEmailSenderConfig
    {
        internal KindleEmailSenderConfig()
        {
            KindleEmail = "";
        }
        public string KindleEmail { get; internal set; }
    }

    public class EmailNotificationSenderConfig : IEmailNotificationSenderConfig
    {
        internal EmailNotificationSenderConfig()
        {
            NotificationEmail = "";
        }
        public string NotificationEmail { get; internal set; }
    }

    public class ArticleRendererConfig : IArticleRendererConfig
    {
        internal ArticleRendererConfig()
        {
            DisplayBlockquotes = false;
            DisplayInfocards = true;
            DisplayPublicationLinks = true;
            EvoPdfLicenseKey = "";
        }
        public string EvoPdfLicenseKey { get; internal set; }
        public bool DisplayInfocards { get; internal set; }
        public bool DisplayPublicationLinks { get; internal set; }
        public bool DisplayBlockquotes { get; internal set; }
    }

    public static class Encryptor
    {
        private static readonly byte[] Key = HexStringToByteArray("7D5236D0E750D1372468A599781A60BD5C809131437F8106C89A20AAD2C2C2C2");
        private static readonly byte[] IV = HexStringToByteArray("F54C6F1B4BF5888A2167F35D0895FAC2");

        public static string EncryptAES(string str)
        {
            if (string.IsNullOrEmpty(str)) 
                return null;
            byte[] encrypted;
            var encryptor = KEY.CreateEncryptor(KEY.Key, KEY.IV);
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(str);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return ConvertToHex(encrypted);
        }

        private static readonly RijndaelManaged KEY = new RijndaelManaged();
        static Encryptor()
        {
            KEY.BlockSize = 128;
            KEY.KeySize = 256;
            KEY.Mode = CipherMode.CBC;
            KEY.Padding = PaddingMode.PKCS7;
            KEY.Key = Key;
            KEY.IV = IV;
        }

        public static string DecryptAES(string encryptedstrs)
        {
            if (string.IsNullOrEmpty(encryptedstrs)) 
                return null;

            var encryptedstr = HexStringToByteArray(encryptedstrs);
            string plain = null;
            var decryptor = KEY.CreateDecryptor(KEY.Key, KEY.IV);
            using (var msDecrypt = new MemoryStream(encryptedstr))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        plain = srDecrypt.ReadToEnd();
                    }
                }
            }
            return plain;
        }

        private static string ConvertToHex(byte[] data)
        {
            var hex = new StringBuilder();
            foreach (var b in data)
            {
                hex.Append(b.ToString("X2"));
            }
            return hex.ToString();
        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            var output = new byte[hexString.Length / 2];

            for (var i = 0; i <= hexString.Length - 2; i += 2)
            {
                output[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return output;
        }
    }
}