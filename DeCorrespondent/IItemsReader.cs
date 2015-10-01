using System;
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
        string BodyHtml { get; }
        IArticleMetadata Metadata { get; }
    }

    public interface IArticleMetadata
    {
        string Title { get; }
        string ReadingTime { get; }
        string AuthorSurname { get; }
        DateTime Published { get; }
        DateTime Modified { get; }
    }
}
