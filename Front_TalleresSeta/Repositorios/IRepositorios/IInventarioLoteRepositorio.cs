using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioLoteRepositorio
    {
        Task<long> CrearAsync(InventarioLote modelo);
        //Task<long> ActualizarAsync(InventarioLote modelo);
        Task<bool> ExisteModeloAsync(string dato);
        Task<long> ActualizarLoteVentaAsync(long tallerId, DtoLoteVenta model);
        Task<bool> RevertirLoteVentaAsync(long tallerId, DtoLoteVenta model);
    }
}
