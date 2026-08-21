using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioEntradaProductosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IInventarioStockRepositorio _inventarioStockRepo;
        private readonly IInventarioLoteRepositorio _inventarioLoteRepo;
        public static string tabla = "InventarioEntradaProductos";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public InventarioEntradaProductosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IInventarioStockRepositorio inventarioStockRepo, IInventarioLoteRepositorio inventarioLoteRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _inventarioStockRepo = inventarioStockRepo;
            _inventarioLoteRepo = inventarioLoteRepo;
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
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioEntradaProducto>>($"InventarioEntradaProductos/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<InventarioEntradaProducto>>($"InventarioEntradaProductos/todos?search={search}&tallerId={logueado.IdTaller}"));
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

                        var detalle = await _httpClient.GetFromJsonAsync<InventarioEntradaProducto>($"InventarioEntradaProductos/obtener/{id}");
                        var detalleStockProducto = await _httpClient.GetFromJsonAsync<InventarioStock>($"InventarioStocks/obtener/{id}");
                        var detalleLoteProducto = await _httpClient.GetFromJsonAsync<InventarioLote>($"InventarioLotes/obtenerUltimoRegistro/{id}");
                        string imageProducto = string.Empty;
                        string imageCodBarras = string.Empty;

                        ViewBag.Proveedor = detalle.InventarioProveedores.RazonSocial;
                        ViewData["Marca"] = detalle.InventarioMarcas.NombreMarca;
                        ViewData["SubCategoria"] = detalle.InventarioSubCategorias.NombreSubCategoria;
                        ViewData["Categoria"] = detalle.InventarioSubCategorias.InventarioCategorias.NombreCategoria;
                        ViewData["Color"] = detalle.Colores_c.NombreColor;
                        ViewData["UnidadMedida"] = detalle.UnidadMedidas.UnidadMedida;
                        ViewData["Medida"] = detalle.UnidadMedidas.Medidas.NombreMedida;
                        ViewData["TallerId"] = detalle.Talleres.RazonSocialTaller;
                        ViewData["StockActual"] = detalleStockProducto.CantStock;
                        ViewData["PrecioCompra"] = detalleLoteProducto.PrecioCompraXuni;
                        ViewData["PrecioVenta"] = detalleLoteProducto.PrecioVentaXuni;
                        ViewBag.Detalle = "DETALLE";

                        if (detalle.ImagenCodigoDeBarras != null && detalle.ImagenCodigoDeBarras.Length > 0)
                        {
                            var base64CodBarras = Convert.ToBase64String(detalle.ImagenCodigoDeBarras);
                            imageCodBarras = $"data:image/png;base64,{base64CodBarras}";
                        }
                        ViewData["CodigoDeBarras"] = imageCodBarras;

                        if (detalle.ImagenProducto != null && detalle.ImagenProducto.Length > 0)
                        {
                            var base64Producto = Convert.ToBase64String(detalle.ImagenProducto);
                            imageProducto = $"data:image/png;base64,{base64Producto}";
                        }
                        ViewData["ImagenProducto"] = imageProducto;
                        //ViewData["ImagenProducto"] = detalle.ImagenProducto;
                        return View(detalle);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del producto, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del producto.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Create()
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
                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todosHabilitados");
                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria");

                    var subCategorias = await _httpClient.GetFromJsonAsync<List<InventarioSubCategoria>>($"InventarioSubCategorias/todos");
                    ViewData["InventarioSubCategoriaId"] = new SelectList(subCategorias, "InventarioSubCategoriaId", "NombreSubCategoria");

                    var medidas = await _httpClient.GetFromJsonAsync<List<Medida_c>>($"Medidas/todos");
                    ViewData["Medida_cId"] = new SelectList(medidas, "Medida_cId", "NombreMedida");

                    var UnidadMedidas = await _httpClient.GetFromJsonAsync<List<UnidadMedida_c>>($"UnidadMedidas/todos");
                    ViewData["UnidadMedida_cId"] = new SelectList(UnidadMedidas, "UnidadMedida_cId", "UnidadMedida");

                    var colores = await _httpClient.GetFromJsonAsync<List<Color_c>>($"Colores/todos");
                    ViewData["Color_cId"] = new SelectList(colores, "Color_cId", "NombreColor");

                    var marcas = await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todosHabilitados");
                    ViewData["InventarioMarcaId"] = new SelectList(marcas, "InventarioMarcaId", "NombreMarca");

                    var proveedores = await _httpClient.GetFromJsonAsync<List<InventarioProveedor>>($"InventarioProveedores/todosHabilitados");
                    ViewData["InventarioProveedorId"] = new SelectList(proveedores, "InventarioProveedorId", "RazonSocial");

                    ViewData["TallerId"] = talleres;
                    ViewData["mensaje"] = "Registra un producto.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear un producto.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodigoProducto,Habilitado,FechaRegistro,NombreProducto,Referencia,Homologado,Detalle,InventarioProveedorId,InventarioMarcaId,InventarioSubCategoriaId,TallerId,Color_cId,UnidadMedida_cId,PosicionProducto,UbicacionProducto,VehiculoAsociado,StockMinimo,ImagenCodigoDeBarras,ConsecutivoCodBarras,ImagenProducto")] InventarioEntradaProducto model, string PrecioCompraPorUni, string PrecioVentaPorUni, int CantidadIngresa = 0, long idCategoria = 0, long? idMedida_c = 0, byte[]? ProductoImagen = null)
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

                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todosHabilitados");
                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria", idCategoria);

                    var subCategorias = await _httpClient.GetFromJsonAsync<List<InventarioSubCategoria>>($"InventarioSubCategorias/SubCategoriasPorCategoriaId/{idCategoria}");
                    ViewData["InventarioSubCategoriaId"] = new SelectList(subCategorias, "InventarioSubCategoriaId", "NombreSubCategoria", model.InventarioSubCategoriaId);

                    var medidas = await _httpClient.GetFromJsonAsync<List<Medida_c>>($"Medidas/todos");
                    ViewData["Medida_cId"] = new SelectList(medidas, "Medida_cId", "NombreMedida", idMedida_c);

                    var UnidadMedidas = await _httpClient.GetFromJsonAsync<List<UnidadMedida_c>>($"UnidadMedidas/UnidadMedidasPorMedidaId/{idMedida_c}");
                    ViewData["UnidadMedida_cId"] = new SelectList(UnidadMedidas, "UnidadMedida_cId", "UnidadMedida", model.UnidadMedida_cId);

                    var colores = await _httpClient.GetFromJsonAsync<List<Color_c>>($"Colores/todos");
                    ViewData["Color_cId"] = new SelectList(colores, "Color_cId", "NombreColor", model.Color_cId);

                    var marcas = await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todosHabilitados");
                    ViewData["InventarioMarcaId"] = new SelectList(marcas, "InventarioMarcaId", "NombreMarca", model.InventarioMarcaId);

                    var proveedores = await _httpClient.GetFromJsonAsync<List<InventarioProveedor>>($"InventarioProveedores/todosHabilitados");
                    ViewData["InventarioProveedorId"] = new SelectList(proveedores, "InventarioProveedorId", "RazonSocial", model.InventarioProveedorId);

                    string imageUrl = string.Empty;
                    if (model.ImagenProducto != null && model.ImagenProducto.Length > 0)
                    {
                        var base64Image = Convert.ToBase64String(model.ImagenProducto);
                        imageUrl = $"data:image/png;base64,{base64Image}";
                    }
                    ViewData["ImagenProducto"] = imageUrl;

                    string pCompra = Regex.Replace(PrecioCompraPorUni, "[^0-9]", "");
                    string pVenta = Regex.Replace(PrecioVentaPorUni, "[^0-9]", "");
                    var precioCompra = Convert.ToInt64(pCompra);
                    var precioVenta = Convert.ToInt64(pVenta);

                    ViewData["PrecioCompraPorUni"] = precioCompra;
                    ViewData["PrecioVentaPorUni"] = precioVenta;
                    ViewData["CantidadIngresa"] = CantidadIngresa;
                    ViewData["TallerId"] = talleres;

                    if (ModelState.IsValid)
                    {
                        if (model.ImagenProducto == null)
                        {
                            model.ImagenProducto = ProductoImagen;
                        }

                        string idItem = string.Empty;
                        model.Habilitado = true;
                        model.FechaRegistro = DateTime.Now;
                        model.Detalle ??= "N/A";

                        var modelLote = new InventarioLote
                        {
                            Habilitado = true,
                            FechaRegistro = model.FechaRegistro,
                            PrecioCompraXuni = precioCompra,
                            PrecioVentaXuni = precioVenta,
                            CantIngresan = CantidadIngresa,
                            CodigoProducto = model.CodigoProducto,
                            TallerId = model.TallerId
                        };

                        var modelStock = new InventarioStock
                        {
                            Habilitado = true,
                            FechaRegistroInicial = model.FechaRegistro,
                            FechaRegistroUpdate = DateTime.Now,
                            CantStock = CantidadIngresa,
                            CantVendidos = 0,
                            CodigoProducto = model.CodigoProducto,
                            TallerId = model.TallerId
                        };

                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"InventarioEntradaProductos/existe/{model.CodigoProducto}");
                        if (existe == true)
                        {
                            try
                            {
                                //mensaje = "Hay caramba!, el producto a registrar ya existe.";
                                // Actualizar el producto
                                var responseEdit = await _httpClient.PutAsJsonAsync("InventarioEntradaProductos/modificar", model);
                                // Verificar si la API devolvió una respuesta exitosa
                                if (!responseEdit.IsSuccessStatusCode)
                                {
                                    mensaje = $"Error al actualizar el producto: {responseEdit.StatusCode}";
                                    return View(model);
                                }
                                var jsonResponse = await responseEdit.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                {
                                    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                    mensaje = "Error: No se recibió el ID del producto al actualizar.";
                                    return View(model);
                                }
                                idItem = responseObject.GetProperty("id").ToString();
                            }
                            catch (Exception)
                            {
                                mensaje = $"No se puede actualizar el producto.";
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }
                        else
                        {
                            try
                            {
                                //Registrar el producto
                                var responseCreate = await _httpClient.PostAsJsonAsync("InventarioEntradaProductos/crear", model);
                                // Verificar si la API devolvió una respuesta exitosa
                                if (!responseCreate.IsSuccessStatusCode)
                                {
                                    mensaje = $"Error al crear el producto: {responseCreate.StatusCode}";
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
                                idItem = responseObject.GetProperty("id").ToString();
                            }
                            catch (Exception)
                            {
                                mensaje = $"No se puede crear el producto.";
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }

                        long LoteId = 0;
                        long idStock = 0;

                        // Registrar lote                                
                        LoteId = await _inventarioLoteRepo.CrearAsync(modelLote);

                        // Registrar stock
                        if (await _inventarioStockRepo.ExisteModeloAsync(modelStock.CodigoProducto))
                        {
                            idStock = await _inventarioStockRepo.ActualizarAsync(modelStock);
                        }
                        else
                        {
                            idStock = await _inventarioStockRepo.CrearAsync(modelStock);
                        }

                        //validar todos los registros estén ok
                        if (!string.IsNullOrEmpty(idItem))
                        {
                            if (LoteId > 0)
                            {
                                if (idStock > 0)
                                {
                                    mensaje = "Registro satisfactorio...";
                                    accion = "CREADO";
                                    return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                                }
                                else
                                {
                                    accion = "CREADO_CON_ERROR";
                                    mensaje = $"El producto se creo y se asocio a un lote, aunque no se logró ingresar al stock.";
                                    return View(model);
                                }
                            }
                            else
                            {
                                accion = "CREADO_CON_ERROR";
                                mensaje = $"El producto se creo, aunque no se logró asociar el producto al lote y al stock.";
                                return View(model);
                            }
                        }
                        else
                        {
                            accion = "CREADO_CON_ERROR";
                            mensaje = $"El producto no se logró crear.";
                            return View(model);
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
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el producto: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<InventarioEntradaProducto>($"InventarioEntradaProductos/obtener/{id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el producto a modificar." });
                    }

                    var idCategoria = model.InventarioSubCategorias.InventarioCategorias.InventarioCategoriaId;
                    var idSubCategoria = model.InventarioSubCategorias.InventarioSubCategoriaId;
                    var idMedida = model.UnidadMedidas.Medidas.Medida_cId;

                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todosHabilitados");
                    var subCategorias = await _httpClient.GetFromJsonAsync<List<InventarioSubCategoria>>($"InventarioSubCategorias/SubCategoriasPorCategoriaId/{idCategoria}");
                    var medidas = await _httpClient.GetFromJsonAsync<List<Medida_c>>($"Medidas/todos");
                    var UnidadMedidas = await _httpClient.GetFromJsonAsync<List<UnidadMedida_c>>($"UnidadMedidas/UnidadMedidasPorMedidaId/{idMedida}");
                    var colores = await _httpClient.GetFromJsonAsync<List<Color_c>>($"Colores/todos");
                    var marcas = await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todosHabilitados");
                    var proveedores = await _httpClient.GetFromJsonAsync<List<InventarioProveedor>>("InventarioProveedores/todosHabilitados");

                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria", idCategoria);
                    ViewData["InventarioSubCategoriaId"] = new SelectList(subCategorias, "InventarioSubCategoriaId", "NombreSubCategoria", idSubCategoria);
                    ViewData["Medida_cId"] = new SelectList(medidas, "Medida_cId", "NombreMedida", idMedida);
                    ViewData["UnidadMedida_cId"] = new SelectList(UnidadMedidas, "UnidadMedida_cId", "UnidadMedida", model.UnidadMedidas.UnidadMedida_cId);
                    ViewData["Color_cId"] = new SelectList(colores, "Color_cId", "NombreColor", model.Colores_c.Color_cId);
                    ViewData["InventarioMarcaId"] = new SelectList(marcas, "InventarioMarcaId", "NombreMarca", model.InventarioMarcas.InventarioMarcaId);
                    ViewBag.InventarioProveedorId = new SelectList(proveedores, "InventarioProveedorId", "RazonSocial", model.InventarioProveedores.InventarioProveedorId);
                    //ViewData["CodigoDeBarras"] = model.ImagenCodigoDeBarras;

                    string imageUrl = string.Empty;
                    if (model.ImagenProducto != null && model.ImagenProducto.Length > 0)
                    {
                        var base64Image = Convert.ToBase64String(model.ImagenProducto);
                        imageUrl = $"data:image/png;base64,{base64Image}";
                    }
                    ViewData["ImagenProducto"] = imageUrl;

                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el producto.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodigoProducto,Habilitado,FechaRegistro,NombreProducto,Referencia,Homologado,Detalle,InventarioProveedorId,InventarioMarcaId,InventarioSubCategoriaId,TallerId,Color_cId,UnidadMedida_cId,PosicionProducto,UbicacionProducto,VehiculoAsociado,StockMinimo,ImagenCodigoDeBarras,ConsecutivoCodBarras,ImagenProducto")] InventarioEntradaProducto model, long InventarioProveedorId, long InventarioMarcaId, long InventarioSubCategoriaId, long InventarioCategoriaId, long? idMedida_c = 0, long? UnidadMedida_cId = 0)
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
                        // Actualizar el producto
                        var responseEdit = await _httpClient.PutAsJsonAsync($"InventarioEntradaProductos/modificar", model);
                        // Verificar si la API devolvió una respuesta exitosa
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar el producto: {responseEdit.StatusCode}";
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
                    var idCategoria = InventarioCategoriaId;
                    var idSubCategoriaId = InventarioSubCategoriaId;
                    var idMedida = idMedida_c;
                    var idUnidadMedida = UnidadMedida_cId;
                    var idProveedor = InventarioProveedorId;
                    var idMarca = InventarioMarcaId;

                    var categorias = await _httpClient.GetFromJsonAsync<List<InventarioCategoria>>($"InventarioCategorias/todosHabilitados");
                    ViewData["InventarioCategoriaId"] = new SelectList(categorias, "InventarioCategoriaId", "NombreCategoria", idCategoria);

                    var subCategorias = await _httpClient.GetFromJsonAsync<List<InventarioSubCategoria>>($"InventarioSubCategorias/SubCategoriasPorCategoriaId/{idCategoria}");
                    ViewData["InventarioSubCategoriaId"] = new SelectList(subCategorias, "InventarioSubCategoriaId", "NombreSubCategoria", idSubCategoriaId);

                    var medidas = await _httpClient.GetFromJsonAsync<List<Medida_c>>($"Medidas/todos");
                    ViewData["Medida_cId"] = new SelectList(medidas, "Medida_cId", "NombreMedida", idMedida);

                    var UnidadMedidas = await _httpClient.GetFromJsonAsync<List<UnidadMedida_c>>($"UnidadMedidas/UnidadMedidasPorMedidaId/{idMedida}");
                    ViewData["UnidadMedida_cId"] = new SelectList(UnidadMedidas, "UnidadMedida_cId", "UnidadMedida", idUnidadMedida);

                    var colores = await _httpClient.GetFromJsonAsync<List<Color_c>>($"Colores/todos");
                    ViewData["Color_cId"] = new SelectList(colores, "Color_cId", "NombreColor", model.Color_cId);

                    var marcas = await _httpClient.GetFromJsonAsync<List<InventarioMarca>>($"InventarioMarcas/todosHabilitados");
                    ViewData["InventarioMarcaId"] = new SelectList(marcas, "InventarioMarcaId", "NombreMarca", idMarca);

                    var proveedores = await _httpClient.GetFromJsonAsync<List<InventarioProveedor>>("InventarioProveedores/todosHabilitados");
                    ViewBag.InventarioProveedorId = new SelectList(proveedores, "InventarioProveedorId", "RazonSocial", idProveedor);

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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"InventarioEntradaProductos/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El producto fue eliminada.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el producto. Cuenta con registros asociados";
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
