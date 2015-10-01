using System;
using System.IO;
using System.Linq;
using EvoPdf;

namespace DeCorrespondent.Impl
{
    public class ArticleRenderer : IArticleRenderer
    {
        private readonly ILogger log;
        private readonly IArticleRendererConfig config;

        public ArticleRenderer(ILogger log, IArticleRendererConfig config)
        {
            this.log = log;
            this.config = config;
        }

        public IArticleEbook Render(IArticle a)
        {
            log.Debug("Rendering article '" + a.Metadata.Title + "' to pdf....");
            var pdfConverter = CreatePdfConverter(a);
            var pdfOutputStream = new MemoryStream();
            //File.WriteAllText("d:\\temp.html", WrapBody(a.BodyHtml));
            pdfConverter.SavePdfFromHtmlStringToStream(CreateHtml(a), pdfOutputStream);
            return new ArticleEbook(FormatName(string.Format("{0} {1}", a.Metadata.ReadingTime.LastOrDefault(), a.Metadata.Title)) + ".pdf", pdfOutputStream.GetBuffer());
        }

        public static string FormatName(string name)
        {
            return string.Join("", name.ToArray().TakeWhile(l => l != '(' && l != '.').Where(l => Char.IsLetter(l) || Char.IsNumber(l) || l == ' ' || l == '-')).Trim();
        }

        private static string CreateHtml(IArticle a)
        {
            const string template = @"<html>
    <head>
    <style>
        body {{ font-family: Georgia, serif; font-size: 55px; }}
        .publication-main-image-description, figcaption {{ font-size: 0.5em; }}
        .infocard-description {{ font-size: 0.7em; font-style: italic}} 
        img {{ max-width:800; }}
        blockquote {{ color: gray; }}
        div.author img {{ align: right; }} 
        div.voorpagina {{ height:1300px; text-align:center; border: 1px solid gray; }}
        div.voorpagina img.logo {{ height:90px; width:378px; }} 
        div.voorpagina img.author {{ float: right; margin-top:110px; }} 
        div.voorpagina h3 {{ text-align:left; }}
        div.voorpagina p {{ font-size: 0.5em; }}
    </style>
    </head>
    <body>
    <div class=""voorpagina"">
        <img class=""logo"" src=""https://static.decorrespondent.nl/images/nl/logo/logo_nl.svg""><br/>
        <img class=""author"" src=""{7}"">
        <h3>{1}</h3>
        <img class=""main"" src=""{6}"">
        <p>{2:dd-MM-yyyy H:mm} - Leestijd: {5} minuten<br/>{3} {4}</p>
    </div>
    {0}
    </body>
</html>";
            return string.Format(template, 
                a.BodyHtml, 
                a.Metadata.Title, 
                a.Metadata.Published, 
                a.Metadata.AuthorFirstname, 
                a.Metadata.AuthorLastname, 
                string.Join("-", a.Metadata.ReadingTime), 
                a.Metadata.MainImgUrl, 
                a.Metadata.AuthorImgUrl );
        }

        private PdfConverter CreatePdfConverter(IArticle article)
        {
            var pdfConverter = new PdfConverter();
            if (!string.IsNullOrEmpty(config.LicenseKey))
                pdfConverter.LicenseKey = config.LicenseKey;
            pdfConverter.ExtensionsEnabled = false;
            pdfConverter.JavaScriptEnabled = false;
            pdfConverter.PdfDocumentInfo.Title = string.Format("{0} {1}", article.Metadata.ReadingTime.LastOrDefault(), article.Metadata.Title);
            pdfConverter.PdfDocumentInfo.AuthorName = string.Format("{0} {1}", article.Metadata.AuthorFirstname, article.Metadata.AuthorLastname);
            pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;
            pdfConverter.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;
            pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
            pdfConverter.PdfDocumentOptions.LiveUrlsEnabled = false;
            pdfConverter.PdfDocumentOptions.AvoidImageBreak = true;
            pdfConverter.PdfDocumentOptions.InternalLinksEnabled = false;
            pdfConverter.PdfDocumentOptions.EmbedFonts = true;
            pdfConverter.PdfDocumentOptions.TopMargin = 10;
            pdfConverter.PdfDocumentOptions.BottomMargin = 10;
            pdfConverter.PdfDocumentOptions.LeftMargin = 10;
            pdfConverter.PdfDocumentOptions.RightMargin = 10;
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
        string LicenseKey { get; }
    }
}
