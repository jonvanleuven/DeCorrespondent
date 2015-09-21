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
            var reader = CreateReader();

            var result = reader.ReadNewItems();

            Assert.NotNull(result);
        }

        [Test]
        public void ReadItems()
        {
            var reader = CreateReader();

            var items = new NewItemsReader(new ConsoleLogger(true), reader).ReadItems(null);

            Assert.IsTrue(items.Any());
            Console.WriteLine(string.Join(",", items.Select(i => i.Id)));Console.WriteLine(string.Join(",", items.Select(i => i.Id)));
            Console.WriteLine(string.Join(",", items.Select(i => i.Id))); Console.WriteLine(string.Join(",", items.Select(i => i.ReadArticle().Title)));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Unable to login, check credentials (username=jon@mailfence.com)")]
        public void LoginFailed()
        {
            var invalidCredentials = new Config();
            invalidCredentials.Password = "p";

            CreateReader(invalidCredentials);
        }

        [Test]
        public void ReadItem()
        {
            var id = 3361;
            var reader = CreateReader();

            var article = reader.ReadArticle(new ArticleReference(id, reader, new ConsoleLogger(true)));

            File.WriteAllText("d:\\article_" + id, article);
        }


        public static WebReader CreateReader(IWebReaderConfig config = null)
        {
            return WebReader.Login(new ConsoleLogger(true), config ?? FileConfig.Load(null) );
        }

        class Config : IWebReaderConfig
        {
            public string Username { get; private set;}
            public string Password { get; set; }
        }
    }
}
