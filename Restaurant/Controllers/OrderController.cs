using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Services;
using Restaurant.Utility;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] OrderDTO dto)
        {
            try
            {
                return Ok(await _orderService.AddAsync(dto));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("GetList")]
        public async Task<IActionResult> GetList(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _orderService.GetListAsync(pageNumber, pageSize);
            return Ok(result);
        }
        [HttpPut]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] OrderDTO dto)
        {
            try
            {
                return Ok(await _orderService.UpdateAsync(dto));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("GetById{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                return Ok(await _orderService.GetByIdAsync(id));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpDelete]
        [Route("Delete{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _orderService.DeleteAsync(id);
            return Ok();
        }
    }
}

