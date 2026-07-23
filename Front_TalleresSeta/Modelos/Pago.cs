using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Pago
    {
        //public Pago()
        //{
        //    Usuarios = new HashSet<Usuario>();
        //    Vehiculos = new HashSet<Vehiculo>();
        //}

        [Key]
        public  long PagoId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public decimal ValorPago { get; set; }

        [StringLength(70)]
        public string? OrigenPago { get; set; } = null;


        [ForeignKey("Sis_MetodosDePago_TipoTarjeta")]
        public long? TipoTarjetaPagoId { get; set; } = null;
        public virtual Sis_MetodosDePago_TipoTarjeta? Sis_MetodosDePago_TipoTarjetas { get; set; } = null!;


        [ForeignKey("Sis_MetodosDePago")]
        public long MetodoDePagoId { get; set; }
        public virtual Sis_MetodosDePago? Sis_MetodosDePagos { get; set; } = null;


        [ForeignKey("Sis_MetodosDePago_Banco")]
        public long? BancoId { get; set; }
        public virtual Sis_MetodosDePago_Banco? Sis_MetodosDePago_Bancos { get; set; } = null!;


        [ForeignKey("InventarioEntradaProducto")]
        public required string CodigoProducto { get; set; }
        public virtual InventarioEntradaProducto? InventarioEntradaProductos { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        [ForeignKey("InventarioSalidaProducto")]
        public long IdInventarioSalidaProducto { get; set; }
        public virtual InventarioSalidaProducto? InventarioSalidaProductos { get; set; } = null;


        //public virtual ICollection<Usuario> Usuarios { get; set; }
        //public virtual ICollection<Vehiculo> Vehiculos { get; set; }
    }
}
