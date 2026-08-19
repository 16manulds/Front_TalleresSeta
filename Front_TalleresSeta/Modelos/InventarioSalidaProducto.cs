using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioSalidaProducto
    {
        //public InventarioSalidaProducto()
        //{
        //    InventarioGanancias = new HashSet<InventarioGanancia>();
        //}

        [Key]
        public long IdInventarioSalidaProducto { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public Int64 PrecioFinalXuni { get; set; } = 0;
        public int? CantVendidos { get; set; } = 0;
        public int? CantDevoluciones { get; set; } = 0;
        public string? EstadoProducto { get; set; } = null;


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto? InventarioEntradaProductos { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        [ForeignKey("Factura")]
        public long? FacturaId { get; set; } = null;
        public virtual Factura? Facturas { get; set; } = null;


        [ForeignKey("Pedido")]
        public long PedidoId { get; set; }
        public virtual Pedido? Pedidos { get; set; } = null;



        //public virtual ICollection<InventarioGanancia> InventarioGanancias { get; set; } = null!;
    }
}
