using Front_TalleresSeta.Modelos.ModelosView;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IFuncionRepositorio
    {
        DatosLogueado ObtenerDatosLogueadoAsync();
        Task<SelectList> ObtenerTallerLogueadoAsync(string tipoUsuario, string tipoRol, long idTaller);
    }
}