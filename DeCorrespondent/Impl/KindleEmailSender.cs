using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace DeCorrespondent.Impl
{
    public class KindleEmailSender : IArticleSender
    {
        private readonly ILogger log;
        private readonly IKindleEmailSenderConfig config;

        public KindleEmailSender(ILogger log, IKindleEmailSenderConfig config)
        {
            this.log = log;
            this.config = config;
        }

        public void Send(IEnumerable<FileStream> ebooks)
        {
            var client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(config.MailUsername, config.MailPassword),
                Timeout = 120000,
            };
            var mm = new MailMessage(config.MailUsername, config.KindleEmail, "", "");
            foreach (var ebook in ebooks)
            {
                mm.Attachments.Add(new Attachment(ebook, Path.GetFileName(ebook.Name)));
            }
            client.Send(mm); 
            log.Info("Mail has been send to '" + config.KindleEmail + "'");
        }
    }

    public interface IKindleEmailSenderConfig
    {
        string KindleEmail { get; }
        string MailUsername { get; set; }
        string MailPassword { get; set; }
    }
}
