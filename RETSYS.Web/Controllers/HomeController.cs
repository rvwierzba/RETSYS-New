using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InertiaCore;
using RETSYS.Web.Models;

namespace RETSYS.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Inertia.Render("Home/LandingPage");
    }

}
