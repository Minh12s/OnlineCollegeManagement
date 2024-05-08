using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCollegeManagement.Data;
using OnlineCollegeManagement.Models;

namespace OnlineCollegeManagement.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly CollegeManagementContext _context;

        public SubjectsController(CollegeManagementContext context)
        {
            _context = context;
        }

        // GET: Subjects
        public async Task<IActionResult> Index(int? page, string searchString, int pageSize = 10)
        {
            int pageNumber = page ?? 1;

            // Lấy danh sách môn học từ cơ sở dữ liệu
            var subjects = _context.Subjects.AsQueryable();

            // Áp dụng tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                subjects = subjects.Where(s => s.SubjectName.Contains(searchString)
                                            || s.SubjectCode.Contains(searchString));
            }

            // Phân trang danh sách môn học
            var paginatedSubjects = await subjects.Skip((pageNumber - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .ToListAsync();

            // Lấy tổng số môn học
            int totalSubjects = await subjects.CountAsync();

            // Chuyển thông tin phân trang và các thông tin khác vào ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalSubjects / pageSize);
            ViewBag.TotalSubjects = totalSubjects;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchString = searchString;

            return View(paginatedSubjects);
        }


       
        // GET: Subjects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subjects model)
        {
            // Kiểm tra xem SubjectCode đã tồn tại trong cơ sở dữ liệu chưa
            var existingSubject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectCode == model.SubjectCode);

            if (existingSubject != null)
            {
                // Nếu đã tồn tại môn học với SubjectCode tương tự trong cơ sở dữ liệu, hiển thị thông báo lỗi
                ModelState.AddModelError("SubjectCode", "This subject already exists");
                return View(model);
            }

            // Nếu không có môn học nào có cùng SubjectCode, thêm môn học mới vào cơ sở dữ liệu
            if (true)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Subjects == null)
            {
                return NotFound();
            }

            var subjects = await _context.Subjects.FindAsync(id);
            if (subjects == null)
            {
                return NotFound();
            }
            return View(subjects);
        }

        // POST: Subjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subjects model)
        {
            // Kiểm tra xem id có tồn tại trong cơ sở dữ liệu hay không
            var existingEvent = await _context.Subjects.FirstOrDefaultAsync(e => e.SubjectsId == id);

            if (existingEvent == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy môn học cần chỉnh sửa
            }

            // Kiểm tra xem có môn học khác trong cơ sở dữ liệu đã có cùng SubjectCode nhưng khác với môn học đang được chỉnh sửa hay không
            var otherSubjectWithSameCode = await _context.Subjects.FirstOrDefaultAsync(e => e.SubjectCode == model.SubjectCode && e.SubjectsId != id);

            if (otherSubjectWithSameCode != null)
            {
                // Nếu đã tồn tại môn học khác có cùng SubjectCode, hiển thị thông báo lỗi
                ModelState.AddModelError("SubjectCode", "This subject already exists");
                return View(model);
            }

            // Cập nhật thông tin môn học chỉnh sửa
            if (ModelState.IsValid)
            {
                existingEvent.SubjectCode = model.SubjectCode;
                existingEvent.SubjectName = model.SubjectName;

                _context.Update(existingEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model); // Trả về view với dữ liệu không hợp lệ nếu ModelState không hợp lệ
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> delete(int id)
        {
            var Subject = await _context.Subjects.FindAsync(id);

            if (Subject == null)
            {
                return NotFound();
            }

            // Xóa ảnh đại diện của blog khỏi thư mục
           

            _context.Subjects.Remove(Subject);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang BlogManagement/Blog
            return RedirectToAction("Index", "Subjects");
        }

      
    }
}
