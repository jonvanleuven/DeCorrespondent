using System;
using System.Collections.Generic;

namespace DeCorrespondent.Impl
{
    public class MemoryLogger : ILogger
    {
        public MemoryLogger()
        {
            Lines = new List<string>();
        }
        public void Info(string message, params object[] args)
        {
            Lines.Add(string.Format("{0}  INFO - {1}", Timestamp(), string.Format(message, args)));
        }

        public void Debug(string message, params object[] args)
        {
            Lines.Add(string.Format("{0} DEBUG - {1}", Timestamp(), string.Format(message, args)));
        }

        public void Error(Exception e)
        {
            Lines.Add(string.Format("{0} ERROR - {1}", Timestamp(), e));
        }

        private static string Timestamp()
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss,ffff}", DateTime.Now);
        }

        public List<string> Lines { get; private set; }
    }
}
