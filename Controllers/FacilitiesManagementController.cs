using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using BCrypt.Net;
using System.Text;
using System.Net.Mail;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    public class FacilitiesManagementController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public FacilitiesManagementController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        // GET: /<controller>/
        public async Task<IActionResult> Facilities(int? page, string FacilityTitle = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var facilities = _context.Facilities.AsQueryable();
            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(FacilityTitle))
            {
                facilities = facilities.Where(b => b.FacilityTitle.Contains(FacilityTitle));
            }
            if (startDate != null)
            {
                facilities = facilities.Where(b => b.FacilityDate >= startDate);
            }

            if (endDate != null)
            {
                // Chú ý: Khi lọc theo ngày kết thúc, hãy thêm 1 ngày vào để bao gồm tất cả các bài đăng được đăng vào ngày kết thúc
                facilities = facilities.Where(b => b.FacilityDate < endDate.Value.AddDays(1));
            }


            if (!string.IsNullOrEmpty(search))
            {
                facilities = facilities.Where(b => b.FacilityTitle.Contains(search)
                                        || b.FacilityDescription.Contains(search));
                                        
            }

            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedfacilities = await facilities.OrderByDescending(b => b.FacilityDate)
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalfacilities = await facilities.CountAsync();
            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalfacilities / pageSize);
            ViewBag.Totalfacilities = totalfacilities;
            ViewBag.PageSize = pageSize;
            return View(paginatedfacilities);
        }

        public IActionResult AddFacilities()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFacilities(Facilities model, IFormFile FacilityImageUrl)
        {
            if (true)
            {
                if (FacilityImageUrl != null && FacilityImageUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Facilities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + FacilityImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await FacilityImageUrl.CopyToAsync(stream);
                    }
                    model.FacilityImageUrl = "/images/Facilities/" + Path.GetFileName(imagePath);

                }

                model.FacilityDate = DateTime.Now;


                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Facilities));
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditFacilities(int id)
        {
            var facilities = await _context.Facilities.FindAsync(id);

            if (facilities == null)
            {
                return NotFound();
            }

            return View(facilities);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFacilities(Facilities model, IFormFile FacilityImageUrl)
        {
            if (true)
            {
                if (FacilityImageUrl != null && FacilityImageUrl.Length > 0)
                {
                    // Nếu có ảnh đại diện mới được cung cấp, hãy cập nhật nó
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Facilities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + FacilityImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await FacilityImageUrl.CopyToAsync(stream);
                    }

                    model.FacilityImageUrl = "/images/Facilities/" + Path.GetFileName(imagePath);
                }
                else
                {
                    // Nếu không có ảnh đại diện mới được cung cấp, giữ giá trị hiện tại
                    var existingFacilities = await _context.Facilities.AsNoTracking().FirstOrDefaultAsync(b => b.FacilitiesId == model.FacilitiesId);
                    if (existingFacilities != null)
                    {
                        model.FacilityImageUrl = existingFacilities.FacilityImageUrl;
                    }
                }

                model.FacilityDate = DateTime.Now;

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Facilities));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFacilities(int id)
        {
            var facilities = await _context.Facilities.FindAsync(id);

            if (facilities == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
            if (!string.IsNullOrEmpty(facilities.FacilityImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", facilities.FacilityImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Facilities.Remove(facilities);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Facilities", "FacilitiesManagement");
        }

    }

}

