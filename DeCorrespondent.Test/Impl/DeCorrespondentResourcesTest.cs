using System;
using System.Linq;
using NUnit.Framework;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class DeCorrespondentResourcesTest
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
            using (var webresources = DeCorrespondentWebReader.Login(new ConsoleLogger(true), config.Username, config.Password))
            {
                var reader = new DeCorrespondentResources(webresources, new ConsoleLogger(true));
                var articleReader = new ArticleReader();

                var result = reader.ReadNieuwItems().Take(20);

                foreach (var item in result)
                {
                    AssertIgnoreSeconds(articleReader.Read(reader.ReadArticle(item.Id)).Metadata.Published, item.Publicationdate);
                }
            }
        }

        private static void AssertIgnoreSeconds(DateTime expected, DateTime actual)
        {
            Assert.AreEqual(
                new DateTime(expected.Year, expected.Month, expected.Day, expected.Hour, expected.Minute, 0), 
                new DateTime(actual.Year, actual.Month, actual.Day, actual.Hour, actual.Minute, 0)
                );
        }

        private static DeCorrespondentResources CreateReader()
        {
            return new DeCorrespondentResources(new FileResources(), new ConsoleLogger(true));
        }
    }
}
