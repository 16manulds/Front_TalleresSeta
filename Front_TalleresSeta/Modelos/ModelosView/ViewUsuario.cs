using System.ComponentModel.DataAnnotations.Schema;

namespace Front_TalleresSeta.Modelos
{
    public partial class ViewUsuario
    {
        public string Documento { get; set; } = null!;
        public bool Habilitado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string PrimerNombre { get; set; } = null!;
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int Sexo { get; set; }
        public string? TelefonoFijo { get; set; }
        public string TelefonoMovil { get; set; } = null!;
        public string? DireccionPrincipal { get; set; }
        public string? DireccionAlterna { get; set; }
        public string? Correo { get; set; }

        public long EstadoId { get; set; }

        public long TipoUsuarioId { get; set; }

        public long TipoDocumentoId { get; set; }

        public long RolUsuarioId { get; set; }

        public long SucursalId { get; set; }
        public long TallerId { get; set; }


    }
}
