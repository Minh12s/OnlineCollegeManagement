using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OnlineCollegeManagement.Models
{
    public class Classes
    {
        public int ClassesId { get; set; }
        public string ClassName { get; set; }
        public DateTime ClassStartDate { get; set; }
        public DateTime ClassEndDate { get; set; }


    }
}

