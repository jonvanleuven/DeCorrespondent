using System;
using System.Collections.Generic;

namespace DeCorrespondent.Impl
{
    public class EmailSummarySender : IArticleSummarySender
    {
        private readonly ILogger log;
        private readonly IEmailSummarySenderConfig config;
        public EmailSummarySender(ILogger log, IEmailSummarySenderConfig config)
        {
            this.log = log;
            this.config = config;
        }
        public void Send(IEnumerable<IArticle> articles)
        {
            log.Debug("Versturen van samenvatting is nog niet geimplementeerd");
        }
    }

    public interface IEmailSummarySenderConfig
    {
        string SummaryEmail { get; }
    }
}
