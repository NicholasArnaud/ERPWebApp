using ERPWebApp.Models.Orders;
using static ERPWebApp.Models.Orders.Order;
using static ERPWebApp.Models.Orders.Order.OrderWeight;

namespace ERPWebApp.UnitTests.Fixtures;

public static class OrderShipmentFixtures
{
    public static List<OrderShipment> GetTestOrderShipment() => [
        // Shipment for order with ERPOrderId = 3 (matches OrderFixtures)
        new OrderShipment{
            OrderShipmentId = 775264,
            shipmentId = 0,
            ERPOrderId = 3,
            orderId = 1666542568,
            userId = "BlakeM",
            orderKey = "OD4567329466",
            createDate = new DateTime(2025, 4, 8, 11, 16, 47, 918),
            shipDate = new DateTime(2025, 4, 8, 11, 16, 47, 918),
            shipmentCost = 3.93m,
            insuranceCost = 0.00m,
            trackingNumber = "9200190382432300024583",
            isReturnLabel = false,
            batchNumber = null,
            carrierCode = "USPS",
            serviceCode = "usps_ground_advantage",
            packageCode = "package",
            confirmation = Confirmation.none,
            warehouseId = null,
            voided = false,
            voidDate = null,
            marketplaceNotified = true,
            notifyErrorMessage = null,
            shipFrom = new OrderShippingInfo
            {
                name = "Warehouse",
                street1 = "123 Ship St",
                city = "ShipCity",
                state = "SC",
                postalCode = "12345",
                country = "US"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Customer One",
                street1 = "456 Delivery Ave",
                city = "RecCity",
                state = "RC",
                postalCode = "67890",
                country = "US",
                residential = true
            },
            weight = new OrderWeight
            {
                value = 3.0m,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 6,
                width = 10,
                height = 1,
                units = OrderDimensions.Units.inches
            },
            labelData = "data:application/pdf;base64,JVBERi0xLjQ",
            testLabel = false,
            ShippingAccountId = null,
            IsExpedited = false,
            ShipEngineShipmentId = null
        },

        // Shipment for order with ERPOrderId = 4 (matches OrderFixtures)
        new OrderShipment{
            OrderShipmentId = 775514,
            shipmentId = 0,
            ERPOrderId = 4,
            orderId = 1666236590,
            userId = "DominicL",
            orderKey = "ee427772-6339-e69a-6947-fc54dd964f87",
            createDate = new DateTime(2025, 4, 8, 13, 0, 21, 170),
            shipDate = new DateTime(2025, 4, 8, 13, 0, 21, 170),
            shipmentCost = 4.15m,
            insuranceCost = 0.00m,
            trackingNumber = "9200190382432300024699",
            isReturnLabel = false,
            batchNumber = null,
            carrierCode = "USPS",
            serviceCode = "usps_ground_advantage",
            packageCode = "package",
            confirmation = Confirmation.none,
            warehouseId = null,
            voided = false,
            voidDate = null,
            marketplaceNotified = true,
            notifyErrorMessage = null,
            shipFrom = new OrderShippingInfo
            {
                name = "Warehouse",
                street1 = "789 Ship St",
                city = "ShipCity",
                state = "SC",
                postalCode = "12345",
                country = "US"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Customer Two",
                street1 = "789 Delivery St",
                city = "RecCity",
                state = "RC",
                postalCode = "67890",
                country = "US",
                residential = true
            },
            weight = new OrderWeight
            {
                value = 6.0m,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 6,
                width = 9,
                height = 1,
                units = OrderDimensions.Units.inches
            },
            labelData = "data:application/pdf;base64,JVBERi0xLjQ",
            testLabel = false,
            ShippingAccountId = null,
            IsExpedited = false,
            ShipEngineShipmentId = null
        },

        // Shipment for order with ERPOrderId = 5 (matches OrderFixtures)
        new OrderShipment{
            OrderShipmentId = 775521,
            shipmentId = 0,
            ERPOrderId = 5,
            orderId = 1666203012,
            userId = "DominicL",
            orderKey = "73eeed9a-4ae0-436c-b920-50c25aa82324",
            createDate = new DateTime(2025, 4, 8, 13, 3, 9, 30),
            shipDate = new DateTime(2025, 4, 8, 13, 3, 9, 30),
            shipmentCost = 4.15m,
            insuranceCost = 0.00m,
            trackingNumber = "9200190382432300024729",
            isReturnLabel = false,
            batchNumber = null,
            carrierCode = "USPS",
            serviceCode = "usps_ground_advantage",
            packageCode = "package",
            confirmation = Confirmation.none,
            warehouseId = null,
            voided = false,
            voidDate = null,
            marketplaceNotified = true,
            notifyErrorMessage = null,
            shipFrom = new OrderShippingInfo
            {
                name = "Warehouse",
                street1 = "321 Ship St",
                city = "ShipCity",
                state = "SC",
                postalCode = "12345",
                country = "US"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Customer Three",
                street1 = "321 Delivery St",
                city = "RecCity",
                state = "RC",
                postalCode = "67890",
                country = "US",
                residential = true
            },
            weight = new OrderWeight
            {
                value = 8.0m,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 6,
                width = 9,
                height = 1,
                units = OrderDimensions.Units.inches
            },
            labelData = "data:application/pdf;base64,JVBERi0xLjQ",
            testLabel = false,
            ShippingAccountId = null,
            IsExpedited = false,
            ShipEngineShipmentId = null
        },

        // UPS Shipment example (different carrier)
        new OrderShipment{
            OrderShipmentId = 775063,
            shipmentId = 0,
            ERPOrderId = 6,
            orderId = 1665948367,
            userId = "danielg",
            orderKey = "3650668289",
            createDate = new DateTime(2025, 4, 7, 17, 14, 59, 633),
            shipDate = new DateTime(2025, 4, 7, 17, 14, 59, 633),
            shipmentCost = 9.87m,
            insuranceCost = 0.00m,
            trackingNumber = "1Z21003R0396407721",
            isReturnLabel = false,
            batchNumber = null,
            carrierCode = "ups",
            serviceCode = "ups_ground",
            packageCode = null,
            confirmation = Confirmation.none,
            warehouseId = null,
            voided = false,
            voidDate = null,
            marketplaceNotified = true,
            notifyErrorMessage = null,
            shipFrom = new OrderShippingInfo
            {
                name = "Warehouse",
                street1 = "456 Ship St",
                city = "ShipCity",
                state = "SC",
                postalCode = "12345",
                country = "US"
            },
            shipTo = new OrderShippingInfo
            {
                name = "Customer Four",
                street1 = "456 Delivery St",
                city = "RecCity",
                state = "RC",
                postalCode = "67890",
                country = "US",
                residential = true
            },
            weight = new OrderWeight
            {
                value = 3.0m,
                units = Units.pounds
            },
            dimensions = new OrderDimensions
            {
                length = 18,
                width = 32,
                height = 1,
                units = OrderDimensions.Units.inches
            },
            labelData = "data:application/pdf;base64,JVBERi0xLjQ",
            testLabel = false,
            ShippingAccountId = null,
            IsExpedited = false,
            ShipEngineShipmentId = "se-1679034854"
        }
    ];
}