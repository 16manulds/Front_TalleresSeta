using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.Complementos;
using Front_TalleresSeta.Permisos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Front_TalleresSeta.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }


        public async Task<IActionResult> Logout()
        {
            // Eliminar las cookies
            Response.Cookies.Delete("CookieName1");
            Response.Cookies.Delete("CookieName2");

            await HttpContext.SignOutAsync();
            return RedirectToAction("Acceso", "Login");
            //return Redirect("/Principal/Index");
        }

        public IActionResult Acceso()
        {
            bool isAuth = User.Identity.IsAuthenticated;
            if (!isAuth) return View("Acceso");
            return RedirectToAction("Index", "Interfaz");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acceso(long id, [Bind(UsuariosCustomBind.Login)] Login login)
        {
            bool isAuth = HttpContext.User.Identity.IsAuthenticated;
            string password = login.Password;
            login.Password = password;

            if (isAuth)
            {
                ModelState.AddModelError("", "Cuenta de usuario ya se encuentra conectada al sistema");
                return View();
            }
            else
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (login.Username != null)
                        {
                            if (login.Password != null)
                            {
                                // Hacer una solicitud a la API para autenticar al usuario
                                var response = await _httpClient.PostAsJsonAsync("Login/Acceso", login);

                                if (!response.IsSuccessStatusCode)
                                {
                                    ModelState.AddModelError("", "Usuario no registrado o contraseña incorrecta");
                                    return View();
                                }

                                var userLogueado = await response.Content.ReadFromJsonAsync<Login>();

                                if (userLogueado != null)
                                {
                                    var isValid = (login.Username == userLogueado.Username && login.Password == userLogueado.Password);

                                    if (!isValid)
                                    {
                                        ModelState.AddModelError("", "Documento o contraseña invalido");
                                        return View();
                                    }
                                    else
                                    {
                                        var responseUsuarios = await _httpClient.GetAsync($"Usuarios/Obtener/{userLogueado.UsuarioId}");
                                        var usuarios = await responseUsuarios.Content.ReadFromJsonAsync<ViewUsuario>();

                                        if (usuarios != null)
                                        {
                                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

                                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ($"{userLogueado.UsuarioId}")));
                                            identity.AddClaim(new Claim(ClaimTypes.Name, ($"{userLogueado.UsuarioId}")));
                                            identity.AddClaim(new Claim("Nombre", ($"{usuarios?.PrimerNombre} {usuarios?.PrimerApellido}")));
                                            identity.AddClaim(new Claim("UsuarioSucursalId", ($"{usuarios?.SucursalId}")));
                                            identity.AddClaim(new Claim("UsuarioTallerId", ($"{usuarios?.TallerId}")));

                                            var userRoles = JsonConvert.DeserializeObject<UsuariosPermisos>(userLogueado.Permisos.ToString());

                                            identity.AddClaim(new Claim(ClaimTypes.Role, ($"{userRoles?.TipoUsuario}")));

                                            if (userRoles?.TipoUsuario != null)
                                            {
                                                identity.AddClaim(new Claim("TipoUsuarioAcceso", ($"{userRoles.TipoUsuario}")));

                                                if (userRoles.TipoRol != null)
                                                {
                                                    identity.AddClaim(new Claim(ClaimTypes.Role, (userRoles.TipoRol)));
                                                    identity.AddClaim(new Claim("TipoRolAcceso", ($"{userRoles.TipoRol}")));
                                                }
                                            }
                                            else
                                            {
                                                identity.AddClaim(new Claim(ClaimTypes.Role, ("Usuarios")));
                                            }

                                            var principal = new ClaimsPrincipal(identity);
                                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

                                            return RedirectToAction("Index", "Interfaz");
                                        }
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Usuario no registrado");
                                    return View();
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Debes ingresar una contraseña...");
                                return View();
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Debes ingresar un usuario...");
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "hace falta ingresar algún dato de acceso");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fallo la login {0}", ex.Message);
                }
            }
            return View();
        }

    }
}
