using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using travelus.Data;
using travelus.Models;
using travelus.Models.ViewModels;
using travelus.Utility;

namespace travelus.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }

        public CartController(ApplicationDbContext db,IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> Index()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            detailCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            

            var cart = _db.BookingCart.Where(c => c.ApplicationUserId == claim.Value);
            if(cart !=null)
            {
                detailCart.listCart = cart.ToList();
            }

            foreach(var list in detailCart.listCart)
            {
                list.TourItem = await _db.TourItem.FirstOrDefaultAsync(m => m.Id == list.TourItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.TourItem.Price * list.People);
                list.TourItem.Description = SD.ConvertToRawHtml(list.TourItem.Description);
                if(list.TourItem.Description.Length>100)
                {
                    list.TourItem.Description = list.TourItem.Description.Substring(0, 99) + "...";
                }

            }
            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal;
            


            return View(detailCart);
        }

        public async Task<IActionResult> Summary()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            detailCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await _db.ApplicationUser.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();
            var cart = _db.BookingCart.Where(c => c.ApplicationUserId == claim.Value);
            if(cart !=null)
            {
                detailCart.listCart = cart.ToList();
            }

            foreach(var list in detailCart.listCart)
            {
                list.TourItem = await _db.TourItem.FirstOrDefaultAsync(m => m.Id == list.TourItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.TourItem.Price * list.People);

            }
            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal;
            detailCart.OrderHeader.CheckinName = applicationUser.Name;
            detailCart.OrderHeader.PhoneNummber = applicationUser.PhoneNumber;
            detailCart.OrderHeader.CheckinTime = DateTime.Now;

            return View(detailCart);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            detailCart.listCart = await _db.BookingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailCart.OrderHeader.OrderDate = DateTime.Now;
            detailCart.OrderHeader.UserId = claim.Value;
            detailCart.OrderHeader.Status = SD.PaymentStatusPending;
            detailCart.OrderHeader.CheckinTime = Convert.ToDateTime(detailCart.OrderHeader.CheckinDate.ToShortDateString() + " " + detailCart.OrderHeader.CheckinTime.ToShortTimeString());

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(detailCart.OrderHeader);
            await _db.SaveChangesAsync();

            detailCart.OrderHeader.OrderTotalOriginal = 0;

            foreach (var item in detailCart.listCart)
            {
                item.TourItem = await _db.TourItem.FirstOrDefaultAsync(m => m.Id == item.TourItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    TourItemId = item.TourItemId,
                    OrderId = detailCart.OrderHeader.Id,
                    Description = item.TourItem.Description,
                    Name = item.TourItem.Name,
                    Price = item.TourItem.Price,
                    People = item.People
                };
                detailCart.OrderHeader.OrderTotalOriginal += orderDetails.People * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails);

            }
            
            detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotalOriginal;
            await _db.SaveChangesAsync();
            _db.BookingCart.RemoveRange(detailCart.listCart);
            HttpContext.Session.SetInt32(SD.ssBookingCartCount, 0);
            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(detailCart.OrderHeader.OrderTotal * 100),
                Currency = "gbp",
                Description = "Order ID : " + detailCart.OrderHeader.Id,
                Source = stripeToken
            };
            var service = new ChargeService();
            Charge charge = service.Create(options);

            if(charge.BalanceTransactionId == null)
            {
                detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                detailCart.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if(charge.Status.ToLower() == "succeeded")
            {
                //Send email after succssful booking
                await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email, "Travelus - Tour Booked " + detailCart.OrderHeader.Id.ToString(), "Tour has been booked successfully");

                detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                detailCart.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }

            await _db.SaveChangesAsync();
            //return RedirectToAction("Index", "Home");
            return RedirectToAction("Confirm", "Order", new { id = detailCart.OrderHeader.Id });
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.BookingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.People += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.BookingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            if(cart.People==1)
            {
                _db.BookingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.BookingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssBookingCartCount, cnt);
            }
            else
            {
                cart.People -= 1;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.BookingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            
            _db.BookingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.BookingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.ssBookingCartCount, cnt);


            return RedirectToAction(nameof(Index));
        }

       
    }
}
