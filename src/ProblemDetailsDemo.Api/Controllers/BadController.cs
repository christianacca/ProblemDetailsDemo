using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProblemDetailsDemo.Api.Controllers
{
    public class BadController : Controller
    {
        [HttpGet]
        public ViewResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Status(int statusCode)
        {
            return StatusCode(statusCode);
        }

        [HttpGet]
        public IActionResult ThrowNotImplemented()
        {
            throw new NotImplementedException("This is an exception thrown from an MVC controller.");
        }

        public IActionResult MissingViewTemplate()
        {
            return View();
        }
    }
}