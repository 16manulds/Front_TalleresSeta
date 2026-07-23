using Microsoft.AspNetCore.Mvc;

namespace Front_TalleresSeta.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //public async Task<JsonResult> SubCategorias(long filtroId)
        //{
        //    var response = await _httpClient.GetFromJsonAsync<List<ViewInventarioSubCategoria>>($"InventarioSubCategorias/obtener/{filtroId}");
        //    return Json(new SelectList(response, "InventarioSubCategoriaId", "NombreSubCategoria"));
        //}

    }
}
