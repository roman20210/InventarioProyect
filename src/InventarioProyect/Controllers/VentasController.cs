using Inv.Microservice.Api.Login.Entities;
using Inv.Microservice.Api.Login.Data.Transfer.Object;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inv.Microservice.Api.Login.Entities.Core.Startup.DbContext;

namespace InventarioProyect.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public VentasController(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        [HttpPost]
        public async Task<IActionResult> RealizarVenta([FromBody] VentaRequest ventaRequest)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(); // Transacción para asegurar consistencia

            try
            {
                foreach (var productoVenta in ventaRequest.Productos)
                {
                    var producto = await _dbContext.product.FirstOrDefaultAsync(p => p.Id == productoVenta.ProductoId);

                    if (producto == null)
                    {
                        return NotFound(new { message = $"Producto con ID {productoVenta.ProductoId} no encontrado." });
                    }

                    if (producto.Stock < productoVenta.Cantidad)
                    {
                        return BadRequest(new { message = $"No hay suficiente cantidad disponible para el producto {producto.Name}." });
                    }

                    // Actualizar cantidad disponible
                    producto.Stock -= productoVenta.Cantidad;
                    _dbContext.product.Update(producto);

                    // Registrar la venta en la base de datos (puedes tener una tabla de ventas)
                    var venta = new Venta
                    {
                        ProductoId = producto.Id,
                        Cantidad = productoVenta.Cantidad,
                        FechaVenta = DateTime.Now,
                        // Otros datos como Usuario, TotalVenta, etc.
                    };

                    await _dbContext.Ventas.AddAsync(venta);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync(); // Confirmar la transacción

                return Ok(new { message = "Venta realizada con éxito." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Revertir la transacción si hay un error
                return StatusCode(500, $"Ocurrió un error durante la venta.{ex.Message}");
            }
        }
    }
}
