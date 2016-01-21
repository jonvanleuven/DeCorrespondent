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
            Lines.Add(string.Format("{0:dd-MM-yyyy HH:mm:ss} INFO  {1}", DateTime.Now, string.Format(message, args)));
        }

        public void Debug(string message, params object[] args)
        {
            Lines.Add(string.Format("{0:dd-MM-yyyy HH:mm:ss} DEBUG {1}", DateTime.Now, string.Format(message, args)));
        }

        public void Error(Exception e)
        {
            Lines.Add(string.Format("{0:dd-MM-yyyy HH:mm:ss} ERROR {1}\n{2}", DateTime.Now, e.Message, e.StackTrace));
        }

        public List<string> Lines { get; private set; }
    }
}
