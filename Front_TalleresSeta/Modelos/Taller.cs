using System.ComponentModel.DataAnnotations;

namespace Front_TalleresSeta.Modelos
{
    public class Taller
    {
        public Taller()
        {
            Sucursales = new HashSet<Sucursal>();
            UnidadMedidas_c = new HashSet<UnidadMedida_c>();
            Medidas_c = new HashSet<Medida_c>();
            Colores_c = new HashSet<Color_c>();
            InventarioCategorias = new HashSet<InventarioCategoria>();
            InventarioDevoluciones = new HashSet<InventarioDevolucion>();
            InventarioMarcas = new HashSet<InventarioMarca>();
            InventarioSubCategorias = new HashSet<InventarioSubCategoria>();
            InventarioLotes = new HashSet<InventarioLote>();
            InventarioStocks = new HashSet<InventarioStock>();
            InventarioProveedores = new HashSet<InventarioProveedor>();
            InventarioGanancias = new HashSet<InventarioGanancia>();
            InventarioEntradaProductos = new HashSet<InventarioEntradaProducto>();
            InventarioSalidaProductos = new HashSet<InventarioSalidaProducto>();
            TipoVehiculos = new HashSet<TipoVehiculo>();
            TipoUsuarios = new HashSet<TipoUsuario>();
            TipoDocumentos = new HashSet<TipoDocumento>();
            Estados = new HashSet<Estado>();
            RolUsuarios = new HashSet<RolUsuario>();
            Vehiculos = new HashSet<Vehiculo>();
            Logins = new HashSet<Login>();
            Pedidos = new HashSet<Pedido>();
            Facturas = new HashSet<Factura>();
            Pagos = new HashSet<Pago>();
        }

        [Key]
        public long TallerId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string NitTaller { get; set; } = null!;

        [StringLength(150)]
        public string RazonSocialTaller { get; set; } = null!;
        public DateTime? FechaFundacion { get; set; }


        [StringLength(3000)]
        public string? Detalle { get; set; } = "N/A";


        public virtual ICollection<Sucursal> Sucursales { get; set; }
        public virtual ICollection<UnidadMedida_c> UnidadMedidas_c { get; set; }
        public virtual ICollection<Medida_c> Medidas_c { get; set; }
        public virtual ICollection<Color_c> Colores_c { get; set; }
        public virtual ICollection<InventarioCategoria> InventarioCategorias { get; set; }
        public virtual ICollection<InventarioDevolucion> InventarioDevoluciones { get; set; }
        public virtual ICollection<InventarioMarca> InventarioMarcas { get; set; }
        public virtual ICollection<InventarioSubCategoria> InventarioSubCategorias { get; set; }
        public virtual ICollection<InventarioLote> InventarioLotes { get; set; }
        public virtual ICollection<InventarioStock> InventarioStocks { get; set; }
        public virtual ICollection<InventarioProveedor> InventarioProveedores { get; set; }
        public virtual ICollection<InventarioGanancia> InventarioGanancias { get; set; }
        public virtual ICollection<InventarioEntradaProducto> InventarioEntradaProductos { get; set; }
        public virtual ICollection<InventarioSalidaProducto> InventarioSalidaProductos { get; set; }
        public virtual ICollection<TipoVehiculo> TipoVehiculos { get; set; }
        public virtual ICollection<TipoUsuario> TipoUsuarios { get; set; }
        public virtual ICollection<TipoDocumento> TipoDocumentos { get; set; }
        public virtual ICollection<Estado> Estados { get; set; }
        public virtual ICollection<RolUsuario> RolUsuarios { get; set; }
        public virtual ICollection<Vehiculo> Vehiculos { get; set; }
        public virtual ICollection<Login> Logins { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
        public virtual ICollection<Factura> Facturas { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }
    }
}
