using System.Collections.Generic;
using System.IO;

namespace DeCorrespondent
{
    public interface IMailer
    {
        void Send(string to, string subject, string body, IEnumerable<FileStream> attachments);
    }
}