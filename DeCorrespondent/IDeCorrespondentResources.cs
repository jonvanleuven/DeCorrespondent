﻿using System;
using System.Collections.Generic;

namespace DeCorrespondent
{
    public interface IDeCorrespondentResources : IDisposable
    {
        IEnumerable<INieuwItem> ReadNieuwItems();
        string ReadArticle(int id);
    }

    public interface INieuwItem
    {
        int Id { get; }
        DateTime Publicationdate { get; }
    }
}