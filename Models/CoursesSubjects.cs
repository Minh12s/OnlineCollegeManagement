using System.ComponentModel.DataAnnotations;

namespace OnlineCollegeManagement.Models
{
    public class CoursesSubjects
    {
        // Composite primary key
        [Key]
        public int CoursesId { get; set; }

        [Key]
        public int SubjectsId { get; set; }

        // Navigation properties
        public Courses Course { get; set; }
        public Subjects Subject { get; set; }
    }
}
