using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioProveedoresController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "InventarioProveedores";
        public static string mensaje = string.Empty;
        public static long idProveedor = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres;

        public InventarioProveedoresController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioProveedor>>($"InventarioProveedores/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioProveedor>>($"InventarioProveedores/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de proveedores" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los proveedores.";
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
                        var inventarioProveedor = await _httpClient.GetFromJsonAsync<InventarioProveedor>($"InventarioProveedores/obtener/{id}");
                        return View(inventarioProveedor);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del proveedor, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del proveedor.";
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
                    ViewData["mensaje"] = "Registra un proveedor.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear un proveedor.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InventarioProveedorId,Habilitado,FechaRegistro,Nitproveedor,RazonSocial,NombreProveedor,Direccion,Telefono,Movil,Url,Correo,Detalle,TallerId")] InventarioProveedor model)
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
                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"InventarioProveedores/existe/{model.Nitproveedor}");
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
                                var responseCreate = await _httpClient.PostAsJsonAsync("InventarioProveedores/crear", model);
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                long idProveedor = responseObject.GetProperty("id").GetInt64();

                                mensaje = "Registro satisfactorio...";
                                accion = "CREADO";
                                return RedirectToAction("Details", new { id = idProveedor, mensaje, accion });
                            }
                            catch (Exception ex)
                            {
                                mensaje = $"No se puede crear el proveedor.";
                                ViewData["TallerId"] = talleres;
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }                            
                        }
                        else
                        {
                            mensaje = "Hay caramba!, el proveedor (NIT) a registrar ya existe.";
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
                    mensaje = $"No se puede crear el proveedor.";
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el proveedor: id no válido." });
                    }

                    var inventarioProveedor = await _httpClient.GetFromJsonAsync<InventarioProveedor>($"InventarioProveedores/obtener/{id}");
                    if (inventarioProveedor == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el proveedor a modificar." });
                    }
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(inventarioProveedor);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el proveedor.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("InventarioProveedorId,Habilitado,FechaRegistro,Nitproveedor,RazonSocial,NombreProveedor,Direccion,Telefono,Movil,Url,Correo,Detalle,TallerId")] InventarioProveedor model)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioProveedores/modificar", model);
                        idProveedor = await responseEdit.Content.ReadFromJsonAsync<long>();
                        return RedirectToAction("Details", new { id = idProveedor, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensaje = $"El proceso para modificar el proveedor falló.";
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"InventarioProveedores/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El proveedor fue eliminada.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el proveedor. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar el proveedor falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
