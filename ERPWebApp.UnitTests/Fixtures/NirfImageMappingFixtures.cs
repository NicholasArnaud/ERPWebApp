using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfImageMappingFixtures
    {
        public static List<NirfImageMapping> GetTestList() => [
            new NirfImageMapping
            {
                Files = new Files { FileName = "file1.jpg" },
                NirfForm = NirfFormFixtures.GetTestList().First(),
                IsThumbnail = false
            },
            new NirfImageMapping
            {
                Files = new Files { FileName = "file2.png" },
                NirfForm = NirfFormFixtures.GetTestList().First(),
                IsThumbnail = true
            },
            new NirfImageMapping
            {
                Files = new Files { FileName = "file3.pdf" },
                NirfForm = NirfFormFixtures.GetTestList().First(),
                IsThumbnail = false
            }
        ];
    }
}