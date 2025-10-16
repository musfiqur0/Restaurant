using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurant.DataAccessLayer.Models;
using Restaurant.Repositories;
using Restaurant.Services;
using Restaurant.Utility;

namespace Restaurant.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenericRepository<Order> _orderRepository;

        public OrderService(IGenericRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDTO> AddAsync(OrderDTO dto)
        {
            try
            {
                Order entity = new Order();
                DynamicMapper.Map(dto, entity);
                var data = await _orderRepository.InsertAsync(entity);
                var SaveStatus = await _orderRepository.SaveAsync();
                if (SaveStatus == 0)
                    throw new Exception("Something error");
                DynamicMapper.Map(data, dto);
                return dto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Object> GetListAsync(int pageNumber, int pageSize)
        {
            try
            {
                var data = _orderRepository.GetAll().Include(o => o.OrderItem);
                var totalCount = await data.CountAsync();
                var propertyMappings = new Dictionary<string, string>
                {
                    { "OrderItem", "OrderItemList" },
                    { "Customer", "CustomerInfo" }, // Example: if you have more nested objects
                    { "DeliveryAddress", "AddressDetails" }, // Another example
                    // Add as many as needed per query
                };
                var retrievedData = await data.OrderBy(x => x.OrderNo)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .SelectTo<Order, OrderDTO>(propertyMappings) // ✨ Pass mappings!
                    .ToListAsync();
                var result = new PaginatedList<object>(retrievedData, totalCount, pageNumber, pageSize);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<OrderDTO> UpdateAsync(OrderDTO dto)
        {
            try
            {
                var entity = await _orderRepository.GetByIdAsync(dto.Id);
                if (entity == null)
                    throw new Exception("Something error");
                DynamicMapper.Map(dto, entity);
                var data = await _orderRepository.UpdateAsync(entity);
                var SaveStatus = await _orderRepository.SaveAsync();
                if (SaveStatus == 0)
                    throw new Exception("Something error");
                DynamicMapper.Map(data, dto);
                return dto;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<OrderDTO> GetByIdAsync(long id)
        {
            try
            {
                //var data = await _orderRepository.GetByIdAsync(id); // without nested child
                var query = _orderRepository.GetAll().Include(o => o.OrderItem);
                var data = query.Where(x => x.Id == id).FirstOrDefault();
                if (data == null)
                {
                    throw new Exception("Something error");
                }
                var dto = new OrderDTO();
                DynamicMapper.Map(data, dto);

                return dto;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task DeleteAsync(long id)
        {
            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveAsync();
        }
    }
}
