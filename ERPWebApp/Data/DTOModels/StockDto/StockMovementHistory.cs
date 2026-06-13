using ERPWebApp.Models;

namespace ERPWebApp.Data.DTOModels.StockDto;

public record StockMovementHistory(
    string PerformedBy,
    DateTime PerformedOn,
    string Sku,
    string MovementType,
    string FromLocation,
    int FromLocationRunningBalance,
    string ToLocation,
    int ToLocationRunningBalance,
    int ActionQuantity,
    string Action
);