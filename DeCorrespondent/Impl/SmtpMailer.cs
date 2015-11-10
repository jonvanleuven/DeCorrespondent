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

        public void Send(IEnumerable<string> to, string subject, string body, IEnumerable<Func<FileStream>> attachments)
        {
            if (to == null || !to.Any())
            {
                log.Info("Er zal geen mail verstuurd worden: email adres van de ontvanger(s) is leeg");
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
            var message = new MailMessage();
            message.From = new MailAddress(config.MailUsername);
            to.ToList().ForEach(t => message.To.Add(new MailAddress(t)));
            message.Subject = subject ?? string.Empty;
            message.Body = body ?? string.Empty;
            message.IsBodyHtml = true;
            var streams = (attachments != null)
                ? attachments.Select(a => a()).ToList()
                : null;
            if( streams != null )
                streams.Select(s => new Attachment(s, Path.GetFileName(s.Name))).ToList().ForEach(s => message.Attachments.Add(s));
            client.Send(message);
            if (streams != null)
                streams.ForEach(s => s.Close());
            log.Info(string.Format("Mail has been send to '{0}' with {1} attachements", string.Join(", ", to), attachments!=null ? attachments.Count() : 0));
        }
    }

    public interface ISmtpMailConfig
    {
        string MailUsername { get; set; }
        string MailPassword { get; set; }
    }
}