//using EMISSOR_DE_CERTIFICADOS.DBConnections;
//using EMISSOR_DE_CERTIFICADOS.Helpers;
//using Microsoft.AspNetCore.Http;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Add IHttpContextAccessor to the services
//builder.Services.AddHttpContextAccessor();

//var connectionStrings = new Dictionary<string, string>
//{
//    { "CertificadoConnection", builder.Configuration.GetConnectionString("CertificadoConnection") },
//};

//builder.Services.AddSingleton(new DBHelpers(connectionStrings));

////===========================================================================================
////Injetar dependencia do "HttpContext" e "Sessao" para usar o controle de sessão da aplicacao
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//builder.Services.AddScoped<ISessao, Sessao>();
////Configurar os Cookies da Sessão
//builder.Services.AddSession(o =>
//{
//    o.Cookie.HttpOnly = true;
//    o.Cookie.IsEssential = true;
//});
////===========================================================================================

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthorization();

////Configurar o uso da Sessao
//app.UseSession();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();

using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

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
    o.IdleTimeout = TimeSpan.FromMinutes(5); 
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
app.UseRouting();
app.UseAuthorization();

//Configurar o uso da Sessao
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


