using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace DeCorrespondent.Impl
{
    public class FileConfig : IKindleEmailSenderConfig, IEmailSummarySenderConfig, IWebReaderConfig, IArticleRendererConfig, ISmtpMailConfig
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
        public IEmailSummarySenderConfig EmailSummarySenderConfig { get { return this; } }
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
        public string SummaryEmail { get; set; }
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
        private static readonly byte[] Key = HexString2ByteArray("7D5236D0E750D1372468A599781A60BD5C809131437F8106C89A20AAD2C2C2C2");
        private static readonly byte[] IV = HexString2ByteArray("F54C6F1B4BF5888A2167F35D0895FAC2");

        public static string EncryptAES(string bsn)
        {
            // argument controle.
            // Argument controleren
            if (string.IsNullOrEmpty(bsn)) { return null; }
            byte[] encrypted;

            var encryptor = KEY.CreateEncryptor(KEY.Key, KEY.IV);

            // Stream aanmaken voor encryptie.
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Alle data wegschrijven naar de stream.
                        swEncrypt.Write(bsn);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return ConverteerNaarHex(encrypted);
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

        public static string DecryptAES(string encryptedBsns)
        {
            // Argument controleren
            if (string.IsNullOrEmpty(encryptedBsns)) { return null; }

            var encryptedBsn = HexString2ByteArray(encryptedBsns);

            // ontcijferde tekst.
            string plattetekst = null;

            // Aanmaken RijndaelManaged object
            // met de gespecificeerde key and IV.
            // Decryptor aanmaken voor stream transform
            var decryptor = KEY.CreateDecryptor(KEY.Key, KEY.IV);

            // Stream aanmaken voor Decryptie
            using (var msDecrypt = new MemoryStream(encryptedBsn))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {

                        // lees de ontcijferfde bytes van de decryptor stream
                        // en plaats hun in de string.
                        plattetekst = srDecrypt.ReadToEnd();
                    }
                }
            }
            return plattetekst;
        }

        private static string ConverteerNaarHex(byte[] data)
        {
            var hex = new StringBuilder();
            foreach (var b in data)
            {
                hex.Append(b.ToString("X2"));
            }
            return hex.ToString();
        }

        private static byte[] HexString2ByteArray(string hexString)
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