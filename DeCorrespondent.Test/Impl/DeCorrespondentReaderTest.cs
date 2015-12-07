using System;
using System.Linq;
using NUnit.Framework;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class DeCorrespondentReaderTest
    {
        [Test]
        public void ReadItems()
        {
            var reader = CreateReader();

            var result = reader.ReadNieuwItems().Take(10);

            Assert.AreEqual(10, result.Count());
            Assert.AreEqual(3352, result.First().Id);
        }

        [Test]
        public void Publicationdate()
        {
            var config = FileConfig.Load(@"..\..\config-test.xml").DeCorrespondentReaderConfig;
            using (var webresources = WebReader.Login(new ConsoleLogger(true), config.Username, config.Password))
            {
                var reader = new DeCorrespondentReader(webresources, new ConsoleLogger(true));
                var articleReader = new ArticleReader();

                var result = reader.ReadNieuwItems().Take(20).ToList();

                foreach (var item in result)
                {
                    Assert.IsTrue(articleReader.Read(reader.ReadArticle(item.Id)).Metadata.Published - item.Publicationdate < new TimeSpan(0, 0, 60));
                }
            }
        }

        private static DeCorrespondentReader CreateReader()
        {
            return new DeCorrespondentReader(new FileResources(), new ConsoleLogger(true));
        }
    }
}
