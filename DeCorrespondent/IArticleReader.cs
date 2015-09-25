using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCorrespondent
{
    public interface IArticleReader
    {
        IArticle Read(string article);
    }
}
