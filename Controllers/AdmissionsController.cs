using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using System.Net.Mail;
using System.Net;
using BCrypt.Net;


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
        public async Task<IActionResult> Admissions(int? page, string RegistrationStatus = null, string UniqueCode = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int pageSize = 5)
        {
            int pageNumber = page ?? 1;

            // Truy vấn dữ liệu sinh viên từ cơ sở dữ liệu
            var registrations = _context.Registrations.Include(r => r.StudentInformation).AsQueryable();


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

            // Truyền dữ liệu RegistrationStatus và Student vào view
            ViewBag.RegistrationStatus = registrationStatus;
            return View(student); // Trả về view "AdmissionsDetail" với dữ liệu sinh viên
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int StudentsInformationId, string status)
        {
            // Tìm thông tin đăng ký của sinh viên
            var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.StudentsInformationId == StudentsInformationId);

            if (registration == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái đăng ký
            registration.RegistrationStatus = status;
            _context.Update(registration);
            await _context.SaveChangesAsync();

            // Kiểm tra nếu trạng thái là "Admitted"
            if (status == "Admitted")
            {
                // Lấy thông tin sinh viên từ cơ sở dữ liệu
                var student = await _context.StudentsInformation.FirstOrDefaultAsync(s => s.StudentsInformationId == StudentsInformationId);
                // Tạo địa chỉ email từ StudentName và loại bỏ dấu cách
                string cleanedEmail = $"{student.StudentName.Replace(" ", "")}{new Random().Next(10000)}@gmail.com";

                // Tạo mật khẩu ngẫu nhiên cho sinh viên
                string originalPassword = GenerateRandomPassword();

                // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(originalPassword);

                // Tạo dữ liệu mới cho bảng Users
                var newUser = new Users
                {
                    Username = student.StudentName,
                    Email = cleanedEmail,
                    Password = hashedPassword,
                    Role = "User"
                };

                // Thêm dữ liệu mới vào bảng Users
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Tạo dữ liệu mới cho bảng OfficialStudent
                var officialStudent = new OfficialStudent
                {
                    StudentsInformationId = student.StudentsInformationId,
                    UsersId = newUser.UsersId,
                    StudentCode = GenerateRandomCode(),
                };

                // Thêm dữ liệu mới vào bảng OfficialStudent
                _context.OfficialStudents.Add(officialStudent);
                await _context.SaveChangesAsync();

                // Gửi email với mật khẩu nguyên thủy (không mã hóa)
                await SendAdmittedConfirmationEmail("", newUser.Username, newUser.Email, originalPassword);
            }

            // Chuyển hướng đến trang chi tiết đăng ký với StudentsInformationId tương ứng
            return RedirectToAction("Admissions", "Admissions", new { StudentsInformationId = StudentsInformationId });
        }

        // Phương thức để tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword()
        {
            // Các ký tự được chấp nhận cho mật khẩu
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            // Tạo mật khẩu ngẫu nhiên có độ dài 8 ký tự
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Phương thức để tạo mã sinh viên ngẫu nhiên
        private string GenerateRandomCode()
        {
            // Các ký tự được chấp nhận cho mã sinh viên
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            // Tạo mã sinh viên ngẫu nhiên có độ dài 8 ký tự
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private async Task SendAdmittedConfirmationEmail(string recipientEmail, string username, string email, string password)
        {
            try
            {
                // Đường dẫn tới mẫu email
                string emailTemplatePath = Path.Combine(_env.ContentRootPath, "Views", "Email", "AdmittedConfirmation.cshtml");

                // Đọc nội dung mẫu email từ file
                string emailContent = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

                // Thay thế các thẻ placeholder trong mẫu email bằng thông tin thích hợp
                emailContent = emailContent.Replace("{Username}", username);
                emailContent = emailContent.Replace("{Email}", email);
                emailContent = emailContent.Replace("{Password}", password); // Sử dụng mật khẩu không mã hóa

                // Tạo đối tượng MailMessage
                var message = new MailMessage();
                message.To.Add(new MailAddress(recipientEmail)); // Địa chỉ email của sinh viên
                message.From = new MailAddress(_configuration["EmailSettings:Username"]);
                message.Subject = "Confirmed successful admission!";
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