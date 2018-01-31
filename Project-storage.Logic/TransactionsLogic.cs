using System;
using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using System.Linq;
using Project_storage.Logic.Extensions;

namespace Project_storage.Logic
{
    public static class TransactionsLogic
    {
        public static void ChangeTransactionStatus(Transaction transaction, TransactionStatus status)
        {
            foreach (var transactionOrder in transaction.TransactionOrders)
            {
                if (transactionOrder.TransactionStatus != TransactionStatus.Reserved)
                    continue;

                if (status == TransactionStatus.Success)
                    transactionOrder.Product.Amount = transactionOrder.Product.Amount - transactionOrder.Amount;

                transactionOrder.TransactionStatus = status;
            }
        }

        public static TransactionStatus CanReserveProduct(Product product, IQueryable<TransactionProduct> transactionProducts, int amount)
        {
            return product.AvailableAmount(transactionProducts) - amount >= 0 ? TransactionStatus.Reserved : TransactionStatus.Failed;
        }
    }
}
