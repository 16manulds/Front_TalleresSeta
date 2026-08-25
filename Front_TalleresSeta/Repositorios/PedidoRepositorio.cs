using Front_TalleresSeta.Repositorios.IRepositorios;
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


        public async Task<string> CrearPedidoAsync(long idTaller)
        {
            try
            {
                // Enviamos solo los datos primitivos necesarios
                var payload = new
                {
                    Habilitado = true,
                    FechaRegistro = DateTime.Now,
                    ConsecutivoPedido = $"P-000-{DateTime.Now:yy}",
                    EstadoPedido = "PEDIDO_PENDIENTE",
                    Detalle = "Se inicia proceso de pedido automaticamente.",
                    TallerId = idTaller
                };

                var responseEdit = await _httpClient.PostAsJsonAsync("Pedidos/crear", payload);

                if (!responseEdit.IsSuccessStatusCode)
                {
                    var errorContent = await responseEdit.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error de Servidor ({responseEdit.StatusCode}): {errorContent}");
                    return (string.Empty);
                }

                var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);

                string consecutivo = string.Empty;

                if (document.RootElement.TryGetProperty("consecutivo", out JsonElement consecutivoElement))
                    consecutivo = consecutivoElement.GetString() ?? string.Empty;

                return (consecutivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CrearPedidoAsync: {ex.Message}");
                return (string.Empty);
            }
        }


        public async Task<bool> EliminarPedidoAsync(string consecutivoPedido, long idTaller)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Pedidos/EliminarPedido/{consecutivoPedido}?filtroId={idTaller}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EliminarPedidoAsync: {ex.Message}");
                return false;
            }
        }

    }
}
