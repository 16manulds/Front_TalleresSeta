namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class ViewInventarioLote
    {
        public long InventarioStockId { get; set; }
        public bool Habilitado { get; set; } = true;
        public DateTime FechaRegistroInicial { get; set; } = DateTime.Now;
        public DateTime? FechaRegistroUpdate { get; set; } = null;
        public string? Detalle { get; set; } = "N/A";
        public int? TotalIngresados { get; set; } = 0;
        public int? CantStock { get; set; } = 0;
        public int? CantVendidos { get; set; } = 0;
        public required string CodigoProducto { get; set; }
        public string? NombreProducto { get; set; } = "N/A";
        public long TallerId { get; set; }
        public string? RazonSocialTaller { get; set; } = "N/A";

    }
}
