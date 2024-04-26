using System;
namespace OnlineCollegeManagement.Models
{
    public class Events
    {
        public int EventsId { get; set; }
        public string EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string EventImageUrl { get; set; }
        public string EventTitle { get; set; }
    }
}

