using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.Data.SqlClient;
using System.Text.Json;


namespace Front_TalleresSeta.Repositorios
{
    public class LoginRepositorio : ILoginRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public LoginRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<long> CrearAsync(Login modelo)
        {
            long result = 0;
            try
            {
                var responseCreate = await _httpClient.PostAsJsonAsync("Logins/crear", modelo);

                if (!responseCreate.IsSuccessStatusCode)
                {
                    result = 0;
                }
                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                if (!responseObject.TryGetProperty("id", out JsonElement idElementStock))
                {
                    Console.WriteLine("La respuesta de la API no contiene 'id' para el login. JSON: " + jsonResponse);
                    result = 0;
                }
                result = (long)responseObject.GetProperty("id").GetInt64();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al validar si existe el login.", ex);
            }
            return result;
        }

        public async Task<long> ActualizarAsync(Login model)
        {
            long result = 0;
            try
            {
                var responseEdit = await _httpClient.PutAsJsonAsync($"Logins/modificar", model);

                if (responseEdit.IsSuccessStatusCode)
                {
                    var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
                    var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                    if (!responseObject.TryGetProperty("id", out JsonElement idElementlote))
                    {
                        Console.WriteLine("La respuesta de la API al actualizar no contiene 'id' para el login. JSON: " + jsonResponse);
                        result = 0;
                    }
                    result = (long)responseObject.GetProperty("id").GetInt64();
                }else
                {
                    result = 0;
                }                
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException($"Error al actualizar el login con ID {model.LoginId}.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error general al actualizar el login.", ex);
            }
            return result;
        }

        public async Task<bool> ExisteModeloAsync(string dato)
        {
            bool result = false;
            try
            {
                result = await _httpClient.GetFromJsonAsync<bool>($"Logins/existe/{dato}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al validar si existe el login en el login.", ex);
            }
            return result;
        }

    }
}
