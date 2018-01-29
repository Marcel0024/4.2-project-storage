using Microsoft.EntityFrameworkCore;
using Project_storage.Data;
using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Extensions
{
    public static class ProductExtensions
    {
        public static async Task<int> AvailableAmount(this Product product, ProjectStorageContext _context)
        {
            var pendingTransactions = await  _context.TransactionProducts
                .Where(t => t.TransactionStatus == TransactionStatus.Reserved)
                .Where(t => t.Product.Id == product.Id)
                .Select(t => t.Amount)
                .ToListAsync();

            int reservedAmount = pendingTransactions.Sum();

            return product.Amount - reservedAmount;
        }
    }
}
