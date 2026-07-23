using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioMarca
    {
        public InventarioMarca()
        {
            InventarioEntradaProductos = new List<InventarioEntradaProducto>();
        }

        [Key]
        public long InventarioMarcaId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreMarca { get; set; } = null!;


        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioEntradaProducto> InventarioEntradaProductos { get; set; }
    }
}
