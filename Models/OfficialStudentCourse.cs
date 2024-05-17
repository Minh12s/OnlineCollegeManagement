using System.ComponentModel.DataAnnotations;

namespace OnlineCollegeManagement.Models
{
    public class OfficialStudentCourse
    {
        [Key]

        public int OfficialStudentId { get; set; }
        [Key]

        public int CoursesId { get; set; }
        public string Telephone { get; set; }
        public DateTime EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public string StudyDays { get; set; }
        public string StudySession { get; set; }
        public OfficialStudent OfficialStudent { get; set; }
        public Courses Course { get; set; }
    }
}
