using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class DeCorrespondentReader : IDeCorrespondentReader
    {
        private readonly ILogger log;
        private readonly IResourceReader resources;

        public static DeCorrespondentReader Login(IDeCorrespondentReaderConfig config, ILogger log)
        {
            return new DeCorrespondentReader(RetryWebReader.Wrap(WebReader.Login(log, config.Username, config.Password), log), log);
        }

        public DeCorrespondentReader(IResourceReader resources, ILogger log)
        {
            this.log = log;
            this.resources = resources;
        }

        public IEnumerable<INieuwItem> ReadNieuwItems()
        {
            return Enumerable.Range(0, int.MaxValue)
                .SelectMany(index => ReadItems(resources.Read("https://decorrespondent.nl/nieuw" + (index != 0 ? "/" + index : ""))));
        }

        public void Dispose()
        {
            this.resources.Dispose();
        }

        private static IEnumerable<INieuwItem> ReadItems(string nieuwPagina)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(nieuwPagina);

            /*
            var t = doc.DocumentNode.SelectNodes("//time")
                .Where(n => n != null)
                .Select(n => n.ParentNode.ParentNode.SelectSingleNode("ul/li/ul[string-length(@data-id) > 0]").Attributes["data-id"].Value)
                .ToList();
            t.ForEach(Console.WriteLine);*/

            var ids = doc.DocumentNode.SelectNodes("//a[string-length(@data-article-id) > 0]")
                .Where(n => n != null)
                .Select(n => int.Parse(n.Attributes["data-article-id"].Value))
                .Distinct()
                .ToList();
            var publicationdates =
                doc.DocumentNode.SelectNodes("//time")
                    .Where(n => n != null).ToList()
                    .Select(n => DateTime.Parse(n.Attributes["title"].Value))
                    .ToList();
            if( ids.Count != publicationdates.Count )
                throw new Exception("Kan html niet parsen met nieuwe items, ids komen niet overeen met publicationdates");
            return ids.Select((id, index) => new NieuwItem(id, publicationdates[index]));
        }

        public string ReadArticle(int articleId)
        {
            return resources.Read("https://decorrespondent.nl/" + articleId);
        }

        public class NieuwItem : INieuwItem
        {
            public NieuwItem(int id, DateTime publicationdate)
            {
                Id = id;
                Publicationdate = publicationdate;
            }

            public int Id { get; private set; }
            public DateTime Publicationdate { get; private set; }
        }
    }

    public interface IDeCorrespondentReaderConfig
    {
        string Username { get; }
        string Password { get; }
    }
}