using Project_storage.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Extensions
{
    public static class TransactionExtensions
    {
        public static bool HasExpired(this Transaction transaction)
        {
            return transaction.ExpirationDate < DateTime.UtcNow;
        }
    }
}
