using System;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class WebReader : IResourceReader
    {
        private readonly CookieCollection sessionCookies;
        private readonly ILogger log;

        public static WebReader Login(ILogger log, string username, string password)
        {
            var web = new HtmlWeb();
            web.UseCookies = true;
            CookieCollection cookies = null;
            web.PostResponse += ((req, resp) => cookies = resp.Cookies);
            var doc = web.Load(string.Format("https://decorrespondent.nl/login?email={0}&password={1}", username, password), "POST");
            if (!doc.DocumentNode.OuterHtml.Contains("Je bent nu ingelogd"))
                throw new Exception("Unable to login, check credentials (username=" + username + ")");
            log.Debug("Logged in with username '" + username + "'");
            return new WebReader(cookies, log);
        }

        public WebReader(ILogger log) : this(null, log)
        { }

        private WebReader(CookieCollection sessionCookies, ILogger log)
        {
            this.sessionCookies = sessionCookies;
            this.log = log;
        }

        public string Read(string url)
        {
            return Request(url);
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

        private string Request(string url, string method = null)
        {
            log.Debug("Requesting url '" + url + "'");
            var web = new HtmlWeb();
            if (sessionCookies != null)
            {
                web.UseCookies = true;
                web.PreRequest += (req =>
                {
                    req.CookieContainer.Add(sessionCookies);
                    return true;
                });
            }
            return web.Load(url, method??"GET").DocumentNode.OuterHtml;
        }

        public void Dispose()
        {
            if (sessionCookies == null)
                return;
            var doc = Request("https://decorrespondent.nl/logout", "POST");
            if (doc.Contains("Je bent nu uitgelogd") )
                log.Info("Logged out");
            else 
                throw new Exception("Uitloggen is mislukt");
        }
    }
}
