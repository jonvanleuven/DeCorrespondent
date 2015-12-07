using System;
using System.Net;

namespace DeCorrespondent.Impl
{
    public class RetryWebReader : IResourceReader
    {
        private readonly ILogger log;
        private readonly IResourceReader delegateReader;

        public static IResourceReader Wrap(IResourceReader delegateReader, ILogger log)
        {
            return new RetryWebReader(delegateReader, log);
        }

        private RetryWebReader(IResourceReader delegateReader, ILogger log)
        {
            this.delegateReader = delegateReader;
            this.log = log;
        }

        public string Read(string url)
        {
            return ReadOrRetryOnTimeout(r => r.Read(url));
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
                log.Info("Timeout detected, retry....");
                return func(delegateReader);
            }
        }

        public void Dispose()
        {
            delegateReader.Dispose();
        }
    }
}
