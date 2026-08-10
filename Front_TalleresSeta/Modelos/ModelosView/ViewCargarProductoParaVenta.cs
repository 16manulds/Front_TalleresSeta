namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class ViewCargarProductoParaVenta
    {
        public required string CodigoProducto { get; set; }
        public string? NombreProducto { get; set; } = null;

        public byte[]? ImagenCodigoDeBarras { get; set; } = null;
        public byte[]? ImagenProducto { get; set; } = null;
        public string? NombreProveedor { get; set; } = null;
        public string? NombreMarca { get; set; } = null;
        public string? NombreCategoria { get; set; } = null;
        public string? NombreSubCategoria { get; set; } = null;
        public string? Taller { get; set; } = null;
        public long? TallerId { get; set; } = null;
        public string? NombreColor { get; set; } = null;
        public string? NombreUnidadMedida { get; set; } = null;
        public string? NombreMedida { get; set; } = null;
        public int? CantStock { get; set; } = 0;
        public Int64 PrecioVentaXuni { get; set; } = 0;

        public string? Referencia { get; set; } = null;
    }

}

