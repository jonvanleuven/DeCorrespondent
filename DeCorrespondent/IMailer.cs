using System;
using System.Collections.Generic;
using System.IO;

namespace DeCorrespondent
{
    public interface IMailer
    {
        void Send(IEnumerable<string> to, string subject, string body, IEnumerable<Func<FileStream>> attachments);
    }
}