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
        public async Task<IActionResult> ViewClassesStudent()
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

            var officialStudentId = officialStudent.OfficialStudentId;

            // Lấy StudentCoursesId từ bảng StudentCourses dựa trên OfficialStudentId
            var studentCoursesList = await _context.StudentCourses
                .Where(sc => sc.OfficialStudentId == officialStudentId)
                .ToListAsync();

            if (studentCoursesList == null || studentCoursesList.Count == 0)
            {
                return View();
            }

            var studentCoursesIds = studentCoursesList.Select(sc => sc.StudentCoursesId).ToList();

            // Lấy ClassesId từ bảng StudentClasses dựa trên danh sách StudentCoursesId
            var studentClassesList = await _context.StudentClasses
                .Where(sc => studentCoursesIds.Contains(sc.StudentCoursesId))
                .ToListAsync();

            if (studentClassesList == null || studentClassesList.Count == 0)
            {
                return View();
            }

            // Tạo một danh sách chứa thông tin lớp học (bao gồm cả ClassId và ClassName)
            var classesInfoList = new List<(int ClassId, string ClassName)>();

            // Lặp qua danh sách StudentClasses để lấy thông tin lớp học từ bảng Classes
            foreach (var studentClass in studentClassesList)
            {
                var classInfo = await _context.Classes
                    .Where(c => c.ClassesId == studentClass.ClassesId)
                    .Select(c => new { c.ClassesId, c.ClassName })
                    .FirstOrDefaultAsync();

                if (classInfo != null)
                {
                    classesInfoList.Add((classInfo.ClassesId, classInfo.ClassName));
                }
            }

            // Truyền danh sách các lớp học vào ViewBag
            ViewBag.Classes = classesInfoList;

            return View();
        }


        public async Task<IActionResult> MyTimetable(int? classesId, int? page, int pageSize = 9)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Kiểm tra nếu classesId không được cung cấp
            if (!classesId.HasValue)
            {
                return BadRequest("Class ID is required.");
            }

            // Lấy dữ liệu lịch học từ bảng ClassSchedules dựa trên ClassesId
            var classSchedulesQuery = _context.ClassSchedules
                .Where(cs => cs.ClassesId == classesId.Value)
                .AsQueryable();

            var paginatedClassSchedules = await classSchedulesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy tổng số lịch học để phân trang
            int totalClassSchedules = await classSchedulesQuery.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalClassSchedules / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.ClassesId = classesId; // Add this line to pass classesId to the view

            return View(paginatedClassSchedules);
        }





    }
}
