using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using System.Text.Json;

namespace Front_TalleresSeta.Repositorios
{
    public class InventarioSalidaProductoRepositorio : IInventarioSalidaProductoRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public InventarioSalidaProductoRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<long> AgregarProductoVentaAsync(long tallerId, DtoAgregarProducto model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"InventarioSalidaProductos/AgregarProductoVenta?filtroId={tallerId}", model);
                if (!response.IsSuccessStatusCode) return 0;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("id", out var idElem))
                {
                    return idElem.GetInt64();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en AgregarProductoVentaAsync: {ex.Message}");
                return 0;
            }
        }


        public async Task<bool> EliminarPorIdAsync(long idProducto, long tallerId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"InventarioSalidaProductos/EliminarPorId/{idProducto}?filtroId={tallerId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EliminarPorIdAsync: {ex.Message}");
                return false;
            }
        }

    }
}
