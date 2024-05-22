using System;
namespace OnlineCollegeManagement.Models
{
    public class ContactInfo
    {
        public int ContactInfoId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public DateTime ContactDate { get; set; }
    }

}

