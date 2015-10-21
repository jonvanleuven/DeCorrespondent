using System.Collections.Generic;
using System.IO;
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

        public void Send(string to, string subject, string body, IEnumerable<FileStream> attachments)
        {
            var client = new SmtpClient
            {
                Host = "smtp.gmail.com", //TODO move to config
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(config.MailUsername, config.MailPassword),
                Timeout = 120000,
            };
            var mm = new MailMessage(config.MailUsername, to, subject, body);
            if (attachments != null)
            {
                foreach (var a in attachments)
                {
                    mm.Attachments.Add(new Attachment(a, Path.GetFileName(a.Name)));
                }    
            }
            client.Send(mm);
            log.Info("Mail has been send to '" + to + "'");
        }
    }

    public interface ISmtpMailConfig
    {
        string MailUsername { get; set; }
        string MailPassword { get; set; }
    }
}