using System;
using System.Linq;
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
            (body.SelectNodes("//time[string-length(@title) > 0]") ?? EmptyNodes).ToList().ForEach(n =>
            {
                n.ParentNode.PrependChild(HtmlNode.CreateNode("<span>"  + n.Attributes["title"].Value + "&nbsp;</span>"));
                n.Remove();
            });
            var title = body.SelectSingleNode("//h1[@data-field='title']").InnerText;
            var readingTime = ReadingTime(body);
            var authorSurname = body.SelectSingleNode("//span[@class='author-surname']").InnerText;
            return new Article(title, readingTime, authorSurname, body.InnerHtml);
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
        internal Article(string title, string readingTime, string authorSurname, string html)
        {
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
}
