namespace ERPWebApp.Models.Reports;

public sealed record WeeklyProfit
(
    int ProductId,
    decimal Profits,
    decimal ItemsSold,
    string ProductName
);

