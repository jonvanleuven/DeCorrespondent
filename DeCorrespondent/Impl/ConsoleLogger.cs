using System;

namespace DeCorrespondent.Impl
{
    public class ConsoleLogger : ILogger
    {
        private readonly bool debugEnabled;
        public ConsoleLogger(bool debugEnabled)
        {
            this.debugEnabled = debugEnabled;
        }

        public void Info(string message, params object[] args)
        {
            Console.WriteLine(string.Format( "{0:dd-MM-yyyy HH:mm:ss} {1}", DateTime.Now, message), args);
        }

        public void Debug(string message, params object[] args)
        {
            if (!debugEnabled) return;
            Console.WriteLine(string.Format("{0:dd-MM-yyyy HH:mm:ss} {1}", DateTime.Now, message), args);
        }
    }
}
