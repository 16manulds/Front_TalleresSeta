using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios
{
    public class TipoVehiculoRepositorio : ITipoVehiculoRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public TipoVehiculoRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

                
        public async Task<SelectList> ObtenerTipoDeVehiculosAsync(long idTaller)
        {
            try
            {
                var tipos = await _httpClient.GetFromJsonAsync<List<TipoVehiculo>>($"TipoVehiculos/mostrarTipoVehiculos/{idTaller}");
                var lista = tipos ?? new List<TipoVehiculo>();

                return new SelectList(lista, "TipoVehiculoId", "TipoV");
            }
            catch (Exception)
            {
                return new SelectList(new List<TipoVehiculo>(), "TipoVehiculoId", "TipoV");
            }
        }



    }
}
