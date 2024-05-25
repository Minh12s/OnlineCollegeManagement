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
using ClosedXML.Excel;

namespace OnlineCollegeManagement.Controllers
{
    public class ExcelController : Controller
    {
        private readonly CollegeManagementContext _context;

        public ExcelController(CollegeManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ExportStudentsToExcel()
        {
            var students = await _context.StudentsInformation.ToListAsync();
            return Json(students);
        }
        public async Task<IActionResult> ExportFacilitiesToExcel()
        {
            var facilities = await _context.Facilities.ToListAsync();
            return Json(facilities);
        }
        public async Task<IActionResult> ExportAchievementsToExcel()
        {
            var achievements = await _context.Achievements.ToListAsync();
            return Json(achievements);
        }
        public async Task<IActionResult> ExportClassesToExcel()
        {
            var classes = await _context.Classes.ToListAsync();
            return Json(classes);
        }

        public async Task<IActionResult> ExportMajorsToExcel()
        {
            var majors = await _context.Majors.ToListAsync();
            return Json(majors);
        }
        public async Task<IActionResult> ExportDepartmentsToExcel()
        {
            var departments = await _context.Departments.ToListAsync();
            return Json(departments);
        }
      
        public async Task<IActionResult> ExportSchedulesToExcel()
        {
            var schedules = await _context.ClassSchedules.ToListAsync();
            return Json(schedules);
        }
        public async Task<IActionResult> ExportEventsToExcel()
        {
            var events = await _context.Events.ToListAsync();
            return Json(events);
        }
        public async Task<IActionResult> ExportCoursesToExcel()
        {
            var courses = await _context.Courses.ToListAsync();
            return Json(courses);
        }
        public async Task<IActionResult> ExportTeachersToExcel()
        {
            var teachers = await _context.Teachers.ToListAsync();
            return Json(teachers);
        }
        public async Task<IActionResult> ExportSubjectsToExcel()
        {
            var subjects = await _context.Subjects.ToListAsync();
            return Json(subjects);
        }
        public async Task<IActionResult> ExportContactInfoToExcel()
        {
            var contactInfo = await _context.ContactInfo.ToListAsync();
            return Json(contactInfo);
        }
        public async Task<IActionResult> ExportAdmissionsToExcel()
        {
            var admissions = await _context.StudentsInformation.ToListAsync();
            return Json(admissions);
        }
        public static class ExcelHelper
        {
            public static byte[] Export<T>(IEnumerable<T> data, string sheetName)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // Ghi dữ liệu vào sheet Excel
                    worksheet.Cell(1, 1).InsertData(data);

                    // Chuyển workbook thành một mảng byte để trả về
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
        }



    }
}
