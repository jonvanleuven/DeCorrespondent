using System.IO;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class PdfArticleRendererTest
    {
        [Test]
        public void Render()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3358"));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            Assert.AreEqual("16 Reis mee door het land dat niet bestaat (en leer hoe belangrijk het is om erkend te worden).pdf", pdf.Name);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3361()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3361"));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);
            
            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3450()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3450"));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3444()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3444"));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        [Test]
        public void Render3430()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3430"));
            var renderer = CreateRenderer();

            var pdf = renderer.Render(article);

            Assert.NotNull(pdf.Content);
            File.WriteAllBytes("d:\\" + pdf.Name, pdf.Content);
        }

        private static IArticleRenderer CreateRenderer()
        {
            var config = FileConfig.Load(@"..\..\config-test.xml");
            return new PdfArticleRenderer(new ConsoleLogger(true), config.ArticleRendererConfig, config.EvoPdfLicenseKey);
        }
    }
}
