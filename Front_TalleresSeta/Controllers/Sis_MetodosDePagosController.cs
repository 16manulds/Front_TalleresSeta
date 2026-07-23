using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class Sis_MetodosDePagosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "Sis_MetodosDePagos";
        public static string mensaje = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static long idMarca = 0;
        public static SelectList talleres;

        public Sis_MetodosDePagosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago>>($"Sis_MetodosDePagos/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe")
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago>>($"Sis_MetodosDePagos/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de metodos de pagos" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los metodos de pago.";
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
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago>>($"Sis_MetodosDePagos/todos?search={search}"));
                    }
                    else if (logueado.TipoUser == "Admin" || logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe")
                    {
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago>>($"Sis_MetodosDePagos/todos?search={search}&tallerId={logueado.IdTaller}"));
                    }
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede mostrar el consolidado de los metodos de pago" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el consolidado de los metodos de pago.";
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
                        return View(await _httpClient.GetFromJsonAsync<Sis_MetodosDePago>($"Sis_MetodosDePagos/obtener/{id}"));
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del metodo de pago con el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del metodo de pago.";
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
                    ViewData["mensaje"] = "Registra un metodo de pago.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear el metodo de pago.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MetodoDePagoId,Habilitado,FechaRegistro,MetodoDePago,TallerId")] Sis_MetodosDePago model)
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
                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"Sis_MetodosDePagos/existe/{model.MetodoDePago}");
                        if (existe == false)
                        {
                            try
                            {                                
                                model.Habilitado = true;
                                model.FechaRegistro = DateTime.Now;
                                var responseCreate = await _httpClient.PostAsJsonAsync("Sis_MetodosDePagos/crear", model);
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                long idMarca = responseObject.GetProperty("id").GetInt64();

                                mensaje = "Registro satisfactorio...";
                                accion = "CREADO";
                                return RedirectToAction("Details", new { id = idMarca, mensaje, accion });
                            }
                            catch (Exception ex)
                            {
                                mensaje = $"No se puede crear el metodo de pago.";
                                ViewData["TallerId"] = talleres;
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }
                        else
                        {
                            mensaje = "Método de pago a registrar ya existe.";
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
                    mensaje = $"No se puede crear el metodo de pago.";
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el metodo de pago con id: " + id + " no válido." });
                    }

                    var result = await _httpClient.GetFromJsonAsync<Sis_MetodosDePago>($"Sis_MetodosDePagos/obtener/{id}");
                    if (result == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el metodo de pago a modificar." });
                    }
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(result);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el metodo de pago.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("MetodoDePagoId,Habilitado,FechaRegistro,MetodoDePago,TallerId")] Sis_MetodosDePago model)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"Sis_MetodosDePagos/modificar", model);
                        idMarca = await responseEdit.Content.ReadFromJsonAsync<long>();
                        return RedirectToAction("Details", new { id = idMarca, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensaje = $"El proceso para modificar el metodo de pago falló.";
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"Sis_MetodosDePagos/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El metodo de pago fue eliminado.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el metodo de pago. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar el metodo de pago falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<JsonResult> CargarMetodosDePago()
        {
            var metodosDePagos = await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago>>("Sis_MetodosDePagos/todosHabilitados");
            return Json(new SelectList(metodosDePagos, "MetodoDePagoId", "MetodoDePago"));
        }


    }
}
