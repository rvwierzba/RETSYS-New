using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using InertiaCore;
using InertiaCore.Extensions;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Interfaces;
using RETSYS.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURAÇÃO DO BANCO DE DADOS (POSTGRESQL)
// ==========================================

// Captura a string de conexão priorizando as variáveis de ambiente da nuvem
var stringConexao = Environment.GetEnvironmentVariable("RETSYS_CONNECTION_STRING") 
                    ?? Environment.GetEnvironmentVariable("DATABASE_URL") 
                    ?? builder.Configuration.GetConnectionString("ConexaoPadrao");

// CONVERSOR INTELIGENTE: Se a string vier no formato de URL (postgres://), reconstrói pro formato .NET
if (!string.IsNullOrEmpty(stringConexao) && stringConexao.StartsWith("postgres://"))
{
    var databaseUri = new Uri(stringConexao);
    var userInfo = databaseUri.UserInfo.Split(':');
    
    stringConexao = $"Host={databaseUri.Host};" +
                    $"Port={databaseUri.Port};" +
                    $"Database={databaseUri.AbsolutePath.TrimStart('/')};" +
                    $"Username={userInfo[0]};" +
                    $"Password={userInfo[1]};" +
                    $"SSL Mode=Require;" +
                    $"Trust Server Certificate=True;";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(stringConexao, b => b.MigrationsAssembly("RETSYS.Infrastructure")));

// ==========================================
// 2. INJEÇÃO DE DEPENDÊNCIAS E SERVIÇOS
// ==========================================

// Ativa o cliente HTTP acoplado à nossa implementação da OpenPix
builder.Services.AddHttpClient<IServicoPix, RETSYS.Infrastructure.Services.ServicoPix>();

// Injetar o Serviço de Criptografia de Senhas (BCrypt)
builder.Services.AddSingleton<IServicoCriptografia, ServicoCriptografia>();

// Inicialização oficial do MVC + AspNetCore.InertiaCore
builder.Services.AddControllersWithViews();
builder.Services.AddInertia();
builder.Services.AddViteHelper(); 

// Ativa o sistema de autenticação por cookies nativo do ASP.NET Core
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => {
        options.LoginPath = "/login"; // Redireciona aqui se tentar acessar algo restrito
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Sessão expira após 8h de trabalho
    });

// ==========================================
// 3. PIPELINE DE EXECUÇÃO (MIDDLEWARES)
// ==========================================

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Exibe o erro real no MVP para facilitar o seu debug na nuvem
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Middleware para compartilhar os dados de sessão com o Vue 3 antes do Inertia renderizar
app.Use(async (context, next) =>
{
    var usuario = context.User;
    
    // Injeta as propriedades compartilhadas no escopo da requisição atual
    Inertia.Share("auth", new {
        usuarioNome = usuario.Identity?.IsAuthenticated == true ? usuario.Identity.Name : null,
        usuarioPerfil = usuario.FindFirst(ClaimTypes.Role)?.Value
    });

    await next();
});

// Ativar o Middleware oficial do Inertia
app.UseInertia();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================================
// 4. CARGA INICIAL DE DADOS (DATABASE SEEDER)
// ==========================================

using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var criptografia = escopo.ServiceProvider.GetRequiredService<IServicoCriptografia>();
    
    // Cria fisicamente todo o esquema de tabelas a partir das Entidades C#
    await contexto.Database.EnsureCreatedAsync();
    
    // Instancia o seeder e executa a carga inicial assíncrona
    var seeder = new DatabaseSeeder(contexto, criptografia);
    await seeder.SemearDadosAsync();
}    

app.Run();