namespace ERPWebApp.Data.DTOModels.ShippingScanout.USPS;

public record UspsManifestResponse(bool Success, string Message, Dictionary<string, string> Manifests);