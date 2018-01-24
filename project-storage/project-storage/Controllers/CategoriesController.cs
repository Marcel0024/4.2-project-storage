using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project_storage.Data;

namespace Project_storage.Controllers
{
    public class CategoriesController : Controller
    {
        private ProjectStorageContext _projectStorageContext;

        public CategoriesController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        public IActionResult Index()
        {
            var categories = _projectStorageContext.ProductCategories
                .ToList()
                .Select(pc => new
                {
                    id = pc.Id.ToString("N"),
                    name = pc.Name
                });

            return Json(new {  categories });
        }
    }
}