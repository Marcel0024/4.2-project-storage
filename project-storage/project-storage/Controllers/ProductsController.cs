using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using System;
using System.Linq;

namespace Project_storage.Controllers
{
    public class ProductsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;
        private Random random = new Random();

        public ProductsController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        public IActionResult Index(string location = null, string category = null)
        {
            var products = _projectStorageContext.ProductOffers
                .Include(po => po.Product).ThenInclude(p => p.ProductCategory)
                .Include(po => po.Location)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                products = products.Where(po => po.Product.ProductCategory.Id == Guid.Parse(category));

            if (!string.IsNullOrWhiteSpace(location))
                products = products.Where(po => po.Location.Name.ToLowerInvariant() == location.ToLowerInvariant());

            var results = products.Select(po => new
            {
                id = po.Id.ToString("N"),
                name = po.Product.Name,
                price = po.Product.Price,
                shortDescription = po.Product.ShortDescription,
                location = po.Location.Name,
                amount = po.Amount,
                imageUrl = po.ImageUrl ?? _randomImageUrl(),
                categoryId = po.Product.ProductCategory.Id.ToString("N")
            });

            return Json(new
            {
                total = results.Count(),
                products = results
            });
        }

        private string _randomImageUrl()
        {
            string[] urls = {
                "https://media.giphy.com/media/kTZBUjdRlZB3G/giphy.gif",
                "https://media.giphy.com/media/GZV2ju1CodSDu/giphy.gif",
                "https://media.giphy.com/media/iCOZGQacrbNZu/giphy.gif",
                "https://media.giphy.com/media/na6lKf6C7eQwM/giphy.gif",
                "http://media.giphy.com/media/Md053RIVMG3Re/giphy.gif",
                "https://media.giphy.com/media/WoGkaE4VyuV4k/giphy.gif",
                "https://media.giphy.com/media/y6IvUPfopAZqg/giphy.gif",
                "https://media.giphy.com/media/i87eYpNGoGTD2/giphy.gif",
                "https://media.giphy.com/media/l0HU4MZKbCCm49yMw/giphy.gif",
                "https://media.giphy.com/media/IITbm6ooxf5Xa/giphy.gif",
                "https://media.giphy.com/media/7eVp9MHlNI90c/giphy.gif",
                "https://media.giphy.com/media/UruEY4Y78edYk/giphy.gif",
                "https://media.giphy.com/media/W8OqjAyhhJv2/giphy.gif"
            };
            
            var index = random.Next(urls.Length);

            return urls[index];
        }
    }
}
