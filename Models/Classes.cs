using System;
namespace OnlineCollegeManagement.Models
{
    public class Classes
    {
        public int ClassesId { get; set; }
        public int CoursesId { get; set; }
        public string ClassName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Courses Course { get; set; }
    }
}

