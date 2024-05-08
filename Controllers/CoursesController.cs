using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    public class CoursesController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public CoursesController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Courses(int? page, string courseName = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int? majorsId = null, int? teachersId = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var courses = _context.Courses.AsQueryable();
            var majors = _context.Majors.AsQueryable();
            var teachers = _context.Teachers.AsQueryable();

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(courseName))
            {
                courses = courses.Where(c => c.CourseName.Contains(courseName));
            }
            if (startDate != null)
            {
                courses = courses.Where(c => c.CourseDate >= startDate);
            }
            if (endDate != null)
            {
                // Chú ý: Khi lọc theo ngày kết thúc, hãy thêm 1 ngày vào để bao gồm tất cả các bài đăng được đăng vào ngày kết thúc
                courses = courses.Where(c => c.CourseDate < endDate.Value.AddDays(1));
            }
            if (majorsId != null)
            {
                courses = courses.Where(c => c.MajorsId == majorsId);
            }
            if (teachersId != null)
            {
                courses = courses.Where(c => c.TeachersId == teachersId);
            }
            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(c => c.CourseName.Contains(search)
                                        || c.Description.Contains(search));
            }

            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedCourses = await courses.OrderByDescending(c => c.CourseDate)
                                                .Skip((pageNumber - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalCourses = await courses.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.majors = majors;
            ViewBag.teachers = teachers;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
            ViewBag.TotalCourses = totalCourses;
            ViewBag.PageSize = pageSize;

            return View(paginatedCourses);
        }

        public IActionResult AddCourses()
        {
            var majors = _context.Majors.OrderBy(c => c.MajorName).ToList();
            var teachers = _context.Teachers.OrderBy(c => c.TeacherName).ToList();
            var subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();

            if (subjects != null)
            {
                ViewBag.Subjects = subjects;
            }
            else
            {
                // Xử lý trường hợp khi không có dữ liệu môn học
                // Ví dụ: ViewBag.Subjects = new List<Subject>();
            }

            ViewBag.Majors = new SelectList(majors, "MajorsId", "MajorName");
            ViewBag.Teachers = new SelectList(teachers, "TeachersId", "TeacherName");

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourses(Courses model, IFormFile CoursesImageUrl, List<int> SelectedSubjects)
        {
            if (true)
            {
                if (CoursesImageUrl != null && CoursesImageUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Courses");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + CoursesImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await CoursesImageUrl.CopyToAsync(stream);
                    }
                    model.CoursesImageUrl = "/images/Courses/" + Path.GetFileName(imagePath);
                }

                model.CourseDate = DateTime.Now;

                _context.Add(model);
                await _context.SaveChangesAsync();

                // Lấy CoursesId vừa tạo
                int coursesId = model.CoursesId;

                // Thêm các môn học vào bảng CourseSubject
                if (SelectedSubjects != null && SelectedSubjects.Any())
                {
                    foreach (var subjectId in SelectedSubjects)
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO CourseSubject (CoursesId, SubjectsId) VALUES ({coursesId}, {subjectId})");
                    }
                }

                return RedirectToAction(nameof(Courses));
            }

            return View(model);
        }




        [HttpGet]
        public async Task<IActionResult> EditCourses(int id)
        {
            var courses = await _context.Courses.FindAsync(id);
            var majors = _context.Majors.OrderBy(c => c.MajorName).ToList();
            var teachers = _context.Teachers.OrderBy(c => c.TeacherName).ToList();
            ViewBag.Majors = new SelectList(majors, "MajorsId", "MajorName");
            ViewBag.Teachers = new SelectList(teachers, "TeachersId", "TeacherName");

            if (courses == null)
            {
                return NotFound();
            }

            return View(courses);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourses(Courses model, IFormFile CoursesImageUrl)
        {
            if (true)
            {
                if (CoursesImageUrl != null && CoursesImageUrl.Length > 0)
                {
                    // Nếu có ảnh đại diện mới được cung cấp, hãy cập nhật nó
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Courses");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + CoursesImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await CoursesImageUrl.CopyToAsync(stream);
                    }

                    model.CoursesImageUrl = "/images/Courses/" + Path.GetFileName(imagePath);
                }
                else
                {
                    // Nếu không có ảnh đại diện mới được cung cấp, giữ giá trị hiện tại
                    var existingCourses = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(b => b.CoursesId == model.CoursesId);
                    if (existingCourses != null)
                    {
                        model.CoursesImageUrl = existingCourses.CoursesImageUrl;
                    }
                }

                model.CourseDate = DateTime.Now;

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Courses));
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourses(int id)
        {
            var courses = await _context.Courses.FindAsync(id);

            if (courses == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
            if (!string.IsNullOrEmpty(courses.CoursesImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", courses.CoursesImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Courses.Remove(courses);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Courses", "Courses");

        }
    }
}

