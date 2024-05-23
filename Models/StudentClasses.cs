using System;
using System.ComponentModel.DataAnnotations;
namespace OnlineCollegeManagement.Models
{
	public class StudentClasses
	{
        [Key]
        public int StudentCoursesId { get; set; }
        [Key]
        public int? ClassesId { get; set; }
        public DateTime? ClassStartDate { get; set; }
        public DateTime? ClassEndDate { get; set; }
        public string? StudentStatus { get; set; }
        public int? DeleteStatus { get; set; }

        public OfficialStudent OfficialStudent { get; set; }
        public Courses Course { get; set; }
        public Classes Classes { get; set; }
    }
}

