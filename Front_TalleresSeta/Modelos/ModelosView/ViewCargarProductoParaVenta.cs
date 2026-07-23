using System.ComponentModel.DataAnnotations;

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

        //public long IdInventarioSalidaProducto { get; set; }
        //public bool Habilitado { get; set; } = true;
        //public DateTime FechaVenta { get; set; } = DateTime.Now;
        //public Int64 PrecioFinalXuni { get; set; } = 0;
        //public int? CantVendidos { get; set; } = 0;
        //public int? CantDevoluciones { get; set; } = 0;
        //public string? Detalle { get; set; } = null;

    }
}

