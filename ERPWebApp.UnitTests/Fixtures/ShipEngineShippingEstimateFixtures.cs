using static ERPWebApp.Data.ShipEngineShippingEstimate;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class ShipEngineShippingEstimateFixtures
    {
        public static List<ShipEngineShippingEstimate> GetTestShipEngineShippingEstimate() => [
                new ShipEngineShippingEstimate
                    {
                        carrier_id = "se-4237417",
                        carrier_code = "UPS",
                        carrier_nickname = "UPS",
                        service_code = "ups_ground",
                        service_type = "Ground",
                        shipping_amount = new Amount { amount = 10.00m, currency = "USD" },
                        delivery_days = 3,
                        guaranteed_service = false,
                        estimated_delivery_date = null,
                        warning_messages = null
                    },
                    new ShipEngineShippingEstimate
                    {
                        carrier_id = "se-67890",
                        carrier_code = "USPS",
                        carrier_nickname = "USPS",
                        service_code = "usps_priority",
                        service_type = "Priority Mail",
                        shipping_amount = new Amount { amount = 12.00m, currency = "USD" },
                        delivery_days = 2,
                        guaranteed_service = false,
                        estimated_delivery_date = null,
                        warning_messages = null
                    }
        ];
    }
}