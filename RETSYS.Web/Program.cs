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

// Captura a string de conexão das variáveis de ambiente ou do arquivo local
var stringConexao = Environment.GetEnvironmentVariable("RETSYS_CONNECTION_STRING") 
                    ?? Environment.GetEnvironmentVariable("DATABASE_URL") 
                    ?? builder.Configuration.GetConnectionString("ConexaoPadrao");

// Faxina defensiva: Remove espaços em branco ou aspas das pontas
if (!string.IsNullOrEmpty(stringConexao))
{
    stringConexao = stringConexao.Trim().Trim('"').Trim('\'');
}

// CONVERSOR INTELIGENTE: Suporta tanto 'postgres://' quanto 'postgresql://'
if (!string.IsNullOrEmpty(stringConexao) && (stringConexao.StartsWith("postgres://") || stringConexao.StartsWith("postgresql://")))
{
    // Normaliza o prefixo para "http://" temporariamente apenas para o parser do .NET extrair os dados sem falhar pelo formato da URL
    string urlTratada = stringConexao.StartsWith("postgresql://") 
        ? stringConexao.Replace("postgresql://", "http://") 
        : stringConexao.Replace("postgres://", "http://");

    var databaseUri = new Uri(urlTratada);
    var userInfo = databaseUri.UserInfo.Split(':');
    
    stringConexao = $"Host={databaseUri.Host};" +
                    $"Port={databaseUri.Port};" +
                    $"Database={databaseUri.AbsolutePath.TrimStart('/')};" +
                    $"Username={userInfo[0]};" +
                    $"Password={userInfo[1]};" +
                    $"SSL Mode=Require;" +
                    $"Trust Server Certificate=True;";
}

// RASTREADOR DE SEGURANÇA (Para auditar o resultado convertido no log)
if (!string.IsNullOrEmpty(stringConexao))
{
    string amostra = stringConexao.Length > 15 ? stringConexao.Substring(0, 15) : stringConexao;
    Console.WriteLine($"=== CONVERTED CONNECTION STRING: '{amostra}' ===");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(stringConexao, b => b.MigrationsAssembly("RETSYS.Infrastructure")));

// ==========================================
// 2. INJEÇÃO DE DEPENDÊNCIAS E SERVIÇOS
// ==========================================

builder.Services.AddHttpClient<IServicoPix, RETSYS.Infrastructure.Services.ServicoPix>();
builder.Services.AddSingleton<IServicoCriptografia, ServicoCriptografia>();
builder.Services.AddControllersWithViews();
builder.Services.AddInertia();
builder.Services.AddViteHelper(); 

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// ==========================================
// 3. PIPELINE DE EXECUÇÃO (MIDDLEWARES)
// ==========================================

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var usuario = context.User;
    Inertia.Share("auth", new {
        usuarioNome = usuario.Identity?.IsAuthenticated == true ? usuario.Identity.Name : null,
        usuarioPerfil = usuario.FindFirst(ClaimTypes.Role)?.Value
    });
    await next();
});

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
    
    await contexto.Database.EnsureCreatedAsync();
    
    var seeder = new DatabaseSeeder(contexto, criptografia);
    await seeder.SemearDadosAsync();
}    

app.Run();