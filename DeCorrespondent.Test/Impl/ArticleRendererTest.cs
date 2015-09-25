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
            var article = new ArticleReader().Read(new NewItemsReaderTest.FileResources().ReadArticle(3358));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            Assert.AreEqual("12-16 Vermeulen-Reis mee door het land dat niet bestaat.pdf", pdf.Name);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3361()
        {
            var article = new ArticleReader().Read(new NewItemsReaderTest.FileResources().ReadArticle(3361));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);
            
            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        private static IArticleRenderer CreateRenderer()
        {
            return new ArticleRenderer(new ConsoleLogger(true), FileConfig.Load(null));
        }
    }
}
