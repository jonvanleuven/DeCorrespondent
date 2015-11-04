using System.Linq;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class EmailNotificationSenderTest
    {
        [Test]
        public void SendNotificationEmail()
        {
            var sender = CreateSender();
            var reader = new ArticleReader();
            var articles = new [] { reader.Read(new NewItemsReaderTest.FileResources().ReadArticle(3530)) };

            sender.Send(articles.ToList());
        }

        private static INotificationSender CreateSender()
        {
            return new EmailNotificationSender(CreateMailer(), CreateConfig(), new NewItemsReaderTest.FileResources());
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
