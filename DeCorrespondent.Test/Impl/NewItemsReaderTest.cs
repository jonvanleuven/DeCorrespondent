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

            Assert.NotNull(result.BodyHtml);
            File.WriteAllText("d:\\test.BodyHtml", result.BodyHtml);
            Assert.IsFalse(result.BodyHtml.Contains("<script"));
            Assert.IsFalse(result.BodyHtml.Contains("publication-sidenote "));
            Assert.IsTrue(result.BodyHtml.Contains("publication-sidenote-link"));
            Assert.IsFalse(result.BodyHtml.Contains("share-publication-footer"));
            Assert.IsFalse(result.BodyHtml.Contains("publication-body-link"));
            Assert.IsFalse(result.BodyHtml.Contains("class=\"header"));
            Assert.IsFalse(result.BodyHtml.Contains("8 uur geleden"));
            Assert.IsTrue(result.BodyHtml.Contains("16-9-2015 5:45"));
            Assert.AreEqual("Drie manieren waarop mobiele telefoons bijdragen aan betere data", result.Metadata.Title);
            Assert.AreEqual("6-7", result.Metadata.ReadingTime);
            Assert.AreEqual("Blauw", result.Metadata.AuthorSurname);
            Assert.AreEqual(new DateTime(2015, 9, 16, 5, 45, 0), result.Metadata.Published);
            Assert.AreEqual(new DateTime(2015, 9, 16, 5, 45, 10), result.Metadata.Modified);
        }

        [Test]
        public void ReadArticle3358()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3358));
            
            Assert.NotNull(result.BodyHtml);
        }

        [Test]
        public void ReadArticle3364()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3364));

            Assert.NotNull(result.BodyHtml);
        }

        [Test]
        public void ReadArticle3366()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3366));
            
            Assert.NotNull(result.BodyHtml);
            Assert.IsTrue(result.BodyHtml.Contains("een simpele telnet-hack"));
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
