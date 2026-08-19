using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Net.Http.Json;
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


        public async Task<long> AgregarProductoVentaAsync(long idTaller, ViewAgregarProducto model)
        {
            try
            {                
                var responseEdit = await _httpClient.PostAsJsonAsync("InventarioSalidaProductos/agregarProdcutoVenta", model);
                if (!responseEdit.IsSuccessStatusCode)
                {
                    return (0);
                }

                var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);

                long id = 0;
                string consecutivo = string.Empty;

                if (document.RootElement.TryGetProperty("id", out JsonElement idElement))
                {
                    id = idElement.GetInt64();
                }

                if (document.RootElement.TryGetProperty("consecutivo", out JsonElement consecutivoElement))
                {
                    consecutivo = consecutivoElement.GetString() ?? string.Empty;
                }

                return (id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CrearInventarioSalidaProductoAsync: {ex.Message}");
                return (0);
            }
        }


    }
}
