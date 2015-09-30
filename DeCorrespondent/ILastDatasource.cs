using System;

namespace DeCorrespondent
{
    public interface ILastDatasource
    {
        DateTime? ReadLast();
        void UpdateLast(DateTime id);
    }
}
