using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Controllers
{
    public class FuncionesController : Controller
    {
        private readonly HttpClient _httpClient;
                
        public FuncionesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<JsonResult> SubCategorias(long filtroId)
        {
            var subCategorias = await _httpClient.GetFromJsonAsync<List<ViewInventarioSubCategoria>>($"InventarioSubCategorias/SubCategoriasPorCategoriaId/{filtroId}");
            return Json(new SelectList(subCategorias, "InventarioSubCategoriaId", "NombreSubCategoria"));
        }

        public async Task<JsonResult> UnidadMedidas(long filtroId)
        {
            var unidadMedidas = await _httpClient.GetFromJsonAsync<List<UnidadMedida_c>>($"UnidadMedidas/UnidadMedidasPorMedidaId/{filtroId}");
            return Json(new SelectList(unidadMedidas, "UnidadMedida_cId", "UnidadMedida"));
        }

        public async Task<JsonResult> ListarTodosLosProductosVentas()
        {
            var productos = await _httpClient.GetFromJsonAsync<List<ViewInventarioStockProducto>>("InventarioStock/listarTodosLosProductosVentas");
            return Json(new SelectList(productos, "InventarioStockId", "NombreProducto"));
        }

        public async Task<JsonResult> CargarDatosProducto(long filtroId)
        {
            var producto = await _httpClient.GetFromJsonAsync<ViewInventarioStockProducto>($"InventarioStockProductos/obtener/{filtroId}");
            return Json(producto);
        }

        public async Task<JsonResult> CargarProductoVendido(long filtroId)
        {
            var productoVendido = await _httpClient.GetFromJsonAsync<ViewInventarioSalidaProducto>($"InventarioSalidaProductos/obtener/{filtroId}");
            return Json(productoVendido);
        }
                
        public async Task<JsonResult> ObtenerUltimoCodigoDeBarrasGenerado()
        {
            var ultimoCodigoDeBarras = await _httpClient.GetFromJsonAsync<int>($"InventarioEntradaProductos/obtenerUltimoCodigoDeBarras");

            return Json(ultimoCodigoDeBarras); // Devuelve el número como JSON al frontend
        }

        public async Task<JsonResult> CargarProductoExiste(string filtroId)
        {
            var producto = await _httpClient.GetFromJsonAsync<InventarioEntradaProducto>($"InventarioEntradaProductos/obtener/{filtroId}");
            return Json(producto);
        }
        
        public async Task<JsonResult> CargarProductoExisteParaVenta(string filtroId)
        {
            var producto = await _httpClient.GetFromJsonAsync<ViewCargarProductoParaVenta>($"InventarioEntradaProductos/cargarProductoExisteParaVenta/{filtroId}");
            return Json(producto);
        }


        public async Task<JsonResult> ActualCantidadIngresanPorLote(long LoteId, string CodigoProducto)
        {
            var actualCantidadIngresan = await _httpClient.GetFromJsonAsync<InventarioLote>($"InventarioLotes/actualCantidadIngresanPorLote?LoteId={LoteId}&codProducto={CodigoProducto}");

            return Json(actualCantidadIngresan); // Devuelve la cantidad actual ingresada en el lote seleccionado
        }

        public async Task<JsonResult> ValidarPlaca(string filtroId)
        {
            bool existe = await _httpClient.GetFromJsonAsync<bool>($"Vehiculos/existe/{filtroId}");
            return Json(existe);
        }

        public async Task<JsonResult> CantidadMetodosPago()
        {
            int result = await _httpClient.GetFromJsonAsync<int>($"Sis_MetodosDePagos/cantidadMetodos/");
            return Json(result);
        }

    }
}
