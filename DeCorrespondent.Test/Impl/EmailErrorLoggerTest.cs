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
            var config = FileConfig.Load(@"..\..\config-test.xml");
            var logger = new EmailErrorLogger(config.NotificationEmail, config.SmtpMailConfig);
            logger.Debug("een debug regel");
            logger.Info("een info regel met '{0}'", "een argument");

            logger.Error(new Exception("dit is een test"));
        }
    }
}

