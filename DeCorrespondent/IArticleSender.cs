using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IArticleSender
    {
        void Send(IEnumerable<IArticleEbook> ebooks);
    }
}
