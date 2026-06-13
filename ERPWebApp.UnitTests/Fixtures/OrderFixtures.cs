using ERPWebApp.Models.Orders;
using static ERPWebApp.Models.Orders.Order;
using static ERPWebApp.Models.Orders.Order.OrderWeight;
using static ERPWebApp.Models.Orders.OrderItem;

namespace ERPWebApp.UnitTests.Fixtures;

public static class OrderFixtures
{
    public static List<Order> GetTestOrders() => [
        new Order{
            orderId = 1,
            ERPOrderId = 2,
            orderNumber = "1",
            orderKey = "12345",
            advancedOptions = {
                storeId = 1 // Matching StoreId from ShipStationStoreFixtures
            },
            items =
            [
                new() {
                    sku = "SKU001",
                    options =
                    [
                        new() {
                            Name = "PONumber",
                            value = "PO123"
                        }
                    ]
                }
            ],
            weight = new OrderWeight
            {
                value = 1,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 6,
                width = 4,
                height = 2,
                units = OrderDimensions.Units.inches
            },
            shipTo = new OrderShippingInfo
            {
                name = "John Doe",
                street1 = "123 Main St",
                city = "Metropolis",
                state = "NY",
                postalCode = "10001",
                country = "US",
                phone = "123-456-7890"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            Sources = [
                new() { OrderSourceId = 0, Name = OrderSourceEnum.custom }
            ]
        },
        new Order{
            orderId = 2,
            ERPOrderId = 3,
            orderNumber = "2",
            orderKey = "23456",
            advancedOptions = {
                storeId = 2 // Matching StoreId from ShipStationStoreFixtures
            },                
            items =
            [
                new() {
                    sku = "SKU002",
                    options =
                    [
                        new() {
                            Name = "PONumber",
                            value = "PO123"
                        }
                    ]
                }
            ],
            weight = new OrderWeight
            {
                value = 1,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 6,
                width = 4,
                height = 2,
                units = OrderDimensions.Units.inches
            },
            shipTo = new OrderShippingInfo
            {
                name = "Jane Smith",
                street1 = "456 Elm St",
                city = "Gotham",
                state = "NJ",
                postalCode = "07001",
                country = "US",
                phone = "987-654-3210"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            Sources = [
                new() { OrderSourceId = 2, Name = OrderSourceEnum.etsy},
                new() { OrderSourceId = 0, Name = OrderSourceEnum.custom }
            ]
        },
        new Order
        {
            ERPOrderId = 3,
            orderId = 1234567890,
            orderNumber = "ABC123",
            orderKey = "123ABC",
            orderDate = new DateTime(2022, 2, 28),
            createDate = new DateTime(2022, 2, 28),
            modifyDate = new DateTime(2022, 3, 1),
            paymentDate = new DateTime(2022, 3, 1),
            shipByDate = new DateTime(2022, 3, 5),
            orderStatus = OrderStatus.awaiting_shipment,
            customerId = 1,
            customerUsername = "johndoe",
            customerEmail = "johndoe@example.com",
            billTo = new OrderShippingInfo
            {
                street1 = "123 Main St",
                street2 = "Apt 4B",
                city = "New York",
                state = "NY",
                postalCode = "10001",
                country = "United States",
                name = "John Doe",
                phone = "555-555-5555",
                company = "ABC Inc.",
                residential = true
            },
            shipTo = new OrderShippingInfo
            {
                name = "Jane Doe",
                company = "",
                street1 = "789 Elm St.",
                street2 = "",
                city = "Othertown",
                state = "NY",
                postalCode = "67890",
                country = "US",
                phone = "555-987-6543",
                residential = true
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Acme Corp.",
                company = "",
                street1 = "456 Oak St.",
                street2 = "",
                city = "Somewhere",
                state = "CA",
                postalCode = "54321",
                country = "US",
                phone = "555-555-5555",
                residential = false
            },
            items =
            [
                new() {
                    sku = "SKU001",
                    name = "Product 1",
                    imageUrl = "https://example.com/product1.jpg",
                    quantity = 2,
                    unitPrice = 10.00m,
                    weight = new OrderWeight { value = 2, units = Units.pounds },
                    warehouseLocation = "A1",
                    options = [
                        new() {
                            Name = "PONumber",
                            value = "PO123"
                        }
                    ],
                    productId = 1
                },
                new() {
                    sku = "SKU002",
                    name = "Product 2",
                    imageUrl = "https://example.com/product2.jpg",
                    quantity = 1,
                    unitPrice = 20.00m,
                    weight = new OrderWeight { value = 1, units = Units.ounces },
                    warehouseLocation = "B2",
                    productId = 1
                }
            ],
            orderTotal = 40.00m,
            amountPaid = 40.00m,
            taxAmount = 0.00m,
            shippingAmount = 5.00m,
            customerNotes = "Please deliver to back door",
            internalNotes = "",
            gift = false,
            giftMessage = "",
            paymentMethod = "Credit Card",
            requestedShippingService = "Standard",
            carrierCode = "",
            carrierNickname = "",
            serviceCode = "",
            packageCode = "",
            confirmation = Confirmation.signature,
            shipDate = null,
            holdUntilDate = null,
            weight = new OrderWeight
            {
                value = 2.5m,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 10,
                width = 5,
                height = 3,
                units = OrderDimensions.Units.inches
            },
            insuranceOptions = new OrderInsuranceOptions {
                provider = OrderInsuranceOptions.Provider.shipsurance,
                insureShipment = true,
                insuredValue = 1000.00M
            },
            internationalOptions = new OrderInternationalOptions {
                contents = OrderInternationalOptions.Contents.merchandise,
                customsItems = [
                    new() {
                            description = "Widget",
                            quantity = 1,
                            value = 19.99m,
                            harmonizedTariffCode = "1234.56",
                            countryOfOrigin = "US"
                        },
                    new() {
                            description = "Gadget",
                            quantity = 2,
                            value = 29.99m,
                            harmonizedTariffCode = "7890.12",
                            countryOfOrigin = "CA"
                        }
                ]
            },
            advancedOptions = new OrderAdvancedOptions {
                warehouseId = 12345,
                nonMachinable = false,
                saturdayDelivery = true,
                containsAlcohol = false,
                storeId = 3, // Matching StoreId from ShipStationStoreFixtures
                storeName = "Example Store",
                customField1 = "Custom Field 1",
                customField2 = "Custom Field 2",
                customField3 = "Custom Field 3",
                source = "Website",
                mergedOrSplit = false,
                mergedIds = [123, 456],
                parentId = 123,
                billToParty = OrderAdvancedOptions.BillToParty.my_account,
                billToAccount = "Account 123",
                billToPostalCode = "12345",
                billToCountryCode = "US",
                billToMyOtherAccount = 456,
                labelMessageReference1 = "Reference 1",
                labelMessageReference2 = "Reference 2",
                labelMessageReference3 = "Reference 3"
            },
            tagIds = new int[] { 1, 2, 3 },
            userId = "user123",
            externallyFulfilled = true,
            externallyFulfilledBy = "Fulfillment Company A",
            orderShipments = [
                new() {
                    OrderShipmentId = 1,
                    shipmentId = 1234567890,
                    ERPOrderId = 1234,
                    Order = new Order(),
                    orderId = 9876543210,
                    userId = "user123",
                    orderKey = "orderkey123",
                    createDate = DateTime.Now.AddDays(-2),
                    shipDate = DateTime.Now.AddDays(-1),
                    shipmentCost = 12.34m,
                    insuranceCost = 1.23m,
                    trackingNumber = "1234567890",
                    isReturnLabel = false,
                    batchNumber = "batch123",
                    carrierCode = "USPS",
                    serviceCode = "Priority Mail",
                    packageCode = "Flat Rate Envelope",
                    confirmation = Confirmation.signature,
                    warehouseId = 1,
                    voided = false,
                    voidDate = null,
                    marketplaceNotified = true,
                    notifyErrorMessage = null,
                    shipFrom = new OrderShippingInfo
                    {
                        name = "John Smith",
                        company = "ABC Inc.",
                        street1 = "123 Main St",
                        street2 = "Apt 5B",
                        city = "Anytown",
                        state = "CA",
                        postalCode = "12345",
                        country = "US",
                        phone = "555-555-1234"
                    },
                    shipTo = new OrderShippingInfo
                    {
                        name = "Jane Doe",
                        company = "",
                        street1 = "456 Elm St",
                        street2 = "",
                        city = "Somewhere",
                        state = "TX",
                        postalCode = "67890",
                        country = "US",
                        phone = "555-555-5678"
                    },
                    weight = new OrderWeight
                    {
                        value = 2.5m,
                        units = Units.pounds
                    },
                    dimensions = new OrderDimensions
                    {
                        length = 10m,
                        width = 8m,
                        height = 6m,
                        units = OrderDimensions.Units.inches
                    },
                    advancedOptions = new OrderAdvancedOptions
                    {
                        warehouseId = 12345,
                        nonMachinable = false,
                        saturdayDelivery = true,
                        containsAlcohol = false,
                        storeId = 456,
                        storeName = "Example Store",
                        customField1 = "Custom Field 1",
                        customField2 = "Custom Field 2",
                        customField3 = "Custom Field 3",
                        source = "Website",
                        mergedOrSplit = false,
                        mergedIds = [123, 456],
                        parentId = 123,
                        billToParty = OrderAdvancedOptions.BillToParty.recipient,
                        billToAccount = "Account 123",
                        billToPostalCode = "12345",
                        billToCountryCode = "US",
                        billToMyOtherAccount = 456,
                        labelMessageReference1 = "Reference 1",
                        labelMessageReference2 = "Reference 2",
                        labelMessageReference3 = "Reference 3"
                    },
                    shipmentItems =
                    [
                        new() {
                            lineItemKey = "item123",
                            sku = "CHKSKU01",
                            name = "CHKSKU01",
                            imageUrl = "https://example.com/image.jpg",
                            weight = new OrderWeight
                            {
                                value = 1.5m,
                                units = Units.grams
                            },
                            quantity = 2,
                            unitPrice = 9.99m,
                            warehouseLocation = "A1"
                        }
                    ],
                    labelData = "Fake label data",
                    formData = "Fake form data",
                    testLabel = true,
                    ShippingAccountId = "Fake shipping account ID",
            IsExpedited = false,
                    ShipEngineShipmentId = "Fake ShipEngine shipment ID"
                }
            ],
            IsCreateShipmentClicked = false,
            IsGetEstimatesClicked = true,
            estimatedShipmentCost = 5.00m,
            Tags =
            [
                new()
                {
                    OrderTagId = 1,
                    tagId = 1,
                    name = "tag 1",
                    color = "#fff"
                },
                new()
                {
                    OrderTagId = 2,
                    tagId = 2,
                    name = "tag 2",
                    color = "#fff"
                },
                new()
                {
                    OrderTagId = 3,
                    tagId = 3,
                    name = "tag 3",
                    color = "#fff"
                }
            ]
        },
        new Order
        {
            ERPOrderId = 4,
            orderId = 1001,
            orderNumber = "ORD-1001",
            orderKey = "KEY-1001",
            orderDate = DateTime.Now.AddDays(-10),
            createDate = DateTime.Now.AddDays(-10),
            modifyDate = DateTime.Now.AddDays(-5),
            paymentDate = DateTime.Now.AddDays(-9),
            shipByDate = DateTime.Now.AddDays(-3),
            orderStatus = OrderStatus.awaiting_shipment,
            customerId = 1,
            customerUsername = "john_doe",
            customerEmail = "john.doe@example.com",
            billTo = new OrderShippingInfo
            {
                name = "John Doe",
                street1 = "123 Main St",
                city = "Metropolis",
                state = "NY",
                postalCode = "10001",
                country = "US",
                phone = "123-456-7890"
            },
            shipTo = new OrderShippingInfo
            {
                name = "John Doe",
                street1 = "123 Main St",
                city = "Metropolis",
                state = "NY",
                postalCode = "10001",
                country = "US",
                phone = "123-456-7890"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            items =
            [
                new OrderItem
                {
                    ERPOrderItemId = 1,
                    orderItemId = 2001,
                    lineItemKey = "ITEM-001",
                    name = "Product 1",
                    sku = "CHKSKU01",
                    quantity = 2,
                    unitPrice = 19.99m,
                    weight = new OrderWeight { value = 1.5m, units = Units.pounds }
                }
            ],
            orderTotal = 39.98m,
            amountPaid = 39.98m,
            taxAmount = 3.00m,
            shippingAmount = 5.00m,
            customerNotes = "Please handle with care.",
            internalNotes = "Priority customer.",
            gift = false,
            paymentMethod = "Credit Card",
            requestedShippingService = "Standard",
            carrierCode = "fedex",
            serviceCode = "fedex_ground",
            confirmation = Confirmation.delivery,
            weight = new OrderWeight { value = 3.0m, units = Units.pounds },
            dimensions = new OrderDimensions { length = 10.0m, width = 5.0m, height = 5.0m, units = OrderDimensions.Units.inches },
            Sources =
            [
                new() { OrderSourceId = 1, Name = OrderSourceEnum.shopify }
            ],
            orderShipments = [],
            orderFulfillments = [],
            estimatedShipmentCost = 8.00m
        },
        new Order
        {
            ERPOrderId = 5,
            orderId = 1002,
            orderNumber = "ORD-1002",
            orderKey = "KEY-1002",
            orderDate = DateTime.Now.AddDays(-7),
            createDate = DateTime.Now.AddDays(-7),
            modifyDate = DateTime.Now.AddDays(-3),
            paymentDate = DateTime.Now.AddDays(-6),
            shipByDate = DateTime.Now.AddDays(1),
            orderStatus = OrderStatus.awaiting_payment,
            customerId = 2,
            customerUsername = "jane_doe",
            customerEmail = "jane.doe@example.com",
            billTo = new OrderShippingInfo
            {
                name = "Jane Doe",
                street1 = "456 Side St",
                city = "Gotham",
                state = "NJ",
                postalCode = "07001",
                country = "US",
                phone = "987-654-3210"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Jane Doe",
                street1 = "456 Side St",
                city = "Gotham",
                state = "NJ",
                postalCode = "07001-4444",
                country = "US",
                phone = "987-654-3210"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            items =
            [
                new OrderItem
                {
                    ERPOrderItemId = 2,
                    orderItemId = 2002,
                    lineItemKey = "ITEM-002",
                    name = "Product 2",
                    sku = "BBQBOXSET",
                    quantity = 1,
                    unitPrice = 49.99m,
                    weight = new OrderWeight { value = 2.0m, units = Units.pounds }
                }
            ],
            orderTotal = 49.99m,
            amountPaid = 0.00m,
            taxAmount = 4.00m,
            shippingAmount = 7.00m,
            customerNotes = "Gift wrap this item.",
            internalNotes = "First-time customer.",
            gift = true,
            giftMessage = "Happy Birthday!",
            paymentMethod = "PayPal",
            requestedShippingService = "Express",
            carrierCode = "ups",
            serviceCode = "ups_2day",
            confirmation = Confirmation.signature,
            weight = new OrderWeight { value = 2.0m, units = Units.pounds },
            dimensions = new OrderDimensions { length = 12.0m, width = 6.0m, height = 6.0m, units = OrderDimensions.Units.inches },
            Sources =
            [
                 new() { OrderSourceId = 0, Name = OrderSourceEnum.custom },
                new() { OrderSourceId = 2, Name = OrderSourceEnum.etsy }
               
            ],
            orderShipments = [],
            orderFulfillments = [],
            estimatedShipmentCost = 10.00m
        },
        new Order
        {
            ERPOrderId = 6,
            orderId = 1003,
            orderNumber = "ORD-1003",
            orderKey = "KEY-1003",
            orderDate = DateTime.Now.AddDays(-3),
            createDate = DateTime.Now.AddDays(-3),
            modifyDate = DateTime.Now,
            paymentDate = DateTime.Now.AddDays(-2),
            shipByDate = DateTime.Now.AddDays(5),
            orderStatus = OrderStatus.awaiting_shipment,
            customerId = 1,
            customerUsername = "alice_wonder",
            customerEmail = "alice.wonder@example.com",
            billTo = new OrderShippingInfo
            {
                name = "Alice Wonder",
                street1 = "789 Wonderland Ave",
                city = "Wonderland",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-123-4567"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Alice Wonder",
                street1 = "789 Wonderland Ave",
                city = "Wonderland",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-123-4567"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            items =
            [
                new OrderItem
                {
                    ERPOrderItemId = 3,
                    orderItemId = 2003,
                    lineItemKey = "ITEM-003",
                    name = "Product 3",
                    sku = "BUNDLESO05",
                    quantity = 3,
                    unitPrice = 29.99m,
                    weight = new OrderWeight { value = 1.0m, units = Units.pounds }
                }
            ],
            orderTotal = 89.97m,
            amountPaid = 89.97m,
            taxAmount = 6.00m,
            shippingAmount = 8.00m,
            customerNotes = "Leave at front door.",
            internalNotes = "Repeat customer.",
            gift = false,
            paymentMethod = "Credit Card",
            requestedShippingService = "Standard",
            carrierCode = "usps",
            serviceCode = "usps_priority",
            confirmation = Confirmation.adult_signature,
            weight = new OrderWeight { value = 3.0m, units = Units.pounds },
            dimensions = new OrderDimensions { length = 14.0m, width = 7.0m, height = 7.0m, units = OrderDimensions.Units.inches },
            Sources =
            [
                new() { OrderSourceId = 3, Name = OrderSourceEnum.amazon },
                new() { OrderSourceId = 0, Name = OrderSourceEnum.custom }
            ],
            orderShipments = [],
            orderFulfillments = [],
            estimatedShipmentCost = 9.00m
        },
        new Order
        {
            orderId = 1004,
            ERPOrderId = 7,
            orderNumber = "ORD-1004",
            orderKey = "KEY-1004",
            orderDate = DateTime.Now.AddDays(-2),
            createDate = DateTime.Now.AddDays(-2),
            modifyDate = DateTime.Now,
            paymentDate = DateTime.Now.AddDays(-1),
            shipByDate = DateTime.Now.AddDays(3),
            orderStatus = OrderStatus.awaiting_shipment,
            customerId = 4,
            customerUsername = "bob_builder",
            customerEmail = "bob.builder@example.com",
            billTo = new OrderShippingInfo
            {
                name = "Bob Builder",
                street1 = "123 Construction Ave",
                city = "Buildtown",
                state = "TX",
                postalCode = "75001",
                country = "US",
                phone = "555-123-4567"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Bob Builder",
                street1 = "123 Construction Ave",
                city = "Buildtown",
                state = "TX",
                postalCode = "75001",
                country = "US",
                phone = "555-123-4567"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            items = [
                new OrderItem
                {
                    ERPOrderItemId = 4,
                    orderItemId = 2004,
                    lineItemKey = "ITEM-004",
                    name = "Product 4",
                    sku = "TOOLSET01",
                    quantity = 1,
                    unitPrice = 99.99m,
                    weight = new OrderWeight { value = 5.0m, units = Units.pounds }
                }
            ],
            orderTotal = 99.99m,
            amountPaid = 99.99m,
            taxAmount = 8.00m,
            shippingAmount = 10.00m,
            customerNotes = "Deliver to the garage.",
            internalNotes = "VIP customer.",
            gift = false,
            paymentMethod = "Credit Card",
            requestedShippingService = "Standard",
            carrierCode = "ups",
            serviceCode = "ups_ground",
            confirmation = Confirmation.delivery,
            weight = new OrderWeight { value = 5.0m, units = Units.pounds },
            dimensions = new OrderDimensions { length = 15.0m, width = 10.0m, height = 8.0m, units = OrderDimensions.Units.inches },
            Sources = [
                new() { OrderSourceId = 4, Name = OrderSourceEnum.amazon }
            ],
            orderShipments = [],
            orderFulfillments = [],
            estimatedShipmentCost = 12.00m
        },
        new Order
        {
            orderId = 1005,
            ERPOrderId = 8,
            orderNumber = "ORD-1005",
            orderKey = "KEY-1005",
            orderDate = DateTime.Now.AddDays(-1),
            createDate = DateTime.Now.AddDays(-1),
            modifyDate = DateTime.Now,
            paymentDate = DateTime.Now,
            shipByDate = DateTime.Now.AddDays(4),
            orderStatus = OrderStatus.awaiting_shipment,
            customerId =5,
            customerUsername = "susan_smith",
            customerEmail = "susan.smith@example.com",
            billTo = new OrderShippingInfo
            {
                name = "Susan Smith",
                street1 = "456 Fashion Blvd",
                city = "Stylestown",
                state = "NY",
                postalCode = "10002",
                country = "US",
                phone = "555-987-6543"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Susan Smith",
                street1 = "456 Fashion Blvd",
                city = "Stylestown",
                state = "NY",
                postalCode = "10002",
                country = "US",
                phone = "555-987-6543"
            },
            shipFrom = new OrderShippingInfo
            {
                name = "Default Warehouse",
                street1 = "123 Default St",
                city = "Default City",
                state = "CA",
                postalCode = "90001",
                country = "US",
                phone = "555-555-5555"
            },
            items = [
                new OrderItem
                {
                    ERPOrderItemId = 5,
                    orderItemId = 2005,
                    lineItemKey = "ITEM-005",
                    name = "Product 5",
                    sku = "DRESS01",
                    quantity = 2,
                    unitPrice = 49.99m,
                    weight = new OrderWeight { value = 2.0m, units = Units.pounds }
                }
            ],
            orderTotal = 99.98m,
            amountPaid = 99.98m,
            taxAmount = 8.00m,
            shippingAmount = 10.00m,
            customerNotes = "Leave at the front door.",
            internalNotes = "First-time customer.",
            gift = true,
            giftMessage = "Happy Anniversary!",
            paymentMethod = "PayPal",
            requestedShippingService = "Express",
            carrierCode = "fedex",
            serviceCode = "fedex_2day",
            confirmation = Confirmation.signature,
            weight = new OrderWeight { value = 4.0m, units = Units.pounds },
            dimensions = new OrderDimensions { length = 12.0m, width = 8.0m, height = 6.0m, units = OrderDimensions.Units.inches },
            Sources = [
                new() { OrderSourceId = 5, Name = OrderSourceEnum.shopify }
            ],
            orderShipments = [],
            orderFulfillments = [],
            estimatedShipmentCost = 15.00m
        }
    ];

    // Get a domestic order (US shipping)
    public static Order GetDomesticShippingOrder()
    {
        var order = GetTestOrders()[0];
        order.shipTo = new OrderShippingInfo
        {
            country = "US",
            state = order.shipTo.state,
            street1 = order.shipTo.street1,
            street2 = order.shipTo.street2,
            city = order.shipTo.city,
            postalCode = order.shipTo.postalCode,
            name = order.shipTo.name,
            company = order.shipTo.company,
            phone = order.shipTo.phone,
            residential = order.shipTo.residential
        };
        return order;
    }

    // Get an international order (non-US shipping)
    public static Order GetInternationalShippingOrder()
    {
        var order = GetTestOrders()[0];
        order.shipTo = new OrderShippingInfo
        {
            country = "CA", // Canada
            state = order.shipTo.state,
            street1 = order.shipTo.street1,
            street2 = order.shipTo.street2,
            city = order.shipTo.city,
            postalCode = order.shipTo.postalCode,
            name = order.shipTo.name,
            company = order.shipTo.company,
            phone = order.shipTo.phone,
            residential = order.shipTo.residential
        };
        return order;
    }

    // Get a Skulabs order with domestic shipping
    public static Order GetSkulabsOrder()
    {
        var order = GetTestOrders()[1];
        // Ensure it has Skulabs source
        order.Sources = new List<OrderSource> { new OrderSource { Name = OrderSourceEnum.skulabs } };
        order.shipTo = new OrderShippingInfo
        {
            country = "US",
            state = order.shipTo.state,
            street1 = order.shipTo.street1,
            street2 = order.shipTo.street2,
            city = order.shipTo.city,
            postalCode = order.shipTo.postalCode,
            name = order.shipTo.name,
            company = order.shipTo.company,
            phone = order.shipTo.phone,
            residential = order.shipTo.residential ?? false
        };
        return order;
    }
}