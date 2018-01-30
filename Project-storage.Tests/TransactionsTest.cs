using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using Project_storage.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Project_storage.Tests
{
    public class TransactionsTest
    {
        [Fact]
        public void ChangeStatus_AssertTrue_WhenAllSuccess()
        {
            // Setup
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Amount = 50
            };

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                ExpirationDate = DateTime.Now.AddMinutes(5),
                OrderId = 10,
                TransactionOrders = new List<TransactionProduct>()
                {
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 4,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product
                    },
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 4,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product
                    }
                }
            };

            // Act
            TransactionsLogic.ChangeTransactionStatus(transaction, TransactionStatus.Success);

            // Assert
            Assert.True(transaction.TransactionOrders.All(to => to.TransactionStatus == TransactionStatus.Success));
        }

        [Fact]
        public void ChangeStatus_AssertTrue_ProductIsSubtractedWhenSuccess()
        {
            // Setup
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Amount = 10
            };

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                ExpirationDate = DateTime.Now.AddMinutes(5),
                OrderId = 10,
                TransactionOrders = new List<TransactionProduct>()
                {
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 5,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product
                    },
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 5,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product
                    }
                }
            };

            // Act
            TransactionsLogic.ChangeTransactionStatus(transaction, TransactionStatus.Success);

            // Assert
            Assert.True(product.Amount == 0);
        }


        [Fact]
        public void CanReserveProduct_AssertTrue_IfAvailable()
        {
            // Setup
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Amount = 10
            };

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                ExpirationDate = DateTime.Now.AddMinutes(5),
                OrderId = 10
            };

            var transactionOrders = new List<TransactionProduct>()
            {
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 5,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product
                    }
            };

            // Act
            var transactionStatus = TransactionsLogic.CanReserveProduct(
                product,
                transaction.TransactionOrders.AsQueryable(),
                5);

            // Assert
            Assert.True(transactionStatus == TransactionStatus.Reserved);
        }

        [Fact]
        public void CanReserveProduct_AssertFail_IfNoneAvailable()
        {
            // Setup
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Amount = 10
            };

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                ExpirationDate = DateTime.Now.AddMinutes(5),
                OrderId = 10
            };

            var transactionOrders = new List<TransactionProduct>()
            {
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 5,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product,
                        Transaction = transaction
                    },
                    new TransactionProduct
                    {
                        Id = Guid.NewGuid(),
                        Amount = 5,
                        TransactionStatus = TransactionStatus.Reserved,
                        Product = product,
                        Transaction = transaction
                    }
            };

            // Act
            var transactionStatus = TransactionsLogic.CanReserveProduct(
                product,
                transaction.TransactionOrders.AsQueryable(),
                1);

            // Assert
            Assert.False(transactionStatus == TransactionStatus.Failed);
        }
    }
}
