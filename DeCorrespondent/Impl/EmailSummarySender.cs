using System.Collections.Generic;
using System.Linq;

namespace DeCorrespondent.Impl
{
    public class EmailSummarySender : IArticleSummarySender
    {
        private readonly IMailer mailer;
        private readonly IEmailSummarySenderConfig config;
        public EmailSummarySender(IMailer mailer, IEmailSummarySenderConfig config)
        {
            this.mailer = mailer;
            this.config = config;
        }
        public void Send(IEnumerable<IArticle> articles)
        {
            var body = string.Join("\n", articles.Select(a => string.Format("- {0} {1}: {2}", a.Metadata.AuthorFirstname, a.Metadata.AuthorLastname, a.Metadata.Title)));
            var subject = string.Format("{0} artikelen verstuurd naar je Kindle", articles.Count());
            mailer.Send(config.SummaryEmail, subject, body, null);
        }
    }

    public interface IEmailSummarySenderConfig
    {
        string SummaryEmail { get; }
    }
}
