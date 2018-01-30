using Project_storage.Data.Enums;
using Project_storage.Data.Models;
using Project_storage.Logic.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Tests
{
    public class ExtensionsTest
    {
        [Fact]
        public void AvailableAmount_AssertTrue_WhenAmounIsReserved()
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
                ExpirationDate = DateTime.UtcNow.AddMinutes(5)
            };

            var transactionProducts = new List<TransactionProduct>()
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
                    TransactionStatus = TransactionStatus.Success,
                    Product = product,
                    Transaction = transaction
                }
            }.AsQueryable();

            // Act
            int result = product.AvailableAmount(transactionProducts);

            // Assert
            Assert.True(result == 5);
        }
        [Fact]
        public void AvailableAmount_AssertTrue_WhenTransactionHasExpired()
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
                ExpirationDate = DateTime.UtcNow.AddMinutes(-1)
            };

            var transactionProducts = new List<TransactionProduct>()
            {
                new TransactionProduct
                {
                    Id = Guid.NewGuid(),
                    Amount = 5,
                    TransactionStatus = TransactionStatus.Reserved,
                    Product = product,
                    Transaction = transaction
                }
            }.AsQueryable();

            // Act
            int result = product.AvailableAmount(transactionProducts);

            // Assert
            Assert.True(result == 10);
        }
    }
}
