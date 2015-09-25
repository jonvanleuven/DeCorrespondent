using System.IO;

namespace DeCorrespondent.Impl
{
    public class FileLastIdDatasource : ILastIdDatasource
    {
        private readonly string filename;
        public FileLastIdDatasource()
        {
            filename = "lastId.txt";
        }
        public int? ReadLastId()
        {
            return File.Exists(filename) ? int.Parse(File.ReadAllText(filename)) : (int?)null;
        }

        public void UpdateLastId(int id)
        {
            File.WriteAllText(filename, "" + id);
        }
    }
}
