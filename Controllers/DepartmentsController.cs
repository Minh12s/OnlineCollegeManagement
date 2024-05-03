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
    public class DepartmentsController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public DepartmentsController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Departments(int? page, string DepartmentName = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var departments = _context.Departments.AsQueryable();
          

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(DepartmentName))
            {
                departments = departments.Where(c => c.DepartmentName.Contains(DepartmentName));
            }
       
  
            var paginatedDepartments = await departments.OrderByDescending(c => c.DepartmentsId)
                                                .Skip((pageNumber - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalDepartments = await departments.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
  
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalDepartments / pageSize);
            ViewBag.TotalDepartments = totalDepartments;
            ViewBag.PageSize = pageSize;

            return View(paginatedDepartments);
        }
        // GET: Departments/Create
        public IActionResult AddDepartments()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDepartments([Bind("DepartmentsId,DepartmentName")] Departments departments)
        {
            // Kiểm tra xem tên bộ phận đã tồn tại hay chưa
            if (DepartmentExists(departments.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", "Department name already exists.");
                return View(departments);
            }

            // Kiểm tra xem dữ liệu đầu vào có hợp lệ không (ví dụ: các trường bắt buộc)
            if (!string.IsNullOrEmpty(departments.DepartmentName))
            {
                // Chuyển đổi DepartmentName sang chữ thường để so sánh
                string departmentNameLower = departments.DepartmentName.ToLower();

                // Kiểm tra xem có bất kỳ bộ phận nào có trùng tên (không phân biệt chữ hoa/chữ thường) không
                if (_context.Departments.Any(d => d.DepartmentName.ToLower() == departmentNameLower))
                {
                    ModelState.AddModelError("DepartmentName", "Department name already exists.");
                    return View(departments);
                }

                // Thêm bộ phận vào cơ sở dữ liệu
                _context.Add(departments);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Departments));
            }
            else
            {
                // Nếu dữ liệu không hợp lệ, hiển thị thông báo lỗi và trả về view để người dùng nhập lại
                ModelState.AddModelError(string.Empty, "Please provide a department name.");
                return View(departments);
            }
        }

        private bool DepartmentExists(string departmentName)
        {
            return _context.Departments.Any(d => d.DepartmentName == departmentName);
        }
        public async Task<IActionResult> EditDepartments(int id)
        {
            // Tìm phòng ban cần chỉnh sửa bằng ID
            var department = await _context.Departments.FindAsync(id);

            // Nếu không tìm thấy phòng ban, trả về NotFound
            if (department == null)
            {
                return NotFound();
            }

            // Trả về view với dữ liệu phòng ban để hiển thị trên form chỉnh sửa
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartments(int id, [Bind("DepartmentsId,DepartmentName")] Departments department)
        {
            // Kiểm tra xem ID của phòng ban được chỉnh sửa có khớp với ID được truyền vào không
            if (id != department.DepartmentsId)
            {
                return NotFound();
            }

            // Kiểm tra xem dữ liệu đầu vào có hợp lệ không (ví dụ: các trường bắt buộc)
            if (!string.IsNullOrEmpty(department.DepartmentName))
            {
                // Chuyển đổi DepartmentName sang chữ thường để so sánh
                string departmentNameLower = department.DepartmentName.ToLower();

                // Kiểm tra xem có bất kỳ bộ phận nào có trùng tên (không phân biệt chữ hoa/chữ thường) ngoại trừ phòng ban hiện tại không
                if (_context.Departments.Any(d => d.DepartmentName.ToLower() == departmentNameLower && d.DepartmentsId != id))
                {
                    ModelState.AddModelError("DepartmentName", "Department name already exists.");
                    return View(department);
                }

                try
                {
                    // Cập nhật thông tin của phòng ban trong cơ sở dữ liệu
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý lỗi khi cơ sở dữ liệu không thể cập nhật do xung đột
                    if (!DepartmentExists(department.DepartmentsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Chuyển hướng đến action hiển thị danh sách phòng ban sau khi chỉnh sửa thành công
                return RedirectToAction(nameof(Departments));
            }
            // Nếu dữ liệu không hợp lệ, hiển thị thông báo lỗi và trả về view để người dùng nhập lại
            ModelState.AddModelError(string.Empty, "Please provide a department name.");
            return View(department);
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentsId == id);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDepartments(int id)
        {
            // Tìm phòng ban cần xóa bằng ID
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            // Xóa phòng ban khỏi cơ sở dữ liệu và lưu thay đổi
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Departments));
        }
        public IActionResult ViewMajors(int departmentId)
        {
            // Lấy danh sách các Majors dựa vào DepartmentsId
            var majors = _context.Majors.Where(m => m.DepartmentsId == departmentId).ToList();

            // Trả về view với danh sách các Majors
            return View(majors);
        }
    }
}

