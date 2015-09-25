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
            var program = CreateProgram(WebReader.Login(new ConsoleLogger(true), FileConfig.Load(null)));

            program.Program.WritePdfs();

            Assert.IsTrue(program.LastIdDs.ReadLastId().HasValue);
        }

        [Test]
        public void ReadItemsSubset()
        {
            var program = CreateProgram(new NewItemsReaderTest.FileResources(), 3339);

            program.Program.WritePdfs();

            Assert.AreEqual(3352, program.LastIdDs.ReadLastId());
            Assert.AreEqual(1, program.NumberNieuwRequests);
            Assert.AreEqual(3, program.NumberArticleRequests);
            Assert.AreEqual(3336, program.ArticlesRequested.First(), "volgorde is niet juist, moet van oud naar nieuw gesorteerd zijn");
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

        private static ProgramWrapper CreateProgram(IResourceReader resources, int? lastId = null)
        {
            var logger = new ConsoleLogger(true);
            var config = FileConfig.Load(null);
            return new ProgramWrapper(logger, resources, new ArticleReader(logger), new ArticleRenderer(logger, config), new NewItemsReader(logger), lastId);
        }

        class ProgramWrapper
        {
            private readonly WrappedResources wrappedResources;
            public ProgramWrapper(ILogger logger, IResourceReader resources, IArticleReader articleReader, IArticleRenderer articleRenderer, IItemsReader newItemsReader, int? lastId)
            {
                wrappedResources = new WrappedResources(resources);
                LastIdDs = new MemoryLastIdDatasource(lastId);
                Program = new DeCorrespondent.Program(logger, wrappedResources, articleReader, articleRenderer, newItemsReader, LastIdDs, 20);
            }

            public DeCorrespondent.Program Program{ get; private set; }
            public int NumberArticleRequests { get { return wrappedResources.ArticlesRequested.Count; } }
            public int NumberNieuwRequests { get { return wrappedResources.NieuwpaginaRequested.Count; } }
            public IList<int> ArticlesRequested { get { return wrappedResources.ArticlesRequested; } }
            public ILastIdDatasource LastIdDs { get; private set; }
        }

        class MemoryLastIdDatasource : ILastIdDatasource
        {
            private int? id;
            internal MemoryLastIdDatasource(int? id)
            {
                this.id = id;
            }

            public int? ReadLastId()
            {
                return id;
            }

            public void UpdateLastId(int id)
            {
                this.id = id;
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
        }

    }
}
