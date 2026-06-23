using Microsoft.AspNetCore.Mvc;
using InertiaCore;

namespace RETSYS.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Inertia.Render("Home/LandingPage");
    }
}