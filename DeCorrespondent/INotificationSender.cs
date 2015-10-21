using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface INotificationSender
    {
        void Send(IEnumerable<IArticle> articles);
    }

}
