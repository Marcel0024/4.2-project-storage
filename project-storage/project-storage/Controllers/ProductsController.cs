using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using Project_storage.Data.Models;
using Project_storage.Helpers;
using Project_storage.Models;

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
                location = po.Location.Name
            });

            return Json(new
            {
                total = results.Count(),
                products = results
            });
        }

        public async Task<IActionResult> Seed()
        {
            var location1 = _projectStorageContext.Locations.FirstOrDefault();
            var location2 = _projectStorageContext.Locations.Skip(1).FirstOrDefault();

            var pcat1 = new ProductCategory
            {
                Id = GuidHelper.GenerateGuid(),
                Name = "Slakken"
            };

            _projectStorageContext.ProductCategories.Add(pcat1);

            var product1 = new Product
            {
                Id = GuidHelper.GenerateGuid(),
                Name = "Slak turrita",
                Price = 2.5m,
                ProductCategory = pcat1,
                ShortDescription = "De Slak turrita is een zeer geliefde aquariumvis uit de renslak familie. Deze aquariumvis eet, net zoals andere vissen graag Algen, Granulaat. De Slak turrita is ook bekend als neritina turrita, en zwemt graag in een aquarium van 60 cm of groter."
            };

            _projectStorageContext.Products.Add(product1);

            var productOffer1 = new ProductOffer
            {
                Location = location2,
                Product = product1
            };

            _projectStorageContext.ProductOffers.Add(productOffer1);

            var product2 = new Product
            {
                Id = GuidHelper.GenerateGuid(),
                Name = "Slak zebra",
                Price = 3.6m,
                ProductCategory = pcat1,
                ShortDescription = "De Slak zebra is een zeer geliefde aquariumvis uit de slak familie. Deze aquariumvis eet, net zoals andere vissen graag Algen, Bodemvuil, Granulaat. De Slak zebra is ook bekend als neritina sp. horned zebra, en zwemt graag in een aquarium van 60 cm of groter."
            };

            _projectStorageContext.Products.Add(product2);

            var productOffer2 = new ProductOffer
            {
                Location = location1,
                Product = product2
            };

            _projectStorageContext.ProductOffers.Add(productOffer2);

            await _projectStorageContext.SaveChangesAsync();

            return Content("Finished");

        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
