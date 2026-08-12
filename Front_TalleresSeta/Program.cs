using Front_TalleresSeta.Repositorios;
using Front_TalleresSeta.Repositorios.IRepositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// se configuran los cookies del inicio de sesión
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});


/*
      h ttps://apiinventario.runasp.net/swagger/index.html
      h ttps://localhost:7196/api/
    */
//Se agrega la conexion al API
builder.Services.AddHttpClient("ApiClient", client =>
{
    //client.BaseAddress = new Uri("https://adminwebapi.talleresseta.com.co/api/");
    // client.BaseAddress = new Uri("https://apiadminweb.talleresseta.com/api/");
    // cambia esto por la URL de tu API. https://apiadminweb.talleresseta.com/swagger/
    // client.BaseAddress = new Uri("https://apiinventario.runasp.net/api/"); // Cambia esto por la URL de tu API.
    client.BaseAddress = new Uri("https://localhost:7196/api/"); // Cambia esto por la URL de tu API.
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromMinutes(5); // Aumenta el tiempo de espera a 5 minutos para consumir el API
});

// Agrega esta línea para los controladores.
builder.Services.AddControllers();

//// Se agrega la conexi�n de HTTP, sesión
builder.Services.AddHttpContextAccessor();

// Agregar servicios
builder.Services.AddScoped<IFuncionRepositorio, FuncionRepositorio>();
builder.Services.AddScoped<IInventarioStockRepositorio, InventarioStockRepositorio>();
builder.Services.AddScoped<IInventarioLoteRepositorio, InventarioLoteRepositorio>();
builder.Services.AddScoped<ILoginRepositorio, LoginRepositorio>();

// configurar la sesión - tiempo activo y direccionamientos
builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(240);
    //La sesión del usuario se mantendrá activa siempre y cuando continúe interactuando con la aplicación el usuario
    options.SlidingExpiration = true; // Habilita la expiración deslizante, renueva las cookies con la interaccion del usuario
    options.AccessDeniedPath = "/Login/Acceso";
    options.LoginPath = "/Login/Acceso";
    options.LogoutPath = "/Login/Logout"; // Corregido el LogoutPath
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// se add para poder usar la autenticación de los cookies
app.UseAuthentication();
app.UseAuthorization();
//app.UseCookiePolicy();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Acceso}/{id?}");

app.Run();



