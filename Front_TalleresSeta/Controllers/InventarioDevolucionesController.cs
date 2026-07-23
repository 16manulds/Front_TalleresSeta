//using Front_TalleresSeta.Modelos;
//using Front_TalleresSeta.Modelos.ModelosView;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;

//namespace Front_TalleresSeta.Controllers
//{
//    public class InventarioDevolucionesController : Controller
//    {
//        private readonly HttpClient _httpClient;

//        public InventarioDevolucionesController(IHttpClientFactory httpClientFactory)
//        {
//            _httpClient = httpClientFactory.CreateClient("ApiClient");
//        }

//        private async Task<DatosLogueado> ObtenerDatosLogueadoAsync()
//        {
//            var response = await _httpClient.GetAsync("Login/datosUsuarioLogeado");
//            return await response.Content.ReadFromJsonAsync<DatosLogueado>();
//        }

//        public async Task<IActionResult> Index(string? search = null, string? mensaje = null, string? mensaje0 = null)
//        {
//            var logueado = await ObtenerDatosLogueadoAsync();
//            if (!logueado.IsAuth)
//            {
//                return RedirectToAction("Acceso", "Login");
//            }

//            try
//            {
//                if (mensaje != null && mensaje0 != null)
//                {
//                    ViewData["mensaje0"] = mensaje0;
//                    ViewData["mensaje"] = mensaje;
//                }
//                else
//                {
//                    ViewData["mensaje0"] = null;
//                    ViewData["mensaje"] = null;
//                }

//                var annosRegistrados = await _httpClient.GetFromJsonAsync<List<ViewAnnosListar>>("inventarioDevoluciones/annosRegistrados");
//                var annos = annosRegistrados
//                    .Select(x => new
//                    {
//                        FechaRegistro = x.Annos
//                    })
//                    .Distinct()
//                    .OrderByDescending(x => x.FechaRegistro)
//                    .ToList();

//                if (logueado.TipoUser == "SuperAdmin")
//                {
//                    ViewData["FechaRegistro"] = new SelectList(annos, "FechaRegistro", "FechaRegistro", search);
//                    return View(await _httpClient.GetFromJsonAsync<List<InventarioDevolucion>>($"inventarioDevoluciones/listarAll?search={search}"));
//                }
//                else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
//                {
//                    ViewData["FechaRegistro"] = new SelectList(annos, "FechaRegistro", "FechaRegistro", search);
//                    return View(await _httpClient.GetFromJsonAsync<List<InventarioDevolucion>>($"inventarioDevoluciones/listarAll?search={search}&tallerId={logueado.IdTaller}"));
//                }
//                else
//                {
//                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado: InventarioDevolucion" });
//                }
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede mostrar datos para: InventarioDevolucion: {ex.Message}" });
//            }
//        }

//        public async Task<IActionResult> Create(long id)
//        {
//            var logueado = await ObtenerDatosLogueadoAsync();
//            if (!logueado.IsAuth)
//            {
//                return RedirectToAction("Acceso", "Login");
//            }

//            try
//            {
//                int cantVendidos = 0;
//                if (id > 0)
//                {
//                    var producto = await _httpClient.GetFromJsonAsync<ViewInventarioSalidaProducto>($"inventarioSalidaProductos/productoVendido/{id}");
//                    if (producto != null)
//                    {
//                        ViewData["cantVendidos"] = 1;
//                        ViewData["idCantidadVendidos"] = producto.CantidadVendidos;
//                        ViewData["idEntradaProductoId"] = producto.InventarioEntradaProductoId;
//                        ViewData["idSalidaProductoId"] = id;
//                        ViewData["idTallerId"] = producto.TallerId;
//                        ViewData["idCodigoProducto"] = producto.CodigoProducto;
//                        ViewData["idNombreProducto"] = producto.NombreProducto;
//                        ViewData["idReferenciaProducto"] = producto.ReferenciaProducto;
//                        ViewData["idProveedorProducto"] = producto.NombreProveedor;
//                        ViewData["idMarcaProducto"] = producto.NombreMarca;
//                        ViewData["ListarProductosVendidos"] = producto;
//                        ViewData["mensaje"] = "CREAR";
//                    }
//                }
//                else
//                {
//                    ViewData["mensaje0"] = "ELIGE_VENTA";
//                    ViewData["mensaje"] = "Selecciona una venta a devolver al inventario";

//                    var listStosck = new List<ViewInventarioSalidaProducto>();
//                    var listProductosVendidos = await _httpClient.GetFromJsonAsync<List<ViewInventarioSalidaProducto>>("inventarioSalidaProductos/todos");

//                    if (listProductosVendidos.Count == 0)
//                    {
//                        ViewData["mensaje0"] = "SIN_VENTAS_CREADAS";
//                        ViewData["mensaje"] = "No existen ventas con al menos 1 producto vendido";
//                        return RedirectToAction("Index", "InventarioDevoluciones", new { mensaje = ViewData["mensaje"], mensaje0 = ViewData["mensaje0"] });
//                    }
//                    else if (listProductosVendidos.Count == 1)
//                    {
//                        ViewData["cantVendidos"] = 2;
//                    }
//                    else
//                    {
//                        ViewData["cantVendidos"] = listProductosVendidos.Count;
//                    }

//                    if (listProductosVendidos.Count > 0)
//                    {
//                        foreach (var oVendidos in listProductosVendidos)
//                        {
//                            var productoVendidos = new ViewInventarioSalidaProducto
//                            {
//                                InventarioSalidaProductoId = oVendidos.InventarioSalidaProductoId,
//                                CodigoProducto = oVendidos.InventarioEntradaProductos.CodigoProducto,
//                                NombreProducto = $"Código: {oVendidos.InventarioEntradaProductos.CodigoProducto}  Producto: {oVendidos.InventarioEntradaProductos.NombreProducto}  - {oVendidos.InventarioEntradaProductos.Referencia}",
//                                ReferenciaProducto = oVendidos.InventarioEntradaProductos.Referencia,
//                                InventarioEntradaProductoId = oVendidos.InventarioEntradaProductoId
//                            };
//                            listStosck.Add(productoVendidos);
//                        }

//                        ViewData["ListarProductosVendidos"] = listStosck;
//                    }
//                }

//                if (logueado.TipoUser == "SuperAdmin")
//                {
//                    ViewData["TallerId"] = new SelectList(await _httpClient.GetFromJsonAsync<List<Taller>>("talleres/listarAll?search="), "TallerId", "RazonSocialTaller", logueado.IdTaller);
//                }
//                else
//                {
//                    ViewData["TallerId"] = new SelectList(await _httpClient.GetFromJsonAsync<List<Taller>>($"talleres/listarAll?tallerId={logueado.IdTaller}"), "TallerId", "RazonSocialTaller");
//                }
//                return View();
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede mostrar el formulario para InventarioDevolucion: {ex.Message}" });
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(InventarioDevolucion model, long VentaId, long ProductoId, long TallerId)
//        {
//            var logueado = await ObtenerDatosLogueadoAsync();
//            if (!logueado.IsAuth)
//            {
//                return RedirectToAction("Acceso", "Login");
//            }

//            try
//            {
//                bool error = true;
//                if (ModelState.IsValid)
//                {
//                    if (VentaId > 0)
//                    {
//                        var productoVendido = await _httpClient.GetFromJsonAsync<InventarioSalidaProducto>($"inventarioSalidaProductos/productoVendido/{VentaId}");

//                        if (productoVendido != null)
//                        {
//                            if (model.CantidadDevolucion <= productoVendido.Cantidad)
//                            {
//                                productoVendido.Cantidad -= model.CantidadDevolucion;

//                                if (ProductoId > 0)
//                                {
//                                    var productoStockVendido = await _httpClient.GetFromJsonAsync<InventarioStock>($"inventarioStocks/productoStockVendido/{ProductoId}");

//                                    if (productoStockVendido != null)
//                                    {
//                                        productoStockVendido.CanStock += model.CantidadDevolucion;
//                                        if (model.CantidadDevolucion <= productoStockVendido.CantidadVendidos)
//                                        {
//                                            productoStockVendido.CantidadVendidos -= model.CantidadDevolucion;

//                                            // Actualizar productos
//                                            await _httpClient.PutAsJsonAsync("inventarioSalidaProductos/update", productoVendido);
//                                            await _httpClient.PutAsJsonAsync("inventarioStocks/update", productoStockVendido);

//                                            // Agregar devolución
//                                            model.FechaRegistro = DateTime.Now;
//                                            model.UsuarioId = logueado.Id;
//                                            model.TallerId = TallerId;
//                                            await _httpClient.PostAsJsonAsync("inventarioDevoluciones/add", model);
//                                            error = false;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                if (error)
//                {
//                    ViewData["mensaje0"] = "ERROR";
//                    ViewData["mensaje"] = "No se pudo guardar la devolución";
//                }
//                else
//                {
//                    ViewData["mensaje0"] = "OK";
//                    ViewData["mensaje"] = "Devolución registrada con éxito";
//                }
//                return RedirectToAction("Index", "InventarioDevoluciones", new { mensaje = ViewData["mensaje"], mensaje0 = ViewData["mensaje0"] });
//            }
//            catch (Exception ex)
//            {
//                ViewData["mensaje0"] = "ERROR";
//                ViewData["mensaje"] = $"No se pudo guardar la devolución: {ex.Message}";
//                return RedirectToAction("Index", "InventarioDevoluciones", new { mensaje = ViewData["mensaje"], mensaje0 = ViewData["mensaje0"] });
//            }
//        }

//        public async Task<IActionResult> Edit(long id)
//        {
//            var logueado = await ObtenerDatosLogueadoAsync();
//            if (!logueado.IsAuth)
//            {
//                return RedirectToAction("Acceso", "Login");
//            }

//            try
//            {
//                //var devolucion = await _httpClient.GetAsync($"inventarioDevoluciones/{id}");                
//                var devolucion = await _httpClient.GetFromJsonAsync<InventarioDevolucion>($"inventarioDevoluciones/obtener/{id}");

//                if (devolucion != null)
//                {
//                    ViewData["FechaRegistro"] = devolucion.FechaRegistro.ToString("yyyy-MM-dd");
//                    ViewData["TallerId"] = devolucion.TallerId;
//                    ViewData["CantidadDevolucion"] = devolucion.CantidadDevolucion;
//                    ViewData["CodigoProducto"] = devolucion.CodigoProducto;
//                    ViewData["NombreProducto"] = devolucion.NombreProducto;
//                    ViewData["ReferenciaProducto"] = devolucion.ReferenciaProducto;
//                    ViewData["ProveedorProducto"] = devolucion.ProveedorProducto;
//                    ViewData["MarcaProducto"] = devolucion.MarcaProducto;
//                    ViewData["id"] = devolucion.InventarioDevolucionId;

//                    return View(devolucion);
//                }
//                else
//                {
//                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró la devolución solicitada" });
//                }
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Error al recuperar la devolución: {ex.Message}" });
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(InventarioDevolucion model)
//        {
//            var logueado = await ObtenerDatosLogueadoAsync();
//            if (!logueado.IsAuth)
//            {
//                return RedirectToAction("Acceso", "Login");
//            }

//            try
//            {
//                if (ModelState.IsValid)
//                {
//                    await _httpClient.PutAsJsonAsync($"inventarioDevoluciones/update/{model.InventarioDevolucionId}", model);
//                    ViewData["mensaje0"] = "OK";
//                    ViewData["mensaje"] = "Devolución actualizada con éxito";
//                }
//                else
//                {
//                    ViewData["mensaje0"] = "ERROR";
//                    ViewData["mensaje"] = "No se pudo actualizar la devolución";
//                }
//                return RedirectToAction("Index", "InventarioDevoluciones", new { mensaje = ViewData["mensaje"], mensaje0 = ViewData["mensaje0"] });
//            }
//            catch (Exception ex)
//            {
//                ViewData["mensaje0"] = "ERROR";
//                ViewData["mensaje"] = $"No se pudo actualizar la devolución: {ex.Message}";
//                return RedirectToAction("Index", "InventarioDevoluciones", new { mensaje = ViewData["mensaje"], mensaje0 = ViewData["mensaje0"] });
//            }
//        }

//        public async Task<IActionResult> Details(long id)
//        {
//            var logueado = await ObtenerDatosLogueadoAsync();
//            if (!logueado.IsAuth)
//            {
//                return RedirectToAction("Acceso", "Login");
//            }

//            try
//            {
//                var devolucion = await _httpClient.GetFromJsonAsync<InventarioDevolucion>($"inventarioDevoluciones/{id}");

//                if (devolucion != null)
//                {
//                    return View(devolucion);
//                }
//                else
//                {
//                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró la devolución solicitada" });
//                }
//            }
//            catch (Exception ex)
//            {
//                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Error al recuperar la devolución: {ex.Message}" });
//            }
//        }
//    }
//}
