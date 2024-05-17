﻿using System;

namespace OnlineCollegeManagement.Models
{
    public class OfficialStudent
    {
        public int OfficialStudentId { get; set; }
        public int StudentsInformationId { get; set; }

        public int UsersId { get; set; }
        public string StudentCode { get; set; }

        public StudentsInformation StudentInformation { get; set; }
        public Users User { get; set; }
        public virtual ICollection<OfficialStudentClasses> Classes { get; set; }

    }
}

