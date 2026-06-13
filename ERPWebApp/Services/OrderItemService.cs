using System.Text.RegularExpressions;
using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using static ERPWebApp.Data.DTOModels.ZazzleDTO.ZazzleRequest.Response.Result;
using ERPWebApp.Models.Orders;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Services;
public class OrderItemService(IUnitOfWork unitOfWork, ILogger<OrderItemService> logger) : Service<OrderItem>(unitOfWork), IOrderItemService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<OrderItemService> _logger = logger;
    private readonly Regex _invalidBundleSkuRegex = new("(SO\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    /// <summary>  
    /// Updates the product IDs of the given order items based on their SKUs.  
    /// </summary>  
    /// <param name="items">The list of OrderItem instances to update.</param>  
    public async Task<List<OrderItem>> AssignProductIds(List<OrderItem> items)
    {
        try
        {
            List<OrderItem> updatedItems = [];
            foreach (OrderItem item in items)
            {
                if (item.ERPBundleId != null || item.ERPProductId != null)
                {
                    updatedItems.Add(item);
                    continue;
                }
                    
                item.sku ??= "";
                Bundle cwaBundle = await _unitOfWork.Bundles.FilterOneAsNoTrackingAsync(b =>
                item.sku.ToUpperInvariant().StartsWith(b.BundleName));
                if (cwaBundle != null)
                {
                    item.ERPBundleId = cwaBundle.BundleId;
                    updatedItems.Add(item);
                }
                else
                {
                    Product cwaProduct = await _unitOfWork.Products.FilterOneAsNoTrackingAsync(
                    p => p.IsActive && p.Departments.Any(d => d.IsProduction) && item.sku.ToUpperInvariant().StartsWith(p.Sku),
                    includes: [p => p.Departments]);
                    
                    //Check to make sure the sku isn't just a typod Bundle Sku or a nonexistant bundle.
                    if (cwaProduct != null && !_invalidBundleSkuRegex.IsMatch(cwaProduct.Sku))
                    {
                        item.ERPProductId = cwaProduct.ProductId;
                        updatedItems.Add(item);
                    }
                    else
                    {
                        updatedItems.Add(item);
                    }
                }

            }
            items = updatedItems;
            _unitOfWork.OrderItems.UpdateRange(items);
            await _unitOfWork.SaveChangesAsync();
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error Updating items: {items} with error {ex}", items, ex.Message);
            throw;
        }
    }

    public async Task<OrderItem> ConvertAttributeToProductSku(OrderItem orderItem)
    {
        string itemAttributes = orderItem.fulfillmentSku.ToUpperInvariant();
        Regex regex = new("(\\w+)=([^&\n]+)");
        Product foundProduct = new();
        string style = "";
        string size = "";
        string color = "";

        foreach (Match attribute in regex.Matches(itemAttributes).ToList())
        {
            var attributeType = attribute.Value.Split('=')[0];
            var attributeValue = attribute.Value.Split('=')[1];

            if (attributeType == "STYLE")
            {
                // These are legacy skus within Zazzle that has been converted to mean different styles from GILDAN currently
                bool containsStyleCode = int.TryParse(attributeValue.Split("_").Last(), out int intParseResult);
                if (intParseResult == 5680 || intParseResult == 5586 || intParseResult == 5250 || intParseResult == 5450 ||
                    (containsStyleCode == false && attributeValue.Split("_").Last() == "S04V"))
                    attributeValue = attributeValue
                        .Replace("5680", "500L")//"L" for ladies
                        .Replace("5586", "2400")
                        .Replace("5250", "5000")//extra "0" for unisex
                        .Replace("5450", "500B")//"B" for youth?? apparently
                        .Replace("S04V", "500VL")
                        .Replace("HANES", "GILDAN");
                else if (intParseResult == 20521)
                    attributeValue = attributeValue.Replace("20521", "570").Replace("SUNAPP", "GILDAN");
                else if (!containsStyleCode && attributeValue.Split("_").First() == "LADIES")
                    attributeValue = attributeValue.Replace("TSHIRT", "6004").Replace("HANES", "GILDAN");
                else if (!containsStyleCode && attributeValue.Split("_").First() == "MENS")
                    attributeValue = attributeValue.Replace("TSHIRT", "3004").Replace("HANES", "GILDAN");
                else if (intParseResult == 2001 && attributeValue.Split("_").First() == "AA")
                    attributeValue = attributeValue.Replace("2001", "3001");
                else if (!containsStyleCode && attributeValue.EndsWith("PC54"))
                    attributeValue = attributeValue.Replace("PC54", "5280");
                else if (!containsStyleCode && attributeValue.EndsWith("PC473"))
                    attributeValue = attributeValue.Replace("PC473", "185B").Replace("HANES", "GILDAN");

                style = attributeValue.Split("_")[0] switch
                {
                    "GILDAN" => $"GIL.G{attributeValue.Split("_").Last()}",
                    "RABBITSKINS" or "LAT" => $"LAT.{attributeValue.Split("_").Last()}",
                    "HANES" or "PORTCOMPANY" => $"HAN.{attributeValue.Split("_").Last()}",
                    "BELLA" or "AA" or _ => $"BEL.{attributeValue.Split("_").Last()}"
                };
            }
            else if (attributeType == "SIZE")
            {
                var sizeTypeSet = attributeValue
                    .Replace("_", "")
                    .Replace("Y", "")
                    .Replace("A", "")
                    .Replace("TODDLER", "T")
                    .Replace("MONTHS", "M")
                    .Replace("PLUS", "P")
                    .Replace("NEWBORN", "NB");
                size = sizeTypeSet switch
                {
                    "2XL" => "XXL",
                    "3XL" => "XXXL",
                    "4XL" => "XXXXL",
                    "5XL" => "XXXXXL",
                    _ => $"{sizeTypeSet}"
                };
                //We need to move the "P" for our product skus
                if (sizeTypeSet.StartsWith("P"))
                    sizeTypeSet = sizeTypeSet.Replace("P", "") + "P";
            }
            else if (attributeType == "COLOR")
            {
                if (style.Equals("BEL.3001") && attributeValue == "NAVY")
                    attributeValue += "BLUE";
                else if ((style.StartsWith("GIL") || style.StartsWith("BEL")) && attributeValue == "NAVYBLUE")
                    attributeValue = "NAVY";
                else if (style.Equals("GIL.G5000") && attributeValue == "BROWN")
                    attributeValue = "DARKCHOCOLATE";
                else if (style.Equals("GIL.G2400") && attributeValue == "GREY")
                    attributeValue = "ASHGREY";
                else if (style.Equals("GIL.G5000") && attributeValue == "ASH")
                    attributeValue += "GREY";

                color = attributeValue switch
                {
                    "LIGHTSTEEL" => "SPORTGREY",
                    "SHAMROCKGREEN" => "IRISHGREEN",
                    "DEEPROYAL" => "ROYAL",
                    "DEEPFOREST" => "FORESTGREEN",
                    "CHARCOALHEATHER" => "DARKHEATHER",
                    "PALEPINK" => "LIGHTPINK",
                    "TEAL" => "SAPPHIRE",
                    "DEEPRED" => "RED",
                    "FATIGUEGREEN" => "MILITARYGREEN",
                    "WOWPINK" => "HELICONIA",
                    "CARDINAL" => "CARDINALRED",
                    "DARKGREY" or "SMOKEGREY" => "CHARCOAL",
                    _ => attributeValue
                };
            }
        }
        orderItem.sku = $"{style}.{size}.{color}";
        foundProduct = await _unitOfWork.Products.FilterOneAsync(x => x.Sku == orderItem.sku);
        orderItem.Product = foundProduct;
        return orderItem;
    }

    public OrderItem ConvertShopifyItemToOrderItem(LineItem li)
    {
        List<OrderItem.OrderItemOption> itemOptions = [];
        decimal weightInGrams = li.Grams;
        decimal weightInOunces = weightInGrams / 28.3495m;

        var orderItem = new OrderItem
        {
            orderItemId = li.Id,
            lineItemKey = li.Id.ToString(),
            sku = li.Sku,
            name = li.Title,
            imageUrl = null,
            quantity = li.Quantity,
            unitPrice = 0,
            taxAmount = 0,
            shippingAmount = 0,
            productId = li.ProductId,
            fulfillmentSku = li.Sku,
            adjustment = false,
            upc = li.Sku,
            createDate = DateTime.Now,
            modifyDate = DateTime.Now,
            weight = new OrderWeight
            {
                value = weightInOunces,
                units = OrderWeight.Units.ounces,
                WeightUnits = (int)OrderWeight.Units.ounces
            }
        };

        return orderItem;
    }

    public OrderItem ConvertZazzleItemToOrderItem(ZazzleOrder.ZazzleLineItem zazzleLineItem)
    {
        List<OrderItem.OrderItemOption> itemOptions = [];
        foreach (string attribute in zazzleLineItem.LineItemAttributes.Split('&'))
        {
            string[] parts = attribute.Split('=');
            itemOptions.Add(new OrderItem.OrderItemOption
            {
                Name = parts[0],
                value = parts[1].Replace("&amp;", "&")
            });
        }
        foreach (var preview in zazzleLineItem.Previews)
        {
            itemOptions.Add(new OrderItem.OrderItemOption
            {
                Name = preview.PreviewDescription,
                value = preview.PreviewUrl
            });
        }
        foreach (var printfile in zazzleLineItem.PrintFiles)
        {
            itemOptions.Add(new OrderItem.OrderItemOption
            {
                Name = printfile.PrintFilesDescription,
                value = printfile.PrintFilesUrl
            });
        }
        var orderItem = new OrderItem
        {
            orderItemId = zazzleLineItem.LineItemId,
            sku = zazzleLineItem.LineItemType,
            fulfillmentSku = zazzleLineItem.LineItemAttributes,
            name = zazzleLineItem.Description,
            quantity = zazzleLineItem.Quantity,
            unitPrice = 0,
            taxAmount = 0,
            shippingAmount = 0,
            productId = zazzleLineItem.ProductId,
            createDate = DateTime.Now,
            modifyDate = DateTime.Now,
            options = itemOptions,
            imageUrl = zazzleLineItem.Previews.FirstOrDefault().PreviewUrl,
        };
        return orderItem;
    }

    public async Task<List<OrderItem>> CustomProductSkuConversion(OrderItem orderItem)
    {
        List<OrderItem> convertedOrderItems = [];
        string itemSku = orderItem.sku;
        if (!itemSku.Contains('-') || !itemSku.Contains('_'))
        {
            convertedOrderItems.Add(orderItem);
            return convertedOrderItems;
        }
        DateTime localDateTime = DateTime.Now;
        TimeZoneInfo centralZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(localDateTime.ToUniversalTime(), centralZone);


        //            Match a single character present in the list below [a-zA-Z]
        //              {2,3} matches the previous token between 2 and 3 times, as many times as possible, giving back as needed (greedy)
        //                a-z matches a single character in the range between a (index 97) and z (index 122) (case sensitive)
        //                A-Z matches a single character in the range between A (index 65) and Z (index 90) (case sensitive)
        //              _ matches the character _ with index 9510 (5F16 or 1378) literally (case sensitive)
        //              \d matches a digit (equivalent to [0-9])
        //                {1,3} matches the previous token between 1 and 3 times, as many times as possible, giving back as needed (greedy)
        //           EX Match: KD_1-GW_2
        Regex regex = new("([a-zA-Z]{2,3}_\\d{1,3})");
        Product foundProduct = new();
        foreach (string skuWQty in regex.Matches(itemSku).Select(match => match.Value).ToList())
        {
            foundProduct = skuWQty[..3].Replace("_", string.Empty) switch
            {
                //CS = Chips Salty/Original
                "CS" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "PCHIPBAG03ZSO01"),
                //CB = Chips BBQ
                "CB" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "PCHIPBAGBBQSO01"),
                //KD = Kinsale decanter 24oz
                "KD" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "WSKDCTKSL24Z"),
                //AD = Antique decanter 24oz
                "AD" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "WSKDCTANT24Z"),
                //CD = Capital decanter 24oz
                "CD" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "WSKDCTCAP24Z"),
                //SGW = Square wiskey glass
                "SGW" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "GW2339"),
                //GW = Lowball glasses
                "GW" => await _unitOfWork.Products.FilterOneAsync(x => x.Sku == "GW2338"),

                _ => throw new Exception("Product not found for sku: " + skuWQty),
            };
            OrderItem tempOrderItem = new()
            {
                Product = foundProduct,
                ERPProductId = foundProduct.ProductId,
                sku = foundProduct.Sku,
                fulfillmentSku = foundProduct.Sku,
                name = foundProduct.Description,
                productId = foundProduct.ProductId,
                quantity = orderItem.quantity * int.Parse(skuWQty.Split('_')[1]),
                ERPOrderId = orderItem.ERPOrderId,
                lineItemKey = orderItem.lineItemKey,
                orderItemId = orderItem.orderItemId,
                imageUrl = orderItem.imageUrl,
                weight = orderItem.weight,
                unitPrice = (orderItem.unitPrice > 0) ? orderItem.unitPrice / int.Parse(skuWQty.Split('_')[1]) : 0,
                taxAmount = orderItem.taxAmount,
                shippingAmount = orderItem.shippingAmount,
                warehouseLocation = orderItem.warehouseLocation,
                adjustment = orderItem.adjustment,
                upc = orderItem.upc,
                createDate = now,
                modifyDate = now,
                options = []
            };
            tempOrderItem.options.AddRange(orderItem.options);
            if (itemSku.EndsWith("EXP"))
                tempOrderItem.sku += "-EXP";

            convertedOrderItems.Add(tempOrderItem);
        }
        return convertedOrderItems;
    }

}
