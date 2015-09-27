using System;
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

            var result = reader.ReadItems(new FileResources().ReadNewItems(0));

            Assert.AreEqual(10, result.Count());
            Assert.AreEqual(3352, result.First().Id);
        }

       

        [Test]
        public void ReadArticle()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(1));

            Assert.NotNull(result.Html);
            File.WriteAllText("d:\\test.html", result.Html);
            Assert.IsFalse(result.Html.Contains("<script"));
            Assert.IsFalse(result.Html.Contains("publication-sidenote "));
            Assert.IsTrue(result.Html.Contains("publication-sidenote-link"));
            Assert.IsFalse(result.Html.Contains("share-publication-footer"));
            Assert.IsFalse(result.Html.Contains("publication-body-link"));
            Assert.IsFalse(result.Html.Contains("class=\"header"));
            Assert.IsFalse(result.Html.Contains("8 uur geleden"));
            Assert.IsTrue(result.Html.Contains("16-9-2015 5:45"));
            Assert.AreEqual("Drie manieren waarop mobiele telefoons bijdragen aan betere data", result.Title);
            Assert.AreEqual("6-7", result.ReadingTime);
            Assert.AreEqual("Blauw", result.AuthorSurname);
        }

        [Test]
        public void ReadArticle3358()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3358));
            
            Assert.NotNull(result.Html);
        }

        [Test]
        public void ReadArticle3364()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3364));

            Assert.NotNull(result.Html);
        }

        [Test]
        public void ReadArticle3366()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3366));
            
            Assert.NotNull(result.Html);
            Assert.IsTrue(result.Html.Contains("een simpele telnet-hack"));
        }

        private static NewItemsReader CreateReader()
        {
            return new NewItemsReader(new ConsoleLogger(true));
        }

        public class FileResources : IResourceReader
        {
            public string ReadNewItems(int index)
            {
                var resource = GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources.nieuw_" + index);
                if (resource == null)
                    throw new Exception("New item resource not found: " + index);
                using (var s = new StreamReader(resource))
                {
                    return s.ReadToEnd();
                }
            }

            public string ReadArticle(int articleId)
            {
                var resource = GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources.article_" + articleId);
                if (resource == null)
                    throw new Exception("Article resource not found: " + articleId);
                using (var s = new StreamReader(resource))
                {
                    return s.ReadToEnd();
                }
            }

            public void Dispose()
            {
            }
        }

    }
}
