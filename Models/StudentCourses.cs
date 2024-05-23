using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineCollegeManagement.Models
{
    public class StudentCourses
    {
        public int StudentCoursesId { get; set; }
        public int OfficialStudentId { get; set; }
        public int CoursesId { get; set; }
        public string? Telephone { get; set; }
        public DateTime? EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public string? StudyDays { get; set; }
        public string? StudySession { get; set; }

        public OfficialStudent OfficialStudent { get; set; }
        public Courses Course { get; set; }
    }
}
