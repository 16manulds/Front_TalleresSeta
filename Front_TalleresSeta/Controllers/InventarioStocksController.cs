using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioStocksController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IInventarioStockRepositorio _inventarioStockRepositorio;
        public static string tabla = "InventarioStocks";
        public static string mensaje = string.Empty;
        public static long idItem = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;
                

        public InventarioStocksController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IInventarioStockRepositorio inventarioStockRepositorio)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _inventarioStockRepositorio = inventarioStockRepositorio;
        }

        public async Task<IActionResult> Index(string? search = null, string? mensaje = null, string? accion = null)
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            talleres = await _funcionRepo.ObtenerTallerLogueadoAsync(logueado.TipoUser, logueado.TipoRol, logueado.IdTaller);

            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    ViewData["accion"] = accion;
                    ViewData["mensaje"] = mensaje;
                    search = (search == "Todos") ? string.Empty : search;
                    var annosRegistrados = await _httpClient.GetFromJsonAsync<List<ViewAnnosListar>>($"Funciones/cargarAnnos/{tabla}");
                    ViewData["FechaRegistro"] = new SelectList(annosRegistrados, "Annos", "Annos", search);

                    if (logueado.TipoUser == "SuperAdmin")
                        return View(await _httpClient.GetFromJsonAsync<List<ViewInventarioLote>>($"InventarioStocks/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<ViewInventarioLote>>($"InventarioStocks/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado del stock" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el Stock.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }


        [HttpPut("InventarioStocks/ActualizarStockVenta")]
        public async Task<IActionResult> ActualizarStockVenta([FromQuery] long filtroId, [FromBody] DtoStockVenta model)
        {
            if (model == null) return BadRequest("El payload no puede ser nulo.");

            try
            {
                long stockId = await _inventarioStockRepositorio.ActualizarStockVentaAsync(filtroId, model);
                if (stockId <= 0) return BadRequest("No se pudo actualizar el stock.");

                return Ok(new { id = stockId, stockId = stockId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: /InventarioStocks/RevertirStockVenta?filtroId=123
        [HttpPut("InventarioStocks/RevertirStockVenta")]
        public async Task<IActionResult> RevertirStockVenta([FromQuery] long filtroId, [FromBody] DtoStockVenta model)
        {
            if (model == null) return BadRequest("El payload no puede ser nulo.");

            try
            {
                bool revertido = await _inventarioStockRepositorio.RevertirStockVentaAsync(filtroId, model);
                if (!revertido) return BadRequest("No se pudo revertir el stock.");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


    }
}
