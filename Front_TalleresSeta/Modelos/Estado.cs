using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Estado
    {
        public Estado()
        {
            Usuarios = new HashSet<Usuario>();
            Vehiculos = new HashSet<Vehiculo>();
        }

        [Key]
        public long EstadoId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreEstado { get; set; } = null!;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<Usuario> Usuarios { get; set; }
        public virtual ICollection<Vehiculo> Vehiculos { get; set; }
    }
}
