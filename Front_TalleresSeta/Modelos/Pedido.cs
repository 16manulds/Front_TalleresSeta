using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public string EstadoPedido { get; set; } = "PEDIDO_CREADO";

        [StringLength(3000)]
        public string? Detalle { get; set; } = "Pedido creado automaticamente.";



        [ForeignKey("Talleres")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;



        [JsonIgnore]
        public virtual ICollection<InventarioSalidaProducto> InventarioSalidaProductos { get; set; } = new HashSet<InventarioSalidaProducto>();

        [JsonIgnore]
        public virtual ICollection<Pago> Pagos { get; set; } = new HashSet<Pago>();

        [JsonIgnore]
        public virtual ICollection<Factura> Facturas { get; set; } = new HashSet<Factura>();
    }
}
