using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class ArticleReader : IArticleReader
    {
        private static readonly HtmlNodeCollection EmptyNodes = new HtmlNodeCollection(null);

        public IArticle Read(string article)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(article);
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
            DateTime? publicationdate = null;
            (body.SelectNodes("//time[string-length(@title) > 0]") ?? EmptyNodes).ToList().ForEach(n =>
            {
                publicationdate = ParseDateTime(n.Attributes["title"].Value);
                n.ParentNode.PrependChild(HtmlNode.CreateNode(string.Format("<span>{0:d-M-yyyy H:mm}&nbsp;</span>", publicationdate)));
                n.Remove();
            });
            var title = RemoveHtmlSpecialCharacters( body.SelectSingleNode("//h1[@data-field='title']").InnerText );
            var readingTime = ReadingTime(body);
            var authorSurname = body.SelectSingleNode("//span[@class='author-surname']").InnerText;
            return new Article(title, readingTime, authorSurname, publicationdate, body.InnerHtml);
        }

        private static DateTime ParseDateTime(string value)
        {
            return DateTime.ParseExact(value, new[] { "dd-MM-yyyy HH:mm", "d-M-yyyy H:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        private static string RemoveHtmlSpecialCharacters(string text)
        {
            return Regex.Replace(text, "&[a-zA-Z0-9]+;", "");
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
        internal Article(string title, string readingTime, string authorSurname, DateTime? publicationdate, string html)
        {
            if (!publicationdate.HasValue)
                throw new Exception("publicationdate cannot be null");
            BodyHtml = html;
            Title = title;
            ReadingTime = readingTime;
            AuthorSurname = authorSurname;
            Publicationdate = publicationdate.Value;
        }
        public IArticleReference Reference { get; private set; }
        public string BodyHtml { get; private set; }
        public string Title { get; private set; }
        public string ReadingTime { get; private set; }
        public string AuthorSurname { get; private set; }
        public DateTime Publicationdate { get; private set; }
    }
}
