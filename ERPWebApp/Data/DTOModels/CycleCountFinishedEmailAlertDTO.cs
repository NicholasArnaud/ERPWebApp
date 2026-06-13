namespace ERPWebApp.Data.DTOModels
{
    public class CycleCountFinishedEmailAlertDTO
    {
        public string Location { get; set; }
        public string Sku { get; set; }
        public int PreviousQuantity { get; set; }
        public int? NewQuantity { get; set; }
    }
}
