using ERPWebApp.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.DTOModels;

public class OrderViewModel
{
    public string OrderNumber { get; set; }
    public OrderStatus[] OrderStatus { get; set; }
    public int StoreId { get; set; }
    public int[] DepartmentIds { get; set; }
    public int[] OrderTagId { get; set; }
    public int? OrderBatchId { get; set; }
    public string OrderStartDate { get; set; }
    public string OrderEndDate { get; set; }
    public string ShipByDate { get; set; }
    public int[] TagIds { get; set; }
    public int[] ProductIds { get; set; }
    public bool AutoEnterDetails { get; set; } = true;
    public bool IncludeBatchedOrders { get; set; } = true;
    public string ItemName { get; set; }
    public List<string> ExcludeItemNames { get; set; }
    public SelectList StoreNames { get; set; }
    public SelectList Products { get; set; }
    public SelectList Departments { get; set; }
    public SelectList Tags { get; set; }
    public SelectList OrderBatches { get; set; }
    public SelectList OrderBatchesForOrderAddition { get; set; }
    public Order Order { get; set; }
    public List<AvailableShipmentCarrier> AvailableShipmentCarriers { get; set; }
}
public record AvailableShipmentCarrier
{
    public string CarrierCode { get; init; }
    public string ServiceCode { get; init; }
    public decimal TotalShipmentEstimate { get; init; }

}