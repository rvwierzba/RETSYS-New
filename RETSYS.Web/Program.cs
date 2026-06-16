using Microsoft.EntityFrameworkCore;
using InertiaCore.Extensions;
using RETSYS.Infrastructure.Data;
using RETSYS.Domain.Interfaces;
using RETSYS.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar a conexão do EF Core com o PostgreSQL 16
var stringConexao = builder.Configuration.GetConnectionString("ConexaoPadrao");
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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 4. Ativar o Middleware oficial do Inertia
app.UseInertia();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Bloco de escopo isolado para rodar a carga inicial de dados com segurança
using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var criptografia = escopo.ServiceProvider.GetRequiredService<IServicoCriptografia>();
    
    // Instancia o seeder e executa a carga inicial assíncrona
    var seeder = new DatabaseSeeder(contexto, criptografia);
    await seeder.SemearDadosAsync();
}    

app.Run();