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
        IList<int> ReadingTime { get; }
        string AuthorFirstname { get; }
        string AuthorLastname { get; }
        DateTime Published { get; }
        DateTime Modified { get; }
        string AuthorImgUrl { get; }
        string MainImgUrl { get; }
        string MainImgUrlSmall { get; }
        string Section { get; }
        string Description { get; }
        IList<IExternalMedia> ExternalMedia { get; }
    }

    public interface IExternalMedia
    {
        string Url { get; }
        string Description { get; }
    }
}
