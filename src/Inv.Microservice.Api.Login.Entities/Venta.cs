using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Inv.Microservice.Api.Login.Entities
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Producto")]
        public int ProductoId { get; set; }

        public Product Producto { get; set; }

        public int Cantidad { get; set; }

        public DateTime FechaVenta { get; set; }
    }
}
