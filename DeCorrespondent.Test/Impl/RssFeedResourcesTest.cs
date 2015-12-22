using System;
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
        public void ReadAndRenderAllFromWebResource() //TODO fix testcase
        {
            var reader = CreateReader(new WebReader(new ConsoleLogger(true)));
            var articleReader = new ArticleReader();
            var config = CreateConfig();
            var renderer = new PdfArticleRenderer(new ConsoleLogger(true), config.ArticleRendererConfig, config.EvoPdfLicenseKey);

            var result = reader.ReadNieuwItems()
                .Select(i => reader.ReadArticle(i.Id))
                .Select(a => articleReader.Read(a))
                .Select(a => renderer.Render(a))
                .ToList();

            Assert.IsNotNull(result);
        }

        private static FileConfig CreateConfig()
        {
            return FileConfig.Load(@"..\..\config-test.xml");
        }

        private static RssFeedResources CreateReader(IResourceReader resources = null)
        {
            return new RssFeedResources(resources ?? new FileResources());
        }
    }
}
