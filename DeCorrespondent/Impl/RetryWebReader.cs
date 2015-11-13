using System;
using System.Net;
using System.Threading;

namespace DeCorrespondent.Impl
{
    public class RetryWebReader : IResourceReader
    {
        private readonly ILogger log;
        private readonly IResourceReader delegateReader;
        private readonly int retryDelayInSeconds;

        public static IResourceReader Wrap(IResourceReader delegateReader, ILogger log, int? retryDelayInSeconds = null)
        {
            return new RetryWebReader(delegateReader, log, retryDelayInSeconds??60);
        }

        private RetryWebReader(IResourceReader delegateReader, ILogger log, int retryDelayInSeconds)
        {
            this.delegateReader = delegateReader;
            this.log = log;
            this.retryDelayInSeconds = retryDelayInSeconds;
        }

        public string ReadNewItems(int index)
        {
            return ReadOrRetryOnTimeout(r => r.ReadNewItems(index));
        }

        public string ReadArticle(int articleId)
        {
            return ReadOrRetryOnTimeout(r => r.ReadArticle(articleId));
        }

        public byte[] ReadBinary(string url)
        {
            return ReadOrRetryOnTimeout(r => r.ReadBinary(url));
        }

        private T ReadOrRetryOnTimeout<T>(Func<IResourceReader, T> func)
        {
            try
            {
                return func(delegateReader);
            }
            catch (WebException e)
            {
                if (e.Message != @"The operation has timed out.") 
                    throw;
                log.Info("Timeout detected, retry in {0} seconds....", retryDelayInSeconds);
                Thread.Sleep(1000 * retryDelayInSeconds);
                return func(delegateReader);
            }
        }

        public void Dispose()
        {
            delegateReader.Dispose();
        }
    }
}
