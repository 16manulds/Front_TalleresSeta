using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Factura
    {
        public Factura()
        {
            //Pagos = new HashSet<Pago>();
            //InventarioSalidaProductos = new HashSet<InventarioSalidaProducto>();
        }

        [Key]
        public long FacturaId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public string NumeroFactura { get; set; } = null!;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";

        public decimal ValorFactura { get; set; }
        public string? EstadoFactura { get; set; } = "FACTURA_CREADA";


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller Talleres { get; set; } = null!;

        [ForeignKey("Usuario")]
        public long Documento { get; set; }
        public virtual Usuario Usuarios { get; set; } = null!;


        [ForeignKey("Vehiculo")]
        public long Placa { get; set; }
        public virtual Vehiculo? Vehiculos { get; set; } = null;


        [ForeignKey("Pedido")]
        public long PedidoId { get; set; }
        public virtual Pedido Pedidos { get; set; } = null!;


        //public virtual ICollection<Pago>? Pagos { get; set; } = null;
        //public virtual ICollection<InventarioSalidaProducto>? InventarioSalidaProductos { get; set; } = null;
    }
}
