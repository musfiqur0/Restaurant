using Restaurant.DataAccessLayer.Models;

namespace Restaurant.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order entity);
        Task SaveChangesAsync();
    }
}
