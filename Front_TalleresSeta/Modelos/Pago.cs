using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class Pago
    {
        public Pago()
        {
            //Usuarios = new HashSet<Usuario>();
            //Vehiculos = new HashSet<Vehiculo>();
        }

        [Key]
        public long PagoId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public decimal ValorPago { get; set; }

        public string? EstadoPago { get; set; } = "PAGO_PENDIENTE";



        [ForeignKey("Sis_MetodosDePago")]
        public long MetodoDePagoId { get; set; }
        public virtual Sis_MetodosDePago Sis_MetodosDePago { get; set; } = null!;


        [ForeignKey("Sis_MetodosDePago_Banco")]
        public long? BancoId { get; set; }
        public virtual Sis_MetodosDePago_Banco? Sis_MetodosDePago_Bancos { get; set; } = null;


        [ForeignKey("Sis_MetodosDePago_TipoTarjeta")]
        public long? TipoTarjetaPagoId { get; set; }
        public virtual Sis_MetodosDePago_TipoTarjeta? Sis_TipoTarjetaPago { get; set; } = null;


        [ForeignKey("Pedido")]
        public long PedidoId { get; set; }
        public virtual Pedido Pedidos { get; set; } = null!;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller Talleres { get; set; } = null!;


        //public virtual ICollection<Usuario> Usuarios { get; set; }
        //public virtual ICollection<Vehiculo> Vehiculos { get; set; }
    }
}
