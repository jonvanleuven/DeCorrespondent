using System.Collections.Generic;
using System.IO;

namespace DeCorrespondent.Impl
{
    public class KindleEmailSender : IArticleSender
    {
        private readonly IKindleEmailSenderConfig config;
        private readonly IMailer mailer;

        public KindleEmailSender(IKindleEmailSenderConfig config, IMailer mailer)
        {
            this.mailer = mailer;
            this.config = config;
        }

        public void Send(IEnumerable<FileStream> ebooks)
        {
            mailer.Send(config.KindleEmail, "", "", ebooks);
        }
    }

    public interface IKindleEmailSenderConfig
    {
        string KindleEmail { get; }
    }
}
