using System.Linq;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
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

            mailer.Send(CreateConfig().NotificationEmail.Split(','), "testcase", "Dit is een testmail verstuurd vanuit het DeCorrepondent project vanuit een nunit test", null);
        }

        [Test]
        public void SendNoMailOnEmptyTo()
        {
            var logger = new LogWrapper(new ConsoleLogger(true));
            var mailer = CreateMailer(logger);

            mailer.Send("".Split(','), "irrevant", "", null);

            Assert.AreEqual(1, logger.Infos.Count);
            Assert.AreEqual("Er zal geen mail verstuurd worden: email adres van de ontvanger(s) is leeg", logger.Infos.First());
        }

        private static IMailer CreateMailer(ILogger logger = null)
        {
            return new SmtpMailer(logger??new ConsoleLogger(true), CreateConfig().SmtpMailConfig);
        }

        private static FileConfig CreateConfig()
        {
            return FileConfig.Load(@"D:\Applications\DeCorrespondent\config.xml");
        }
    }
}
