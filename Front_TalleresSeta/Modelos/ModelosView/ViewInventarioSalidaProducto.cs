namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class ViewInventarioSalidaProducto
    {
        public long InventarioSalidaProductoId { get; set; }
        public bool? Habilitado { get; set; }
        public string? NombreProducto { get; set; }
        public int CantidadVendidos { get; set; }
        public string? ReferenciaProducto { get; set; }
        public string? NombreMarca { get; set; }
        public string? NombreProveedor { get; set; }
        public string CodigoProducto { get; set; } = null!;
        public long TallerId { get; set; }

    }
}

