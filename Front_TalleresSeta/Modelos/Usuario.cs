using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Usuario
    {
        public Usuario()
        {
            Vehiculos = new HashSet<Vehiculo>();
            Logins = new HashSet<Login>();
        }

        [Key]
        [Required(ErrorMessage = "Ingresa un documento valido")]
        public long Documento { get; set; } = 222222222222;
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
                
        [Required(ErrorMessage = "Ingresa un nombre al usuario")]
        [StringLength(30)]
        public string PrimerNombre { get; set; } = "Clientes";

        [StringLength(30)]
        public string? SegundoNombre { get; set; }

        [Required(ErrorMessage = "Ingresa un apellido al usuario")]
        [StringLength(30)]
        public string PrimerApellido { get; set; } = "Varios";

        [StringLength(30)]
        public string? SegundoApellido { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Selecciona un genero")]
        public int Sexo { get; set; } = 3; //1 Hombre, 2 Mujer, 3 Otro

        [StringLength(10)]
        public string? TelefonoFijo { get; set; }

        [Required(ErrorMessage = "Ingresa un número celular")]
        [StringLength(10)]
        public string TelefonoMovil { get; set; } = null!;

        [StringLength(300)]
        public string? DireccionPrincipal { get; set; }

        [StringLength(300)]
        public string? DireccionAlterna { get; set; }

        [StringLength(300)]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "Selecciona el estado del usuario")]
        [ForeignKey("Estado")]
        public long? EstadoId { get; set; }
        public virtual Estado? Estados { get; set; } = null;

        [Required(ErrorMessage = "Selecciona un tipo de usuario")]
        [ForeignKey("TipoUsuario")]
        public long TipoUsuarioId { get; set; }
        public virtual TipoUsuario TipoUsuarios { get; set; } = null!;


        [Required(ErrorMessage = "Selecciona un tipo de documento")]
        [ForeignKey("TipoDocumento")]
        public long TipoDocumentoId { get; set; }
        public virtual TipoDocumento TipoDocumentos { get; set; } = null!;


        [Required(ErrorMessage = "Selecciona un rol de usuario")]
        [ForeignKey("RolUsuario")]
        public long RolUsuarioId { get; set; }
        public virtual RolUsuario RolUsuarios { get; set; } = null!;

        [Required(ErrorMessage = "Selecciona una sucursal")]
        [ForeignKey("Sucursal")]
        public long SucursalId { get; set; }
        public virtual Sucursal Sucursales { get; set; } = null!;
                        

        public virtual ICollection<Vehiculo> Vehiculos { get; set; }
        public virtual ICollection<Login> Logins { get; set; }
    }
}
