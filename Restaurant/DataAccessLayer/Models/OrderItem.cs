namespace Restaurant.DataAccessLayer.Models
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ItemId { get; set; }
        public long ItemTypeId { get; set; }
        public long? CategoryId { get; set; }
        public long OrderQty { get; set; }
        public long UomId { get; set; }
        public decimal? Price { get; set; }
        public virtual Order Order { get; set; } = null!;
    }
}
