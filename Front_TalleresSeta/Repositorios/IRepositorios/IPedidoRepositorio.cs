using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IPedidoRepositorio
    {
        Task<string> CrearPedidoAsync(long idTaller);
        Task<bool> EliminarPedidoAsync(string consecutivoPedido, long idTaller);
    }
}