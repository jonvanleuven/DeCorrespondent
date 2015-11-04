using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeCorrespondent.Impl
{
    public class EmailNotificationSender : INotificationSender
    {
        private readonly IMailer mailer;
        private readonly IEmailNotificationSenderConfig config;
        private readonly IResourceReader resources;

        public EmailNotificationSender(IMailer mailer, IEmailNotificationSenderConfig config, IResourceReader resources)
        {
            this.mailer = mailer;
            this.config = config;
            this.resources = resources;
        }
        public void Send(IEnumerable<IArticle> articlesEnumerable)
        {
            var articles = articlesEnumerable.ToList();
            var list = string.Join("\n", articles.Select(a => string.Format("<p><b>{2}</b> {0} {1}<br/>{5}<br/><i>{3}</i></p>{4}<hr/>", a.Metadata.AuthorFirstname, a.Metadata.AuthorLastname, a.Metadata.Title, a.Metadata.Description, ExternalMediaList(a.Metadata.ExternalMedia), ImageHtml(a))));
            var body = new StringBuilder();
            body.Append(string.Format("<h3>Artikelen:</h3>{0}", list));
            var subject = string.Format("{0} artikel{1} verstuurd naar je Kindle", articles.Count(), articles.Count() > 1 ? "en" : "");
            mailer.Send(config.NotificationEmail, subject, body.ToString(), null);
        }

        private string ImageHtml(IArticle article)
        {
            if (string.IsNullOrEmpty(article.Metadata.MainImgUrlSmall))
                return null;
//            var image = resources.ReadBinary(article.Metadata.MainImgUrlSmall);
//            if (image == null)
//                return null;
            return string.Format(@"<img src=""{0}"">", article.Metadata.MainImgUrlSmall);
            //return string.Format(@"<img src=""data:image/jpg;base64,{0}"">", Convert.ToBase64String(image));
        }

        private static string ExternalMediaList(IList<IExternalMedia> externalMedia)
        {
            if (!externalMedia.Any())
                return string.Empty;
            return string.Format("<ul>{0}</ul>",
                string.Join("", externalMedia.Select(l => string.Format(@"<li><a href=""{0}"">{1}</a></li>", l.Url, AnchorText(l))))
                );
        }

        private static string AnchorText(IExternalMedia l)
        {
            if (string.IsNullOrEmpty(l.Description))
            {
                if (l.Url.StartsWith("http://www.youtube.com"))
                    return "video";
                if (l.Url.StartsWith("http://player.vimeo.com"))
                    return "video";
                if (l.Url.StartsWith("https://w.soundcloud.com"))
                    return "audio";
                return "audio/video";
            } 
            return l.Description;
        }
    }

    public interface IEmailNotificationSenderConfig
    {
        string NotificationEmail { get; }
    }
}
