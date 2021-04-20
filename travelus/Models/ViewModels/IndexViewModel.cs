using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace travelus.Models.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<TourItem> TourItem { get; set; }
        public IEnumerable<Category> Category { get; set; }
    }
}
