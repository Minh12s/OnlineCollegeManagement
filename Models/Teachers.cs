using System;
namespace OnlineCollegeManagement.Models
{
    public class Teachers
    {
        public int TeachersId { get; set; }
        public string TeacherName { get; set; }
        public int DepartmentsId { get; set; }
        public DateTime JoinDate { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public Departments Department { get; set; }
    }
}

