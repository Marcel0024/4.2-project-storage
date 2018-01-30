using System;
using System.Collections.Generic;
using System.Text;

namespace Project_storage.Data.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public decimal Price { get; set; }

        public int Amount { get; set; }

        public string ImageUrl { get; set; }

        public virtual Location Location { get; set; }

        public virtual ProductCategory ProductCategory { get; set; }
    }
}
