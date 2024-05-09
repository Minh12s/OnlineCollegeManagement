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
using System.IO;
using OnlineCollegeManagement.Heplers;
using OnlineCollegeManagement.Models.Authentication;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
    public class AchievementsController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AchievementsController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        // GET: /<controller>/
        public async Task<IActionResult> Achievements(int? page, string AchievementTitle = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var achievements = _context.Achievements.AsQueryable();
            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(AchievementTitle))
            {
                achievements = achievements.Where(b => b.AchievementTitle.Contains(AchievementTitle));
            }
            if (startDate != null)
            {
                achievements = achievements.Where(b => b.AchievementDate >= startDate);
            }

            if (endDate != null)
            {
                // Chú ý: Khi lọc theo ngày kết thúc, hãy thêm 1 ngày vào để bao gồm tất cả các bài đăng được đăng vào ngày kết thúc
                achievements = achievements.Where(b => b.AchievementDate < endDate.Value.AddDays(1));
            }


            if (!string.IsNullOrEmpty(search))
            {
                achievements = achievements.Where(b => b.AchievementTitle.Contains(search)
                                        || b.AchievementDescription.Contains(search));

            }

            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedachievements = await achievements.OrderByDescending(b => b.AchievementDate)
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalachievements = await achievements.CountAsync();
            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalachievements / pageSize);
            ViewBag.Totalachievements = totalachievements;
            ViewBag.PageSize = pageSize;
            return View(paginatedachievements);
        }
        public IActionResult AddAchievements()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAchievements(Achievements model, IFormFile AchievementImageUrl)
        {
            if (true)
            {
                if (AchievementImageUrl != null && AchievementImageUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Achievements");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + AchievementImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await AchievementImageUrl.CopyToAsync(stream);
                    }
                    model.AchievementImageUrl = "/images/Achievements/" + Path.GetFileName(imagePath);

                }

                model.AchievementDate = DateTime.Now;


                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Achievements));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditAchievements(int id)
        {
            var achievements = await _context.Achievements.FindAsync(id);

            if (achievements == null)
            {
                return NotFound();
            }

            return View(achievements);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAchievements(Achievements model, IFormFile AchievementImageUrl)
        {
            if (true)
            {
                if (AchievementImageUrl != null && AchievementImageUrl.Length > 0)
                {
                    // Nếu có ảnh đại diện mới được cung cấp, hãy cập nhật nó
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Achievements");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + AchievementImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await AchievementImageUrl.CopyToAsync(stream);
                    }

                    model.AchievementImageUrl = "/images/Achievements/" + Path.GetFileName(imagePath);
                }
                else
                {
                    // Nếu không có ảnh đại diện mới được cung cấp, giữ giá trị hiện tại
                    var existingAchievements = await _context.Achievements.AsNoTracking().FirstOrDefaultAsync(b => b.AchievementsId == model.AchievementsId);
                    if (existingAchievements != null)
                    {
                        model.AchievementImageUrl = existingAchievements.AchievementImageUrl;
                    }
                }

                model.AchievementDate = DateTime.Now;

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Achievements));
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAchievements(int id)
        {
            var achievements = await _context.Achievements.FindAsync(id);

            if (achievements == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
            if (!string.IsNullOrEmpty(achievements.AchievementImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", achievements.AchievementImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Achievements.Remove(achievements);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Achievements", "Achievements");
        }

    }
}

