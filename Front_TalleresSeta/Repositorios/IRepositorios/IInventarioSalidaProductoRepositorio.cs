using Front_TalleresSeta.Modelos.ModelosView;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioSalidaProductoRepositorio
    {
        Task<long> AgregarProductoVentaAsync(long tallerId, DtoAgregarProducto model);
        Task<bool> EliminarPorIdAsync(long idProducto, long tallerId);
    }
}