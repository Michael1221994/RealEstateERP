using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Pdf;
using RazorEngine;
using RazorEngine.Templating;

namespace Infrastracture.Base.EF.ITextPdfGenerator
{
    public class ITextPdfGenerator : IPDFGenerator
    {
        public byte[] GeneratePdfFromRazorTemplate<T>(string templateContent, T model)


        {
            string htmlContent = Engine.Razor.RunCompile(templateContent, Guid.NewGuid().ToString(), typeof(T), model);
            string fullHtml = $"<html>{htmlContent}</html>";

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            var properties = new ConverterProperties();
            var fontProvider = new DefaultFontProvider();
            fontProvider.AddFont("./nyala.ttf");
            properties.SetFontProvider(fontProvider);


            HtmlConverter.ConvertToPdf(fullHtml, pdf, properties);
            return stream.ToArray();
        }
    }
}
