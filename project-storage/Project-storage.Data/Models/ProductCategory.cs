using System;
using System.Collections.Generic;

namespace Project_storage.Data.Models
{
    public class ProductCategory
    {
        public ProductCategory()
        {
            Products = new HashSet<Product>();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public HashSet<Product> Products { get; set; }
    }
}