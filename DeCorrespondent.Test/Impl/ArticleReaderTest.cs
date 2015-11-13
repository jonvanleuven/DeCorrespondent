using System;
using System.IO;
using System.Linq;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class ArticleReaderTest
    {
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
            //Assert.IsFalse(result.BodyHtml.Contains("publication-body-link"));
            Assert.IsFalse(result.BodyHtml.Contains("class=\"header"));
            Assert.IsFalse(result.BodyHtml.Contains("8 uur geleden"));
            //Assert.IsTrue(result.BodyHtml.Contains("16-9-2015 5:45"));
            Assert.AreEqual(3342, result.Metadata.Id);
            Assert.AreEqual("https://decorrespondent.nl/3342", result.Metadata.Url);
            Assert.AreEqual("Drie manieren waarop mobiele telefoons bijdragen aan betere data", result.Metadata.Title);
            Assert.AreEqual(new[] { 6, 7 }, result.Metadata.ReadingTime.ToArray());
            Assert.AreEqual("Sanne", result.Metadata.AuthorFirstname);
            Assert.AreEqual("Blauw", result.Metadata.AuthorLastname);
            Assert.AreEqual("https://dynamic.decorrespondent.nl/ff-1442375110/media/1024/55f8734d4e0455501740378.jpg", result.Metadata.MainImgUrl);
            Assert.AreEqual("https://dynamic.decorrespondent.nl/ff-1442375110/media/660/55f8734d4e0455501740378.jpg", result.Metadata.MainImgUrlSmall);
            Assert.AreEqual("https://dynamic.decorrespondent.nl/ff-1441719053/media/190/55eee30dc2bdd5475014700.png", result.Metadata.AuthorImgUrl);
            Assert.AreEqual("Ontcijferen", result.Metadata.Section);
            Assert.AreEqual("Data uit ontwikkelingslanden zijn vaak van slechte kwaliteit. Ik ging op zoek naar nieuwe technologie&euml;n om betere data te verzamelen. In deel twee van de serie: drie manieren waarop mobiele telefoons een verschil kunnen maken.", result.Metadata.Description);
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
        public void ReadArticle2404()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(2404));

            Assert.NotNull(result.BodyHtml);
            Assert.AreEqual(1, result.Metadata.ExternalMedia.Count());
            Assert.AreEqual("https://w.soundcloud.com/player/?url=https://api.soundcloud.com/tracks/188622180?secret_token=s-OBgTL&amp;color=df5b57&amp;auto_play=false&amp;show_artwork=true", result.Metadata.ExternalMedia.First().Url);
            Assert.AreEqual("Interview", result.Metadata.ExternalMedia.First().Description);
        }

        [Test]
        public void ReadArticle3530()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3530));

            Assert.NotNull(result.BodyHtml);
            Assert.AreEqual(6, result.Metadata.ExternalMedia.Count());
            Assert.AreEqual("Klimaatactivist Marjan Minnesma schetst de toekomst van een duurzaam Nederland", result.Metadata.ExternalMedia.First().Description);
        }

        [Test]
        public void ReadArticle3450()
        {
            var reader = new ArticleReader();

            var result = reader.Read(new FileResources().ReadArticle(3450));

            Assert.AreEqual("Alle wereldleiders waren afgelopen week bij de Algemene Vergadering van de Verenigde Naties. Hoe gaat zo&rsquo;n wereldtop eraan toe? Een verslag uit New York, aan de hand van tien voorwerpen.", result.Metadata.Description);
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
    }
}
