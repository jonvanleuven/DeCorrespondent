using System;
using System.Collections.Generic;
using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void GetItemsFromWebAndRenderPdf()
        {
            using (var webresources = WebReader.Login(new ConsoleLogger(true), FileConfig.Load(null)))
            {
                var program = CreateProgram(webresources);

                program.Program.WritePdfs();

                Assert.IsTrue(program.LastDs.ReadLast().HasValue);
            }
        }

        [Test]
        public void ReadItemsSubset()
        {
            var program = CreateProgram(new NewItemsReaderTest.FileResources(), new DateTime(2015, 9, 14, 12, 0, 0));

            program.Program.WritePdfs();

            Assert.IsNotNull(program.LastDs.ReadLast());
            Assert.AreEqual(1, program.NumberNieuwRequests);
            Assert.AreEqual(3, program.NumberArticleRequests);
            Assert.AreEqual(3352, program.ArticlesRequested.First(), "volgorde is niet juist, moet van nieuw naar oud gesorteerd zijn");
        }

        [Test]
        public void CreateConfigFile()
        {
            var config = new FileConfig();
            config.Username = "username";
            config.Password = "password";
            config.LicenseKey = "lickey";
            config.MaxAantalArticles = 20;
            config.Save("d:\\config.xml");
        }

        [Test]
        public void ReadPublicationDates()
        {
            var logger = new ConsoleLogger(true);
            var newItemsReader = new NewItemsReader(logger);
            var reader = new ArticleReader();
            using (var webresources = WebReader.Login(new ConsoleLogger(true), FileConfig.Load(null)))
            {
                var regels = Enumerable.Range(0, int.MaxValue)
                    .SelectMany(index => newItemsReader.ReadItems(webresources.ReadNewItems(index)))
                    .Select(r => new {reader.Read(webresources.ReadArticle(r.Id)).Publicationdate, Reference = r})
                    .TakeWhile(x => x.Publicationdate > new DateTime(2015, 9, 30))
                    .Take(10) //max 10
                    .ToList();
                foreach (var regel in regels)
                {
                    Console.WriteLine(regel.Reference.Id + ": " + regel.Publicationdate);
                }
            }
        }

        [Test]
        public void AssertDeferredExecution()
        {
            var program = CreateProgram(new NewItemsReaderTest.FileResources(), new DateTime(2015, 9, 14, 12, 0, 0));

            program.Program.WritePdfs();

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
            var config = FileConfig.Load(null);
            return new ProgramWrapper(logger, resources, new ArticleReader(), new ArticleRenderer(logger, config), new NewItemsReader(logger), lastId);
        }

        public class LogWrapper : ILogger
        {
            private readonly ILogger logDelegate;
            internal LogWrapper(ILogger logDelegate)
            {
                this.logDelegate = logDelegate;
                Infos = new List<string>();
                Debugs = new List<string>();
            }

            public List<string> Infos { get; private set; }
            public List<string> Debugs { get; private set; }

            public void Info(string message, params object[] args)
            {
                logDelegate.Info(message, args);
                Infos.Add(string.Format(message, args));
            }

            public void Debug(string message, params object[] args)
            {
                logDelegate.Debug(message, args);
                Debugs.Add(string.Format(message, args));
            }
        }

        class ProgramWrapper
        {
            private readonly WrappedResources wrappedResources;
            private readonly LogWrapper logger;

            public ProgramWrapper(LogWrapper logger, IResourceReader resources, IArticleReader articleReader, IArticleRenderer articleRenderer, IItemsReader newItemsReader, DateTime? last)
            {
                this.logger = logger;
                wrappedResources = new WrappedResources(resources);
                LastDs = new MemoryLastDatasource(last);
                Program = new DeCorrespondent.Program(logger, wrappedResources, articleReader, articleRenderer, newItemsReader, LastDs, 20);
            }

            public IList<string> InfoLog { get { return logger.Infos; } }
            public IList<string> DebugLog { get { return logger.Debugs; } }
            public DeCorrespondent.Program Program { get; private set; }
            public int NumberArticleRequests { get { return wrappedResources.ArticlesRequested.Count; } }
            public int NumberNieuwRequests { get { return wrappedResources.NieuwpaginaRequested.Count; } }
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
                NieuwpaginaRequested = new List<int>();
                
            }

            public List<int> ArticlesRequested { get; private set; }
            public List<int> NieuwpaginaRequested { get; private set; }

            public string ReadNewItems(int index)
            {
                NieuwpaginaRequested.Add(index);
                return delegateReader.ReadNewItems(index);
            }

            public string ReadArticle(int articleId)
            {
                ArticlesRequested.Add(articleId);
                return delegateReader.ReadArticle(articleId);
            }

            public void Dispose()
            {
                delegateReader.Dispose();
            }
        }

    }
}
