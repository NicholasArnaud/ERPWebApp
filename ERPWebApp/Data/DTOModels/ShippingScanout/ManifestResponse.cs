namespace ERPWebApp.Data.DTOModels.ShippingScanout;

public record ManifestResponse(
   string ManifestId,
   string CarrierId,
   string CarrierName,
   int Shipments,
   DateTime ShipmentDate,
   DateTime ClosedDate,
   string ManifestFile
);