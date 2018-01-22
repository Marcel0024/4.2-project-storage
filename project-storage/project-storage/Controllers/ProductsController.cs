using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project_storage.Models;

namespace Project_storage.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
           return Content("<h1> Online </h1>");
        }

        public IActionResult All()
        {
            return Json(new { Hoi = "lol"});
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
