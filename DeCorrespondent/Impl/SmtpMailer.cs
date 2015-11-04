using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace DeCorrespondent.Impl
{
    public class SmtpMailer : IMailer
    {
        private readonly ILogger log;
        private readonly ISmtpMailConfig config;

        public SmtpMailer(ILogger log, ISmtpMailConfig config)
        {
            this.log = log;
            this.config = config;
        }

        public void Send(string to, string subject, string body, IEnumerable<Func<FileStream>> attachments)
        {
            if (string.IsNullOrEmpty(to))
            {
                log.Info("Er zal geen mail verstuurd worden: email adres van de ontvanger is leeg");
                return;
            }

            var client = new SmtpClient
            {
                Host = "smtp.gmail.com", //TODO move to config
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(config.MailUsername, config.MailPassword),
                Timeout = 120000,
            };
            var mm = new MailMessage(config.MailUsername, to, subject ?? string.Empty, body ?? string.Empty);
            mm.IsBodyHtml = true;
            var streams = (attachments != null)
                ? attachments.Select(a => a()).ToList()
                : null;
            if( streams != null )
                streams.Select(s => new Attachment(s, Path.GetFileName(s.Name))).ToList().ForEach(s => mm.Attachments.Add(s));
            client.Send(mm);
            if (streams != null)
                streams.ForEach(s => s.Close());
            log.Info(string.Format("Mail has been send to '{0}' with {1} attachements", to, attachments!=null ? attachments.Count() : 0));
        }
    }

    public interface ISmtpMailConfig
    {
        string MailUsername { get; set; }
        string MailPassword { get; set; }
    }
}