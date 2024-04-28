using System;

namespace OnlineCollegeManagement.Models
{
    public class StudentsInformation
    {
        public int StudentsInformationId { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string CurrentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public int MajorsId { get; set; } 
        public string? SchoolName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string Center { get; set; }
        public string AcademicMajor { get; set; }
        public string? FieldOfStudy { get; set; }
        public decimal MarksObtained { get; set; }
        public decimal TotalMarks { get; set; }
        public string? Grade { get; set; }
        public string? SportsInfo { get; set; }
        public Majors Major { get; set; }
    }
}
