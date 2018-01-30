using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Web.Models.Transactions
{
    public class PrepareVM
    {
        public PrepareVM()
        {
            Products = new List<Products>();
        }

        [Required]
        public int Order_Id { get; set; }

        public int Status { get; set; }

        public List<Products> Products { get; set; }
    }

    public class Products
    {
        [Required]
        public int Amount { get; set; }

        [Required]
        public string Product_Id { get; set; }
    }
}
