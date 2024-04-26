using System;
namespace OnlineCollegeManagement.Models
{
    public class Subjects
    {
        public int SubjectsId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int CoursesId { get; set; }

        public Courses Course { get; set; }
    }
}

