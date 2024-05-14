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
        private readonly CollegeManagementContext db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MyTranscriptController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            db = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> MyTranscript()
        {
            return View();
        }
      
        public async Task<IActionResult> ChangePassword()
        {
            // Lấy AccountBalance từ session
            int usersId = Convert.ToInt32(HttpContext.Session.GetString("UsersId"));
            var user = db.Users.FirstOrDefault(u => u.UsersId == usersId);

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
            int usersId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var user = db.Users.FirstOrDefault(u => u.UsersId == usersId);
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
            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();
            TempData["Message"] = "Password changed successfully.";
            TempData["MessageColor"] = "alert-success"; // Màu xanh lá cây
            return RedirectToAction("ChangePassword", "MyTranscript");
        }
        public async Task<IActionResult> MyTimetable()
        {
            return View();
        }
    }
}
