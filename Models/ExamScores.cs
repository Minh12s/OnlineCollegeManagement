using System;
namespace OnlineCollegeManagement.Models
{
    public class ExamScores
    {
        public int ExamScoresId { get; set; }
        public int OfficialStudentId { get; set; }
        public int SubjectsId { get; set; }
        public DateTime ExamDate { get; set; }
        public decimal Score { get; set; }

        public OfficialStudent OfficialStudent { get; set; }
        public Subjects Subject { get; set; }
    }
}

