using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioProveedor
    {
        public InventarioProveedor()
        {
            InventarioEntradaProductos = new HashSet<InventarioEntradaProducto>();
        }

        [Key]
        public long InventarioProveedorId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Nitproveedor { get; set; } = null!;

        [StringLength(150)]
        public string RazonSocial { get; set; } = "Sin razón social";

        [StringLength(150)]
        public string? NombreProveedor { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [StringLength(15)]
        public string? Telefono { get; set; }

        [StringLength(15)]
        public string? Movil { get; set; }

        [StringLength(3000)]
        public string? Url { get; set; }

        [StringLength(300)]
        public string? Correo { get; set; }

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        public virtual ICollection<InventarioEntradaProducto> InventarioEntradaProductos { get; set; }
    }
}
