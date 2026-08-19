using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Login
    {
        [Key]
        public long LoginId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string Username { get; set; } = null!;

        [StringLength(150)]
        public string Password { get; set; } = null!;

        public string Permisos { get; set; } = "{ 'TipoUsuario':'Invitado', 'TipoRol':'Invitado' }";
        public bool RememberMe { get; set; } = false;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        [ForeignKey("Usuario")]
        public long Documento { get; set; }
        public virtual Usuario? Usuarios { get; set; } = null;

    }
}
