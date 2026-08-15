using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios
{
    public class TipoDocumentoRepositorio : ITipoDocumentoRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public TipoDocumentoRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<SelectList> ObtenerTipoDeDocumentosAsync(long idTaller)
        {
            try
            {
                var tiposD = await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/mostrarTipoDocumentos/{idTaller}");
                var lista = tiposD ?? new List<TipoDocumento>();

                return new SelectList(lista, "TipoDocumentoId", "TipoD");
            }
            catch (Exception)
            {
                return new SelectList(new List<TipoDocumento>(), "TipoDocumentoId", "TipoD");
            }
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
