using System.Collections.Generic;
using System.IO;

namespace DeCorrespondent
{
    public interface IArticleSender
    {
        void Send(IEnumerable<FileStream> ebooks);
    }
}
