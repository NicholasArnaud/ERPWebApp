namespace ERPWebApp.Models.Reports
{
    public sealed record YearlyProductShippedReport
    (
        int ProductId,
        string Sku,
        string Year,
        string Label,
        decimal Quantity
    );
}
