using System;
namespace OnlineCollegeManagement.Models
{
    public class ClassSchedules
    {
        public int ClassSchedulesId { get; set; }
        public int ClassesId { get; set; }
        public int CoursesId { get; set; }
        public string SubjectName { get; set; }
        public string? StudyDays { get; set; }
        public string? StudySession { get; set; }
        public DateTime SchedulesDate{ get; set; }
       

        public Courses Course { get; set; }
        public Classes Class { get; set; }
    }
}

