using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OnlineCollegeManagement.Models
{
	public class OfficialStudentClasses
	{
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StudentStatus { get; set; }
        public Classes Classes { get; set; }
        public OfficialStudent OfficialStudent { get; set; }
    }
}

