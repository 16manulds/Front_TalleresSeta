using Front_TalleresSeta.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace Front_TalleresSeta.Controllers
{
    public class TipoVehiculosController : Controller
    {
        private readonly HttpClient _httpClient;

        public TipoVehiculosController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: TipoVehiculos
        public async Task<IActionResult> Index()
        {
            try
            {
                var tipoVehiculos = await _httpClient.GetFromJsonAsync<List<TipoVehiculo>>("TipoVehiculos");
                return View(tipoVehiculos);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede cargar la lista de tipos de vehículos: {ex.Message}" });
            }
        }

        // GET: TipoVehiculos/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var tipoVehiculo = await _httpClient.GetFromJsonAsync<TipoVehiculo>($"TipoVehiculos/{id}");
                if (tipoVehiculo == null)
                {
                    return NotFound();
                }
                return View(tipoVehiculo);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede mostrar el detalle del tipo de vehículo: {ex.Message}" });
            }
        }

        // GET: TipoVehiculos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoVehiculos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoVehiculoId,Habilitado,TipoV")] TipoVehiculo tipoVehiculo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("TipoVehiculos", tipoVehiculo);
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Manejo de errores
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede crear el tipo de vehículo: {ex.Message}" });
                }
            }
            return View(tipoVehiculo);
        }

        // GET: TipoVehiculos/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var tipoVehiculo = await _httpClient.GetFromJsonAsync<TipoVehiculo>($"TipoVehiculos/{id}");
                if (tipoVehiculo == null)
                {
                    return NotFound();
                }
                return View(tipoVehiculo);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede mostrar el formulario para editar el tipo de vehículo: {ex.Message}" });
            }
        }

        // POST: TipoVehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("TipoVehiculoId,Habilitado,TipoV")] TipoVehiculo tipoVehiculo)
        {
            if (id != tipoVehiculo.TipoVehiculoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"TipoVehiculos/{id}", tipoVehiculo);
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Manejo de errores
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede actualizar el tipo de vehículo: {ex.Message}" });
                }
            }
            return View(tipoVehiculo);
        }

        // GET: TipoVehiculos/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var tipoVehiculo = await _httpClient.GetFromJsonAsync<TipoVehiculo>($"TipoVehiculos/{id}");
                if (tipoVehiculo == null)
                {
                    return NotFound();
                }
                return View(tipoVehiculo);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede mostrar el formulario para eliminar el tipo de vehículo: {ex.Message}" });
            }
        }

        // POST: TipoVehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"TipoVehiculos/{id}");
                response.EnsureSuccessStatusCode();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"No se puede eliminar el tipo de vehículo: {ex.Message}" });
            }
        }
    }
}
