using System.IO;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class HtmlArticleRendererTest
    {
        [Test]
        public void Render()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3358"));
            var renderer = CreateRenderer();

            var Html = renderer.Render(article);

            Assert.NotNull(Html.Content);
            Assert.AreEqual("16 Reis mee door het land dat niet bestaat (en leer hoe belangrijk het is om erkend te worden) (Vermeulen).html", Html.Name);
            File.WriteAllBytes("d:\\" + Html.Name, Html.Content);
        }

        [Test]
        public void Render3785()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3785"));
            var renderer = CreateRenderer();

            var Html = renderer.Render(article);
            
            Assert.NotNull(Html.Content);
            File.WriteAllBytes("d:\\" + Html.Name, Html.Content);
        }

        [Test]
        public void Render3450()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3450"));
            var renderer = CreateRenderer();

            var Html = renderer.Render(article);

            Assert.NotNull(Html.Content);
            File.WriteAllBytes("d:\\" + Html.Name, Html.Content);
        }

        [Test]
        public void Render3444()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3444"));
            var renderer = CreateRenderer();

            var Html = renderer.Render(article);

            Assert.NotNull(Html.Content);
            File.WriteAllBytes("d:\\" + Html.Name, Html.Content);
        }

        [Test]
        public void Render3430()
        {
            var article = new ArticleReader().Read(new FileResources().Read("http://t/3430"));
            var renderer = CreateRenderer();

            var Html = renderer.Render(article);

            Assert.NotNull(Html.Content);
            File.WriteAllBytes("d:\\" + Html.Name, Html.Content);
        }

        private static IArticleRenderer CreateRenderer()
        {
            var config = FileConfig.Load(@"..\..\config-test.xml");
            return new HtmlArticleRenderer(new ConsoleLogger(true), config);
        }
    }
}
