using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeCorrespondent.Impl
{
    public class KindleEmailSender : IEReaderSender
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
            var groupSize = 5; //verstuur 1 email per 5 attachments
            var list = ebooks.ToList();
            Enumerable.Range(0, list.Count / groupSize)
                .Select(i => list.Skip(i * groupSize).Take(groupSize))
                .ToList()
                .ForEach(g => mailer.Send(config.KindleEmail, "", "", g));
        }
    }

    public interface IKindleEmailSenderConfig
    {
        string KindleEmail { get; }
    }
}
