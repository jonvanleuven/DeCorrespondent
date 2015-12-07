
using System;

namespace DeCorrespondent
{
    public interface IResourceReader : IDisposable
    {
        string Read(string url);
        byte[] ReadBinary(string url);
    }
}
