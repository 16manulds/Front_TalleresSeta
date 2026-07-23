using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Color_c
    {
        public Color_c()
        {
            InventarioEntradaProductos = new HashSet<InventarioEntradaProducto>();
        }

        [Key]
        public long Color_cId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreColor { get; set; } = "Sin color";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioEntradaProducto> InventarioEntradaProductos { get; set; }
    }
}
