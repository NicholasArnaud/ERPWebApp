using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Invoices;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Services.IServices;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic.FileIO;
using ERPWebApp.Extensions;
using System.Reflection;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using ERPWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Services
{
    public class InvoiceService(IUnitOfWork unitOfWork, IFilesService fileService, ILogger<InvoiceService> logger) : Service<InvoicedOrders>(unitOfWork), IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IFilesService _fileService = fileService;
        private readonly ILogger<InvoiceService> _logger = logger;

        public async Task<List<DHLInvoices>> ParseDHLInvoicesCsvFileAsync(IFormFile csvFile, string User)
        {
            if (await _unitOfWork.AzureBlobStorage.FileExistsAsync(csvFile.FileName, FileType.DHLInvoice))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            var fileUrl = await _fileService.UploadToAzureAsync(csvFile, FileType.DHLInvoice);

            if (await FileAlreadyImportedAsync(csvFile.FileName, CarrierType.DHL))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            // Fetch related data from orderFulfillments and orderShipments  
            var orderFulfillments = await _unitOfWork.OrderFulfillments.GetListByQueryAsync(of => of
                .Select(of => new OrderFulfillmentInvoiceInfo
                {
                    trackingNumber = of.trackingNumber,
                    ERPOrderId = of.ERPOrderId,
                    OrderNumber = of.Order.orderNumber
                }));

            var orderShipments = await _unitOfWork.OrderShipments.GetListByQueryAsync(os => os
                .Select(os => new OrderShipmentInvoiceInfo
                {
                    trackingNumber = os.trackingNumber,
                    ERPOrderId = os.ERPOrderId,
                    OrderNumber = os.Order.orderNumber
                }));

            var invoices = new List<DHLInvoices>();
            var genericInvoices = new List<InvoicedOrders>();
            var properties = typeof(DHLInvoices).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                .Where(p => p.Name != "DHLInvoiceId" && p.Name != "fileName" && p.Name != "FileUrl" && p.Name != "ImportDate" && p.Name != "ImportedBy" && p.Name != "GeneralInvoiceId" && p.Name != "GeneralInvoice")
                                                .ToList();
            using (var stream = new StreamReader(csvFile.OpenReadStream()))
            using (var parser = new TextFieldParser(stream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Skip the first line, since it's a header with very limiting information and not useful for our purposes.  
                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    var values = parser.ReadFields();
                    if (values == null) continue;

                    try
                    {
                        var invoice = new DHLInvoices();
                        for (int i = 0; i < properties.Count; i++)
                        {
                            var property = properties[i];
                            var value = values[i];

                            if (!string.IsNullOrEmpty(value))
                            {
                                if (property.Name == nameof(DHLInvoices.InternalTrackingNum))
                                {
                                    value = value.Trim('\'');
                                }
                                if (property.Name == nameof(DHLInvoices.PUDATE))
                                {
                                    property.SetValue(invoice, ParseDate(value));
                                }
                                if (property.PropertyType == typeof(int?) && int.TryParse(value, out int intValue))
                                {
                                    property.SetValue(invoice, intValue);
                                }
                                else if (property.PropertyType == typeof(long?) && long.TryParse(value, out long longValue))
                                {
                                    property.SetValue(invoice, longValue);
                                }
                                else if (property.PropertyType == typeof(decimal?) && decimal.TryParse(value, out decimal decimalValue))
                                {
                                    property.SetValue(invoice, decimalValue);
                                }
                                else if (property.PropertyType == typeof(DateTime?) && DateTime.TryParse(value, out DateTime dateValue))
                                {
                                    property.SetValue(invoice, dateValue);
                                }
                                else if (property.PropertyType == typeof(string))
                                {
                                    property.SetValue(invoice, value);
                                }
                            }
                        }
                        invoice.fileName = csvFile.FileName;
                        invoice.FileUrl = fileUrl;
                        invoice.ImportedBy = User;
                        invoice.ImportDate = DateTime.Now;
                        invoices.Add(invoice);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Console.WriteLine($"IndexOutOfRangeException: {ex.Message}");
                        for (int i = 0; i < values.Length; i++)
                        {
                            Console.WriteLine($"Header: {properties[i]}, Value: {values[i]}");
                        }
                        // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                        var uri = new Uri(fileUrl);
                        var fileName = Path.GetFileName(uri.LocalPath);

                        // Remove the file from blob storage  
                        await _fileService.RemoveAzureBlobAsync(fileName, FileType.DHLInvoice);

                        throw;
                    }
                }
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.DHLInvoices.AddRangeAsync(invoices);
                await _unitOfWork.SaveChangesAsync();

                foreach (var invoice in invoices)
                {
                    string orderNumber = null;
                    string trackingNumber = null;
                    InvoiceType invoiceType = InvoiceType.Normal;

                    if (invoice.BillRef2 != null && (invoice.BillRef2.StartsWith("SKULabs") || invoice.BillRef2.StartsWith("ML")) ||
                        invoice.CustRef != null && (invoice.CustRef.StartsWith("SKULabs") || invoice.CustRef.StartsWith("ML")) ||
                        invoice.BillRef2 == "NONE")
                    {
                        invoiceType = InvoiceType.Skulabs;
                        orderNumber = orderNumber ?? invoice.BillRef2 ?? invoice.CustRef;
                        trackingNumber = invoice.DeliveryConfirm;

                        InvoicedOrders genericInvoice = ProcessInvoices(orderNumber, trackingNumber, Carrier.DHL, orderShipments, orderFulfillments, invoice.PUDATE, invoice.DHLInvoiceId);
                        genericInvoices.Add(genericInvoice);
                    }
                    else if (invoice.Country != "US" && invoice.Country != null)
                    {
                        invoiceType = InvoiceType.International;
                        orderNumber = invoice.CustRef;
                        trackingNumber = invoice.CustomerConfirm;

                        InvoicedOrders genericInvoice = ProcessInvoices(orderNumber, trackingNumber, Carrier.DHL, orderShipments, orderFulfillments, invoice.PUDATE, invoice.DHLInvoiceId);
                        genericInvoices.Add(genericInvoice);
                    }
                    else
                    {
                        orderNumber = invoice.BillRef2;
                        trackingNumber = invoice.DeliveryConfirm;

                        InvoicedOrders genericInvoice = ProcessInvoices(orderNumber, trackingNumber, Carrier.DHL, orderShipments, orderFulfillments, invoice.PUDATE, invoice.DHLInvoiceId);
                        genericInvoices.Add(genericInvoice);
                    }
                }

                await _unitOfWork.InvoicedOrders.AddRangeAsync(genericInvoices);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();

                // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                // Remove the file from blob storage  
                await _fileService.RemoveAzureBlobAsync(fileName, FileType.DHLInvoice);
                throw;
            }

            return invoices;
        }

        public async Task<List<StampsUSPSInvoices>> ParseStampsUSPSInvoicesCsvFileAsync(IFormFile csvFile, string User)
        {
            if (await _unitOfWork.AzureBlobStorage.FileExistsAsync(csvFile.FileName, FileType.StampsUSPSInvoice))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            var fileUrl = await _fileService.UploadToAzureAsync(csvFile, FileType.StampsUSPSInvoice);

            if (await FileAlreadyImportedAsync(csvFile.FileName, CarrierType.StampsUSPS))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            // Fetch related data from orderFulfillments and orderShipments  
            var orderFulfillments = await _unitOfWork.OrderFulfillments.GetListByQueryAsync(of => of
                .Select(of => new OrderFulfillmentInvoiceInfo
                {
                    trackingNumber = of.trackingNumber,
                    ERPOrderId = of.ERPOrderId,
                    OrderNumber = of.Order.orderNumber
                }));

            var orderShipments = await _unitOfWork.OrderShipments.GetListByQueryAsync(os => os
                .Select(os => new OrderShipmentInvoiceInfo
                {
                    trackingNumber = os.trackingNumber,
                    ERPOrderId = os.ERPOrderId,
                    OrderNumber = os.Order.orderNumber
                }));

            var invoices = new List<StampsUSPSInvoices>();
            var genericInvoices = new List<InvoicedOrders>();

            using (var stream = new StreamReader(csvFile.OpenReadStream()))
            using (var parser = new TextFieldParser(stream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                var headers = parser.ReadFields();
                if (headers == null)
                {
                    throw new Exception("The CSV file does not contain headers.");
                }

                while (!parser.EndOfData)
                {
                    var values = parser.ReadFields();
                    if (values == null) continue;

                    try
                    {
                        var invoice = new StampsUSPSInvoices
                        {
                            DatePrinted = ParseDate(values[Array.IndexOf(headers, "Date Printed")]),
                            AmountPaid = ParseDecimal(values[Array.IndexOf(headers, "Amount Paid")]),
                            AdjustedAmount = ParseDecimal(values[Array.IndexOf(headers, "Adjusted Amount")]),
                            QuotedAmount = ParseDecimal(values[Array.IndexOf(headers, "Quoted Amount")]),
                            PaymentType = ParseEnum<PaymentType>(values[Array.IndexOf(headers, "Payment Type")]),
                            Shipment = ParseEnum<ShipmentStatus>(values[Array.IndexOf(headers, "Shipment Status")]),
                            TrackingNumber = NormalizeString(values[Array.IndexOf(headers, "Tracking #")]),
                            DateDelivered = ParseDate(values[Array.IndexOf(headers, "Date Delivered")]),
                            Recipient = NormalizeString(values[Array.IndexOf(headers, "Recipient")]),
                            Name = NormalizeString(values[Array.IndexOf(headers, "Name")]),
                            Address1 = NormalizeString(values[Array.IndexOf(headers, "Address 1")]),
                            Address2 = NormalizeString(values[Array.IndexOf(headers, "Address 2")]),
                            Address3 = NormalizeString(values[Array.IndexOf(headers, "Address 3")]),
                            City = NormalizeString(values[Array.IndexOf(headers, "City")]),
                            StateOrProvince = NormalizeString(values[Array.IndexOf(headers, "State/Province")]),
                            PostalCode = NormalizeString(values[Array.IndexOf(headers, "Postal Code")]),
                            Country = NormalizeString(values[Array.IndexOf(headers, "Country")]),
                            OriginZip = NormalizeString(values[Array.IndexOf(headers, "Origin Zip")]),
                            Weight = NormalizeString(values[Array.IndexOf(headers, "Weight")]),
                            Carrier = NormalizeString(values[Array.IndexOf(headers, "Carrier")]),
                            Service = NormalizeString(values[Array.IndexOf(headers, "Service")]),
                            TrackingConfirmation = NormalizeString(values[Array.IndexOf(headers, "Tracking Confirmation")]),
                            ExtraService = NormalizeString(values[Array.IndexOf(headers, "Extra Services")]),
                            InsuredFor = ParseDecimal(values[Array.IndexOf(headers, "Insured For")]),
                            ShipDate = ParseDate(values[Array.IndexOf(headers, "Ship Date")]),
                            CostCode = NormalizeString(values[Array.IndexOf(headers, "Cost Code")]),
                            PrintedMessage = NormalizeString(values[Array.IndexOf(headers, "Printed Message")]),
                            User = NormalizeString(values[Array.IndexOf(headers, "User")]),
                            RefundType = NormalizeString(values[Array.IndexOf(headers, "Refund Type")]),
                            RefundRequestDate = ParseDate(values[Array.IndexOf(headers, "Refund Request Date")]),
                            RefundStatus = ParseEnum<RefundStatus>(values[Array.IndexOf(headers, "Refund Status")]),
                            RefundRequested = ParseDecimal(values[Array.IndexOf(headers, "Refund Requested")]),
                            Reference1 = NormalizeString(values[Array.IndexOf(headers, "Reference 1")]),
                            OrderID = NormalizeString(values[Array.IndexOf(headers, "Order ID")]),
                            Store = NormalizeString(values[Array.IndexOf(headers, "Store")]),
                            OrderDate = ParseDate(values[Array.IndexOf(headers, "Order Date")]),
                            OrderTotal = ParseDecimal(values[Array.IndexOf(headers, "Order Total")]),
                            ItemSKUs = NormalizeString(values[Array.IndexOf(headers, "Item SKUs")]),
                            Items = NormalizeString(values[Array.IndexOf(headers, "Items")]),
                            ProductTotal = ParseDecimal(values[Array.IndexOf(headers, "Product Total")]),
                            ShippingPaid = NormalizeString(values[Array.IndexOf(headers, "Shipping Paid")]),
                            TaxPaid = NormalizeString(values[Array.IndexOf(headers, "Tax Paid")]),
                            InsuranceProvider = NormalizeString(values[Array.IndexOf(headers, "Insurance Provider")]),
                            DutiesTaxesAmount = ParseDecimal(values[Array.IndexOf(headers, "Duties and Taxes Amount")])
                        };

                        // Need to do this since some entries only have the order number in the first line of the printed message.
                        invoice.orderNumber = ExtractStampsOrderNumber(invoice.PrintedMessage);
                        invoice.fileName = csvFile.FileName;
                        invoice.FileUrl = fileUrl;
                        invoice.ImportedBy = User;
                        invoice.ImportDate = DateTime.Now;
                        invoices.Add(invoice);


                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Console.WriteLine($"IndexOutOfRangeException: {ex.Message}");
                        for (int i = 0; i < headers.Length; i++)
                        {
                            Console.WriteLine($"Header: {headers[i]}, Value: {values[i]}");
                        }
                        // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                        var uri = new Uri(fileUrl);
                        var fileName = Path.GetFileName(uri.LocalPath);

                        // Remove the file from blob storage  
                        await _fileService.RemoveAzureBlobAsync(fileName, FileType.StampsUSPSInvoice);
                        throw;
                    }
                }
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.StampsUSPSInvoices.AddRangeAsync(invoices);
                await _unitOfWork.SaveChangesAsync();

                foreach (var invoice in invoices)
                {
                    string orderNumber = invoice.orderNumber;
                    string trackingNumber = invoice.TrackingNumber;

                    InvoicedOrders genericInvoice = ProcessInvoices(orderNumber, trackingNumber, Carrier.StampsUSPS, orderShipments, orderFulfillments, invoice.DatePrinted, invoice.StampsUSPSInvoiceId);
                    genericInvoices.Add(genericInvoice);
                }

                await _unitOfWork.InvoicedOrders.AddRangeAsync(genericInvoices);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();

                // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                // Remove the file from blob storage  
                await _fileService.RemoveAzureBlobAsync(fileName, FileType.StampsUSPSInvoice);
                throw;
            }

            return invoices;
        }

        public async Task<List<UPSInvoices>> ParseUPSInvoicesExcelFileAsync(IFormFile excelFile, string User)
        {
            if (await _unitOfWork.AzureBlobStorage.FileExistsAsync(excelFile.FileName, FileType.UPSInvoice))
            {
                throw new Exception($"The file '{excelFile.FileName}' has already been imported.");
            }

            var fileUrl = await _fileService.UploadToAzureAsync(excelFile, FileType.UPSInvoice);

            if (await FileAlreadyImportedAsync(excelFile.FileName, CarrierType.UPS))
            {
                throw new Exception($"The file '{excelFile.FileName}' has already been imported.");
            }

            // Fetch related data from orderFulfillments and orderShipments  
            var orderFulfillments = await _unitOfWork.OrderFulfillments.GetListByQueryAsync(of => of
                .Select(of => new OrderFulfillmentInvoiceInfo
                {
                    trackingNumber = of.trackingNumber,
                    ERPOrderId = of.ERPOrderId,
                    OrderNumber = of.Order.orderNumber
                }));

            var orderShipments = await _unitOfWork.OrderShipments.GetListByQueryAsync(os => os
                .Select(os => new OrderShipmentInvoiceInfo
                {
                    trackingNumber = os.trackingNumber,
                    ERPOrderId = os.ERPOrderId,
                    OrderNumber = os.Order.orderNumber
                }));

            var invoices = new List<UPSInvoices>();
            var genericInvoices = new List<InvoicedOrders>();
            var allowedProperties = GetImportProperties().Select(x => x.Name).ToList();

            if (excelFile == null || excelFile.Length == 0)
            {
                throw new ArgumentException("The uploaded file is empty or null.");
            }

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;
                IWorkbook workbook;
                try
                {
                    if (excelFile.FileName.EndsWith(".xls"))
                    {
                        workbook = new HSSFWorkbook(stream);
                    }
                    else if (excelFile.FileName.EndsWith(".xlsx"))
                    {
                        workbook = new XSSFWorkbook(stream);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid file format. Please upload an Excel file with .xls or .xlsx extension.");
                    }

                    var sheet = workbook.GetSheetAt(0); // Going to assume the data is in the first sheet. Can add stuff for more sheets if that becomes a necessity.
                    var headerRow = sheet.GetRow(0);

                    var headers = new Dictionary<string, int>();
                    for (int i = 0; i < headerRow.LastCellNum; i++)
                    {
                        var cellValue = headerRow.GetCell(i)?.ToString().Trim();
                        headers[cellValue] = i;
                    }

                    for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++) // Skipping header row
                    {
                        var row = sheet.GetRow(rowIndex);
                        if (row == null) continue;

                        var values = headers.ToDictionary(
                            header => header.Key,
                            header => row.GetCell(header.Value)?.ToString());

                        var invoice = new UPSInvoices
                        {
                            CustomerNumber = NormalizeString(values.GetValueOrDefault("Customer #")),
                            InvoiceNumber = NormalizeString(values.GetValueOrDefault("Invoice #")),
                            LineOfBusiness = NormalizeString(values.GetValueOrDefault("Line of Business")),
                            AirbillNumber = NormalizeString(values.GetValueOrDefault("Airbill #")),
                            ShipDate = ParseDate(values.GetValueOrDefault("Ship date")),
                            ProNumber = NormalizeString(values.GetValueOrDefault("PRO #")),
                            BolNumber = NormalizeString(values.GetValueOrDefault("BOL #")),
                            Scac = NormalizeString(values.GetValueOrDefault("SCAC")),
                            BillType = NormalizeString(values.GetValueOrDefault("Bill Type")),
                            ShippersName = NormalizeString(values.GetValueOrDefault("Shippers Name")),
                            ShippersAddress1 = NormalizeString(values.GetValueOrDefault("Shippers Address 1")),
                            ShippersAddress2 = NormalizeString(values.GetValueOrDefault("Shippers Address 2")),
                            ShippersAddress3 = NormalizeString(values.GetValueOrDefault("Shippers Address 3")),
                            ShippersCity = NormalizeString(values.GetValueOrDefault("Shippers City")),
                            ShippersState = NormalizeString(values.GetValueOrDefault("Shippers State")),
                            ShippersZip = NormalizeString(values.GetValueOrDefault("Shippers ZIP")),
                            ReceiverName = NormalizeString(values.GetValueOrDefault("Receiver Name")),
                            ReceiverAddress1 = NormalizeString(values.GetValueOrDefault("Receiver Address 1")),
                            ReceiverAddress2 = NormalizeString(values.GetValueOrDefault("Receiver Address 2")),
                            ReceiverAddress3 = NormalizeString(values.GetValueOrDefault("Receiver Address 3")),
                            ReceiverCity = NormalizeString(values.GetValueOrDefault("Receiver City")),
                            ReceiverState = NormalizeString(values.GetValueOrDefault("Receiver State")),
                            ReceiverZip = NormalizeString(values.GetValueOrDefault("Receiver ZIP")),
                            ConsigneeName = NormalizeString(values.GetValueOrDefault("Consignee Name")),
                            ConsigneeCity = NormalizeString(values.GetValueOrDefault("Consignee City")),
                            ConsigneeState = NormalizeString(values.GetValueOrDefault("Consignee State")),
                            ConsigneeZip = NormalizeString(values.GetValueOrDefault("Consignee Zip")),
                            OriginatingCustomer = NormalizeString(values.GetValueOrDefault("Originating Customer")),
                            CustomerName = NormalizeString(values.GetValueOrDefault("Customer Name")),
                            CustomerAddress1 = NormalizeString(values.GetValueOrDefault("Customer Address 1")),
                            CustomerAddress2 = NormalizeString(values.GetValueOrDefault("Customer Address 2")),
                            CustomerCity = NormalizeString(values.GetValueOrDefault("Customer City")),
                            CustomerState = NormalizeString(values.GetValueOrDefault("Customer State")),
                            CustomerZip = ParseInt(values.GetValueOrDefault("Customer ZIP")),
                            HandlingUnit = ParseInt(values.GetValueOrDefault("Handling Unit")),
                            Pieces = NormalizeString(values.GetValueOrDefault("Pieces")),
                            OriginalWeight = NormalizeString(values.GetValueOrDefault("Original Weight")),
                            ChargedWeight = NormalizeString(values.GetValueOrDefault("Charged Weight")),
                            Class = ParseInt(values.GetValueOrDefault("Class")),
                            ChargeType1 = NormalizeString(values.GetValueOrDefault("Charge Type 1")),
                            ChargeAmount1 = ParseDecimal(values.GetValueOrDefault("Charge Amount 1")),
                            ChargeType2 = NormalizeString(values.GetValueOrDefault("Charge Type 2")),
                            ChargeAmount2 = ParseDecimal(values.GetValueOrDefault("Charge Amount 2")),
                            ChargeType3 = NormalizeString(values.GetValueOrDefault("Charge Type 3")),
                            ChargeAmount3 = ParseDecimal(values.GetValueOrDefault("Charge Amount 3")),
                            ChargeType4 = NormalizeString(values.GetValueOrDefault("Charge Type 4")),
                            ChargeAmount4 = ParseDecimal(values.GetValueOrDefault("Charge Amount 4")),
                            ChargeType5 = NormalizeString(values.GetValueOrDefault("Charge Type 5")),
                            ChargeAmount5 = ParseDecimal(values.GetValueOrDefault("Charge Amount 5")),
                            ChargeType6 = NormalizeString(values.GetValueOrDefault("Charge Type 6")),
                            ChargeAmount6 = ParseDecimal(values.GetValueOrDefault("Charge Amount 6")),
                            ChargeType7 = NormalizeString(values.GetValueOrDefault("Charge Type 7")),
                            ChargeAmount7 = ParseDecimal(values.GetValueOrDefault("Charge Amount 7")),
                            ChargeType8 = NormalizeString(values.GetValueOrDefault("Charge Type 8")),
                            ChargeAmount8 = ParseDecimal(values.GetValueOrDefault("Charge Amount 8")),
                            ChargeTotal = ParseDecimal(values.GetValueOrDefault("Charge Total")),
                            InvoiceDate = ParseDate(values.GetValueOrDefault("Invoice Date")),
                            BillingReference1 = NormalizeString(values.GetValueOrDefault("Billing Reference 1")),
                            BillingReference2 = NormalizeString(values.GetValueOrDefault("Billing Reference 2")),
                            VendorReference1 = NormalizeString(values.GetValueOrDefault("Vendor Reference 1")),
                            VendorReference2 = NormalizeString(values.GetValueOrDefault("Vendor Reference 2")),
                            SentBy = NormalizeString(values.GetValueOrDefault("Sent By")),
                            ServiceLevel = NormalizeString(values.GetValueOrDefault("Service level")),
                            Zone = ParseInt(values.GetValueOrDefault("Zone")),
                            YouOweAs = ParseDecimal(values.GetValueOrDefault("You Owe As")),
                            Description1 = NormalizeString(values.GetValueOrDefault("Description1")),
                            Description2 = NormalizeString(values.GetValueOrDefault("Description2")),
                            Description3 = NormalizeString(values.GetValueOrDefault("Description3")),
                            Description4 = NormalizeString(values.GetValueOrDefault("Description4")),
                            PickupLocation = NormalizeString(values.GetValueOrDefault("PickupLocation")),
                            SenderNo = NormalizeString(values.GetValueOrDefault("SenderNo")),
                            ReceiverNo = NormalizeString(values.GetValueOrDefault("ReceiverNo")),
                            ReceiverLine1 = NormalizeString(values.GetValueOrDefault("ReceiverLine1")),
                            ReceiverLine2 = NormalizeString(values.GetValueOrDefault("ReceiverLine2")),
                            PackageReference1 = NormalizeString(values.GetValueOrDefault("Package Reference 1")),
                            PackageReference2 = NormalizeString(values.GetValueOrDefault("Package Reference 2")),
                            PackageReference3 = NormalizeString(values.GetValueOrDefault("Package Reference 3")),
                            PackageReference4 = NormalizeString(values.GetValueOrDefault("Package Reference 4")),
                            PackageReference5 = NormalizeString(values.GetValueOrDefault("Package Reference 5")),
                            PackageReference6 = NormalizeString(values.GetValueOrDefault("Package Reference 6")),
                            PackageReference7 = NormalizeString(values.GetValueOrDefault("Package Reference 7")),
                            PackageReference8 = NormalizeString(values.GetValueOrDefault("Package Reference 8")),
                            UpsNumber = NormalizeString(values.GetValueOrDefault("UPS #"))
                        };
                        invoice.fileName = excelFile.FileName;
                        invoice.FileUrl = fileUrl;
                        invoice.ImportedBy = User;
                        invoice.ImportDate = DateTime.Now;
                        invoices.Add(invoice);
                    }
                }
                catch (Exception ex)
                {
                    // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                    var uri = new Uri(fileUrl);
                    var fileName = Path.GetFileName(uri.LocalPath);

                    // Remove the file from blob storage  
                    await _fileService.RemoveAzureBlobAsync(fileName, FileType.UPSInvoice);
                    throw new InvalidDataException("An error occurred while processing the Excel file.", ex);
                }
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.UPSInvoices.AddRangeAsync(invoices);
                await _unitOfWork.SaveChangesAsync();

                foreach (var invoice in invoices)
                {
                    string orderNumber = invoice.BillingReference1;
                    string trackingNumber = invoice.AirbillNumber;

                    InvoicedOrders genericInvoice = ProcessInvoices(orderNumber, trackingNumber, Carrier.UPS, orderShipments, orderFulfillments, invoice.InvoiceDate, invoice.UPSInvoiceId);
                    genericInvoices.Add(genericInvoice);
                }

                await _unitOfWork.InvoicedOrders.AddRangeAsync(genericInvoices);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();

                // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                // Remove the file from blob storage  
                await _fileService.RemoveAzureBlobAsync(fileName, FileType.UPSInvoice);
                throw;
            }

            return invoices;
        }

        public async Task<List<EasyPostInvoices>> ParseEasyPostInvoicesCsvFileAsync(IFormFile csvFile, string User)
        {
            if (await _unitOfWork.AzureBlobStorage.FileExistsAsync(csvFile.FileName, FileType.EasyPostInvoice))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            var fileUrl = await _fileService.UploadToAzureAsync(csvFile, FileType.EasyPostInvoice);
            if (await FileAlreadyImportedAsync(csvFile.FileName, CarrierType.EasyPost))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            // Fetch related data from orderFulfillments and orderShipments  
            var orderFulfillments = await _unitOfWork.OrderFulfillments.GetListByQueryAsync(of => of
                .Select(of => new OrderFulfillmentInvoiceInfo
                {
                    trackingNumber = of.trackingNumber,
                    ERPOrderId = of.ERPOrderId,
                    OrderNumber = of.Order.orderNumber
                }));
            var orderShipments = await _unitOfWork.OrderShipments.GetListByQueryAsync(os => os
                .Select(os => new OrderShipmentInvoiceInfo
                {
                    trackingNumber = os.trackingNumber,
                    ERPOrderId = os.ERPOrderId,
                    OrderNumber = os.Order.orderNumber
                }));

            var invoices = new List<EasyPostInvoices>();
            var genericInvoices = new List<InvoicedOrders>();

            using (var stream = new StreamReader(csvFile.OpenReadStream()))
            using (var parser = new TextFieldParser(stream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                var headers = parser.ReadFields();
                if (headers == null)
                {
                    throw new Exception("The CSV file does not contain headers.");
                }

                while (!parser.EndOfData)
                {
                    var values = parser.ReadFields();
                    if (values == null) continue;

                    try
                    {
                        var invoice = new EasyPostInvoices
                        {
                            CreatedAt = ParseDate(values[Array.IndexOf(headers, "created_at")]),
                            Rate = ParseDecimal(values[Array.IndexOf(headers, "rate")]),
                            TrackingCode = NormalizeString(values[Array.IndexOf(headers, "tracking_code")]),
                            Status = NormalizeString(values[Array.IndexOf(headers, "status")]),
                            Reference = NormalizeString(values[Array.IndexOf(headers, "reference")]),
                            Service = NormalizeString(values[Array.IndexOf(headers, "service")]),
                            Carrier = NormalizeString(values[Array.IndexOf(headers, "carrier")]),
                            InsuredValue = ParseDecimal(values[Array.IndexOf(headers, "insured_value")]),
                            IsReturn = ParseBool(values[Array.IndexOf(headers, "is_return")]),
                            RefundStatus = NormalizeString(values[Array.IndexOf(headers, "refund_status")]),
                            LabelFee = ParseDecimal(values[Array.IndexOf(headers, "label_fee")]),
                            PostageFee = ParseDecimal(values[Array.IndexOf(headers, "postage_fee")]),
                            InsuranceFee = ParseDecimal(values[Array.IndexOf(headers, "insurance_fee")]),
                            Options = values[Array.IndexOf(headers, "options")],
                            PostageLabelCreatedAt = ParseDate(values[Array.IndexOf(headers, "postage_label_created_at")]),
                            RateId = NormalizeString(values[Array.IndexOf(headers, "rate_id")]),
                            ParcelId = NormalizeString(values[Array.IndexOf(headers, "parcel_id")]),
                            FromAddressId = NormalizeString(values[Array.IndexOf(headers, "from_address_id")]),
                            FromName = NormalizeString(values[Array.IndexOf(headers, "from_name")]),
                            FromCompany = NormalizeString(values[Array.IndexOf(headers, "from_company")]),
                            FromStreet1 = NormalizeString(values[Array.IndexOf(headers, "from_street1")]),
                            FromStreet2 = NormalizeString(values[Array.IndexOf(headers, "from_street2")]),
                            FromCity = NormalizeString(values[Array.IndexOf(headers, "from_city")]),
                            FromState = NormalizeString(values[Array.IndexOf(headers, "from_state")]),
                            FromZip = NormalizeString(values[Array.IndexOf(headers, "from_zip")]),
                            FromCountry = NormalizeString(values[Array.IndexOf(headers, "from_country")]),
                            FromResidential = ParseBool(values[Array.IndexOf(headers, "from_residential")]),
                            ToAddressId = NormalizeString(values[Array.IndexOf(headers, "to_address_id")]),
                            ToName = NormalizeString(values[Array.IndexOf(headers, "to_name")]),
                            ToCompany = NormalizeString(values[Array.IndexOf(headers, "to_company")]),
                            ToStreet1 = NormalizeString(values[Array.IndexOf(headers, "to_street1")]),
                            ToStreet2 = NormalizeString(values[Array.IndexOf(headers, "to_street2")]),
                            ToCity = NormalizeString(values[Array.IndexOf(headers, "to_city")]),
                            ToState = NormalizeString(values[Array.IndexOf(headers, "to_state")]),
                            ToZip = NormalizeString(values[Array.IndexOf(headers, "to_zip")]),
                            ToCountry = NormalizeString(values[Array.IndexOf(headers, "to_country")]),
                            ToResidential = ParseBool(values[Array.IndexOf(headers, "to_residential")]),
                            Length = ParseDecimal(values[Array.IndexOf(headers, "length")]),
                            Width = ParseDecimal(values[Array.IndexOf(headers, "width")]),
                            Height = ParseDecimal(values[Array.IndexOf(headers, "height")]),
                            Weight = ParseDecimal(values[Array.IndexOf(headers, "weight")]),
                            PredefinedPackage = NormalizeString(values[Array.IndexOf(headers, "predefined_package")])
                        };
                        invoice.fileName = csvFile.FileName;
                        invoice.FileUrl = fileUrl;
                        invoice.ImportedBy = User;
                        invoice.ImportDate = DateTime.Now;
                        invoices.Add(invoice);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Console.WriteLine($"IndexOutOfRangeException: {ex.Message}");
                        for (int i = 0; i < headers.Length; i++)
                        {
                            Console.WriteLine($"Header: {headers[i]}, Value: {values[i]}");
                        }
                        // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                        var uri = new Uri(fileUrl);
                        var fileName = Path.GetFileName(uri.LocalPath);

                        // Remove the file from blob storage  
                        await _fileService.RemoveAzureBlobAsync(fileName, FileType.EasyPostInvoice);
                        throw;
                    }
                }
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.EasyPostInvoices.AddRangeAsync(invoices);
                await _unitOfWork.SaveChangesAsync();

                int? cwaOrderId = null;
                foreach (var invoice in invoices)
                {
                    string orderNumber = invoice.Reference;
                    string trackingNumber = invoice.TrackingCode;
                    InvoicedOrders genericInvoice = ProcessInvoices(orderNumber, trackingNumber, Carrier.EasyPost, orderShipments, orderFulfillments, null, invoice.EasyPostInvoiceId);
                    genericInvoices.Add(genericInvoice);
                }

                // Save generic invoices  
                await _unitOfWork.InvoicedOrders.AddRangeAsync(genericInvoices);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
            }
            catch(DbUpdateException dbex)
            {
               _logger.LogError(dbex, "Database update error occurred while processing EasyPost invoices.");
                await _unitOfWork.RollbackAsync();

                // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                // Remove the file from blob storage  
                await _fileService.RemoveAzureBlobAsync(fileName, FileType.EasyPostInvoice);
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Database update error occurred while processing EasyPost invoices.");
                await _unitOfWork.RollbackAsync();

                // Extracting the file name from the file URL, since we also want to delete the uploaded file from blob storage when this happens.  
                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                // Remove the file from blob storage  
                await _fileService.RemoveAzureBlobAsync(fileName, FileType.EasyPostInvoice);
                throw;
            }

            return invoices;
        }
        #region Skulabs Imports
        public async Task<List<SkulabsImport>> ParseSkulabsImportsCsvFileAsync(IFormFile csvFile, string user)
        {
            // Check if the file already exists in blob storage  
            if (await _unitOfWork.AzureBlobStorage.FileExistsAsync(csvFile.FileName, FileType.SkulabsImport))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            var fileUrl = await _fileService.UploadToAzureAsync(csvFile, FileType.SkulabsImport);

            if (await SkulabsFileAlreadyImportedAsync(csvFile.FileName))
            {
                throw new Exception($"The file '{csvFile.FileName}' has already been imported.");
            }

            var imports = new List<SkulabsImport>();
            var properties = typeof(SkulabsImport).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                   .Where(p => p.Name != "Id")
                                                   .ToList();

            // Define a mapping dictionary to map CSV headers to property names.  
            var headerToPropertyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Store", "Store" },
        { "Order #", "Order" },
        { "Order Tags", "OrderTags" },
        { "Tracking Number", "TrackingNumber" },
        { "Line ID", "Line_ID" },
        { "Order Status", "OrderStatus" },
        { "Status", "OrderStatus" },
        { "Archived", "Archived" },
        { "Image", "Image" },
        { "Name", "Name" },
        { "Listing Name", "ListingName" },
        { "Variant Name", "VariantName" },
        { "SKU", "SKU" },
        { "Cost", "Cost" },
        { "Wholesale", "Wholesale" },
        { "Retail", "Retail" },
        { "Price Sold", "PriceSold" },
        { "Quantity", "Quantity" },
        { "Drop Shipped", "DropShipped" },
        { "Metadata", "Metadata" },
        { "Cleared", "Cleared" },
        { "Picked Quantity", "PickedQuantity" },
        { "Order Date", "OrderDate" },
        { "Shipment Date", "ShipmentDate" },
        { "Manual Shipment", "ManualShipment" },
        { "Type", "Type" },
        { "Assigned Warehouse", "AssignedWarehouse" },
        { "Line SKU", "LineSKU" },
        { "Line Name", "LineName" },
        { "Company", "Company" },
        { "Customer Name", "CustomerName" },
        { "Customer Email", "CustomerEmail" },
        { "Customer Number", "CustomerNumber" },
        { "Address Line 1", "AddressLine1" },
        { "Address Line 2", "AddressLine2" },
        { "City", "City" },
        { "State", "State" },
        { "Zip", "Zip" },
        { "Country", "Country" },
        { "Postage", "Postage" },
        { "Provider", "Provider" },
        { "Method", "Method" },
        { "3PL Partner SKU", "_3PLPartnerSKU" },
        { "personalization_1_key", "Personalization1Key" },
        { "personalization_1_value", "Personalization1Value" },
        { "personalization_2_key", "Personalization2Key" },
        { "personalization_2_value", "Personalization2Value" },
        { "personalization_3_key", "Personalization3Key" },
        { "personalization_3_value", "Personalization3Value" },
        { "personalization_4_key", "Personalization4Key" },
        { "personalization_4_value", "Personalization4Value" },
        { "personalization_5_key", "Personalization5Key" },
        { "personalization_5_value", "Personalization5Value" },
        { "personalization_6_key", "Personalization6Key" },
        { "personalization_6_value", "Personalization6Value" },
        { "personalization_7_key", "Personalization7Key" },
        { "personalization_7_value", "Personalization7Value" },
        { "personalization_8_key", "Personalization8Key" },
        { "personalization_8_value", "Personalization8Value" },
        { "personalization_9_key", "Personalization9Key" },
        { "personalization_9_value", "Personalization9Value" }
    };

            using (var stream = new StreamReader(csvFile.OpenReadStream()))
            using (var parser = new TextFieldParser(stream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Read the header line and trim extra quotes for stuff like the store.  
                var headerFields = parser.ReadLine().Split(',')
                                                     .Select(h => h.Trim('"'))
                                                     .ToArray();

                // Create a dictionary to map header names to property info to make life a bit easier.  
                var propertyMap = new Dictionary<int, PropertyInfo>();
                for (int i = 0; i < headerFields.Length; i++)
                {
                    var header = headerFields[i];
                    if (headerToPropertyMap.TryGetValue(header, out var propertyName))
                    {
                        var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            propertyMap.Add(i, property);
                        }
                    }
                }

                while (!parser.EndOfData)
                {
                    var values = parser.ReadFields();
                    if (values == null) continue;

                    try
                    {
                        var import = new SkulabsImport();
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (propertyMap.TryGetValue(i, out var property))
                            {
                                var value = values[i];
                                if (!string.IsNullOrEmpty(value))
                                {
                                    if ((property.PropertyType == typeof(float?) || property.PropertyType == typeof(float)) && float.TryParse(value, out float floatValue))
                                    {
                                        property.SetValue(import, floatValue);
                                    }
                                    else if ((property.PropertyType == typeof(int?) || property.PropertyType == typeof(int)) && int.TryParse(value, out int intValue))
                                    {
                                        property.SetValue(import, intValue);
                                    }
                                    else if ((property.PropertyType == typeof(long?) || property.PropertyType == typeof(long)) && long.TryParse(value, out long longValue))
                                    {
                                        property.SetValue(import, longValue);
                                    }
                                    else if ((property.PropertyType == typeof(decimal?) || property.PropertyType == typeof(decimal)) && decimal.TryParse(value, out decimal decimalValue))
                                    {
                                        property.SetValue(import, decimalValue);
                                    }
                                    else if ((property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime)) && DateTime.TryParse(value, out DateTime dateValue))
                                    {
                                        property.SetValue(import, dateValue);
                                    }
                                    else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                                    {
                                        if (value.Equals("Yes", StringComparison.OrdinalIgnoreCase) || value.Equals("No", StringComparison.OrdinalIgnoreCase))
                                        {
                                            property.SetValue(import, value.Equals("Yes", StringComparison.OrdinalIgnoreCase));
                                        }
                                        if (value.Equals("Y", StringComparison.OrdinalIgnoreCase) || value.Equals("N", StringComparison.OrdinalIgnoreCase))
                                        {
                                            property.SetValue(import, value.Equals("Y", StringComparison.OrdinalIgnoreCase));
                                        }
                                    }
                                    else if (property.PropertyType == typeof(string))
                                    {
                                        var maxLength = property.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? int.MaxValue;
                                        if (value.Length > maxLength)
                                        {
                                            value = value.Substring(0, maxLength);
                                        }
                                        property.SetValue(import, value);
                                    }
                                }
                            }
                        }
                        import.fileName = csvFile.FileName;
                        import.FileUrl = fileUrl;
                        import.ImportedBy = user;
                        import.ImportDate = DateTime.Now;

                        imports.Add(import);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Console.WriteLine($"IndexOutOfRangeException: {ex.Message}");
                        for (int i = 0; i < values.Length; i++)
                        {
                            Console.WriteLine($"Header: {headerFields[i]}, Value: {values[i]}");
                        }
                        throw;
                    }
                }
            }

            await _unitOfWork.SkulabsImports.AddRangeAsync(imports);
            await _unitOfWork.SaveChangesAsync();
            return imports;
        }

        #endregion
        #region Invoice Table View

        public async Task<(List<InvoiceViewModel>, int)> GetInvoicesAsync(
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
        )
        {
            IQueryable<InvoiceViewModel> allInvoices;

            // Check if orderNumber or trackingCode is present. If it is, we follow a different logic to get the linked invoices efficiently.
            if (!string.IsNullOrEmpty(orderNumber) || !string.IsNullOrEmpty(trackingCode))
            {
                var invoicedOrdersQuery = _unitOfWork.InvoicedOrders.QueryFilter(q => q.AsQueryable());

                if (!string.IsNullOrEmpty(orderNumber))
                {
                    invoicedOrdersQuery = invoicedOrdersQuery.Where(io => io.OrderNumber.ToLower().Contains(orderNumber.ToLower()));
                }

                if (!string.IsNullOrEmpty(trackingCode))
                {
                    invoicedOrdersQuery = invoicedOrdersQuery.Where(io => io.TrackingNumber.ToLower().Contains(trackingCode.ToLower()));
                }

                var invoicedOrderIds = await invoicedOrdersQuery.Select(io => new
                {
                    io.DHLInvoiceId,
                    io.UPSInvoiceId,
                    io.StampsUSPSInvoiceId,
                    io.EasyPostInvoiceId
                }).ToListAsync();

                var invoiceIds = invoicedOrderIds.SelectMany(io => new[]
                {
                    io.DHLInvoiceId,
                    io.UPSInvoiceId,
                    io.StampsUSPSInvoiceId,
                    io.EasyPostInvoiceId
                }).Where(id => id.HasValue).Select(id => id.Value).ToList();

                        // Fetch the invoices from carrier tables  
                        var dhlInvoices = await _unitOfWork.DHLInvoices.QueryFilter(q => q
                            .Where(i => invoiceIds.Contains(i.DHLInvoiceId))
                            .Select(i => new InvoiceViewModel
                            {
                                Id = i.DHLInvoiceId,
                                CarrierType = Carrier.DHL.ToString(),
                                UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                                FileName = i.fileName,
                                UploadedBy = i.ImportedBy ?? "Unknown",
                                FileUrl = i.FileUrl ?? "#",
                                TotalCost = (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.Charge) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeContentEndors) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeUnassignableAdd) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeSpecialHandling) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeLateArrival) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeUSPSQualif) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeClientSRD) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeIrregular) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.Tax) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.SurchargeFuel) ?? 0) +
                                            (q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.PeakSurcharge) ?? 0)
                            })).ToListAsync();

                        var upsInvoices = await _unitOfWork.UPSInvoices.QueryFilter(q => q
                            .Where(i => invoiceIds.Contains(i.UPSInvoiceId))
                            .Select(i => new InvoiceViewModel
                            {
                                Id = i.UPSInvoiceId,
                                CarrierType = Carrier.UPS.ToString(),
                                UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                                FileName = i.fileName,
                                UploadedBy = i.ImportedBy ?? "Unknown",
                                FileUrl = i.FileUrl ?? "#",
                                TotalCost = q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.ChargeTotal) ?? 0
                            })).ToListAsync();

                        var stampsUSPSInvoices = await _unitOfWork.StampsUSPSInvoices.QueryFilter(q => q
                            .Where(i => invoiceIds.Contains(i.StampsUSPSInvoiceId))
                            .Select(i => new InvoiceViewModel
                            {
                                Id = i.StampsUSPSInvoiceId,
                                CarrierType = Carrier.StampsUSPS.ToString(),
                                UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                                FileName = i.fileName,
                                UploadedBy = i.ImportedBy ?? "Unknown",
                                FileUrl = i.FileUrl ?? "#",
                                TotalCost = q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.AmountPaid) ?? 0
                            })).ToListAsync();

                        var easyPostInvoices = await _unitOfWork.EasyPostInvoices.QueryFilter(q => q
                            .Where(i => invoiceIds.Contains(i.EasyPostInvoiceId))
                            .Select(i => new InvoiceViewModel
                            {
                                Id = i.EasyPostInvoiceId,
                                CarrierType = Carrier.EasyPost.ToString(),
                                UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                                FileName = i.fileName,
                                UploadedBy = i.ImportedBy ?? "Unknown",
                                FileUrl = i.FileUrl ?? "#",
                                TotalCost = q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.Rate) ?? 0
                            })).ToListAsync();

                        // Combine the lists into a single IQueryable  
                        allInvoices = dhlInvoices
                            .Concat(upsInvoices)
                            .Concat(stampsUSPSInvoices)
                            .Concat(easyPostInvoices)
                            .AsQueryable();
            }

            // If there is no order number or tracking code present in the filters, then we fetch the invoices as normal.
            else
            {
                // Fetch data from multiple tables  
                var dhlInvoices = await _unitOfWork.DHLInvoices.QueryFilter(q => q
                    .GroupBy(i => i.fileName)
                    .Select(g => g.Select(i => new InvoiceViewModel
                    {
                        Id = i.DHLInvoiceId,
                        CarrierType = Carrier.DHL.ToString(),
                        UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                        FileName = i.fileName,
                        UploadedBy = i.ImportedBy ?? "Unknown",
                        FileUrl = i.FileUrl ?? "#",
                        TotalCost = g.Sum(u => (u.Charge ?? 0) +
                                               (u.SurchargeContentEndors ?? 0) +
                                               (u.SurchargeUnassignableAdd ?? 0) +
                                               (u.SurchargeSpecialHandling ?? 0) +
                                               (u.SurchargeLateArrival ?? 0) +
                                               (u.SurchargeUSPSQualif ?? 0) +
                                               (u.SurchargeClientSRD ?? 0) +
                                               (u.SurchargeIrregular ?? 0) +
                                               (u.Tax ?? 0) +
                                               (u.SurchargeFuel ?? 0) +
                                               (u.PeakSurcharge ?? 0))
                    }).FirstOrDefault()))
                    .ToListAsync();

                var upsInvoices = await _unitOfWork.UPSInvoices.QueryFilter(q => q
                    .GroupBy(i => i.fileName)
                    .Select(g => g.Select(i => new InvoiceViewModel
                    {
                        Id = i.UPSInvoiceId,
                        CarrierType = Carrier.UPS.ToString(),
                        UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                        FileName = i.fileName,
                        UploadedBy = i.ImportedBy ?? "Unknown",
                        FileUrl = i.FileUrl ?? "#",
                        TotalCost = g.Sum(i => (decimal?)i.ChargeTotal) ?? 0
                    }).FirstOrDefault()))
                    .ToListAsync();

                var stampsUSPSInvoices = await _unitOfWork.StampsUSPSInvoices.QueryFilter(q => q
                    .GroupBy(i => i.fileName)
                    .Select(g => g.Select(i => new InvoiceViewModel
                    {
                        Id = i.StampsUSPSInvoiceId,
                        CarrierType = Carrier.StampsUSPS.ToString(),
                        UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                        FileName = i.fileName,
                        UploadedBy = i.ImportedBy ?? "Unknown",
                        FileUrl = i.FileUrl ?? "#",
                        TotalCost = q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.AmountPaid) ?? 0
                    }).FirstOrDefault()))
                    .ToListAsync();

                var easyPostInvoices = await _unitOfWork.EasyPostInvoices.QueryFilter(q => q
                    .GroupBy(i => i.fileName)
                    .Select(g => g.Select(i => new InvoiceViewModel
                    {
                        Id = i.EasyPostInvoiceId,
                        CarrierType = Carrier.EasyPost.ToString(),
                        UploadDate = i.ImportDate ?? new DateTime(2025, 1, 1),
                        FileName = i.fileName,
                        UploadedBy = i.ImportedBy ?? "Unknown",
                        FileUrl = i.FileUrl ?? "#",
                        TotalCost = q.Where(u => u.fileName == i.fileName).Sum(u => (decimal?)u.Rate) ?? 0
                    }).FirstOrDefault()))
                    .ToListAsync();

                allInvoices = dhlInvoices
                    .Concat(upsInvoices)
                    .Concat(stampsUSPSInvoices)
                    .Concat(easyPostInvoices)
                    .AsQueryable();
            }

            // Apply filtering  
            if (!string.IsNullOrEmpty(carrierType))
            {
                allInvoices = allInvoices.Where(i => i.CarrierType.Contains(carrierType, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(uploadDateStart) && DateTime.TryParseExact(uploadDateStart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
            {
                allInvoices = allInvoices.Where(i => i.UploadDate.Date >= startDate.Date);
            }
            if (!string.IsNullOrEmpty(uploadDateEnd) && DateTime.TryParseExact(uploadDateEnd, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                allInvoices = allInvoices.Where(i => i.UploadDate.Date <= endDate.Date);
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                allInvoices = allInvoices.Where(i => i.FileName.Contains(fileName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(uploadedBy))
            {
                allInvoices = allInvoices.Where(i => i.UploadedBy.Contains(uploadedBy, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting  
            allInvoices = ApplySorting(allInvoices, invoiceColumn, invoiceDir.Equals("asc", StringComparison.OrdinalIgnoreCase));

            // Get total count before pagination  
            var totalCount = allInvoices.Count();

            // Apply pagination  
            var paginatedInvoices = allInvoices.Skip(start).Take(length).ToList();

            return (paginatedInvoices, totalCount);
        }

        private IQueryable<InvoiceViewModel> ApplySorting(IQueryable<InvoiceViewModel> query, string column, bool ascending)
        {
            return column switch
            {
                "CarrierType" => ascending ? query.OrderBy(i => i.CarrierType) : query.OrderByDescending(i => i.CarrierType),
                "UploadDate" => ascending ? query.OrderBy(i => i.UploadDate) : query.OrderByDescending(i => i.UploadDate),
                "FileName" => ascending ? query.OrderBy(i => i.FileName) : query.OrderByDescending(i => i.FileName),
                "UploadedBy" => ascending ? query.OrderBy(i => i.UploadedBy) : query.OrderByDescending(i => i.UploadedBy),
                _ => ascending ? query.OrderBy(i => i.UploadDate) : query.OrderByDescending(i => i.UploadDate),
            };
        }

        #endregion
        #region Generic Methods
        private DateTime? ParseDate(string date)
        {
            string[] formats = { "MM/dd/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ssZ", "yyyyMMdd" };
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    return parsedDate;
                }
            }
            return null;
        }
        private int? ParseInt(string value)
        {
            return int.TryParse(value, out var parsedInt) ? (int?)parsedInt : null;
        }

        private decimal? ParseDecimal(string value)
        {
            return decimal.TryParse(value, out var parsedDecimal) ? (decimal?)parsedDecimal : null;
        }
        private bool ParseBool(string boolString)
        {
            return bool.TryParse(boolString, out var boolValue) && boolValue;
        }
        private T? ParseEnum<T>(string value) where T : struct
        {
            return Enum.TryParse(value, out T parsedEnum) ? (T?)parsedEnum : null;
        }
        private static List<PropertyInfo> GetImportProperties()
        {
            return typeof(UPSInvoices).GetProperties()
                .Where(x =>
                    (x.PropertyType == typeof(string) ||
                     x.PropertyType == typeof(int?) ||
                     x.PropertyType == typeof(decimal?) ||
                     x.PropertyType == typeof(DateTime?) ||
                     x.PropertyType == typeof(int) ||
                     x.PropertyType == typeof(decimal) ||
                     x.PropertyType == typeof(DateTime)) &&
                    !x.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            // Handles Stamps TrackingNumber and PostalCode format. 
            if (input.StartsWith("=\"") && input.EndsWith("\""))
            {
                input = input.Substring(2, input.Length - 3);
            }

            return input.Trim().ToUpper();
        }

        private string ExtractStampsOrderNumber(string printedMessage)
        {
            if (string.IsNullOrEmpty(printedMessage))
            {
                return string.Empty;
            }

            // Consider only the part before the first newline character, since some entries have a newline split.
            int newlineIndex = printedMessage.IndexOf('\n');
            if (newlineIndex != -1)
            {
                printedMessage = printedMessage.Substring(0, newlineIndex);
            }
            int orderNumberEndIndex = 0;
            while (orderNumberEndIndex < printedMessage.Length &&
                   (char.IsLetterOrDigit(printedMessage[orderNumberEndIndex])))
            {
                orderNumberEndIndex++;
            }

            // Returning the extracted order number.
            return printedMessage.Substring(0, orderNumberEndIndex);
        }

        // Methods used by all of the parsing methods.
        private async Task<bool> FileAlreadyImportedAsync(string fileName, CarrierType carrier)
        {
            return await Task.Run(() =>
            {
                switch (carrier)
                {
                    case CarrierType.UPS:
                        return _unitOfWork.UPSInvoices.Any(invoice => invoice.fileName == fileName);
                    case CarrierType.StampsUSPS:
                        return _unitOfWork.StampsUSPSInvoices.Any(invoice => invoice.fileName == fileName);
                    case CarrierType.DHL:
                        return _unitOfWork.DHLInvoices.Any(invoice => invoice.fileName == fileName);
                    case CarrierType.EasyPost:
                        return _unitOfWork.EasyPostInvoices.Any(invoice => invoice.fileName == fileName);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(carrier), carrier, null);
                }
            });
        }

        private async Task<bool> SkulabsFileAlreadyImportedAsync(string fileName)
        {
            return await Task.Run(() =>
            {
                return _unitOfWork.SkulabsImports.Any(import => import.fileName == fileName);
            });
        }

        // Method for processing the invoiced files and creating a generic invoice object for them.
        public InvoicedOrders ProcessInvoices(string invoiceOrderNumber, string invoiceTrackingNumber, Carrier carrier, List<OrderShipmentInvoiceInfo> orderShipments, List<OrderFulfillmentInvoiceInfo> orderFulfillments, DateTime? invoiceDate, int invoiceId)
        {
            int? cwaOrderId = null;
            var matchingShipment = orderShipments.FirstOrDefault
                (os => !string.IsNullOrEmpty(os.trackingNumber) && !string.IsNullOrEmpty(invoiceTrackingNumber) && os.trackingNumber.EndsWith(invoiceTrackingNumber));
            var matchingFulfillment = orderFulfillments.FirstOrDefault
                (of => !string.IsNullOrEmpty(of.trackingNumber) && !string.IsNullOrEmpty(invoiceTrackingNumber) && of.trackingNumber.EndsWith(invoiceTrackingNumber));

            if (matchingShipment != null)
            {
                cwaOrderId = matchingShipment.ERPOrderId;
                invoiceOrderNumber = matchingShipment.OrderNumber;
                invoiceTrackingNumber = matchingShipment.trackingNumber;

            }
            else if (matchingFulfillment != null)
            {
                cwaOrderId = matchingFulfillment.ERPOrderId;
                invoiceOrderNumber = matchingFulfillment.OrderNumber;
                invoiceTrackingNumber = matchingFulfillment.trackingNumber;
            }
            else if (!string.IsNullOrEmpty(invoiceOrderNumber))
            {
                matchingShipment = orderShipments.FirstOrDefault(os => os.OrderNumber == invoiceOrderNumber);
                matchingFulfillment = orderFulfillments.FirstOrDefault(of => of.OrderNumber == invoiceOrderNumber);

                if (matchingShipment != null)
                {
                    cwaOrderId = matchingShipment.ERPOrderId;
                    invoiceOrderNumber = matchingShipment.OrderNumber;
                    invoiceTrackingNumber = matchingShipment.trackingNumber;
                }
                else if (matchingFulfillment != null)
                {
                    cwaOrderId = matchingFulfillment.ERPOrderId;
                    invoiceOrderNumber = matchingFulfillment.OrderNumber;
                    invoiceTrackingNumber = matchingFulfillment.trackingNumber;
                }
            }

            var genericInvoice = new InvoicedOrders
            {
                DateInvoiced = invoiceDate,
                OrderNumber = invoiceOrderNumber,
                OrderCarrier = carrier,
                ERPOrderId = cwaOrderId,
                TrackingNumber = invoiceTrackingNumber
            };
            switch (carrier)
            {
                case Carrier.UPS:
                    genericInvoice.UPSInvoiceId = invoiceId;
                    break;
                case Carrier.StampsUSPS:
                    genericInvoice.StampsUSPSInvoiceId = invoiceId;
                    break;
                case Carrier.DHL:
                    genericInvoice.DHLInvoiceId = invoiceId;
                    break;
                case Carrier.EasyPost:
                    genericInvoice.EasyPostInvoiceId = invoiceId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carrier), carrier, null);
            }


            return genericInvoice;
        }

        private async Task<(List<OrderData> OrderFulfillments, List<OrderData> OrderShipments)> FetchOrderDataAsync()
        {
            var orderFulfillmentsTask = _unitOfWork.OrderFulfillments.GetListByQueryAsync(of => of
                .Select(of => new OrderData
                {
                    trackingNumber = of.trackingNumber,
                    cWAOrderId = of.ERPOrderId,
                    orderNumber = of.Order.orderNumber
                })
            );

            var orderShipmentsTask = _unitOfWork.OrderShipments.GetListByQueryAsync(os => os
                .Select(os => new OrderData
                {
                    trackingNumber = os.trackingNumber,
                    cWAOrderId = os.ERPOrderId,
                    orderNumber = os.Order.orderNumber
                })
            );

            await Task.WhenAll(orderFulfillmentsTask, orderShipmentsTask);

            return (await orderFulfillmentsTask, await orderShipmentsTask);
        }

        // Class to store the fulfillment and shipment data.
        public class OrderData
        {
            public string trackingNumber { get; set; }
            public int? cWAOrderId { get; set; }
            public string orderNumber { get; set; }
        }
        public class OrderShipmentInvoiceInfo
        {
            public string trackingNumber { get; set; }
            public int? ERPOrderId { get; set; }
            public string OrderNumber { get; set; }
        }

        public class OrderFulfillmentInvoiceInfo
        {
            public string trackingNumber { get; set; }
            public int? ERPOrderId { get; set; }
            public string OrderNumber { get; set; }
        }
        // To denote what cases to follow when using the generic methods with differing instructions.
        public enum CarrierType
        {
            UPS,
            StampsUSPS,
            DHL,
            EasyPost
        }
        public enum InvoiceType
        {
            Normal,
            Skulabs,
            International
        }
        #endregion
    }
}
