using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrando o PessoaEventosRepository no container de injeção de dependências
builder.Services.AddScoped<PessoaEventosRepository>();

builder.Logging.AddConsole();
// Configura o nível mínimo de logging para Debug
builder.Logging.SetMinimumLevel(LogLevel.Debug);

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
    o.IdleTimeout = TimeSpan.FromMinutes(60);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
//===========================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//Configurar o uso da Sessao
app.UseSession();
// Adicionar o middleware de verificação de sessão
app.UseMiddleware<SessaoTimeoutHelper>();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.Run();