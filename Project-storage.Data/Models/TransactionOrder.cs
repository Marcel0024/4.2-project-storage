using Project_storage.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project_storage.Data.Models
{
    public class TransactionProduct
    {
        public Guid Id { get; set; }

        public virtual Transaction Transaction { get; set; }

        public virtual Product Product { get; set; }

        public int Amount { get; set; }

        public Decimal Price { get; set; }

        public TransactionStatus TransactionStatus { get; set; }
    }
}
