using Front_TalleresSeta.Modelos;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioStockRepositorio
    {
        Task<long> CrearAsync(InventarioStock modelo);
        Task<long> ActualizarAsync(InventarioStock modelo);
        Task<string> ActualizarCantidadStockAsync(string CodigoProducto);
        Task<bool> ExisteModeloAsync(string dato);
    }
}
