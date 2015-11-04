using System;
using System.Collections.Generic;
using System.IO;

namespace DeCorrespondent
{
    public interface IEReaderSender
    {
        void Send(IEnumerable<Func<FileStream>> ebooks);
    }
}
