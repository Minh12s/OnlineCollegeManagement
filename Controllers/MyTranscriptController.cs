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
using System.Net;
using System.IO;
using OnlineCollegeManagement.Models.Authentication;


namespace OnlineCollegeManagement.Controllers
{
    [Authentication]

    public class MyTranscriptController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MyTranscriptController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> MyTranscript()
        {
            return View();
        }
      
        public async Task<IActionResult> ChangePassword()
        {
           
            int usersId = Convert.ToInt32(HttpContext.Session.GetString("UsersId"));
            var user = _context.Users.FirstOrDefault(u => u.UsersId == usersId);

            return View();
        }
        [HttpPost]

        public async Task<IActionResult> ChangePassword(string current_password, string new_password, string new_password_confirmation)
        {
            if (new_password != new_password_confirmation)
            {
                TempData["Message"] = "Password confirmation does not match.";
                TempData["MessageColor"] = "alert-danger"; 
                return RedirectToAction("ChangePassword", "MyTranscript");
            }
            int usersId = Convert.ToInt32(HttpContext.Session.GetString("UsersId"));
            var user = _context.Users.FirstOrDefault(u => u.UsersId == usersId);
            if (user == null)
            {
                return NotFound();
            }

            if (!BCrypt.Net.BCrypt.Verify(current_password, user.Password))
            {
                TempData["Message"] = "Current password is incorrect.";
                TempData["MessageColor"] = "alert-danger"; // Màu đỏ
                return RedirectToAction("ChangePassword", "MyTranscript");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(new_password);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            TempData["Message"] = "Password changed successfully.";
            TempData["MessageColor"] = "alert-success"; // Màu xanh lá cây
            return RedirectToAction("ChangePassword", "MyTranscript");
        }
        public async Task<IActionResult> MyTimetable(int? page, int pageSize = 10)
        {
            // Lấy UsersId từ session
            var usersIdString = HttpContext.Session.GetString("UsersId");

            // Kiểm tra nếu session không có UsersId hoặc giá trị không hợp lệ
            if (string.IsNullOrEmpty(usersIdString) || !int.TryParse(usersIdString, out int usersId) || usersId == 0)
            {
                return RedirectToAction("Login", "Page");
            }

            // Lấy OfficialStudentId dựa trên UsersId
            var officialStudent = await _context.OfficialStudents
                .FirstOrDefaultAsync(os => os.UsersId == usersId);

            if (officialStudent == null)
            {
                return NotFound("Official student not found.");
            }

            // Lấy ClassesId từ bảng MergedStudentData dựa trên OfficialStudentId
            var mergedStudentData = await _context.MergedStudentData
                .FirstOrDefaultAsync(msd => msd.OfficialStudentId == officialStudent.OfficialStudentId);

            if (mergedStudentData == null || !mergedStudentData.ClassesId.HasValue)
            {
                return NotFound("Class not found for the official student.");
            }

            var classesId = mergedStudentData.ClassesId.Value;

            // Tính toán số lượng lịch học và trang hiện tại
            int totalClassSchedules = await _context.ClassSchedules
                .Where(cs => cs.ClassesId == classesId)
                .CountAsync();
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu lịch học cho trang hiện tại
            var classSchedules = await _context.ClassSchedules
                .Where(cs => cs.ClassesId == classesId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalClassSchedules / pageSize);
            ViewBag.TotalClassSchedules = totalClassSchedules;
            ViewBag.PageSize = pageSize;

            return View(classSchedules);
        }

    }
}
