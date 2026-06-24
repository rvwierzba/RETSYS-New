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

var stringConexao = Environment.GetEnvironmentVariable("RETSYS_CONNECTION_STRING") 
                    ?? Environment.GetEnvironmentVariable("DATABASE_URL") 
                    ?? builder.Configuration.GetConnectionString("ConexaoPadrao");

if (!string.IsNullOrEmpty(stringConexao))
{
    // 🧹 Limpeza pesada contra aspas, espaços ou lixo de cópia
    stringConexao = stringConexao.Trim().Trim('"').Trim('\'');

    // 🔄 CONVERSOR ADAPTATIVO: Transforma 'postgres://' ou 'postgresql://' no formato ADO.NET
    if (stringConexao.StartsWith("postgres://") || stringConexao.StartsWith("postgresql://"))
    {
        string urlParaParse = stringConexao.StartsWith("postgresql://") 
            ? stringConexao.Replace("postgresql://", "postgres://") 
            : stringConexao;

        var databaseUri = new Uri(urlParaParse);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        var usuario = Uri.UnescapeDataString(userInfo[0]);
        var senha = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        
        int portaBanco = databaseUri.Port == -1 ? 5432 : databaseUri.Port;
        var nomeBanco = databaseUri.AbsolutePath.TrimStart('/');

        stringConexao = $"Host={databaseUri.Host};" +
                        $"Port={portaBanco};" +
                        $"Database={nomeBanco};" +
                        $"Username={usuario};" +
                        $"Password={senha};" +
                        $"SSL Mode=Require;" +
                        $"Trust Server Certificate=True;";
    }
}

// 👁️ LOG DE SEGURANÇA NO PAINEL DO RENDER
Console.WriteLine($"=== FINAL DATABASE CONFIG: '{stringConexao?.Split(';')[0]}' ===");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(stringConexao, b => b.MigrationsAssembly("RETSYS.Infrastructure")));

// ==========================================
// 2. INJEÇÃO DE DEPENDÊNCIAS E SERVIÇOS
// ==========================================

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IServicoPix, RETSYS.Infrastructure.Services.ServicoPix>();
builder.Services.AddSingleton<IServicoCriptografia, ServicoCriptografia>();
builder.Services.AddControllersWithViews();
builder.Services.AddInertia();
builder.Services.AddViteHelper(); 

// Configuração de Sessão em memória para persistência de tokens temporários de streaming
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;    
    options.Cookie.IsEssential = true; 
});

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

app.UseSession(); 

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var usuario = context.User;
    bool estaAutenticado = usuario?.Identity?.IsAuthenticated == true;

    Inertia.Share("auth", new {
        usuarioNome = estaAutenticado ? (usuario?.Identity?.Name ?? "Colaborador") : "Colaborador",
        usuarioPerfil = estaAutenticado ? (usuario?.FindFirst(ClaimTypes.Role)?.Value ?? "Vendedor") : "Vendedor",
        usuarioFoto = estaAutenticado ? (usuario?.FindFirst("FotoUrl")?.Value ?? usuario?.FindFirst(ClaimTypes.UserData)?.Value) : null,
        spotifyTokenAtivo = context.Session.GetString("SpotifyToken") != null
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
    
    // 🔥 MECÂNICA DE AUTO-SINCRONIZAÇÃO PREVENTIVA (EnsureCreated -> Migrations)
    // Esse bloco corrige o banco do Render sem que você precise apagar nada na mão.
    try
    {
        using (var comando = contexto.Database.GetDbConnection().CreateCommand())
        {
            await contexto.Database.OpenConnectionAsync();
            
            // 1. Força a criação da tabela de controle do EF Core caso ela não exista
            comando.CommandText = """
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL,
                    "ProductVersion" character varying(32) NOT NULL,
                    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                );
            """;
            await comando.ExecuteNonQueryAsync();

            // 2. Descobre se a tabela 'clientes' já existe do histórico antigo do banco
            comando.CommandText = """
                SELECT COUNT(*) FROM pg_class c 
                JOIN pg_namespace n ON n.oid = c.relnamespace 
                WHERE c.relname = 'clientes' AND n.nspname = 'public';
            """;
            var bancoJaPossuiEstrutura = Convert.ToInt32(await comando.ExecuteScalarAsync()) > 0;

            if (bancoJaPossuiEstrutura)
            {
                // 3. Insere o ID da migração conflitante no histórico para o EF entender que ela já foi feita
                comando.CommandText = """
                    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES ('20260624145606_AddFotoUrlNoUsuario', '10.0')
                    ON CONFLICT DO NOTHING;
                """;
                await comando.ExecuteNonQueryAsync();

                // 4. Como pulamos a execução da migração, injetamos manualmente a nova coluna de fotos
                comando.CommandText = """
                    ALTER TABLE "usuarios" ADD COLUMN IF NOT EXISTS "FotoUrl" text;
                """;
                await comando.ExecuteNonQueryAsync();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Auto-Sync Banco Aviso]: {ex.Message}");
    }

    // Agora o MigrateAsync vai rodar perfeitamente liso e sem tentar duplicar tabelas!
    await contexto.Database.MigrateAsync();
    
    var seeder = new DatabaseSeeder(contexto, criptografia);
    await seeder.SemearDadosAsync();
}    

app.Run();