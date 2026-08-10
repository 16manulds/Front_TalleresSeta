namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class ViewMostrarLotesPorProducto
    {
        //public required string CodigoProducto { get; set; }
        public long IdLote { get; set; }
        public string? Referencia { get; set; } = null;

        public int CantRestante { get; set; }
        public Int64 PrecioVentaXuni { get; set; }
    }

}

