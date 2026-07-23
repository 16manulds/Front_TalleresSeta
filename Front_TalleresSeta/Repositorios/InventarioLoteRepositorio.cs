using Front_TalleresSeta.Modelos;
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

        //public async Task<long> ActualizarAsync(InventarioLote modelo)
        //{
        //    long result = 0;
        //    try
        //    {
        //        var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioLotes/modificar", modelo);

        //        if (!responseEdit.IsSuccessStatusCode)
        //        {
        //            result = 0;
        //        }
        //        var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
        //        var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
        //        if (!responseObject.TryGetProperty("id", out JsonElement idElementlote))
        //        {
        //            Console.WriteLine("La respuesta de la API al actualizar no contiene 'id' para el lote. JSON: " + jsonResponse);
        //            result = 0;
        //        }
        //        string idString = responseObject.GetProperty("id").GetString();
        //        result = long.Parse(idString);
        //    }
        //    catch (SqlException sqlEx)
        //    {
        //        throw new ApplicationException($"Error al actualizar el lote con ID {modelo.CodigoProducto}.", sqlEx);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("Error general al actualizar el lote.", ex);
        //    }
        //    return result;
        //}

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

    }
}
