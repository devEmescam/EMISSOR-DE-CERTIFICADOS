using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add IHttpContextAccessor to the services
builder.Services.AddHttpContextAccessor();

var connectionStrings = new Dictionary<string, string>
{
    { "CertificadoConnection", builder.Configuration.GetConnectionString("CertificadoConnection") },
};

builder.Services.AddSingleton(new DBHelpers(connectionStrings));

//===========================================================================================
//Injetar dependencia do "HttpContext" e "Sessao" para usar o controle de sessão da aplicacao
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ISessao, Sessao>();
//Configurar os Cookies da Sessão
builder.Services.AddSession(o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
//===========================================================================================

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
app.UseAuthorization();

//Configurar o uso da Sessao
app.UseSession();

app.MapControllerRoute(
    name: "home_organizador",
    pattern: "Home_Organizador/{action=Index}/{id?}",
    defaults: new { controller = "Home_Organizador", action = "Index" });

app.MapControllerRoute(
    name: "home_participante",
    pattern: "Home_Participante/{action=Index}/{id?}",
    defaults: new { controller = "Home_Participante", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
