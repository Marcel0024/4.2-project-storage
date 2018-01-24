using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Models.Transactions
{
    public class PrepareVM
    {
        public PrepareVM()
        {
            Products = new List<Transactions.Products>();
        }

        public string Token { get; set; }

        public int Order_Id { get; set; }

        public int Status { get; set; }

        public List<Products> Products { get; set; }
    }

    public class Products
    {
        public int Amount { get; set; }

        public string Product_Id { get; set; }
    }
}
