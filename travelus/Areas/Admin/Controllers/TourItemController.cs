using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using travelus.Data;
using travelus.Models;
using travelus.Models.ViewModels;
using travelus.Utility;

namespace travelus.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class TourItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public TourItemViewModel TourItemVM { get; set; }

        public TourItemController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            TourItemVM = new TourItemViewModel()
            {
                Category = _db.Category,
                TourItem = new Models.TourItem()
            };
        }

        public async Task<IActionResult> Index()
        {
            var tourItems = await _db.TourItem.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(tourItems);
        }

        //GET FOR CREATE
        public IActionResult Create()
        {
            return View(TourItemVM);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            TourItemVM.TourItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if(!ModelState.IsValid)
            {
                return View(TourItemVM);
            }

            _db.TourItem.Add(TourItemVM.TourItem);
            await _db.SaveChangesAsync();

            //Image saving section

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var tourItemFromDb = await _db.TourItem.FindAsync(TourItemVM.TourItem.Id);

            if(files.Count>0)
            {
                //files have been uploaded successfully
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads,TourItemVM.TourItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                tourItemFromDb.Image = @"\images\" + TourItemVM.TourItem.Id + extension;
            }
            else
            {
                //No file has been uploaded., so use default instrad
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultTourImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + TourItemVM.TourItem.Id + ".png");
                tourItemFromDb.Image = @"\images\" + TourItemVM.TourItem.Id + ".png";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //GET FOR EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            TourItemVM.TourItem = await _db.TourItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(mbox => mbox.Id == id);
            TourItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == TourItemVM.TourItem.CategoryId).ToListAsync();

            if(TourItemVM.TourItem ==null)
            {
                return NotFound();
            }
            return View(TourItemVM);
        }

        [HttpPost,ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {

            if(id==null)
            {
                return NotFound();
            }
            TourItemVM.TourItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if(!ModelState.IsValid)
            {
                TourItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == TourItemVM.TourItem.CategoryId).ToListAsync();
                return View(TourItemVM);
            }

            //Image saving section

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var tourItemFromDb = await _db.TourItem.FindAsync(TourItemVM.TourItem.Id);

            if(files.Count>0)
            {
                //New image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original file
                var imagePath = Path.Combine(webRootPath, tourItemFromDb.Image.TrimStart('\\'));

                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //Upload the new file and save to db
                using (var filesStream = new FileStream(Path.Combine(uploads,TourItemVM.TourItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                tourItemFromDb.Image = @"\images\" + TourItemVM.TourItem.Id + extension_new;
            }

            tourItemFromDb.Name = TourItemVM.TourItem.Name;
            tourItemFromDb.Description = TourItemVM.TourItem.Description;
            tourItemFromDb.Price = TourItemVM.TourItem.Price;
            tourItemFromDb.Season = TourItemVM.TourItem.Season;
            tourItemFromDb.CategoryId = TourItemVM.TourItem.CategoryId;
            tourItemFromDb.SubCategoryId = TourItemVM.TourItem.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET FOR DETAILS - TOURITEM
        public async Task<IActionResult> Details (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TourItemVM.TourItem = await _db.TourItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(mbox => mbox.Id == id);

            if(TourItemVM.TourItem == null)
            {
                return NotFound();
            }

            return View(TourItemVM);
        }

        //GET FOR Delete TourItem
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TourItemVM.TourItem = await _db.TourItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (TourItemVM.TourItem == null)
            {
                return NotFound();
            }

            return View(TourItemVM);
        }

        //POST for DELETE TourItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            TourItem tourItem = await _db.TourItem.FindAsync(id);

            if (tourItem != null)
            {
                var imagePath = Path.Combine(webRootPath, tourItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.TourItem.Remove(tourItem);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }

    }
}
