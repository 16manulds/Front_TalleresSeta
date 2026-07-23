using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioDevolucion
    {
        public long InventarioDevolucionId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaDevolucion { get; set; } = DateTime.Now;
        public int CantDevuelta { get; set; } = 0;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto? InventarioEntradaProductos { get; set; } = null;
    }
}
