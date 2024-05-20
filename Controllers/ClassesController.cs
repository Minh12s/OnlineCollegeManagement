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
        public async Task<IActionResult> EditClasses(int id, [Bind("ClassesId,ClassName,StartDate,EndDate")] Classes classes)
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

            try
            {
                // Xóa tất cả các bản ghi OfficialStudentClasses có ClassesId tương ứng
                var studentClasses = _context.MergedStudentData.Where(osc => osc.ClassesId == id);
                _context.MergedStudentData.RemoveRange(studentClasses);

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

            var officialStudentClasses = await _context.MergedStudentData
                .Include(osc => osc.Classes)
                .Include(osc => osc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(osc => osc.OfficialStudent)
                    .ThenInclude(os => os.User) // Bao gồm thông tin từ bảng User
                .Where(osc => osc.Classes.ClassesId == classesId)
                .ToListAsync();

            return View(officialStudentClasses);
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

            // Retrieve all students from the MergedStudentData table
            var students = await _context.MergedStudentData
                .Include(msd => msd.OfficialStudent)
                .ThenInclude(os => os.StudentInformation)
                .Include(msd => msd.Course)
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

            if (selectedStudents != null && selectedStudents.Any())
            {
                List<string> errors = new List<string>();

                foreach (var student in selectedStudents)
                {
                    var parts = student.Split('-');
                    int officialStudentId = int.Parse(parts[0]);
                    int coursesId = int.Parse(parts[1]);

                    // Kiểm tra xem sinh viên đã đăng ký khoá học khác trong cùng một lớp hay không
                    var existingRecordWithSameStudentAndDifferentCourse = await _context.MergedStudentData
                        .FirstOrDefaultAsync(sc => sc.OfficialStudentId == officialStudentId
                                                    && sc.ClassesId == classesId
                                                    && sc.CoursesId != coursesId);

                    if (existingRecordWithSameStudentAndDifferentCourse != null)
                    {
                        errors.Add($"Student with ID {officialStudentId} is already enrolled in class {existingRecordWithSameStudentAndDifferentCourse.Classes.ClassName} for a different course.");
                    }
                    else
                    {
                        var existingRecord = await _context.MergedStudentData
                            .Include(sc => sc.Classes) // Include related class data
                            .FirstOrDefaultAsync(sc => sc.OfficialStudentId == officialStudentId && sc.CoursesId == coursesId);

                        if (existingRecord != null && existingRecord.ClassesId.HasValue)
                        {
                            errors.Add($"Student with ID {officialStudentId} is already enrolled in class {existingRecord.Classes.ClassName}.");
                        }
                        else if (existingRecord != null)
                        {
                            existingRecord.ClassesId = classesId;
                            existingRecord.ClassStartDate = classes.ClassStartDate;
                            existingRecord.ClassEndDate = classes.ClassEndDate;
                            existingRecord.StudentStatus = "Studying";
                            existingRecord.DeleteStatus = 0;
                            _context.MergedStudentData.Update(existingRecord);
                        }
                        else
                        {
                            var studentEntity = await _context.OfficialStudents.FindAsync(officialStudentId);
                            if (studentEntity != null)
                            {
                                var studentClass = new MergedStudentData
                                {
                                    OfficialStudentId = officialStudentId,
                                    CoursesId = coursesId,
                                    ClassesId = classesId,
                                    ClassStartDate = classes.ClassStartDate,
                                    ClassEndDate = classes.ClassEndDate,
                                    StudentStatus = "Studying",
                                    DeleteStatus = 0
                                };
                                _context.MergedStudentData.Add(studentClass);
                            }
                        }
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
        private async Task<List<MergedStudentData>> GetStudentsPagedList(int page, int pageSize)
        {
            return await _context.MergedStudentData
                .Include(msd => msd.OfficialStudent)
                .ThenInclude(os => os.StudentInformation)
                .Include(msd => msd.Course)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromClass(int classesId, int officialStudentId)
        {
            // Tìm OfficialStudentClasses dựa trên ClassesId và OfficialStudentId
            var officialStudentClass = await _context.MergedStudentData
                .FirstOrDefaultAsync(osc => osc.ClassesId == classesId && osc.OfficialStudentId == officialStudentId);

            if (officialStudentClass == null)
            {
                return NotFound(); // Trả về NotFound nếu không tìm thấy
            }

            // Thay đổi thuộc tính DeleteStatus thành 1 thay vì xóa khỏi cơ sở dữ liệu
            officialStudentClass.DeleteStatus = 1;

            // Lưu thay đổi
            await _context.SaveChangesAsync();

            // Chuyển hướng đến action hiển thị danh sách học sinh trong lớp học
            return RedirectToAction(nameof(ViewStudents), new { classesId = classesId });
        }

        public async Task<IActionResult> DetailsStudent(int? officialStudentId, int? courseId, int? classesId)
        {
            if (officialStudentId == null || courseId == null || classesId == null)
            {
                return NotFound();
            }

            // Lấy thông tin sinh viên từ bảng OfficialStudentClasses kèm theo thông tin liên quan
            var student = await _context.MergedStudentData
                .Include(os => os.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation) // Load thông tin sinh viên
                .Include(os => os.OfficialStudent)
                    .ThenInclude(os => os.User) // Load thông tin người dùng
                .FirstOrDefaultAsync(s => s.OfficialStudentId == officialStudentId && s.CoursesId == courseId && s.ClassesId == classesId);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int? officialStudentId, int? courseId, int? classesId, string studentStatus, string returnUrl)
        {
            if (officialStudentId == null || courseId == null || classesId == null)
            {
                return NotFound();
            }

            var officialStudentClasses = await _context.MergedStudentData.FirstOrDefaultAsync(s => s.OfficialStudentId == officialStudentId && s.CoursesId == courseId && s.ClassesId == classesId);

            if (officialStudentClasses == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái cho sinh viên
            officialStudentClasses.StudentStatus = studentStatus;
            _context.Update(officialStudentClasses);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến action "ViewStudents" trong controller "Classes" với tham số "classesId"
            return RedirectToAction("ViewStudents", "Classes", new { classesId = classesId });
        }


    }
}
