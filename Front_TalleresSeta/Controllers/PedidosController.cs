using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{
    public class PedidosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IPedidoRepositorio _pedidoRepo;
        public static string tabla = "Pedidos";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public PedidosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IPedidoRepositorio pedidoRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _pedidoRepo = pedidoRepo;
        }


        [HttpPost]
        public async Task<IActionResult> CrearPedido([FromQuery] long filtroId)
        {
            if (filtroId <= 0)
            {
                return BadRequest(new { success = false, mensaje = "El identificador del taller es requerido." });
            }

            try
            {
                var (id, consecutivo) = await _pedidoRepo.CrearPedidoAsync(filtroId);

                if (id <= 0)
                {
                    return StatusCode(500, new { success = false, mensaje = "No se pudo generar el pedido en el servidor." });
                }

                var pedidoCreado = new ViewPedidoCreado
                {
                    PedidoId = id,
                    ConsecutivoPedidoCreado = consecutivo
                };

                return Json(pedidoCreado);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { success = false, mensaje = "Error de comunicación con el servicio de Pedidos.", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mensaje = "Ocurrió un error inesperado al crear el pedido.", detalle = ex.Message });
            }
        }


    }
}
