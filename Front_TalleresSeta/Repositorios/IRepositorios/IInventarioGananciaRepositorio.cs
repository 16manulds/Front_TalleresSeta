using Front_TalleresSeta.Modelos.ModelosView;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface IInventarioGananciaRepositorio
    {
        Task<long> ActualizarGananciaVentaAsync(long filtroId, DtoGananciaVenta model);
    }
}