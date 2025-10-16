//using Restaurant.DataAccessLayer;
//using Restaurant.DataAccessLayer.Models;
//using Restaurant.Repositories;

//namespace Restaurant.Repositories
//{
//    public class OrderRepository: IOrderRepository, IGenericRepository<Order>
//    {
//        private RestaurantDbContext _context;

//        public OrderRepository(RestaurantDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Order> AddAsync(Order entity)
//        {
//            try
//            {
//                await _context.Order.AddAsync(entity);
//                return entity;
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//        public async Task SaveChangesAsync()
//        {
//            try
//            {
//                await _context.SaveChangesAsync();

//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//    }
//}
