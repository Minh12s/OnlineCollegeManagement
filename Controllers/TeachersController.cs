using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using OnlineCollegeManagement.Models.Authentication;

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
    public class TeachersController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public TeachersController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Lecturer(int? page, string searchString, int? departmentId, DateTime? joinDate, int pageSize = 5)
        {
            // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu giáo viên từ cơ sở dữ liệu, bao gồm cả thông tin phòng ban
            var lecturersQuery = _context.Teachers.Include(t => t.Department).AsQueryable();

            // Lọc theo phòng ban nếu departmentId được cung cấp
            if (departmentId.HasValue)
            {
                lecturersQuery = lecturersQuery.Where(t => t.DepartmentsId == departmentId);
            }

            // Tìm kiếm theo TeacherName hoặc Description nếu searchString không rỗng
            if (!string.IsNullOrEmpty(searchString))
            {
                lecturersQuery = lecturersQuery.Where(t => t.TeacherName.Contains(searchString) || t.Description.Contains(searchString));
            }

            // Lọc theo ngày tham gia nếu joinDate được cung cấp
            if (joinDate.HasValue)
            {
                lecturersQuery = lecturersQuery.Where(t => t.JoinDate.Date == joinDate.Value.Date);
            }

            // Phân trang danh sách giáo viên
            var paginatedLecturers = await lecturersQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // Lấy tổng số giáo viên
            int totalLecturers = await lecturersQuery.CountAsync();

            // Lấy danh sách phòng ban để hiển thị trong dropdown lọc
            var departments = await _context.Departments.ToListAsync();

            // Chuyển thông tin phân trang và các thông tin khác vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalLecturers / pageSize);
            ViewBag.TotalLecturers = totalLecturers;
            ViewBag.PageSize = pageSize;
            ViewBag.Departments = departments;
            ViewBag.SelectedDepartment = departmentId;
            ViewBag.SearchString = searchString;
            ViewBag.JoinDate = joinDate;

            // Truyền danh sách giáo viên đã phân trang vào view để hiển thị
            return View(paginatedLecturers);
        }



        public IActionResult AddLecturer()
        {
            var departments = _context.Departments.ToList();
            ViewBag.Departments = new SelectList(departments, "DepartmentsId", "DepartmentName");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLecturer(Teachers model, IFormFile ImageUrl)
        {
            if (true)
            {
                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Lecturer");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + ImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(stream);
                    }

                    model.ImageUrl = "/images/Lecturer/" + Path.GetFileName(imagePath);

                }

                model.JoinDate = DateTime.Now;

                _context.Add(model); // Thêm sự kiện vào DbSet trong context
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction(nameof(Lecturer)); // Chuyển hướng sau khi thêm thành công
            }

            // Nếu mô hình không hợp lệ, trở lại view AddEvent với mô hình hiện tại
            return View(model);
        }




        public async Task<IActionResult> EditLecturer(int id)
        {
            // Truy vấn thông tin về giáo viên từ cơ sở dữ liệu
            var teacher = await _context.Teachers.Include(t => t.Department).FirstOrDefaultAsync(t => t.TeachersId == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Truy vấn danh sách các phòng ban từ cơ sở dữ liệu
            var departments = await _context.Departments.ToListAsync();

            // Đưa danh sách phòng ban vào ViewBag hoặc ViewData
            ViewBag.Departments = departments; // hoặc ViewData["Departments"] = departments;

            return View( teacher);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(int id, Teachers model, IFormFile ImageUrl)
        {
            if (true)
            {
                // Kiểm tra sự tồn tại của sự kiện cần chỉnh sửa dựa trên id
                var existingEvent = await _context.Teachers.FirstOrDefaultAsync(e => e.TeachersId == id);
                if (existingEvent == null)
                {
                    return NotFound(); // Trả về lỗi 404 nếu không tìm thấy sự kiện cần chỉnh sửa
                }

                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    // Nếu có hình ảnh mới được cung cấp, cập nhật hình ảnh
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Lecturer");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + ImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(stream);
                    }

                    // Cập nhật đường dẫn hình ảnh mới
                    existingEvent.ImageUrl = "/images/Lecturer/" + Path.GetFileName(imagePath);
                }

                // Cập nhật các trường dữ liệu khác của sự kiện
                existingEvent.TeacherName = model.TeacherName;
                existingEvent.Description = model.Description;
                existingEvent.JoinDate = DateTime.Now; // Cập nhật ngày sự kiện nếu cần
                existingEvent.DepartmentsId = model.DepartmentsId;

                _context.Update(existingEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Lecturer));
            }

            return View(model); // Trả về view với dữ liệu không hợp lệ nếu ModelState không hợp lệ
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> deleteLecturer(int id)
        {
            var Teachers = await _context.Teachers.FindAsync(id);

            if (Teachers == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
            if (!string.IsNullOrEmpty(Teachers.ImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", Teachers.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Teachers.Remove(Teachers);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Lecturer", "Teachers");
        }

    }
}
