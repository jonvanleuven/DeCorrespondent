﻿using System;
using System.Collections.Generic;
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
            var metadataValues = doc.DocumentNode.SelectNodes("//meta")
                .Where(n => n.Attributes.Contains("content") && (n.Attributes.Contains("name") || n.Attributes.Contains("property")))
                .GroupBy(n => n.Attributes.Contains("name") ? n.Attributes["name"].Value : n.Attributes["property"].Value, n => n.Attributes["content"].Value)
                .ToDictionary(x => x.Key, x => x.FirstOrDefault());
            var body = doc.DocumentNode.SelectSingleNode("//body");
            var metadata = new ArticleMetadata(metadataValues);
            //var introNode = body.SelectSingleNode("//p[@class='intro']");
            //metadata.Description = (introNode != null ? introNode.InnerText : string.Empty).UnescapeHtml();
            metadata.Section = body.SelectSingleNode("//div[@class='publication-authors-department']")?.InnerText.Trim().UnescapeHtml().Replace("Correspondent ", "");
            metadata.ReadingTime = ReadingTime(body);
            RemoveNodes(body, "//script");
            RemoveNodes(body, "//noscript");
            RemoveNodes(body, "//div", "share-publication-footer");
            //RemoveNodes(body, "//div", "publication-body-link");
            RemoveNodes(body, "//div[@id='notificationBar']");
            RemoveNodes(body, "//aside");
            RemoveNodes(body, "//header", "header");
            RemoveNodes(body, "//svg", "preloader");
            RemoveNodes(body, "//section[@id='comments']");
            RemoveNodes(body, "//article", "campaign-article garden");
            RemoveNodes(body, "//a", "publication-sidenote");
            RemoveNodes(body, "//header");
            RemoveNodes(body, "//p", "publication-main-image-description");
            (body.SelectNodes("//img[string-length(@data-srcset) > 0]") ??EmptyNodes).Where(n => n != null).ToList().ForEach(n =>
            {
                var isMainImage = n.GetAttributeValue("class", "").Contains("article-mainvisual-image");
                var urlPlain = n.Attributes["data-srcset"].Value;
                var url = isMainImage
                    ? urlPlain.Split(',').Where(i => i.EndsWith("1024w")).Select(x => x.Trim().Split(' ').FirstOrDefault()).FirstOrDefault()
                    : urlPlain.Split(',').Where(i => i.EndsWith("904w")).Select(x => x.Trim().Split(' ').FirstOrDefault()).FirstOrDefault();
                    //320, 600 of 904 (of 660, 1024 of 1920 voor main image)
                n.SetAttributeValue("src", url);
                n.SetAttributeValue("data-srcset", "");
                if (isMainImage)
                {
                    metadata.MainImgUrl = url;
                    metadata.MainImgUrlSmall = urlPlain.Split(',').Where(i => i.EndsWith("660w")).Select(x => x.Trim().Split(' ').FirstOrDefault()).FirstOrDefault(); ;
                    n.Remove();
                }
            });


            (body.SelectNodes("//iframe[string-length(@src) > 0]") ?? EmptyNodes).Where(n => n != null).ToList().ForEach(n =>
            {
                var url = n.Attributes["src"].Value;
                url = url.StartsWith("//") ? "http:" + url : url;
                //n.SetAttributeValue("src", url);
                //n.SetAttributeValue("data-src", "");
                var descriptionNode = n.ParentNode.ParentNode.SelectSingleNode("p[@class='contentitem-caption']");
                metadata.ExternalMedia.Add(new ExternalMedia(url, descriptionNode!= null ? descriptionNode.InnerText : null));
            });
            return new Article(body.InnerHtml, metadata);
        }

        private static IList<int> ReadingTime(HtmlNode body)
        {
            var node = body.SelectSingleNode("//div[@class='article-head-metadata-data mod-readingtime']");
            if (node == null) return new int[0];
            return string.Join("", node.InnerText.Where(l => Char.IsDigit(l) || l == '-')).Split('-')
                .Select(int.Parse)
                .ToList();
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
        internal Article(string bodyHtml, IArticleMetadata metadata)
        {
            BodyHtml = bodyHtml;
            Metadata = metadata;
        }
        public string BodyHtml { get; private set; }
        public IArticleMetadata Metadata { get; private set; }
    }

    public class ArticleMetadata : IArticleMetadata
    {
        private readonly IDictionary<string, string> metadata;
        internal ArticleMetadata(IDictionary<string, string> metadata)
        {
            this.metadata = metadata;
            ExternalMedia = new List<IExternalMedia>();
        }

        public int Id { get { return int.Parse(Url.Split('/').Last()); } }
        public string Title { get { return GetValue("og:title").UnescapeHtml(); } }
        public IList<int> ReadingTime { get; internal set; }

        public string ReadingTimeDisplay
        {
            get
            {
                if (ReadingTime == null || !ReadingTime.Any() || (ReadingTime.Count() == 1 && ReadingTime.First() == 1))
                    return "1 minuut";
                return string.Format("{0} minuten", string.Join("-", ReadingTime));
            }
        }

        public string AuthorFirstname { get { return GetValue("article:author:first_name"); } }
        public string AuthorLastname { get { return GetValue("article:author:last_name"); } }
        public DateTime Published { get { return ParseDate(GetValue("article:published_time")); } }
        public DateTime Modified { get { return ParseDate(GetValue("article:modified_time")?? GetValue("article:published_time")); } }
        public string MainImgUrl { get; internal set; }
        public string MainImgUrlSmall { get; set; }
        public string AuthorImgUrl { get { return GetValue("article:author:image"); } }
        public string Section { get; internal set; }
        public string Description { get { return GetValue("og:description").UnescapeHtml().UnescapeHtml(); } }
        public IList<IExternalMedia> ExternalMedia { get; private set; }

        public string Url
        {
            get
            {
                var url = GetValue("og:url");
                return string.IsNullOrEmpty(url) ? url : string.Join("/", url.Split('/').Take(4));
            }
        }

        private static DateTime ParseDate(string str)
        {
            return DateTime.Parse(str);
        }

        private string GetValue(string key)
        {
            string result = null;
            metadata.TryGetValue(key, out result);
            return result;
        }
    }
    
    public class ExternalMedia : IExternalMedia
    {
        internal ExternalMedia(string url, string description)
        {
            Url = url;
            Description = description;
        }
        public string Url { get; private set; }
        public string Description { get; private set; }

        public ExternalMediaType? Type
        {
            get
            {
                if (Url.StartsWith("http://www.youtube.com"))
                    return ExternalMediaType.YouTube;
                if (Url.StartsWith("http://player.vimeo.com"))
                    return ExternalMediaType.Vimeo;
                if (Url.StartsWith("https://w.soundcloud.com"))
                    return ExternalMediaType.Soundcloud;
                return null;
            }
        }
    }
}
