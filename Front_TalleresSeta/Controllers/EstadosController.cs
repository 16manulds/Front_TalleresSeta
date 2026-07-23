using Front_TalleresSeta.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace Front_TalleresSeta.Controllers
{
    public class EstadosController : Controller
    {
        private readonly HttpClient _httpClient;

        public EstadosController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<IActionResult> Index()
        {
            var estados = await _httpClient.GetFromJsonAsync<List<Estado>>("Estados/todos");
            return View(estados);
        }


        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estado = await _httpClient.GetFromJsonAsync<Estado>($"Estados/obtener/{id}");
            if (estado == null)
            {
                return NotFound();
            }

            return View(estado);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EstadoId,Habilitado,NombreEstado")] Estado estado)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("Estados/crear", estado);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al crear el estado.");
            }
            return View(estado);
        }


        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estado = await _httpClient.GetFromJsonAsync<Estado>($"Estados/modificar/{id}");
            if (estado == null)
            {
                return NotFound();
            }
            return View(estado);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("EstadoId,Habilitado,NombreEstado")] Estado estado)
        {
            if (id != estado.EstadoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"Estados/modificar/{id}", estado);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Error al actualizar el estado.");
                }
                catch
                {
                    if (!await EstadoExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(estado);
        }

        public async Task<IActionResult> Habilitado(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var estado = await _httpClient.GetFromJsonAsync<Estado>($"Estados/habilitado/{id}");
            if (estado == null)
            {
                return NotFound();
            }

            return View(estado);
        }

        [HttpPost, ActionName("Habilitado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var response = await _httpClient.DeleteAsync($"Estados/eliminar/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return Problem("No se pudo eliminar el estado.");
        }

        private async Task<bool> EstadoExists(long id)
        {
            var response = await _httpClient.GetAsync($"Estados/existe/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
