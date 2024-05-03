using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;

namespace OnlineCollegeManagement.Controllers
{
    public class MajorsController : Controller
    {

        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public MajorsController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Majors(int? page, string searchString, int? departmentId, int pageSize = 10)
        {
            // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu giáo viên từ cơ sở dữ liệu, bao gồm cả thông tin phòng ban
            var MajorsQuery = _context.Majors.Include(t => t.Department).AsQueryable();

            // Lọc theo phòng ban nếu departmentId được cung cấp
            if (departmentId.HasValue)
            {
                MajorsQuery = MajorsQuery.Where(t => t.DepartmentsId == departmentId);
            }

            // Tìm kiếm theo TeacherName hoặc Description nếu searchString không rỗng
            if (!string.IsNullOrEmpty(searchString))
            {
                MajorsQuery = MajorsQuery.Where(t => t.MajorName.Contains(searchString));
            }



            // Phân trang danh sách giáo viên
            var paginatedMajors = await MajorsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // Lấy tổng số giáo viên
            int totalMajors = await MajorsQuery.CountAsync();

            // Lấy danh sách phòng ban để hiển thị trong dropdown lọc
            var departments = await _context.Departments.ToListAsync();

            // Chuyển thông tin phân trang và các thông tin khác vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMajors / pageSize);
            ViewBag.TotalLecturers = totalMajors;
            ViewBag.PageSize = pageSize;
            ViewBag.Departments = departments;
            ViewBag.SelectedDepartment = departmentId;
            ViewBag.SearchString = searchString;

            // Truyền danh sách giáo viên đã phân trang vào view để hiển thị
            return View(paginatedMajors);
        }

        public IActionResult AddMajors()
        {
            var departments = _context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentsId", "DepartmentName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMajors(Majors model)
        {
            if (true)
            {
                // Kiểm tra xem có ngành học nào khác có cùng MajorName không
                var existingMajor = await _context.Majors.FirstOrDefaultAsync(m => m.MajorName == model.MajorName);
                if (existingMajor != null)
                {
                    // Nếu đã tồn tại ngành học có cùng MajorName, thêm thông báo lỗi vào ModelState
                    ModelState.AddModelError("MajorName", "This major already exists.");
                    // Lấy danh sách các phòng ban từ cơ sở dữ liệu
                    var departmentList = await _context.Departments.ToListAsync();
                    // Truyền danh sách phòng ban vào view
                    ViewBag.Departments = new SelectList(departmentList, "DepartmentsId", "DepartmentName");
                    return View(model);
                }

                try
                {
                    // Thêm ngành học vào DbSet trong context
                    _context.Add(model);
                    await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                    return RedirectToAction(nameof(Majors)); // Chuyển hướng sau khi thêm thành công
                }
                catch (Exception ex)
                {
                    // Xử lý các ngoại lệ nếu có
                    // Ví dụ: Log ngoại lệ và hiển thị thông báo lỗi cho người dùng
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the major. Please try again.");
                    // hoặc return View("Error");
                }
            }

            // Lấy danh sách các phòng ban từ cơ sở dữ liệu
            var departments = await _context.Departments.ToListAsync();
            // Truyền danh sách phòng ban vào view
            ViewBag.Departments = new SelectList(departments, "DepartmentsId", "DepartmentName");

            // Nếu mô hình không hợp lệ, trở lại view AddMajors với mô hình hiện tại
            return View(model);
        }


        public async Task<IActionResult> EditMajors(int id)
        {
            // Truy vấn thông tin về giáo viên từ cơ sở dữ liệu
            var Majors = await _context.Majors.Include(t => t.Department).FirstOrDefaultAsync(t => t.MajorsId == id);

            if (Majors == null)
            {
                return NotFound();
            }

            // Truy vấn danh sách các phòng ban từ cơ sở dữ liệu
            var departments = await _context.Departments.ToListAsync();

            // Đưa danh sách phòng ban vào ViewBag hoặc ViewData
            ViewBag.Departments = departments; // hoặc ViewData["Departments"] = departments;

            return View(Majors);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMajors(int id, Majors model)
        {
            if (true)
            {
                // Kiểm tra xem có ngành học nào khác có cùng MajorName không
                var existingMajor = await _context.Majors.FirstOrDefaultAsync(m => m.MajorName == model.MajorName && m.MajorsId != id);
                if (existingMajor != null)
                {
                    ModelState.AddModelError("MajorName", "This major already exists");
                    var departments = await _context.Departments.ToListAsync();

                    ViewBag.Departments = departments;

                    return View(model);
                }

                try
                {
                    // Lấy ngành học cần chỉnh sửa từ cơ sở dữ liệu
                    var existingMajors = await _context.Majors.FindAsync(id);
                    if (existingMajors == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường dữ liệu của ngành học
                    existingMajors.MajorName = model.MajorName;
                    existingMajors.DepartmentsId = model.DepartmentsId;

                    // Cập nhật trong cơ sở dữ liệu
                    _context.Update(existingMajors);
                    await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

                    return RedirectToAction(nameof(Majors)); // Chuyển hướng sau khi cập nhật thành công
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }

            // Nếu mô hình không hợp lệ, trở lại view AddMajors với mô hình hiện tại
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> deleteMajors(int id)
        {
            var Majors = await _context.Majors.FindAsync(id);

            if (Majors == null)
            {
                return NotFound();
            }
            _context.Majors.Remove(Majors);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Majors", "Majors");
        }

    }
}
