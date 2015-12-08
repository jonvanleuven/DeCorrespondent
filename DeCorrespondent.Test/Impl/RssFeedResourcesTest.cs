using System;
using System.IO;
using System.Linq;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class RssFeedResourcesTest
    {
        [Test]
        public void Items()
        {
            var reader = CreateReader();

            var items = reader.ReadNieuwItems().ToList();

            Assert.AreEqual(50, items.Count());
            var item = items.First();
            Assert.AreEqual(3391, item.Id);
            Assert.AreEqual(new DateTime(2015, 12, 1, 6, 40, 6), item.Publicationdate);
        }

        [Test]
        public void ReadArticle()
        {
            var reader = CreateReader();

            var result = reader.ReadArticle(3391);

            Assert.IsNotNull(result);
        }

        [Test]
        public void ReadAllArticles()
        {
            var reader = CreateReader(new WebReader(new ConsoleLogger(true)));

            var result = reader.ReadNieuwItems().Select(i => reader.ReadArticle(i.Id)).ToList();

            Assert.IsNotNull(result);
        }

        private static RssFeedResources CreateReader(IResourceReader resources = null)
        {
            return new RssFeedResources(resources ?? new FileResources());
        }

        private string ReadFeedXml()
        {
            var name = "DeCorrespondent.Test.Resources.rss.xml";
            var resource = GetType().Assembly.GetManifestResourceStream(name);
            if (resource == null)
                throw new Exception("Article resource not found: " + name);
            using (var s = new StreamReader(resource))
            {
                return s.ReadToEnd();
            }

        }
    }
}
