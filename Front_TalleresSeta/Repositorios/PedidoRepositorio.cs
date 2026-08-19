using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Net.Http.Json;
using System.Text.Json;

namespace Front_TalleresSeta.Repositorios
{
    public class PedidoRepositorio : IPedidoRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public PedidoRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<SelectList> ObtenerConsecutivoPedidoAsync(long idTaller)
        {
            try
            {
                var tipos = await _httpClient.GetFromJsonAsync<List<Pedido>>($"Pedidos/obtenerConsecutivoPedido/{idTaller}");
                var lista = tipos ?? new List<Pedido>();

                return new SelectList(lista, "PedidoId", "TipoV");
            }
            catch (Exception)
            {
                return new SelectList(new List<Pedido>(), "PedidoId", "ConsecutivoPedido");
            }
        }

        public async Task<(long id, string consecutivo)> CrearPedidoAsync(long idTaller)
        {
            try
            {
                var model = new Pedido
                {
                    ConsecutivoPedido = $"P-{DateTime.Now:yyyyMMddHHmmss}",
                    EstadoPedido = "PENDIENTE",
                    TallerId = idTaller
                };

                var responseEdit = await _httpClient.PostAsJsonAsync("Pedidos/crear", model);
                if (!responseEdit.IsSuccessStatusCode)
                {
                    return (0, string.Empty);
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

                return (id, consecutivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CrearPedidoAsync: {ex.Message}");
                return (0, string.Empty);
            }
        }


    }
}
