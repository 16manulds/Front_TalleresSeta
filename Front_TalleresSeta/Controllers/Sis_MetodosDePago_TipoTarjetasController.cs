using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class Sis_MetodosDePago_TipoTarjetasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "Sis_MetodosDePago_TipoTarjetas";
        public static string mensaje = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static long idMarca = 0;
        public static SelectList talleres;

        public Sis_MetodosDePago_TipoTarjetasController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago_TipoTarjeta>>($"Sis_MetodosDePago_TipoTarjetas/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe")
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago_TipoTarjeta>>($"Sis_MetodosDePago_TipoTarjetas/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de tipos de tarjetas de pago" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los tipos de tarjetas de pago.";
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
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago_TipoTarjeta>>($"Sis_MetodosDePago_TipoTarjetas/todos?search={search}"));
                    }
                    else if (logueado.TipoUser == "Admin" || logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe")
                    {
                        return View(await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago_TipoTarjeta>>($"Sis_MetodosDePago_TipoTarjetas/todos?search={search}&tallerId={logueado.IdTaller}"));
                    }
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede mostrar el consolidado de los tipos de tarjetas de pago" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el consolidado de los tipos de tarjetas de pago.";
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
                        return View(await _httpClient.GetFromJsonAsync<Sis_MetodosDePago_TipoTarjeta>($"Sis_MetodosDePago_TipoTarjetas/obtener/{id}"));
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del tipo de tarjeta de pago con el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del tipo de tarjeta de pago.";
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
                    ViewData["mensaje"] = "Registra un tipo de tarjeta de pago.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear el tipo de tarjeta de pago.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoTarjetaPagoId,Habilitado,FechaRegistro,NombreTipoTarjetaPago,TallerId")] Sis_MetodosDePago_TipoTarjeta model)
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
                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"Sis_MetodosDePago_TipoTarjetas/existe/{model.NombreTipoTarjetaPago}");
                        if (existe == false)
                        {
                            try
                            {
                                model.Habilitado = true;
                                model.FechaRegistro = DateTime.Now;
                                var responseCreate = await _httpClient.PostAsJsonAsync("Sis_MetodosDePago_TipoTarjetas/crear", model);
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                long idMarca = responseObject.GetProperty("id").GetInt64();

                                mensaje = "Registro satisfactorio...";
                                accion = "CREADO";
                                return RedirectToAction("Details", new { id = idMarca, mensaje, accion });
                            }
                            catch (Exception ex)
                            {
                                mensaje = $"No se puede crear el tipo de tarjeta de pago.";
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
                    mensaje = $"No se puede crear el tipo de tarjeta de pago.";
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el tipo de tarjeta de pago con id: " + id + " no válido." });
                    }

                    var result = await _httpClient.GetFromJsonAsync<Sis_MetodosDePago_TipoTarjeta>($"Sis_MetodosDePago_TipoTarjetas/obtener/{id}");
                    if (result == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el tipo de tarjeta de pago a modificar." });
                    }
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(result);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el tipo de tarjeta de pago.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("TipoTarjetaPagoId,Habilitado,FechaRegistro,NombreTipoTarjetaPago,TallerId")] Sis_MetodosDePago_TipoTarjeta model)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"Sis_MetodosDePago_TipoTarjetas/modificar", model);
                        idMarca = await responseEdit.Content.ReadFromJsonAsync<long>();
                        return RedirectToAction("Details", new { id = idMarca, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensaje = $"El proceso para modificar el tipo de tarjeta de pago falló.";
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"Sis_MetodosDePago_TipoTarjetas/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El tipo de tarjeta de pago fue eliminado.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el tipo de tarjeta de pago. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar el tipo de tarjeta de pago falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }


        public async Task<JsonResult> CargarMetodosDePagoTipoTarjeta()
        {
            var tipoTarjetas = await _httpClient.GetFromJsonAsync<List<Sis_MetodosDePago_TipoTarjeta>>("Sis_MetodosDePago_TipoTarjetas/todosHabilitados");
            return Json(new SelectList(tipoTarjetas, "TipoTarjetaPagoId", "NombreTipoTarjetaPago"));
        }


    }
}
