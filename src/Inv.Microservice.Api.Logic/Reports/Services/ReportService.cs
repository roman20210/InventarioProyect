using Inv.Microservice.Api.Logic.Products.Services;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf; 
using Inv.Microservice.Api.Login.Entities;

namespace Inv.Microservice.Api.Logic.Reports.Services
{
    public class ReportService : IReportService
    {
        private readonly IProductService _productService;
        public ReportService(IProductService productService)
        {
            _productService = productService;
        }

        // Método para generar el reporte en PDF o Excel
        public async Task<byte[]> GenerateInventoryReportAsync(string format)
        {
            var products = await _productService.GetAllProductsAsync(); // Obtener todos los productos

            if (format.ToLower() == "pdf")
            {
                return GeneratePdfReport(products);
            }
            else if (format.ToLower() == "xlsx")
            {
                return GenerateExcelReport(products);
            }
            else
            {
                return null;
            }
        }

        // Generar reporte en PDF
        private byte[] GeneratePdfReport(IEnumerable<Product> products)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate());
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Título del reporte
                document.Add(new Paragraph("Reporte de Inventario"));

                // Crear una tabla para los datos del inventario
                PdfPTable table = new PdfPTable(5); // 3 columnas (Id, Nombre, Precio)
                table.AddCell("Id");
                table.AddCell("Nombre");
                table.AddCell("Precio");
                table.AddCell("Categoria");
                table.AddCell("Stock");

                foreach (var product in products)
                {
                    table.AddCell(product.Id.ToString());
                    table.AddCell(product.Name);
                    table.AddCell(product.Price.ToString("C")); // Formato moneda
                    table.AddCell(product.Category);
                    table.AddCell(product.Stock.ToString());

                }

                document.Add(table);
                document.Close();

                return ms.ToArray();
            }
        }

        // Generar reporte en Excel
        private byte[] GenerateExcelReport(IEnumerable<Product> products)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Inventario");
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "Precio";
                worksheet.Cells[1, 4].Value = "Categoria";
                worksheet.Cells[1, 5].Value = "Stock";

                int row = 2;
                foreach (var product in products)
                {
                    worksheet.Cells[row, 1].Value = product.Id;
                    worksheet.Cells[row, 2].Value = product.Name;
                    worksheet.Cells[row, 3].Value = product.Price;
                    worksheet.Cells[row, 4].Value = product.Category;
                    worksheet.Cells[row, 5].Value = product.Stock;

                    row++;
                }

                return package.GetAsByteArray();
            }
        }
    }
}
