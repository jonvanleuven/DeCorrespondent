using System;
using System.IO;
using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class WebReaderTest
    {
        [Test]
        public void LoginAndReadNewItems()
        {
            using (var reader = CreateReader())
            {
                var result = reader.ReadNewItems(0);

                Assert.NotNull(result);
            }
        }

        [Test]
        public void Logout()
        {
            var reader = CreateReader();
            reader.Dispose();

            var result = reader.ReadNewItems(0);

            Assert.IsTrue(result.Contains("Log dan nu in"));
        }

        [Test]
        public void ReadItems()
        {
            using (var reader = CreateReader())
            {
                var items = new NewItemsReader(new ConsoleLogger(true)).ReadItems(reader.ReadNewItems(0));

                Assert.IsTrue(items.Any());
                Console.WriteLine(string.Join(",", items.Select(i => i.Id)));
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
            using (var reader = CreateReader())
            {
                var article = reader.ReadArticle(id);

                File.WriteAllText("d:\\article_" + id, article);
            }
        }

        private static WebReader CreateReader(IWebReaderConfig config = null)
        {
            return WebReader.Login(new ConsoleLogger(true), config ?? FileConfig.Load(@"D:\Applications\DeCorrespondent\config.xml"));
        }

        class Config : IWebReaderConfig
        {
            public string Username { get; set;}
            public string Password { get; set; }
        }
    }
}
