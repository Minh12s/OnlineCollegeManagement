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
using System.Text.RegularExpressions;

namespace OnlineCollegeManagement.Controllers
{

    public class PageController : Controller
    {
        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public PageController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }
        public async Task<IActionResult> Home()
        {
            return View();
        }
        public async Task<IActionResult> Search(string searchTerm)
        {
            // Thực hiện tìm kiếm dựa trên UniqueCode
            var registrations = await _context.Registrations
                .Include(r => r.StudentInformation)
                    .ThenInclude(si => si.Major)
                .Where(r => r.UniqueCode.Contains(searchTerm))
                .ToListAsync();

            // Trả về kết quả tìm kiếm
            return View(registrations);
        }
        public async Task<IActionResult> DetailsSearch(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentInfo = await _context.StudentsInformation
                .Include(si => si.Major)
                .FirstOrDefaultAsync(si => si.StudentsInformationId == id);

            if (studentInfo == null)
            {
                return NotFound();
            }

            return View(studentInfo);
        }
        public async Task<IActionResult> About()
        {
            return View();
        }
        public async Task<IActionResult> Facilities(int? page)
        {
            // Số lượng cơ sở vật lý trên mỗi trang
            int pageSize = 3;

            // Trang hiện tại (mặc định là trang 1 nếu không có giá trị được cung cấp)
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu từ bảng Facilities và sắp xếp theo ngày mới nhất
            var facilities = await _context.Facilities
                .OrderByDescending(f => f.FacilityDate) // Sắp xếp giảm dần theo ngày
                .Skip((pageNumber - 1) * pageSize)     // Bỏ qua các cơ sở vật lý trên các trang trước
                .Take(pageSize)                        // Lấy số lượng cơ sở vật lý trên trang hiện tại
                .ToListAsync();

            // Tổng số cơ sở vật lý
            int totalFacilities = await _context.Facilities.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling((double)totalFacilities / pageSize);

            // Truyền các thông tin về phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            // Trả về view và truyền dữ liệu facilities vào view
            return View(facilities);
        }
        public async Task<IActionResult> Achievements(int? page)
        {
            // Số lượng cơ sở vật lý trên mỗi trang
            int pageSize = 3;

            // Trang hiện tại (mặc định là trang 1 nếu không có giá trị được cung cấp)
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu từ bảng Facilities và sắp xếp theo ngày mới nhất
            var achievements = await _context.Achievements
                .OrderByDescending(f => f.AchievementDate) // Sắp xếp giảm dần theo ngày
                .Skip((pageNumber - 1) * pageSize)     // Bỏ qua các cơ sở vật lý trên các trang trước
                .Take(pageSize)                        // Lấy số lượng cơ sở vật lý trên trang hiện tại
                .ToListAsync();

            // Tổng số cơ sở vật lý
            int totalAchievements = await _context.Achievements.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling((double)totalAchievements / pageSize);

            // Truyền các thông tin về phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            // Trả về view và truyền dữ liệu facilities vào view
            return View(achievements);
        }
        public async Task<IActionResult> Courses(int? page, int pageSize = 9)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Lấy userId từ session
            var userIdString = HttpContext.Session.GetString("UserId");

            // Kiểm tra xem user đã đăng nhập hay chưa
            bool isLoggedIn = !string.IsNullOrEmpty(userIdString);

            if (isLoggedIn)
            {
                int userId = Convert.ToInt32(userIdString);

                // Truy vấn cơ sở dữ liệu để lấy thông tin sinh viên chính thức từ bảng OfficialStudent dựa trên user id
                var officialStudent = await _context.OfficialStudents
                                                    .FirstOrDefaultAsync(os => os.UsersId == userId);

                if (officialStudent == null)
                {
                    // Xử lý khi không tìm thấy thông tin sinh viên
                    return RedirectToAction("Error");
                }

                // Lấy student information id từ thông tin sinh viên chính thức
                int studentInformationId = officialStudent.StudentsInformationId;

                // Truy vấn thông tin sinh viên từ bảng StudentInformation dựa trên student information id
                var studentInformation = await _context.StudentsInformation
                                                       .FirstOrDefaultAsync(si => si.StudentsInformationId == studentInformationId);

                if (studentInformation == null)
                {
                    // Xử lý khi không tìm thấy thông tin sinh viên
                    return RedirectToAction("Error");
                }

                // Lấy major id từ thông tin sinh viên
                int majorId = studentInformation.MajorsId;

                // Truy vấn danh sách khóa học dựa trên major id
                var coursesQuery = _context.Courses.Where(c => c.MajorsId == majorId).AsQueryable();
                var paginatedCourses = await coursesQuery
                                              .Skip((pageNumber - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();

                // Lấy tổng số khóa học sau khi áp dụng các tiêu chí lọc
                int totalCourses = await coursesQuery.CountAsync();

                // Kiểm tra xem sinh viên đã đăng ký cả hai nhóm ngày học hay chưa
                var studyDaysGroups = await _context.MergedStudentData
                                                    .Where(msd => msd.OfficialStudentId == officialStudent.OfficialStudentId)
                                                    .Select(msd => msd.StudyDays)
                                                    .Distinct()
                                                    .ToListAsync();
                bool hasEnrolledAllStudyDays = studyDaysGroups.Contains("Monday, Wednesday, Friday") &&
                                               studyDaysGroups.Contains("Tuesday, Thursday, Saturday");

                // Chuyển thông tin phân trang vào ViewBag
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
                ViewBag.totalCourses = totalCourses;
                ViewBag.PageSize = pageSize;

                // Truyền thông tin đăng nhập và trạng thái đăng ký ngày học xuống view
                ViewBag.IsLoggedIn = true;
                ViewBag.HasEnrolledAllStudyDays = hasEnrolledAllStudyDays;

                return View(paginatedCourses);
            }
            else
            {
                // Nếu không có user id, truy vấn tất cả các khóa học
                var allCourses = await _context.Courses.ToListAsync();

                // Phân trang tất cả các khóa học
                var paginatedCourses = allCourses.Skip((pageNumber - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .ToList();

                // Tính toán thông tin phân trang
                int totalCourses = allCourses.Count;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
                ViewBag.totalCourses = totalCourses;
                ViewBag.PageSize = pageSize;

                // Truyền thông tin đăng nhập xuống view
                ViewBag.IsLoggedIn = false;
                ViewBag.HasEnrolledAllStudyDays = false;

                return View(paginatedCourses);
            }
        }
        public async Task<IActionResult> Enroll(int courseId)
        {
            // Kiểm tra xem user đã đăng nhập hay chưa
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                // Nếu user chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Page");
            }

            // Lấy thông tin khóa học
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                // Xử lý khi khóa học không tồn tại
                return NotFound("Course not found");
            }

            int userId = Convert.ToInt32(userIdString);

            // Kiểm tra xem sinh viên đã đăng ký khóa học này chưa
            var existingEnrollment = await _context.MergedStudentData
                                                   .FirstOrDefaultAsync(osc => osc.OfficialStudent.UsersId == userId && osc.CoursesId == courseId);
            if (existingEnrollment != null)
            {
                // Nếu đã đăng ký, hiển thị thông báo lỗi hoặc chuyển hướng
                TempData["ErrorMessage"] = "You have already enrolled in this course.";
                return RedirectToAction("Courses");
            }

            // Truyền thông tin khóa học vào view
            ViewBag.Course = course;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId, string telephone, string studyDays, string studySession)
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Page");
            }

            int userId = Convert.ToInt32(userIdString);

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            var officialStudent = await _context.OfficialStudents.FirstOrDefaultAsync(os => os.UsersId == userId);
            if (officialStudent == null)
            {
                return RedirectToAction("Error");
            }

            var existingEnrollment = await _context.MergedStudentData
                                                   .FirstOrDefaultAsync(osc => osc.OfficialStudentId == officialStudent.OfficialStudentId && osc.CoursesId == courseId);
            if (existingEnrollment != null)
            {
                TempData["ErrorMessage"] = "You have already enrolled in this course.";
                ViewBag.Course = course;
                return View(course);
            }

            var conflictingEnrollment = await _context.MergedStudentData
      .Where(osc => osc.OfficialStudentId == officialStudent.OfficialStudentId && osc.StudyDays == studyDays)
      .ToListAsync();

            if (conflictingEnrollment.Any())
            {
                // Tạo một chuỗi để lưu trữ các ngày học trùng
                string conflictingStudyDays = string.Join(", ", conflictingEnrollment.Select(ce => ce.StudyDays));

                TempData["ErrorMessage"] = $"You have already enrolled in another course with the same study days: {conflictingStudyDays}.";
                ViewBag.Course = course;
                return View(course);
            }


            var mergedStudentData = new MergedStudentData
            {
                OfficialStudentId = officialStudent.OfficialStudentId,
                CoursesId = courseId,
                Telephone = telephone,
                StudyDays = studyDays,
                StudySession = studySession,
                EnrollmentStartDate = DateTime.Now
            };

            Match match = Regex.Match(course.CourseTime, @"(\d+)\s*months?");
            if (match.Success && match.Groups.Count > 1)
            {
                if (int.TryParse(match.Groups[1].Value, out int courseDurationInMonths))
                {
                    mergedStudentData.EnrollmentEndDate = mergedStudentData.EnrollmentStartDate.Value.AddMonths(courseDurationInMonths);
                }
                else
                {
                    var errorMessage = $"Invalid format for CourseTime: {course.CourseTime}. Unable to convert to months.";
                    TempData["ErrorMessage"] = errorMessage;
                    ViewBag.Course = course;
                    return View(course);
                }
            }
            else
            {
                var errorMessage = $"Invalid format for CourseTime: {course.CourseTime}. Unable to extract months.";
                TempData["ErrorMessage"] = errorMessage;
                ViewBag.Course = course;
                return View(course);
            }

            try
            {
                _context.MergedStudentData.Add(mergedStudentData);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "You have successfully enrolled in the course!";
                return RedirectToAction("Courses");
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = $"An error occurred while saving the data: {ex.InnerException?.Message}";
                Console.WriteLine(errorMessage);
                TempData["ErrorMessage"] = errorMessage;
                ViewBag.Course = course;
                return View(course);
            }
        }
        private async Task SendEnrollmentConfirmationEmail(string recipientEmail, string courseName)
        {
            try
            {
                // Đường dẫn tới mẫu email
                string emailTemplatePath = Path.Combine(_env.ContentRootPath, "Views", "Email", "EnrollmentConfirmation.cshtml");

                // Đọc nội dung mẫu email từ file
                string emailContent = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                // Thay thế các thẻ placeholder trong mẫu email bằng thông tin thích hợp
                emailContent = emailContent.Replace("{courseName}", courseName);

                // Tạo đối tượng MailMessage
                var message = new MailMessage();
                message.To.Add(new MailAddress(recipientEmail)); // Địa chỉ email của sinh viên
                message.From = new MailAddress(_configuration["EmailSettings:Username"]);
                message.Subject = "Confirmed successful course registration!";
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

        public async Task<IActionResult> coursesDetails(int id)
        {
            // Tìm kiếm sự kiện theo ID
            var selectedcourses = await _context.Courses.FindAsync(id);

            if (selectedcourses == null)
            {
                return NotFound();
            }

            return View(selectedcourses);
        }
        public async Task<IActionResult> Teacher(int? page, int pageSize = 9)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            var lecturersQuery = _context.Teachers.Include(t => t.Department).AsQueryable();
            var paginatedLecturer = await lecturersQuery
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalLecturer = await lecturersQuery.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalLecturer / pageSize);
            ViewBag.TotalEvent = totalLecturer;
            ViewBag.PageSize = pageSize;
            return View(paginatedLecturer);
        }
        public async Task<IActionResult> TeacherDetails(int id)
        {
            // Tìm kiếm giáo viên theo ID, bao gồm thông tin về phòng ban
            var selectedTeacher = await _context.Teachers.Include(t => t.Department).FirstOrDefaultAsync(t => t.TeachersId == id);

            if (selectedTeacher == null)
            {
                return NotFound();
            }

            return View(selectedTeacher);
        }
        public async Task<IActionResult> Blog()
        {
            return View();
        }
        public async Task<IActionResult> BlogDetails()
        {
            return View();
        }
        public async Task<IActionResult> Events(int? page, int pageSize = 9)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Truy vấn dữ liệu sự kiện từ cơ sở dữ liệu
            var eventsQuery = _context.Events.AsQueryable();
            var paginatedEvent = await eventsQuery
                                          .Skip((pageNumber - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            // Lấy tổng số bài đăng sau khi áp dụng các tiêu chí lọc
            int totalEvent = await eventsQuery.CountAsync();

            // Chuyển thông tin phân trang vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalEvent / pageSize);
            ViewBag.TotalEvent = totalEvent;
            ViewBag.PageSize = pageSize;
            // Truyền danh sách sự kiện vào view để hiển thị
            return View(paginatedEvent);
        }

        public async Task<IActionResult> EventsDetail(int id)
        {
            // Tìm kiếm sự kiện theo ID
            var selectedEvent = await _context.Events.FindAsync(id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            return View(selectedEvent);
        }

        public async Task<IActionResult> Contact()
        {
            return View();
        }
        public IActionResult Admission()
        {
            var majors = _context.Majors.ToList();
            ViewBag.Majors = majors;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SubmitAdmissionForm(StudentsInformation studentInfo)
        {
            if (true)
            {
                try
                {
                    // Lưu thông tin sinh viên vào cơ sở dữ liệu
                    _context.StudentsInformation.Add(studentInfo);
                    await _context.SaveChangesAsync();

                    // Tạo một đối tượng mới của model Registrations
                    var registration = new Registrations
                    {
                        StudentsInformationId = studentInfo.StudentsInformationId,
                        RegistrationStatus = "Pending",
                        UniqueCode = GenerateUniqueCode(),
                        RegistrationDate = DateTime.UtcNow // Gán ngày đăng ký là thời gian hiện tại

                    };

                    // Lưu thông tin đăng ký vào cơ sở dữ liệu
                    _context.Registrations.Add(registration);
                    await _context.SaveChangesAsync();

                    // Gửi email thông báo đến địa chỉ email mà người dùng nhập vào trong form
                    await SendAdmissionConfirmationEmail("dungprohn1409@gmail.com", registration.UniqueCode);

                    TempData["SuccessMessage"] = "Sign Up Success! The Code has been sent to your email address.";
                    return RedirectToAction("Admission", "Page");
                }
                catch (Exception ex)
                {
                    // Xử lý nếu có lỗi xảy ra trong quá trình lưu dữ liệu
                    TempData["ErrorMessage"] = "An error occurred while processing the request. Please try again later.";
                    // Log lỗi ex.Message
                    return RedirectToAction("Admission", "Page");
                }
            }

            // Nếu dữ liệu không hợp lệ, quay lại trang Admission để người dùng nhập lại
            return View("Admission", studentInfo);
        }



        // Phương thức tạo mã ngẫu nhiên duy nhất cho thuộc tính UniqueCode
        private string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private async Task SendAdmissionConfirmationEmail(string recipientEmail, string uniqueCode)
        {
            try
            {
                // Đường dẫn tới mẫu email
                string emailTemplatePath = Path.Combine(_env.ContentRootPath, "Views", "Email", "AdmissionConfirmation.cshtml");

                // Đọc nội dung mẫu email từ file
                string emailContent = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                // Thay thế các thẻ placeholder trong mẫu email bằng thông tin thích hợp
                emailContent = emailContent.Replace("{UniqueCode}", uniqueCode);

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

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return View();
            }
            else
            {
                string userRole = HttpContext.Session.GetString("Role");
                if (userRole == "User")
                {

                    return RedirectToAction("Home", "Page");
                }
                else if (userRole == "Admin")
                {
                    // Nếu là Admin, hiển thị thông báo lỗi "Email or password is incorrect"
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                    return View();
                }
            }
            // Trường hợp còn lại, chuyển hướng đến trang Home của User
            return RedirectToAction("Home", "Page");
        }

        [HttpPost]
        public IActionResult Login(Users user)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                Users u = _context.Users.FirstOrDefault(x => x.Email.Equals(user.Email));

                if (u != null && BCrypt.Net.BCrypt.Verify(user.Password, u.Password))
                {
                    if (u.Role == "Admin")
                    {
                        // Trường hợp đăng nhập thành công nhưng là Admin, hiển thị thông báo lỗi "Email or password is incorrect"
                        ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                        return View();
                    }
                    else if (u.Role == "User")
                    {
                        // Chỉ đặt Session khi thông tin đăng nhập hợp lệ
                        HttpContext.Session.SetString("Username", u.Username.ToString());
                        HttpContext.Session.SetString("Role", u.Role);
                        HttpContext.Session.SetString("UsersId", u.UsersId.ToString());

                        return RedirectToAction("Home", "Page"); // Chuyển hướng đến "Home", "Page" nếu là User
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                    return View();
                }
            }

            return RedirectToAction("Home", "Page");
        }




        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Username");
            return RedirectToAction("login", "Page");
        }
    }
}