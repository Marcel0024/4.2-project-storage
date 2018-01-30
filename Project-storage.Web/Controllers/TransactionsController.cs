using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project_storage.Data;
using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using Project_storage.Logic;
using Project_storage.Logic.Extensions;
using Project_storage.Web.Helpers;
using Project_storage.Web.Models.Transactions;

namespace Project_storage.Web.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private ProjectStorageContext _projectStorageContext;
        private AppSettings _appSettings;

        public TransactionsController(ProjectStorageContext projectStorageContext, IOptions<AppSettings> appSettings)
        {
            _projectStorageContext = projectStorageContext;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Prepare a order
        /// Add products with reserve status
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Prepare([FromBody] PrepareVM vm)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var transactionDb = _projectStorageContext.Transactions.SingleOrDefault(x => x.OrderId == vm.Order_Id);

            if (transactionDb != null)
                return BadRequest("OrderId already exist");

            var transaction = await _storeTransaction(vm);

            return Json(new
            {
                transactionId = transaction.Id.ToString("N"),
                expirationDate = transaction.ExpirationDate.AddHours(1).ToString(),
                products = transaction.TransactionOrders.Select(to =>
                {
                    return new
                    {
                        productId = to.Product.Id.ToString("N"),
                        price = to.Product.Price,
                        name = to.Product.Name,
                        shortDescription = to.Product.ShortDescription,
                        available = to.Product.AvailableAmount(_projectStorageContext.TransactionProducts),
                        amountReserved = to.Amount,
                        result = to.TransactionStatus.ToString().ToLowerInvariant()
                    };
                })
            });
        }

        /// <summary>
        /// Action method to success / failed a transaction
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Commit([FromBody] CommitVM vm)
        {
            var transaction = await _projectStorageContext.Transactions
                .Include(t => t.TransactionOrders).ThenInclude(to => to.Product)
                .FirstOrDefaultAsync(t => t.OrderId == vm.Order_Id);

            if (transaction == null)
                return BadRequest("Order not found");

            if (transaction.HasExpired() && transaction.TransactionOrders.Any(to => to.TransactionStatus == TransactionStatus.Reserved))
            {
                TransactionsLogic.ChangeTransactionStatus(transaction, TransactionStatus.Failed);

                await _projectStorageContext.SaveChangesAsync();

                return BadRequest("Transaction has expired");
            }

            var newStatus = TransactionStatus.Failed;
            if (vm.Status.ToLowerInvariant().Trim() == "success")
                newStatus = TransactionStatus.Success;

            TransactionsLogic.ChangeTransactionStatus(transaction, newStatus);

            await _projectStorageContext.SaveChangesAsync();

            return Ok();
        }


        /// <summary>
        /// Saves the PrepareVM to database
        /// </summary>
        private async Task<Transaction> _storeTransaction(PrepareVM vm)
        {
            var transaction = new Transaction
            {
                Id = GuidHelper.GenerateGuid(),
                ExpirationDate = DateTime.UtcNow.AddMinutes(_appSettings.TransactionValidInMinutes),
                OrderId = vm.Order_Id,
                TransactionOrders = vm.Products.Select(p =>
                {
                    var product = _projectStorageContext.Products.Find(Guid.Parse(p.Product_Id));

                    if (product == null)
                        return null;

                    var transactionProducts = _projectStorageContext.TransactionProducts
                    .Include(tp => tp.Transaction)
                    .Include(tp => tp.Product);

                    return new TransactionProduct
                    {
                        Id = GuidHelper.GenerateGuid(),
                        Price = product.Price,
                        Amount = p.Amount,
                        Product = product,
                        TransactionStatus = TransactionsLogic.CanReserveProduct(product, transactionProducts, p.Amount)
                    };
                }).Where(p => p != null).ToList()
            };

            _projectStorageContext.Transactions.Add(transaction);
            await _projectStorageContext.SaveChangesAsync();

            return transaction;
        }

        [AllowAnonymous]
        public IActionResult Authorized()
        {
            return Json(User.Identity.Name);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExpiredTransactions()
        {
            var transactionOrders = await _projectStorageContext.TransactionProducts
                .Where(t => t.TransactionStatus == TransactionStatus.Reserved)
                .Where(t => t.Transaction.HasExpired())
                .ToListAsync();

            foreach (var transactionOrder in transactionOrders)
                transactionOrder.TransactionStatus = TransactionStatus.Failed;

            await _projectStorageContext.SaveChangesAsync();

            return Ok();
        }

        [AllowAnonymous]
        public IActionResult Status()
        {
            var transactions = _projectStorageContext.Transactions
                .Include(t => t.TransactionOrders)
                .ThenInclude(to => to.Product)
                .ThenInclude(to => to.Location)
                .OrderByDescending(to => to.ExpirationDate)
                .ToList();

            return Json(transactions.Select(t => new
            {
                id = t.Id.ToString("N"),
                expire = t.ExpirationDate.AddHours(1).ToString(),
                orderId = t.OrderId,
                products = t.TransactionOrders.Select(p =>
                new
                {
                    amount = p.Amount,
                    status = p.TransactionStatus.ToString(),
                    product = new
                    {
                        name = p.Product.Name,
                        price = p.Product.Price,
                        available = p.Product.AvailableAmount(_projectStorageContext.TransactionProducts),
                        inDb = p.Product.Amount
                    }
                })
            }));
        }

        //[AllowAnonymous]
        //public async Task<IActionResult> DeleteTransactions()
        //{
        //    var transactionsP = await _projectStorageContext.TransactionProducts
        //        .ToListAsync();

        //    foreach (var transaction in transactionsP)
        //    {
        //        _projectStorageContext.TransactionProducts.Remove(transaction);
        //    }

        //    var transactions = await _projectStorageContext.Transactions.ToListAsync();

        //    foreach (var transaction in transactions)
        //    {
        //        _projectStorageContext.Transactions.Remove(transaction);
        //    }

        //    await _projectStorageContext.SaveChangesAsync();

        //    return Content("Success");
        //}
    }
}