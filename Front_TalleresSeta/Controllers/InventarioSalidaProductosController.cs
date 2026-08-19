using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Front_TalleresSeta.Controllers
{
    public class InventarioSalidaProductosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IInventarioStockRepositorio _inventarioStockRepo;
        private readonly IInventarioLoteRepositorio _inventarioLoteRepo;
        private readonly ITipoDocumentoRepositorio _tipoDocumentoRepo;
        private readonly ITipoVehiculoRepositorio _tipoVehiculoRepo;
        private readonly IPedidoRepositorio _pedidoRepo;
        private readonly IInventarioSalidaProductoRepositorio _inventarioSalidaProducto;
        public static string tabla = "InventarioSalidaProductos";
        public static string mensaje = string.Empty;
        public static string idItem = string.Empty;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public InventarioSalidaProductosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IInventarioStockRepositorio inventarioStockRepo, IInventarioLoteRepositorio inventarioLoteRepo, ITipoDocumentoRepositorio tipoDocumentoRepo, ITipoVehiculoRepositorio tipoVehiculoRepo, IPedidoRepositorio pedidoRepo, IInventarioSalidaProductoRepositorio inventarioSalidaProducto  )
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _inventarioStockRepo = inventarioStockRepo;
            _inventarioLoteRepo = inventarioLoteRepo;
            _tipoDocumentoRepo = tipoDocumentoRepo;
            _tipoVehiculoRepo = tipoVehiculoRepo;
            _pedidoRepo = pedidoRepo;
            _inventarioSalidaProducto = inventarioSalidaProducto;
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
                    ViewData["FechaVenta"] = new SelectList(annosRegistrados, "Annos", "Annos", search);

                    if (logueado.TipoUser == "SuperAdmin")
                        return View(await _httpClient.GetFromJsonAsync<List<ViewVentaProducto>>($"InventarioSalidaProductos/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<ViewVentaProducto>>($"InventarioSalidaProductos/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de productos" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los productos vendidos.";
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

                        var detalle = await _httpClient.GetFromJsonAsync<ViewVentaProducto>($"InventarioSalidaProductos/obtener/{id}");

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
                    //Registrar ID Pedido
                    //var idPedido = await _pedidoRepo.CrearPedidoAsync(logueado.IdTaller);
                    //ViewBag.PedidoId = idPedido.consecutivo;

                    SelectList tipoDocumentos = await _tipoDocumentoRepo.ObtenerTipoDeDocumentosAsync(logueado.IdTaller);
                    ViewBag.TipoDocumentoId = tipoDocumentos;

                    SelectList tipoVehiculos = await _tipoVehiculoRepo.ObtenerTipoDeVehiculosAsync(logueado.IdTaller);
                    ViewBag.TipoVehiculoId = tipoVehiculos;


                    ViewData["mensaje"] = "Registra la venta de un producto.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear la venta del producto.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }


        [HttpPut]
        public async Task<IActionResult> AgregarProductoVenta([FromQuery] long filtroId, [FromBody] ViewAgregarProducto model)
        {
            if (filtroId <= 0)
            {
                return BadRequest(new { success = false, mensaje = "El identificador del taller es requerido." });
            }

            if (model == null || model.PedidoId <= 0)
            {
                return BadRequest(new { success = false, mensaje = "Los datos del producto o el ID del pedido no son válidos." });
            }

            try
            {
                // Invoca el repositorio o servicio backend que guarda el producto en BD
                long idProductoGuardado = await _inventarioSalidaProducto.AgregarProductoVentaAsync(filtroId, model);

                if (idProductoGuardado <= 0)
                {
                    return StatusCode(500, new { success = false, mensaje = "No se pudo insertar el producto en la base de datos." });
                }

                // Retorna la propiedad 'id' esperada por el fetch en JavaScript
                return Json(new { id = idProductoGuardado });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { success = false, mensaje = "Error de comunicación con el servicio de Inventario.", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mensaje = "Ocurrió un error inesperado al agregar el producto.", detalle = ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Habilitado, CodigoProducto, FechaVenta, NombreProducto, UnidadesStock, PrecioVentaPorUni, PrecioVentaTotal, PrecioTotalPagado, CantVendidos, CantDevoluciones,  MetodoDePago, ImagenProducto, NombreProveedor, NombreMarca, NombreCategoria, NombreSubCategoria, NombreColor, NombreMedida, UnidadMedida, Taller, TallerId, Pagos")] ViewVentaProducto model)
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
                        DateTime fechaRegistro = DateTime.Now;
                        long idVenta = 0;
                        long idPago = 0;
                        long idLote = 0;
                        string idStock = "0";
                        long idGanancia = 0;


                        if (model.PrecioVentaTotal == model.PrecioTotalPagado)
                        {
                            bool existeP = await _httpClient.GetFromJsonAsync<bool>($"InventarioEntradaProductos/existe/{model.CodigoProducto}");

                            if (existeP == true)
                            {
                                var stockProducto = await _httpClient.GetFromJsonAsync<InventarioStock>($"InventarioStocks/obtener/{model.CodigoProducto}");

                                if (!string.IsNullOrEmpty(stockProducto!.CodigoProducto))
                                {
                                    if (stockProducto!.CantStock >= model.CantVendidos)
                                    {
                                        accion = "CREADO";


                                        string precioUnidad = Regex.Replace(model.PrecioVentaPorUni!, "[^0-9]", "");
                                        string precioVenta = Regex.Replace(model.PrecioVentaTotal!, "[^0-9]", "");
                                        string precioVentaPagada = Regex.Replace(model.PrecioTotalPagado!, "[^0-9]", "");
                                        Int64 pUnidad = Convert.ToInt64(precioUnidad);
                                        Int64 pVenta = Convert.ToInt64(precioVenta);
                                        Int64 pVentaPagada = Convert.ToInt64(precioVentaPagada);


                                        //modelo de la venta
                                        var modelVentaProducto = new InventarioSalidaProducto
                                        {
                                            Habilitado = true,
                                            FechaRegistro = fechaRegistro,
                                            PrecioFinalXuni = pUnidad,
                                            //PrecioFinalVenta = pVenta,
                                            //PrecioFinalVentaPagado = pVentaPagada,
                                            CantVendidos = model.CantVendidos,
                                            CantDevoluciones = model.CantDevoluciones,
                                            //Detalle = model.Detalle ??= "N/A",
                                            TallerId = model.TallerId,
                                            CodigoProducto = model.CodigoProducto!
                                        };

                                        //Registrar venta del producto
                                        var responseCreate = await _httpClient.PostAsJsonAsync("InventarioSalidaProductos/crear", modelVentaProducto);

                                        if (!responseCreate.IsSuccessStatusCode)
                                        {
                                            mensaje = $"Error al crear la venta del producto: {responseCreate.StatusCode}";
                                            return View(model);
                                        }
                                        var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                        var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                        if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                        {
                                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                            mensaje = "Error: No se recibió el ID de la venta.";
                                            return View(model);
                                        }
                                        idVenta = idElement.GetInt64();


                                        // Insertar metodos de pagos
                                        if (model.Pagos != null && model.Pagos.Count != 0)
                                        {
                                            foreach (var pago in model.Pagos)
                                            {
                                                var modelPago = new Pago
                                                {
                                                    Habilitado = true,
                                                    FechaRegistro = fechaRegistro,
                                                    ValorPago = pago.Monto!,
                                                    //OrigenPago = idItem,
                                                    TipoTarjetaPagoId = pago.TipoTarjetaId,
                                                    MetodoDePagoId = pago.MetodoId,
                                                    BancoId = pago.BancoId,
                                                    //CodigoProducto = model.CodigoProducto,
                                                    TallerId = model.TallerId,
                                                    //IdInventarioSalidaProducto = idVenta
                                                };

                                                //Registrar venta del pago
                                                var responseCreatePagos = await _httpClient.PostAsJsonAsync("api/Pagos/crear", modelPago);

                                                if (!responseCreatePagos.IsSuccessStatusCode)
                                                {
                                                    mensaje = $"Error al crear el pago del producto: {responseCreatePagos.StatusCode}";
                                                    return View(model);
                                                }
                                                var jsonResponsePagos = await responseCreate.Content.ReadAsStringAsync();
                                                var responseObjectPagos = JsonSerializer.Deserialize<JsonElement>(jsonResponsePagos);
                                                if (!responseObjectPagos.TryGetProperty("id", out JsonElement idElementPagos))
                                                {
                                                    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponsePagos);
                                                    mensaje = "Error: No se recibió el ID del pago.";
                                                    return View(model);
                                                }
                                                idPago = idElementPagos.GetInt64();
                                            }
                                        }


                                        // Variables de control
                                        int unidadesPorProcesar = model.CantVendidos;
                                        int cantidadPendiente = 0;

                                        // Bucle para agotar la cantidad vendida contra los lotes existentes
                                        while (unidadesPorProcesar > 0)
                                        {
                                            // Preparar modelo para actualizar el lote actual
                                            InventarioLote modelLote = new()
                                            {
                                                CodigoProducto = model.CodigoProducto,
                                                CantRestante = unidadesPorProcesar,
                                                TallerId = model.TallerId
                                            };

                                            // Llamada a la API de Lotes
                                            var responseUpdateLote = await _httpClient.PutAsJsonAsync("InventarioLotes/modificarLoteVenta", modelLote);

                                            if (!responseUpdateLote.IsSuccessStatusCode)
                                            {
                                                mensaje = $"Error al actualizar lote: {responseUpdateLote.StatusCode}";
                                                return View(model);
                                            }

                                            var jsonResultLote = await responseUpdateLote.Content.ReadAsStringAsync();
                                            var resultObjectLote = JsonSerializer.Deserialize<JsonElement>(jsonResultLote);

                                            // Extraer ID del lote y Cantidad Pendiente
                                            idLote = resultObjectLote.GetProperty("id").GetInt64();
                                            cantidadPendiente = resultObjectLote.GetProperty("cantidadPendiente").GetInt32();
                                            int unidadesDescontadasEnEstePaso = unidadesPorProcesar - cantidadPendiente;

                                            if (idLote == 0 && unidadesPorProcesar > 0)
                                            {
                                                mensaje = "Error: No hay suficiente stock en los lotes para completar la venta.";
                                                break;
                                            }

                                            var loteActual = await _httpClient.GetFromJsonAsync<InventarioLote>($"InventarioLotes/obtenerLote?idLote={idLote}&codProducto={model.CodigoProducto}");

                                            // Calcular Ganancia Proporcional para este lote específico
                                            long precioCompraUnidadLoteActual = loteActual!.PrecioCompraXuni;
                                            //se toma el precio de compra por unidad del lote actual
                                            long costoTotalLote = precioCompraUnidadLoteActual * unidadesDescontadasEnEstePaso;
                                            //se toma pUnidad que es el precio de venta por unidad del producto, para calcular la ganancia
                                            long ventaTotalLote = pUnidad * unidadesDescontadasEnEstePaso;
                                            long gananciaCalculada = ventaTotalLote - costoTotalLote;

                                            var modelGanancia = new InventarioGanancia
                                            {
                                                Habilitado = true,
                                                FechaRegistro = fechaRegistro,
                                                CantVendida = unidadesDescontadasEnEstePaso,
                                                PrecioCompraXuni = precioCompraUnidadLoteActual,
                                                PrecioVentaXuni = pUnidad,
                                                Ganancia = gananciaCalculada,
                                                InventarioSalidaProductoId = idVenta,
                                                IdLote = idLote,
                                                CodigoProducto = model.CodigoProducto,
                                                TallerId = model.TallerId
                                            };

                                            // Registrar Ganancia
                                            var responseCreateGanancia = await _httpClient.PostAsJsonAsync("InventarioGanancias/crear", modelGanancia);

                                            // Control de errores de ganancia...
                                            if (responseCreateGanancia.IsSuccessStatusCode)
                                            {
                                                var resGan = await responseCreateGanancia.Content.ReadAsStringAsync();
                                                idGanancia = JsonSerializer.Deserialize<JsonElement>(resGan).GetProperty("id").GetInt64();
                                            }

                                            unidadesPorProcesar = cantidadPendiente;
                                        }


                                        //actualizar stock
                                        idStock = await _inventarioStockRepo.ActualizarCantidadStockAsync(model.CodigoProducto!);


                                        //// modelo al lote
                                        //InventarioLote modelLote = new()
                                        //{
                                        //    CodigoProducto = model.CodigoProducto!,
                                        //    //se envian las unidades vendidas para que el lote reste esa cantidad a su cantidad restante, y asi actualizar el lote
                                        //    CantRestante = model.CantVendidos!,
                                        //    TallerId = model.TallerId
                                        //};

                                        ////Actualizar lote
                                        //var responseUpdateLote = await _httpClient.PutAsJsonAsync("InventarioLotes/modificarLoteVenta", modelLote);

                                        //if (!responseUpdateLote.IsSuccessStatusCode)
                                        //{
                                        //    mensaje = $"Error al actualizar el lote por venta de producto: {responseUpdateLote.StatusCode}";
                                        //    return View(model);
                                        //}
                                        //var jsonResultLote = await responseUpdateLote.Content.ReadAsStringAsync();
                                        //var resultObjectLote = JsonSerializer.Deserialize<JsonElement>(jsonResultLote);
                                        //if (!resultObjectLote.TryGetProperty("id", out JsonElement idResultLote))
                                        //{
                                        //    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResultLote);
                                        //    mensaje = "Error: No se recibió el ID del lote.";
                                        //    return View(model);
                                        //}
                                        //idLote = idResultLote.GetInt64();


                                        ////actualizar stock
                                        //idStock = await _inventarioStockRepo.ActualizarCantidadStockAsync(model.CodigoProducto!);


                                        //Int64 costoCantUnidad = pUnidad * model.CantVendidos;
                                        //Int64 gananciaCalculada = (costoCantUnidad - pVenta);

                                        ////Insertar las ganancias
                                        //var modelGanancia = new InventarioGanancia
                                        //{
                                        //    Habilitado = true,
                                        //    FechaRegistro = fechaRegistro,
                                        //    CantVendida = model.CantVendidos,
                                        //    PrecioCompraXuni = pUnidad,
                                        //    PrecioVentaXuni = pVenta,
                                        //    Ganancia = gananciaCalculada,
                                        //    InventarioSalidaProductoId = idVenta,
                                        //    IdLote = idLote,
                                        //    CodigoProducto = model.CodigoProducto,
                                        //    TallerId = model.TallerId,

                                        //};

                                        ////Registrar venta del producto
                                        //var responseCreateGanancia = await _httpClient.PostAsJsonAsync("InventarioGanancias/crear", modelGanancia);

                                        //if (!responseCreateGanancia.IsSuccessStatusCode)
                                        //{
                                        //    mensaje = $"Error al crear la Ganancia de la venta: {responseCreate.StatusCode}";
                                        //    return View(model);
                                        //}
                                        //var jsonResponseGanancia = await responseCreateGanancia.Content.ReadAsStringAsync();
                                        //var responseObjectGanancia = JsonSerializer.Deserialize<JsonElement>(jsonResponseGanancia);
                                        //if (!responseObjectGanancia.TryGetProperty("id", out JsonElement jsonGananciaId))
                                        //{
                                        //    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseGanancia);
                                        //    mensaje = "Error: No se recibió el ID de la Ganancia.";
                                        //    return View(model);
                                        //}
                                        //idGanancia = jsonGananciaId.GetInt64();



                                        //venta, pagos, lote, stock y ganancias
                                        if (!string.IsNullOrEmpty(idItem))
                                        {
                                            if (idLote > 0)
                                            {
                                                if (!string.IsNullOrEmpty(idStock))
                                                {
                                                    mensaje = "Venta satisfactoria...";
                                                    accion = "CREADO";
                                                    return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                                                }
                                                else
                                                {
                                                    accion = "CREADO_CON_ERROR";
                                                    mensaje = $"Se registra la venta del producto y se asocio a un lote, aunque no se logró actualizar en el stock.";
                                                    return View(model);
                                                }
                                            }
                                            else
                                            {
                                                accion = "CREADO_CON_ERROR";
                                                mensaje = $"Se registro la venta del producto, aunque no se logró asociar el producto al lote y al stock.";
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
                                        accion = "FALLO";
                                        mensaje = $"No se pueden vender más de " + model.UnidadesStock + " unidades, del actual Stock.";
                                    }
                                }
                            }
                            else
                            {
                                accion = "FALLO";
                                mensaje = $"El producto de código: " + model.CodigoProducto + " no esta registrado en el stock.";
                            }
                        }
                        else
                        {
                            accion = "FALLO";
                            mensaje = $"Por favor valida el valor pagado. Debe estar a la totalidad del valor de la venta.";
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
                catch (Exception)
                {
                    mensaje = $"No se puede registrar la venta del producto.";
                    //mensajeError = $"Error ocurrido: {ex.Message}";
                    ViewData["TallerId"] = talleres;
                    ViewData["mensaje"] = mensaje;
                    ViewData["accion"] = accion;
                    return View(model);
                    //return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
