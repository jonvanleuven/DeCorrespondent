using System.Linq;
using System.Net;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class RetryWebReaderTest
    {
        [Test]
        public void Retry()
        {
            var logger = new ProgramTest.LogWrapper(new ConsoleLogger(true));
            var reader = RetryWebReader.Wrap(new TimeoutReader(), logger, 2);

            reader.ReadNewItems(0);

            Assert.AreEqual(1, logger.Infos.Count());
            Assert.AreEqual("Timeout detected, retry in 2 seconds....", logger.Infos.First());
        }

        internal class TimeoutReader : IResourceReader
        {
            private bool timeoutDone = false;
            private readonly ConsoleLogger logger = new ConsoleLogger(true);

            public string ReadNewItems(int index)
            {
                return TimeoutOnce<string>();
            }

            public string ReadArticle(int articleId)
            {
                return TimeoutOnce<string>();
            }

            public byte[] ReadBinary(string url)
            {
                return TimeoutOnce<byte[]>();
            }

            private T TimeoutOnce<T>()
            {
                if (timeoutDone)
                {
                    logger.Debug("Read without timeout");
                    return default(T);
                }
                timeoutDone = true;
                throw new WebException(@"The operation has timed out.");
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
