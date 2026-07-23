using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioLotesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IInventarioStockRepositorio _inventarioStockRepo;
        public static string tabla = "InventarioLotes";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public InventarioLotesController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IInventarioStockRepositorio inventarioStockRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _inventarioStockRepo = inventarioStockRepo;
        }

        public async Task<IActionResult> VerLotes(string id, long loteId, string? search = null, string? mensaje = null, string? accion = null)
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
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioLote>>($"InventarioLotes/lotesPorProducto?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioLote>>($"InventarioLotes/lotesPorProducto?search={search}&tallerId={logueado.IdTaller}&codProducto={id}&idLote={loteId}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de productos" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los productos.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Edit(string id, long loteId)
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el producto: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<InventarioLote>($"InventarioLotes/obtenerLote?idLote={loteId}&codProducto={id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el lote a modificar." });
                    }

                    string imageProducto = string.Empty;
                    if (model.InventarioEntradaProductos.ImagenProducto != null && model.InventarioEntradaProductos.ImagenProducto.Length > 0)
                    {
                        var base64Producto = Convert.ToBase64String(model.InventarioEntradaProductos.ImagenProducto);
                        imageProducto = $"data:image/png;base64,{base64Producto}";
                    }
                    ViewData["ImagenProducto"] = imageProducto;

                    ViewData["PrecioCompraPorUni"] = model.PrecioCompraXuni;
                    ViewData["PrecioVentaPorUni"] = model.PrecioVentaXuni;
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el lote.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdLote,CodigoProducto,Habilitado,FechaRegistro,NombreProducto,PrecioCompraXuni,PrecioVentaXuni,CantIngresan,CantRestante,TallerId")] InventarioLote model, string PrecioCompraPorUni, string PrecioVentaPorUni)
        {
            accion = "ERROR_DATOS";            
            mensaje = "Algo salió mal con los datos...";
            
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
                        string pCompra = Regex.Replace(PrecioCompraPorUni, "[^0-9]", "");
                        string pVenta = Regex.Replace(PrecioVentaPorUni, "[^0-9]", "");
                        var precioCompra = Convert.ToInt64(pCompra);
                        var precioVenta = Convert.ToInt64(pVenta);

                        model.PrecioCompraXuni = precioCompra;
                        model.PrecioVentaXuni = precioVenta;
                        model.CantRestante = model.CantIngresan;

                        ViewData["PrecioCompra"] = model.PrecioCompraXuni;
                        ViewData["PrecioVenta"] = model.PrecioVentaXuni;

                        // Actualizar el lote
                        var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioLotes/modificar", model);
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar el lote: {responseEdit.StatusCode}";
                            return View(model);
                        }
                        var jsonResponseEdit = await responseEdit.Content.ReadAsStringAsync();
                        var responseObjectEdit = JsonSerializer.Deserialize<JsonElement>(jsonResponseEdit);
                        if (!responseObjectEdit.TryGetProperty("id", out JsonElement idElement))
                        {
                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseEdit);
                            mensaje = "Error: No se recibió el ID del lote al actualizar.";
                            return View(model);
                        }
                        
                        idItem = responseObjectEdit.GetProperty("id").ToString();

                        //actualizar stock
                        string idStock = "0";
                        idStock = await _inventarioStockRepo.ActualizarCantidadStockAsync(model.CodigoProducto);

                        if (!string.IsNullOrEmpty(idItem))
                        {
                            if (!string.IsNullOrEmpty(idStock))
                            {
                                falloActualizar = false;
                                accion = "ACTUALIZADO";
                                mensaje = "Actualización satisfactoria...";
                            }
                            else
                            {
                                accion = "ERROR_DATOS";
                                mensaje = $"No se logro actualizar el stock del lote.";
                            }
                        }
                        else
                        {
                            accion = "ERROR_DATOS";
                            mensaje = $"No se logro actualizar el lote.";
                        }
                    }
                    catch (Exception ex)
                    {
                        mensajeError = $"Error ocurrido: {ex.Message}";
                        return View(model);
                    }
                }

                if (falloActualizar == false)
                {
                    return RedirectToAction("VerLotes", new { id = idItem, mensaje, accion });

                } else
                {
                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ERROR_DATOS";
                    ViewData["mensaje"] = mensaje;
                    return View(model);
                }                
            }
        }

    }
}
