using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.DependencyResolver;
using BCrypt.Net;
using OnlineCollegeManagement.Models;
using System.Collections;
using System.Net.NetworkInformation;

namespace OnlineCollegeManagement.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new CollegeManagementContext(
                serviceProvider.GetRequiredService<DbContextOptions<CollegeManagementContext>>()))
            {
                // Look for any existing data
                if (context.Users.Any() || context.Departments.Any() || context.Majors.Any() ||
                    context.Teachers.Any() || context.Courses.Any() || context.Classes.Any() ||
                    context.StudentsInformation.Any() || context.OfficialStudents.Any() ||
                    context.Subjects.Any() || context.ClassSchedules.Any() || context.ExamScores.Any() ||
                    context.Registrations.Any() || context.Events.Any() || context.Achievements.Any() ||
                    context.Facilities.Any() || context.ContactInfo.Any() || context.CoursesSubjects.Any() || context.MergedStudentData.Any())
                {
                    return; // Database has been seeded
                }

                // Seed data for Users
                var users = new Users[]
                {
                    new Users { Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Email="admin@gmail.com", Role = "Admin" },
                    new Users { Username = "user1", Password = BCrypt.Net.BCrypt.HashPassword("12345678"), Email="user1gmail.com",Role = "User" },
                    new Users { Username = "user2", Password = BCrypt.Net.BCrypt.HashPassword("12345678"), Email="user2@gmail.com",Role = "User" }
                };
                foreach (var user in users)
                {
                    context.Users.Add(user);
                }
                context.SaveChanges();

                // Seed data for Departments
                var departments = new Departments[]
                {
                  new Departments { DepartmentName = "Computer Science" },
                  new Departments { DepartmentName = "Mathematics" },
                  new Departments { DepartmentName = "Physics" },
                  new Departments { DepartmentName = "Information Technology" },
                  new Departments { DepartmentName = "Biology" },
                  new Departments { DepartmentName = "Engineering" },
                  new Departments { DepartmentName = "Literature" },
                  new Departments { DepartmentName = "History" },
                  new Departments { DepartmentName = "Economics" },
                  new Departments { DepartmentName = "Psychology" }
                };
                foreach (var department in departments)
                {
                    context.Departments.Add(department);
                }
                context.SaveChanges();

                // Seed data for Majors
                var majorNames = new List<string> { "Software Engineering", "Data Science", "Applied Mathematics", "Computer Engineering", "Electrical Engineering" };
                var departments1 = context.Departments.ToList();

                var majors = new List<Majors>();
                foreach (var name in majorNames)
                {
                    // Chọn phòng ban theo thứ tự từ danh sách đã có
                    var departmentId = departments1[majors.Count % departments1.Count].DepartmentsId;
                    var major = new Majors { MajorName = name, DepartmentsId = departmentId };
                    majors.Add(major);
                }

                foreach (var major in majors)
                {
                    context.Majors.Add(major);
                }
                context.SaveChanges();


                // Seed data for Teachers
                var random = new Random();
                var departments2 = context.Departments.ToList();
                var thumbnailPaths2 = Enumerable.Range(1, 9).Select(i => $"/images/person_{i}.jpg").ToList();
                var teachers = new List<Teachers>
{
    new Teachers {
        TeacherName = "John Doe",
        Description = "Experienced Mathematics Teacher with a passion for teaching algebra, geometry, and calculus.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "Jane Smith",
        Description = "Dedicated English Teacher with a focus on literature, writing, and grammar skills development.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "David Brown",
        Description = "Innovative Science Teacher specializing in biology, chemistry, and physics education.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "Michael Johnson",
        Description = "Passionate History Teacher dedicated to fostering critical thinking and historical analysis skills.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "Emily Wilson",
        Description = "Creative Art Teacher inspiring students to explore various art forms and express themselves creatively.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "James Anderson",
        Description = "Dynamic Physical Education Teacher promoting physical fitness, teamwork, and sportsmanship.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "Sarah Martinez",
        Description = "Motivated Foreign Language Teacher fostering language proficiency and cultural understanding.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "Robert Taylor",
        Description = "Tech-Savvy Computer Science Teacher introducing students to coding, programming, and computer skills.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "Jennifer Lee",
        Description = "Enthusiastic Music Teacher nurturing musical talent and appreciation through performance and theory.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    },
    new Teachers {
        TeacherName = "William Clark",
        Description = "Adaptive Special Education Teacher providing personalized support and resources for students with diverse learning needs.",
        DepartmentsId = departments[random.Next(departments2.Count)].DepartmentsId,
        ImageUrl = thumbnailPaths2[random.Next(thumbnailPaths2.Count)],
        JoinDate = DateTime.Now
    }
};

                context.Teachers.AddRange(teachers);
                context.SaveChanges();

                // Seed data for Subjects
                for (int i = 0; i < 10; i++)
                {
                    var randomNumberOfSessions = random.Next(1, 21); // Số ngẫu nhiên từ 1 đến 20
                    var subject = new Subjects
                    {
                        SubjectCode = GenerateRandomSubjectCode(),
                        SubjectName = GenerateRandomSubjectName(),
                        NumberOfSessions = randomNumberOfSessions,
                    };

                    // Thêm môn học vào danh sách
                    context.Subjects.Add(subject);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                context.SaveChanges();


                // Hàm sinh mã ngẫu nhiên cho môn học
                string GenerateRandomSubjectCode()
                {
                    // Tạo một chuỗi ngẫu nhiên cho mã môn học
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    var subjectCode = new string(Enumerable.Repeat(chars, 6)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    return subjectCode;
                }

                // Hàm sinh tên ngẫu nhiên cho môn học
                string GenerateRandomSubjectName()
                {
                    // Danh sách các tên môn học có thể chọn
                    string[] subjectNames = { "Introduction to Programming", "Calculus", "Newtonian Mechanics", "Data Structures", "Organic Chemistry", "Artificial Intelligence", "World History", "English Literature", "Statistics", "Economics" };

                    // Lấy ngẫu nhiên một tên môn học từ danh sách
                    var random = new Random();
                    var subjectName = subjectNames[random.Next(subjectNames.Length)];
                    return subjectName;
                }
                var majors1 = context.Majors.ToList();
                var teachers1 = context.Teachers.ToList();
                var thumbnailPaths4 = Enumerable.Range(1, 6).Select(i => $"/images/course-{i}.jpg").ToList();
                var courseNames = new List<string> { "Web Development", "Database Management", "Machine Learning", "Software Testing", "Computer Graphics", "Network Security", "Digital Signal Processing", "Operating Systems", "Data Mining", "Artificial Intelligence", "Computer Vision", "Mobile Application Development", "Information Security", "Cloud Computing", "Data Analysis", "Game Development", "Embedded Systems", "Robotics", "Human-Computer Interaction", "IoT Development", "Natural Language Processing", "Quantum Computing", "Blockchain Technology", "Virtual Reality", "Design microchips" };
                var CourseTime = new List<string> { "Six months", "one year", "two year", "three year" };

                var courses = new List<Courses>();

                int courseIndex = 0; // Biến đếm tên khóa học đã sử dụng
                foreach (var major in majors1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var courseName = courseNames[courseIndex % courseNames.Count]; // Sử dụng toán tử modulo để lặp lại từ đầu danh sách
                        var randomTeacherId = teachers1[random.Next(teachers1.Count)].TeachersId;
                        var randomCourseTime = CourseTime[random.Next(CourseTime.Count)];

                        var course = new Courses
                        {
                            CourseName = courseName,
                            Description = $"Course description for {courseName}",
                            MajorsId = major.MajorsId,
                            CourseTime = randomCourseTime,
                            CoursesImageUrl = thumbnailPaths4[random.Next(thumbnailPaths4.Count)],
                            TeachersId = randomTeacherId,
                        };
                        courses.Add(course);

                        courseIndex++;
                    }
                }

                foreach (var course in courses)
                {
                    context.Courses.Add(course);
                }
                context.SaveChanges();



                // Seed data for Classes
                var classNames = new List<string> { "T2210A", "T2310A", "T2210A", "T1210A", "T2210A", "T1230A", "T2610A", "T72210A", "T3210A", "T1110A" };

                var classes = new List<Classes>();
                foreach (var className in classNames)
                {
                    // Randomly generate class names based on the classNames list
                    var randomClassName = $"{className}{random.Next(101, 301)}";

                    var classObj = new Classes
                    {
                        ClassName = randomClassName,
                        ClassStartDate = DateTime.Now,
                        ClassEndDate = DateTime.Now.AddMonths(3)
                    };
                    classes.Add(classObj);
                }

                foreach (var classObj in classes)
                {
                    context.Classes.Add(classObj);
                }
                context.SaveChanges();



                // Seed data for StudentsInformation
                var majors2 = context.Majors.ToList();

                var studentsInformation = new StudentsInformation[]
                {
                  new StudentsInformation {
                       StudentName = "Alice",
                       FatherName = "John",
                       MotherName = "Mary",
                       DateOfBirth = new DateTime(2000, 1, 1),
                       Gender = "Female",
                       CurrentAddress = "123 Main St",
                       PermanentAddress = "456 Oak St",
                       MajorsId = majors[random.Next(majors2.Count)].MajorsId, // Random MajorId từ danh sách majors
                       SchoolName = "ABC High School",
                       RegistrationNumber = "123456",
                       Center = "Center A",
                       AcademicMajor = "Computer Science",
                       FieldOfStudy = "Web Development",
                       MarksObtained = 85.5m,
                       TotalMarks = 100m,
                       Grade = "A",
                       SportsInfo = "Basketball player"
                },
    new StudentsInformation {
        StudentName = "Bob",
        FatherName = "Mike",
        MotherName = "Susan",
        DateOfBirth = new DateTime(2001, 2, 2),
        Gender = "Male",
        CurrentAddress = "789 Elm St",
        PermanentAddress = "101 Pine St",
        MajorsId = majors[random.Next(majors2.Count)].MajorsId, // Random MajorId từ danh sách majors
        SchoolName = "XYZ High School",
        RegistrationNumber = "654321",
        Center = "Center B",
        AcademicMajor = "Data Science",
        FieldOfStudy = "Data Analysis",
        MarksObtained = 78.2m,
        TotalMarks = 100m,
        Grade = "B+",
        SportsInfo = "Football player"
    },
    new StudentsInformation {
        StudentName = "Charlie",
        FatherName = "David",
        MotherName = "Linda",
        DateOfBirth = new DateTime(1999, 5, 15),
        Gender = "Male",
        CurrentAddress = "456 Pine St",
        PermanentAddress = "789 Elm St",
        MajorsId = majors[random.Next(majors2.Count)].MajorsId, // Random MajorId từ danh sách majors
        SchoolName = "DEF High School",
        RegistrationNumber = "987654",
        Center = "Center C",
        AcademicMajor = "Electrical Engineering",
        FieldOfStudy = "Power Systems",
        MarksObtained = 92.7m,
        TotalMarks = 100m,
        Grade = "A+",
        SportsInfo = "Tennis player"
    },
    new StudentsInformation {
        StudentName = "Diana",
        FatherName = "James",
        MotherName = "Emily",
        DateOfBirth = new DateTime(1998, 8, 20),
        Gender = "Female",
        CurrentAddress = "789 Oak St",
        PermanentAddress = "101 Pine St",
        MajorsId = majors[random.Next(majors2.Count)].MajorsId, // Random MajorId từ danh sách majors
        SchoolName = "GHI High School",
        RegistrationNumber = "135798",
        Center = "Center D",
        AcademicMajor = "Mechanical Engineering",
        FieldOfStudy = "Thermodynamics",
        MarksObtained = 88.9m,
        TotalMarks = 100m,
        Grade = "A",
        SportsInfo = "Soccer player"
    },
    new StudentsInformation {
        StudentName = "Eva",
        FatherName = "Peter",
        MotherName = "Sophia",
        DateOfBirth = new DateTime(2002, 4, 10),
        Gender = "Female",
        CurrentAddress = "101 Elm St",
        PermanentAddress = "456 Oak St",
        MajorsId = majors[random.Next(majors2.Count)].MajorsId, // Random MajorId từ danh sách majors
        SchoolName = "JKL High School",
        RegistrationNumber = "246810",
        Center = "Center E",
        AcademicMajor = "Civil Engineering",
        FieldOfStudy = "Structural Engineering",
        MarksObtained = 79.5m,
        TotalMarks = 100m,
        Grade = "B",
        SportsInfo = "Swimming player"
    },
                };
                foreach (var studentInfo in studentsInformation)
                {
                    context.StudentsInformation.Add(studentInfo);
                }
                context.SaveChanges();



                // Seed data for OfficialStudents
                var studentsInformationIds = context.StudentsInformation.Select(s => s.StudentsInformationId).ToList();
                var users1 = context.Users.Select(u => u.UsersId).ToList();
                var courses1 = context.Courses.Select(c => c.CoursesId).ToList();

                var officialStudents = new List<OfficialStudent>
                {
                    new OfficialStudent
                    {
                        StudentCode = GenerateRandomStudentCode(),
                        StudentsInformationId = studentsInformationIds[random.Next(studentsInformationIds.Count)],
                        UsersId = users1[random.Next(users1.Count)]
                    },
                    new OfficialStudent
                    {
                        StudentCode = GenerateRandomStudentCode(),
                        StudentsInformationId = studentsInformationIds[random.Next(studentsInformationIds.Count)],
                        UsersId = users1[random.Next(users1.Count)]
                    }
                };

                context.OfficialStudents.AddRange(officialStudents);
                context.SaveChanges();


                // Function to generate random student code
                string GenerateRandomStudentCode()
                {
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    return new string(Enumerable.Repeat(chars, 8)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                }


                // Seed data for ClassSchedules
                var classes2 = context.Classes.ToList();
                var courses3 = context.Courses.ToList();

                for (int i = 0; i < 10; i++)
                {
                    var randomClassId = classes2[random.Next(classes2.Count)].ClassesId;
                    var randomCoursesId = courses3[random.Next(courses3.Count)].CoursesId;

                    var newSchedule = new ClassSchedules
                    {
                        ClassesId = randomClassId,
                        CoursesId = randomCoursesId,
                        StudyDays = "Tuesday, Thursday, Saturday", // Ví dụ: Học vào thứ 3, 5, 7
                        StudySession = "Afternoon",// Ví dụ: Ca học buổi chiều
                        SchedulesDate = DateTime.Now.AddHours(random.Next(24)),
                        SubjectName = "IT"
                    };

                    context.ClassSchedules.Add(newSchedule);
                }

                context.SaveChanges();


                // Seed data for ExamScores
                var officialStudent = context.OfficialStudents.ToList();
                var course3 = context.Courses.ToList();
                var classes3 = context.Classes.ToList();


                var examScores = new ExamScores[]
                {
                  new ExamScores {
                      OfficialStudentId = officialStudent[random.Next(officialStudent.Count)].OfficialStudentId,
                      CoursesId = course3[random.Next(course3.Count)].CoursesId,
                      ClassesId = classes3[random.Next(classes3.Count)].ClassesId,
                   
                      SubjectName = "IT",
                      Score = 90
                  },
                    new ExamScores {
                      OfficialStudentId = officialStudent[random.Next(officialStudent.Count)].OfficialStudentId,
                      CoursesId = course3[random.Next(course3.Count)].CoursesId,
                      ClassesId = classes3[random.Next(classes3.Count)].ClassesId,
                     
                      SubjectName = "IT",
                      Score = 50
                  },
                      new ExamScores {
                      OfficialStudentId = officialStudent[random.Next(officialStudent.Count)].OfficialStudentId,
                      CoursesId = course3[random.Next(course3.Count)].CoursesId,
                      ClassesId = classes3[random.Next(classes3.Count)].ClassesId,
                      
                      SubjectName = "IT",
                      Score = 80
                  }

                };

                // Cập nhật Status dựa trên Score
                foreach (var score in examScores)
                {
                    score.Status = score.Score >= 40 ? "Passed" : "Not Passed";
                    context.ExamScores.Add(score);
                }

                context.SaveChanges(); // Lưu các thay đổi vào cơ sở dữ liệu


                // Seed data for Registrations
                var students = context.StudentsInformation.ToList();
                var registrations = new List<Registrations>();

                foreach (var student in students)
                {
                    var registration = new Registrations
                    {
                        StudentsInformationId = student.StudentsInformationId,
                        RegistrationStatus = GetRandomRegistrationStatus(),
                        UniqueCode = GenerateUniqueCode(),
                        RegistrationDate = DateTime.Now.AddDays(random.Next(1, 30)),

                    };

                    registrations.Add(registration);
                }

                foreach (var registration in registrations)
                {
                    context.Registrations.Add(registration);
                }
                context.SaveChanges();

                // Hàm để chọn ngẫu nhiên trạng thái đăng ký
                string GetRandomRegistrationStatus()
                {
                    var random = new Random();
                    var statuses = new string[] { "Pending", "Not Admitted", "Admitted" };
                    return statuses[random.Next(statuses.Length)];
                }

                // Hàm để tạo mã duy nhất
                string GenerateUniqueCode()
                {
                    // Bạn có thể tạo mã duy nhất ở đây theo logic mong muốn
                    return Guid.NewGuid().ToString();
                }



                // Seed data for Events
                var thumbnailPaths = Enumerable.Range(1, 6).Select(i => $"/images/event-{i}.jpg").ToList();
                var eventDescriptions = new string[]
                {
                "Welcome Party",
                "Seminar on Data Science",
                "Business Conference",
                "Annual Charity Gala",
                "Art Exhibition",
                "Tech Workshop",
                "Music Festival",
                "Fashion Show",
                "Film Screening",
                "Fitness Bootcamp"
                };

                for (int i = 0; i < 10; i++)
                {
                    var eventIndex = random.Next(eventDescriptions.Length);

                    var newEvent = new Events
                    {
                        EventDescription = eventDescriptions[eventIndex],
                        EventDate = DateTime.Now.AddDays(random.Next(1, 30)),
                        EventImageUrl = thumbnailPaths[random.Next(thumbnailPaths.Count)],
                        EventTitle = $"Event {i + 1}"
                    };

                    context.Events.Add(newEvent);
                }

                context.SaveChanges();


                // Seed data for Achievements
                var thumbnailPaths1 = Enumerable.Range(1, 6).Select(i => $"/images/event-{i}.jpg").ToList();
                var achievementDescriptions = new string[]
 {
    "First Prize in Coding Competition",
    "Runner-up in Mathematics Olympiad",
    "Best Paper Award at Conference",
    "Outstanding Performance in Sports Meet",
    "Excellence in Community Service",
    "Leadership Award",
    "Achievement in Fine Arts",
    "Innovation Award",
    "Scholarship Recipient",
    "Recognition for Academic Excellence"
 };

                for (int i = 0; i < 10; i++)
                {
                    var achievementIndex = random.Next(achievementDescriptions.Length);

                    var newAchievement = new Achievements
                    {
                        AchievementDescription = achievementDescriptions[achievementIndex],
                        AchievementTitle = $"Achievement {i + 1}",
                        AchievementDate = DateTime.Now.AddDays(random.Next(1, 30)),
                        AchievementImageUrl = thumbnailPaths1[random.Next(thumbnailPaths1.Count)],
                    };

                    context.Achievements.Add(newAchievement);
                }

                context.SaveChanges();


                // Seed data for Facilities
                var thumbnailPaths3 = Enumerable.Range(1, 6).Select(i => $"/images/event-{i}.jpg").ToList();
                var facilityDescriptions = new string[]
                {
    "State-of-the-art Computer Lab",
    "Modern Library with Extensive Collection",
    "Science Laboratory equipped for Experiments",
    "Fitness Center with Latest Equipment",
    "Art Studio for Creative Expression",
    "Cafeteria serving Healthy and Delicious Meals",
    "Sports Facilities including Basketball Court and Swimming Pool",
    "Auditorium for Events and Presentations",
    "Research Lab for Academic Projects",
    "Language Lab for Enhancing Language Skills"
                };

                for (int i = 0; i < 10; i++)
                {
                    var facilityIndex = random.Next(facilityDescriptions.Length);

                    var newFacility = new Facilities
                    {
                        FacilityDescription = facilityDescriptions[facilityIndex],
                        FacilityTitle = $"Facility {i + 1}",
                        FacilityDate = DateTime.Now.AddDays(random.Next(1, 30)),
                        FacilityImageUrl = thumbnailPaths3[random.Next(thumbnailPaths3.Count)]
                    };

                    context.Facilities.Add(newFacility);
                }

                context.SaveChanges();
                // Seed data for ContactInfo
                var contactInfos = new ContactInfo[]
                {
    new ContactInfo
    {
        Name = "Main Campus",
        Message = "hello",
        Subject = "hello",
        Email = "info@example.com",
        ContactDate = DateTime.Now // hoặc giá trị ngày tháng cụ thể
    },
    new ContactInfo
    {
        Name = "Downtown Campus",
        Message = "hi",
        Subject = "hi",
        Email = "info@downtown.example.com",
        ContactDate = DateTime.Now // hoặc giá trị ngày tháng cụ thể
    }
                };

                foreach (var contactInfo in contactInfos)
                {
                    context.ContactInfo.Add(contactInfo);
                }
                context.SaveChanges();

                // Seed data for CoursesSubjects
                var Courses1 = context.Courses.ToList();
                var subjects1 = context.Subjects.ToList();

                for (int i = 0; i < 10; i++)
                {
                    var randomCoursesId = Courses1[random.Next(Courses1.Count)].CoursesId;
                    var randomSubjectId = subjects1[random.Next(subjects1.Count)].SubjectsId;
                    var randomNumericalOrder = random.Next(1, 51); // Số ngẫu nhiên từ 1 đến 50

                    var newCoursesSubjects = new CoursesSubjects
                    {
                        CoursesId = randomCoursesId,
                        SubjectsId = randomSubjectId,

                        NumericalOrder = randomNumericalOrder
                    };

                    context.CoursesSubjects.Add(newCoursesSubjects);
                }

                context.SaveChanges();



                // Cập nhật CourseTime cho từng khóa học
                foreach (var course in courses)
                {
                    var totalSessionsTask = context.CoursesSubjects
                        .Where(cs => cs.CoursesId == course.CoursesId)
                        .Include(cs => cs.Subject)
                        .SumAsync(cs => cs.Subject.NumberOfSessions);

                    // Đợi tác vụ hoàn thành và nhận kết quả
                    var totalSessions = totalSessionsTask.Result;

                    // Tính số tuần cần thiết để hoàn thành các buổi học (3 buổi học mỗi tuần)
                    int sessionsPerWeek = 3;
                    double totalWeeks = (double)totalSessions / sessionsPerWeek;

                    // Tính số tháng (giả sử mỗi tháng có 4 tuần)
                    double totalMonths = totalWeeks / 4;

                    // Làm tròn số tháng đến số nguyên gần nhất
                    int roundedMonths = (int)Math.Round(totalMonths);

                    // Cập nhật CourseTime với giá trị đã làm tròn
                    course.CourseTime = $"{roundedMonths} months";

                    context.Courses.Update(course);
                }

                context.SaveChanges(); // Lưu các thay đổi vào cơ sở dữ liệu

                // Seed data for MergedStudentData
                var officialStudents1 = context.OfficialStudents.ToList();
                var courses4 = context.Courses.ToList();
                var classes4 = context.Classes.ToList();
                var mergedStudentData = new List<MergedStudentData>
                {
                    new MergedStudentData
                    {
                        OfficialStudentId = officialStudents1[0].OfficialStudentId,
                        CoursesId = courses4[0].CoursesId,
                        ClassesId = classes4[0].ClassesId,
                        Telephone = "1234567890",
                        EnrollmentStartDate = DateTime.UtcNow,
                        EnrollmentEndDate = DateTime.UtcNow.AddDays(30),
                        StudyDays = "Monday, Wednesday, Friday",
                        StudySession = "Morning",
                        ClassStartDate = DateTime.UtcNow,
                        ClassEndDate = DateTime.UtcNow,
                        StudentStatus = "Studying",
                        DeleteStatus = 0
                    },
                    new MergedStudentData
                    {
                       OfficialStudentId = officialStudents1[0].OfficialStudentId,
                        CoursesId = courses4[0].CoursesId,
                        ClassesId = classes4[0].ClassesId,
                        Telephone = "0987654321",
                        EnrollmentStartDate = DateTime.UtcNow.AddDays(10),
                        EnrollmentEndDate = DateTime.UtcNow.AddDays(40),
                        StudyDays = "Tuesday, Thursday, Saturday",
                        StudySession = "Afternoon",
                         ClassStartDate = DateTime.UtcNow,
                        ClassEndDate = DateTime.UtcNow,
                        StudentStatus = "Complete",
                        DeleteStatus = 0
                    }
                };

                context.MergedStudentData.AddRange(mergedStudentData);
                context.SaveChanges();
            }

        }
    }
}
