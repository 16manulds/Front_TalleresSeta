using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioSubCategoria
    {
        public InventarioSubCategoria()
        {
            InventarioEntradaProductos = new HashSet<InventarioEntradaProducto>();
        }

        [Key]
        public long InventarioSubCategoriaId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreSubCategoria { get; set; } = null!;


        [ForeignKey("InventarioCategoria")]
        public long InventarioCategoriaId { get; set; }
        public virtual InventarioCategoria? InventarioCategorias { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioEntradaProducto> InventarioEntradaProductos { get; set; }
    }
}
