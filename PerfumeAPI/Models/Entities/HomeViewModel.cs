using PerfumeAPI.Models.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PerfumeAPI.Models
{
    public class HomeViewModel
    {
        public List<Product> FeaturedPerfumes { get; set; } = new List<Product>();
        public List<Product> CustomerFavorites { get; set; } = new List<Product>();

        public bool HasFeaturedProducts => FeaturedPerfumes?.Any() ?? false;
        public bool HasCustomerFavorites => CustomerFavorites?.Any() ?? false;
    }
}