using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IArticleRenderer
    {
        IArticleEbook Render(IArticle article);
    }

    public interface IArticleEbook
    {
        string Name { get; }
        byte[] Content { get; }
        EbookType Type { get; }
    }

    public enum EbookType
    {
        Pdf,
//        Mobi,
//        Epub
    }
}
