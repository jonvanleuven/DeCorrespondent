using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Net;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class NewItemsReader : IItemsReader
    {
        private readonly ILogger log;
        private readonly IResourceReader resources;

        public NewItemsReader(ILogger log, IResourceReader resources)
        {
            this.log = log;
            this.resources = resources;
        }

        public IEnumerable<IArticleReference> ReadItems(int? lastReadId)
        {
            var result = new List<IArticleReference>();
            for (var i = 0; i < 3; i++) //TODO werk met eigen IEnumerable implementatie ipv magic number
            {
                var requestNextPage = ReadItems(lastReadId, resources.ReadNewItems(i), result);
                if (!requestNextPage) break;
            }
            return result;
        }

        private bool ReadItems(int? lastReadId, string html, List<IArticleReference> result)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var all = doc.DocumentNode.SelectNodes("//a[string-length(@data-article-id) > 0]")
                .Where(n => n != null)
                .Select(n => int.Parse(n.Attributes["data-article-id"].Value))
                .Distinct()
                .Select(id => new ArticleReference(id, resources, log))
                .ToList();
            log.Debug("Aantal items op pagina: " + all.Count);
            var partial = all.TakeWhile(r => r.Id != lastReadId).Reverse().ToList();
            log.Debug("Aantal nieuw: " + result.Count());
            result.AddRange(partial);
            return partial.Count() == all.Count();
        }

    }

    public class ArticleReference : IArticleReference
    {
        private readonly IResourceReader reader;
        private readonly ILogger log;
        private static readonly HtmlNodeCollection EmptyNodes = new HtmlNodeCollection(null);
        public ArticleReference(int id, IResourceReader reader, ILogger log)
        {
            Id = id;
            this.reader = reader;
            this.log = log;
        }
        public int Id { get; private set; }
        public IArticle ReadArticle()
        {
            log.Debug("Reading article: " + Id);
            var doc = new HtmlDocument();
            doc.LoadHtml(reader.ReadArticle(this));
            var body = doc.DocumentNode.SelectSingleNode("//body");
            RemoveNodes(body, "//script");
            RemoveNodes(body, "//noscript");
            RemoveNodes(body, "//div", "share-publication-footer");
            RemoveNodes(body, "//div", "publication-body-link");
            RemoveNodes(body, "//div[@id='notificationBar']");
            RemoveNodes(body, "//aside");
            RemoveNodes(body, "//header", "header");
            RemoveNodes(body, "//svg", "preloader");
            RemoveNodes(body, "//section[@id='comments']");
            RemoveNodes(body, "//article", "campaign-article garden");
            RemoveNodes(body, "//a", "publication-sidenote");
            (body.SelectNodes("//img[string-length(@data-src) > 0]")??EmptyNodes).Where(n => n != null).ToList().ForEach(n =>
            {
                var isMainImage = n.GetAttributeValue("class", "").Contains("mainimage");
                var url = n.Attributes["data-src"].Value;
                url = url.Replace("{breakpoint-name}", !isMainImage ? "904" : "1024"); //320, 600 of 904 (of 660, 1024 of 1920 voor main image)
                n.SetAttributeValue("src", url);
                n.SetAttributeValue("data-src", "");
            });
            (body.SelectNodes("//iframe[string-length(@data-src) > 0]") ?? EmptyNodes).Where(n => n != null).ToList().ForEach(n =>
            {
                var url = n.Attributes["data-src"].Value;
                url = url.StartsWith("//") ? "http:" + url : url;
                n.SetAttributeValue("src", url);
                n.SetAttributeValue("data-src", "");
            });
            (body.SelectNodes("//time[string-length(@title) > 0]") ?? EmptyNodes).ToList().ForEach(n =>
            {
                n.ParentNode.PrependChild(HtmlNode.CreateNode("<span>"  + n.Attributes["title"].Value + "&nbsp;</span>"));
                n.Remove();
            });
            var title = body.SelectSingleNode("//h1[@data-field='title']").InnerText;
            var readingTime = ReadingTime(body);
            var authorSurname = body.SelectSingleNode("//span[@class='author-surname']").InnerText;
            return new Article(this, title, readingTime, authorSurname, body.InnerHtml);
        }

        private static string ReadingTime(HtmlNode body)
        {
            var node = body.SelectSingleNode("//span[@class='reading-time']");
            if (node == null) return "";
            return string.Join("", node.InnerText.Where(l => Char.IsDigit(l) || l == '-'));
        }

        private static void RemoveNodes(HtmlNode body, string xpath)
        {
            (body.SelectNodes(xpath)??EmptyNodes).Where(n => n != null).ToList().ForEach(n => n.Remove());
        }

        private static void RemoveNodes(HtmlNode body, string xpath, string className)
        {
            (body.SelectNodes(xpath + "[string-length(@class) > 0]") ?? EmptyNodes).Where(n => n != null)
                .Where( n => n.Attributes["class"].Value.Trim() == className ||
                             n.Attributes["class"].Value.Contains(className+" ") || 
                             n.Attributes["class"].Value.Contains(" "+className) )
                .ToList().ForEach(n => n.Remove());
        }
    }

    public class Article : IArticle
    {
        internal Article(IArticleReference reference, string title, string readingTime, string authorSurname, string html)
        {
            Reference = reference;
            Html = html;
            Title = title;
            ReadingTime = readingTime;
            AuthorSurname = authorSurname;
        }
        public IArticleReference Reference { get; private set; }
        public string Html { get; private set; }
        public string Title { get; private set; }
        public string ReadingTime { get; private set; }
        public string AuthorSurname { get; private set; }
    }

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
            return Get("https://decorrespondent.nl/nieuw" + (index!=0?"/"+index:""));
        }

        public string ReadArticle(IArticleReference reference)
        {
            return Get("https://decorrespondent.nl/" + reference.Id);
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

    public interface IResourceReader
    {
        string ReadNewItems(int index);
        string ReadArticle(IArticleReference reference);
    }
}
 