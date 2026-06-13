namespace ERPWebApp.Models.Inventory
{
    public class ProductTag
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int TagId { get; set; }
        public ProductTagsRegistry Tag { get; set; }
    }
}