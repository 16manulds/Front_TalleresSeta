using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioCategoria
    {
        public InventarioCategoria()
        {
            InventarioSubCategorias = new HashSet<InventarioSubCategoria>();
        }

        [Key]
        public long InventarioCategoriaId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreCategoria { get; set; } = null!;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioSubCategoria> InventarioSubCategorias { get; set; }
    }
}
