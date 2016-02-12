using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class HtmlArticleRenderer : IArticleRenderer
    {
        private static readonly HtmlNodeCollection EmptyNodes = new HtmlNodeCollection(null);
        private readonly ILogger log;
        private readonly IArticleRendererConfig config;
        private readonly IResourceReader resources;

        public HtmlArticleRenderer(ILogger log, IArticleRendererConfig config)
        {
            this.log = log;
            this.config = config;
            this.resources = new WebReader(new ConsoleLogger(false));
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
                if (src.StartsWith("/"))
                    src = "https://decorrespondent.nl" + src;
                try
                {
                    if (src.EndsWith(".svg"))
                    {
                        var svg = resources.Read(src);
                        n.ParentNode.ReplaceChild(HtmlNode.CreateNode(svg), n);
                    }
                    else
                    {
                        var image = resources.ReadBinary(src);
                        var extension = src.Split('.').LastOrDefault();
                        n.Attributes["src"].Value = string.Format("data:image/{1};base64,{0}", Convert.ToBase64String(image), extension);
                    }
                }
                catch (WebException e)
                {
                    log.Info("Ignoring error reading image source: '" + src + "', error: " + e.Message );
                }
            });
            (body.SelectNodes("//iframe") ?? EmptyNodes).Where(n => n != null).ToList().ForEach(n => n.ParentNode.ReplaceChild(HtmlNode.CreateNode(@"<img src=""data:image/png;base64,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""/>"), n));
            return new ArticleEbook(FormatName(string.Format("{0} {1} ({2})", a.Metadata.ReadingTime.Select(i => (int?)i).LastOrDefault(), a.Metadata.Title, a.Metadata.AuthorLastname)).Trim() + ".html", doc.Encoding.GetBytes(doc.DocumentNode.OuterHtml));
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
        body {{ text-align: left;}}
        .publication-main-image-description, figcaption {{ font-size: 0.5em; }}
        .infocard-description {{ font-size: 0.7em; font-style: italic; {10} }} 
        div.publication-body-link {{ background-color: #D3D3D3; {11} }}
        img {{ max-width:200; }}
        svg {{ max-width:50; }}
        h1, h2, h3, h4 {{ font-weight:bold; text-align: left; }}
        img.author {{ max-height:50; }}
        blockquote {{ color: gray; {12} }}
        div.voorpagina img.author {{ align: right; }} 
        div.voorpagina {{ text-align:center; }}
        div.voorpagina p {{ font-size: 0.7em; }}
        a {{ color:#000000; }}
        div.voorpagina img.author {{ float: right; margin-top:10px; }} 
        p.publication-body-description {{ font-size: 0.7em; }}

        /*
        div.voorpagina {{ height:1300px; text-align:center; }}
        div.voorpagina img.logo {{ height:90px; width:378px; }} 
        div.voorpagina img.main {{ min-width:100%; }} 
        div.publication-body-link img {{ float: left; }}
        div.voorpagina h3 {{ text-align:left; }}
        div.voorpagina p {{ font-size: 0.7em; }}
        div.descriptionpagina {{ height:1300px; }}
        */
    </style>
    </head>
    <body>
    <div class=""voorpagina"">
        <img class=""logo"" src=""https://static.decorrespondent.nl/images/nl/logo/logo_nl.svg""><br/>
        {7}
        <h2>{1}</h2>
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
                a.Metadata.ReadingTimeDisplay, //5
                a.Metadata.MainImgUrl, //6
                string.IsNullOrEmpty(a.Metadata.AuthorImgUrl)?"":string.Format(@"<img class=""author"" src=""{0}"">", a.Metadata.AuthorImgUrl), //7
                HtmlEntity.Entitize(a.Metadata.Section), //8
                HtmlEntity.Entitize(a.Metadata.Description), //9
                config.DisplayInfocards ? "" : "display:none;", //10
                config.DisplayPublicationLinks ? "" : "display:none;", //11
                config.DisplayBlockquotes ? "" : "display:none;" //12
                ); 
        }
    }
}
