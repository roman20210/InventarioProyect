namespace Inv.Microservice.Api.Logic.Reports.Services
{
    public  interface IReportService
    {
        // Método para generar reporte de inventario en PDF o Excel
        Task<byte[]> GenerateInventoryReportAsync(string format);
    }
}
