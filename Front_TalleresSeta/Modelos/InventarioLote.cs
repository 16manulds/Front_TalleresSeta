using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioLote
    {
        public InventarioLote()
        {
            InventarioGanancias = new HashSet<InventarioGanancia>();
        }

        [Key]
        public long IdLote { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public Int64 PrecioCompraXuni { get; set; } = 0;
        public Int64 PrecioVentaXuni { get; set; } = 0;
        public int CantIngresan { get; set; } = 0;
        public int CantRestante { get; set; } = 0;


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto? InventarioEntradaProductos { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioGanancia> InventarioGanancias { get; set; } = null!;
    }
}
