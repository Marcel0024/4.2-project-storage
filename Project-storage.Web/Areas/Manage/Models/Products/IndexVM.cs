using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_storage.Web.Areas.Manage.Models.Products
{
    public class IndexVM
    {
        public IndexVM()
        {
            Products = new List<ProductVM>();
        }

        public List<ProductVM> Products { get; set; }
    }
}
