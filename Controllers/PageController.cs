using Microsoft.AspNetCore.Mvc;

namespace OnlineCollegeManagement.Controllers
{
    public class PageController : Controller
    {
        public async Task<IActionResult> Home()
        {
            return View();
        }
        public async Task<IActionResult> About()
        {
            return View();
        }
        public async Task<IActionResult> Courses()
        {
            return View();
        }
        public async Task<IActionResult> Teacher()
        {
            return View();
        }
        public async Task<IActionResult> TeacherDetails()
        {
            return View();
        }
        public async Task<IActionResult> Blog()
        {
            return View();
        }
        public async Task<IActionResult> BlogDetails()
        {
            return View();
        }
        public async Task<IActionResult> Events()
        {
            return View();
        }
        public async Task<IActionResult> Contact()
        {
            return View();
        }
    }
}
