using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using System.Text;
using ERPWebApp.Providers.Interfaces;
using Microsoft.AspNetCore.Identity;


namespace ERPWebApp.Services
{
    public class TriggerEmailAlertService : ITriggerEmailAlertService
    {
        private readonly IGraphAPIService _graphAPIService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITriggerEmailAlertManager _triggerEmailAlertManager;
        private Dictionary<int, bool> _lowStockAlertSent;
        private readonly IUserProvider _userProvider;
        private readonly UserManager<IdentityUser> _userManager;

        public TriggerEmailAlertService(IGraphAPIService graphAPIService, IUnitOfWork unitOfWork, ITriggerEmailAlertManager triggerEmailAlertManager,IUserProvider userProvider, UserManager<IdentityUser> userManager)
        {
            _graphAPIService = graphAPIService;
            _unitOfWork = unitOfWork;
            _triggerEmailAlertManager = triggerEmailAlertManager;
            _lowStockAlertSent = new Dictionary<int, bool>();
            _userProvider = userProvider;
            _userManager = userManager;
        }

        public async Task<List<EmailAlert>> GetTriggeredEmailAlertsAsync()
        {
            return await _unitOfWork.EmailAlerts.FindAsync(e => e.AlertType == AlertType.TriggerBased && e.IsActive);
        }
        public async Task<List<EmailAlert>> GetUserTriggeredEmailAlertsAsync()
        {
            return await _unitOfWork.EmailAlerts.FindAsync(e => e.AlertType == AlertType.TriggerBased && e.IsActive && e.AlertTemplateId== (int)EmailTemplateId.UserCreateEmail);
        }

        public async Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId)
        {
            return await _unitOfWork.EmailAlerts.GetRecipientsForEmailAlertAsync(emailAlertId);
        }

        public async void SendEmails(string subject, string body, List<string> recipients, byte[] attachment = null)
        {
            foreach (var recipient in recipients)
            {
                string userId = await _graphAPIService.GetUserIdByEmail(recipient);
                await _graphAPIService.SendEmailAlert(subject, body, recipient, userId, attachment);
            }
        }

        private string ReplacePlaceholdersInBody(string body, Product products = null, List<Stock> stocks = null, CycleCountFinishedEmailAlertDTO cycleCount=null, UserEmailAlertDTO userEmailData=null)
        {
            if (stocks != null)
            {
                StringBuilder stockList = new StringBuilder();
                stockList.AppendLine("Low stock products list:<br>");
                foreach (var stock in stocks)
                {
                    stockList.AppendLine($"Product SKU: {stock.Products.Sku}, Product Name: {stock.Products.Description}, Minimum Inventory: {stock.Products.MinInventory}, Total Available: {stock.TotalAvailable}<br>");
                }

                body = body.Replace("{stock_list}", stockList.ToString());
            }

            if (cycleCount != null)
            {
                StringBuilder CycleCount = new StringBuilder();
                CycleCount.AppendLine("<br>");
                CycleCount.AppendLine($"Location: {cycleCount.Location}<br>");
                CycleCount.AppendLine($"SKU: {cycleCount.Sku}<br>"); 
                CycleCount.AppendLine($"Previous QTY Amount: {cycleCount.PreviousQuantity}<br>");  
                CycleCount.AppendLine($"New QTY Amount: {cycleCount.NewQuantity}<br>");

                body = body.Replace("{cycle_count}", CycleCount.ToString());
            }

            if (userEmailData != null) 
            {
                body = body.Replace("{userEmail}", userEmailData.userEmail.ToString())
                   .Replace("{userName}", userEmailData.userName.ToString())
                   .Replace("{password}", userEmailData.password.ToString());
            }

            // Will be adding more replacements for other parameters as needed  
            // When calling this, be sure to follow the following example;
            // string scanBody = ReplacePlaceholdersInBody(body, products: inputtedProducts);  
            // If we want to pass in orders, but not stock, we can do it this way so that we have stock remain null.

            return body;
        }

        // Checks happening when stock updates. Example, minimum stock checks.
        public async Task NotifyOnStockUpdateAsync(Stock oldStock, Stock newStock)
        {
            string alertType = "LowStockAlert";
            bool lowStockAlertSent;
            _triggerEmailAlertManager.TryGetValue(alertType, newStock.StockId, out lowStockAlertSent);
            int requiredStock = newStock.Products.MinInventory + newStock.Products.OnOrder;

            if (newStock.TotalAvailable <= requiredStock)
            {
                if (!lowStockAlertSent)
                {
                    // Get all stocks    
                    var allStocks = await _unitOfWork.Stocks.GetAllStocksWithProductAndLocationAsync();

                    // Filter the low stocks based on the receive-only condition    
                    var lowStocks = allStocks.Where(s => s.Products != null && s.Location != null && s.Products.IsActive && s.TotalAvailable <= s.Products.MinInventory && s.Location.Type != LocationType.ReceiveOnly).ToList();

                    // Group low stocks by product and sum their TotalAvailable quantities  
                    var groupedLowStocks = lowStocks.GroupBy(s => s.Products.ProductId)
                                                    .Select(g => new Stock
                                                    {
                                                        Products = g.First().Products,
                                                        Location = null,
                                                        TotalAvailable = g.Sum(s => s.TotalAvailable)
                                                    }).ToList();

                    await SendLowStockAlerts(groupedLowStocks);

                    // Update the flags for all the stocks  
                    foreach (var stock in lowStocks)
                    {
                        _triggerEmailAlertManager.Update(alertType, stock.StockId, true);
                    }
                }
            }
            else
            {
                if (lowStockAlertSent)
                {
                    _triggerEmailAlertManager.Update(alertType, newStock.StockId, false);
                }
            }
        }

        private async Task SendLowStockAlerts(List<Stock> lowStocks)
        {
            var triggeredEmailAlerts = await GetTriggeredEmailAlertsAsync();

            foreach (var emailAlert in triggeredEmailAlerts)
            {
                if (emailAlert.AlertTemplateId == 2)
                {
                    var recipients = await GetRecipientsForEmailAlertAsync(emailAlert.EmailAlertId);
                    var subject = emailAlert.Subject;
                    var body = emailAlert.Body;

                    // Generate the CSV file content  
                    string lowStockCsv = GenerateLowStockCsv(lowStocks);
                    byte[] csvBytes = Encoding.UTF8.GetBytes(lowStockCsv);

                    // Update the email body to inform about the attachment  
                    body = body.Replace("{stock_list}", "Please find the low stock report attached as a CSV file.");

                    SendEmails(subject, body, recipients, csvBytes);
                }
            }
        }
        public async Task SendFinishedCycleCountAlerts(CycleCountFinishedEmailAlertDTO cycleCount)
        {
            var triggeredEmailAlerts = await GetTriggeredEmailAlertsAsync();

            foreach (var emailAlert in triggeredEmailAlerts)
            {
                if (emailAlert.AlertTemplateId == 3)
                {
                    var recipients = await GetRecipientsForEmailAlertAsync(emailAlert.EmailAlertId);
                    var subject = emailAlert.Subject;
                    var body = emailAlert.Body;

                    body = ReplacePlaceholdersInBody(body, cycleCount: cycleCount);

                    SendEmails(subject, body, recipients);
                }
            }
        }

        // Create a trigger template for; user creation.
        // Create a trigger template for; stock in specified locations for when quantity is above a set threshold that the user decides for the location.
        // Create a trigger template for; cycle counts. When a cycle count is finished, it should send an email that contains all the stock that was modified with a different quantity amount.
        // This template should show location, sku, previous quantity, and new quantity.

        public async Task SendTriggerEmailAlertNow(EmailAlert emailAlert, List<string> recipients)
        {
            if (emailAlert.AlertTemplateId == 2)
            {
                // Get all stocks    
                var allStocks = await _unitOfWork.Stocks.GetAllStocksWithProductAndLocationAsync();

                // Filter the low stocks based on the receive-only condition    
                var lowStocks = allStocks.Where(s => s.Products != null && s.Location != null && s.Products.IsActive && s.TotalAvailable <= s.Products.MinInventory && s.Location.Type != LocationType.ReceiveOnly).ToList();

                // Group low stocks by product and sum their TotalAvailable quantities, that way we don't have a bunch of duplicate products in the low stocks list. 
                var groupedLowStocks = lowStocks.GroupBy(s => s.Products.ProductId)
                                                .Select(g => new Stock
                                                {
                                                    Products = g.First().Products,
                                                    Location = null,
                                                    TotalAvailable = g.Sum(s => s.TotalAvailable)
                                                }).ToList();

                await SendLowStockAlerts(groupedLowStocks);
            }
            else
            {
                // Add logic for other AlertTemplateIds once we add them later.  
            }
        }
        private string GenerateLowStockCsv(List<Stock> lowStocks)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Product SKU,Product Name,Minimum Inventory,Total Available");

            var sortedLowStocks = lowStocks.OrderBy(stock => stock.Products.Sku);

            foreach (var stock in sortedLowStocks)
            {
                // This replaces new line characters with spaces and bypasses double quotes by doubling up on them. This makes sure that the CSV is formatted correctly.
                string description = stock.Products.Description.Replace("\r\n", " ").Replace("\n", " ").Replace("\"", "\"\"");
                if (description.Contains(","))
                {
                    description = $"\"{description}\"";
                }
                csv.AppendLine($"{stock.Products.Sku},{description},{stock.Products.MinInventory},{stock.TotalAvailable}");
            }

            return csv.ToString();
        }

        public async Task SendUserCreateEmail(UserEmailAlertDTO userEmailData)
        {
            var loggedUserID= _userProvider.GetCurrentUserId();
            var loggedUser = await _userManager.FindByIdAsync(loggedUserID.ToString());
            var loggedUserEmail = loggedUser.Email;
            var triggeredEmailAlerts = await GetUserTriggeredEmailAlertsAsync();

            if (triggeredEmailAlerts == null || !triggeredEmailAlerts.Any())
            {
                var subject = "Create new user account";
                var body = $"Created new user account for {userEmailData.userEmail} with user name {userEmailData.userName} and password {userEmailData.password}";
                string userId = await _graphAPIService.GetUserIdByEmail(loggedUserEmail);
                await _graphAPIService.SendEmailAlert(subject, body, loggedUserEmail, userId);
            }
            else
            {
                foreach (var emailAlert in triggeredEmailAlerts)
                {
                    var recipients = await GetRecipientsForEmailAlertAsync(emailAlert.EmailAlertId);
                    bool isEmailPresent = recipients.Contains(loggedUserEmail);
                    if (!isEmailPresent)
                    {
                        recipients.Add(loggedUserEmail);
                    }
                    var subject = emailAlert.Subject;
                    var body = emailAlert.Body;

                    body = ReplacePlaceholdersInBody(body, userEmailData: userEmailData);
                    SendEmails(subject, body, recipients);
                }
            }
            
        }
        public enum EmailTemplateId
        {
            UserCreateEmail = 5,
        }

    }

}
