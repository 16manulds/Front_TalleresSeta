using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{
    public class RolUsuariosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "RolUsuarios";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public RolUsuariosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                        return View(await _httpClient.GetFromJsonAsync<List<RolUsuario>>($"RolUsuarios/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<RolUsuario>>($"RolUsuarios/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de productos" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los roles de usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Details(string id, string? mensaje = null, string? accion = null)
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
                    if (!string.IsNullOrEmpty(id))
                    {
                        ViewData["accion"] = accion;
                        ViewData["mensaje"] = mensaje;

                        var detalle = await _httpClient.GetFromJsonAsync<RolUsuario>($"RolUsuarios/obtener/{id}");

                        ViewData["TallerId"] = detalle.Talleres.RazonSocialTaller;
                        ViewBag.Detalle = "DETALLE";
                        return View(detalle);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del rol de usuario, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del rol de usuario.";
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
                    ViewData["mensaje"] = "Registra un rol de usuario.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear un rol de usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Habilitado,FechaRegistro,NombreRol,TallerId")] RolUsuario model)
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
                    ViewData["TallerId"] = talleres;

                    if (ModelState.IsValid)
                    {
                        string idItem = string.Empty;
                        model.Habilitado = true;
                        model.FechaRegistro = DateTime.Now;

                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"RolUsuarios/existe/{model.NombreRol}");
                        if (existe == false)
                        {
                            try
                            {
                                var responseCreate = await _httpClient.PostAsJsonAsync("RolUsuarios/crear", model);
                                if (responseCreate.IsSuccessStatusCode)
                                {
                                    var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                    var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                    if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                    {
                                        Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                        mensaje = "Error: No se recibió el ID del rol de usuario.";
                                    }
                                    idItem = responseObject.GetProperty("id").ToString();

                                    //validar todos los registros estén ok
                                    if (!string.IsNullOrEmpty(idItem))
                                    {
                                        mensaje = "Registro satisfactorio...";
                                        accion = "CREADO";
                                        return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                                    }
                                    else
                                    {
                                        accion = "CREADO_CON_ERROR";
                                        mensaje = $"El rol de usuario no se logró crear.";
                                    }
                                }
                                else
                                {
                                    mensaje = $"Error al crear el rol de usuario: {responseCreate.StatusCode}";
                                }
                            }
                            catch (Exception)
                            {
                                mensaje = $"No se puede crear el rol del usuario.";
                            }
                        }
                        else
                        {
                            mensaje = $"El rol del usuario ya existe.";
                        }
                    }
                    else
                    {
                        mensaje = "Revisa los datos a registrar.";
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear el rol de usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
                ViewData["TallerId"] = talleres;
                ViewData["mensaje"] = mensaje;
                ViewData["accion"] = accion;
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(string id)
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
                    if (string.IsNullOrEmpty(id))
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el rol de usuario: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<RolUsuario>($"RolUsuarios/obtener/{id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el rol de usuario a modificar." });
                    }

                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el rol de usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RolUsuarioId, Habilitado,FechaRegistro,NombreRol,TallerId")] RolUsuario model)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"RolUsuarios/modificar", model);
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar el rol de usuario: {responseEdit.StatusCode}";
                            return View(model);
                        }
                        var jsonResponseEdit = await responseEdit.Content.ReadAsStringAsync();
                        var responseObjectEdit = JsonSerializer.Deserialize<JsonElement>(jsonResponseEdit);
                        if (!responseObjectEdit.TryGetProperty("id", out JsonElement idElement))
                        {
                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseEdit);
                            mensaje = "Error: No se recibió el ID del rol de usuario al actualizar.";
                            return View(model);
                        }
                        falloActualizar = false;
                        idItem = responseObjectEdit.GetProperty("id").ToString();
                        return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensajeError = $"Error ocurrido: {ex.Message}";
                    }
                }

                if (falloActualizar)
                {                    
                    ViewData["TallerId"] = talleres;
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"RolUsuarios/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El rol de usuario fue eliminado.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el rol de usuario. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar el producto falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
