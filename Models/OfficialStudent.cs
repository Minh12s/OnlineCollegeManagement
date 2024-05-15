using System;

namespace OnlineCollegeManagement.Models
{
    public class OfficialStudent
    {
        public int OfficialStudentId { get; set; }
        public int StudentsInformationId { get; set; }
        public int? CoursesId { get; set; }
        
        public int UsersId { get; set; }
        public string StudentCode { get; set; }
        public string? Telephone { get; set; }
        public DateTime? EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public string? StudyDays { get; set; } 
        public string? StudySession { get; set; } // Ví dụ: "Morning" hoặc "Afternoon"

        public StudentsInformation StudentInformation { get; set; }
        public Users User { get; set; }
        public Courses Course { get; set; }
        public virtual ICollection<OfficialStudentClasses> Classes { get; set; }

    }
}

