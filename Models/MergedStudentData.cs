using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineCollegeManagement.Models
{
    public class MergedStudentData
    {
        [Key]
        public int OfficialStudentId { get; set; }
        [Key]
        public int CoursesId { get; set; }

        public int? ClassesId { get; set; }
        public string? Telephone { get; set; }
        public DateTime? EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public string? StudyDays { get; set; }
        public string? StudySession { get; set; }
        public DateTime? ClassStartDate { get; set; }
        public DateTime? ClassEndDate { get; set; }
        public string? StudentStatus { get; set; }
        public int? DeleteStatus { get; set; }

        public OfficialStudent OfficialStudent { get; set; }
        public Courses Course { get; set; }
        public Classes Classes { get; set; }
    }
}
