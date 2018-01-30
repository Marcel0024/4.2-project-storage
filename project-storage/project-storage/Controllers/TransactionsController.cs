using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    //[Authorize]
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
                await _changeTransactionStatus(transaction, TransactionStatus.Failed);

                return BadRequest("Transaction has expired");
            }

            var newStatus = TransactionStatus.Failed;
            if (vm.Status.ToLowerInvariant().Trim() == "success")
                newStatus = TransactionStatus.Success;

            await _changeTransactionStatus(transaction, newStatus);

            return Ok();
        }

        private async Task _changeTransactionStatus(Transaction transaction, TransactionStatus status)
        {
            foreach (var transactionOrder in transaction.TransactionOrders)
            {
                if (status == TransactionStatus.Success)
                    transactionOrder.Product.Amount = transactionOrder.Product.Amount - transactionOrder.Amount;

                transactionOrder.TransactionStatus = status;
            }

            await _projectStorageContext.SaveChangesAsync();
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

            return transaction;
        }

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
                id = t.Id,
                expire = t.ExpirationDate.AddHours(1).ToString(),
                orderId = t.OrderId,
                products = t.TransactionOrders.Select(async p =>
                new
                {
                    amount = p.Amount,
                    status = p.TransactionStatus,
                    product = new
                    {
                        name = p.Product.Name,
                        available = await p.Product.AvailableAmount(_projectStorageContext),
                        inDb = p.Product.Amount
                    }
                }).Select(to => to.Result)
            }));
        }

        [AllowAnonymous]
        public async Task<IActionResult> DeleteTransactions()
        {
            var transactionsP = await _projectStorageContext.TransactionProducts
                .ToListAsync();

            foreach (var transaction in transactionsP)
            {
                _projectStorageContext.TransactionProducts.Remove(transaction);
            }

            var transactions = await _projectStorageContext.Transactions.ToListAsync();

            foreach (var transaction in transactions)
            {
                _projectStorageContext.Transactions.Remove(transaction);
            }

            await _projectStorageContext.SaveChangesAsync();

            return Content("Success");
        }
    }
}