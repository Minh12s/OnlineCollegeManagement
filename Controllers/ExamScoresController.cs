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

        public async Task<IActionResult> ViewStudents(int classesId)
        {
            // Lưu classesId vào ViewBag để sử dụng trong view
            ViewBag.ClassesId = classesId;

            // Lấy danh sách sinh viên dựa trên classesId từ bảng StudentClasses
            var studentCoursesWithClasses = await _context.StudentCourses
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.StudentInformation)
                .Include(sc => sc.OfficialStudent)
                    .ThenInclude(os => os.User) // Bao gồm thông tin từ bảng User
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

            return View(studentCoursesWithClasses);
        }




        public async Task<IActionResult> ExamScores(int? page, int OfficialStudentId, int CourseId, int pageSize = 10)
        {
            int pageNumber = page ?? 1;

            // Truy vấn cơ sở dữ liệu để lấy các bản ghi ExamScores tương ứng với OfficialStudentId và CoursesId
            var examScores = _context.ExamScores
                .Include(e => e.OfficialStudent)
                .Include(e => e.Subject)
                .Where(e => e.OfficialStudentId == OfficialStudentId && e.CoursesId == CourseId)
                .AsQueryable();

            // Pagination
            var paginatedExamScores = await examScores
                .OrderBy(s => s.ExamScoresId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalExamScores = await examScores.CountAsync();

            // Pass pagination info to ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalExamScores / pageSize);
            ViewBag.TotalExamScores = totalExamScores;
            ViewBag.PageSize = pageSize;
            ViewBag.OfficialStudentId = OfficialStudentId;
            ViewBag.CoursesId = CourseId;

            // Truyền dữ liệu vào view
            return View(paginatedExamScores);
        }

    


        public async Task<IActionResult> EditScores(int id)
        {
            // Lấy thông tin của bản ghi ExamScores có id tương ứng
            var examScore = await _context.ExamScores
                .Include(e => e.Subject)
                .Include(e => e.OfficialStudent)
                .FirstOrDefaultAsync(e => e.ExamScoresId == id);

            if (examScore == null)
            {
                return NotFound();
            }

            // Lấy danh sách các môn thuộc khóa học của examScore
            var subjectsForCourse = await _context.CoursesSubjects
                .Where(cs => cs.CoursesId == examScore.CoursesId)
                .Select(cs => cs.Subject)
                .OrderBy(s => s.SubjectName)
                .ToListAsync();

            // Truyền danh sách các môn học thuộc khóa học đó vào ViewBag
            ViewBag.Subjects = subjectsForCourse;
            ViewBag.OfficialStudentId = examScore.OfficialStudentId;
            @ViewBag.CoursesId = examScore.CoursesId;
            return View(examScore);
        }

        [HttpPost]
        public async Task<IActionResult> EditScores(int id, decimal score)
        {
            var examScoreInDb = await _context.ExamScores.FindAsync(id);

            if (examScoreInDb == null)
            {
                return NotFound();
            }

            examScoreInDb.Score = score;
            examScoreInDb.Status = score >= 40 ? "Passed" : "Not Passed";

            await _context.SaveChangesAsync();

            var subjectName = (await _context.Subjects.FindAsync(examScoreInDb.SubjectsId))?.SubjectName;

            // Gửi email xác nhận với tên môn học
            await SendEditExamScoresEmail("longtqth2209038@fpt.edu.vn", subjectName);

            return RedirectToAction("ExamScores", "ExamScores", new { OfficialStudentId = examScoreInDb.OfficialStudentId, CourseId = examScoreInDb.CoursesId });
        }




        private async Task SendAddExamScoresEmail(string recipientEmail, string SubjectName)
        {
            try
            {
                // Đường dẫn tới mẫu email
                string emailTemplatePath = Path.Combine(_env.ContentRootPath, "Views", "Email", "AddExamScores.cshtml");

                // Đọc nội dung mẫu email từ file
                string emailContent = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                // Thay thế các thẻ placeholder trong mẫu email bằng thông tin thích hợp
                emailContent = emailContent.Replace("{SubjectName}", SubjectName);

                // Tạo đối tượng MailMessage
                var message = new MailMessage();
                message.To.Add(new MailAddress(recipientEmail)); // Địa chỉ email của sinh viên
                message.From = new MailAddress(_configuration["EmailSettings:Username"]);
                message.Subject = "Confirm successful registration!";
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
                message.Subject = "Confirm successful registration!";
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
