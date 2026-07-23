using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public class InventarioEntradaProducto
    {
        public InventarioEntradaProducto()
        {
            InventarioDevoluciones = new HashSet<InventarioDevolucion>();
            InventarioGanancias = new HashSet<InventarioGanancia>();
            InventarioLotes = new HashSet<InventarioLote>();
            InventarioSalidaProductos = new HashSet<InventarioSalidaProducto>();
            InventarioStocks = new HashSet<InventarioStock>();
        }

        [Key]
        [StringLength(300)]
        public required string CodigoProducto { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public string NombreProducto { get; set; } = null!;
        public string Referencia { get; set; } = null!;
        public bool Homologado { get; set; } = false;

        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";

        [StringLength(150)]
        public string? PosicionProducto { get; set; }

        [StringLength(150)]
        public string? UbicacionProducto { get; set; }

        [StringLength(500)]
        public string? VehiculoAsociado { get; set; }
        public int StockMinimo { get; set; } = 2;
        public byte[]? ImagenCodigoDeBarras { get; set; }
        public int? ConsecutivoCodBarras { get; set; } = null;
        public byte[]? ImagenProducto { get; set; }


        [ForeignKey("InventarioProveedor")]
        public long InventarioProveedorId { get; set; }
        public virtual InventarioProveedor? InventarioProveedores { get; set; } = null;


        [ForeignKey("InventarioMarca")]
        public long InventarioMarcaId { get; set; }
        public virtual InventarioMarca? InventarioMarcas { get; set; } = null;


        [ForeignKey("InventarioSubCategoria")]
        public long InventarioSubCategoriaId { get; set; }
        public virtual InventarioSubCategoria? InventarioSubCategorias { get; set; }


        [ForeignKey("Taller")]
        public long TallerId { get; set; }
        public virtual Taller? Talleres { get; set; } = null;


        [ForeignKey("Color_c")]
        public long? Color_cId { get; set; }
        public virtual Color_c? Colores_c { get; set; } = null;


        [ForeignKey("UnidadMedida_c")]
        public long? UnidadMedida_cId { get; set; } = 0;
        public virtual UnidadMedida_c? UnidadMedidas { get; set; } = null;


        public virtual ICollection<InventarioDevolucion> InventarioDevoluciones { get; set; }
        public virtual ICollection<InventarioGanancia> InventarioGanancias { get; set; }
        public virtual ICollection<InventarioLote> InventarioLotes { get; set; }
        public virtual ICollection<InventarioSalidaProducto> InventarioSalidaProductos { get; set; }
        public virtual ICollection<InventarioStock> InventarioStocks { get; set; }
    }
}
