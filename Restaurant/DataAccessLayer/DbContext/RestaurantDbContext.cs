using Microsoft.EntityFrameworkCore;
using Restaurant.DataAccessLayer.Models;

namespace Restaurant.DataAccessLayer
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
    }
}
