using System;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DeCorrespondent.Impl
{

    public class WebReader : IResourceReader
    {
        private readonly CookieCollection sessionCookies;
        private readonly ILogger log;

        public static WebReader Login(ILogger log, IWebReaderConfig config)
        {
            var web = new HtmlWeb();
            web.UseCookies = true;
            CookieCollection cookies = null;
//            web.PreRequest += r =>
//            {
//                var data = Encoding.UTF8.GetBytes(string.Format("email={0}&password={1}", config.Username, config.Password));
//                r.GetRequestStream().Write(data, 0, data.Length);
//                return true;
//            };
            web.PostResponse += ((req, resp) => cookies = resp.Cookies);
            var doc = web.Load(string.Format("https://decorrespondent.nl/login?email={0}&password={1}", config.Username, config.Password), "POST");
            if (!doc.DocumentNode.OuterHtml.Contains("Je bent nu ingelogd"))
                throw new Exception("Unable to login, check credentials (username=" + config.Username + ")");
            log.Debug("Logged in with username '" + config.Username + "'");
            return new WebReader(cookies, log);
        }

        private WebReader(CookieCollection sessionCookies, ILogger log)
        {
            this.sessionCookies = sessionCookies;
            this.log = log;
        }

        public string ReadNewItems(int index)
        {
            return Request("https://decorrespondent.nl/nieuw" + (index != 0 ? "/" + index : ""));
        }

        public string ReadArticle(int articleId)
        {
            return Request("https://decorrespondent.nl/" + articleId);
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
            web.UseCookies = true;
            web.PreRequest += (req =>
            {
                req.CookieContainer.Add(sessionCookies);
                return true;
            });
            return web.Load(url, method??"GET").DocumentNode.OuterHtml;
        }

        public void Dispose()
        {
            var doc = Request("https://decorrespondent.nl/logout", "POST");
            if (doc.Contains("Je bent nu uitgelogd") )
                log.Info("Logged out");
            else 
                throw new Exception("Uitloggen is mislukt");
        }
    }

    public interface IWebReaderConfig
    {
        string Username { get; }
        string Password { get; }
    }
}
