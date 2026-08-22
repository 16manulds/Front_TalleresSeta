using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioGananciasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IInventarioGananciaRepositorio _inventarioGananciaRepo;
        public static string tabla = "InventarioGanancias";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public InventarioGananciasController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IInventarioGananciaRepositorio inventarioGananciaRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _inventarioGananciaRepo = inventarioGananciaRepo;
        }


        [HttpPut("InventarioGanancias/ActualizarGananciaVenta")]
        public async Task<IActionResult> ActualizarGananciaVenta([FromQuery] long filtroId, [FromBody] DtoGananciaVenta model)
        {
            try
            {
                long idResultado = await _inventarioGananciaRepo.ActualizarGananciaVentaAsync(filtroId, model);
                return Ok(new { id = idResultado > 0 ? idResultado : 1, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }






    }
}
