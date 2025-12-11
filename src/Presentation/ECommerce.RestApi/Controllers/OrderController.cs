namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        public readonly OrderService _orderService;
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }
        //CREATE
        [HttpPost]
        public async Task<IActionResult> Create (OrderCreateDto dto)
        {
            var order = await _orderService.CreateOrderAsync(dto.CustomerId, dto.OrderItems);
            return Ok(order);
        }
        //READ - Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new {message = "Sipariş Bulunamadı"});
            }
            return Ok(order);
        }
        //READ - Get all by Customer
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetAllByCustomer(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }
        //READ - Get by Company
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetAllByCompany(int companyId)
        {
            var orders = await _orderService.GetOrdersByCompanyIdAsync(companyId);
            return Ok(orders);
        }
        //UPDATE - Change Status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] )
        {
            var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            if (updatedOrder == null)
            {
                return NotFound(new { message = "Sipariş Bulunamadı" });
            }
            return Ok(updatedOrder);
        }
    }
}