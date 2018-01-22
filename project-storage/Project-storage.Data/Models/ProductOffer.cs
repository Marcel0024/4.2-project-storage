using System;
using System.Collections.Generic;
using System.Text;

namespace Project_storage.Data.Models
{
    public class ProductOffer
    {
        public Guid Id { get; set; }

        public virtual Product Product { get; set; }

        public virtual Location Location { get; set; }

     //   public int Amount { get; set; }
    }
}
