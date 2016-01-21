using System;
using System.Collections.Generic;
using System.IO;

namespace DeCorrespondent.Impl
{
    public class EmailErrorLogger : ILogger
    {
        private readonly MemoryLogger allLogLines;
        private readonly string emailAddress;
        private readonly ISmtpMailConfig config;

        public EmailErrorLogger(string emailAddress, ISmtpMailConfig config)
        {
            this.emailAddress = emailAddress;
            this.config = config;
            this.allLogLines = new MemoryLogger();
        }

        public void Info(string message, params object[] args)
        {
            allLogLines.Info(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            allLogLines.Debug(message, args);
        }

        public void Error(Exception error)
        {
            allLogLines.Error(error);
            var logger = new Log4NetLogger();
            try
            {
                File.WriteAllLines("log.txt", allLogLines.Lines);
                var body = string.Format("<pre>Fout: {0}\n\n{1}</pre>", error.Message, error.StackTrace);
                new SmtpMailer(logger, config).Send(emailAddress.Split(','), "DeCorrespondent.exe. Fout opgetreden", body, 
                    new Func<FileStream>[] {() => new FileStream("log.txt", FileMode.Open)});
            }
            catch (Exception e)
            {
                logger.Error(e);
                logger.Error(error);
            }
            finally
            {
                if (File.Exists("log.txt"))
                    File.Delete("log.txt");
            }
        }
    }
}
