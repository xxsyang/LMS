using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from en in db.Enrolleds
                        join c in db.Classes
                        on en.Class equals c.ClassId into left

                        from l in left
                        join r in db.Courses
                        on l.Listing equals r.CatalogId
                        where en.Student == uid

                        select new
                        {
                            subject = r.Department,
                            number = r.Number,
                            name = r.Name,
                            season = l.Season,
                            year = l.Year,
                            grade = en.Grade
                        };


            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from cl in db.Classes

                        join ac in db.AssignmentCategories
                        on cl.ClassId equals ac.CategoryId

                        join a in db.Assignments
                        on ac.CategoryId equals a.Category

                        join co in db.Courses
                        on cl.Listing equals co.CatalogId

                        where cl.Season == season
                        where cl.Year == year
                        where co.Department == subject
                        where co.Number == num

                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            score = (from s in db.Submissions
                                     where s.Student == uid
                                     where s.Assignment == a.AssignmentId
                                     select s.Score
                                     ).ToList()
                        };

            var result = new List<object>();
            foreach (var q in query)
            {
                result.Add(
                    new
                    {
                        q.aname,
                        q.cname,
                        q.due,
                        score = (q.score.Any()) ? q.score : null
                    }
                );
            }

            return Json(result.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            bool submitted = false;

            var assignmentQuery = (from c in db.Classes
                         join co in db.Courses
                         on c.Listing equals co.CatalogId
                         join ac in db.AssignmentCategories
                         on c.ClassId equals ac.InClass
                         join a in db.Assignments
                         on ac.CategoryId equals a.Category

                         where co.Department == subject
                         where co.Number == num
                         where c.Season == season
                         where c.Year == year
                         where ac.Name == asgname

                         select new
                         {
                             assignmentId = a.AssignmentId,
                             //assignmentName = a.Name,
                             //classId = c.ClassId
                         }).FirstOrDefault();

            var submissionQuery = (from sub in db.Submissions
                                   where sub.Assignment == assignmentQuery.assignmentId
                                   where sub.Student == uid
                                   select sub).FirstOrDefault();

            if (submissionQuery == null) // if no submission
            {
                // create submission
                //uint idCount = 1;

                if (db.Submissions.Any())
                {
                    //var last = db.Submissions.OrderBy(x => x.Time).Last(); // last submitted entry
                    //var id = last.

                    Submission submission = new Submission();

                    submission.Student = uid;
                    submission.Assignment = assignmentQuery.assignmentId;
                    submission.SubmissionContents = contents;
                    submission.Time = DateTime.Now;
                    submission.Score = 0;

                    db.Add(submission);
                }
            }
            else // already submitted
            {
                submissionQuery.SubmissionContents = contents;
                submissionQuery.Time = DateTime.Now;
                submitted = true;
            }

            db.SaveChanges();

            return Json(new { success = submitted });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            bool enrolled = false;

            var enroll = from e in db.Enrolleds
                         join cl in db.Classes
                         on e.Class equals cl.ClassId
                         join co in db.Courses
                         on cl.Listing equals co.CatalogId

                         where e.Student == uid
                         where co.Department == subject
                         where co.Number == num
                         where cl.Season == season
                         where cl.Year == year

                         select e;

            var classID = from cl in db.Classes
                          join co in db.Courses
                          on cl.Listing equals co.CatalogId

                          where co.Department == subject
                          where co.Number == num
                          where cl.Season == season
                          where cl.Year == year
                          select cl;

            if (!enroll.Any()) // not enrolled
            {
                Enrolled enrollment = new Enrolled();

                enrollment.Student = uid;
                enrollment.Grade = "--";
                enrollment.Class = classID.FirstOrDefault().ClassId;
                enrolled = true;
                // add to db
                db.Enrolleds.Add(enrollment);
            }
            //else enrolled = false;

            return Json(new { success = enrolled});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            double grades = 0.0;
            int gradeCount = 0;

            var gradeQuery = from e in db.Enrolleds
                             where e.Student == uid
                             select e.Grade;

            foreach (var grade in gradeQuery)
            {
                if (grade == "A")
                {
                    grades += 4.0;
                    gradeCount++;
                    break;
                }
                else if (grade == "A-")
                {
                    grades += 3.7;
                    gradeCount++;
                    break;
                }
                else if (grade == "B+")
                {
                    grades += 3.3;
                    gradeCount++;
                    break;
                }
                else if (grade == "B")
                {
                    grades += 3.0;
                    gradeCount++;
                    break;
                }
                else if (grade == "B-")
                {
                    grades += 2.7;
                    gradeCount++;
                    break;
                }
                else if (grade == "C+")
                {
                    grades += 2.3;
                    gradeCount++;
                    break;
                }
                else if (grade == "C+")
                {
                    grades += 2.3;
                    gradeCount++;
                    break;
                }
                else if (grade == "C")
                {
                    grades += 2.0;
                    gradeCount++;
                    break;
                }
                else if (grade == "C-")
                {
                    grades += 1.7;
                    gradeCount++;
                    break;
                }
                else if (grade == "D+")
                {
                    grades += 1.3;
                    gradeCount++;
                    break;
                }
                else if (grade == "D")
                {
                    grades += 1.0;
                    gradeCount++;
                    break;
                }
                else if (grade == "D-")
                {
                    grades += 0.7;
                    gradeCount++;
                    break;
                }
                else if (grade == "E")
                {
                    //grades += 0.0;
                    gradeCount++;
                    break;
                }
            }

            double gpa = grades / gradeCount;

            return Json(gpa);
        }
                
        /*******End code to modify********/

    }
}

