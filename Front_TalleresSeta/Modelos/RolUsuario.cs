using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public partial class RolUsuario
    {
        public RolUsuario()
        {
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        public long RolUsuarioId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string NombreRol { get; set; } = null!;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
