using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IPedidoRepositorio
    {
        Task<SelectList> ObtenerConsecutivoPedidoAsync(long idTaller);
        Task<(long id, string consecutivo)> CrearPedidoAsync(long idTaller);
        Task<bool> EliminarPedidoAsync(long pedidoId, long idTaller);
    }
}