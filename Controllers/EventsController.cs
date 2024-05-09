using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;
using OnlineCollegeManagement.Heplers;
using OnlineCollegeManagement.Models.Authentication;

namespace OnlineCollegeManagement.Controllers
{
    [Authentication]
    public class EventsController : Controller
    {

        private readonly CollegeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public EventsController(CollegeManagementContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
        }

        public async Task<IActionResult> Event(int? page, string EventTitle = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int pageSize = 10)
        {
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1 nếu không có page được cung cấp

            // Truy xuất dữ liệu sự kiện từ nguồn dữ liệu
            var eventsQuery = _context.Events.AsQueryable();

            // Áp dụng các tiêu chí lọc nếu chúng được cung cấp
            if (!string.IsNullOrEmpty(EventTitle))
            {
                eventsQuery = eventsQuery.Where(b => b.EventTitle.Contains(EventTitle));
            }
            if (startDate != null)
            {
                eventsQuery = eventsQuery.Where(b => b.EventDate >= startDate);
            }

            if (endDate != null)
            {
                // Chú ý: Khi lọc theo ngày kết thúc, hãy thêm 1 ngày vào để bao gồm tất cả các bài đăng được đăng vào ngày kết thúc
                eventsQuery = eventsQuery.Where(b => b.EventDate < endDate.Value.AddDays(1));
            }


            if (!string.IsNullOrEmpty(search))
            {
                eventsQuery = eventsQuery.Where(b => b.EventTitle.Contains(search)
                                        || b.EventDescription.Contains(search));

            }

            // Phân trang danh sách bài đăng và sắp xếp theo thời gian gần nhất
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

            // Truyền dữ liệu sự kiện vào view thông qua mô hình hoặc ViewBag
            return View( paginatedEvent);
        }


        public IActionResult AddEvent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent(Events model, IFormFile EventImageUrl)
        {
            if (true)
            {
                if (EventImageUrl != null && EventImageUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Event");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + EventImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await EventImageUrl.CopyToAsync(stream);
                    }

                    model.EventImageUrl = "/images/Event/" + Path.GetFileName(imagePath);

                }

                model.EventDate = DateTime.Now;

                _context.Add(model); // Thêm sự kiện vào DbSet trong context
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction(nameof(Event)); // Chuyển hướng sau khi thêm thành công
            }

            // Nếu mô hình không hợp lệ, trở lại view AddEvent với mô hình hiện tại
            return View( model);
        }



        public async Task<IActionResult> EditEvent(int id)
        {
            var events = await _context.Events.FindAsync(id);

            if (events == null)
            {
                return NotFound();
            }

            return View( events);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, Events model, IFormFile EventImageUrl)
        {
            if (true)
            {
                // Kiểm tra sự tồn tại của sự kiện cần chỉnh sửa dựa trên id
                var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventsId == id);
                if (existingEvent == null)
                {
                    return NotFound(); // Trả về lỗi 404 nếu không tìm thấy sự kiện cần chỉnh sửa
                }

                if (EventImageUrl != null && EventImageUrl.Length > 0)
                {
                    // Nếu có hình ảnh mới được cung cấp, cập nhật hình ảnh
                    var uploadsFolder = Path.Combine("wwwroot", "images", "Event");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imagePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + "_" + EventImageUrl.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await EventImageUrl.CopyToAsync(stream);
                    }

                    // Cập nhật đường dẫn hình ảnh mới
                    existingEvent.EventImageUrl = "/images/Event/" + Path.GetFileName(imagePath);
                }

                // Cập nhật các trường dữ liệu khác của sự kiện
                existingEvent.EventTitle = model.EventTitle;
                existingEvent.EventDescription = model.EventDescription;
                existingEvent.EventDate = DateTime.Now; // Cập nhật ngày sự kiện nếu cần

                _context.Update(existingEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Event));
            }

            return View(model); // Trả về view với dữ liệu không hợp lệ nếu ModelState không hợp lệ
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> deleteEvent(int id)
        {
            var events = await _context.Events.FindAsync(id);

            if (events == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
            if (!string.IsNullOrEmpty(events.EventImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", events.EventImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Events.Remove(events);
            await _context.SaveChangesAsync();

            return RedirectToAction("Event", "Events");
        }

    }
}
