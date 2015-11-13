using System;
using System.Collections.Generic;

namespace DeCorrespondent.Test.Util
{
    public class LogWrapper : ILogger
    {
        private readonly ILogger logDelegate;
        internal LogWrapper(ILogger logDelegate)
        {
            this.logDelegate = logDelegate;
            Infos = new List<string>();
            Debugs = new List<string>();
        }

        public List<string> Infos { get; private set; }
        public List<string> Debugs { get; private set; }

        public void Info(string message, params object[] args)
        {
            logDelegate.Info(message, args);
            Infos.Add(string.Format(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            logDelegate.Debug(message, args);
            Debugs.Add(string.Format(message, args));
        }

        public void Error(Exception e)
        {
            throw e;
        }
    }
}
