namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class _ViewInventarioStockElementosAgrupados
    {
        public long InventarioStockId { get; set; }
        public bool Habilitado { get; set; }
        public DateTime? FechaRegistroUpdate { get; set; }
        public string? CodigoProducto { get; set; }
        public string? NombreProducto { get; set; }
        public string? ReferenciaProducto { get; set; }
        public bool HomologadoProducto { get; set; }
        public string? UnidadMedida { get; set; }
        public string? NombreColor { get; set; }
        public int CanStock { get; set; }
        public int MinimoStock { get; set; }
        public string? NombreMarca { get; set; }
        public string? NombreProveedor { get; set; }
        public string? NombreSubCategoria { get; set; }
        public int PrecioCompraUni { get; set; }
        public int PrecioVentaUni { get; set; }
        //public string? NombreTaller { get; set; }
        public long InventarioEntradaProductoId { get; set; }
    }
}

