using System;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class EmailErrorLoggerTest
    {
        [Test]
        public void LogException()
        {
            var config = FileConfig.Load(@"D:\Applications\DeCorrespondent\config.xml");
            var logger = new EmailErrorLogger(config.NotificationEmail, config.SmtpConfig);

            logger.Error(new Exception("dit is een test"));
        }
    }
}
