using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace DeCorrespondent.Impl
{
    public class Log4NetLogger : ILogger
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Log4NetLogger()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4netconfig.xml"));
        }

        public void Info(string message, params object[] args)
        {
            if (!Log.IsInfoEnabled)
                return;
            Log.InfoFormat(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            if (!Log.IsDebugEnabled)
                return;
            Log.DebugFormat(message, args);
        }
    }
}
