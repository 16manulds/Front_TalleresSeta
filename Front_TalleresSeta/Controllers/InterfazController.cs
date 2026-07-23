using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;

namespace Front_TalleresSeta.Controllers
{
    public class InterfazController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        //public static string tabla = "Home";
        //public static string mensaje = string.Empty;
        //public static string idItem = string.Empty;
        //public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        //public static SelectList talleres = null!;

        public InterfazController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
        }

                
        public async Task<IActionResult> Index(string? mensaje = null, string? accion = null)
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
                    var modelo = await _httpClient.GetFromJsonAsync<ViewInterfazDashboardIndex>($"Funciones/DatosDashboardIndex/{logueado.IdTaller}");

                    // Validar si el modelo es null (ej. si la API no devolvió datos)
                    if (modelo == null)
                    {
                        mensaje = "No se pudieron obtener los datos del dashboard.";
                        mensajeError = "La API no devolvió un modelo válido.";
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado a los datos del index en el Dashboard." });
                    }
                                                          
                    if (logueado.TipoUser == "SuperAdmin")
                        return View(modelo);
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(modelo);
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado a los datos del index en el Dashboard." });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar los datos de la index en el Dashboard.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }




        public IActionResult MuyPronto()
        {
            return View();
        }

        public IActionResult AccesoDenegado(string mensaje, string mensajeError)
        {
            ViewData["mensaje"] = mensaje;
            ViewData["mensajeError"] = mensajeError;
            return View();
        }

        public IActionResult SinDataos(string mensaje)
        {
            ViewData["mensaje"] = mensaje;
            return View();
        }
    }
}
