using Front_TalleresSeta.Modelos;

namespace Front_TalleresSeta.Repositorios.IRepositorios
{
    public interface ILoginRepositorio
    {
        Task<long> CrearAsync(Login modelo);
        Task<long> ActualizarAsync(Login modelo);
        Task<bool> ExisteModeloAsync(string dato);
    }
}
