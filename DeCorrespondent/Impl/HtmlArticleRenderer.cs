using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class HtmlArticleRenderer : IArticleRenderer
    {
        private static readonly HtmlNodeCollection EmptyNodes = new HtmlNodeCollection(null);
        private readonly ILogger log;
        private readonly IArticleRendererConfig config;
        private readonly IResourceReader resources;

        public HtmlArticleRenderer(ILogger log, IArticleRendererConfig config, IResourceReader resources)
        {
            this.log = log;
            this.config = config;
            this.resources = resources;
        }

        public IArticleEbook Render(IArticle a)
        {
            log.Debug("Rendering article '" + a.Metadata.Title + "' to html....");

            var doc = new HtmlDocument();
            doc.LoadHtml(CreateHtml(a));
            var body = doc.DocumentNode.SelectSingleNode("//body");
            (body.SelectNodes("//img[string-length(@src) > 0]") ?? EmptyNodes).Where(n => n != null).ToList().ForEach(n =>
            {
                var src = n.Attributes["src"].Value;
                if (src.EndsWith(".svg"))
                {
                    var svg = resources.Read(src);
                    n.ParentNode.ReplaceChild(HtmlNode.CreateNode(svg), n);
                }
                else
                {
                    var image = resources.ReadBinary(src);
                    n.Attributes["src"].Value = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(image));
                }

            });
            return new ArticleEbook(FormatName(string.Format("{0} {1}", a.Metadata.ReadingTime.Select(i => (int?)i).LastOrDefault(), a.Metadata.Title)).Trim() + ".html", Encoding.UTF8.GetBytes(doc.DocumentNode.OuterHtml));
        }

        public static string FormatName(string name)
        {
            const string invalid = @"<>:""/\|?*";
            return string.Join("", name.ToArray().Where(l => !invalid.Contains(l))).Trim();
        }

        private string CreateHtml(IArticle a)
        {
            const string template = @"<html>
    <head>
    <META NAME=""Author"" CONTENT=""{3} {4}"">
    <style>
        body {{ font-family: ""Times New Roman"", Georgia, Serif; text-align: left;}}
        .publication-main-image-description, figcaption {{ font-size: 0.5em; }}
        .infocard-description {{ font-size: 0.7em; font-style: italic; {10} }} 
        div.publication-body-link {{ background-color: #D3D3D3; {11} }}
        img {{ max-width:200; }}
        svg {{ max-width:50; }}
        img.author {{ max-height:50; }}
        blockquote {{ color: gray; {12} }}
        div.voorpagina img.author {{ align: right; }} 
        div.voorpagina {{ text-align:center; }}
        a {{ color:#000000; }}
        div.voorpagina img.author {{ float: right; margin-top:10px; }} 
        /*
        div.publication-body-link img {{ float: left; margin-right: 20px; }}
        div.voorpagina {{ height:1300px; text-align:center; }}
        div.voorpagina img.logo {{ height:90px; width:378px; }} 
        div.voorpagina img.main {{ min-width:100%; }} 
        
        div.voorpagina h3 {{ text-align:left; }}
        div.voorpagina p {{ font-size: 0.7em; }}
        div.descriptionpagina {{ height:1300px; }}
        */
    </style>
    </head>
    <body>
    <div class=""voorpagina"">
        <img class=""logo"" src=""https://static.decorrespondent.nl/images/nl/logo/logo_nl.svg""><br/>
        <img class=""author"" src=""{7}"">
        <h3>{1}</h3>
        <p>{3} {4} - {8}</p>
        <img class=""main"" src=""{6}"">
        <p>{2:dd-MM-yyyy H:mm} - Leestijd: {5}</p>
    </div>
    <div class=""descriptionpagina""><hr/>{9}<hr/></div>
    {0}
    </body>
</html>";
            return string.Format(template, 
                a.BodyHtml, //0
                HtmlEntity.Entitize(a.Metadata.Title), //1
                a.Metadata.Published, //2 
                HtmlEntity.Entitize(a.Metadata.AuthorFirstname), //3
                HtmlEntity.Entitize(a.Metadata.AuthorLastname),  //4
                DisplayReadingTime(a.Metadata.ReadingTime), //5
                a.Metadata.MainImgUrl, //6
                a.Metadata.AuthorImgUrl, //7
                HtmlEntity.Entitize(a.Metadata.Section), //8
                HtmlEntity.Entitize(a.Metadata.Description), //9
                config.DisplayInfocards ? "" : "display:none;", //10
                config.DisplayPublicationLinks ? "" : "display:none;", //11
                config.DisplayBlockquotes ? "" : "display:none;" //12
                ); 
        }

        private static string DisplayReadingTime(IList<int> readingTimes)
        {
            if (readingTimes == null || !readingTimes.Any() || (readingTimes.Count() == 1 && readingTimes.First() == 1) )
                return "1 minuut";
            return string.Format("{0} minuten", string.Join("-", readingTimes));
        }
    }
}
