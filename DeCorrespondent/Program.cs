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
                var lastIdDs = new FileLastIdDatasource();
                //var sender = new KindleEmailSender(logger, config.KindleEmailSenderConfig);
                //var summarySender = new EmailSummarySender(logger, config.EmailSummarySenderConfig);

                var p = new Program(logger, resources, reader, renderer, newItemsParser, lastIdDs, config.MaxAantalArticles);

                p.WritePdfs();
            }
        }

        private readonly IItemsReader newItemsParser;
        private readonly IResourceReader resources;
        private readonly IArticleReader reader;
        private readonly IArticleRenderer renderer;
        private readonly ILogger logger;
        private readonly int maxAantalArticles;
        private readonly ILastIdDatasource lastIdDS;

        public Program(ILogger logger, IResourceReader resources, IArticleReader reader, IArticleRenderer renderer, IItemsReader newItemsParser, ILastIdDatasource lastIdDS, int maxAantalArticles)
        {
            this.logger = logger;
            this.resources = resources;
            this.reader = reader;
            this.renderer = renderer;
            this.newItemsParser = newItemsParser;
            this.lastIdDS = lastIdDS;
            this.maxAantalArticles = maxAantalArticles;
        }

        public void WritePdfs()
        {
            var lastId = lastIdDS.ReadLastId();
            var references = Enumerable.Range(0, int.MaxValue)
                .SelectMany(NewItems)
                .TakeWhile(reference => reference.Id != lastId)
                .Take(maxAantalArticles)
                .Reverse();
            foreach (var reference in references)
            {
                var article = ReadArticle(reference.Id);
                var pdf = RenderArticle(article, reference.Id);
                WritePdf(pdf);
                lastIdDS.UpdateLastId(reference.Id);
            }
        }

        private void WritePdf(IArticleEbook ebook)
        {
            File.WriteAllBytes(ebook.Name, ebook.Content);
            logger.Debug("File written: '{0}'", ebook.Name);
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
