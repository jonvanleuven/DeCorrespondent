using System.IO;
using System.Linq;
using NUnit.Framework;
using DeCorrespondent.Impl;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class NewItemsReaderTest
    {
        [Test]
        public void ReadItems()
        {
            var reader = CreateReader();

            var result = reader.ReadItems(null);

            Assert.AreEqual(10, result.Count());
            Assert.AreEqual(3341, result.First().Id);
        }

        [Test]
        public void ReadItemsSubset()
        {
            var reader = CreateReader();

            var result = reader.ReadItems(3339);

            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void ReadArticle()
        {
            var reference = new ArticleReference(1, new FileResources(), new ConsoleLogger(true));

            var result = reference.ReadArticle();

            Assert.NotNull(result.Html);
            File.WriteAllText("d:\\test.html", result.Html);
            Assert.IsFalse(result.Html.Contains("<script"));
            Assert.IsFalse(result.Html.Contains("publication-sidenote"));
            Assert.IsFalse(result.Html.Contains("share-publication-footer"));
            Assert.IsFalse(result.Html.Contains("publication-body-link"));
            Assert.IsFalse(result.Html.Contains("class=\"header"));
            Assert.NotNull(result.Reference);
            Assert.AreEqual("Drie manieren waarop mobiele telefoons bijdragen aan betere data", result.Title);
            Assert.AreEqual("6-7", result.ReadingTime);
            Assert.AreEqual("Blauw", result.AuthorSurname);
        }

        [Test]
        public void ReadArticle3358()
        {
            var reference = new ArticleReference(3358, new FileResources(), new ConsoleLogger(true));

            var result = reference.ReadArticle();

            Assert.NotNull(result.Html);
        }

        [Test]
        public void ReadArticle3364()
        {
            var reference = new ArticleReference(3364, new FileResources(), new ConsoleLogger(true));

            var result = reference.ReadArticle();

            Assert.NotNull(result.Html);
        }

        private static NewItemsReader CreateReader()
        {
            return new NewItemsReader(new ConsoleLogger(true), new FileResources());
        }

        public class FileResources : IResourceReader
        {
            public string ReadNewItems()
            {
                using (var s = new StreamReader(GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources.nieuw")))
                {
                    return s.ReadToEnd();
                }
            }

            public string ReadArticle(IArticleReference reference)
            {
                using (var s = new StreamReader(GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources.article_" + reference.Id)))
                {
                    return s.ReadToEnd();
                }
            }
        }

    }
}
