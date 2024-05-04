using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OnlineCollegeManagement.Models
{
    public class Subjects
    {
        public int SubjectsId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }

        public virtual ICollection<Courses> Courses { get; set; }

    }
}

