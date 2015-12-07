using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DeCorrespondent.Test.Util
{
    public class FileResources : IResourceReader
    {
        public string Read(string url)
        {
            var name = string.Join("_", url.Split('/').Skip(3));
            int t;
            if (int.TryParse(name, out t))
                name = "article_" + name;

            var resource = GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources." + name);
            if (resource == null)
                throw new Exception("Resource not found: " + name);
            using (var s = new StreamReader(resource))
            {
                return s.ReadToEnd();
            }
        }


        public byte[] ReadBinary(string url)
        {
            var name = "img_" + url.Substring(url.LastIndexOf("/") + 1, url.Length - url.LastIndexOf("/") - 1);
            var resource = GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources." + name);
            if (resource == null)
                throw new Exception("Binary resource not found: " + name);
            using (var memoryStream = new MemoryStream())
            {
                resource.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public void Dispose()
        {
        }
    }
}
