using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project_storage.Data.Models;

namespace Project_storage.Web.Areas.Manage.Models.Products
{
    public class ProductVM
    {
        public Guid Id { get; set; }

        public ProductCategory ProductCategory { get; set; }

        public decimal Price { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public Guid ChosenProductCategory { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public Guid ChosenLocationId { get; set; }

        public IEnumerable<SelectListItem> Locations { get; set; }

        public string ImageUrl { get; set; }

        public int Amount { get; set; }
    }
}
