using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface ITipoVehiculoRepositorio
    {
        Task<SelectList> ObtenerTipoDeVehiculosAsync(long idTaller);
    }
}