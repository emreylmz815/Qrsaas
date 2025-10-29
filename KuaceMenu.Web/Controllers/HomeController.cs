using Microsoft.AspNetCore.Mvc;

namespace KuaceMenu.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
