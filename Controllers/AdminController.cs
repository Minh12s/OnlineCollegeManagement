using Microsoft.AspNetCore.Mvc;

namespace OnlineCollegeManagement.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View("DashboardAdmin/Dashboard");
        }
        public IActionResult Student()
        {
            return View("StudentManagement/Student");
        }
      
        public IActionResult EditStudent()
        {
            return View("StudentManagement/EditStudent");
        }

      
        public IActionResult Course()
        {
            return View("CourseManagement/Course");
        }


        public IActionResult AddCourse()
        {
            return View("CourseManagement/AddCourse");
        }

        public IActionResult EditCourse()
        {
            return View("CourseManagement/EditCourse");
        }


        public IActionResult Lecturer()
        {
            return View("LecturerManagement/Lecturer");
        }

        public IActionResult AddLecturer()
        {
            return View("LecturerManagement/AddLecturer");
        }

        public IActionResult EditLecturer()
        {
            return View("LecturerManagement/EditLecturer");
        }

    }
}
