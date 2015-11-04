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
            var config = FileConfig.Load(null);
            if (HandleArguments(args, config))
                return;
            ILogger logger = new Log4NetLogger();
            try
            {
                logger = new CompositeLogger(logger, new EmailErrorLogger(config.NotificationEmail, config.SmtpConfig));
                using (var resources = WebReader.Login(logger, config.CorrespondentCredentails))
                {
                    var p = Program.Instance(logger, resources, config);
                    var pdfs = p.ReadDeCorrespondentAndWritePdfs();
                    p.SendToKindleAndSendNotificationMail(pdfs);
                    pdfs.Select(pdf => pdf.FileName).ToList().ForEach(p.DeleteFile);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        private static Program Instance(ILogger logger, IResourceReader resources, FileConfig config)
        {
            var newItemsParser = new NewItemsReader(logger);
            var reader = new ArticleReader();
            var renderer = new ArticleRenderer(logger, config.ArticleRendererConfig);
            var lastIdDs = new FileLastDatasource();
            var mailer = new SmtpMailer(logger, config.SmtpConfig);
            var kindle = new KindleEmailSender(config.KindleEmailSenderConfig, mailer);
            var summarySender = new EmailNotificationSender(mailer, config.EmailNotificationSenderConfig, resources);
            return new Program(logger, resources, reader, renderer, newItemsParser, lastIdDs, kindle, summarySender, config.MaxAantalArticles);
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

        public void DeleteFile(string filename)
        {
            logger.Info("Deleting file '{0}'", filename);
            File.Delete(filename);
        }
        
        public void SendToKindleAndSendNotificationMail(IList<ArticlePdf> pdfs)
        {
            if (!pdfs.Any()) 
                return;
            kindle.Send(pdfs.Select(f => (Func<FileStream>)(() => new FileStream(f.FileName, FileMode.Open))));
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

        private static bool HandleArguments(string[] args, FileConfig config)
        {
            if (args.Length == 0)
                return false;
            if (args[0] == @"?" || args[0] == @"/?" || args[0] == @"\?" || args[0] == @"-help" || args[0] == @"-h")
            {
                Console.WriteLine("Commandline parameters om je configuratie aan te passen:");
                typeof (FileConfig).GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(FileConfig.ConfigurableViaCommandLine), true).Any())
                    .Select(p => new
                    {
                        Property = p, 
                        Attribute = p.GetCustomAttributes(typeof(FileConfig.ConfigurableViaCommandLine), true).OfType<FileConfig.ConfigurableViaCommandLine>().First(),
                        Value = p.GetGetMethod().Invoke(config, new object[0]) as string
                    })
                    .ToList()
                    .ForEach(x => Console.WriteLine("\n{0}=waarde (huidige waarde: '{2}')\n {1}", x.Property.Name, x.Attribute.Description, x.Attribute.Display(x.Value)));
            }
            else
            {
                args.Select(a => a.Split('='))
                    .Where(s => s.Length == 2)
                    .Select(s => new { Key = s[0], Value = s[1], Property = typeof(FileConfig).GetProperties().FirstOrDefault(p => p.Name == s[0]) })
                    .Where(x => x.Property != null)
                    .ToList()
                    .ForEach(x => x.Property.GetSetMethod().Invoke(config, new object[] {x.Value}));
                config.Save(null);
                Console.WriteLine("Configuration saved.");
            }
            return true;
        }
    }
}
