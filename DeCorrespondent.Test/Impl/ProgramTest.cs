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
            var program = CreateProgram(WebReaderTest.CreateReader()).Program;

            var result = program.WritePdfs(null);

            Assert.IsTrue(result.HasValue);
        }

        [Test]
        public void ReadItemsSubset()
        {
            var program = CreateProgram(new NewItemsReaderTest.FileResources());

            var result = program.Program.WritePdfs(3339);

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

        private ProgramWrapper CreateProgram(IResourceReader resources)
        {
            var logger = new ConsoleLogger(true);
            var config = FileConfig.Load(null);
            return new ProgramWrapper(logger, resources, new ArticleReader(logger), new ArticleRenderer(logger, config), new NewItemsReader(logger));
        }

        class ProgramWrapper
        {
            private readonly WrappedResources wrappedResources;
            public ProgramWrapper(ILogger logger, IResourceReader resources, IArticleReader articleReader, IArticleRenderer articleRenderer, IItemsReader newItemsReader)
            {
                wrappedResources = new WrappedResources(resources);
                Program = new DeCorrespondent.Program(logger, wrappedResources, articleReader, articleRenderer, newItemsReader, 20);
            }

            public DeCorrespondent.Program Program{ get; private set; }
            public int NumberArticleRequests { get { return wrappedResources.ArticlesRequested.Count; } }
            public int NumberNieuwRequests { get { return wrappedResources.NieuwpaginaRequested.Count; } }
            public IList<int> ArticlesRequested { get { return wrappedResources.ArticlesRequested; } }
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
