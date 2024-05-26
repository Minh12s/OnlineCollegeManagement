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
    public class ClassesController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ClassesController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }

        public async Task<IActionResult> Classes(int? page, string className = null, DateTime? StartDate = null, DateTime? EndDate = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
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

        // GET: Departments/Create
        public IActionResult AddClasses()
        {
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClasses([Bind("ClassesId,ClassName,ClassStartDate,ClassEndDate")] Classes classes)
        {
            // Kiểm tra xem tên lớp đã tồn tại hay chưa
            if (ClassesExists(classes.ClassName))
            {
                ModelState.AddModelError("ClassName", "Class name already exists.");
                return View(classes);
            }

            // Kiểm tra xem dữ liệu đầu vào có hợp lệ không (ví dụ: các trường bắt buộc)
            if (!string.IsNullOrEmpty(classes.ClassName))
            {
                // Chuyển đổi ClassName sang chữ thường để so sánh
                string classNameLower = classes.ClassName.ToLower();

                // Kiểm tra xem có bất kỳ lớp nào có trùng tên (không phân biệt chữ hoa/chữ thường) không
                if (_context.Classes.Any(c => c.ClassName.ToLower() == classNameLower))
                {
                    ModelState.AddModelError("ClassName", "Class name already exists.");
                    return View(classes);
                }

                // Thêm lớp vào cơ sở dữ liệu
                _context.Add(classes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Classes));
            }
            else
            {
                // Nếu dữ liệu không hợp lệ, hiển thị thông báo lỗi và trả về view để người dùng nhập lại
                ModelState.AddModelError(string.Empty, "Please provide a class name.");
                return View(classes);
            }
        }


        private bool ClassesExists(string className)
        {
            return _context.Classes.Any(d => d.ClassName == className);
        }
        public async Task<IActionResult> EditClasses(int id)
        {
            // Tìm phòng ban cần chỉnh sửa bằng ID
            var classes = await _context.Classes.FindAsync(id);

            // Nếu không tìm thấy phòng ban, trả về NotFound
            if (classes == null)
            {
                return NotFound();
            }

            // Trả về view với dữ liệu phòng ban để hiển thị trên form chỉnh sửa
            return View(classes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClasses(int id, [Bind("ClassesId,ClassName,ClassStartDate,ClassEndDate")] Classes classes)
        {
            // Kiểm tra xem ID của phòng ban được chỉnh sửa có khớp với ID được truyền vào không
            if (id != classes.ClassesId)
            {
                return NotFound();
            }

            // Kiểm tra xem dữ liệu đầu vào có hợp lệ không (ví dụ: các trường bắt buộc)
            if (!string.IsNullOrEmpty(classes.ClassName))
            {
                // Chuyển đổi DepartmentName sang chữ thường để so sánh
                string classesNameLower = classes.ClassName.ToLower();

                // Kiểm tra xem có bất kỳ bộ phận nào có trùng tên (không phân biệt chữ hoa/chữ thường) ngoại trừ phòng ban hiện tại không
                if (_context.Classes.Any(d => d.ClassName.ToLower() == classesNameLower && d.ClassesId != id))
                {
                    ModelState.AddModelError("Classesname", "Classes name already exists.");
                    return View(classes);
                }

                try
                {
                    // Cập nhật thông tin của phòng ban trong cơ sở dữ liệu
                    _context.Update(classes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý lỗi khi cơ sở dữ liệu không thể cập nhật do xung đột
                    if (!ClassesExists(classes.ClassesId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Chuyển hướng đến action hiển thị danh sách phòng ban sau khi chỉnh sửa thành công
                return RedirectToAction(nameof(Classes));
            }
            // Nếu dữ liệu không hợp lệ, hiển thị thông báo lỗi và trả về view để người dùng nhập lại
            ModelState.AddModelError(string.Empty, "Please provide a department name.");
            return View(classes);
        }

        private bool ClassesExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentsId == id);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteClasses(int id)
        {
            // Tìm lớp cần xóa bằng ID
            var classes = await _context.Classes.FindAsync(id);
            if (classes == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu lớp đã kết thúc
            if (classes.ClassEndDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Cannot delete this class because it has already ended.";
                return RedirectToAction("Classes");
            }

            // Kiểm tra nếu có học sinh nào trong lớp có DeleteStatus là 0
            var studentInClass = await _context.StudentClasses
                .AnyAsync(sc => sc.ClassesId == id && sc.DeleteStatus == 0);
            if (studentInClass)
            {
                // Nếu có học sinh, trả về thông báo lỗi
                TempData["ErrorMessage"] = "Cannot delete this class because there are active students in it.";
                return RedirectToAction("Classes");
            }

            try
            {
                // Xóa tất cả các bản ghi StudentClasses có ClassesId tương ứng
                var studentClasses = _context.StudentClasses.Where(sc => sc.ClassesId == id);
                _context.StudentClasses.RemoveRange(studentClasses);

                // Xóa lớp khỏi cơ sở dữ liệu
                _context.Classes.Remove(classes);

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Chuyển hướng đến action "Classes" sau khi xóa thành công
                return RedirectToAction(nameof(Classes));
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có bất kỳ lỗi nào xảy ra
                // Trong trường hợp này, bạn có thể muốn ghi log, hiển thị thông báo lỗi, vv.
                return StatusCode(500); // Hoặc trả về một view với thông báo lỗi
            }
        }



        public async Task<IActionResult> ViewStudents(int classesId)
        {
            // Lưu classesId vào ViewBag để sử dụng trong view
            ViewBag.ClassesId = classesId;

            // Lấy danh sách sinh viên dựa trên classesId từ bảng StudentClasses
            var studentCoursesWithClasses = await _context.StudentCourses
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.User) // Bao gồm thông tin từ bảng User
                .Join(
                    _context.StudentClasses,

                    studentCourse => studentCourse.StudentCoursesId,
                    studentClass => studentClass.StudentCoursesId,
                    (studentCourse, studentClass) => new StudentCourseClassViewModel
                    {
                        StudentCourse = studentCourse,
                        StudentClass = studentClass
                    })
                .Where(sc => sc.StudentClass.ClassesId == classesId && sc.StudentClass.DeleteStatus == 0)
                .ToListAsync();

            return View(studentCoursesWithClasses);
        }


        [HttpGet]
        public async Task<IActionResult> AddStudentToClass(int classesId, int page = 1, int pageSize = 10)
        {
            // Lấy thông tin lớp học từ ID
            var classes = await _context.Classes.FindAsync(classesId);

            if (classes == null)
            {
                return NotFound();
            }

            // Retrieve all students from the StudentCourses table
            var students = await _context.StudentCourses
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(sc => sc.Course)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();




            // Tính toán số trang
            int totalStudents = await _context.OfficialStudents.CountAsync();
            int totalPages = (int)Math.Ceiling(totalStudents / (double)pageSize);

            // Truy vấn lại thông tin lớp học và gán cho ViewBag

            ViewBag.ClassesId = classesId;
            ViewBag.ClassesStartDate = classes.ClassStartDate;
            ViewBag.ClassesEndDate = classes.ClassEndDate;
            // Pass paging info through ViewBag
            ViewBag.PageNumber = page;
            ViewBag.TotalPages = totalPages;

            return View(students);
        }
        [HttpPost]
        public async Task<IActionResult> AddStudentToClass(int classesId, List<string> selectedStudents, int page = 1, int pageSize = 10)
        {
            var classes = await _context.Classes.FindAsync(classesId);

            if (classes == null)
            {
                return NotFound();
            }

            ViewBag.ClassesId = classesId;
            ViewBag.ClassesStartDate = classes.ClassStartDate;
            ViewBag.ClassesEndDate = classes.ClassEndDate;

            // Kiểm tra xem lớp học đã kết thúc hay chưa
            if (classes.ClassEndDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "The class has ended, students cannot be added to the class.";
                return RedirectToAction(nameof(AddStudentToClass), new { classesId = classesId });
            }

            if (selectedStudents != null && selectedStudents.Any())
            {
                List<string> errors = new List<string>();

                foreach (var studentCoursesId in selectedStudents)
                {
                    if (!int.TryParse(studentCoursesId, out int courseId))
                    {
                        // Handle invalid studentCoursesId here
                        ModelState.AddModelError("", $"Invalid studentCoursesId: {studentCoursesId}");
                        continue;
                    }

                    // Lấy thông tin StudentCourses dựa trên courseId
                    var studentCourse = await _context.StudentCourses.FindAsync(courseId);
                    if (studentCourse == null)
                    {
                        ModelState.AddModelError("", $"StudentCourse with ID {courseId} not found.");
                        continue;
                    }

                    // Kiểm tra xem học sinh đã đăng ký một khóa học khác trong cùng lớp hay chưa
                    var existingRecord = await _context.StudentClasses
                        .Include(sc => sc.StudentCourses)
                        .ThenInclude(sc => sc.Course) // Bổ sung để lấy thông tin về khóa học
                        .FirstOrDefaultAsync(sc => sc.StudentCourses.OfficialStudentId == studentCourse.OfficialStudentId && sc.ClassesId == classesId);

                    if (existingRecord != null && existingRecord.StudentCourses != null && existingRecord.StudentCourses.Course != null)
                    {
                        errors.Add($"Student with ID {studentCourse.OfficialStudentId} is already enrolled in class {classes.ClassName} with course {existingRecord.StudentCourses.Course.CourseName}.");
                    }
                    else if (existingRecord != null && existingRecord.Classes != null)
                    {
                        errors.Add($"Student with ID {studentCourse.OfficialStudentId} is already enrolled in class {classes.ClassName}.");
                    }
                    else
                    {
                        var studentClass = new StudentClasses
                        {
                            StudentCoursesId = courseId,
                            ClassesId = classesId,
                            ClassStartDate = classes.ClassStartDate,
                            ClassEndDate = classes.ClassEndDate,
                            StudentStatus = "Studying",
                            DeleteStatus = 0
                        };
                        _context.StudentClasses.Add(studentClass);
                    }
                }

                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(await GetStudentsPagedList(page, pageSize));
                }
                else
                {
                    await _context.SaveChangesAsync();
                    return Redirect("/Classes/ViewStudents?classesId=" + classesId);
                }
            }
            else
            {
                return View(await GetStudentsPagedList(page, pageSize));
            }
        }



        // Helper method to retrieve paginated student list
        private async Task<List<StudentCourses>> GetStudentsPagedList(int page, int pageSize)
        {
            return await _context.StudentCourses
                  .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(sc => sc.Course)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromClass(int coursesId, int classesId, int officialStudentId)
        {
            // Lấy thông tin lớp học
            var classInfo = await _context.Classes
                .FirstOrDefaultAsync(c => c.ClassesId == classesId);

            if (classInfo == null)
            {
                // Nếu không tìm thấy lớp học, trả về lỗi
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction(nameof(ViewStudents), new { classesId = classesId });
            }

            // Kiểm tra xem lớp học đã kết thúc hay chưa
            if (classInfo.ClassEndDate <= DateTime.Now)
            {
                // Nếu lớp học đã kết thúc, trả về thông báo lỗi
                TempData["ErrorMessage"] = "The class has ended, students cannot be removed from the class.";
                return RedirectToAction(nameof(ViewStudents), new { classesId = classesId });
            }

            // Kiểm tra xem tất cả các bản ghi của học sinh trong bảng ExamScores có Score là null không
            var examScores = await _context.ExamScores
                .Where(e => e.CoursesId == coursesId && e.OfficialStudentId == officialStudentId)
                .ToListAsync();

            if (examScores.All(e => e.Score == null))
            {
                // Nếu tất cả các bản ghi đều có Score là null, xoá học sinh khỏi bảng StudentClasses và ExamScores
                var studentClass = await _context.StudentClasses
                    .FirstOrDefaultAsync(sc => sc.StudentCourses.CoursesId == coursesId && sc.StudentCourses.OfficialStudentId == officialStudentId && sc.ClassesId == classesId);

                if (studentClass != null)
                {
                    _context.StudentClasses.Remove(studentClass);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Nếu có bản ghi nào không phải là null, cập nhật DeleteStatus thành 1
                var studentClass = await _context.StudentClasses
                    .FirstOrDefaultAsync(sc => sc.StudentCourses.CoursesId == coursesId && sc.StudentCourses.OfficialStudentId == officialStudentId && sc.ClassesId == classesId);

                if (studentClass != null)
                {
                    studentClass.DeleteStatus = 1;
                    await _context.SaveChangesAsync();
                }
            }

            // Chuyển hướng đến action hiển thị danh sách học sinh trong lớp học
            return RedirectToAction(nameof(ViewStudents), new { classesId = classesId });
        }




        public async Task<IActionResult> DetailsStudent(int? officialStudentId, int? courseId, int? classesId)
        {
            if (officialStudentId == null || courseId == null || classesId == null)
            {
                return NotFound();
            }

            var studentCourses = await _context.StudentCourses
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.User)
                .FirstOrDefaultAsync(sc => sc.OfficialStudentId == officialStudentId && sc.CoursesId == courseId);

            var studentClasses = await _context.StudentClasses
                .Include(sc => sc.Classes)
                .FirstOrDefaultAsync(sc => sc.ClassesId == classesId);

            if (studentCourses == null || studentClasses == null)
            {
                return NotFound();
            }

            var viewModel = new StudentCourseClassViewModel
            {
                StudentCourse = studentCourses,
                StudentClass = studentClasses
            };
            ViewBag.ClassesId = classesId;
            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int? officialStudentId, int? courseId, int? classesId, string studentStatus, string returnUrl)
        {
            if (officialStudentId == null || courseId == null || classesId == null)
            {
                return NotFound();
            }

            // Lấy thông tin sinh viên từ bảng StudentClasses
            var studentClass = await _context.StudentClasses
                .Include(sc => sc.StudentCourses) // Liên kết với bảng StudentCourses
                .FirstOrDefaultAsync(sc => sc.StudentCourses.OfficialStudentId == officialStudentId && sc.ClassesId == classesId);

            if (studentClass == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái cho sinh viên
            studentClass.StudentStatus = studentStatus;
            _context.Update(studentClass);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến action "ViewStudents" trong controller "Classes" với tham số "classesId"
            return RedirectToAction("ViewStudents", "Classes", new { classesId = classesId });
        }





    }
}
