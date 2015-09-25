using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class NewItemsReader : IItemsReader
    {
        private readonly ILogger log;
        public NewItemsReader(ILogger log)
        {
            this.log = log;
        }

        public IEnumerable<IArticleReference> ReadItems(string nieuwPagina)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(nieuwPagina);
            return doc.DocumentNode.SelectNodes("//a[string-length(@data-article-id) > 0]")
                .Where(n => n != null)
                .Select(n => int.Parse(n.Attributes["data-article-id"].Value))
                .Distinct()
                .Select(id => new ArticleReference(id))
                .ToList();
        }

    }

    public class ArticleReference : IArticleReference
    {
        public ArticleReference(int id)
        {
            Id = id;
        }
        public int Id { get; private set; }   
    }
}
 