using System;
using System.IO;
using System.Net;
using System.Text;

namespace DeCorrespondent.Impl
{
    public class WebReader : IResourceReader
    {
        private readonly ILogger log;

        public WebReader(ILogger log)
        {
            this.log = log;
        }

        public string Read(string url)
        {
            log.Debug("Requesting url '" + url + "'");
            var req = WebRequest.Create(url);
            var response = req.GetResponse();
            var stream = response.GetResponseStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return new UTF8Encoding().GetString(memoryStream.ToArray());
            }
        }

        public byte[] ReadBinary(string url)
        {
            log.Debug("Requesting url '" + url + "'");
            var req = WebRequest.Create(url);
            var response = req.GetResponse();
            var stream = response.GetResponseStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public void Dispose()
        {
        }
    }
}
