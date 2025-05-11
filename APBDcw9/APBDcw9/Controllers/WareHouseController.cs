using APBDcw9.Exceptions;
using APBDcw9.Modeks;
using APBDcw9.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBDcw9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WareHouseController : ControllerBase
    {
        private readonly IwareHouseService _service;

        public WareHouseController(IwareHouseService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductTowareHGouse([FromBody] WarehouseRequest warehouseRequest)
        {
            if (warehouseRequest == null)
            {
                return BadRequest("Dane wejściowe są niepoprawne.");
            }

            Console.WriteLine($"ProduktId: {warehouseRequest.IdProduct},aaaaaa" +
                              $" WarehouseId: {warehouseRequest.IdWarehouse}, Amount: {warehouseRequest.Amount}, CreatedAt: {warehouseRequest.CreatedAt}");
            try
            {
                int productWarehouseId = await _service.add(warehouseRequest);
                return Ok(productWarehouseId);
            }
            catch (ConflictEx ex) 
            {
               return Conflict(ex.Message);
            }
            catch (NotFoundEx ex) 
            {
               return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Some internal error");
            }
        }

        [HttpPost("procedure")]
        public async Task<IActionResult> AddProductToWarehousebyProcedure([FromBody] WarehouseRequest warehouseRequest)
        {
            try
            {
                await _service.ProcedureAsync(warehouseRequest.IdProduct, warehouseRequest.IdWarehouse,
                    warehouseRequest.Amount, warehouseRequest.CreatedAt);
                return Ok(warehouseRequest.IdProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Some internal procedure error");
            }
        }
    }
}
