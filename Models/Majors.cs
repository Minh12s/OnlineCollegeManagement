using System;
namespace OnlineCollegeManagement.Models
{
    public class Majors
    {
        public int MajorsId { get; set; }
        public string MajorName { get; set; }
        public int DepartmentsId { get; set; }

        public Departments Department { get; set; }
    }
}

