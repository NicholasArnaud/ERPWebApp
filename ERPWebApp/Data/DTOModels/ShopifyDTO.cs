using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ERPWebApp.Data.DTOModels
{
    public class ShopifyDTO
    {
        [JsonPropertyName("id")]
        public long OrderId { get; set; }

        [JsonPropertyName("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonPropertyName("app_id")]
        public long AppId { get; set; }

        [JsonPropertyName("browser_ip")]
        public string BrowserIp { get; set; }

        [JsonPropertyName("buyer_accepts_marketing")]
        public bool BuyerAcceptsMarketing { get; set; }

        [JsonPropertyName("cancel_reason")]
        public string CancelReason { get; set; }

        [JsonPropertyName("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [JsonPropertyName("cart_token")]
        public string CartToken { get; set; }

        [JsonPropertyName("checkout_id")]
        public long CheckoutId { get; set; }

        [JsonPropertyName("checkout_token")]
        public string CheckoutToken { get; set; }

        [JsonPropertyName("client_details")]
        public ClientDetails ClientDetails { get; set; }

        [JsonPropertyName("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonPropertyName("company")]
        public Company Company { get; set; }

        [JsonPropertyName("confirmation_number")]
        public string ConfirmationNumber { get; set; }

        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }

        [JsonPropertyName("contact_email")]
        public string ContactEmail { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("current_subtotal_price")]
        public string CurrentSubtotalPrice { get; set; }

        [JsonPropertyName("current_subtotal_price_set")]
        public MoneySet CurrentSubtotalPriceSet { get; set; }

        [JsonPropertyName("current_total_additional_fees_set")]
        public MoneySet CurrentTotalAdditionalFeesSet { get; set; }

        [JsonPropertyName("current_total_discounts")]
        public string CurrentTotalDiscounts { get; set; }

        [JsonPropertyName("current_total_discounts_set")]
        public MoneySet CurrentTotalDiscountsSet { get; set; }

        [JsonPropertyName("current_total_duties_set")]
        public MoneySet CurrentTotalDutiesSet { get; set; }

        [JsonPropertyName("current_total_price")]
        public string CurrentTotalPrice { get; set; }

        [JsonPropertyName("current_total_price_set")]
        public MoneySet CurrentTotalPriceSet { get; set; }

        [JsonPropertyName("current_total_tax")]
        public string CurrentTotalTax { get; set; }

        [JsonPropertyName("current_total_tax_set")]
        public MoneySet CurrentTotalTaxSet { get; set; }

        [JsonPropertyName("customer_locale")]
        public string CustomerLocale { get; set; }

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; }

        [JsonPropertyName("discount_codes")]
        public List<object> DiscountCodes { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("estimated_taxes")]
        public bool EstimatedTaxes { get; set; }

        [JsonPropertyName("financial_status")]
        public string FinancialStatus { get; set; }

        [JsonPropertyName("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonPropertyName("landing_site")]
        public string LandingSite { get; set; }

        [JsonPropertyName("landing_site_ref")]
        public string LandingSiteRef { get; set; }

        [JsonPropertyName("location_id")]
        public long? LocationId { get; set; }

        [JsonPropertyName("merchant_of_record_app_id")]
        public long? MerchantOfRecordAppId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("note_attributes")]
        public List<object> NoteAttributes { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("order_number")]
        public int OrderNumber { get; set; }

        [JsonPropertyName("order_status_url")]
        public string OrderStatusUrl { get; set; }

        [JsonPropertyName("original_total_additional_fees_set")]
        public MoneySet OriginalTotalAdditionalFeesSet { get; set; }

        [JsonPropertyName("original_total_duties_set")]
        public MoneySet OriginalTotalDutiesSet { get; set; }

        [JsonPropertyName("payment_gateway_names")]
        public List<string> PaymentGatewayNames { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("po_number")]
        public string PoNumber { get; set; }

        [JsonPropertyName("presentment_currency")]
        public string PresentmentCurrency { get; set; }

        [JsonPropertyName("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        [JsonPropertyName("referring_site")]
        public string ReferringSite { get; set; }

        [JsonPropertyName("source_identifier")]
        public string SourceIdentifier { get; set; }

        [JsonPropertyName("source_name")]
        public string SourceName { get; set; }

        [JsonPropertyName("source_url")]
        public string SourceUrl { get; set; }

        [JsonPropertyName("subtotal_price")]
        public string SubtotalPrice { get; set; }

        [JsonPropertyName("subtotal_price_set")]
        public MoneySet SubtotalPriceSet { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; }

        [JsonPropertyName("tax_exempt")]
        public bool TaxExempt { get; set; }

        [JsonPropertyName("tax_lines")]
        public List<object> TaxLines { get; set; }

        [JsonPropertyName("taxes_included")]
        public bool TaxesIncluded { get; set; }

        [JsonPropertyName("test")]
        public bool Test { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("total_discounts")]
        public string TotalDiscounts { get; set; }

        [JsonPropertyName("total_discounts_set")]
        public MoneySet TotalDiscountsSet { get; set; }

        [JsonPropertyName("total_line_items_price")]
        public string TotalLineItemsPrice { get; set; }

        [JsonPropertyName("total_line_items_price_set")]
        public MoneySet TotalLineItemsPriceSet { get; set; }

        [JsonPropertyName("total_outstanding")]
        public string TotalOutstanding { get; set; }

        [JsonPropertyName("total_price")]
        public string TotalPrice { get; set; }

        [JsonPropertyName("total_price_set")]
        public MoneySet TotalPriceSet { get; set; }

        [JsonPropertyName("total_shipping_price_set")]
        public MoneySet TotalShippingPriceSet { get; set; }

        [JsonPropertyName("total_tax")]
        public string TotalTax { get; set; }

        [JsonPropertyName("total_tax_set")]
        public MoneySet TotalTaxSet { get; set; }

        [JsonPropertyName("total_tip_received")]
        public string TotalTipReceived { get; set; }

        [JsonPropertyName("total_weight")]
        public int TotalWeight { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        [JsonPropertyName("billing_address")]
        public Address BillingAddress { get; set; }

        [JsonPropertyName("customer")]
        public Customer Customer { get; set; }

        [JsonPropertyName("discount_applications")]
        public List<object> DiscountApplications { get; set; }

        [JsonPropertyName("fulfillments")]
        public List<object> Fulfillments { get; set; }

        [JsonPropertyName("line_items")]
        public List<LineItem> LineItems { get; set; }

        [JsonPropertyName("payment_terms")]
        public string PaymentTerms { get; set; }

        [JsonPropertyName("refunds")]
        public List<object> Refunds { get; set; }

        [JsonPropertyName("shipping_address")]
        public Address ShippingAddress { get; set; }

        [JsonPropertyName("shipping_lines")]
        public List<ShippingLine> ShippingLines { get; set; }
    }

    public class ClientDetails
    {
        [JsonPropertyName("accept_language")]
        public string AcceptLanguage { get; set; }

        [JsonPropertyName("browser_height")]
        public int? BrowserHeight { get; set; }

        [JsonPropertyName("browser_ip")]
        public string BrowserIp { get; set; }

        [JsonPropertyName("browser_width")]
        public int? BrowserWidth { get; set; }

        [JsonPropertyName("session_hash")]
        public string SessionHash { get; set; }

        [JsonPropertyName("user_agent")]
        public string UserAgent { get; set; }
    }

    public class Company
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("location_id")]
        public long LocationId { get; set; }
    }

    public class MoneySet
    {
        [JsonPropertyName("shop_money")]
        public Money ShopMoney { get; set; }

        [JsonPropertyName("presentment_money")]
        public Money PresentmentMoney { get; set; }
    }

    public class Money
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("address1")]
        public string Address1 { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("zip")]
        public string Zip { get; set; }

        [JsonPropertyName("province")]
        public string Province { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("address2")]
        public string Address2 { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("province_code")]
        public string ProvinceCode { get; set; }
    }

    public class Customer
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonPropertyName("multipass_identifier")]
        public string MultipassIdentifier { get; set; }

        [JsonPropertyName("tax_exempt")]
        public bool TaxExempt { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("email_marketing_consent")]
        public EmailMarketingConsent EmailMarketingConsent { get; set; }

        [JsonPropertyName("sms_marketing_consent")]
        public SmsMarketingConsent SmsMarketingConsent { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("tax_exemptions")]
        public List<object> TaxExemptions { get; set; }

        [JsonPropertyName("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }
    }

    public class EmailMarketingConsent
    {
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("opt_in_level")]
        public string OptInLevel { get; set; }

        [JsonPropertyName("consent_updated_at")]
        public DateTime? ConsentUpdatedAt { get; set; }
    }

    public class SmsMarketingConsent
    {
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("opt_in_level")]
        public string OptInLevel { get; set; }

        [JsonPropertyName("consent_updated_at")]
        public DateTime? ConsentUpdatedAt { get; set; }

        [JsonPropertyName("consent_collected_from")]
        public string ConsentCollectedFrom { get; set; }
    }

    public class LineItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonPropertyName("attributed_staffs")]
        public List<object> AttributedStaffs { get; set; }

        [JsonPropertyName("current_quantity")]
        public int CurrentQuantity { get; set; }

        [JsonPropertyName("fulfillable_quantity")]
        public int FulfillableQuantity { get; set; }

        [JsonPropertyName("fulfillment_service")]
        public string FulfillmentService { get; set; }

        [JsonPropertyName("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonPropertyName("gift_card")]
        public bool GiftCard { get; set; }

        [JsonPropertyName("grams")]
        public int Grams { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("price_set")]
        public MoneySet PriceSet { get; set; }

        [JsonPropertyName("product_exists")]
        public bool ProductExists { get; set; }

        [JsonPropertyName("product_id")]
        public long? ProductId { get; set; }

        [JsonPropertyName("properties")]
        public List<object> Properties { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("requires_shipping")]
        public bool RequiresShipping { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; }

        [JsonPropertyName("taxable")]
        public bool Taxable { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("total_discount")]
        public string TotalDiscount { get; set; }

        [JsonPropertyName("total_discount_set")]
        public MoneySet TotalDiscountSet { get; set; }

        [JsonPropertyName("variant_id")]
        public long? VariantId { get; set; }

        [JsonPropertyName("variant_inventory_management")]
        public string VariantInventoryManagement { get; set; }

        [JsonPropertyName("variant_title")]
        public string VariantTitle { get; set; }

        [JsonPropertyName("vendor")]
        public string Vendor { get; set; }

        [JsonPropertyName("tax_lines")]
        public List<object> TaxLines { get; set; }

        [JsonPropertyName("duties")]
        public List<object> Duties { get; set; }

        [JsonPropertyName("discount_allocations")]
        public List<object> DiscountAllocations { get; set; }
    }

    public class ShippingLine
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("carrier_identifier")]
        public string CarrierIdentifier { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("discounted_price")]
        public string DiscountedPrice { get; set; }

        [JsonPropertyName("discounted_price_set")]
        public MoneySet DiscountedPriceSet { get; set; }

        [JsonPropertyName("is_removed")]
        public bool IsRemoved { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("price_set")]
        public MoneySet PriceSet { get; set; }

        [JsonPropertyName("requested_fulfillment_service_id")]
        public long? RequestedFulfillmentServiceId { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("tax_lines")]
        public List<object> TaxLines { get; set; }

        [JsonPropertyName("discount_allocations")]
        public List<object> DiscountAllocations { get; set; }
    }
}