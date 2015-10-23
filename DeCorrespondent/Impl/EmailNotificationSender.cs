using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeCorrespondent.Impl
{
    public class EmailNotificationSender : INotificationSender
    {
        private readonly IMailer mailer;
        private readonly IEmailNotificationSenderConfig config;
        public EmailNotificationSender(IMailer mailer, IEmailNotificationSenderConfig config)
        {
            this.mailer = mailer;
            this.config = config;
        }
        public void Send(IEnumerable<IArticle> articlesEnumerable)
        {
            var articles = articlesEnumerable.ToList();
            var list = string.Join("\n", articles.Select(a => string.Format("<p><b>{2}</b> {0} {1}<br/><i>{3}</i></p><hr/>", a.Metadata.AuthorFirstname, a.Metadata.AuthorLastname, a.Metadata.Title, a.Metadata.Description)));
            var externalMediaList = string.Join("\n", articles.SelectMany(a => a.Metadata.ExternalMedia.Select(url => new { a.Metadata.Title, Url = url })).Select(l => string.Format(@"<p><a href=""{0}"">{1}</a></p>", l.Url, l.Title)));
            var body = new StringBuilder();
            body.Append(string.Format("<h3>Artikelen:</h3>{0}", list));
            if (externalMediaList.Any())
                body.Append(string.Format("<h3>Video/audio:</h3>{0}", externalMediaList));
            var subject = string.Format("{0} artikel{1} verstuurd naar je Kindle", articles.Count(), articles.Count() > 1 ? "en" : "");
            mailer.Send(config.NotificationEmail, subject, body.ToString(), null);
        }
    }

    public interface IEmailNotificationSenderConfig
    {
        string NotificationEmail { get; }
    }
}
