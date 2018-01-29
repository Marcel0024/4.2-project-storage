using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project_storage.Data;
using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using Project_storage.Extensions;
using Project_storage.Helpers;
using Project_storage.Models.Transactions;

namespace Project_storage.Controllers
{
    public class TransactionsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;
        private AppSettings _appSettings;

        public TransactionsController(ProjectStorageContext projectStorageContext, IOptions<AppSettings> appSettings)
        {
            _projectStorageContext = projectStorageContext;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Prepare([FromBody] PrepareVM vm)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var transactionDb = _projectStorageContext.Transactions.SingleOrDefault(x => x.OrderId == vm.Order_Id);

            if (transactionDb != null)
                return BadRequest("OrderId already exist");

            var transaction = new Transaction
            {
                Id = GuidHelper.GenerateGuid(),
                ExpirationDate = DateTime.UtcNow.AddMinutes(_appSettings.TransactionValidInMinutes),
                OrderId = vm.Order_Id,
                TransactionOrders = vm.Products.Select(async p =>
                {
                    var product = _projectStorageContext.Products.Find(Guid.Parse(p.Product_Id));

                    if (product == null)
                        return null;

                    return new TransactionProduct
                    {
                        Id = GuidHelper.GenerateGuid(),
                        Price = product.Price,
                        Amount = p.Amount,
                        Product = product,
                        TransactionStatus = (await product.AvailableAmount(_projectStorageContext)) - p.Amount >= 0 ? TransactionStatus.Reserved : TransactionStatus.Failed
                    };
                }).Where(p => p != null).Select(p => p.Result).ToList()
            };

            _projectStorageContext.Transactions.Add(transaction);
            await _projectStorageContext.SaveChangesAsync();

            return Json(new
            {
                transactionId = transaction.Id.ToString("N"),
                expirationDate = transaction.ExpirationDate.AddHours(1).ToString(),
                products = transaction.TransactionOrders.Select(async to =>
                {
                    return new
                    {
                        productId = to.Product.Id.ToString("N"),
                        price = to.Product.Price,
                        name = to.Product.Name,
                        shortDescription = to.Product.ShortDescription,
                        available = await to.Product.AvailableAmount(_projectStorageContext),
                        amountReserved = to.Amount,
                        result = to.TransactionStatus.ToString().ToLowerInvariant()
                    };
                }).Select(t => t.Result)
            });
        }

        public async Task<IActionResult> ExpiredTransactions()
        {
            try
            {
                var transactions = await _projectStorageContext.Transactions
                    .Where(t => t.ExpirationDate < DateTime.UtcNow)
                    .Where(t => t.TransactionOrders.Any(to => to.TransactionStatus == TransactionStatus.Reserved))
                    .ToListAsync();

                foreach (var transaction in transactions)
                {
                    foreach (var transactionOrder in transaction.TransactionOrders)
                    {
                        transactionOrder.TransactionStatus = TransactionStatus.Failed;
                    }
                }

                return Ok();
            }

            catch (Exception e)
            {
                return StatusCode(400, e.ToString());
            }
        }

        public async Task<IActionResult> Test()
        {
            var transactions = await _projectStorageContext.Transactions
                .Include(t => t.TransactionOrders)
                .ThenInclude(to => to.Product)
                .ToListAsync();

            return Json(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> Commit([FromBody] CommitVM vm)
        {
            var transaction = await _projectStorageContext.Transactions
                .Include(t => t.TransactionOrders).ThenInclude(to => to.Product)
                .FirstOrDefaultAsync(t => t.OrderId == vm.Order_Id);

            if (transaction == null)
                return BadRequest("Order not found");

            foreach (var transactionOrder in transaction.TransactionOrders)
            {
                if (transactionOrder.TransactionStatus == TransactionStatus.Reserved)
                {
                    if (vm.Status.ToLowerInvariant().Trim() == "success")
                    {
                        transactionOrder.Product.Amount = transactionOrder.Product.Amount - transactionOrder.Amount;
                        transactionOrder.TransactionStatus = TransactionStatus.Success;
                    }
                    else
                    {
                        transactionOrder.TransactionStatus = TransactionStatus.Failed;
                    }
                }
            }

            await _projectStorageContext.SaveChangesAsync();

            return Ok();
        }


        public async Task<IActionResult> DeleteTransactions()
        {
            var transactionsP = await _projectStorageContext.TransactionProducts
                .ToListAsync();

            foreach (var transaction in transactionsP)
            {
                _projectStorageContext.TransactionProducts.Remove(transaction);
            }

            var transactions = await _projectStorageContext.Transactions
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                _projectStorageContext.Transactions.Remove(transaction);
            }

            await _projectStorageContext.SaveChangesAsync();

            return Content("Success");
        }
    }
}