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
using Azure;

namespace OnlineCollegeManagement.Controllers
{

    public class PageController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public PageController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Home()
        {
            return View();
        }
        public async Task<IActionResult> About()
        {
            return View();
        }
        public async Task<IActionResult> Facilities(int? page)
        {
            // Số lượng cơ sở vật lý trên mỗi trang
            int pageSize = 3;

            // Trang hiện tại (mặc định là trang 1 nếu không có giá trị được cung cấp)
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu từ bảng Facilities và sắp xếp theo ngày mới nhất
            var facilities = await _context.Facilities
                .OrderByDescending(f => f.FacilityDate) // Sắp xếp giảm dần theo ngày
                .Skip((pageNumber - 1) * pageSize)     // Bỏ qua các cơ sở vật lý trên các trang trước
                .Take(pageSize)                        // Lấy số lượng cơ sở vật lý trên trang hiện tại
                .ToListAsync();

            // Tổng số cơ sở vật lý
            int totalFacilities = await _context.Facilities.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling((double)totalFacilities / pageSize);

            // Truyền các thông tin về phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            // Trả về view và truyền dữ liệu facilities vào view
            return View(facilities);
        }
        public async Task<IActionResult> Achievements()
        {
            return View();
        }
        public async Task<IActionResult> Courses()
        {
            return View();
        }
        public async Task<IActionResult> Teacher()
        {
            return View();
        }
        public async Task<IActionResult> TeacherDetails()
        {
            return View();
        }
        public async Task<IActionResult> Blog()
        {
            return View();
        }
        public async Task<IActionResult> BlogDetails()
        {
            return View();
        }
        public async Task<IActionResult> Events(int? page, int pageSize=9)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Truy vấn dữ liệu sự kiện từ cơ sở dữ liệu
            var eventsQuery = _context.Events.AsQueryable();
            var paginatedEvent = await eventsQuery
                                          .Skip((pageNumber - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalEvent = await eventsQuery.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalEvent / pageSize);
            ViewBag.TotalEvent = totalEvent;
            ViewBag.PageSize = pageSize;
            // Truyền danh sách sự kiện vào view để hiển thị
            return View(paginatedEvent);
        }

        public async Task<IActionResult> Contact()
        {
            return View();
        }
        public async Task<IActionResult> Admission()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SubmitAdmissionForm(StudentsInformation studentInfo)
        {
            if (true)
            {
                // Lưu thông tin sinh viên vào cơ sở dữ liệu
                _context.StudentsInformation.Add(studentInfo);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công!";

                // Chuyển hướng trở lại trang Admission
                return RedirectToAction("Admission", "Page");
            }

            // Nếu dữ liệu không hợp lệ, quay lại trang Admission để người dùng nhập lại
            return View("Admission", studentInfo);
        }


    }
}
