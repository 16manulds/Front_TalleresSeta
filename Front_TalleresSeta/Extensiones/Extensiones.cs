using Front_TalleresSeta.Repositorios;
using Front_TalleresSeta.Repositorios.IRepositorios;

namespace Front_TalleresSeta.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFrontendRepositories(this IServiceCollection services)
        {
            // Repositorios existentes
            services.AddScoped<IFuncionRepositorio, FuncionRepositorio>();
            services.AddScoped<IInventarioStockRepositorio, InventarioStockRepositorio>();
            services.AddScoped<IInventarioLoteRepositorio, InventarioLoteRepositorio>();
            services.AddScoped<ILoginRepositorio, LoginRepositorio>();
            services.AddScoped<ITipoDocumentoRepositorio, TipoDocumentoRepositorio>();
            services.AddScoped<ITipoVehiculoRepositorio, TipoVehiculoRepositorio>();
            services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
            services.AddScoped<IInventarioSalidaProductoRepositorio, InventarioSalidaProductoRepositorio>();

            return services;
        }
    }
}