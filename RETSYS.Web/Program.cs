using System.Linq;
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
builder.Services.AddHttpClient<IServicoIa, RETSYS.Infrastructure.Services.ServicoOllama>();
builder.Services.AddSingleton<IServicoCriptografia, ServicoCriptografia>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opcoes =>
    {
        // Campos vazios ("") vindos dos formulários Vue viram null em vez de invalidar o DTO inteiro
        opcoes.JsonSerializerOptions.Converters.Add(new RETSYS.Web.Json.ConversorJsonTolerante());
    });
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
        // Sem esta rota o Forbid() redirecionava para /Account/AccessDenied (inexistente → 404)
        options.AccessDeniedPath = "/dashboard";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// ==========================================
// 3. PIPELINE DE EXECUÇÃO (MIDDLEWARES)
// ==========================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            if (context.Request.Headers.ContainsKey("X-Inertia"))
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                var referer = context.Request.Headers["Referer"].ToString();
                context.Response.Headers["X-Inertia-Location"] = !string.IsNullOrEmpty(referer) ? referer : "/dashboard";
                return;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Ocorreu um erro inesperado. Nossa equipe já foi notificada. Tente novamente em alguns instantes.");
        });
    });
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
    var perfilClaim = estaAutenticado ? (usuario?.FindFirst(ClaimTypes.Role)?.Value ?? "Vendedor") : "Vendedor";
    
    bool ehSistema = string.Equals(perfilClaim, nameof(RETSYS.Domain.Enums.PerfilUsuario.Sistema), StringComparison.OrdinalIgnoreCase) || (usuario?.IsInRole("Sistema") == true);
    bool ehDono = string.Equals(perfilClaim, nameof(RETSYS.Domain.Enums.PerfilUsuario.Dono), StringComparison.OrdinalIgnoreCase) || (usuario?.IsInRole("Dono") == true);
    bool ehAdmin = string.Equals(perfilClaim, nameof(RETSYS.Domain.Enums.PerfilUsuario.Admin), StringComparison.OrdinalIgnoreCase) || (usuario?.IsInRole("Admin") == true);
    bool ehAdminOuGerente = ehSistema || ehDono || ehAdmin || string.Equals(perfilClaim, "Gerente", StringComparison.OrdinalIgnoreCase) || (usuario?.IsInRole("Gerente") == true);

    Guid oticaAtivaId = Guid.Empty;
    string nomeOtica = "Ótica Matriz";
    string? nomeUsuarioAtual = null;
    string? fotoUsuarioAtual = null;
    object? oticasDisponiveis = null;

    if (estaAutenticado)
    {
        var db = context.RequestServices.GetService<ApplicationDbContext>();

        // 1. Obtém o OticaId cadastrado no usuário (loja base), lido do banco como no TenantController,
        //    para que a loja exibida seja a mesma usada nos filtros mesmo após uma transferência.
        Guid oticaUsuarioId = Guid.Empty;
        var userIdStr = usuario?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (db != null && Guid.TryParse(userIdStr, out var uid))
        {
            var userDb = db.Usuarios.AsNoTracking().FirstOrDefault(u => u.Id == uid);
            if (userDb != null)
            {
                oticaUsuarioId = userDb.OticaId;
                // Nome e foto do banco: refletem a edição do perfil sem precisar refazer login
                nomeUsuarioAtual = userDb.Nome;
                fotoUsuarioAtual = userDb.FotoUrl;
            }
        }
        if (oticaUsuarioId == Guid.Empty && Guid.TryParse(usuario?.FindFirst("OticaId")?.Value, out var claimGuid))
        {
            oticaUsuarioId = claimGuid;
        }

        if (ehSistema && db != null)
        {
            // Sistema pode selecionar qualquer ótica via Sessão
            var sessaoOticaId = context.Session.GetString("OticaAtivaId");
            if (!string.IsNullOrEmpty(sessaoOticaId) && Guid.TryParse(sessaoOticaId, out var oticaSessaoGuid) && oticaSessaoGuid != Guid.Empty)
            {
                if (db.Oticas.Any(o => o.Id == oticaSessaoGuid))
                {
                    oticaAtivaId = oticaSessaoGuid;
                }
            }

            if (oticaAtivaId == Guid.Empty)
            {
                oticaAtivaId = oticaUsuarioId != Guid.Empty ? oticaUsuarioId : (db.Oticas.Select(o => o.Id).FirstOrDefault());
            }

            oticasDisponiveis = db.Oticas
                .AsNoTracking()
                .OrderBy(o => o.Nome)
                .Select(o => new { id = o.Id.ToString(), nome = o.Nome })
                .ToList();
        }
        else if (ehDono && db != null)
        {
            // Dono tem acesso à Matriz e todas as Filiais da sua rede
            var matrizId = oticaUsuarioId;
            var oticaUser = db.Oticas.AsNoTracking().FirstOrDefault(o => o.Id == oticaUsuarioId);
            if (oticaUser?.MatrizId != null && oticaUser.MatrizId.Value != Guid.Empty)
            {
                matrizId = oticaUser.MatrizId.Value;
            }

            var lojasRede = db.Oticas
                .AsNoTracking()
                .Where(o => o.Id == matrizId || o.MatrizId == matrizId)
                .OrderBy(o => o.Nome)
                .Select(o => new { id = o.Id.ToString(), nome = o.Nome })
                .ToList();

            var sessaoOticaId = context.Session.GetString("OticaAtivaId");
            if (!string.IsNullOrEmpty(sessaoOticaId) && Guid.TryParse(sessaoOticaId, out var oticaSessaoGuid) && oticaSessaoGuid != Guid.Empty)
            {
                if (lojasRede.Any(l => l.id == oticaSessaoGuid.ToString()))
                {
                    oticaAtivaId = oticaSessaoGuid;
                }
            }

            if (oticaAtivaId == Guid.Empty)
            {
                oticaAtivaId = oticaUsuarioId != Guid.Empty ? oticaUsuarioId : (lojasRede.Select(l => Guid.Parse(l.id)).FirstOrDefault());
            }

            if (lojasRede.Count > 1)
            {
                oticasDisponiveis = lojasRede;
            }
        }
        else
        {
            // Admin e Vendedor: estritamente vinculados à sua loja
            oticaAtivaId = oticaUsuarioId;
        }

        if (oticaAtivaId != Guid.Empty && db != null)
        {
            var otica = db.Oticas.AsNoTracking().FirstOrDefault(o => o.Id == oticaAtivaId);
            if (otica != null)
            {
                nomeOtica = otica.Nome;
            }
        }
    }

    Inertia.Share("auth", new {
        usuarioNome = estaAutenticado ? (nomeUsuarioAtual ?? usuario?.Identity?.Name ?? "Colaborador") : "Colaborador",
        usuarioPerfil = perfilClaim,
        ehSistema = ehSistema,
        ehDono = ehDono,
        ehAdmin = ehAdmin,
        ehAdminOuGerente = ehAdminOuGerente,
        usuarioFoto = estaAutenticado ? (fotoUsuarioAtual ?? usuario?.FindFirst("FotoUrl")?.Value ?? usuario?.FindFirst(ClaimTypes.UserData)?.Value) : null,
        oticaId = oticaAtivaId != Guid.Empty ? oticaAtivaId.ToString() : null,
        oticaNome = nomeOtica,
        oticasDisponiveis = oticasDisponiveis,
        spotifyTokenAtivo = context.Session.GetString("SpotifyToken") != null
    });

    // FLASH DE MENSAGENS: os controllers usam MensagemFlash.Definir("erro"/"sucesso") e em seguida redirecionam.
    // O Inertia.Share vive só na requisição atual, então a mensagem é guardada na sessão no redirect
    // e devolvida como prop na próxima página renderizada.
    var flashErro = context.Session.GetString("Flash:erro");
    var flashSucesso = context.Session.GetString("Flash:sucesso");
    if (flashErro != null) { Inertia.Share("erro", flashErro); context.Session.Remove("Flash:erro"); }
    if (flashSucesso != null) { Inertia.Share("sucesso", flashSucesso); context.Session.Remove("Flash:sucesso"); }
    Inertia.Share("flash", new { erro = flashErro, sucesso = flashSucesso });

    await next();

    if (context.Response.StatusCode is >= 300 and < 400 && !context.Response.HasStarted)
    {
        foreach (var chave in new[] { "erro", "sucesso" })
        {
            if (context.Items.TryGetValue(RETSYS.Web.Infrastructure.MensagemFlash.PrefixoItem + chave, out var valor)
                && valor is string texto && !string.IsNullOrWhiteSpace(texto))
            {
                context.Session.SetString($"Flash:{chave}", texto);
            }
            else
            {
                // Redirect encadeado (ex.: /armacoes → /estoque): a mensagem lida nesta requisição
                // não chegou a ser exibida, então volta para a sessão
                var pendente = chave == "erro" ? flashErro : flashSucesso;
                if (pendente != null) context.Session.SetString($"Flash:{chave}", pendente);
            }
        }
    }
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

    // A antiga "Limpeza SQL Matriz" (renomear/fundir óticas chamadas "Matriz") foi removida:
    // com o cadastro público, empresas distintas com o mesmo nome seriam fundidas em uma só.
}    

app.Run();
