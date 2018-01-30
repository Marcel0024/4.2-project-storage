using System;
using System.Collections.Generic;

namespace Project_storage.Data.Models
{
    public class Transaction
    {
        public Transaction()
        {
            TransactionOrders = new HashSet<TransactionProduct>();
        }

        public Guid Id { get; set; }

        public DateTime ExpirationDate { get; set; }

        public int OrderId { get; set; }

        public ICollection<TransactionProduct> TransactionOrders { get; set; }
    }
}
