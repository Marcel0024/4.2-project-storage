using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_storage.Areas.Manage.Models.ProductOffers;
using Project_storage.Data;
using Project_storage.Helpers;

namespace Project_storage.Areas.Manage.Controllers
{
    [Area("manage")]
    public class ProductOffersController : Controller
    {
        private ProjectStorageContext _projectStorageContext;

        public ProductOffersController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        public IActionResult Index()
        {
            var vm = new IndexVM
            {
                ProductOffers = _projectStorageContext.ProductOffers
                .Include(po => po.Location)
                .Include(po => po.Product)
                .ToList()
                .Select(p => new ProductOfferVM
                {
                    Id = p.Id,
                    ChosenLocationId = p.Location.Id,
                    Locations = _getLocations(),
                    ChosenProductId = p.Product.Id,
                    Products = _getProducts(p.Id),
                    Amount = p.Amount,
                    ImageUrl = p.ImageUrl
                }).ToList()
            };

            vm.ProductOffers.Add(new ProductOfferVM
            {
                Locations = _getLocations(),
                Products = _getProducts(Guid.Empty)
            });

            return View(vm);
        }

        public async Task<IActionResult> SaveProductOffers(IndexVM vm)
        {
            foreach (var productOffer in vm.ProductOffers)
            {
                var productOfferDb = await _projectStorageContext.ProductOffers
                    .Include(p => p.Location)
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.Id == productOffer.Id);

                var location = _projectStorageContext.Locations.Find(productOffer.ChosenLocationId);
                var product = _projectStorageContext.Products.Find(productOffer.ChosenProductId);

                if (product == null || location == null)
                    continue;

                if (productOfferDb == null)
                {
                    productOfferDb = new Data.Models.ProductOffer { Id = GuidHelper.GenerateGuid() };
                    _projectStorageContext.ProductOffers.Add(productOfferDb);
                }

                productOfferDb.Location = location;
                productOfferDb.Product = product;
                productOfferDb.Amount = productOffer.Amount;
                productOfferDb.ImageUrl = productOffer.ImageUrl;

                await _projectStorageContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> _getLocations()
        {
            return _projectStorageContext.Locations.ToList().Select(l => new SelectListItem
            {
                Text = l.Name,
                Value = l.Id.ToString()
            });
        }
        private IEnumerable<SelectListItem> _getProducts(Guid productId)
        {
            var existingProducts = _projectStorageContext.ProductOffers
                .Where(po => po.Id != productId)
                .ToList()
                .Select(po => po.Product)
                .ToList();

            return _projectStorageContext.Products
                .ToList()
                .Except(existingProducts)
                .Select(l => new SelectListItem
            {
                Text = l.Name,
                Value = l.Id.ToString()
            });
        }
    }
}