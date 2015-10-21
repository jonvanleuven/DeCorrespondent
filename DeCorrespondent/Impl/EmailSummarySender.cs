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
            var list = string.Join("\n", articles.Select(a => string.Format("<p><b>{0} {1}</b>: {2}</p>", a.Metadata.AuthorFirstname, a.Metadata.AuthorLastname, a.Metadata.Title)));
            var externalMediaList = string.Join("\n", articles.SelectMany(a => a.Metadata.ExternalMedia.Select(url => new { a.Metadata.Title, Url = url })).Select(l => string.Format(@"<p><a href=""{0}"">{1}</a></p>", l.Url, l.Title)));
            var body = string.Format("<h3>Artikelen:</h3>{0}<h3>Video/audio:</h3>{1}", list, externalMediaList);
            var subject = string.Format("{0} artikel{1} verstuurd naar je Kindle", articles.Count(), articles.Count() > 1 ? "en" : "");
            mailer.Send(config.SummaryEmail, subject, body, null);
        }
    }

    public interface IEmailSummarySenderConfig
    {
        string SummaryEmail { get; }
    }
}
