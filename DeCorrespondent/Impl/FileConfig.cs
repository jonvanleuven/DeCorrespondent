using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace DeCorrespondent.Impl
{
    public class FileConfig : IKindleEmailSenderConfig, IEmailNotificationSenderConfig, IWebReaderConfig, IArticleRendererConfig, ISmtpMailConfig
    {
        public static FileConfig Load(string path)
        {
            return DeserializeXml(File.ReadAllText(path ?? @"config.xml"));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, SerializeXml(this));
        }

        [XmlIgnore]
        public IKindleEmailSenderConfig KindleEmailSenderConfig { get { return this; } }
        [XmlIgnore]
        public IEmailNotificationSenderConfig EmailNotificationSenderConfig { get { return this; } }
        [XmlIgnore]
        public IWebReaderConfig CorrespondentCredentails { get { return this; } }
        [XmlIgnore]
        public IArticleRendererConfig ArticleRendererConfig { get { return this; } }
        [XmlIgnore]
        public ISmtpMailConfig SmtpConfig { get { return this; } }
        public string Username { get; set; }
        [XmlIgnore]
        public string Password { get { return Encryptor.DecryptAES(PasswordEncrypted); } set { PasswordEncrypted = Encryptor.EncryptAES(value); } }
        public string PasswordEncrypted { get; set; }
        public string KindleEmail { get; set; }
        public string MailUsername { get; set; }
        [XmlIgnore]
        public string MailPassword { get { return Encryptor.DecryptAES(MailPasswordEncrypted); } set { MailPasswordEncrypted = Encryptor.EncryptAES(value); } }
        public string MailPasswordEncrypted { get; set; }
        public string NotificationEmail { get; set; }
        public string LicenseKey { get; set; }
        public bool DisplayInfocards { get; set; }
        public bool DisplayPublicationLinks { get; set; }
        public bool DisplayBlockquotes { get; set; }
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