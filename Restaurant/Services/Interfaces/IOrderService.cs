using Microsoft.AspNetCore.Mvc;
using Restaurant.Utility;

namespace Restaurant.Services
{
    public interface IOrderService
    {
        Task<Object> GetListAsync(int pageNumber, int pageSize);
        Task<OrderDTO> AddAsync(OrderDTO dto);
        Task<OrderDTO> UpdateAsync(OrderDTO dto);
        Task<OrderDTO> GetByIdAsync(long id);
        Task DeleteAsync(long id);
    }
}
