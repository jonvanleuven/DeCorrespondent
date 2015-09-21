using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IItemsReader
    {
        IEnumerable<IArticleReference> ReadItems(int? lastReadId);
    }

    public interface IArticleReference
    {
        int Id { get; }
        IArticle ReadArticle();
    }

    public interface IArticle
    {
        IArticleReference Reference { get; }
        string Html { get; }
        string Title { get; }
        string ReadingTime { get; }
        string AuthorSurname { get; }
    }
}
