using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Logic.Extensions
{
    public static class ProductExtensions
    {
        public static int AvailableAmount(this Product product, IQueryable<TransactionProduct> transactionProducts)
        {
            var pendingTransactions = transactionProducts
                .Where(t => t.TransactionStatus == TransactionStatus.Reserved)
                .Where(t => t.Product.Id == product.Id)
                .Where(t => t.Transaction.ExpirationDate > DateTime.UtcNow)
                .Select(t => t.Amount)
                .ToList();

            int reservedAmount = pendingTransactions.Sum();

            return product.Amount - reservedAmount;
        }
    }
}
