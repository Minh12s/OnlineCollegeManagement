using System;
namespace OnlineCollegeManagement.Models
{
    public class Courses
    {
        public int CoursesId { get; set; }
        public string CourseName { get; set; }
        public string CourseTime { get; set; }
        public string Description { get; set; }
        public string CoursesImageUrl { get; set; }
        public int MajorsId { get; set; }
        public int TeachersId { get; set; }

        public Majors Major { get; set; }
        public Teachers Teacher { get; set; }

    }
}
