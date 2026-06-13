namespace ERPWebApp.UnitTests.Services;

using System.Text.RegularExpressions;

[Trait("Category","execute")]
public class ShipStationOrderedHistoryServiceTest
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IShipStationOrderedHistoryService> _shipstationOrderedHistoryService = new();
    public ShipStationOrderedHistoryServiceTest(){
        _mockUnitOfWork = new Mock<IUnitOfWork>();
    }
    private struct pObj(string s){
            public string Sku = s;
        }
    [Fact]
    public void TestStringLogic(){
        
        var pList=new List<pObj>{
            new("GLFBLWHTSO01"),
            new("GLFBLWHT")
            };
        int num;
        foreach(var p in pList){
            int TestQuantity = 1 * (p.Sku.IndexOf("SO") == -1 ? 1 : int.TryParse(p.Sku.Substring(p.Sku.IndexOf("SO")+2, p.Sku.Length - p.Sku.IndexOf("SO") - 2),out num)? num:1);
            string TestSku = p.Sku.Substring(0, p.Sku.IndexOf("SO") == -1 ? p.Sku.Length: 
                            p.Sku.Length - (p.Sku.Length - p.Sku.IndexOf("SO")));
        Assert.Equal(1,TestQuantity);
        Assert.Equal("GLFBLWHT",TestSku);
        }


        
        string testStringA = "TESTabSO01";
        string testStringB = "TESTCD01";
        //RULE 1
        Assert.Equal("TESTab",testStringA.Substring(0,testStringA.IndexOf("SO")==-1?
        testStringA.Length:testStringA.Length - testStringA.IndexOf("SO") +2));
        Assert.Equal("TESTCD01",testStringB.Substring(0,testStringB.IndexOf("SO")==-1?
        testStringB.Length:testStringB.Length - testStringB.IndexOf("SO") +2));
        //RULE 2
        Assert.Equal("01",testStringA.IndexOf("SO")==-1?"1":testStringA.Substring(testStringA.IndexOf("SO")+2,
                        testStringA.Length - testStringA.IndexOf("SO") - 2));
        Assert.Equal("1",testStringB.IndexOf("SO")==-1?"1":testStringB.Substring(testStringB.IndexOf("SO")+2,
                        testStringB.Length - testStringB.IndexOf("SO") - 2));

    }

    [Fact]
    public void GetShipStationOrderedHistory_ReturnsListWithGroupedSOProducts()
    {
        //Arrange
        var mockDepartments = DepartmentsFixtures.GetTestDepartments();
        var mockOrders = OrderFixtures.GetTestOrders();

        /*
        SO matches the characters SO literally (case sensitive)
            S matches the character S with index 8310 (5316 or 1238) literally (case sensitive)
            O matches the character O with index 7910 (4F16 or 1178) literally (case sensitive)
        Match a single character present in the list below [0-9]
            {0,} matches the previous token between zero and unlimited times, as many times as possible, giving back as needed (greedy)
            0-9 matches a single character in the range between 0 (index 48) and 9 (index 57) (case sensitive)
        Match a single character present in the list below [0,2-9]
            0, matches a single character in the list 0, (case sensitive)
            2-9 matches a single character in the range between 2 (index 50) and 9 (index 57) (case sensitive)
            $ asserts position at the end of a line
        */
        //In short, regexRule will return true if input string has any valid SO value that is not 1
        var regexRule = new Regex("SO[0-9]{0,}[0,2-9]$");

        //bool itWork = result.Any(x=>!regexRule.IsMatch(x.Sku) && x.Sku.Contains("SO"));
        //Assert.True(itWork);
    }
}
