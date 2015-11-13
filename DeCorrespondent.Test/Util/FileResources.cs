using System;
using System.IO;

namespace DeCorrespondent.Test.Util
{

    public class FileResources : IResourceReader
    {
        public string ReadNewItems(int index)
        {
            var resource = GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources.nieuw_" + index);
            if (resource == null)
                throw new Exception("New item resource not found: " + index);
            using (var s = new StreamReader(resource))
            {
                return s.ReadToEnd();
            }
        }

        public string ReadArticle(int articleId)
        {
            var resource = GetType().Assembly.GetManifestResourceStream("DeCorrespondent.Test.Resources.article_" + articleId);
            if (resource == null)
                throw new Exception("Article resource not found: " + articleId);
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
