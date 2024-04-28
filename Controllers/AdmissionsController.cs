using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;

namespace OnlineCollegeManagement.Controllers
{
    public class AdmissionsController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AdmissionsController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public IActionResult Admissions()
        {
            // Truy vấn dữ liệu sinh viên từ cơ sở dữ liệu
            var admissions = _context.StudentsInformation.ToList();

            // Trả về view với dữ liệu sinh viên
            return View("~/Views/Admin/AdmissionsManagement/Admissions.cshtml", admissions);
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

            return View("~/Views/Admin/AdmissionsManagement/AdmissionsDetail.cshtml", admission);
        }


    }
}
