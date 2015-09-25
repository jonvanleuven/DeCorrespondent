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
            var lastId = File.Exists("lastId.txt") ? int.Parse(File.ReadAllText("lastId.txt")) : (int?) null;

            var logger = new ConsoleLogger(true);
            var config = FileConfig.Load(null);
            var resources = WebReader.Login(logger, config.CorrespondentCredentails);
            var newItemsParser = new NewItemsReader(logger);
            var reader = new ArticleReader(logger);
            var renderer = new ArticleRenderer(logger, config.ArticleRendererConfig);
            //var sender = new KindleEmailSender(logger, config.KindleEmailSenderConfig);
            //var summarySender = new EmailSummarySender(logger, config.EmailSummarySenderConfig);

            var p = new Program(logger, resources, reader, renderer, newItemsParser, config.MaxAantalArticles);

            var newLastId = p.WritePdfs(lastId);

            if (newLastId.HasValue )
                File.WriteAllText("lastId.txt", "" + newLastId);
        }

        private readonly IItemsReader newItemsParser;
        private readonly IResourceReader resources;
        private readonly IArticleReader reader;
        private readonly IArticleRenderer renderer;
        private readonly ILogger logger;
        private readonly int maxAantalArticles;

        public Program(ILogger logger, IResourceReader resources, IArticleReader reader, IArticleRenderer renderer, IItemsReader newItemsParser, int maxAantalArticles)
        {
            this.logger = logger;
            this.resources = resources;
            this.reader = reader;
            this.renderer = renderer;
            this.newItemsParser = newItemsParser;
            this.maxAantalArticles = maxAantalArticles;
        }

        public int? WritePdfs(int? lastId)
        {
            return Enumerable.Range(0, int.MaxValue)
                .SelectMany(i => NewItems(i))
                .TakeWhile(reference => reference.Id != lastId)
                .Take(maxAantalArticles)
                .Reverse()
                .Select(reference => new { Reference = reference, Article = ReadArticle(reference.Id) })
                .Select(x => new { x.Reference, Pdf = RenderArticle(x.Article)})
                .Select(x => new { x.Reference, FileName = WritePdf(x.Pdf, x.Reference) })
                .Select(x => (int?)x.Reference.Id)
                .ToList()
                .LastOrDefault();
        }

        private int WritePdf(IArticleEbook ebook, IArticleReference reference)
        {
            File.WriteAllBytes(ebook.Name, ebook.Content);
            logger.Info("File written: '{0}'", ebook.Name);
            return reference.Id;
        }

        private IArticleEbook RenderArticle(IArticle a)
        {
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
