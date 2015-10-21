using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IArticleSummarySender
    {
        void Send(IEnumerable<IArticle> articles);
    }

}
