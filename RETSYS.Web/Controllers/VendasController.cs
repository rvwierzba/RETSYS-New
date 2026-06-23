using Microsoft.AspNetCore.Mvc;
using InertiaCore;

namespace RETSYS.Web.Controllers;

public class VendasController : Controller
{
    [HttpGet("/vendas")]
    public IActionResult Index()
    {
        // Renderiza a página Frontend/Pages/Vendas/Index.vue
        return Inertia.Render("Vendas/Index");
    }
}