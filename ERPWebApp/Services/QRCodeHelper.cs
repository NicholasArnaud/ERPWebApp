using QRCoder;

namespace ERPWebApp.Services
{
    public static class QRCodeHelper
    {
        public static string GenerateQRCodeBase64(string text)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var pngByteQRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = pngByteQRCode.GetGraphic(20);

            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}
