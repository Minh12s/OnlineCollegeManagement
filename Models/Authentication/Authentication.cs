using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlineCollegeManagement.Models.Authentication
{
    public class Authentication : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("Username") == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Controller","Page"},
                        {"Action","login" }
                    });
            }
            else
            {
                string userRole = context.HttpContext.Session.GetString("Role");

                // Kiểm tra nếu đang cố gắng chuyển hướng từ trang Admin
                bool isRedirectedFromAdmin = context.HttpContext.Request.Headers["Referer"].ToString().Contains("Admin/dashboard");

                // Lấy danh sách các controller mà cần kiểm tra quyền
                string[] adminControllers = {
                    "Admin", "Courses", "Classes" , "Admissions", "Achievements","Student","Schedule",
                    "Departments", "Events", "Facilities" , "Majors" ,"Subjects","Teachers"
                };

                // Lấy danh sách các controller mà admin không được phép truy cập
                string[] adminRestrictedControllers = {
                    // Danh sách các controller mà admin không được phép truy cập
                    "MyTranscript"
                };

                // Kiểm tra xem controller hiện tại có trong danh sách các controller admin hay không
                bool isAdminController = Array.Exists(adminControllers, controller => controller.Equals(context.RouteData.Values["Controller"].ToString()));

                // Kiểm tra xem controller hiện tại có trong danh sách các controller admin bị hạn chế hay không
                bool isAdminRestrictedController = Array.Exists(adminRestrictedControllers, controller => controller.Equals(context.RouteData.Values["Controller"].ToString()));

                if (userRole == "Admin" && isRedirectedFromAdmin && !isAdminController)
                {
                    // Nếu có cố gắng truy cập trái phép vào trang Admin, trả về mã lỗi 404
                    context.Result = new NotFoundResult();
                }
                else if (userRole == "Admin" && isAdminRestrictedController)
                {
                    // Nếu admin cố gắng truy cập vào controller bị hạn chế, trả về mã lỗi 404
                    context.Result = new NotFoundResult();
                }
                else if (userRole == "User" && isAdminController)
                {
                    // Nếu người dùng có vai trò là "User" cố gắng truy cập phần "Admin", trả về mã lỗi 404
                    context.Result = new NotFoundResult();
                }
            }
        }
    }
}
