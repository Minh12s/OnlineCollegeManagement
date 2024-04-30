using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;

namespace OnlineCollegeManagement.Controllers
{
    public class AdmissionsManagement : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AdmissionsManagement(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Admissions(int? page, string searchString, int pageSize = 5)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Truy vấn dữ liệu sinh viên từ cơ sở dữ liệu
            var registrationsQuery = _context.Registrations.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                registrationsQuery = registrationsQuery.Where(e => e.RegistrationStatus.Contains(searchString));
            }
            // Thực hiện truy vấn để lấy dữ liệu sự kiện
            var events = await registrationsQuery.ToListAsync();

            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedRegistration = await registrationsQuery
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalRegistration = await registrationsQuery.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistration / pageSize);
            ViewBag.TotalRegistration = totalRegistration;
            ViewBag.PageSize = pageSize;

            // Truyền dữ liệu sự kiện vào view thông qua mô hình hoặc ViewBag
            return View(paginatedRegistration);
        }




        public async Task<IActionResult> AdmissionsDetail(int? StudentsInformationId)
        {
            if (StudentsInformationId == null)
            {
                return NotFound();
            }

            var admission = await _context.StudentsInformation
                .FirstOrDefaultAsync(s => s.StudentsInformationId == StudentsInformationId);

            if (admission == null)
            {
                return NotFound();
            }

            return View("AdmissionsDetail", admission); // Chỉ định tên view là "AdmissionsDetail"
        }


    }
}
