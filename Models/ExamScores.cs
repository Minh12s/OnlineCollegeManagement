using System;
namespace OnlineCollegeManagement.Models
{
    public class ExamScores
    {
        public int ExamScoresId { get; set; }
        public int OfficialStudentId { get; set; }
        public int SubjectsId { get; set; }
        public int CoursesId { get; set; }

        public string Status { get; set; }
        public decimal? Score { get; set; } // Allow null values

        public OfficialStudent OfficialStudent { get; set; }
        public Courses Course { get; set; }

        public Subjects Subject { get; set; }

    }
}

