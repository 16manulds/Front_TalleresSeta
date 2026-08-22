using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.Data.SqlClient;
using System.Text.Json;


namespace Front_TalleresSeta.Repositorios
{
    public class InventarioStockRepositorio : IInventarioStockRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public InventarioStockRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<long> CrearAsync(InventarioStock modelo)
        {
            long result = 0;
            try
            {
                var responseCreate = await _httpClient.PostAsJsonAsync("InventarioStocks/crear", modelo);

                if (!responseCreate.IsSuccessStatusCode)
                {
                    result = 0;
                }
                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                if (!responseObject.TryGetProperty("id", out JsonElement idElementStock))
                {
                    Console.WriteLine("La respuesta de la API no contiene 'id' para el stock. JSON: " + jsonResponse);
                    result = 0;
                }
                result = (long)responseObject.GetProperty("id").GetInt64();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al validar si existe producto en el stock.", ex);
            }
            return result;
        }

        public async Task<long> ActualizarAsync(InventarioStock model)
        {
            long result = 0;
            try
            {
                var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioStocks/modificar", model);

                if (responseEdit.IsSuccessStatusCode)
                {
                    var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
                    var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                    if (!responseObject.TryGetProperty("id", out JsonElement idElementlote))
                    {
                        Console.WriteLine("La respuesta de la API al actualizar no contiene 'id' para el stock. JSON: " + jsonResponse);
                        result = 0;
                    }
                    string idString = responseObject.GetProperty("id").GetString();
                    result = long.Parse(idString); 
                }else
                {
                    result = 0;
                }                
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException($"Error al actualizar el lote con ID {model.CodigoProducto}.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error general al actualizar el stock.", ex);
            }
            return result;
        }

        public async Task<string> ActualizarCantidadStockAsync(string CodigoProducto)
        {
            string result = "0";
            try
            {
                var responseEdit = await _httpClient.GetAsync($"InventarioStocks/modificarCantidadStockPorLote/{CodigoProducto}");

                if (responseEdit.IsSuccessStatusCode)
                {
                    var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<string>(jsonResponse);
                }
                else
                {
                    result = "0";
                }
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException($"Error al actualizar la cantidad del stock del producto.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar la cantidad del stock del producto.", ex);
            }
            return result;
        }

        public async Task<bool> ExisteModeloAsync(string dato)
        {
            bool result = false;
            try
            {
                result = await _httpClient.GetFromJsonAsync<bool>($"InventarioStocks/existe/{dato}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al validar si existe producto en el stock.", ex);
            }
            return result;
        }


        public async Task<long> ActualizarStockVentaAsync(long tallerId, DtoStockVenta model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"InventarioStocks/ActualizarStockVenta?filtroId={tallerId}", model);
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
                Console.WriteLine($"Error en ActualizarStockVentaAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> RevertirStockVentaAsync(long tallerId, DtoStockVenta model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"InventarioStocks/RevertirStockVenta?filtroId={tallerId}", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en RevertirStockVentaAsync: {ex.Message}");
                return false;
            }
        }


    }
}
