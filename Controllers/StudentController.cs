﻿using System;
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
        public async Task<IActionResult> Student(int? page, string studentCode = null, string studentName = null, int? majorsId = null, int? courseId = null, string email = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            var officialStudents = _context.OfficialStudents
                .Include(s => s.StudentInformation)
                    .ThenInclude(si => si.Major)
                .Include(s => s.User)


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

            if (!string.IsNullOrEmpty(email))
            {
                officialStudents = officialStudents.Where(s => s.User.Email.Contains(email));
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


            return View(paginatedStudents);
        }
        public async Task<IActionResult> ViewClassStudent(int officialStudentId)
        {
            // Lấy tất cả các khóa học mà học sinh đang tham gia dựa vào OfficialStudentId
            var studentCourses = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Include(sc => sc.OfficialStudent)
                .ThenInclude(os => os.StudentInformation)
                .Where(sc => sc.OfficialStudentId == officialStudentId)
                .ToListAsync();

            // Lấy tên của sinh viên từ thông tin học sinh và đặt vào ViewBag
            ViewBag.StudentName = studentCourses.FirstOrDefault()?.OfficialStudent?.StudentInformation?.StudentName;

            // Lấy tất cả các lớp học mà học sinh đang tham gia dựa vào StudentCoursesId
            var studentClasses = await _context.StudentClasses
                .Include(sc => sc.Classes)
                .Where(sc => studentCourses.Select(s => s.StudentCoursesId).Contains(sc.StudentCoursesId))
                .ToListAsync();

            // Tạo ViewModel để truyền dữ liệu vào view
            var viewModel = new StudentClassViewModel
            {
                StudentCourses = studentCourses,
                StudentClasses = studentClasses
            };

            return View(viewModel);
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
