using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_storage.Areas.Manage.Models;
using Project_storage.Areas.Manage.Models.Products;
using Project_storage.Data;
using Project_storage.Data.Models;
using Project_storage.Helpers;

namespace Project_storage.Areas.Manage.Controllers
{
    [Area("manage")]
    public class ProductsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;

        public ProductsController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        public IActionResult Index()
        {
            var vm = new IndexVM
            {
                Products = _projectStorageContext.Products
                .Include(p => p.ProductCategory)
                .ToList()
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ProductCategory = p.ProductCategory,
                    ShortDescription = p.ShortDescription,
                    LongDescription = p.LongDescription,
                    ChosenCategoryId = p.ProductCategory.Id,
                    Categories = _getProductCategories()
                }).ToList()
            };

            vm.Products.Add(new ProductVM { Categories = _getProductCategories() });

            return View(vm);
        }

        public async Task<IActionResult> SaveProducts(IndexVM vm)
        {
            foreach(var product in vm.Products)
            {
                var productDb = await _projectStorageContext.Products
                    .Include(p => p.ProductCategory)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (productDb == null)
                {
                    if (string.IsNullOrEmpty(product.Name) || product.Price == 0)
                        continue;

                    productDb = new Product { Id = GuidHelper.GenerateGuid() };
                    _projectStorageContext.Products.Add(productDb);
                }

                productDb.LongDescription = product.LongDescription;
                productDb.ShortDescription = product.ShortDescription;
                productDb.Name = product.Name;
                productDb.Price = product.Price;

                productDb.ProductCategory = _projectStorageContext.ProductCategories.Find(product.ChosenCategoryId);

                await _projectStorageContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private List<SelectListItem> _getProductCategories()
        {
           return _projectStorageContext.ProductCategories.ToList().Select(pc => new SelectListItem{
               Text = pc.Name,
               Value = pc.Id.ToString(),
           }).ToList();
        }
    }
}