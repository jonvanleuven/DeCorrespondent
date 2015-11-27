using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class EmailNotificationSender : INotificationSender
    {
        private readonly IMailer mailer;
        private readonly IEmailNotificationSenderConfig config;
        private readonly IResourceReader resources;
        private readonly ILogger log;

        public EmailNotificationSender(ILogger log, IMailer mailer, IEmailNotificationSenderConfig config, IResourceReader resources)
        {
            this.log = log;
            this.mailer = mailer;
            this.config = config;
            this.resources = resources;
        }
        public void Send(IEnumerable<IArticle> articlesEnumerable)
        {
            if (string.IsNullOrEmpty(config.NotificationEmail))
            {
                log.Info("Er zal geen notificatie email worden verstuurd, email adres van de ontvanger is leeg");
                return;
            }
            var articles = articlesEnumerable.ToList();
            var list = string.Join("\n", articles.Select(a => string.Format(@"<p><b><a href=""{6}"" style=""color:black;"">{2}</a></b> {0} {1}<br/>{5}<br/><i>{3}</i></p>{4}<hr/>", 
                HtmlEntity.Entitize(a.Metadata.AuthorFirstname), 
                HtmlEntity.Entitize(a.Metadata.AuthorLastname),
                HtmlEntity.Entitize(a.Metadata.Title), 
                HtmlEntity.Entitize(a.Metadata.Description), 
                ExternalMediaList(a), 
                ImageHtml(a), 
                a.Metadata.Url)));
            var body = new StringBuilder();
            body.Append(string.Format("<h3>Artikelen:</h3>{0}", list));
            var subject = string.Format("{0} artikel{1} verstuurd naar je Kindle", articles.Count(), articles.Count() > 1 ? "en" : "");
            mailer.Send(config.NotificationEmail.Split(','), subject, body.ToString(), null);
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

        private static string ExternalMediaList(IArticle article)
        {
            if (!article.Metadata.ExternalMedia.Any())
                return string.Empty;
            return string.Format("<ul>{0}</ul>",
                string.Join("", article.Metadata.ExternalMedia.Select(l => string.Format(@"<li>{0}</li>", ExternalMediaHtml(l, article))))
                );
        }

        private static string ExternalMediaHtml(IExternalMedia l, IArticle article)
        {
            var url = l.Url.StartsWith("http://player.vimeo.com")
                ? article.Metadata.Url //rechtstreeks naar Vimeo linken werkt niet: link naar artikel
                : l.Url;
            var description = l.Description;
            if (string.IsNullOrEmpty(l.Description))
            {
                if (l.Url.StartsWith("http://www.youtube.com"))
                    description = "video";
                if (l.Url.StartsWith("http://player.vimeo.com"))
                    description = "video";
                if (l.Url.StartsWith("https://w.soundcloud.com"))
                    description = "audio";
            }
            return string.Format(@"<a href=""{0}"">{1}</a>", url, HtmlEntity.Entitize(description));
        }
    }

    public interface IEmailNotificationSenderConfig
    {
        string NotificationEmail { get; }
    }
}
