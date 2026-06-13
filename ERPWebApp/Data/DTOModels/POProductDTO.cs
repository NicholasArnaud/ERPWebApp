namespace ERPWebApp.Data.DTOModels
{
    public class POProductDTO
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public int TotalRecieved { get; set; }

    }
}
