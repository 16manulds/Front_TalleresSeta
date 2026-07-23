namespace Front_TalleresSeta.Modelos.ModelosView
{
    public partial class DatosLogueado
    {
        public bool IsAuth { get; set; }
        public long IdUser { get; set; }
        public string TipoUser { get; set; } = string.Empty;
        public string TipoRol { get; set; } = string.Empty;
        public long IdTaller { get; set; } = 0;
    }
}

