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
using OnlineCollegeManagement.Heplers;
using OnlineCollegeManagement.Models.Authentication;

namespace OnlineCollegeManagement.Controllers
{

    public class AdminController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AdminController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        [Authentication]
        public IActionResult Dashboard(int page = 1)
        {
            // Courses
            var recentCourses = _context.Courses
               .OrderByDescending(c => c.CoursesId)
               .Take(10)
               .ToList();
            // Classes
            var recentClasses = _context.Classes
               .OrderByDescending(c => c.StartDate)
               .Take(10)
               .ToList();
            // pending Registrations
            int pageSize = 5;

            var pendingRegistrations = _context.Registrations
                .Include(r => r.StudentInformation)
                .Where(r => r.RegistrationStatus == "Pending")
                .OrderByDescending(r => r.RegistrationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalClasses = _context.Classes.Count();
            int totalCourses = _context.Courses.Count();
            int totalStudents = _context.OfficialStudents.Count();
            int totalTeachers = _context.Teachers.Count();
            int totalEvents = _context.Events.Count();
            int totalMajors = _context.Majors.Count();

            int totalPendingRegistrations = _context.Registrations
                .Count(r => r.RegistrationStatus == "Pending");
            
           
            ViewBag.RecentCourses = recentCourses;
            ViewBag.RecentClasses = recentClasses;
            ViewBag.PendingRegistrations = pendingRegistrations;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalMajors = totalMajors;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalPendingRegistrations / (double)pageSize);

            return View();
        }






        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return View();
            }
            else
            {
                string userRole = HttpContext.Session.GetString("Role");
                if (userRole == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (userRole == "User")
                {
                    // Nếu là Admin, hiển thị thông báo lỗi "Email or password is incorrect"
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                    return View();
                }
            }
            // Trường hợp còn lại, chuyển hướng đến trang Home của User
            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        public IActionResult Login(Users user)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                Users u = _context.Users.FirstOrDefault(x => x.Email.Equals(user.Email));

                if (u != null && BCrypt.Net.BCrypt.Verify(user.Password, u.Password))
                {
                    if (u.Role == "User")
                    {
                        // Trường hợp đăng nhập thành công nhưng là Admin, hiển thị thông báo lỗi "Email or password is incorrect"
                        ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                        return View();
                    }
                    else if (u.Role == "Admin")
                    {
                        // Chỉ đặt Session khi thông tin đăng nhập hợp lệ
                        HttpContext.Session.SetString("Username", u.Username.ToString());
                        HttpContext.Session.SetString("Role", u.Role);
                        HttpContext.Session.SetString("UserId", u.UsersId.ToString());

                        return RedirectToAction("Dashboard", "Admin"); // Chuyển hướng đến "Home", "Page" nếu là User
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                    return View();
                }
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Login", "Admin");
        }

    }
}
