using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using OnlineCollegeManagement.Heplers;
using OnlineCollegeManagement.Models.Authentication;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
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
                                        || c.Description.Contains(search)
                                        || c.CourseTime.Contains(search));
            }

            var paginatedCourses = await courses
      .OrderByDescending(c => c.CoursesId)
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

        public async Task<IActionResult> AddCourses(int? page, int pageSize = 5)
        {
            int pageNumber = page ?? 1;

            // Sắp xếp danh sách môn học theo tên môn học
            var subjects = _context.Subjects.OrderBy(s => s.SubjectName);

            // Tính toán số lượng trang và số lượng môn học
            int totalSubjects = await subjects.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalSubjects / pageSize);

            // Kiểm tra nếu pageNumber vượt quá số trang tối đa hoặc nhỏ hơn 1
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            // Lấy danh sách các môn học trên trang hiện tại
            var paginatedSubjects = await subjects
                                              .Skip((pageNumber - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();

            var majors = await _context.Majors.OrderBy(c => c.MajorName).ToListAsync();
            var teachers = await _context.Teachers.OrderBy(c => c.TeacherName).ToListAsync();



            // Gán dữ liệu cho ViewBag
            ViewBag.Subjects = paginatedSubjects;
            ViewBag.Majors = new SelectList(majors, "MajorsId", "MajorName");
            ViewBag.Teachers = new SelectList(teachers, "TeachersId", "TeacherName");
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalSubjects = totalSubjects;
            ViewBag.PageSize = pageSize;



            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourses(Courses model, IFormFile CoursesImageUrl)
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



                _context.Add(model);
                await _context.SaveChangesAsync();

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

        public IActionResult CoursesDetail(int courseId)
        {
            // Truy vấn dữ liệu từ bảng CoursesSubjects chỉ lấy những dòng có CoursesId trùng với courseId được truyền vào,
            // và nạp dữ liệu từ bảng Courses và Subjects tương ứng
            var coursesSubjects = _context.CoursesSubjects
                                      .Include(cs => cs.Course)
                                      .Include(cs => cs.Subject)
                                      .Where(cs => cs.CoursesId == courseId)
                                      .ToList();
            ViewBag.CourseId = courseId;
            // Trả về view và truyền dữ liệu đã lọc qua view
            return View(coursesSubjects);
        }

        public async Task<IActionResult> AddSubject(int courseId, int? page, int pageSize = 5)
        {
            int pageNumber = page ?? 1;

            // Sắp xếp danh sách môn học theo tên môn học
            var subjects = _context.Subjects.OrderBy(s => s.SubjectName);

            // Tính toán số lượng trang và số lượng môn học
            int totalSubjects = await subjects.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalSubjects / pageSize);

            // Kiểm tra nếu pageNumber vượt quá số trang tối đa hoặc nhỏ hơn 1
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            // Lấy danh sách các môn học trên trang hiện tại
            var paginatedSubjects = await subjects
                                              .Skip((pageNumber - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();
            // Lấy tên khóa học từ cơ sở dữ liệu dựa trên courseId
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                // Xử lý khi không tìm thấy khóa học
                return NotFound();
            }
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.CourseName;

            // Lấy danh sách các môn học từ cơ sở dữ liệu
            ViewBag.Subjects = paginatedSubjects;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalSubjects = totalSubjects;
            ViewBag.PageSize = pageSize;

            // Trả về view và truyền danh sách môn học và tên khóa học đã lấy từ cơ sở dữ liệu
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSubject(int courseId, List<int> selectedSubjects)
        {
            if (selectedSubjects != null && selectedSubjects.Any())
            {
                foreach (var subjectId in selectedSubjects)
                {
                    // Kiểm tra xem môn học đã tồn tại trong khóa học chưa
                    if (!_context.CoursesSubjects.Any(cs => cs.CoursesId == courseId && cs.SubjectsId == subjectId))
                    {
                        var coursesSubjectsToAdd = new CoursesSubjects
                        {
                            CoursesId = courseId,
                            SubjectsId = subjectId
                        };

                        _context.CoursesSubjects.Add(coursesSubjectsToAdd);
                    }
                    else
                    {
                        // Nếu môn đã tồn tại trong khóa học, hiển thị thông báo lỗi
                        var duplicateSubject = await _context.Subjects.FindAsync(subjectId);
                        var subjects = _context.Subjects.OrderBy(s => s.SubjectName);

                        var duplicateSubjectsIds = await _context.CoursesSubjects
                            .Where(cs => cs.CoursesId == courseId)
                            .Select(cs => cs.SubjectsId)
                            .ToListAsync();

                        var duplicateSubjectCodes = subjects
                            .Where(s => duplicateSubjectsIds.Contains(s.SubjectsId))
                            .Select(s => s.SubjectCode);


                        ViewBag.ErrorMessage = $"Subjects : {string.Join(", ", duplicateSubjectCodes)} already exist in the course";

                        ViewBag.CourseId = courseId; // Giữ lại courseId
                        var course = await _context.Courses.FindAsync(courseId);
                        @ViewBag.CourseName = course.CourseName;

                        // Cập nhật lại thông tin phân trang

                        int pageNumber = 1; // Trang đầu tiên
                        int pageSize = 5; // Số lượng mục trên mỗi trang
                        var totalSubjects = await _context.Subjects.CountAsync();
                        var totalPages = (int)Math.Ceiling((double)totalSubjects / pageSize);

                        var paginatedSubjects = await subjects
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();


                        ViewBag.CurrentPage = pageNumber;
                        ViewBag.TotalPages = totalPages;
                        ViewBag.PageSize = pageSize;

                        // Trả về cùng một view với danh sách môn học phân trang
                        ViewBag.subjects = paginatedSubjects;
                        return View();
                    }


                }

                await _context.SaveChangesAsync();

                return RedirectToAction("CoursesDetail", "Courses", new { courseId = courseId });
            }
            else
            {
                // Nếu không có môn nào được chọn
                var subjects = _context.Subjects.OrderBy(s => s.SubjectName);
                ViewBag.ErrorMessages = "Please select at least one subject.";

                ViewBag.CourseId = courseId; // Giữ lại courseId
                var course = await _context.Courses.FindAsync(courseId);
                @ViewBag.CourseName = course.CourseName;



                int pageNumber = 1; // Trang đầu tiên
                int pageSize = 5; // Số lượng mục trên mỗi trang
                var totalSubjects = await _context.Subjects.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalSubjects / pageSize);

                var paginatedSubjects = await subjects
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();


                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;

                // Trả về cùng một view với danh sách môn học phân trang
                ViewBag.subjects = paginatedSubjects;
                return View();
            }
        }






        [HttpPost]
        public async Task<IActionResult> DeleteSubject(int courseId, int subjectId)
        {
            var courseSubjectToDelete = await _context.CoursesSubjects.FindAsync(courseId, subjectId);
            if (courseSubjectToDelete == null)
            {
                return NotFound();
            }

            _context.CoursesSubjects.Remove(courseSubjectToDelete);
            await _context.SaveChangesAsync();

            return RedirectToAction("CoursesDetail", "Courses", new { courseId = courseId });
        }

    }
}

