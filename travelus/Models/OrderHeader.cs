using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace travelus.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public double OrderTotalOriginal { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name ="Order Total")]
        public double OrderTotal { get; set; }

        [Required]
        [Display(Name ="Check-In Time")]
        public DateTime CheckinTime { get; set; }

        [Required]
        [NotMapped]
        public DateTime CheckinDate { get; set; }

        [Display(Name = "Payment Status")]
        public string Status { get; set; }
        public string PaymentStatus { get; set; }

        [Display(Name = "Check-In Name")]
        public string CheckinName { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNummber { get; set; }

        public string TransactionId { get; set; }

    }
}
