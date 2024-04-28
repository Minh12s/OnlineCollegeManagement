using System;

namespace OnlineCollegeManagement.Models
{
    public class Achievements
    {
        public int AchievementsId { get; set; }
        public string AchievementDescription { get; set; }
        public string AchievementImageUrl { get; set; }
        public DateTime AchievementDate { get; set; }
        public string AchievementTitle { get; set; }
    }
}
