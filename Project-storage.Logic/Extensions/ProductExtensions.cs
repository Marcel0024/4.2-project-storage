using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using System;
using System.Linq;

namespace Project_storage.Logic.Extensions
{
    public static class ProductExtensions
    {
        public static int AvailableAmount(this Product product, IQueryable<TransactionProduct> transactionProducts)
        {
            var pendingTransactions = transactionProducts
                .Where(t => t.TransactionStatus == TransactionStatus.Reserved)
                .Where(t => t.Product.Id == product.Id)
                .Where(t => !t.Transaction.HasExpired())
                .Select(t => t.Amount)
                .ToList();

            int reservedAmount = pendingTransactions.Sum();

            return product.Amount - reservedAmount;
        }
    }
}
