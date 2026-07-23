using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Medida_c
    {
        public Medida_c()
        {
            UnidadMedidas = new HashSet<UnidadMedida_c>();
        }

        [Key]
        public long Medida_cId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreMedida { get; set; } = null!;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<UnidadMedida_c> UnidadMedidas { get; set; }
    }
}
