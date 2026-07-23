using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioGanancia
    {
        [Key]
        public long IdGanancia { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public Int64 CantVendida { get; set; } = 0;
        public Int64 Ganancia { get; set; } = 0;
        public Int64 PrecioCompraXuni { get; set; } = 0;
        public Int64 PrecioVentaXuni { get; set; } = 0;


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto? InventarioEntradaProductos { get; set; } = null;


        [ForeignKey("InventarioSalidaProducto")]
        public long InventarioSalidaProductoId { get; set; }
        public virtual InventarioSalidaProducto? InventarioSalidaProductos { get; set; } = null;


        [ForeignKey("InventarioLote")]
        public long IdLote { get; set; }
        public virtual InventarioLote? InventarioLotes { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;

    }
}
