using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class SucursalesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "Sucursales";
        public static string mensaje = string.Empty;
        public static long idItem = 0;
        //public static long idSucursal = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;


        public SucursalesController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                        return View(await _httpClient.GetFromJsonAsync<List<Sucursal>>($"Sucursales/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<Sucursal>>($"Sucursales/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de las sucursales" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el sucursal.";
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
                        var detalle = await _httpClient.GetFromJsonAsync<Sucursal>($"Sucursales/obtener/{id}");
                        ViewData["accion"] = accion;
                        ViewData["mensaje"] = mensaje;
                        ViewData["TallerId"] = detalle?.Talleres?.RazonSocialTaller;
                        return View(detalle);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle de la sucursal, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle de la sucursal.";
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
                    ViewData["mensaje"] = "Registra una sucursal.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear una sucursal.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SucursalId,Habilitado,FechaRegistro,Codigo,Nombre,FechaFundacion,Direccion,Detalle,TelFijo,TelMovil,Correo,TallerId")] Sucursal model)
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
                        model.FechaRegistro = DateTime.Now;
                        model.Detalle ??= "N/A";

                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"Sucursales/existe/{model.SucursalId}");
                        if (existe == false)
                        {
                            try
                            {
                                var responseCreate = await _httpClient.PostAsJsonAsync("Sucursales/crear", model);
                                if (!responseCreate.IsSuccessStatusCode)
                                {
                                    mensaje = $"Error al crear la sucursal: {responseCreate.StatusCode}";
                                    return View(model);
                                }
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                {
                                    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                    mensaje = "Error: No se recibió el ID de la sucursal.";
                                    return View(model);
                                }
                                idItem = responseObject.GetProperty("id").GetInt64();

                                if (idItem > 0)
                                {
                                    mensaje = "Registro satisfactorio...";
                                    accion = "CREADO";
                                    return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                                }
                                else
                                {
                                    accion = "CREADO_CON_ERROR";
                                    mensaje = $"La sucursal no se logró crear.";
                                }
                            }
                            catch (Exception)
                            {
                                mensaje = $"No se puede crear la sucursal.";
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                            }
                        }
                        else
                        {
                            ViewData["accion"] = accion;
                            mensaje = "La sucursal ya existe.";
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
                    mensaje = $"No se puede crear el producto.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
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
                    ViewData["accion"] = accion;
                    if (string.IsNullOrEmpty(id))
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar la sucursal: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<Sucursal>($"Sucursales/obtener/{id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró la sucursal a modificar." });
                    }

                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar la sucursal.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("SucursalId,Habilitado,FechaRegistro,Codigo,Nombre,FechaFundacion,Direccion,Detalle,TelFijo,TelMovil,Correo,TallerId")] Sucursal model)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"Sucursales/modificar", model);
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar la sucursal: {responseEdit.StatusCode}";
                            return View(model);
                        }
                        var jsonResponseEdit = await responseEdit.Content.ReadAsStringAsync();
                        var responseObjectEdit = JsonSerializer.Deserialize<JsonElement>(jsonResponseEdit);
                        if (!responseObjectEdit.TryGetProperty("id", out JsonElement idElement))
                        {
                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseEdit);
                            mensaje = "Error: No se recibió el ID de la sucursal al actualizar.";
                            return View(model);
                        }
                        idItem = responseObjectEdit.GetProperty("id").GetInt64();
                        falloActualizar = false;
                        return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensajeError = $"Error ocurrido: {ex.Message}";
                        ViewData["TallerId"] = talleres;
                    }
                }
                else
                {
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ERROR_DATOS";
                    ViewData["mensaje"] = "Algo salió mal con los datos...";
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"Sucursales/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "La sucursal fue eliminada.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar la sucursal. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar la sucursal falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
