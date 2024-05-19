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
using System.Net;
using System.IO;
using OnlineCollegeManagement.Heplers;
using OnlineCollegeManagement.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
    public class ScheduleController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ScheduleController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Schedule(int? page, string className = null, DateTime? StartDate = null, DateTime? EndDate = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            // Lấy dữ liệu từ bảng Classes
            var classes = _context.Classes.AsQueryable();

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(className))
            {
                classes = classes.Where(c => c.ClassName.Contains(className));
            }

            if (StartDate != null && EndDate != null)
            {
                classes = classes.Where(c => c.ClassStartDate >= StartDate && c.ClassEndDate <= EndDate);
            }


            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedClasses = await classes.OrderByDescending(c => c.ClassEndDate)
                                                .Skip((pageNumber - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalClasses = await classes.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalClasses / pageSize);
            ViewBag.TotalClasses = totalClasses;
            ViewBag.PageSize = pageSize;

            return View(paginatedClasses);
        }
        public async Task<IActionResult> ViewSchedule(int classesId)
        {
            // Lưu classesId vào ViewBag để sử dụng trong view
            ViewBag.ClassesId = classesId;
            return View();
        }
        public async Task<IActionResult> AddSchedule(int classesId)
        {
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = new SelectList(courses, "CoursesId", "CourseName"); 
            ViewBag.ClassesId = classesId;
            return View();
        }

    }
}

