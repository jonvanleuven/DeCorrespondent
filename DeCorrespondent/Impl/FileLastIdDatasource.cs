using System.IO;

namespace DeCorrespondent.Impl
{
    public class FileLastIdDatasource : ILastIdDatasource
    {
        public int? ReadLastId()
        {
            return File.Exists("lastId.txt") ? int.Parse(File.ReadAllText("lastId.txt")) : (int?)null;
        }

        public void UpdateLastId(int id)
        {
            File.WriteAllText("lastId.txt", "" + id);
        }
    }
}
