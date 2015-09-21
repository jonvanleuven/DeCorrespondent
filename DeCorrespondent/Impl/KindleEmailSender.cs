using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void Send(IEnumerable<IArticleEbook> ebooks)
        {
            return;
            //TODO
            var mail = new MailMessage("junknown", "unknown");
            var client = new SmtpClient("smtp.live.com", 25);
            client.Credentials = new System.Net.NetworkCredential("unknown", "unknown");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            mail.Subject = "this is a test email.";
            mail.Body = "this is my test email body";
            foreach (var ebook in ebooks.Take(1))
            {
                mail.Attachments.Add(new Attachment(new MemoryStream(ebook.Content), ebook.Name));
            }
            client.Send(mail);            
        }
    }

    public interface IKindleEmailSenderConfig
    {
        string KindleEmail { get; }
    }
}
