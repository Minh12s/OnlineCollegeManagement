﻿using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OnlineCollegeManagement.Models
{
    public class Classes
    {
        public int ClassesId { get; set; }
        public string ClassName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
 
        public virtual ICollection<OfficialStudentClasses> OfficialStudent { get; set; }

    }
}

