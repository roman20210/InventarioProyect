using Inv.Microservice.Api.Login.Entities;
using Inv.Microservice.Api.Login.Data.Transfer.Object;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inv.Microservice.Api.Login.Entities.Core.Startup.DbContext;

namespace InventarioProyect.Controllers
{
    /// <summary>
    /// Controlador de ventas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        /// <summary>
        /// Contiene la conexion a la base de datos
        /// </summary>
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Crea una nueva instacia de VentasController
        /// </summary>
        /// <param name="context">Conexion a la base de datos</param>
        public VentasController(ApplicationDbContext context)
        {
            _dbContext = context;
        }
       /// <summary>
       /// POST para enviar la venta a la base de datos y actualizar el stok disponible (pendiente crear tabla de informe de ventas)
       /// </summary>
       /// <param name="ventaRequest">Contiene la lista de los productos vendidos</param>
       /// <returns>El resultado de la accion representada en tarea</returns>
        [HttpPost]
        public async Task<IActionResult> RealizarVenta([FromBody] VentaRequest ventaRequest)
        {
            // Transacción para asegurar consistencia
            using var transaction = await _dbContext.Database.BeginTransactionAsync(); 

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

                    // Registrar la venta en la base de datos
                    var venta = new Venta
                    {
                        ProductoId = producto.Id,
                        Cantidad = productoVenta.Cantidad,
                        FechaVenta = DateTime.Now,
                    };

                    await _dbContext.Ventas.AddAsync(venta);
                }

                await _dbContext.SaveChangesAsync();
                // Confirmar la transacción
                await transaction.CommitAsync();

                return Ok(new { message = "Venta realizada con éxito." });
            }
            catch (Exception ex)
            {
                // Revertir la transacción si hay un error
                await transaction.RollbackAsync();
                return StatusCode(500, $"Ocurrió un error durante la venta.{ex.Message}");
            }
        }
    }
}
