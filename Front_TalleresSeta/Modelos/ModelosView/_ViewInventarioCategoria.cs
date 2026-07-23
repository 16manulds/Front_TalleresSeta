namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class ViewInventarioCategoria
    {
        public long InventarioCategoriaId { get; set; }
        public bool Habilitado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreCategoria { get; set; }
        public string? Detalle { get; set; }
        public string RazonSocialTaller { get; set; }
        public long TallerId { get; set; }
    }
}
