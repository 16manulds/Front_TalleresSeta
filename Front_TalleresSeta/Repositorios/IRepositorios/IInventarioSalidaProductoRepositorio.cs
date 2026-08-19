using Front_TalleresSeta.Modelos.ModelosView;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioSalidaProductoRepositorio
    {
        Task<long> AgregarProductoVentaAsync(long idTaller, ViewAgregarProducto model);
    }
}