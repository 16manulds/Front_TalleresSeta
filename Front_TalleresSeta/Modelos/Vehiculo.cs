using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Front_TalleresSeta.Modelos
{
    public class Vehiculo
    {
        [Key]
        public long VehiculoId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(6)]
        public string Placa { get; set; } = "XXX000";

        [StringLength(50)]
        public string? Marca { get; set; } = null;

        [StringLength(50)]
        public string? Modelo { get; set; } = null;

        [StringLength(30)]
        public string? Color { get; set; } = null;

        public long? Kilometraje { get; set; }

        public int? Cilindraje { get; set; }

        [StringLength(100)]
        public string? Serial { get; set; } = null;

        [StringLength(50)]
        public string? Transmision { get; set; } = null;

        [StringLength(50)]
        public string? Motor { get; set; } = null;

        public int? Llantas { get; set; } = 0;

        public int? Puertas { get; set; } = 0;


        [ForeignKey("TipoVehiculo")]
        public long TipoVehiculoId { get; set; }
        public virtual TipoVehiculo? TipoVehiculos { get; set; } = null;


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;

    }
}
