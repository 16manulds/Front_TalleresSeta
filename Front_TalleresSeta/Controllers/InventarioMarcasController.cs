using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioMarcasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "InventarioMarcas";
        public static string mensaje = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static long idMarca = 0;
        public static SelectList talleres;

        public InventarioMarcasController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                    search = search == "Todos" ? string.Empty : search;
                    var annosRegistrados = await _httpClient.GetFromJsonAsync<List<ViewAnnosListar>>($"Funciones/cargarAnnos/{tabla}");
                    ViewData["FechaRegistro"] = new SelectList(annosRegistrados, "Annos", "Annos", search);

                    if (logueado.TipoUser == "SuperAdmin")
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe")
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de marcas" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o las marcas.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Consolidado(string search)
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    var annosRegistrados = await _httpClient.GetFromJsonAsync<List<ViewAnnosListar>>($"Funciones/cargarAnnos/{tabla}");
                    ViewData["FechaRegistro"] = new SelectList(annosRegistrados, "Annos", "Annos", search);

                    if (logueado.TipoUser == "SuperAdmin")
                    {
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todos?search={search}"));
                    }
                    else if (logueado.TipoUser == "Admin" || logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe")
                    {
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todos?search={search}&tallerId={logueado.IdTaller}"));
                    }
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede mostrar el consolidado de las marcas" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el consolidado de las marcas.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Details(long id, string? mensaje = null, string? accion = null)
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    if (id > 0)
                    {
                        ViewData["accion"] = accion;
                        ViewData["mensaje"] = mensaje;
                        return View(await _httpClient.GetFromJsonAsync<InventarioMarca>($"InventarioMarcas/obtener/{id}"));
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle de la marca, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle de la marca.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Create()
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    ViewData["TallerId"] = talleres;
                    ViewData["mensaje"] = "Registra una marca.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear la marca.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InventarioMarcaId,Habilitado,FechaRegistro,NombreMarca,Detalle,TallerId")] InventarioMarca model)
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    accion = "FALLO";
                    if (ModelState.IsValid)
                    {
                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"InventarioMarcas/existe/{model.NombreMarca}");
                        if (existe == false)
                        {
                            try
                            {
                                if (model.Detalle == null)
                                {
                                    model.Detalle = "N/A";
                                }

                                model.Habilitado = true;
                                model.FechaRegistro = DateTime.Now;
                                var responseCreate = await _httpClient.PostAsJsonAsync("InventarioMarcas/crear", model);
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                long idMarca = responseObject.GetProperty("id").GetInt64();

                                mensaje = "Registro satisfactorio...";
                                accion = "CREADO";
                                return RedirectToAction("Details", new { id = idMarca, mensaje, accion });
                            }
                            catch (Exception ex)
                            {
                                mensaje = $"No se puede crear la marca.";
                                ViewData["TallerId"] = talleres;
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }
                        else
                        {
                            mensaje = "La marca a registrar ya existe.";
                        }
                    }
                    else
                    {
                        mensaje = "Revisa los datos a registrar.";
                    }
                    ViewData["TallerId"] = talleres;
                    ViewData["mensaje"] = mensaje;
                    ViewData["accion"] = accion;
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear la marca.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Edit(long id)
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    if (id < 1)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar la marca: id no válido." });
                    }

                    var marca = await _httpClient.GetFromJsonAsync<InventarioMarca>($"InventarioMarcas/obtener/{id}");
                    if (marca == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró la marca a modificar." });
                    }
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(marca);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar la marca.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("InventarioMarcaId,Habilitado,FechaRegistro,NombreMarca,Detalle,TallerId")] InventarioMarca model)
        {
            accion = "ACTUALIZADO";
            mensaje = "Actualización satisfactoria...";
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        model.FechaRegistro = DateTime.Now;
                        var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioMarcas/modificar", model);
                        idMarca = await responseEdit.Content.ReadFromJsonAsync<long>();
                        return RedirectToAction("Details", new { id = idMarca, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensaje = $"El proceso para modificar la marca falló.";
                        mensajeError = $"Error ocurrido: {ex.Message}";
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                    }
                }
                else
                {
                    ViewData["accion"] = "ERROR_DATOS";
                    ViewData["mensaje"] = "Algo salió mal con los datos...";
                    ViewData["TallerId"] = talleres;
                }
                return View(model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            accion = "NO_ELIMINADO";
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            else
            {
                try
                {
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"InventarioMarcas/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "La marca fue eliminada.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar la marca. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar la marca falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
