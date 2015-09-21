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
            Console.WriteLine(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            if (!debugEnabled) return;
            Console.WriteLine(message, args);
        }
    }
}
