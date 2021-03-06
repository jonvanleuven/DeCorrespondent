using System;
using System.Collections.Generic;
using System.Linq;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void GetItemsFromWebAndRenderPdf()
        {
            var config = FileConfig.Load(@"..\..\config-test.xml").DeCorrespondentReaderConfig;
            using (var webresources = DeCorrespondentWebReader.Login(new ConsoleLogger(true), config.Username, config.Password))
            {
                var program = CreateProgram(webresources);

                program.Program.ReadDeCorrespondentAndWritePdfs();

                Assert.IsTrue(program.LastDs.ReadLast().HasValue);
            }
        }

        [Test]
        public void ReadItemsSubset()
        {
            var program = CreateProgram(new FileResources(), new DateTime(2015, 9, 14, 12, 0, 0));

            program.Program.ReadDeCorrespondentAndWritePdfs();

            Assert.IsNotNull(program.LastDs.ReadLast());
            Assert.AreEqual(1, program.NumberNieuwRequests);
            Assert.AreEqual(2, program.NumberArticleRequests);
            Assert.AreEqual(3352, program.ArticlesRequested.First(), "volgorde is niet juist, moet van nieuw naar oud gesorteerd zijn");
        }

        [Test]
        public void CreateConfigFile()
        {
            var config = new FileConfig();
            config.DeCorrespondentUsername = "username";
            config.DeCorrespondentPassword = Encryptor.EncryptAES("password");
            config.EvoPdfLicenseKey = "lickey";
            config.MaxAantalArticles = 20;
            config.KindleEmail = "t";
            config.SmtpUsername = "t";
            config.SmtpPassword = Encryptor.EncryptAES(@"password");
            config.Save("d:\\config.xml");
        }

        /*[Test]
        public void ReadPublicationDates()
        {
            var logger = new ConsoleLogger(true);
            var newItemsReader = new NewItemsReader(logger);
            var reader = new ArticleReader();
            using (var webresources = WebReader.Login(new ConsoleLogger(true), FileConfig.Load(@"..\..\config-test.xml").WebReaderConfig))
            {
                var regels = Enumerable.Range(0, int.MaxValue)
                    .SelectMany(index => newItemsReader.ReadItems(webresources.ReadNewItems(index)))
                    .Select(r => new { Publicationdate = reader.Read(webresources.ReadArticle(r.Id)).Metadata.Published, Reference = r })
                    .TakeWhile(x => x.Publicationdate > new DateTime(2015, 9, 30))
                    .Take(10) //max 10
                    .ToList();
                foreach (var regel in regels)
                {
                    Console.WriteLine(regel.Reference.Id + ": " + regel.Publicationdate);
                }
            }
        }*/

        [Test]
        public void AssertDeferredExecution()
        {
            var program = CreateProgram(new FileResources(), new DateTime(2015, 9, 14, 12, 0, 0));

            program.Program.ReadDeCorrespondentAndWritePdfs();

            Assert.IsTrue(program.DebugLog[0].StartsWith("Reading article"));
            Assert.IsTrue(program.DebugLog[1].StartsWith("Rendering article"));
            Assert.IsTrue(program.DebugLog[2].StartsWith("Writing article"));

            Assert.IsTrue(program.DebugLog[3].StartsWith("Reading article"));
            Assert.IsTrue(program.DebugLog[4].StartsWith("Rendering article"));
            Assert.IsTrue(program.DebugLog[5].StartsWith("Writing article"));
        }

        private static ProgramWrapper CreateProgram(IResourceReader resources, DateTime? lastId = null)
        {
            var logger = new LogWrapper(new ConsoleLogger(true));
            var config = FileConfig.Load(@"..\..\config-test.xml");
            var mailer = new SmtpMailer(logger, config.SmtpMailConfig);
            var r = new WrappedResources(resources);
            return new ProgramWrapper(logger, 
                r, 
                new ArticleReader(), 
                new PdfArticleRenderer(logger, config, config.EvoPdfLicenseKey), 
                new DeCorrespondentResources(r, logger), 
                new KindleEmailSender(logger, config.KindleEmailSenderConfig, mailer),
                new EmailNotificationSender(logger, mailer, config.EmailNotificationSenderConfig), 
                lastId );
        }

        class ProgramWrapper
        {
            private readonly WrappedResources wrappedResources;
            private readonly LogWrapper logger;

            public ProgramWrapper(LogWrapper logger, WrappedResources resources, IArticleReader articleReader, IArticleRenderer articleRenderer, IDeCorrespondentResources reader, IEReaderSender sender, INotificationSender summarySender, DateTime? last)
            {
                this.logger = logger;
                wrappedResources = resources;
                LastDs = new MemoryLastDatasource(last);
                Program = new DeCorrespondent.Program(new DeCorrespondent.Program.ProgramArguments(true), logger, articleReader, articleRenderer, reader, LastDs, sender, summarySender, 20);
            }

            public IList<string> InfoLog { get { return logger.Infos; } }
            public IList<string> DebugLog { get { return logger.Debugs; } }
            public DeCorrespondent.Program Program { get; private set; }
            public int NumberArticleRequests { get { return wrappedResources.ArticlesRequested.Count; } }
            public int NumberNieuwRequests { get { return wrappedResources.NieuwpaginaRequested; } }
            public IList<int> ArticlesRequested { get { return wrappedResources.ArticlesRequested; } }
            public ILastDatasource LastDs { get; private set; }
        }

        class MemoryLastDatasource : ILastDatasource
        {
            private DateTime? d;
            internal MemoryLastDatasource(DateTime? d)
            {
                this.d = d;
            }

            public DateTime? ReadLast()
            {
                return d;
            }

            public void UpdateLast(DateTime id)
            {
                this.d = id;
            }
        }

        class WrappedResources : IResourceReader
        {
            private readonly IResourceReader delegateReader;

            internal WrappedResources(IResourceReader delegateReader)
            {
                this.delegateReader = delegateReader;
                ArticlesRequested = new List<int>();
                NieuwpaginaRequested = 0;
                
            }

            public List<int> ArticlesRequested { get; private set; }
            public int NieuwpaginaRequested { get; private set; }

            public string Read(string url)
            {
                if (url.Contains("/nieuw"))
                    NieuwpaginaRequested++;
                else
                    ArticlesRequested.Add(int.Parse(url.Split('/').Last()));

                return delegateReader.Read(url);
            }

            public byte[] ReadBinary(string url)
            {
                return delegateReader.ReadBinary(url);
            }

            public void Dispose()
            {
                delegateReader.Dispose();
            }
        }

    }
}
