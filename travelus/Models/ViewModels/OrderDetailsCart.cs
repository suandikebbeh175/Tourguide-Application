using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace travelus.Models.ViewModels
{
    public class OrderDetailsCart
    {
        public List<BookingCart> listCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
