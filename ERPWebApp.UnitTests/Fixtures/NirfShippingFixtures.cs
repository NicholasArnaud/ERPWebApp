using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class NirfShippingFixtures
    {
        public static List<NirfShipping> GetTestList() => [
            new NirfShipping
            {
                NirfShippingId = 1,
                SignedBy = "John Doe",
                SignedOn = DateTime.Now,
                AspUserId = "johndoe123",
                NirfFormId = 1,
                Comments = "Please handle with care",
                NirfShippingProvider = NirfShippingProdivderFixtures.GetTestList()
            }
         ];
    }
}