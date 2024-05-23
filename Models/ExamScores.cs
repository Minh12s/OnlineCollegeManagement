using System;
namespace OnlineCollegeManagement.Models
{
    public class ExamScores
    {
        public int ExamScoresId { get; set; }
        public int OfficialStudentId { get; set; }
        public int CoursesId { get; set; }
        public int ClassesId { get; set; }
        public string SubjectName { get; set; }
        public string Status { get; set; }
        public decimal Score { get; set; }

        public OfficialStudent OfficialStudent { get; set; }
        public Courses Course { get; set; }
        public Classes Classes { get; set; }
    }
}

