using System.IO;
using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void GetItemsAndRenderPdf()
        {
            var reader = WebReaderTest.CreateReader();
            var renderer = new ArticleRenderer(new ConsoleLogger(true), FileConfig.Load(null));
            var items = new NewItemsReader(new ConsoleLogger(true), reader).ReadItems(null);
            var articles = items.Select(i => i.ReadArticle());

            var ebooks = renderer.Render(articles);

            new KindleEmailSender(new ConsoleLogger(true), null).Send(ebooks);

            foreach (var r in items)
            {
                var a = r.ReadArticle();
                File.WriteAllText("d:\\temp\\CP\\" + ArticleRenderer.FormatName(string.Format("{0} {1}-{2}", a.ReadingTime, a.AuthorSurname, a.Title)) + ".html", a.Html);
            }
            foreach (var article in ebooks)
            {
                File.WriteAllBytes("d:\\temp\\CP\\" + article.Name, article.Content );
            }
        }

        [Test]
        public void CreateConfigFile()
        {
            var config = new FileConfig();
            config.Username = "username";
            config.Password = "password";
            config.LicenseKey = "lickey";
            config.Save("d:\\config.xml");
        }
    }
}
