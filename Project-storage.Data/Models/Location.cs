using System;

namespace Project_storage.Data.Models
{
    public class Location
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string StreetName { get; set; }

        public string Country { get; set; }

        public string Postcode { get; set; }
    }
}