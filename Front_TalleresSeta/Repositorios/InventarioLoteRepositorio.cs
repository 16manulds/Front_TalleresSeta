using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Text.Json;


namespace Front_TalleresSeta.Repositorios
{
    public class InventarioLoteRepositorio : IInventarioLoteRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public InventarioLoteRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<long> CrearAsync(InventarioLote modelo)
        {
            long result = 0;
            try
            {
                var responseCreate = await _httpClient.PostAsJsonAsync("InventarioLotes/crear", modelo);

                if (!responseCreate.IsSuccessStatusCode)
                {
                    result = 0;
                }
                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                if (!responseObject.TryGetProperty("id", out JsonElement idElementlote))
                {
                    Console.WriteLine("La respuesta de la API al crear no contiene 'id' para el lote. JSON: " + jsonResponse);
                    result = 0;
                }
                result = (long)responseObject.GetProperty("id").GetInt64();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al validar si existe producto en el lote.", ex);
            }
            return result;
        }


        public async Task<bool> ExisteModeloAsync(string dato)
        {
            bool result = false;
            try
            {
                result = await _httpClient.GetFromJsonAsync<bool>($"InventarioLotes/existe/{dato}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al validar si existe producto en el lote.", ex);
            }
            return result;
        }


        public async Task<long> ActualizarLoteVentaAsync(long tallerId, DtoLoteVenta model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"InventarioLotes/ActualizarLotesVenta?filtroId={tallerId}", model);
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
                Console.WriteLine($"Error en ActualizarLoteVentaAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> RevertirLoteVentaAsync(long tallerId, DtoLoteVenta model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"InventarioLotes/RevertirLoteVenta?filtroId={tallerId}", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en RevertirLoteVentaAsync: {ex.Message}");
                return false;
            }
        }


    }
}
