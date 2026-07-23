namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class _ViewInventarioStock
    {
        public bool Habilitado { get; set; }
        public DateTime FechaRegistroInicial { get; set; }
        public DateTime? FechaRegistroUpdate { get; set; }
        public string? Detalle { get; set; }
        public int CanStock { get; set; }
        public int MinimoStock { get; set; }
        public string? CodigoProducto { get; set; }
        public string? ReferenciaProducto { get; set; }
        public string? TamanoProducto { get; set; }
        public bool HomologadoProducto { get; set; }
        public string? MarcaIdProducto { get; set; }
        public string? ProveedorIdProducto { get; set; }

    }
}

