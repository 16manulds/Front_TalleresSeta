using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioStockRepositorio
    {
        Task<long> CrearAsync(InventarioStock modelo);
        Task<long> ActualizarAsync(InventarioStock modelo);
        Task<string> ActualizarCantidadStockAsync(string CodigoProducto);
        Task<bool> ExisteModeloAsync(string dato);
        Task<long> ActualizarStockVentaAsync(long tallerId, DtoStockVenta model);
        Task<bool> RevertirStockVentaAsync(long tallerId, DtoStockVenta model);
    }
}
