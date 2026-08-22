namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class DtoLoteVenta
    {
        public long LoteId { get; set; }
        public int CantVendidos { get; set; }
        public string CodigoProducto { get; set; } = null!;
        public long TallerId { get; set; }
    }
}

