using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EvoPdf;
using HtmlAgilityPack;

namespace DeCorrespondent.Impl
{
    public class PdfArticleRenderer : IArticleRenderer
    {
        private readonly ILogger log;
        private readonly IArticleRendererConfig config;
        private readonly string evoPdfLicenseKey;

        public PdfArticleRenderer(ILogger log, IArticleRendererConfig config, string evoPdfLicenseKey )
        {
            this.log = log;
            this.config = config;
            this.evoPdfLicenseKey = evoPdfLicenseKey;
        }

        public IArticleEbook Render(IArticle a)
        {
            log.Debug("Rendering article '" + a.Metadata.Title + "' to pdf....");
            var pdfConverter = CreatePdfConverter(a);
            var pdfOutputStream = new MemoryStream();
            //File.WriteAllText("d:\\temp.html", CreateHtml(a));
            pdfConverter.SavePdfFromHtmlStringToStream(CreateHtml(a), pdfOutputStream);
            return new ArticleEbook(FormatName(string.Format("{0} {1}", a.Metadata.ReadingTime.Select(i => (int?)i).LastOrDefault(), a.Metadata.Title)).Trim() + ".pdf", pdfOutputStream.GetBuffer());
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
    <style>
        body {{ font-family: Georgia, serif; font-size: 55px; }}
        .publication-main-image-description, figcaption {{ font-size: 0.5em; }}
        .infocard-description {{ font-size: 0.7em; font-style: italic; {10} }} 
        div.publication-body-link, div.publication-insertion-link {{ background-color: #D3D3D3; {11} }}
        div.publication-body-link img {{ float: left; margin-right: 20px; }}
        img {{ max-width:800; }}
        blockquote {{ color: gray; {12} }}
        div.author img {{ align: right; }} 
        div.voorpagina {{ height:1300px; text-align:center; }}
        div.voorpagina img.logo {{ height:90px; width:378px; }} 
        div.voorpagina img.main {{ min-width:100%; }} 
        div.voorpagina img.author {{ float: right; margin-top:110px; }} 
        div.voorpagina h3 {{ text-align:left; }}
        div.voorpagina p {{ font-size: 0.7em; }}
        div.descriptionpagina {{ height:1300px; }}
        a {{ color:#000000; }}
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
    <div class=""descriptionpagina"">{9}</div>
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

        private PdfConverter CreatePdfConverter(IArticle article)
        {
            var pdfConverter = new PdfConverter();
            if (!string.IsNullOrEmpty(evoPdfLicenseKey))
                pdfConverter.LicenseKey = evoPdfLicenseKey;
            pdfConverter.ExtensionsEnabled = false;
            pdfConverter.JavaScriptEnabled = false;
            pdfConverter.PdfDocumentInfo.Title = string.Format("{0} {1}", article.Metadata.ReadingTime.Select(i => (int?)i).LastOrDefault(), article.Metadata.Title).Trim();
            pdfConverter.PdfDocumentInfo.AuthorName = string.Format("{0} {1}", article.Metadata.AuthorFirstname, article.Metadata.AuthorLastname);
            pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;
            pdfConverter.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;
            pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
            pdfConverter.PdfDocumentOptions.LiveUrlsEnabled = false;
            pdfConverter.PdfDocumentOptions.AvoidImageBreak = true;
            pdfConverter.PdfDocumentOptions.InternalLinksEnabled = false;
            pdfConverter.PdfDocumentOptions.EmbedFonts = true;
            pdfConverter.PdfDocumentOptions.TopMargin = 0;
            pdfConverter.PdfDocumentOptions.BottomMargin = 0;
            pdfConverter.PdfDocumentOptions.LeftMargin = 0;
            pdfConverter.PdfDocumentOptions.RightMargin = 0;
            pdfConverter.PdfSecurityOptions.CanEditContent = false;
            pdfConverter.PdfSecurityOptions.CanEditAnnotations = false;
            pdfConverter.PdfSecurityOptions.CanAssembleDocument = false;
            pdfConverter.PdfSecurityOptions.CanCopyContent = true;
            pdfConverter.PdfSecurityOptions.CanFillFormFields = false;
            pdfConverter.PdfSecurityOptions.CanPrint = true;
            return pdfConverter;
        }
    }

    public class ArticleEbook : IArticleEbook
    {
        internal ArticleEbook(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; private set; }
        public byte[] Content { get; private set; }
        public EbookType Type { get { return EbookType.Pdf;} }
    }

    public interface IArticleRendererConfig
    {
        bool DisplayInfocards { get; }
        bool DisplayPublicationLinks { get; }
        bool DisplayBlockquotes { get; }
    }
}
