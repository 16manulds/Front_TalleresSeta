using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class UnidadMedida_c
    {
        public UnidadMedida_c()
        {
            InventarioEntradaProductos = new HashSet<InventarioEntradaProducto>();
        }

        [Key]
        public long UnidadMedida_cId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string UnidadMedida { get; set; } = null!;


        [ForeignKey("Medida_c")]
        public long Medida_cId { get; set; } = 0;
        public virtual Medida_c? Medidas { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioEntradaProducto> InventarioEntradaProductos { get; set; }
    }
}
