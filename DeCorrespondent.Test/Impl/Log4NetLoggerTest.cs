using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class Log4NetLoggerTest
    {
        [Test]
        public void LogLine()
        {
            var logger = new Log4NetLogger();
            //BasicConfigurator.Configure();

            logger.Info("test {0}", "bla");
        }
    }
}
