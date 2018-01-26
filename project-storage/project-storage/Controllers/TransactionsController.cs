using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using Project_storage.Data.Models;
using Project_storage.Helpers;
using Project_storage.Models.Transactions;

namespace Project_storage.Controllers
{
    public class TransactionsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;

        public TransactionsController(ProjectStorageContext projectStorageContext)
        {
            _projectStorageContext = projectStorageContext;
        }

        [HttpPost]
        public IActionResult Prepare([FromBody] PrepareVM vm)
        {
            return Json(new
            {
               transactionId = GuidHelper.GenerateGuid().ToString("N"),
               expirationDate = DateTime.UtcNow.AddHours(1).AddMinutes(5).ToString(),
               products = vm.Products.Select(p => {

                   var productOffer = _projectStorageContext.ProductOffers
                   .Include(po => po.Product)
                   .FirstOrDefault(po => po.Id == Guid.Parse(p.Product_Id));

                   return new
                   {
                       productId = p.Product_Id,
                       price = productOffer.Product.Price,
                       name = productOffer.Product.Name,
                       shorDescription = productOffer.Product.ShortDescription,
                       available = productOffer.Amount, // Todo - subtract reserves
                       result = "reserved"
                   };
               })
            });
        }
    }
}