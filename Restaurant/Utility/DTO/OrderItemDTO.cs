using Restaurant.DataAccessLayer.Models;

namespace Restaurant.Utility
{
    public class OrderItemDTO
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ItemId { get; set; }
        public long ItemTypeId { get; set; }
        public long? CategoryId { get; set; }
        public long OrderQty { get; set; }
        public long UomId { get; set; }
        public decimal? Price { get; set; }

    }
}
