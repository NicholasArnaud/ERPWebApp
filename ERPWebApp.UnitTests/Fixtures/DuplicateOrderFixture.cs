using ERPWebApp.Data.DTOModels;
using static ERPWebApp.Models.Orders.Order;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class DuplicateOrderFixture
    {
        public static List<DuplicateOrderDTO> GetTestDuplicateOrders() =>
        [
            new DuplicateOrderDTO
            {
                ERPOrderId = 123,
                OrderNumber = "ABC123",
                StoreId = 1,
                StoreName = "Sample Store",
                CustomerNotes = "Customer notes",
                InternalNotes = "Internal notes",
                FromResidential = true,
                FromName = "John Doe",
                FromStreet1 = "123 Main St",
                FromCity = "Anytown",
                FromState = "State",
                FromPostalCode = "12345",
                FromCountry = "Country",
                FromPhone = "123-456-7890",
                FromCompany = "Company",
                ToResidential = false,
                ToName = "Jane Smith",
                ToStreet1 = "456 Elm St",
                ToCity = "Othertown",
                ToState = "Another State",
                ToPostalCode = "54321",
                ToCountry = "Another Country",
                ToPhone = "987-654-3210",
                ToCompany = "Another Company",
                DuplicationReason = DuplicationReason.test,
                OrderStatus = OrderStatus.awaiting_shipment,
                TagIds = new int[] { 1, 2, 3 },
                OrderItems =
                [
                    new() {
                        ERPOrderItemId = 1,
                        Quantity = 2,
                        Sku = "SKU123",
                        Name = "Product Name",
                        ImageUrl = "http://example.com/image.jpg",
                        UnitPrice = 10.99m,
                        ProductId = 1001,
                        Product = new Product()
                        {
                            ProductId = 1,
                            SubCategoryId =1,
                            SubCategory = new Models.SubCategory()
                            {
                                SubCategoryId =1,
                                Description ="Sub_01"
                            },
                            Sku ="100",
                            Description = "Product_01",
                            Departments = DepartmentsFixtures.GetTestDepartments().FindAll(static x=>x.DepartmentId == 1 || x.DepartmentId == 2),
                            ProductImages = ProductImageFixtures.GetProductImageFixtures(),
                            StockTotalAvailable = 10,
                            IsActive = true,
                            IsExternalProduct =true,
                        }
                    }
                ],
                AdvancedOptionsDTO = new DuplicateAdvancedOptionsDTO
                {
                    NonMachinable = true,
                    CustomField1 = "Custom Field 1",
                    CustomField2 = "Custom Field 2",
                    CustomField3 = "Custom Field 3",
                    Source = "Source",
                    BillToParty = OrderAdvancedOptions.BillToParty.third_party,
                    BillToAccount = "Bill to Account",
                    BillToPostalCode = "54321",
                    BillToCountryCode = "Country Code",
                    BillToMyOtherAccount = "Other Account",
                    LabelMessageReference1 = "Label Message 1",
                    LabelMessageReference2 = "Label Message 2",
                    LabelMessageReference3 = "Label Message 3"
                }
            }
        ];

    }
}
