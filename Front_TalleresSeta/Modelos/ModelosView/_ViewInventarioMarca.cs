namespace Front_TalleresSeta.Modelos.ModelosView
{
    public class _ViewInventarioMarca
    {
        public long InventarioMarcaId { get; set; }
        public bool Habilitado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreMarca { get; set; }
        public string? Detalle { get; set; }

        public string RazonSocialTaller { get; set; }
        public long TallerId { get; set; }

    }
}
