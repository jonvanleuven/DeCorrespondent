using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IItemsReader
    {
        // orderer from new to old
        IEnumerable<IArticleReference> ReadItems(string nieuwPagina);
    }

    public interface IArticleReference
    {
        int Id { get; }
    }

    public interface IArticle
    {
        string Html { get; }
        string Title { get; }
        string ReadingTime { get; }
        string AuthorSurname { get; }
    }
}
