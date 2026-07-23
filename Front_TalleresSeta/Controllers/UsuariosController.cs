using Compartida.Compartido;
using Front_TalleresSeta.Modelos;
using Front_TalleresSeta.Modelos.ModelosView;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Front_TalleresSeta.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFuncionRepositorio _funcionRepo;
        private readonly IInventarioStockRepositorio _inventarioStockRepo;
        private readonly IInventarioLoteRepositorio _inventarioLoteRepo;
        private readonly ILoginRepositorio _loginRepo;
        public static string tabla = "Usuarios";
        public static string mensaje = string.Empty;
        public static long idItem = 0;
        public static string accion = string.Empty;
        public static string mensajeError = string.Empty;
        public static SelectList talleres = null!;

        public UsuariosController(IHttpClientFactory httpClientFactory, IFuncionRepositorio funcionRepo, IInventarioStockRepositorio inventarioStockRepo, IInventarioLoteRepositorio inventarioLoteRepo, ILoginRepositorio loginRepo)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _funcionRepo = funcionRepo;
            _inventarioStockRepo = inventarioStockRepo;
            _inventarioLoteRepo = inventarioLoteRepo;
            _loginRepo = loginRepo;
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
                        return View(await _httpClient.GetFromJsonAsync<List<Usuario>>($"Usuarios/todos?search={search}"));
                    else if (logueado.TipoUser == "Admin" || (logueado.TipoUser == "Empleado" && logueado.TipoRol == "Jefe"))
                        return View(await _httpClient.GetFromJsonAsync<List<Usuario>>($"Usuarios/todos?search={search}&tallerId={logueado.IdTaller}"));
                    else
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "Acceso denegado al listado de usuarios" });
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el o los usuarios.";
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

                        var detalle = await _httpClient.GetFromJsonAsync<Usuario>($"Usuarios/obtener/{id}");

                        SexoList sexoSeleccionado = SexoList.FiltrarporId(detalle.Sexo);

                        ViewBag.Sexo = sexoSeleccionado;
                        ViewBag.FechaNacimiento = Convert.ToDateTime(detalle?.FechaNacimiento).ToString("yyyy-MM-dd");
                        ViewBag.EstadoId = detalle?.Estados?.NombreEstado;
                        ViewBag.TipoUsuarioId = detalle?.TipoUsuarios?.TipoU;
                        ViewBag.TipoDocumentoId = detalle?.TipoDocumentos?.TipoD;
                        ViewBag.RolUsuarioId = detalle?.RolUsuarios?.NombreRol;
                        ViewBag.SucursalId = detalle?.Sucursales?.Nombre;

                        return View(detalle);
                    }
                    else
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = $"Acceso denegado al detalle del usuario, el id: {id} no es válido o no existe en base de datos." });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede mostrar el detalle del usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        public async Task<IActionResult> Create()
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
                    var listaDeSexos = SexoList.List();
                    ViewData["Sexo"] = new SelectList(listaDeSexos, "Id", "Nombre");

                    var estados = await _httpClient.GetFromJsonAsync<List<Estado>>($"Estados/todosHabilitados");
                    ViewData["EstadoId"] = new SelectList(estados, "EstadoId", "NombreEstado");

                    var tipoUsuarios = await _httpClient.GetFromJsonAsync<List<TipoUsuario>>($"TipoUsuarios/todosHabilitados");
                    ViewData["TipoUsuarioId"] = new SelectList(tipoUsuarios, "TipoUsuarioId", "TipoU");

                    var tipoDocumentos = await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/todosHabilitados");
                    ViewData["TipoDocumentoId"] = new SelectList(tipoDocumentos, "TipoDocumentoId", "NombreDocumento");

                    var rolUsuario = await _httpClient.GetFromJsonAsync<List<RolUsuario>>($"RolUsuarios/todosHabilitados");
                    ViewData["RolUsuarioId"] = new SelectList(rolUsuario, "RolUsuarioId", "NombreRol");

                    var sucursales = await _httpClient.GetFromJsonAsync<List<Sucursal>>($"Sucursales/todosHabilitados");
                    ViewData["SucursalId"] = new SelectList(sucursales, "SucursalId", "Nombre");

                    //ViewData["TallerId"] = talleres;
                    ViewData["mensaje"] = "Registra un usuario.";
                    return View();
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear un usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsuarioId,Habilitado,FechaRegistro,Documento,PrimerNombre,SegundoNombre,PrimerApellido,SegundoApellido,FechaNacimiento,Sexo,TelefonoFijo,TelefonoMovil,DireccionPrincipal,DireccionAlterna,Correo,EstadoId,TipoUsuarioId,TipoDocumentoId,RolUsuarioId,SucursalId")] Usuario model)
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

                    var listaDeSexos = SexoList.List();
                    ViewData["Sexo"] = new SelectList(listaDeSexos, "Id", "Nombre", model.Sexo);

                    var estados = await _httpClient.GetFromJsonAsync<List<Estado>>($"Estados/todosHabilitados");
                    ViewData["EstadoId"] = new SelectList(estados, "EstadoId", "NombreEstado", model.EstadoId);

                    var tipoUsuarios = await _httpClient.GetFromJsonAsync<List<TipoUsuario>>($"TipoUsuarios/todosHabilitados");
                    ViewData["TipoUsuarioId"] = new SelectList(tipoUsuarios, "TipoUsuarioId", "TipoU", model.TipoUsuarioId);

                    var tipoDocumentos = await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/todosHabilitados");
                    ViewData["TipoDocumentoId"] = new SelectList(tipoDocumentos, "TipoDocumentoId", "NombreDocumento", model.TipoDocumentoId);

                    var rolUsuario = await _httpClient.GetFromJsonAsync<List<RolUsuario>>($"RolUsuarios/todosHabilitados");
                    ViewData["RolUsuarioId"] = new SelectList(rolUsuario, "RolUsuarioId", "NombreRol", model.RolUsuarioId);

                    var sucursales = await _httpClient.GetFromJsonAsync<List<Sucursal>>($"Sucursales/todosHabilitados");
                    ViewData["SucursalId"] = new SelectList(sucursales, "SucursalId", "Nombre", model.SucursalId);

                    //ViewData["TallerId"] = talleres;

                    if (ModelState.IsValid)
                    {
                        idItem = 0;
                        model.Habilitado = true;
                        model.FechaRegistro = DateTime.Now;
                        if (string.IsNullOrEmpty(model.FechaNacimiento.ToString()))
                        {
                            model.FechaNacimiento = new DateTime(1900, 1, 1);
                        }


                        bool existe = await _httpClient.GetFromJsonAsync<bool>($"Usuarios/existe/{model.Documento}");
                        if (existe == true)
                        {
                            idItem = model.Documento;
                            accion = "YA_EXISTE";
                            mensaje = "Usuario ya existe en el sistema.";
                            //Si existe redireccionar al detalle
                            return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                        }
                        else
                        {
                            try
                            {
                                //Registrar el usuario
                                var responseCreate = await _httpClient.PostAsJsonAsync("Usuarios/crear", model);
                                // Verificar si la API devolvió una respuesta exitosa
                                if (!responseCreate.IsSuccessStatusCode)
                                {
                                    mensaje = $"Error al crear el usuario: {responseCreate.StatusCode}";
                                    return View(model);
                                }
                                var jsonResponse = await responseCreate.Content.ReadAsStringAsync();
                                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                                if (!responseObject.TryGetProperty("id", out JsonElement idElement))
                                {
                                    Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponse);
                                    mensaje = "Error: No se recibió el ID del usuario.";
                                    return View(model);
                                }
                                //idItem = responseObject.GetProperty("id");
                                if (!idElement.TryGetInt64(out idItem))
                                {
                                    Console.WriteLine($"El ID no es un número válido de 64 bits. JSON: {jsonResponse}");
                                    mensaje = "Error: El ID recibido no tiene un formato numérico válido.";
                                    return View(model);
                                }
                            }
                            catch (Exception)
                            {
                                mensaje = $"No se puede crear el usuario.";
                                ViewData["mensaje"] = mensaje;
                                ViewData["accion"] = accion;
                                return View(model);
                            }
                        }

                        long idLoguin = 0;
                        //validar todos los registros estén ok
                        if (idItem > 0)
                        {
                            string username = Convert.ToString(model.Documento);
                            string password = Convert.ToString(model.Documento);

                            var sucursalesLogueo = await _httpClient.GetFromJsonAsync<Sucursal>($"Sucursales/obtener/{model.SucursalId}");

                            //Crear login
                            var logueo = new Login
                            {
                                Habilitado = true,
                                FechaRegistro = model.FechaRegistro,
                                Username = username,
                                Password = password,
                                Permisos = "",
                                RememberMe = false,
                                TallerId = sucursalesLogueo.TallerId,
                                UsuarioId = model.UsuarioId
                            };

                            //Crear y asociar login
                            if (await _loginRepo.ExisteModeloAsync(logueo.Username))
                            {
                                idLoguin = await _loginRepo.ActualizarAsync(logueo);
                            }
                            else
                            {
                                idLoguin = await _loginRepo.CrearAsync(logueo);
                            }

                            if (idLoguin > 0)
                            {
                                mensaje = "Se registro el usuario y el acceso del login correctamente...";
                                accion = "CREADO";
                                return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                            }
                            else
                            {
                                accion = "CREADO_CON_ERROR";
                                mensaje = $"El usuario se creo aunque no se logró completar el registro del login.";
                            }
                        }
                        else
                        {
                            accion = "FALLO";
                            mensaje = $"El usuario no se logró crear.";
                        }
                    }
                    else
                    {
                        accion = "DATOS_ERROR";
                        mensaje = "Revisa los datos a registrar.";
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede crear el usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
                ViewData["mensaje"] = mensaje;
                ViewData["accion"] = accion;
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(long id)
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
                    if (id < 0)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se puede modificar el usuario: id no válido." });
                    }

                    var model = await _httpClient.GetFromJsonAsync<Usuario>($"Usuarios/obtener/{id}");
                    if (model == null)
                    {
                        return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje = "No se encontró el usuario a modificar." });
                    }

                    var estados = await _httpClient.GetFromJsonAsync<List<Estado>>($"Estados/todosHabilitados");
                    ViewData["EstadoId"] = new SelectList(estados, "EstadoId", "NombreEstado", model.Estados?.EstadoId);

                    var tipoUsuarios = await _httpClient.GetFromJsonAsync<List<TipoUsuario>>($"TipoUsuarios/todosHabilitados");
                    ViewData["TipoUsuarioId"] = new SelectList(tipoUsuarios, "TipoUsuarioId", "TipoU", model.TipoUsuarios?.TipoUsuarioId);

                    var tipoDocumentos = await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/todosHabilitados");
                    ViewData["TipoDocumentoId"] = new SelectList(tipoDocumentos, "TipoDocumentoId", "TipoD", model.TipoDocumentos?.TipoDocumentoId);

                    var rolUsuario = await _httpClient.GetFromJsonAsync<List<RolUsuario>>($"RolUsuarios/todosHabilitados");
                    ViewData["RolUsuarioId"] = new SelectList(rolUsuario, "RolUsuarioId", "NombreRol", model.RolUsuarios?.RolUsuarioId);

                    var sucursales = await _httpClient.GetFromJsonAsync<List<Sucursal>>($"Sucursales/todosHabilitados");
                    ViewData["SucursalId"] = new SelectList(sucursales, "SucursalId", "Nombre", model.Sucursales?.SucursalId);

                    ViewBag.FechaNacimiento = Convert.ToDateTime(model?.FechaNacimiento).ToString("yyyy-MM-dd");

                    ViewData["TallerId"] = talleres;
                    ViewData["accion"] = "ACTUALIZAR";
                    ViewData["mensaje"] = "Ingresa los datos a modificar.";
                    return View(model);
                }
                catch (Exception ex)
                {
                    mensaje = $"No se puede modificar el usuario.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("UsuarioId,Habilitado,FechaRegistro,Documento,PrimerNombre,SegundoNombre,PrimerApellido,SegundoApellido,FechaNacimiento,Sexo,TelefonoFijo,TelefonoMovil,DireccionPrincipal,DireccionAlterna,Correo,EstadoId,TipoUsuarioId,TipoDocumentoId,RolUsuarioId,SucursalId")] Usuario model)
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
                        // Actualizar el usuario
                        var responseEdit = await _httpClient.PutAsJsonAsync($"Usuarios/modificar", model);
                        // Verificar si la API devolvió una respuesta exitosa
                        if (!responseEdit.IsSuccessStatusCode)
                        {
                            mensaje = $"Error al actualizar el usuario: {responseEdit.StatusCode}";
                            return View(model);
                        }
                        var jsonResponseEdit = await responseEdit.Content.ReadAsStringAsync();
                        var responseObjectEdit = JsonSerializer.Deserialize<JsonElement>(jsonResponseEdit);
                        if (!responseObjectEdit.TryGetProperty("id", out JsonElement idElement))
                        {
                            Console.WriteLine("La respuesta de la API no contiene 'id'. JSON: " + jsonResponseEdit);
                            mensaje = "Error: No se recibió el ID del usuario al actualizar.";
                            return View(model);
                        }
                        falloActualizar = false;
                        idItem = Convert.ToInt64(responseObjectEdit.GetProperty("id"));
                        return RedirectToAction("Details", new { id = idItem, mensaje, accion });
                    }
                    catch (Exception ex)
                    {
                        mensajeError = $"Error ocurrido: {ex.Message}";
                    }
                }

                if (falloActualizar)
                {
                    var estados = await _httpClient.GetFromJsonAsync<List<Estado>>($"Estados/todosHabilitados");
                    ViewData["EstadoId"] = new SelectList(estados, "EstadoId", "NombreEstado", model.EstadoId);

                    var tipoUsuarios = await _httpClient.GetFromJsonAsync<List<TipoUsuario>>($"TipoUsuarios/todos");
                    ViewData["TipoUsuarioId"] = new SelectList(tipoUsuarios, "TipoUsuarioId", "TipoD", model.TipoUsuarioId);

                    var tipoDocumentos = await _httpClient.GetFromJsonAsync<List<TipoDocumento>>($"TipoDocumentos/todos");
                    ViewData["TipoDocumentoId"] = new SelectList(tipoDocumentos, "TipoDocumentoId", "NombreDocumento", model.TipoDocumentoId);

                    var rolUsuario = await _httpClient.GetFromJsonAsync<List<RolUsuario>>($"RolUsuarios/todos");
                    ViewData["RolUsuarioId"] = new SelectList(rolUsuario, "RolUsuarioId", "NombreRol", model.RolUsuarioId);

                    var sucursales = await _httpClient.GetFromJsonAsync<List<Sucursal>>($"Sucursales/todos");
                    ViewData["SucursalId"] = new SelectList(sucursales, "SucursalId", "Nombre", model.SucursalId);

                    ViewData["accion"] = "ERROR_DATOS";
                    ViewData["mensaje"] = "Algo salió mal con los datos...";
                }
                return View(model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
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
                    bool eliminado = await _httpClient.DeleteFromJsonAsync<bool>($"Usuarios/eliminar/{id}");
                    if (eliminado)
                    {
                        accion = "ELIMINADO";
                        mensaje = "El usuario fue eliminado.";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Index", new { mensaje, accion });
                    }
                    else
                    {
                        mensaje = "No se logró eliminar el usuario. Cuenta con registros asociados";
                        ViewData["mensaje"] = mensaje;
                        return RedirectToAction("Details", new { id, mensaje, accion });
                    }
                }
                catch (Exception ex)
                {
                    mensaje = $"El proceso para eliminar el usuario falló.";
                    mensajeError = $"Error ocurrido: {ex.Message}";
                    return RedirectToAction("AccesoDenegado", "Interfaz", new { mensaje, mensajeError });
                }
            }
        }

    }
}
