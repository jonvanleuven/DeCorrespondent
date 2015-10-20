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
            var logger = new ConsoleLogger(true);
            var config = FileConfig.Load(null);
            using (var resources = WebReader.Login(logger, config.CorrespondentCredentails))
            {
                var newItemsParser = new NewItemsReader(logger);
                var reader = new ArticleReader();
                var renderer = new ArticleRenderer(logger, config.ArticleRendererConfig);
                var lastIdDs = new FileLastDatasource();
                var sender = new KindleEmailSender(logger, config.KindleEmailSenderConfig);
                //var summarySender = new EmailSummarySender(logger, config.EmailSummarySenderConfig);

                var p = new Program(logger, resources, reader, renderer, newItemsParser, lastIdDs, sender, config.MaxAantalArticles);

                p.SendPdfs();
            }
        }

        private readonly IItemsReader newItemsParser;
        private readonly IResourceReader resources;
        private readonly IArticleReader reader;
        private readonly IArticleRenderer renderer;
        private readonly ILogger logger;
        private readonly int maxAantalArticles;
        private readonly ILastDatasource lastDs;
        private readonly IArticleSender sender;

        public Program(ILogger logger, IResourceReader resources, IArticleReader reader, IArticleRenderer renderer, IItemsReader newItemsParser, ILastDatasource lastDs, IArticleSender sender, int maxAantalArticles)
        {
            this.logger = logger;
            this.resources = resources;
            this.reader = reader;
            this.renderer = renderer;
            this.newItemsParser = newItemsParser;
            this.lastDs = lastDs;
            this.sender = sender;
            this.maxAantalArticles = maxAantalArticles;
        }

        public void SendPdfs()
        {
            var last = lastDs.ReadLast() ?? DateTime.Today.AddDays(-1);
            var regels = Enumerable.Range(0, int.MaxValue)
                .SelectMany(NewItems)
                .Select(reference => new { Article = ReadArticle(reference.Id), Reference = reference })
                .TakeWhile(x => x.Article.Metadata.Published > last)
                .Take(maxAantalArticles);
            List<string> files = new List<string>();
            foreach (var r in regels)
            {
                var pdf = RenderArticle(r.Article, r.Reference.Id);
                var file = WritePdf(pdf, r.Article);
                lastDs.UpdateLast(DateTime.Now);
                files.Add(file);
            }
            sender.Send(files.Select(f => new FileStream(f, FileMode.Open)));
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

    }
}
