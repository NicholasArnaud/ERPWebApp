using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Extensions;

public static class WebhooksExtensions
{
    public static string GenerateUspsValidationQueryString(this Order.OrderShippingInfo shippingInfo)
    {
        if (shippingInfo == null)
        {
            throw new ArgumentNullException(nameof(shippingInfo), "Shipping information cannot be null.");
        }

        var queryParams = new List<string>();
        var (zipCode, zipPlus4) = GetZipCodes(shippingInfo.postalCode);

        if (!string.IsNullOrEmpty(shippingInfo.company))
            queryParams.Add($"firm={Uri.EscapeDataString(shippingInfo.company)}");
        if (!string.IsNullOrEmpty(shippingInfo.street1))
            queryParams.Add($"streetAddress={Uri.EscapeDataString(shippingInfo.street1)}");
        if (!string.IsNullOrEmpty(shippingInfo.street2))
            queryParams.Add($"secondaryAddress={Uri.EscapeDataString(shippingInfo.street2)}");
        if (!string.IsNullOrEmpty(shippingInfo.city))
            queryParams.Add($"city={Uri.EscapeDataString(shippingInfo.city)}");
        if (!string.IsNullOrEmpty(shippingInfo.state))
            queryParams.Add($"state={Uri.EscapeDataString(shippingInfo.state)}");
        if (!string.IsNullOrEmpty(zipCode))
            queryParams.Add($"ZIPCode={Uri.EscapeDataString(zipCode)}");
        if (!string.IsNullOrEmpty(zipPlus4))
            queryParams.Add($"ZIPPlus4={Uri.EscapeDataString(zipPlus4)}");

        return string.Join("&", queryParams);
    }
    
    public static string GenerateUspsValidationQueryString(this ShipEngineWarehouseAddress address)
    {
        var queryParams = new List<string>();
        var (zipCode, zipPlus4) = GetZipCodes(address.PostalCode);

        if (!string.IsNullOrEmpty(address.Name))
            queryParams.Add($"firm={Uri.EscapeDataString(address.Name)}");
        if (!string.IsNullOrEmpty(address.AddressLine1))
            queryParams.Add($"streetAddress={Uri.EscapeDataString(address.AddressLine1)}");
        if (!string.IsNullOrEmpty(address.AddressLine2))
            queryParams.Add($"secondaryAddress={Uri.EscapeDataString(address.AddressLine2)}");
        if (!string.IsNullOrEmpty(address.City))
            queryParams.Add($"city={Uri.EscapeDataString(address.City)}");
        if (!string.IsNullOrEmpty(address.State))
            queryParams.Add($"state={Uri.EscapeDataString(address.State)}");
        if (!string.IsNullOrEmpty(zipCode))
            queryParams.Add($"ZIPCode={Uri.EscapeDataString(zipCode)}");
        if (!string.IsNullOrEmpty(zipPlus4))
            queryParams.Add($"ZIPPlus4={Uri.EscapeDataString(zipPlus4)}");

        return string.Join("&", queryParams);
    }
    
    public static UspsAddress ConvertToUspsAddress(this ShipEngineWarehouseAddress address)
    {
        var (zipCode, zipPlus4) = GetZipCodes(address.PostalCode);

        return new UspsAddress(
            address.AddressLine1,
            "",
            address.AddressLine2,
            "",
            address.City,
            address.State,
            "",
            zipCode,
            zipPlus4
        );
    }

    private static (string zipCode, string zipPlus4) GetZipCodes(string postalCode)
    {
        var postalCodes = (postalCode ?? "").Split('-');
        return (postalCodes.Length > 0 ? postalCodes[0] : "", postalCodes.Length > 1 ? postalCodes[1] : "");
    }



    public static string Validate(this UspsAddressValidationResponse address)
    {
        if (address is null)
            return "The Shipping Address was not found. \nProceed with shipping anyway?";

        var additionalInfo = address.AdditionalInfo;

        var dpvResults = new Dictionary<string, string>
        {
            {
                "N",
                "Both primary and (if present) secondary number information failed to DPV confirm. \nProceed with shipping anyway?"
            },
            {
                "D",
                "The Shipping Address was DPV confirmed for the primary number only, and the secondary number information was missing. \nProceed with shipping anyway?"
            },
            {
                "S",
                "The Shipping Address was DPV confirmed for the primary number only, and the secondary number information was present but not confirmed. \nProceed with shipping anyway?"
            }
        };

        return dpvResults.TryGetValue(additionalInfo.DpvConfirmation, out var message) ? message : "";
    }

}