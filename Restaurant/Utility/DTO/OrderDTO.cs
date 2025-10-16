
using Restaurant.DataAccessLayer.Models;

namespace Restaurant.Utility
{
   
    public class OrderDTO
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
        //public List<OrderItemDTO> OrderItem { get; set; } = new List<OrderItemDTO>();
        public ICollection<OrderItemDTO> OrderItem { get; set; } = new List<OrderItemDTO>();
    }
}
