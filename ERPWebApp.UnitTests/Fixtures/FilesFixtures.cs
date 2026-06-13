using System.Text;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class FilesFixtures
    {
        public static List<Files> GetTestFiles() =>
        [
            new Files
            {
                FileId = 1,
                ProductId = 1,
                FileName = "test.png",
                FileType = FileType.Image,
                ContentType = "image/png",
                IsThumbnail = true,
                IsDetailed = true,
                Content = Encoding.UTF8.GetBytes("test image")
            },
            new Files
            {
                FileId = 2,
                ProductId = 1,
                FileName = "test.png",
                FileType = FileType.Image,
                ContentType = "image/png",
                IsThumbnail = true,
                IsDetailed = true,
                Content = Encoding.UTF8.GetBytes("test image")
            }
        ];
    }
}