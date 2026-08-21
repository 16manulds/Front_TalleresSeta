using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioGanancia
    {
        [Key]
        public long GananciaId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public Int64 ValorGanancia { get; set; } = 0;


        [ForeignKey("InventarioSalidaProducto")]
        public long InventarioSalidaProductoId { get; set; }
        public virtual InventarioSalidaProducto InventarioSalidaProductos { get; set; } = null!;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller Talleres { get; set; } = null!;

    }
}
