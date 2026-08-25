namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class DtoAgregarProducto
    {
        public int PrecioFinalXuni { get; set; }
        public int CantVendidos { get; set; }
        public string CodigoProducto { get; set; } = null!;
        public long TallerId { get; set; }
        public string ConsecutivoPedido { get; set; } = null!;
        public string? EstadoProducto { get; set; } = "VENTA_PENDIENTE";
        public long LoteId { get; set; }
    }
}

