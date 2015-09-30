using System;
using System.Globalization;
using System.IO;

namespace DeCorrespondent.Impl
{
    public class FileLastDatasource : ILastDatasource
    {
        private readonly string filename;
        public FileLastDatasource()
        {
            filename = "lastId.txt";
        }
        public DateTime? ReadLast()
        {
            if (!File.Exists(filename))
                return null;
            var filecontent = File.ReadAllText(filename);
            return DateTime.ParseExact(filecontent, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public void UpdateLast(DateTime d)
        {
            File.WriteAllText(filename, string.Format("{0:ddMMyyyyHHmmss}", d));
        }
    }
}
