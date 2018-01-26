using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project_storage.Data.Models;

namespace Project_storage.Areas.Manage.Models.Products
{
    public class ProductVM
    {
        public Guid Id { get; set; }

        public ProductCategory ProductCategory { get; set; }

        public decimal Price { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public Guid ChosenCategoryId { get; set; }

        public List<SelectListItem> Categories { get; set; }
    }
}
