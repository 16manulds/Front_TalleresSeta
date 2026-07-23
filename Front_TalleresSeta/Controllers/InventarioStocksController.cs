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
        public static string tabla = "InventarioStocks";
        public static string mensaje = string.Empty;
        public static long idItem = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;
                

        public InventarioStocksController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
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

        //public async Task<IActionResult> Index(string? search = null, string? mensaje = null, string? accion = null)
        //{
        //    var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
        //    talleres = await _funcionRepo.ObtenerTallerLogueadoAsync(logueado.TipoUser, logueado.TipoRol, logueado.IdTaller);

        //    if (!logueado.IsAuth)
        //    {
        //        return RedirectToAction("Acceso", "Login");
        //    }
        //    else
        //    {
        //        try
        //        {
        //            ViewData["accion"] = accion;
        //            ViewData["mensaje"] = mensaje;
        //            search = (search == "Todos") ? string.Empty : search;
        //            var annosRegistrados = await _httpClient.GetFromJsonAsync<List<ViewAnnosListar>>($"Funciones/cargarAnnos/{tabla}");
        //            ViewData["FechaRegistro"] = new SelectList(annosRegistrados, "Annos", "Annos", search);

        //            if (logueado.TipoUser == "SuperAdmin")
        //                return View(await _httpClient.GetFromJsonAsync<List<InventarioStock>>($"InventarioStocks/todos?search={search}"));
        //            else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
        //                return View(await _httpClient.GetFromJsonAsync<List<InventarioStock>>($"InventarioStocks/todos?search={search}&tallerId={logueado.IdTaller}"));
        //            else
        //                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de productos" });
        //        }
        //        catch (Exception ex)
        //        {
        //            mensaje = $"No se puede mostrar el Stock.";
        //            mensajeError = $"Error ocurrido: {ex.Message}";
        //            return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
        //        }
        //    }
        //}

        //public async Task<IActionResult> Details(string id, string? mensaje = null, string? accion = null)
        //{
        //    var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
        //    if (!logueado.IsAuth)
        //    {
        //        return RedirectToAction("Acceso", "Login");
        //    }
        //    else
        //    {
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(id))
        //            {
        //                ViewData["accion"] = accion;
        //                ViewData["mensaje"] = mensaje;
        //                var inventarioData = await _httpClient.GetFromJsonAsync<InventarioStock>($"InventarioStocks/obtener/{id}");
        //                return View(inventarioData);
        //            }
        //            else
        //            {
        //                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al stock con el id: {id} no es válido o no existe en base de datos." });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            mensaje = $"No se puede mostrar el detalle del stock.";
        //            mensajeError = $"Error ocurrido: {ex.Message}";
        //            return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
        //        }
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Habilitado,FechaRegistroInicial,Detalle,CantStock,CodigoProducto,TallerId")] InventarioStock model)
        //{
        //    var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
        //    if (!logueado.IsAuth)
        //    {
        //        return RedirectToAction("Acceso", "Login");
        //    }
        //    else
        //    {
        //        try
        //        {
        //            accion = "FALLO";                    
        //            ViewData["TallerId"] = talleres;

        //            if (ModelState.IsValid)
        //            {
        //                bool existe = await _httpClient.GetFromJsonAsync<bool>($"InventarioStocks/existe/{model.CodigoProducto}");
        //                if (existe == false)
        //                {
        //                    try
        //                    {
        //                        model.Habilitado = true;
        //                        string fecha = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        //                        model.FechaRegistroInicial = DateTime.ParseExact(fecha, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
        //                        model.Detalle ??= "N/A";

        //                        var modelStock = new InventarioStock
        //                        {
        //                            Habilitado = model.Habilitado,
        //                            FechaRegistroInicial = model.FechaRegistroInicial,
        //                            Detalle = model.Detalle,
        //                            CantStock = model.CantStock,
        //                            CodigoProducto = model.CodigoProducto,
        //                            TallerId = model.TallerId
        //                        };

        //                        //Registrar el Stock
        //                        var responseCreateStock = await _httpClient.PostAsJsonAsync("InventarioStocks/crear", modelStock);
        //                        if (!responseCreateStock.IsSuccessStatusCode)
        //                        {
        //                            mensaje += $"No se pudo registrar en el stock asociado al producto: {model.CodigoProducto}, por el error: {responseCreateStock.StatusCode}";
        //                            return View(model);
        //                        }
        //                        var jsonResponseStock = await responseCreateStock.Content.ReadAsStringAsync();
        //                        var responseObjectStock = JsonSerializer.Deserialize<JsonElement>(jsonResponseStock);
        //                        if (!responseObjectStock.TryGetProperty("id", out JsonElement idElementStock))
        //                        {
        //                            Console.WriteLine("La respuesta de la API no contiene 'id' para el stock. JSON: " + jsonResponseStock);
        //                            mensaje += "Error: No se recibió el ID del Stock.";
        //                            return View(model);
        //                        }
        //                        long idItemStock = responseObjectStock.GetProperty("id").GetInt64();

        //                        //validar todos los registros estén ok
        //                        if (idItemStock > 0)
        //                        {
        //                            mensaje = "Registro satisfactorio...";
        //                            accion = "CREADO";
        //                            //return RedirectToAction("Details", new { id = idItem, mensaje, accion });
        //                        }
        //                    }
        //                    catch (Exception)
        //                    {
        //                        mensaje = $"No se pudo registrar el stock.";
        //                        ViewData["mensaje"] = mensaje;
        //                        ViewData["accion"] = accion;
        //                        return View(model);
        //                    }
        //                }
        //                else
        //                {

        //                }
        //            }
        //            else
        //            {
        //                mensaje = $"No se puedo registrar el stock. Revisa los datos a registrar.";
        //            }
        //            ViewData["TallerId"] = talleres;
        //            ViewData["mensaje"] = mensaje;
        //            ViewData["accion"] = accion;
        //            return View(model);
        //        }
        //        catch (Exception ex)
        //        {
        //            mensaje = $"No se logró registrar el stock.";
        //            mensajeError = $"Error ocurrido: {ex.Message}";
        //            return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
        //        }
        //    }
        //}

        //public async Task<IActionResult> Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var response = await _httpClient.GetAsync($"InventarioStocks/{id}");
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return NotFound();
        //    }

        //    var inventarioStock = await response.Content.ReadFromJsonAsync<InventarioStock>();

        //    var talleres = await _httpClient.GetFromJsonAsync<List<Taller>>("Talleres");
        //    ViewData["TallerId"] = new SelectList(talleres, "TallerId", "RazonSocialTaller", inventarioStock.TallerId);
        //    return View(inventarioStock);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(long id, [Bind("InventarioStockId,Habilitado,FechaRegistroInicial,FechaRegistroUpdate,Detalle,CanStock,MinimoStock,InventarioStocksId,TallerId")] InventarioStock inventarioStock)
        //{
        //    if (id != inventarioStock.InventarioStockId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var response = await _httpClient.PutAsJsonAsync($"InventarioStocks/{id}", inventarioStock);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return RedirectToAction(nameof(Index));
        //        }
        //        ModelState.AddModelError("", "No se pudo actualizar el stock.");
        //    }

        //    var talleres = await _httpClient.GetFromJsonAsync<List<Taller>>("Talleres");
        //    ViewData["TallerId"] = new SelectList(talleres, "TallerId", "RazonSocialTaller", inventarioStock.TallerId);
        //    return View(inventarioStock);
        //}

        //public async Task<IActionResult> Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var response = await _httpClient.GetAsync($"InventarioStocks/{id}");
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return NotFound();
        //    }

        //    var inventarioStock = await response.Content.ReadFromJsonAsync<InventarioStock>();
        //    return View(inventarioStock);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(long id)
        //{
        //    var response = await _httpClient.DeleteAsync($"InventarioStocks/{id}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return Problem("No se pudo eliminar el stock.");
        //}
    }
}
