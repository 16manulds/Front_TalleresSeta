using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioSubCategoriasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        public static string tabla = "InventarioSubCategorias";
        public static string mensaje = string.Empty;
        public static long idSubCategoria = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public InventarioSubCategoriasController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
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

                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todos");
                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria");

                    if (logueado.TipoUser == "SuperAdmin")
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioSubCategoria>>($"InventarioSubCategorias/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioSubCategoria>>($"InventarioSubCategorias/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado subcategorías" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar subcategoría.";
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
                        var subCategorias = await _httpClient.GetFromJsonAsync<InventarioSubCategoria>($"InventarioSubCategorias/obtener/{id}");
                        ViewData["NombreCategoria"] = subCategorias?.InventarioCategorias.NombreCategoria;
                        ViewData["RazonSocialTaller"] = subCategorias?.Talleres.RazonSocialTaller;
                        return View(subCategorias);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle de la subcategoría, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle de la subcategoría.";
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
                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todos");

                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria");
                    ViewData["TallerId"] = talleres;
                    ViewData["mensaje"] = "Registra una categoría.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear la categoría.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InventarioSubCategoriaId,Habilitado,FechaRegistro,NombreSubCategoria,InventarioCategoriaId,TallerId")] InventarioSubCategoria model)
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
                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todos");
                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria");
                    if (ModelState.IsValid)
                    {
                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"InventarioSubCategorias/existe/{model.NombreSubCategoria}");
                        if (existe == false)
                        {
                            try
                            {
                                model.Habilitado = true;
                                model.FechaRegistro = DateTime.Now;
                                var responseCreate = await _httpClient.PostAsJsonAsync("InventarioSubCategorias/crear", model);
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                long idSubCategoria = responseObject.GetProperty("id").GetInt64();

                                mensaje = "Registro satisfactorio...";
                                accion = "CREADO";
                                return RedirectToAction("Details", new { id = idSubCategoria, mensaje, accion });
                            }
                            catch (Exception ex)
                            {
                                mensaje = $"No se puede crear la subcategoría.";
                                ViewData["TallerId"] = talleres;
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }
                        else
                        {
                            mensaje = "Hay caramba!, la subcategoría a registrar ya existe.";
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
                    mensaje = $"No se puede crear la subcategoría.";
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar la subcategoría: id no válido." });
                    }

                    var result = await _httpClient.GetFromJsonAsync<InventarioSubCategoria>($"InventarioSubCategorias/obtener/{id}");
                    if (result == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró la subcategoría a modificar." });
                    }
                                        
                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>("InventarioCategorias/todos");
                    var idCategoriaActual = result?.InventarioCategorias?.InventarioCategoriaId;

                    ViewData["InventarioCategoriaId"] = new SelectList(
                        categorias,
                        "InventarioCategoriaId",
                        "NombreCategoria",
                        idCategoriaActual // <- Este es el valor seleccionado por defecto
                    );

                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(result);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar la subcategoría.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("InventarioSubCategoriaId,Habilitado,NombreSubCategoria,InventarioCategoriaId,TallerId")] InventarioSubCategoria model, long InventarioCategoriaId)
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
                        var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioSubCategorias/modificar", model);
                        idSubCategoria = await responseEdit.Content.ReadFromJsonAsync<long>();
                        return RedirectToAction("Details", new { id = idSubCategoria, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensaje = $"El proceso para modificar la subcategoría falló.";
                        mensajeError = $"Error ocurrido: {ex.Message}";
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                    }
                }
                else
                {
                    ViewData["accion"] = "ERROR_DATOS";
                    ViewData["mensaje"] = "Algo salió mal con los datos...";
                    ViewData["TallerId"] = talleres;
                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todos");
                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria", InventarioCategoriaId);
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"InventarioSubCategorias/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "La subcategoría fue eliminada.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar la subcategoría. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar la subcategoría falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
