using System.Globalization;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Invoices;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Controllers;

//[CwaFeatureGate(CwaFeatures.INVOICES)]
[AutoValidateAntiforgeryToken]
public class InvoiceController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<InvoiceController> _logger;
    public InvoiceController(IInvoiceService invoiceService, IServiceScopeFactory serviceScopeFactory, ILogger<InvoiceController> logger)
    {
        _invoiceService = invoiceService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DHLInvoiceImport(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("csvFile", "Please upload a valid CSV file.");
            return View();
        }

        var userName = User.Identity.Name;
        var tempFilePath = Path.GetTempFileName();
        await using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await csvFile.CopyToAsync(stream);
        }

        ProcessInvoiceFile(tempFilePath, csvFile, Carrier.DHL, userName, false);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UPSInvoiceImport(IFormFile xlsFile)
    {
        if (xlsFile == null || xlsFile.Length == 0)
        {
            ModelState.AddModelError("xlsFile", "Please upload a valid Excel file.");
            return View();
        }
        var userName = User.Identity.Name;
        var tempFilePath = Path.GetTempFileName();
        await using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await xlsFile.CopyToAsync(stream);
        }

        ProcessInvoiceFile(tempFilePath, xlsFile, Carrier.UPS, userName, false);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> StampsUSPSInvoiceImport(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("csvFile", "Please upload a valid CSV file.");
            return View();
        }
        var userName = User.Identity.Name;
        var tempFilePath = Path.GetTempFileName();
        await using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await csvFile.CopyToAsync(stream);
        }

        ProcessInvoiceFile(tempFilePath, csvFile, Carrier.StampsUSPS, userName, false);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> EasyPostInvoiceImport(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("csvFile", "Please upload a valid CSV file.");
            return View();
        }

        var userName = User.Identity.Name;
        var tempFilePath = Path.GetTempFileName();
        await using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await csvFile.CopyToAsync(stream);
        }

        ProcessInvoiceFile(tempFilePath, csvFile, Carrier.EasyPost, userName, false);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> SkulabsImport(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("csvFile", "Please upload a valid CSV file.");
            return View();
        }
        var userName = User.Identity.Name;
        var tempFilePath = Path.GetTempFileName();
        await using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await csvFile.CopyToAsync(stream);
        }

        ProcessInvoiceFile(tempFilePath, csvFile, default, userName, true);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices(
        int draw,
        int start,
        int length,
        string carrierType,
        string uploadDateStart,
        string uploadDateEnd,
        string fileName,
        string uploadedBy,
        string orderNumber,
        string trackingCode,
        int invoiceColumn = 0,
        string invoiceDir = "asc"
    )
    {
        if (start < 0 || length <= 0)
        {
            return Json(new { draw, recordsTotal = 0, recordsFiltered = 0, data = new List<object>() });
        }

        try
        {
            var (invoices, totalCount) = await _invoiceService.GetInvoicesAsync(
                start,
                length,
                carrierType,
                uploadDateStart,
                uploadDateEnd,
                fileName,
                uploadedBy,
                orderNumber,
                trackingCode,
                GetSortColumn(invoiceColumn),
                invoiceDir
            );

            var data = invoices.Select(i => new
            {
                i.CarrierType,
                FormattedUploadDate = i.UploadDate.ToString("yyyy-MM-dd"),
                i.FileName,
                i.UploadedBy,
                i.FileUrl,
                i.TotalCost
            }).ToList();

            return Json(new { draw, recordsTotal = totalCount, recordsFiltered = totalCount, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching invoices.");
            return BadRequest(ex.ToString());
        }
    }

    private string GetSortColumn(int invoiceColumn)
    {
        return invoiceColumn switch
        {
            1 => "CarrierType",
            2 => "UploadDate",
            3 => "FileName",
            4 => "UploadedBy",
            _ => "UploadDate",
        };
    }

    private void ProcessInvoiceFile(string tempFilePath, IFormFile file, Carrier carrier, string userName, bool isSkulabs)
    {
        Task.Run(() =>
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                try
                {
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var tempFile = new FormFile(fileStream, 0, file.Length, file.Name, file.FileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = file.ContentType
                        };

                        if (!isSkulabs)
                        {
                            switch (carrier)
                            {
                                case Carrier.UPS:
                                    invoiceService.ParseUPSInvoicesExcelFileAsync(tempFile, userName).GetAwaiter().GetResult();
                                    break;
                                case Carrier.StampsUSPS:
                                    invoiceService.ParseStampsUSPSInvoicesCsvFileAsync(tempFile, userName).GetAwaiter().GetResult();
                                    break;
                                case Carrier.EasyPost:
                                    invoiceService.ParseEasyPostInvoicesCsvFileAsync(tempFile, userName).GetAwaiter().GetResult();
                                    break;
                                case Carrier.DHL:
                                    invoiceService.ParseDHLInvoicesCsvFileAsync(tempFile, userName).GetAwaiter().GetResult();
                                    break;
                            }
                        }
                        else
                        {
                            invoiceService.ParseSkulabsImportsCsvFileAsync(tempFile, userName).GetAwaiter().GetResult();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the invoice.");
                    // I suppose once we have the notification system implemented and working, this can lead to that.
                }
                finally
                {
                    // Clean up the temporary file, otherwise it'll start clogging up temp folders.  
                    System.IO.File.Delete(tempFilePath);
                }
            }
        });
    }
}
