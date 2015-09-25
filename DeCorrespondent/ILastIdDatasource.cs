namespace DeCorrespondent
{
    public interface ILastIdDatasource
    {
        int? ReadLastId();
        void UpdateLastId(int id);
    }
}
