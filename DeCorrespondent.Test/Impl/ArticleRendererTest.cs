using System.IO;
using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class ArticleRendererTest
    {
        [Test]
        public void Render()
        {
            var reference = new ArticleReference(3358, new NewItemsReaderTest.FileResources(), new ConsoleLogger(true));
            var article = reference.ReadArticle();
            var renderer = CreateRenderer();

            var result = renderer.Render(new[] {article}).ToList();

            Assert.AreEqual(1, result.Count());
            var pdf = result.First();
            Assert.NotNull(pdf.Content);
            Assert.AreEqual("12-16 Vermeulen-Reis mee door het land dat niet bestaat.pdf", pdf.Name);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3361()
        {
            var reference = new ArticleReference(3361, new NewItemsReaderTest.FileResources(), new ConsoleLogger(true));
            var article = reference.ReadArticle();
            var renderer = CreateRenderer();

            var result = renderer.Render(new[] { article }).ToList();

            Assert.AreEqual(1, result.Count());
            var pdf = result.First();
            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        private static IArticleRenderer CreateRenderer()
        {
            return new ArticleRenderer(new ConsoleLogger(true), FileConfig.Load(null));
        }
    }
}
