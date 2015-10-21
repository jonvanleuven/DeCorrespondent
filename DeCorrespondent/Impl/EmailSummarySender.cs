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
        public void Send(IEnumerable<IArticle> articlesEnumerable)
        {
            var articles = articlesEnumerable.ToList();
            var list = string.Join("\n", articles.Select(a => string.Format("<li><b>{0} {1}</b>: {2}</li>", a.Metadata.AuthorFirstname, a.Metadata.AuthorLastname, a.Metadata.Title)));
            var body = string.Format("<p><ul>{0}</ul></p>", list);
            var subject = string.Format("{0} artikel{1} verstuurd naar je Kindle", articles.Count(), articles.Count() > 1 ? "en" : "");
            mailer.Send(config.SummaryEmail, subject, body, null);
        }
    }

    public interface IEmailSummarySenderConfig
    {
        string SummaryEmail { get; }
    }
}
