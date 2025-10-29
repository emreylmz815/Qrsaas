using System.IO;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KuaceMenu.Web.Services;

public interface IQrCodeService
{
    byte[] GeneratePng(string url);
    byte[] GeneratePdf(string url, string? title = null);
}

public class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string url)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic(20);
    }

    public byte[] GeneratePdf(string url, string? title = null)
    {
        var png = GeneratePng(url);
        QuestPDF.Settings.License = LicenseType.Community;

         var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A6);
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().AlignCenter().Text(title ?? "QR Menü").FontSize(18).SemiBold();
                    column.Item().AlignCenter().Image(png);
                    column.Item().AlignCenter().Text(url).FontSize(10);
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}
