using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.Repositories;
using EMISSOR_DE_CERTIFICADOS.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configurar Kestrel para escutar na rede local (porta 5113)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5113); // Permite acessos via IP local, ex: 192.168.x.x:5113
});

// ✅ Adicionar política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

ConfigureServices(builder.Services, builder.Configuration);
ConfigureLogging(builder.Logging);

var app = builder.Build();
ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers();

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

    var connectionStrings = new Dictionary<string, string>
    {
        { "CertificadoConnection", configuration.GetConnectionString("CertificadoConnection") },
    };
    services.AddSingleton<IDBHelpers>(provider => new DBHelpers(connectionStrings));

    RegisterRepositories(services);
    RegisterServices(services);

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

    // ✅ Aplicar política de CORS
    app.UseCors("AllowAll");

    app.UseAuthorization();
    app.MapControllers();
}
