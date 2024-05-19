using System;
namespace OnlineCollegeManagement.Models
{
    public class StudentCourseViewModel
    {
        public int OfficialStudentId { get; set; }
        public int CoursesId { get; set; }
        public string StudentCode { get; set; }
        public string Telephone { get; set; }
        public string ClassName { get; set; }
        public DateTime EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public string StudyDays { get; set; }
        public string StudySession { get; set; }
        public string CourseName { get; set; }
    }
}


