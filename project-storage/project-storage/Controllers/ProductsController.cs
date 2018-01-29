using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using Project_storage.Extensions;
using System;
using System.Linq;

namespace Project_storage.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;
        private Random random = new Random();

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

            var results = products.OrderBy(p => p.Price).Select(async p => new
            {
                id = p.Id.ToString("N"),
                name = p.Name,
                price = p.Price,
                shortDescription = p.ShortDescription,
                location = p.Location.Name,
                amount = await p.AvailableAmount(_projectStorageContext),
                imageUrl = p.ImageUrl ?? _randomImageUrl(),
                categoryId = p.ProductCategory.Id.ToString("N")
            }).Select(x => x.Result);

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
                "https://media.giphy.com/media/W8OqjAyhhJv2/giphy.gif",
                "https://media.giphy.com/media/nxoHVeNF9ZLDa/giphy.gif",
                "https://media.giphy.com/media/xD0kepGsYSVeE/giphy.gif"
            };

            var index = random.Next(urls.Length);

            return urls[index];
        }
    }
}
