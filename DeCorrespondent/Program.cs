using System;
using System.Collections;
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
            var arguments = HandleArguments(args, config);
            if (!arguments.RunProgram)
                return;
            ILogger logger = new Log4NetLogger();
            try
            {
                logger = new CompositeLogger(logger, new EmailErrorLogger(config.NotificationEmail, config.SmtpMailConfig));
                using (var resources = RetryWebReader.Wrap( WebReader.Login(logger, config.WebReaderConfig), logger))
                {
                    var p = Program.Instance(arguments, logger, resources, config);
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

        private static Program Instance(ProgramArguments args, ILogger logger, IResourceReader resources, FileConfig config)
        {
            var newItemsParser = new NewItemsReader(logger);
            var reader = new ArticleReader();
            var renderer = new ArticleRenderer(logger, config.ArticleRendererConfig);
            var lastIdDs = new FileLastDatasource();
            var mailer = new SmtpMailer(logger, config.SmtpMailConfig);
            var kindle = new KindleEmailSender(logger, config.KindleEmailSenderConfig, mailer);
            var summarySender = new EmailNotificationSender(logger, mailer, config.EmailNotificationSenderConfig, resources);
            return new Program(args, logger, resources, reader, renderer, newItemsParser, lastIdDs, kindle, summarySender, config.MaxAantalArticles);
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
        private readonly ProgramArguments args;

        public Program(ProgramArguments args, ILogger logger, IResourceReader resources, IArticleReader reader, IArticleRenderer renderer, IItemsReader newItemsParser, ILastDatasource lastDs, IEReaderSender kindle, INotificationSender summarySender, int maxAantalArticles)
        {
            this.args = args;
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
            var regels = args.ArticleId.HasValue
                ? new[] { new { Article = ReadArticle(args.ArticleId.Value), Id = args.ArticleId.Value } }.AsEnumerable()
                : Enumerable.Range(0, int.MaxValue)
                    .SelectMany(NewItems)
                    .Select(reference => new { Article = ReadArticle(reference.Id), reference.Id })
                    .TakeWhile(x => x.Article.Metadata.Published > last)
                    .Take(maxAantalArticles);
            var result = new List<ArticlePdf>();
            foreach (var r in regels)
            {
                var pdf = RenderArticle(r.Article, r.Id);
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

        private static ProgramArguments HandleArguments(string[] args, FileConfig config)
        {
            if (args.Length == 0 && !string.IsNullOrEmpty(config.WebReaderConfig.Username))
                return new ProgramArguments(true);
            if (args.Length > 0 && args[0] == "/config")
            {
                typeof (FileConfig).GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof (FileConfig.ConfigurableViaCommandLine), true).Any())
                    .Select(p => new
                    {
                        Property = p,
                        Attribute = p.GetCustomAttributes(typeof (FileConfig.ConfigurableViaCommandLine), true).OfType<FileConfig.ConfigurableViaCommandLine>().First(),
                        Value = p.GetGetMethod().Invoke(config, new object[0]) as string
                    })
                    .Select(x => new { x.Property, x.Attribute, x.Value, IsEncrypted = x.Attribute.IsPassword })
                    .ToList()
                    .ForEach(x =>
                    {
                        Console.WriteLine("{0}:\n [enter]='{1}'", x.Attribute.Description, x.Attribute.Display(x.Value));
                        var newValue = Console.ReadLine();
                        if (!string.IsNullOrEmpty(newValue))
                            x.Property.GetSetMethod().Invoke(config, new object[] { x.IsEncrypted ? Encryptor.EncryptAES(newValue) : newValue });
                    });
                config.Save(null);
                Console.WriteLine("Configuratie opgeslagen.");
                return new ProgramArguments(false);
            }
            else if (args.Length > 0 && args[0].StartsWith("/id="))
            {
                var result = new ProgramArguments(true);
                result.ArticleId = int.Parse(args[0].Split('=')[1]);
                return result;
            }
            else
            {
                Console.WriteLine("/config : Pas de configuratie aan (config.xml)");
                Console.WriteLine("/id={articleId} : Run voor 1 specifiek artikel (id)");
                return new ProgramArguments(false);
            }
        }

        public class ProgramArguments
        {
            public ProgramArguments(bool run)
            {
                RunProgram = run;
            }
            public bool RunProgram { get; private set; }
            public int? ArticleId { get; set; }
        }
    }
}
