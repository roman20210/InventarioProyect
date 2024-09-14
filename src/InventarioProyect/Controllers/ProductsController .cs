using Inv.Microservice.Api.Logic.Products.Services;
using Inv.Microservice.Api.Logic.Reports.Services;
using Inv.Microservice.Api.Login.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProyect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IReportService _reportService;
        public ProductsController(IProductService productService, IReportService reportService)
        {
            _productService = productService;
            _reportService = reportService;
        }

        // Obtener todo el inventario
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // Buscar producto por id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // Añadir nuevo producto
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newProduct = await _productService.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        // Editar producto existente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedProduct = await _productService.UpdateProductAsync(id, product);
            if (updatedProduct == null) return NotFound();

            return Ok(updatedProduct);
        }

        // Eliminar producto
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        // Buscar productos por palabra clave
        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            var products = await _productService.SearchProductsAsync(keyword);
            return Ok(products);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GenerateInventoryReport([FromQuery] string format)
        {
            var report = await _reportService.GenerateInventoryReportAsync(format);
            if (report == null) return BadRequest("Formato no soportado");

            var contentType = format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"Reporte_Inventario_{DateTime.Now:yyyyMMdd}.{format}";

            return File(report, contentType, fileName);
        }
        
    }

}