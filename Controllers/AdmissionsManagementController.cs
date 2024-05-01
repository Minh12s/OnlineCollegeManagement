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
        public async Task<IActionResult> Admissions(int? page, string RegistrationStatus = null, string UniqueCode = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int pageSize = 5)
        {
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu sinh viên từ cơ sở dữ liệu
            var registrations = _context.Registrations.AsQueryable();

            // Lọc theo trạng thái mong muốn
            if (!string.IsNullOrEmpty(RegistrationStatus))
            {
                registrations = registrations.Where(e => e.RegistrationStatus == RegistrationStatus);
            }
            if (!string.IsNullOrEmpty(UniqueCode))
            {
                registrations = registrations.Where(e => e.UniqueCode == UniqueCode);
            }
            if (startDate != null)
            {
                registrations = registrations.Where(c => c.RegistrationDate >= startDate);
            }
            if (endDate != null)
            {
                // Chú ý: Khi lọc theo ngày kết thúc, hãy thêm 1 ngày vào để bao gồm tất cả các bài đăng được đăng vào ngày kết thúc
                registrations = registrations.Where(c => c.RegistrationDate < endDate.Value.AddDays(1));
            }
            // Thực hiện truy vấn để lấy dữ liệu đăng ký
            var paginatedRegistrations = await registrations.OrderByDescending(c => c.RegistrationDate)
                                               .Skip((pageNumber - 1) * pageSize)
                                               .Take(pageSize)
                                               .ToListAsync();


            // Lấy tổng số đăng ký sau khi áp dụng các tiêu chí lọc
            int totalRegistrations = await registrations.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistrations / pageSize);
            ViewBag.TotalRegistrations = totalRegistrations;
            ViewBag.PageSize = pageSize;

            return View(paginatedRegistrations);
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
