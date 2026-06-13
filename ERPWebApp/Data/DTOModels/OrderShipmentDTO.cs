namespace ERPWebApp.Data.DTOModels;

public class OrderShipmentDTO
{

}
public record OrderShipmentsByServiceDTO
{
    public string ServiceCode { get; init; }
    public string CarrierCode { get; init; }
    public int TotalShipments { get; init; }
}
