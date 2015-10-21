using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class SmtpMailerTest
    {
        [Test]
        public void SendText()
        {
            var mailer = CreateMailer();

            mailer.Send(CreateConfig().NotificationEmail, "testcase", "Dit is een testmail verstuurd vanuit het DeCorrepondent project vanuit een nunit test", null);
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
