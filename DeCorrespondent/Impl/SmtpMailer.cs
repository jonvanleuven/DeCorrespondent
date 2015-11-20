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
            var toList = to != null ? to.Where(i => !string.IsNullOrEmpty(i)).ToList() : null;
            if (toList == null || !toList.Any())
            {
                log.Info("Er zal geen mail verstuurd worden: email adres van de ontvanger(s) is leeg");
                return;
            }

            var client = new SmtpClient
            {
                Host = config.Server,
                Port = config.Port,
                EnableSsl = config.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(config.Username, config.Password),
                Timeout = 120000,
            };
            var message = new MailMessage();
            message.From = new MailAddress(config.Username);
            toList.ForEach(i => message.To.Add(new MailAddress(i)));
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
            log.Info(string.Format("Mail has been send to '{0}' with {1} attachements", string.Join(", ", toList), attachments!=null ? attachments.Count() : 0));
        }
    }

    public interface ISmtpMailConfig
    {
        string Username { get; }
        string Password { get; }
        string Server { get; }
        int Port { get; }
        bool EnableSsl { get; }
    }
}