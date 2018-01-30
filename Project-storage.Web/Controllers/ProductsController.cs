using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using Project_storage.Logic.Extensions;
using System;
using System.Linq;

namespace Project_storage.Web.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;

        public ProductsController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        public IActionResult Index(string category = "")
        {
            var products = _projectStorageContext.Products
                .Include(p => p.ProductCategory)
                .Include(po => po.Location)
                .AsEnumerable();

            if (Guid.TryParse(category, out Guid parsedGuid))
            {
                products = products.Where(p => p.ProductCategory.Id == parsedGuid);
            }
            else if (!string.IsNullOrWhiteSpace(category))
            {
                return StatusCode(508, "Category not found");
            }

            var results = products.OrderBy(p => p.Price).Select(p => new
            {
                id = p.Id.ToString("N"),
                name = p.Name,
                price = p.Price,
                shortDescription = p.ShortDescription,
                location = p.Location.Name,
                amount = p.AvailableAmount(_projectStorageContext.TransactionProducts),
                imageUrl = p.ImageUrl ?? "https://ak0.picdn.net/shutterstock/videos/8802700/thumb/1.jpg",
                categoryId = p.ProductCategory.Id.ToString("N")
            });

            return Json(new
            {
                total = results.Count(),
                products = results
            });
        }
    }
}
