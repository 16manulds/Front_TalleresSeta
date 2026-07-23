using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioSalidaProducto
    {
        public InventarioSalidaProducto()
        {
            InventarioGanancias = new HashSet<InventarioGanancia>();
        }

        [Key]
        public long IdInventarioSalidaProducto { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public Int64 PrecioFinalXuni { get; set; } = 0;
        public Int64 PrecioFinaVenta { get; set; } = 0;
        public Int64 PrecioFinalVentaPagado { get; set; } = 0;
        public int? CantVendidos { get; set; } = 0;
        public int? CantDevoluciones { get; set; } = 0;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto? InventarioEntradaProductos { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioGanancia> InventarioGanancias { get; set; } = null!;
    }
}
