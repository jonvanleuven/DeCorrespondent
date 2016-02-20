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
            var arguments = HandleArguments(args, config);
            if (!arguments.RunProgram)
                return;
            ILogger logger = new Log4NetLogger();
            try
            {
                logger = new CompositeLogger(logger, new EmailErrorLogger(config.NotificationEmail, config.SmtpMailConfig));
                var login = !string.IsNullOrEmpty(config.DeCorrespondentReaderConfig.Username);
                using (var session = (login ? DeCorrespondentResources.Login(config.DeCorrespondentReaderConfig, logger) : RssFeedResources.Instance(logger)))
                {
                    var p = Program.Instance(arguments, logger, session, config);
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

        private static Program Instance(ProgramArguments args, ILogger logger, IDeCorrespondentResources decorrespondent, FileConfig config)
        {
            var reader = new ArticleReader();
            var renderer = string.IsNullOrEmpty(config.EvoPdfLicenseKey) 
                ? new HtmlArticleRenderer(logger, config.ArticleRendererConfig)
                : new PdfArticleRenderer(logger, config.ArticleRendererConfig, config.EvoPdfLicenseKey) as IArticleRenderer;
            var lastIdDs = new FileLastDatasource();
            var mailer = new SmtpMailer(logger, config.SmtpMailConfig);
            var kindle = new KindleEmailSender(logger, config.KindleEmailSenderConfig, mailer);
            var summarySender = new EmailNotificationSender(logger, mailer, config.EmailNotificationSenderConfig);
            return new Program(args, logger, reader, renderer, decorrespondent, lastIdDs, kindle, summarySender, config.MaxAantalArticles);
        }

        private readonly IDeCorrespondentResources decorrespondent;
        private readonly IArticleReader reader;
        private readonly IArticleRenderer renderer;
        private readonly ILogger logger;
        private readonly int maxAantalArticles;
        private readonly ILastDatasource lastDs;
        private readonly IEReaderSender kindle;
        private readonly INotificationSender summarySender;
        private readonly ProgramArguments args;

        public Program(ProgramArguments args, ILogger logger, IArticleReader reader, IArticleRenderer renderer, IDeCorrespondentResources decorrespondent, ILastDatasource lastDs, IEReaderSender kindle, INotificationSender summarySender, int maxAantalArticles)
        {
            this.args = args;
            this.logger = logger;
            this.reader = reader;
            this.renderer = renderer;
            this.decorrespondent = decorrespondent;
            this.lastDs = lastDs;
            this.kindle = kindle;
            this.summarySender = summarySender;
            this.maxAantalArticles = maxAantalArticles;
        }

        public IList<ArticlePdf> ReadDeCorrespondentAndWritePdfs()
        {
            var last = lastDs.ReadLast() ?? DateTime.Today.AddDays(-1);
            var query = args.ArticleId.HasValue
                ? new[] {args.ArticleId.Value}
                : decorrespondent.ReadNieuwItems()
                    .TakeWhile(i => i.Publicationdate > last)
                    .Select(i => i.Id);

            if (args.RunInteractiveMode)
                query = query.Where(AskIncludeArticle).ToList();

            var regels = query
                    .Select(id => new { Article = ReadArticle(id), Id = id })
                    .Take(maxAantalArticles);
            var result = new List<ArticlePdf>();
            foreach (var r in regels)
            {
                var pdf = RenderArticle(r.Article, r.Id);
                var file = WritePdf(pdf, r.Article);
                lastDs.UpdateLast(DateTime.Now);
                result.Add(new ArticlePdf(file, r.Article));
            }
            lastDs.UpdateLast(DateTime.Now);
            return result;
        }

        private bool AskIncludeArticle(int id)
        {
            var article = reader.Read(decorrespondent.ReadArticle(id));
            Console.WriteLine(string.Empty);
            Console.WriteLine("** {0} {1} - {2} **", article.Metadata.AuthorFirstname, article.Metadata.AuthorLastname, article.Metadata.Section);
            Console.WriteLine("** {0} **", article.Metadata.Title);
            Console.WriteLine("** Leestijd: {0} **", article.Metadata.ReadingTimeDisplay);
            Console.WriteLine(ToTextBlock(70, article.Metadata.Description, "  "));
            Console.Write("Ja of Nee (J/N)? ");
            var key = Console.ReadKey();
            Console.WriteLine(string.Empty);
            return key.KeyChar.ToString().ToLower() == "j" ||
                   key.KeyChar.ToString().ToLower() == "y";
        }

        private static string ToTextBlock(int width, string text, string linePrefix = null)
        {
            var words = text.Split(' ');
            var line = 1;
            return (linePrefix??string.Empty) + words.Aggregate(string.Empty, (left, right) => {
                if (line * width < (left.Length+right.Length))
                {
                    line++;
                    return left + "\n" + (linePrefix ?? string.Empty) + right;
                }
                return left + " " + right;
            }).Trim();
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
            return reader.Read(decorrespondent.ReadArticle(id));
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
            if (args.Length == 0)
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
            else if (args.Length > 0 && args[0].StartsWith("/i"))
            {
                var result = new ProgramArguments(true);
                result.RunInteractiveMode = true;
                return result;
            }
            else
            {
                Console.WriteLine("/config : Pas de configuratie aan (config.xml)");
                Console.WriteLine("/id={articleId} : Run voor 1 specifiek artikel (id)");
                Console.WriteLine("/i : Interactieve modus: vraag welke artikelen te versturen");
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
            public bool RunInteractiveMode { get; set; }
        }
    }
}
