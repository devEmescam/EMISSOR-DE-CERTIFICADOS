using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using EMISSOR_DE_CERTIFICADOS.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
ConfigureServices(builder.Services, builder.Configuration);

// Configuração de logging
ConfigureLogging(builder.Logging);

var app = builder.Build();

// Configuração do pipeline de requisição HTTP
ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Adicionar controladores
    services.AddControllers();

    // Configurar Swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "EMISSOR DE CERTIFICADOS API",
            Version = "v1",
            Description = "API para gerenciamento de certificados",
        });
    });

    // Configurar conexões de banco de dados
    var connectionStrings = new Dictionary<string, string>
    {
        { "CertificadoConnection", configuration.GetConnectionString("CertificadoConnection") },
    };
    services.AddSingleton<IDBHelpers>(provider => new DBHelpers(connectionStrings));

    // Registrar dependências
    RegisterRepositories(services);
    RegisterServices(services);

    // Configurar sessão
    services.AddHttpContextAccessor();
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddScoped<ISessao, Sessao>();
    services.AddDistributedMemoryCache();
    services.AddSession(o =>
    {
        o.IdleTimeout = TimeSpan.FromMinutes(60);
        o.Cookie.HttpOnly = true;
        o.Cookie.IsEssential = true;
    });
}

void RegisterRepositories(IServiceCollection services)
{
    services.AddScoped<IPessoaEventosRepository, PessoaEventosRepository>();
    services.AddScoped<IEventoPessoasRepository, EventoPessoasRepository>();
    services.AddScoped<IPessoaRepository, PessoaRepository>();
    services.AddScoped<IOrganizadorRepository, OrganizadorRepository>();
    services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    services.AddScoped<ICertificadosRepository, CertificadosRepository>();
    services.AddScoped<IParticipanteRepository, ParticipanteRepository>();
    services.AddScoped<IEmailRepository, EmailRepository>();
    services.AddScoped<IEmailConfigRepository, EmailConfigRepository>();
    services.AddScoped<IValidarCertificadoRepository, ValidarCertificadoRepository>();
}

void RegisterServices(IServiceCollection services)
{
    services.AddScoped<IPessoaService, PessoaService>();
    services.AddScoped<IOrganizadorService, OrganizadorService>();
    services.AddScoped<IUsuarioService, UsuariosService>();
    services.AddScoped<ICertificadosService, CertificadosService>();
    services.AddScoped<IParticipanteService, ParticipanteService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IEmailConfigService, EmailConfigService>();
    services.AddScoped<IValidarCertificadoService, ValidarCertificadoService>();
}

void ConfigureLogging(ILoggingBuilder logging)
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EMISSOR DE CERTIFICADOS API v1");
            c.RoutePrefix = string.Empty;
        });
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSession();
    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();
}
