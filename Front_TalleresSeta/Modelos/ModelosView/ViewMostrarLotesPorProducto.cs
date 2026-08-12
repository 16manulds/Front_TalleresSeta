namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class ViewMostrarLotesPorProducto
    {
        public long IdLote { get; set; }
        public string? Referencia { get; set; } = null;
        public int CantRestante { get; set; }
        public Int64 PrecioCompraXuni { get; set; }
        public Int64 PrecioVentaXuni { get; set; }

    }

}

