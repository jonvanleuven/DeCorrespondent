using System.IO;
using System.Linq;
using DeCorrespondent.Impl;

namespace DeCorrespondent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var lastId = File.Exists("lastId.txt") ? int.Parse(File.ReadAllText("lastId.txt")) : (int?) null;

            var logger = new ConsoleLogger(true);
            var config = FileConfig.Load(null);
            var reader = new NewItemsReader(logger, WebReader.Login(logger, config.CorrespondentCredentails));
            var renderer = new ArticleRenderer(logger, config.ArticleRendererConfig);
            var sender = new KindleEmailSender(logger, config.KindleEmailSenderConfig);
            var summarySender = new EmailSummarySender(logger, config.EmailSummarySenderConfig);

            var refs = reader.ReadItems(lastId).ToList();
            var ebooks = renderer.Render(refs.Select(r => r.ReadArticle()));

            sender.Send(ebooks);
            //TODO summarySender.Send(ebooks);

            //temp:
            foreach (var r in refs)
            {
                var a = r.ReadArticle();
                File.WriteAllText(ArticleRenderer.FormatName(string.Format("{0} {1}-{2}", a.ReadingTime, a.AuthorSurname, a.Title)) + ".html", a.Html);
            }
            foreach (var article in ebooks)
            {
                File.WriteAllBytes(article.Name, article.Content);
                File.WriteAllText("lastId.txt", "" + refs.First().Id);
            }
        }
    }
}
