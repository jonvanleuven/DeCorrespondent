
using System;

namespace DeCorrespondent
{
    public interface IResourceReader : IDisposable
    {
        string ReadNewItems(int index);
        string ReadArticle(int articleId);
        byte[] ReadBinary(string url);
    }
}
