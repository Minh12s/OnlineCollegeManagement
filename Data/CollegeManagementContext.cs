using System;
using OnlineCollegeManagement.Models;
using OnlineCollegeManagement.Data;
using Microsoft.EntityFrameworkCore;


namespace OnlineCollegeManagement.Data
{
    public class CollegeManagementContext : DbContext
    {
        public CollegeManagementContext(DbContextOptions<CollegeManagementContext> options)
           : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Majors> Majors { get; set; }
        public DbSet<Teachers> Teachers { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Classes> Classes { get; set; }
        public DbSet<StudentsInformation> StudentsInformation { get; set; }
        public DbSet<OfficialStudent> OfficialStudents { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<ClassSchedules> ClassSchedules { get; set; }
        public DbSet<ExamScores> ExamScores { get; set; }
        public DbSet<Registrations> Registrations { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<Achievements> Achievements { get; set; }
        public DbSet<Facilities> Facilities { get; set; }
        public DbSet<ContactInfo> ContactInfo { get; set; }

   
    }
}


