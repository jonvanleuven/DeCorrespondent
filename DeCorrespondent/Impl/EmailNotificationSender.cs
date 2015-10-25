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
            var list = string.Join("\n", articles.Select(a => string.Format("<p><b>{2}</b> {0} {1}<br/><i>{3}</i></p>{4}<hr/>", a.Metadata.AuthorFirstname, a.Metadata.AuthorLastname, a.Metadata.Title, a.Metadata.Description, ExternalMediaList(a.Metadata.ExternalMedia))));
            var body = new StringBuilder();
            body.Append(string.Format("<h3>Artikelen:</h3>{0}", list));
            var subject = string.Format("{0} artikel{1} verstuurd naar je Kindle", articles.Count(), articles.Count() > 1 ? "en" : "");
            mailer.Send(config.NotificationEmail, subject, body.ToString(), null);
        }

        private static string ExternalMediaList(IList<IExternalMedia> externalMedia)
        {
            if (!externalMedia.Any())
                return string.Empty;
            return string.Format("<ul>{0}</ul>", 
                string.Join("", externalMedia.Select(l => string.Format(@"<li><a href=""{0}"">{1}</a></li>", l.Url, l.Description??"audio/video")))
                );
        }
    }

    public interface IEmailNotificationSenderConfig
    {
        string NotificationEmail { get; }
    }
}
