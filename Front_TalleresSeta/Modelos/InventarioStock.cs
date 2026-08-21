using Front_TalleresSeta.Modelos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioStock
    {
        [Key]
        public long InventarioStockId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistroInicial { get; set; } = DateTime.Now;
        public DateTime? FechaRegistroUpdate { get; set; } = null;
        public int CantStock { get; set; } = 0;
        public int? CantVendidos { get; set; } = 0;


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto InventarioEntradaProductos { get; set; } = null!;

        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller Talleres { get; set; } = null!;


    }
}
