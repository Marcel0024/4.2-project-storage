using System;
using System.Collections.Generic;
using System.Text;

namespace Project_storage.Data.Models
{
    public class ProductOffer
    {
        public Guid Id { get; set; }

        public Product Product { get; set; }

        public Location Location { get; set; }
    }
}
