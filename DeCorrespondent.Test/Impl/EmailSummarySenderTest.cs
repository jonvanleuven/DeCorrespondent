using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class EmailSummarySenderTest
    {
        [Test]
        public void SendSummary()
        {
            var sender = CreateSender();
            var reader = new ArticleReader();
            var articles = new IArticle[] { reader.Read(new NewItemsReaderTest.FileResources().ReadArticle(2404)) };

            sender.Send(articles.ToList());


        }

        private static IArticleSummarySender CreateSender()
        {
            return new EmailSummarySender(CreateMailer(), CreateConfig());
        }

        private static IMailer CreateMailer()
        {
            return new SmtpMailer(new ConsoleLogger(true), CreateConfig());
        }

        private static FileConfig CreateConfig()
        {
            return FileConfig.Load(@"D:\Applications\DeCorrespondent\config.xml");
        }
    }
}
