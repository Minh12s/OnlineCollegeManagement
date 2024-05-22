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
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
    public class ScheduleController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ScheduleController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Schedule(int? page, string className = null, DateTime? StartDate = null, DateTime? EndDate = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
            // Lấy dữ liệu từ bảng Classes
            var classes = _context.Classes.AsQueryable();

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(className))
            {
                classes = classes.Where(c => c.ClassName.Contains(className));
            }

            if (StartDate != null && EndDate != null)
            {
                classes = classes.Where(c => c.ClassStartDate >= StartDate && c.ClassEndDate <= EndDate);
            }


            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
            var paginatedClasses = await classes.OrderByDescending(c => c.ClassEndDate)
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
        public async Task<IActionResult> ViewSchedule(int classesId)
        {
            // Lấy lịch học dựa trên class ID
            var schedules = await _context.ClassSchedules
                .Where(schedule => schedule.ClassesId == classesId)
                .ToListAsync();
            ViewBag.ClassesId = classesId;
            // Truyền danh sách lịch học vào view để hiển thị
            return View(schedules);
        }


        [HttpGet]
        public async Task<IActionResult> AddSchedule(int classesId)
        {
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Courses = new SelectList(courses, "CoursesId", "CourseName");
            ViewBag.ClassesId = classesId;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddSchedule(ClassSchedules model, int classesId)
        {
            // Kiểm tra khóa ngoại CoursesId có tồn tại hay không
            var course = await _context.Courses.FindAsync(model.CoursesId);
            if (course == null)
            {
                // Xử lý khi khóa ngoại CoursesId không tồn tại
                var courses = await _context.Courses.ToListAsync();
                ViewBag.Courses = new SelectList(courses, "CoursesId", "CourseName");
                ViewBag.ClassesId = model.ClassesId;
                TempData["ErrorMessage"] = "Selected course does not exist.";
                ViewBag.ClassesId = classesId;
                return RedirectToAction("AddSchedule", "Schedule", new { classesId = classesId });
            }
            // Kiểm tra xem lớp học đã có lịch học hay chưa
            var existingSchedules = await _context.ClassSchedules
                .AnyAsync(schedule => schedule.ClassesId == classesId);

            if (existingSchedules)
            {
                // Lớp học đã có lịch học, trả về thông báo lỗi
                TempData["ErrorMessage"] = "This class already has schedules.";
                // Load danh sách khóa học cho dropdown
                var courses = await _context.Courses.ToListAsync();
                ViewBag.Courses = new SelectList(courses, "CoursesId", "CourseName");
                ViewBag.ClassesId = classesId;

                // Trả về view với thông báo lỗi và model
                return RedirectToAction("AddSchedule", "Schedule", new { classesId = classesId });
            }
            // Kiểm tra khóa ngoại ClassesId có tồn tại hay không
            var classEntity = await _context.Classes.FindAsync(model.ClassesId);
            if (classEntity == null)
            {
                // Xử lý khi khóa ngoại ClassesId không tồn tại
                var courses = await _context.Courses.ToListAsync();
                ViewBag.Courses = new SelectList(courses, "CoursesId", "CourseName");
                ViewBag.ClassesId = model.ClassesId;
                TempData["ErrorMessage"] = "Selected class does not exist.";
                return RedirectToAction("AddSchedule", "Schedule", new { classesId = classesId });
            }

            var courseSubjects = await _context.CoursesSubjects
                .Where(cs => cs.CoursesId == model.CoursesId)
                .OrderBy(cs => cs.NumericalOrder)
                .ToListAsync();

            var studyDays = model.StudyDays.Split(',').Select(day => day.Trim()).ToArray();
            var studySession = model.StudySession;
            var startDate = DateTime.Today;
            var studyDayIndices = studyDays.Select(day => (int)Enum.Parse(typeof(DayOfWeek), day, true)).ToArray();

            if (!studyDayIndices.Contains((int)startDate.DayOfWeek))
            {
                startDate = startDate.AddDays((7 - (int)startDate.DayOfWeek + studyDayIndices.First()) % 7);
            }

            foreach (var courseSubject in courseSubjects)
            {
                var subject = await _context.Subjects.FindAsync(courseSubject.SubjectsId);
                if (subject == null) continue;

                for (int i = 0; i < subject.NumberOfSessions; i++)
                {
                    var schedule = new ClassSchedules
                    {
                        ClassesId = classesId,
                        CoursesId = model.CoursesId,
                        SubjectName = subject.SubjectName,
                        StudyDays = model.StudyDays,
                        StudySession = model.StudySession,
                        SchedulesDate = startDate
                    };

                    _context.ClassSchedules.Add(schedule);

                    // Tính ngày học tiếp theo
                    int nextStudyDayIndex = (Array.IndexOf(studyDayIndices, (int)startDate.DayOfWeek) + 1) % studyDayIndices.Length;
                    int daysUntilNextStudyDay = (studyDayIndices[nextStudyDayIndex] - (int)startDate.DayOfWeek + 7) % 7;
                    startDate = startDate.AddDays(daysUntilNextStudyDay);
                }

                // Thêm một bản ghi cho ngày thi (test) sau khi kết thúc các phiên học của môn học
                var testSchedule = new ClassSchedules
                {
                    ClassesId = classesId,
                    CoursesId = model.CoursesId,
                    SubjectName = subject.SubjectName + " Test",
                    StudyDays = model.StudyDays,
                    StudySession = model.StudySession,
                    SchedulesDate = startDate
                };

                _context.ClassSchedules.Add(testSchedule);

                // Tính ngày học tiếp theo sau ngày thi
                int nextTestStudyDayIndex = (Array.IndexOf(studyDayIndices, (int)startDate.DayOfWeek) + 1) % studyDayIndices.Length;
                int daysUntilNextTestStudyDay = (studyDayIndices[nextTestStudyDayIndex] - (int)startDate.DayOfWeek + 7) % 7;
                startDate = startDate.AddDays(daysUntilNextTestStudyDay);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewSchedule", "Schedule", new { classesId = classesId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int classesId)
        {
            try
            {
                // Kiểm tra xem lớp học có lịch học không
                var hasSchedules = await _context.ClassSchedules.AnyAsync(schedule => schedule.ClassesId == classesId);

                // Nếu lớp học không có lịch học, thông báo lỗi
                if (!hasSchedules)
                {
                    TempData["ErrorMessage"] = "This class does not have any schedules to delete.";
                    return RedirectToAction("ViewSchedule", "Schedule", new { classesId = classesId });
                }

                // Nếu lớp học có lịch học, tiến hành xóa lịch học
                var schedulesToDelete = await _context.ClassSchedules
                    .Where(schedule => schedule.ClassesId == classesId)
                    .ToListAsync();

                _context.ClassSchedules.RemoveRange(schedulesToDelete);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công vào TempData
                TempData["SuccessMessage"] = "Schedule deleted successfully.";

                // Chuyển hướng người dùng về trang xem lịch học cho lớp học đã xoá lịch
                return RedirectToAction("ViewSchedule", "Schedule", new { classesId = classesId });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                TempData["ErrorMessage"] = "An error occurred while deleting schedules.";
                return RedirectToAction("ViewSchedule", "Schedule", new { classesId = classesId });
            }
        }



    }

}

