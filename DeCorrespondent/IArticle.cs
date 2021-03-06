﻿using System;
using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IArticle
    {
        string BodyHtml { get; }
        IArticleMetadata Metadata { get; }
    }

    public interface IArticleReference
    {
        int Id { get; }
    }

    public interface IArticleMetadata
    {
        int Id { get; }
        string Url { get; }
        string Title { get; }
        IList<int> ReadingTime { get; }
        string ReadingTimeDisplay { get; }
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
        ExternalMediaType? Type { get; }
    }

    public enum ExternalMediaType
    {
        YouTube,
        Vimeo,
        Soundcloud
    }
}
