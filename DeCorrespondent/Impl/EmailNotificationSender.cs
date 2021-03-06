﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class EmailNotificationSender : INotificationSender
    {
        private readonly IMailer mailer;
        private readonly IEmailNotificationSenderConfig config;
        private readonly ILogger log;

        public EmailNotificationSender(ILogger log, IMailer mailer, IEmailNotificationSenderConfig config)
        {
            this.log = log;
            this.mailer = mailer;
            this.config = config;
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
                a.Metadata.AuthorFirstname.EscapeHtml(), 
                a.Metadata.AuthorLastname.EscapeHtml(),
                a.Metadata.Title.EscapeHtml(), 
                a.Metadata.Description.EscapeHtml(), 
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
            var description = l.Description;
            if (string.IsNullOrEmpty(l.Description))
            {
                if (l.Type == ExternalMediaType.YouTube)
                    description = "video";
                if (l.Type == ExternalMediaType.Vimeo)
                    description = "video";
                if (l.Type == ExternalMediaType.Soundcloud)
                    description = "audio";
            }
            var url = l.Type == ExternalMediaType.Vimeo
                ? article.Metadata.Url //rechtstreeks naar Vimeo linken werkt niet: link naar artikel
                : l.Url;
            return string.Format(@"<a href=""{0}"">{1}</a>", url, description.EscapeHtml());
        }
    }

    public interface IEmailNotificationSenderConfig
    {
        string NotificationEmail { get; }
    }
}
