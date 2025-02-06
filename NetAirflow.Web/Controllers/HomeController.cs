using Hangfire;
using Microsoft.AspNetCore.Mvc;
using NetAirflow.Web.Comun;
using NetAirflow.Web.Models;
using System.Diagnostics;
using static NetAirflow.Web.Controllers.HomeController;

namespace NetAirflow.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(DdlService ddlService)
        {
            //BackgroundJob.Enqueue(() => MyJobs.WriteHello());
            //return RedirectToAction("Index");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static class MyJobs
        {
            public static void WriteHello()
            {
                Console.WriteLine("hola");
            }
        }
    }
}
