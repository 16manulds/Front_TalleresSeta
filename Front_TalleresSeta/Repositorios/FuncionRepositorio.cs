using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front_TalleresSeta.Repositorios
{
    public class FuncionRepositorio : IFuncionRepositorio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public FuncionRepositorio(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public DatosLogueado ObtenerDatosLogueadoAsync()
        {
            var result = new DatosLogueado();
            try
            {
                result.IsAuth = _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
                result.IdUser = int.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
                result.TipoUser = _httpContextAccessor.HttpContext.User.FindFirst("TipoUsuarioAcceso").Value;
                result.TipoRol = _httpContextAccessor.HttpContext.User.FindFirst("TipoRolAcceso").Value;
                result.IdTaller = Convert.ToInt64(_httpContextAccessor.HttpContext.User.FindFirst("UsuarioTallerId").Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("El proceso de listar los datos del usuario logueado fallo: {0}", ex.Message);
            }
            return result;
        }

        public async Task<SelectList> ObtenerTallerLogueadoAsync(string tipoUsuario = null, string tipoRol = null, long idTaller = 0)
        {
            try
            {
                List<Taller> talleres = null;
                //string cantTalleres = "UNO";

                if (tipoUsuario == "SuperAdmin")
                {
                    talleres = await _httpClient.GetFromJsonAsync<List<Taller>>("talleres/todos");
                }
                else
                {
                    talleres = await _httpClient.GetFromJsonAsync<List<Taller>>($"talleres/todos?tallerId={idTaller}");
                }

                if (talleres != null)
                {
                    return new SelectList(talleres, "TallerId", "RazonSocialTaller", idTaller);
                }
                else
                {
                    Console.WriteLine("No se encontraron talleres.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("El proceso de obtener los datos del taller logueado falló: {0}", ex.Message);
                return null;
            }
        }


    }
}
