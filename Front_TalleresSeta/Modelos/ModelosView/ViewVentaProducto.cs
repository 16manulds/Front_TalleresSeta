namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class ViewVentaProducto
    {
        public bool Habilitado { get; set; }
        public required string CodigoProducto { get; set; } = null!;
        public DateTime? FechaVenta { get; set; } = DateTime.Now;
        public string? NombreProducto { get; set; }
        public int? UnidadesStock { get; set; }
        public string? PrecioVentaPorUni { get; set; }
        public string? PrecioVentaTotal { get; set; }
        public string? PrecioTotalPagado { get; set; }
        public int CantVendidos { get; set; }
        //public int? CantStockActual { get; set; }
        public int? CantDevoluciones { get; set; }
        public string? Detalle { get; set; }
        public long MetodoDePago { get; set; }
        public byte[]? ImagenProducto { get; set; }
        public string? NombreProveedor { get; set; }
        public string? NombreMarca { get; set; }
        public string? NombreCategoria { get; set; }
        public string? NombreSubCategoria { get; set; }
        public string? NombreColor { get; set; }
        public string? NombreMedida { get; set; }
        public string? UnidadMedida { get; set; }
        public string? Taller { get; set; }
        public string? TipoDocumentoId { get; set; }
        public string? TipoVehiculoId { get; set; }
        public required long TallerId { get; set; }

        public List<MetodoPagoIngreso> Pagos { get; set; } = new List<MetodoPagoIngreso>();

    }

    public class MetodoPagoIngreso
    {
        public int MetodoId { get; set; }
        public long? BancoId { get; set; }
        public long? TipoTarjetaId { get; set; }
        public decimal Monto { get; set; }
    }

}

