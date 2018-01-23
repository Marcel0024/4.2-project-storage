using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using System.Linq;

namespace Project_storage.Controllers
{
    public class ProductsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;

        public ProductsController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        public IActionResult Index()
        {
            return Content("Online");
        }

        public IActionResult All(string category = null, string location = null)
        {
            var products = _projectStorageContext.ProductOffers
                .Include(po => po.Product).ThenInclude(p => p.ProductCategory)
                .Include(po => po.Location)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                products = products.Where(po => po.Product.ProductCategory.Name.ToLowerInvariant() == category.ToLowerInvariant());

            if (!string.IsNullOrWhiteSpace(location))
                products = products.Where(po => po.Location.Name.ToLowerInvariant() == location.ToLowerInvariant());

            var results = products.Select(po => new
            {
                id = po.Id.ToString("N"),
                name = po.Product.Name,
                price = po.Product.Price,
                shortDescription = po.Product.ShortDescription,
                category = po.Product.ProductCategory.Name,
                location = po.Location.Name,
                amount = po.Amount,
                imageUrl = "http://via.placeholder.com/250?text=hoi"
            });

            return Json(new
            {
                total = results.Count(),
                products = results
            });
        }       
    }
}
