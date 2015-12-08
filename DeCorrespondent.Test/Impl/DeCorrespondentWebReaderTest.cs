using System;
using System.IO;
using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class DeCorrespondentWebReaderTest
    {
        [Test]
        public void LoginAndReadNewItems()
        {
            using (var reader = CreateReader())
            {
                var result = reader.Read("https://decorrespondent.nl/nieuw/0");

                Assert.NotNull(result);
            }
        }

        [Test]
        public void Logout()
        {
            var reader = CreateReader();
            reader.Dispose();

            var result = reader.Read("https://decorrespondent.nl/nieuw/0");

            Assert.IsTrue(result.Contains("Log dan nu in"));
        }

        [Test]
        public void ReadItems()
        {
            using (var reader = new DeCorrespondentResources(CreateReader(), new ConsoleLogger(true)))
            {
                var items = reader.ReadNieuwItems().Take(10).ToList();

                Assert.IsTrue(items.Any());
                Console.WriteLine(string.Join(",", items.Select(i => i.Id)));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Unable to login, check credentials (username=jon@mailfence.com)")]
        public void LoginFailed()
        {
            var invalidCredentials = new Config();
            invalidCredentials.Username = "jon@mailfence.com";
            invalidCredentials.Password = "p";

            CreateReader(invalidCredentials);
        }

        [Test]
        public void ReadItem()
        {
            var id = 3530;
            using (var reader = new DeCorrespondentResources(CreateReader(), new ConsoleLogger(true)))
            {
                var article = reader.ReadArticle(id);

                File.WriteAllText("d:\\article_" + id, article);
            }
        }

        [Test]
        public void ReadBinary()
        {
            using (var reader = CreateReader())
            {
                var data = reader.ReadBinary("https://dynamic.decorrespondent.nl/ff-1445921139/media/660/562a36b4969f44427492345.jpg");

                Assert.NotNull(data);
                File.WriteAllBytes("d:\\img_.jpg", data);
            }
        }

        private static DeCorrespondentWebReader CreateReader(IDeCorrespondentReaderConfig config = null)
        {
            var cfg = config ?? FileConfig.Load(@"..\..\config-test.xml").DeCorrespondentReaderConfig;
            return DeCorrespondentWebReader.Login(new ConsoleLogger(true), cfg.Username, cfg.Password);
        }

        class Config : IDeCorrespondentReaderConfig
        {
            public string Username { get; set;}
            public string Password { get; set; }
        }
    }
}
