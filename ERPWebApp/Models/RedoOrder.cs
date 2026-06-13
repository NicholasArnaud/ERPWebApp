namespace ERPWebApp.Models
{
    public class RedoOrder
    {
        public int RedoOrderId { get; set; }
        public string OrderNumber { get; set; }
        public string RedoReason { get; set; }
        //public string Explanation { get; set; }
        //public int DaysOld { get; set; }
        //REMOVED FROM PRODUCT MODEL
        //public DepartmentType Department { get; set; }
        public DateTime DateReported { get; set; }
        public DateTime OrderDate { get; set; }
        public string ItemSku { get; set; }
        public int Quantity { get; set; }
        public string LoggedBy { get; set; }
    }
}
