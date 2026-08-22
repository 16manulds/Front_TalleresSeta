namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class DtoAgregarProducto
    {
        public int PrecioFinalXuni { get; set; }
        public int CantVendidos { get; set; }
        public string CodigoProducto { get; set; } = null!;
        public long TallerId { get; set; }
        public long PedidoId { get; set; }
        public long LoteId { get; set; }
    }
}

