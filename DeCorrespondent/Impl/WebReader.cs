using System;
using System.Net;
using HtmlAgilityPack;

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
            return Get("https://decorrespondent.nl/nieuw" + (index != 0 ? "/" + index : ""));
        }

        public string ReadArticle(int articleId)
        {
            return Get("https://decorrespondent.nl/" + articleId);
        }

        private string Get(string url)
        {
            log.Debug("Requesting url '" + url + "'");
            var web = new HtmlWeb();
            web.UseCookies = true;
            web.PreRequest += (req =>
            {
                req.CookieContainer.Add(sessionCookies);
                return true;
            });
            return web.Load(url).DocumentNode.OuterHtml;
        }
    }

    public interface IWebReaderConfig
    {
        string Username { get; }
        string Password { get; }
    }
}
