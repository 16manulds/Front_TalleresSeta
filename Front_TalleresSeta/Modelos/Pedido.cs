using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Pedido
    {
        public Pedido()
        {
            Pagos = new HashSet<Pago>();
            InventarioSalidaProductos = new HashSet<InventarioSalidaProducto>();
            Facturas = new HashSet<Factura>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PedidoId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string ConsecutivoPedido { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string EstadoPedido { get; set; } = null!;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "Pedido creado automaticamente.";

        [ForeignKey("Talleres")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; }

        
        public virtual ICollection<InventarioSalidaProducto> InventarioSalidaProductos { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }
        public virtual ICollection<Factura> Facturas { get; set; }
    }
}