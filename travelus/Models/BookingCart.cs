using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace travelus.Models
{
    public class BookingCart
    {
        public BookingCart()
        {
            People = 1;
        }
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        [NotMapped]
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public int TourItemId { get; set; }

        [NotMapped]
        [ForeignKey("TourItemId")]
        public virtual TourItem TourItem { get; set; }


        [Range(1,int.MaxValue, ErrorMessage ="Please enter a value greater than or equal to {1}")]
        public int People { get; set; }
    }
}
