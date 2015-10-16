using System.IO;
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
            Assert.AreEqual("16 Reis mee door het land dat niet bestaat (en leer hoe belangrijk het is om erkend te worden).pdf", pdf.Name);
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

        [Test]
        public void Render3450()
        {
            var article = new ArticleReader().Read(new NewItemsReaderTest.FileResources().ReadArticle(3450));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3444()
        {
            var article = new ArticleReader().Read(new NewItemsReaderTest.FileResources().ReadArticle(3444));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3430()
        {
            var article = new ArticleReader().Read(new NewItemsReaderTest.FileResources().ReadArticle(3430));
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
