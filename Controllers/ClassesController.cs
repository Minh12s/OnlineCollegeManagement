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
                classes = classes.Where(c => c.StartDate >= StartDate && c.EndDate <= EndDate);
            }


            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedClasses = await classes.OrderByDescending(c => c.EndDate)
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
        public async Task<IActionResult> AddClasses([Bind("ClassesId,ClassName,StartDate,EndDate")] Classes classes)
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
                var studentClasses = _context.OfficialStudentClasses.Where(osc => osc.ClassesId == id);
                _context.OfficialStudentClasses.RemoveRange(studentClasses);

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

            var officialStudentClasses = await _context.OfficialStudentClasses
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
        public async Task<IActionResult> AddStudentToClass(int classesId, int page = 1, int pageSize = 1)
        {
            // Lấy thông tin lớp học từ ID
            var classes = await _context.Classes.FindAsync(classesId);

            if (classes == null)
            {
                return NotFound();
            }

            // Lấy danh sách sinh viên từ cơ sở dữ liệu, phân trang
            var students = await _context.OfficialStudents
                                        .Include(s => s.Course)
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            // Tính toán số trang
            int totalStudents = await _context.OfficialStudents.CountAsync();
            int totalPages = (int)Math.Ceiling(totalStudents / (double)pageSize);

            // Truy vấn lại thông tin lớp học và gán cho ViewBag
            ViewBag.ClassesId = classesId;
            ViewBag.ClassesStartDate = classes.StartDate;
            ViewBag.ClassesEndDate = classes.EndDate;

            // Pass paging info through ViewBag
            ViewBag.PageNumber = page;
            ViewBag.TotalPages = totalPages;

            return View(students);
        }


        [HttpPost]
        public async Task<IActionResult> AddStudentToClass(int classesId, List<int> selectedStudents, int page = 1, int pageSize = 1)
        {
            // Lấy thông tin lớp học từ ID
            var classes = await _context.Classes.FindAsync(classesId);

            if (classes == null)
            {
                return NotFound();
            }

            // Truy vấn lại thông tin lớp học và gán cho ViewBag
            ViewBag.ClassesId = classesId;
            ViewBag.ClassesStartDate = classes.StartDate;
            ViewBag.ClassesEndDate = classes.EndDate;

            if (selectedStudents != null && selectedStudents.Any())
            {
                // Biến để lưu trữ danh sách các sinh viên trùng lặp
                List<int> duplicateStudentIds = new List<int>();

                // Kiểm tra xem sinh viên đã tồn tại trong lớp học chưa
                foreach (var studentId in selectedStudents)
                {
                    var existingRecord = await _context.OfficialStudentClasses
                        .FirstOrDefaultAsync(sc => sc.OfficialStudentId == studentId && sc.ClassesId == classesId);

                    if (existingRecord != null)
                    {
                        // Nếu sinh viên đã tồn tại trong lớp học, thêm vào danh sách sinh viên trùng lặp
                        duplicateStudentIds.Add(studentId);
                    }
                    else
                    {
                        // Nếu sinh viên không tồn tại, thêm vào lớp học
                        var student = await _context.OfficialStudents.FindAsync(studentId);
                        if (student != null)
                        {
                            // Tạo một đối tượng OfficialStudentClasses và thêm vào bảng
                            var studentClass = new OfficialStudentClasses
                            {
                                StartDate = classes.StartDate,
                                EndDate = classes.EndDate,
                                StudentStatus = "Studying",
                                OfficialStudentId = studentId,
                                ClassesId = classesId
                            };
                            _context.OfficialStudentClasses.Add(studentClass);
                        }
                    }
                }

                if (duplicateStudentIds.Any())
                {
                    // Nếu có sinh viên trùng lặp, thêm thông báo lỗi vào ModelState
                    ModelState.AddModelError("", $"Students with ID {string.Join(", ", duplicateStudentIds)} already exist in this class.");

                    // Lấy lại dữ liệu sinh viên với phân trang và hiển thị lại trang
                    var students = await _context.OfficialStudents
                                                .Include(s => s.Course)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();
                    ViewBag.PageNumber = page;
                    ViewBag.TotalPages = (int)Math.Ceiling(await _context.OfficialStudents.CountAsync() / (double)pageSize);
                    return View(students);
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Thay đổi đường dẫn trả về
                return Redirect("/Classes/ViewStudents?classesId=" + classesId);
            }
            else
            {
                // Trả về view với thông tin lớp học nếu không có sinh viên được chọn
                return View(classes);
            }
        }


        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromClass(int classesId, int officialStudentId)
        {
            // Tìm OfficialStudentClasses dựa trên ClassesId và OfficialStudentId
            var officialStudentClass = await _context.OfficialStudentClasses
                .FirstOrDefaultAsync(osc => osc.ClassesId == classesId && osc.OfficialStudentId == officialStudentId);

            if (officialStudentClass == null)
            {
                return NotFound(); // Trả về NotFound nếu không tìm thấy
            }

            // Xóa OfficialStudentClasses khỏi cơ sở dữ liệu và lưu thay đổi
            _context.OfficialStudentClasses.Remove(officialStudentClass);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến action hiển thị danh sách học sinh trong lớp học
            return RedirectToAction(nameof(ViewStudents), new { classesId = classesId });
        }


       public async Task<IActionResult> DetailsStudent(int? Id, int? classesId)
        {
            var classes = await _context.Classes.FindAsync(classesId);
            ViewBag.ClassesId = classesId;

            if (Id == null)
            {
                return NotFound();
            }

            // Lấy thông tin sinh viên từ bảng OfficialStudentClasses kèm theo thông tin liên quan
            var student = await _context.OfficialStudentClasses
                .Include(os => os.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation) // Load thông tin sinh viên
                .Include(os => os.OfficialStudent)
                    .ThenInclude(os => os.User) // Load thông tin người dùng
                .FirstOrDefaultAsync(s => s.Id == Id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string studentStatus, string returnUrl)
        {
            var officialStudentClasses = await _context.OfficialStudentClasses.FindAsync(id);

            if (officialStudentClasses == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái cho sinh viên
            officialStudentClasses.StudentStatus = studentStatus;
            _context.Update(officialStudentClasses);
            await _context.SaveChangesAsync();

            // Chuyển hướng đến returnUrl
            return Redirect(returnUrl);
        }
    }
 }

