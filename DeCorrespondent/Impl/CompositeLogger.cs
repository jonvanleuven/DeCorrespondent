using System;
using System.Collections.Generic;
using System.Linq;

namespace DeCorrespondent.Impl
{
    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> loggers;
        public CompositeLogger(params ILogger[] loggers)
        {
            this.loggers = loggers.ToList();
        }

        public void Info(string message, params object[] args)
        {
            loggers.ForEach(l => l.Info(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            loggers.ForEach(l => l.Debug(message, args));
        }

        public void Error(Exception e)
        {
            loggers.ForEach(l => l.Error(e));
        }
    }
}
