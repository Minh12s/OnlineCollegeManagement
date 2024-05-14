using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using BCrypt.Net;
using System.Text;
using System.Net.Mail;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using System.Net;
using System.IO;
using OnlineCollegeManagement.Heplers;
using OnlineCollegeManagement.Models.Authentication;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
    public class StudentController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public StudentController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Student(int? page, string studentCode = null, string studentName = null, int? majorsId = null, int? courseId = null, string email = null, string telephone = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var officialStudents = _context.OfficialStudents
                .Include(s => s.StudentInformation)
                    .ThenInclude(si => si.Major)
                .Include(s => s.User)
                .Include(s => s.Course) // Ensure this is included
                .Include(s => s.Classes)
                .AsQueryable();

            // Fetch the list of courses and majors
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Majors = await _context.Majors.ToListAsync();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(studentCode))
            {
                officialStudents = officialStudents.Where(s => s.StudentCode.Contains(studentCode));
            }
            if (!string.IsNullOrEmpty(studentName))
            {
                officialStudents = officialStudents.Where(s => s.StudentInformation.StudentName.Contains(studentName));
            }
            if (majorsId.HasValue)
            {
                officialStudents = officialStudents.Where(s => s.StudentInformation.MajorsId == majorsId.Value);
            }
            if (courseId.HasValue)
            {
                officialStudents = officialStudents.Where(s => s.Course.CoursesId == courseId.Value); // Corrected
            }
            if (!string.IsNullOrEmpty(email))
            {
                officialStudents = officialStudents.Where(s => s.User.Email.Contains(email));
            }
            if (!string.IsNullOrEmpty(telephone))
            {
                officialStudents = officialStudents.Where(s => s.Telephone.Contains(telephone));
            }

            // Pagination
            var paginatedStudents = await officialStudents
                .OrderBy(s => s.StudentCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalStudents = await officialStudents.CountAsync();

            // Pass pagination info to ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalStudents / pageSize);
            ViewBag.TotalStudents = totalStudents;
            ViewBag.PageSize = pageSize;
            ViewBag.StudentCode = studentCode;
            ViewBag.StudentName = studentName;
            ViewBag.MajorsId = majorsId;
            ViewBag.CourseId = courseId;
            ViewBag.Email = email;
            ViewBag.Telephone = telephone;

            return View(paginatedStudents);
        }



        public async Task<IActionResult> DetailsStudent(int? StudentsInformationId)
        {
            if (StudentsInformationId == null)
            {
                return NotFound();
            }

            // Truy vấn RegistrationStatus dựa trên StudentsInformationId
            var registrationStatus = await _context.Registrations
                .Where(r => r.StudentsInformationId == StudentsInformationId)
                .Select(r => r.RegistrationStatus)
                .FirstOrDefaultAsync();

            if (registrationStatus == null)
            {
                return NotFound();
            }

            // Lấy thông tin sinh viên
            var student = await _context.StudentsInformation
                .FirstOrDefaultAsync(s => s.StudentsInformationId == StudentsInformationId);

            if (student == null)
            {
                return NotFound();
            }
            return View(student); // Trả về view "AdmissionsDetail" với dữ liệu sinh viên
        }
       
    }
}

