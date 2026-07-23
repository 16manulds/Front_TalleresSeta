namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class ViewInventarioSubCategoria
    {
        public long InventarioSubCategoriaId { get; set; }
        public bool Habilitado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreSubCategoria { get; set; }
        public long InventarioCategoriaId { get; set; }
        public long TallerId { get; set; }

        public string NombreCategoria { get; set; }
        public string RazonSocialTaller { get; set; }
    }
}
