using System;
namespace OnlineCollegeManagement.Models
{
    public class ClassSchedules
    {
        public int ClassSchedulesId { get; set; }
        public int ClassesId { get; set; }
        public int SubjectsId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Subjects Subject { get; set; }
        public Classes Class { get; set; }
    }
}

