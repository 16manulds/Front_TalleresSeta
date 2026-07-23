using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Sucursal
    {
        public Sucursal()
        {
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        public long SucursalId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string? Codigo { get; set; }

        [StringLength(150)]
        public string Nombre { get; set; } = null!;
        public DateTime? FechaFundacion { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";

        [StringLength(15)]
        public string? TelFijo { get; set; }

        [StringLength(15)]
        public string? TelMovil { get; set; }

        [StringLength(300)]
        public string? Correo { get; set; }


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
