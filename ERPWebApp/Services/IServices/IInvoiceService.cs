using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Services.IServices
{
    public interface IInvoiceService : IService<InvoicedOrders>
    {
        Task<List<StampsUSPSInvoices>> ParseStampsUSPSInvoicesCsvFileAsync(IFormFile csvFile, string User);
        Task<List<UPSInvoices>> ParseUPSInvoicesExcelFileAsync(IFormFile excelFile, string User);
        Task<List<DHLInvoices>> ParseDHLInvoicesCsvFileAsync(IFormFile csvFile, string User);
        Task<List<EasyPostInvoices>> ParseEasyPostInvoicesCsvFileAsync(IFormFile csvFile, string User);
        Task<List<SkulabsImport>> ParseSkulabsImportsCsvFileAsync(IFormFile csvFile, string user);
        Task<(List<InvoiceViewModel>, int)> GetInvoicesAsync(
            int start,
            int length,
            string carrierType,
            string uploadDateStart,
            string uploadDateEnd,
            string fileName,
            string uploadedBy,
            string orderNumber,
            string trackingCode,
            string invoiceColumn,
            string invoiceDir = "asc"
        );
    }
}
