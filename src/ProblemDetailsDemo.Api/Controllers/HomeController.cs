using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsDemo.Api.Models;

namespace ProblemDetailsDemo.Api.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
  }
}