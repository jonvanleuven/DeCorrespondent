using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IArticleRenderer
    {
        IEnumerable<IArticleEbook> Render(IEnumerable<IArticle> articles);
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
