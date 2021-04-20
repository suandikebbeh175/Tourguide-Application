using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using travelus.Data;
using travelus.Models;
using travelus.Models.ViewModels;
using travelus.Utility;

namespace travelus.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel()
            {
                TourItem = await _db.TourItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Category = await _db.Category.ToListAsync(),
            };

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim!=null)
            {
                var cnt = _db.BookingCart.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssBookingCartCount, cnt);
            }

            return View(IndexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var tourItemFromDb = await _db.TourItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();

            BookingCart cartObj = new BookingCart()
            {
                TourItem = tourItemFromDb,
                TourItemId = tourItemFromDb.Id
            };

            return View(cartObj);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details (BookingCart CartObject)
        {
            CartObject.Id = 0;
            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                BookingCart cartFromDb = await _db.BookingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId
                                                && c.TourItemId == CartObject.TourItemId).FirstOrDefaultAsync();

                if(cartFromDb==null)
                {
                    await _db.BookingCart.AddAsync(CartObject);
                }
                else
                {
                    cartFromDb.People = cartFromDb.People + CartObject.People;
                }
                await _db.SaveChangesAsync();

                var count = _db.BookingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssBookingCartCount, count);

                return RedirectToAction("Index");
            }
            else
            {
                var tourItemFromDb = await _db.TourItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == CartObject.TourItemId).FirstOrDefaultAsync();

                BookingCart cartObj = new BookingCart()
                {
                    TourItem = tourItemFromDb,
                    TourItemId = tourItemFromDb.Id
                };

                return View(cartObj);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
