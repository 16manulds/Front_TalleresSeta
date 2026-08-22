using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using System.Text.Json;

namespace Front_TalleresSeta.Repositorios
{
    public class InventarioGananciaRepositorio : IInventarioGananciaRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public InventarioGananciaRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<long> ActualizarGananciaVentaAsync(long tallerId, DtoGananciaVenta model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"InventarioGanancias/ActualizarGananciaVenta?filtroId={tallerId}", model);
                if (!response.IsSuccessStatusCode) return 0;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("id", out var idElem))
                {
                    return idElem.GetInt64();
                }
                return 1; // Si no retorna un id explícito pero fue OK, retorna 1 como exitoso
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ActualizarGananciaVentaAsync: {ex.Message}");
                return 0;
            }
        }

    }
}
