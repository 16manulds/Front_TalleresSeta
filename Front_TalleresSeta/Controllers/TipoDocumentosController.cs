using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{
    public class TipoDocumentosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "TipoDocumentos";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public TipoDocumentosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                        return View(await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de productos" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los tipos de documento.";
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

                        var detalle = await _httpClient.GetFromJsonAsync<TipoDocumento>($"TipoDocumentos/obtener/{id}");

                        ViewData["TallerId"] = detalle.Talleres.RazonSocialTaller;
                        ViewBag.Detalle = "DETALLE";
                        return View(detalle);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del tipo de documento, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del tipo de documento.";
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
                    ViewData["mensaje"] = "Registra un tipo de documento.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear un tipo de documento.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoDocumentoId, Habilitado,FechaRegistro,TipoD,NombreDocumento,TallerId")] TipoDocumento model)
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

                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"TipoDocumentos/existe/{model.NombreDocumento}");
                        if (existe == false)
                        {
                            try
                            {
                                var responseCreate = await _httpClient.PostAsJsonAsync("TipoDocumentos/crear", model);
                                if (responseCreate.IsSuccessStatusCode)
                                {
                                    var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                    var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                    if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                    {
                                        Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                        mensaje = "Error: No se recibió el ID del tipo de documento.";
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
                                        mensaje = $"El tipo de documento no se logró crear.";
                                    }
                                }
                                else
                                {
                                    mensaje = $"Error al crear el tipo de documento: {responseCreate.StatusCode}";
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
                    mensaje = $"No se puede crear el tipo de documento.";
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el tipo de documento: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<TipoDocumento>($"TipoDocumentos/obtener/{id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el tipo de documento a modificar." });
                    }

                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el tipo de documento.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TipoDocumentoId, Habilitado,FechaRegistro,TipoD,NombreDocumento,TallerId")] TipoDocumento model)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"TipoDocumentos/modificar", model);
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar el tipo de documento: {responseEdit.StatusCode}";
                            return View(model);
                        }
                        var jsonResponseEdit = await responseEdit.Content.ReadAsStringAsync();
                        var responseObjectEdit = JsonSerializer.Deserialize<JsonElement>(jsonResponseEdit);
                        if (!responseObjectEdit.TryGetProperty("id", out JsonElement idElement))
                        {
                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseEdit);
                            mensaje = "Error: No se recibió el ID del tipo de documento al actualizar.";
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"TipoDocumentos/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El tipo de documento fue eliminado.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el tipo de documento. Cuenta con registros asociados";
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
