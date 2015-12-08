using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DeCorrespondent.Impl
{
    public class RssFeedResources : IDeCorrespondentResources
    {
        private readonly IList<RssItem> items;
        private readonly IResourceReader resources;

        public static RssFeedResources Instance(ILogger log)
        {
            return new RssFeedResources(new WebReader(log));
        }

        public RssFeedResources(IResourceReader resources)
        {
            this.resources = resources;
            var r = XmlReader.Create(new MemoryStream(new UTF8Encoding().GetBytes(resources.Read("http://molecule.nl/decorrespondent/rss.php"))));
            var xml = XDocument.Load(r);
            items = xml.XPathSelectElements("rss/channel/item").Select(i => new RssItem(i)).ToList();
        }

        public IEnumerable<INieuwItem> ReadNieuwItems()
        {
            return items;
        }

        public string ReadArticle(int id)
        {
            return resources.Read(items.OrderByDescending(i => i.Publicationdate).First(i => i.Id == id).Url);
        }

        public class RssItem : INieuwItem
        {
            public RssItem(XNode xElement)
            {
                Url = xElement.XPathSelectElement("link").Value;
                Id = int.Parse(Url.Split('/').Skip(3).Take(1).First());
                Publicationdate = DateTime.Parse(xElement.XPathSelectElement("pubDate").Value);
            }
            public int Id { get; private set; }
            public string Url { get; private set; }
            public DateTime Publicationdate { get; private set; }
        }

        public void Dispose()
        {
            this.resources.Dispose();
        }
    }
}
