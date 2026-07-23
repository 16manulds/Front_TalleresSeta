using Front_TalleresSeta.Modelos;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioLoteRepositorio
    {
        Task<long> CrearAsync(InventarioLote modelo);
        //Task<long> ActualizarAsync(InventarioLote modelo);
        Task<bool> ExisteModeloAsync(string dato);
    }
}
