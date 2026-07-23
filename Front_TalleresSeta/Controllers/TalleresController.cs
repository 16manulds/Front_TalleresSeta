using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{

    public class TalleresController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private static string mensaje = string.Empty;
        public static string tabla = "Talleres";
        public static long idItem = 0;
        public static long idTaller = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;

        public TalleresController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
        }


        public async Task<IActionResult> Index(string? search = null, string? mensaje = null, string? accion = null)
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
                    ViewData["accion"] = accion;
                    ViewData["mensaje"] = mensaje;
                    search = (search == "Todos") ? string.Empty : search;
                    var annosRegistrados = await _httpClient.GetFromJsonAsync<List<ViewAnnosListar>>($"Funciones/cargarAnnos/{tabla}");
                    ViewData["FechaRegistro"] = new SelectList(annosRegistrados, "Annos", "Annos", search);

                    if (logueado.TipoUser == "SuperAdmin")
                        return View(await _httpClient.GetFromJsonAsync<List<Taller>>($"Talleres/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return RedirectToAction("Details", "Talleres", new { id = logueado.IdTaller });
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de talleres." });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el taller.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Details(long id)
        {
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();

            if (!logueado.IsAuth)
            {
                return RedirectToAction("Acceso", "Login");
            }
            try
            {
                if (id < 1)
                {
                    id = logueado.IdTaller;
                }
                if (id > 0)
                {
                    if (mensaje == "CREADO")
                    {
                        mensaje = "Taller Registrado...";
                    }

                    var detalle = await _httpClient.GetFromJsonAsync<Taller>($"Talleres/obtener/{id}");
                    ViewData["mensaje"] = mensaje;
                    ViewData["accion"] = accion;
                    return View(detalle);
                }
                else
                {
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del Taller, el id: {id} no es válido o no existe en base de datos." });
                }
            }
            catch (Exception ex)
            {
                mensaje = $"No se puede mostrar el detalle del taller.";
                mensajeError = $"Error ocurrido: {ex.Message}";
                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
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
                    ViewData["mensaje"] = "Registra un Taller.";
                    if (logueado.TipoUser == "SuperAdmin")
                        return View(); 
                    else
                        return RedirectToAction("Details", "Talleres", new { id = logueado.IdTaller });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear un Taller.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TallerId,Habilitado,FechaRegistro,NitTaller,RazonSocialTaller,FechaFundacion,Detalle")] Taller model)
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
                        model.Habilitado = true;
                        model.FechaRegistro = DateTime.Now;
                        model.Detalle ??= "N/A";

                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"Talleres/existe/{model.NitTaller}");
                        if (existe == false)
                        {
                            try
                            {
                                var responseCreate = await _httpClient.PostAsJsonAsync("Talleres/crear", model);
                                if (!responseCreate.IsSuccessStatusCode)
                                {
                                    mensaje = $"Error al crear el taller: {responseCreate.StatusCode}";
                                    return View(model);
                                }
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                {
                                    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                    mensaje = "Error: No se recibió el ID del producto.";
                                    return View(model);
                                }
                                idItem = responseObject.GetProperty("id").GetInt64();

                                //validar todos los registros estén ok
                                if (idItem > 0)
                                {
                                    mensaje = "Registro satisfactorio...";
                                    accion = "CREADO";
                                    return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                                }
                                else
                                {
                                    accion = "CREADO_CON_ERROR";
                                    mensaje = $"El taller no se logro crear.";
                                    ViewData["mensaje"] = mensaje;
                                    ViewData["accion"] = accion;
                                    return View(model);
                                }
                            }
                            catch (Exception)
                            {
                                mensaje = $"No se puede crear el taller.";
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }
                        else
                        {
                            mensaje = "Hay caramba!, el NIT a registrar ya existe.";
                        }
                    }
                    else
                    {
                        mensaje = "Revisa los datos a actualizar.";
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear el producto.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
                return View(model);
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
                    ViewData["accion"] = accion;
                    if (id < 0)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el producto: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<Taller>($"Talleres/obtener/{id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el taller a modificar." });
                    }

                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el taller.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TallerId,Habilitado,FechaRegistro,NitTaller,RazonSocialTaller,FechaFundacion,Detalle")] Taller model)
        {
            accion = "ACTUALIZADO";
            mensaje = "Actualización satisfactoria...";
            var logueado = _funcionRepo.ObtenerDatosLogueadoAsync();
            Boolean falloActualizar = true;

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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"Talleres/modificar", model);
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar el Taller: {responseEdit.StatusCode}";
                            return View(model);
                        }
                        var jsonResponseEdit = await responseEdit.Content.ReadAsStringAsync();
                        var responseObjectEdit = JsonSerializer.Deserialize<JsonElement>(jsonResponseEdit);
                        if (!responseObjectEdit.TryGetProperty("id", out JsonElement idElement))
                        {
                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseEdit);
                            mensaje = "Error: No se recibió el ID del producto al actualizar.";
                            return View(model);
                        }
                        idItem = responseObjectEdit.GetProperty("id").GetInt64();
                        falloActualizar = false;
                        return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensajeError = $"Error ocurrido: {ex.Message}";
                    }
                }
                if (falloActualizar)
                {
                    ViewData["accion"] = "ERROR_DATOS";
                    ViewData["mensaje"] = "Algo salió mal con los datos...";
                }
                return View(model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"Talleres/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El taller fue eliminado.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el Taller. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar el taller falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }



    }
}
