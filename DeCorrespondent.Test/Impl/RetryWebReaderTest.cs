using System.Linq;
using System.Net;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class RetryWebReaderTest
    {
        [Test]
        public void Retry()
        {
            var logger = new LogWrapper(new ConsoleLogger(true));
            var reader = RetryWebReader.Wrap(new TimeoutReader(), logger);

            try
            {
                reader.ReadNewItems(0);
            }
            catch (WebException)
            {
            }

            Assert.AreEqual(1, logger.Infos.Count());
            Assert.AreEqual("Timeout detected, retry....", logger.Infos.First());
        }

        internal class TimeoutReader : IResourceReader
        {
            public string ReadNewItems(int index)
            {
                return Timeout<string>();
            }

            public string ReadArticle(int articleId)
            {
                return Timeout<string>();
            }

            public byte[] ReadBinary(string url)
            {
                return Timeout<byte[]>();
            }

            private T Timeout<T>()
            {
                throw new WebException(@"The operation has timed out.");
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
