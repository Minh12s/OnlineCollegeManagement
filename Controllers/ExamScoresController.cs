using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using System.Drawing.Printing;
using System.Net.Mail;
using System.Net;

namespace OnlineCollegeManagement.Controllers
{
    public class ExamScoresController : Controller
    {

        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ExamScoresController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }

        public async Task<IActionResult> Classes(int? page, string className = null, DateTime? StartDate = null, DateTime? EndDate = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1;
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

        public async Task<IActionResult> ViewSubject(int classesId)
        {
            ViewBag.ClassesId = classesId;

            var studentCoursesWithClasses = await _context.StudentCourses
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.User)
                .Join(
                    _context.StudentClasses,
                    studentCourse => studentCourse.StudentCoursesId,
                    studentClass => studentClass.StudentCoursesId,
                    (studentCourse, studentClass) => new StudentCourseClassViewModel
                    {
                        StudentCourse = studentCourse,
                        StudentClass = studentClass
                    })
                .Where(sc => sc.StudentClass.ClassesId == classesId && sc.StudentClass.DeleteStatus == 0)
                .ToListAsync();

            var courseId = studentCoursesWithClasses.FirstOrDefault()?.StudentCourse.CoursesId;
            if (courseId.HasValue)
            {
                ViewBag.CourseId = courseId.Value;

                var officialStudentIds = await _context.StudentCourses
                    .Where(sc => sc.CoursesId == courseId.Value)
                    .Select(sc => sc.OfficialStudentId)
                    .ToListAsync();
                ViewBag.OfficialStudentIds = officialStudentIds;

                var subjects = await _context.CoursesSubjects
                    .Include(cs => cs.Subject)
                    .Where(cs => cs.CoursesId == courseId.Value)
                    .Select(cs => cs.Subject)
                    .ToListAsync();

                ViewBag.Subjects = subjects;

            }
            else
            {
                ViewBag.Subjects = new List<Subjects>();
                ViewBag.CourseId = null;
                ViewBag.OfficialStudentIds = new List<int>();
            }

            // Truyền studentCoursesWithClasses vào ViewBag.StudentInfo

            return View(studentCoursesWithClasses);
        }




        public async Task<IActionResult> ExamScores(int classesId, int coursesId, int subjectsId)
        {
            var examScores = await _context.ExamScores
                .Include(es => es.Subject)
                .Include(es => es.Course)
                .Include(es => es.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Where(es => es.CoursesId == coursesId && es.SubjectsId == subjectsId)
                .Join(
                    _context.StudentClasses.Where(sc => sc.ClassesId == classesId && sc.DeleteStatus == 0),
                    examScore => examScore.OfficialStudentId,
                    studentClass => studentClass.StudentCourses.OfficialStudentId,
                    (examScore, studentClass) => examScore
                )
                .ToListAsync();

            // Lưu các tham số vào ViewBag để sử dụng trong view
            ViewBag.ClassesId = classesId;
            ViewBag.CoursesId = coursesId;
            ViewBag.SubjectsId = subjectsId;

            return View(examScores);
        }

        [HttpPost]
        public async Task<IActionResult> ExamScores(Dictionary<int, decimal?> scores, int classesId, int coursesId, int subjectsId)
        {
            try
            {
                foreach (var kvp in scores)
                {
                    var examScore = await _context.ExamScores.FindAsync(kvp.Key);
                    if (examScore != null)
                    {
                        examScore.Score = kvp.Value;
                        examScore.Status = kvp.Value >= 40 ? "Passed" : "Not Passed";
                        _context.Entry(examScore).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                var subject = await _context.Subjects.FindAsync(subjectsId);
                await SendEditExamScoresEmail("dungprohn1409@gmail.com", subject.SubjectName);

                // Redirect or return success response
                return RedirectToAction(nameof(ExamScores), new { classesId = classesId, coursesId = coursesId, subjectsId = subjectsId });
            }
            catch (Exception ex)
            {
                // Handle error
                return StatusCode(500, new { message = "An error occurred while updating scores", error = ex.Message });
            }
        }





       


       

        private async Task SendEditExamScoresEmail(string recipientEmail, string SubjectName)
        {
            try
            {
                // Đường dẫn tới mẫu email
                string emailTemplatePath = Path.Combine(_env.ContentRootPath, "Views", "Email", "EditExamScores.cshtml");

                // Đọc nội dung mẫu email từ file
                string emailContent = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                // Thay thế các thẻ placeholder trong mẫu email bằng thông tin thích hợp
                emailContent = emailContent.Replace("{SubjectName}", SubjectName);

                // Tạo đối tượng MailMessage
                var message = new MailMessage();
                message.To.Add(new MailAddress(recipientEmail)); // Địa chỉ email của sinh viên
                message.From = new MailAddress(_configuration["EmailSettings:Username"]);
                message.Subject = $"Notification: Exam Scores for {SubjectName}";
                message.Body = emailContent;
                message.IsBodyHtml = true;

                // Tạo đối tượng SmtpClient để gửi email
                using (var smtp = new SmtpClient(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:Port"])))
                {
                    var credentials = new NetworkCredential
                    {
                        UserName = _configuration["EmailSettings:Username"],
                        Password = _configuration["EmailSettings:Password"]
                    };
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = true;

                    // Gửi email
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi xảy ra khi gửi email
                // Log lỗi ex.Message
            }
        }

    }
}
