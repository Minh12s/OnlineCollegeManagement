using System;
namespace OnlineCollegeManagement.Models
{
    public class Registrations
    {
        public int RegistrationsId { get; set; }
        public int StudentsInformationId { get; set; }
        public string RegistrationStatus { get; set; }
        public string UniqueCode { get; set; }
        public DateTime RegistrationDate { get; set; }



        public StudentsInformation StudentInformation { get; set; }
    }
}

