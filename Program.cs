using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using EMISSOR_DE_CERTIFICADOS.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Alterado para API

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EMISSOR DE CERTIFICADOS API",
        Version = "v1",
        Description = "API para gerenciamento de certificados",
    });
});

// Registrando injeções de dependências no container 
var connectionStrings = new Dictionary<string, string>
{
    { "CertificadoConnection", builder.Configuration.GetConnectionString("CertificadoConnection") },
};
builder.Services.AddSingleton<IDBHelpers>(provider => new DBHelpers(connectionStrings));

builder.Services.AddScoped<IPessoaEventosRepository, PessoaEventosRepository>();
builder.Services.AddScoped<IEventoPessoasRepository, EventoPessoasRepository>();

builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();

builder.Services.AddScoped<IOrganizadorService, OrganizadorService>();
builder.Services.AddScoped<IOrganizadorRepository, OrganizadorRepository>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuariosService>();

builder.Services.AddScoped<ICertificadosService, CertificadosService>();
builder.Services.AddScoped<ICertificadosRepository, CertificadosRepository>();

builder.Services.AddScoped<IParticipanteRepository, ParticipanteRepository>();
builder.Services.AddScoped<IParticipanteService, ParticipanteService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddScoped<IEmailConfigRepository, EmailConfigRepository>();
builder.Services.AddScoped<IEmailConfigService, EmailConfigService>();

builder.Services.AddScoped<IValidarCertificadoRepository, ValidarCertificadoRepository>();
builder.Services.AddScoped<IValidarCertificadoService, ValidarCertificadoService>();

builder.Logging.AddConsole();
// Configura o nível mínimo de logging para Debug
builder.Logging.SetMinimumLevel(LogLevel.Debug);
// Add IHttpContextAccessor to the services
builder.Services.AddHttpContextAccessor();

//===========================================================================================
//Injetar dependencia do "HttpContext" e "Sessao" para usar o controle de sessão da aplicacao
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ISessao, Sessao>();
// Configurar serviços de sessão
builder.Services.AddDistributedMemoryCache(); // Usar cache em memória para armazenar sessões
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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EMISSOR DE CERTIFICADOS API v1");
        c.RoutePrefix = string.Empty; // Deixa o Swagger na raiz do aplicativo
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//Configurar o uso da Sessao
app.UseSession();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.Run();
