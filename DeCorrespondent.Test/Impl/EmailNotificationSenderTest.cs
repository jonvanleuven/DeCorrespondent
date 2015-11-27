using System.Linq;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
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
            var articles = new [] { reader.Read(new FileResources().ReadArticle(3530)) };

            sender.Send(articles.ToList());
        }

        private static INotificationSender CreateSender()
        {
            return new EmailNotificationSender(new ConsoleLogger(true), CreateMailer(), CreateConfig().EmailNotificationSenderConfig, new FileResources());
        }

        private static IMailer CreateMailer()
        {
            return new SmtpMailer(new ConsoleLogger(true), CreateConfig().SmtpMailConfig);
        }

        private static FileConfig CreateConfig()
        {
            return FileConfig.Load(@"..\..\config-test.xml");
        }
    }
}
