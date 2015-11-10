using System;

namespace DeCorrespondent.Impl
{
    public class EmailErrorLogger : ILogger
    {
        private readonly string emailAddress;
        private readonly ISmtpMailConfig config;

        public EmailErrorLogger(string emailAddress, ISmtpMailConfig config)
        {
            this.emailAddress = emailAddress;
            this.config = config;
        }

        public void Info(string message, params object[] args)
        {
        }

        public void Debug(string message, params object[] args)
        {
        }

        public void Error(Exception error)
        {
            var logger = new Log4NetLogger();
            try
            {
                var body = string.Format("<pre>Fout: {0}\n\n{1}</pre>", error.Message, error.StackTrace);
                new SmtpMailer(logger, config).Send(emailAddress.Split(','), "DeCorrespondent.exe. Fout opgetreden", body, null);
            }
            catch (Exception e)
            {
                logger.Error(e);
                logger.Error(error);
            }
        }
    }
}
