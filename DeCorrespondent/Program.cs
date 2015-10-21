using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeCorrespondent.Impl;

namespace DeCorrespondent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var logger = new ConsoleLogger(true);
                var config = FileConfig.Load(null);
                using (var resources = WebReader.Login(logger, config.CorrespondentCredentails))
                {
                    var newItemsParser = new NewItemsReader(logger);
                    var reader = new ArticleReader();
                    var renderer = new ArticleRenderer(logger, config.ArticleRendererConfig);
                    var lastIdDs = new FileLastDatasource();
                    var mailer = new SmtpMailer(logger, config.SmtpConfig);
                    var kindle = new KindleEmailSender(config.KindleEmailSenderConfig, mailer);
                    var summarySender = new EmailNotificationSender(mailer, config.EmailNotificationSenderConfig);

                    var p = new Program(logger, resources, reader, renderer, newItemsParser, lastIdDs, kindle, summarySender, config.MaxAantalArticles);

                    var pdfs = p.ReadDeCorrespondentAndWritePdfs();
                    p.SendToKindleAndSendNotificationMail(pdfs);
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private readonly IItemsReader newItemsParser;
        private readonly IResourceReader resources;
        private readonly IArticleReader reader;
        private readonly IArticleRenderer renderer;
        private readonly ILogger logger;
        private readonly int maxAantalArticles;
        private readonly ILastDatasource lastDs;
        private readonly IEReaderSender kindle;
        private readonly INotificationSender summarySender;

        public Program(ILogger logger, IResourceReader resources, IArticleReader reader, IArticleRenderer renderer, IItemsReader newItemsParser, ILastDatasource lastDs, IEReaderSender kindle, INotificationSender summarySender, int maxAantalArticles)
        {
            this.logger = logger;
            this.resources = resources;
            this.reader = reader;
            this.renderer = renderer;
            this.newItemsParser = newItemsParser;
            this.lastDs = lastDs;
            this.kindle = kindle;
            this.summarySender = summarySender;
            this.maxAantalArticles = maxAantalArticles;
        }

        public IList<ArticlePdf> ReadDeCorrespondentAndWritePdfs()
        {
            var last = lastDs.ReadLast() ?? DateTime.Today.AddDays(-1);
            var regels = Enumerable.Range(0, int.MaxValue)
                .SelectMany(NewItems)
                .Select(reference => new { Article = ReadArticle(reference.Id), Reference = reference })
                .TakeWhile(x => x.Article.Metadata.Published > last)
                .Take(maxAantalArticles);
            var result = new List<ArticlePdf>();
            foreach (var r in regels)
            {
                var pdf = RenderArticle(r.Article, r.Reference.Id);
                var file = WritePdf(pdf, r.Article);
                lastDs.UpdateLast(DateTime.Now);
                result.Add(new ArticlePdf(file, r.Article));
            }
            return result;
        }
        
        public void SendToKindleAndSendNotificationMail(IList<ArticlePdf> pdfs)
        {
            if (!pdfs.Any()) 
                return;
            kindle.Send(pdfs.Select(f => new FileStream(f.FileName, FileMode.Open)));
            summarySender.Send(pdfs.Select(f => f.Article));
        }
        private string WritePdf(IArticleEbook ebook, IArticle article)
        {
            File.WriteAllBytes(ebook.Name, ebook.Content);
            File.SetCreationTime(ebook.Name, article.Metadata.Published);
            File.SetLastWriteTime(ebook.Name, article.Metadata.Modified);
            logger.Debug("Writing article file: '{0}'", ebook.Name);
            return ebook.Name;
        }

        private IArticleEbook RenderArticle(IArticle a, int id)
        {
            logger.Debug("Reading article: " + id);
            return renderer.Render(a);
        }

        private IArticle ReadArticle(int id)
        {
            return reader.Read( resources.ReadArticle(id) );
        }

        private IEnumerable<IArticleReference> NewItems(int nieuwPagina)
        {
            return newItemsParser.ReadItems(resources.ReadNewItems(nieuwPagina));
        }

        public class ArticlePdf
        {
            internal ArticlePdf(string filename, IArticle article)
            {
                FileName = filename;
                Article = article;
            }
            public string FileName { get; private set; }
            public IArticle Article { get; private set; }
        }

        private static void HandleError(Exception e)
        {
            try
            {
                var config = FileConfig.Load(null);
                var body = string.Format("<p>Fout: {0}</p><pre>{1}</pre>", e.Message, e.StackTrace);
                new SmtpMailer(new ConsoleLogger(true), config).Send(config.NotificationEmail, "DeCorrespondent.exe. Fout opgetreden", body, null);
                throw e;
            }
            catch (Exception)
            {
                throw e;
            }
        }
    }

}
