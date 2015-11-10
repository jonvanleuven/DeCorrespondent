using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeCorrespondent.Impl
{
    public class KindleEmailSender : IEReaderSender
    {
        private readonly IKindleEmailSenderConfig config;
        private readonly IMailer mailer;
        private readonly ILogger log;

        public KindleEmailSender(ILogger log, IKindleEmailSenderConfig config, IMailer mailer)
        {
            this.log = log;
            this.mailer = mailer;
            this.config = config;
        }

        public void Send(IEnumerable<Func<FileStream>> ebooks)
        {
            if( string.IsNullOrEmpty(config.KindleEmail) )
            {
                log.Info("Er zal niks naar de Kindle verstuurd worden, kindle email adres is leeg");
                return;
            }
            var groupSize = 5; //verstuur 1 email per 5 attachments
            var list = ebooks.ToList();
            var groups = Enumerable.Range(0, (list.Count / groupSize)+1)
                .Select(i => list.Skip(i * groupSize).Take(groupSize))
                .Where(g => g.Any())
                .ToList();
            foreach (var g in groups)
            {
                mailer.Send(config.KindleEmail.Split(','), null, null, g);
            }
        }
    }

    public interface IKindleEmailSenderConfig
    {
        string KindleEmail { get; }
    }
}
