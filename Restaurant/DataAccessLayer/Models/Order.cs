namespace Restaurant.DataAccessLayer.Models
{
    public class Order
    {
        public long Id { get; set; }
        public long OrderNo { get; set; }
        public long CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? OrderTime { get; set; }
        public DateTime DeliveryDate { get; set; }
        public long DeliveryAddressId { get; set; }
        public string? Remarks { get; set; }
        public long? EntryBy { get; set; }
        public virtual ICollection<OrderItem> OrderItem { get; set; } = new List<OrderItem>();
    }
}
