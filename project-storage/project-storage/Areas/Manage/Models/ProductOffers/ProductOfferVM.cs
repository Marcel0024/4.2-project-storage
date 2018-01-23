using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project_storage.Areas.Manage.Models.ProductOffers
{
    public class ProductOfferVM
    {
        public Guid Id { get; set; }

        public Guid ChosenLocationId { get; set; }

        public IEnumerable<SelectListItem> Locations { get; set; }

        public Guid ChosenProductId { get; set; }

        public IEnumerable<SelectListItem> Products { get; set; }

        public int Amount { get; set; }
    }
}
