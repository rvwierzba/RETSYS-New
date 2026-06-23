using Microsoft.EntityFrameworkCore;
using InertiaCore.Extensions;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Interfaces;
using RETSYS.Infrastructure.Security;
using System.Security.Claims;
using InertiaCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar a conexão do EF Core com o PostgreSQL 16 (Prioriza Docker, Fallback para Local)
var stringConexao = Environment.GetEnvironmentVariable("RETSYS_CONNECTION_STRING") 
                    ?? builder.Configuration.GetConnectionString("ConexaoPadrao");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(stringConexao, b => b.MigrationsAssembly("RETSYS.Infrastructure")));

// Ativa o cliente HTTP acoplado à nossa implementação da OpenPix
builder.Services.AddHttpClient<IServicoPix, RETSYS.Infrastructure.Services.ServicoPix>();

// 2. Injetar o Serviço de Criptografia de Senhas (BCrypt)
builder.Services.AddSingleton<IServicoCriptografia, ServicoCriptografia>();

// 3. Inicialização oficial do MVC + AspNetCore.InertiaCore
builder.Services.AddControllersWithViews();
builder.Services.AddInertia();
builder.Services.AddViteHelper(); 

// Ativa o sistema de autenticação por cookies nativo do ASP.NET Core
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => {
        options.LoginPath = "/login"; // Redireciona aqui se tentar acessar algo restrito
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Sessão expira após 8h de trabalho
    });

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

// 🌟Middleware nativo para compartilhar os dados de sessão com o Vue 3 antes do Inertia renderizar
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

// Ativar o Middleware oficial do Inertia (Sem argumentos)
app.UseInertia();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Bloco de escopo isolado para rodar a carga inicial de dados com segurança
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