using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from e in db.Enrolleds
                        join s in db.Students on e.Student equals s.UId
                        join cl in db.Classes on e.Class equals cl.ClassId
                        join co in db.Courses on cl.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade
                        };

            //Debug.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++" + query.FirstOrDefault().fname);

            return Json(query.ToList());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name = "subject" > The course subject abbreviation</param>
        /// <param name = "num" > The course number</param>
        /// <param name = "season" > The season part of the semester for the class the assignment belongs to</param>
        /// <param name = "year" > The year part of the semester for the class the assignment belongs to</param>
        /// <param name = "category" > The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var thisCourse = db.Courses.Where(co => co.Number == num && co.Department == subject).Single();
            var thisClass = db.Classes.Where(c => c.Season == season && c.Year == year && c.Listing == thisCourse.CatalogId).Single();

            ICollection<Assignment> assignments = null;
            if (category == null)
            {
                assignments = db.Assignments.Where(ass => ass.CategoryNavigation.InClass == thisClass.ClassId).ToList();
            }
            else
            {
                assignments = db.Assignments.Where(ass => ass.CategoryNavigation.Name == category && ass.CategoryNavigation.InClass == thisClass.ClassId).ToList();
            }


            if (category == null)
            {
                var queryWithoutCategory = from ass in db.Assignments
                                           join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                                           join cl in db.Classes on ac.InClass equals cl.ClassId
                                           join co in db.Courses on cl.Listing equals co.CatalogId
                                           where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                                           select new
                                           {
                                               aname = ass.Name,
                                               cname = ac.Name,
                                               due = ass.Due,
                                               submissions = (from sub in db.Submissions
                                                              where sub.Assignment == ass.AssignmentId
                                                              select sub.Assignment).Count()
                                           };
                

                return Json(queryWithoutCategory.ToArray());
            }

            var query = from ass in db.Assignments
                        join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                        join cl in db.Classes on ac.InClass equals cl.ClassId
                        join co in db.Courses on cl.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category
                        select new
                        {
                            aname = ass.Name,
                            cname = ac.Name,
                            due = ass.Due,
                            submissions = (from sub in db.Submissions
                                           where sub.Assignment == ass.AssignmentId
                                           select sub.Assignment).Count()
                        };



            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from ac in db.AssignmentCategories
                        join cl in db.Classes on ac.InClass equals cl.ClassId
                        join co in db.Courses on cl.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            bool categoryExists = false;

            if (catweight < 0)
            {
                categoryExists = true;
                return Json(new { success = false });
            }

            var getCategory = from ac in db.AssignmentCategories
                              join cl in db.Classes on ac.InClass equals cl.ClassId
                              join co in db.Courses on cl.Listing equals co.CatalogId
                              select co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category;

            if (!getCategory.Any())
            {
                categoryExists = true;
                return Json(new { success = false });
            }

            var getClassID = from co in db.Courses
                             join cl in db.Classes on co.CatalogId equals cl.Listing
                             where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                             select cl.ClassId;

            uint classID = (uint)getClassID.FirstOrDefault();

            Class thisClass = db.Classes.Where(c => c.ListingNavigation.Department == subject && c.ListingNavigation.Number == num && c.Season == season && c.Year == year).SingleOrDefault();
            if (thisClass == null)
            {
                return Json(new { success = false });

            }
            var newAssignmentCategory = new AssignmentCategory
            {
                Name = category,
                InClass = classID,
                Weight = (uint)catweight,
                InClassNavigation = thisClass
            };


            db.AssignmentCategories.Add(newAssignmentCategory);
            db.SaveChanges();

            return Json(new { success = true });


        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            bool assignmentExist = false;

            var getAssignment = from ass in db.Assignments
                                join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                                join cl in db.Classes on ac.InClass equals cl.ClassId
                                join co in db.Courses on cl.Listing equals co.CatalogId
                                select co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category && ass.Name == asgname;

            if (!getAssignment.Any())
            {
                assignmentExist = true;
                return Json(new { success = false });
            }

            var getClassID = from co in db.Courses
                             join cl in db.Classes on co.CatalogId equals cl.Listing
                             where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                             select cl.ClassId;

            uint classID = (uint)getClassID.FirstOrDefault();

            Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& classID" + classID);

            var getCategoryID = from ac in db.AssignmentCategories
                                where ac.InClass == classID && ac.Name == category
                                select ac.CategoryId;

            uint categoryID = (uint)getCategoryID.FirstOrDefault();

            Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& categoryID" + categoryID);

            var newAssignment = new Assignment
            {
                Category = categoryID,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Name = asgname
            };

            db.Add(newAssignment);
            db.SaveChanges();

            //////////////update grade part
            var thisClass = db.Classes.Where(c => c.ListingNavigation.Department == subject && c.ListingNavigation.Number == num && c.Season == season && c.Year == year).SingleOrDefault();

            if (thisClass == null)
            {
                return Json(new { success = false });
            }


            foreach (Student student in thisClass.Enrolleds.Select(enr => enr.StudentNavigation))
            {
                UpdateGrade(student, thisClass);
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from sub in db.Submissions
                        join ass in db.Assignments on sub.Assignment equals ass.AssignmentId
                        join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                        join cl in db.Classes on ac.InClass equals cl.ClassId
                        join co in db.Courses on cl.Listing equals co.CatalogId
                        join stu in db.Students on sub.Student equals stu.UId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category && ass.Name == asgname
                        select new
                        {
                            fname = stu.FName,
                            lname = stu.LName,
                            uid = stu.UId,
                            time = sub.Time,
                            score = sub.Score
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            if (score < 0)
            {
                return Json(new { success = false });
            }


            var thisClass = db.Classes.Where(c => c.ListingNavigation.Department == subject && c.ListingNavigation.Number == num && c.Season == season && c.Year == year).SingleOrDefault();
            var student = db.Students.Where(s => s.UId == uid).SingleOrDefault();


            Debug.WriteLine("-----------------------------------------------------------------------------------thisClass" + thisClass.ClassId);

            if (thisClass == null)
                return Json(new { success = false });

            var assignment = db.Assignments.Where(ass => ass.Name == asgname && ass.CategoryNavigation.Name == category && ass.CategoryNavigation.InClass == thisClass.ClassId).SingleOrDefault();

            Debug.WriteLine("-----------------------------------------------------------------------------------assignment.AssignmentId GradeSubmission" + assignment.AssignmentId);

            if (student == null || assignment == null)
                return Json(new { success = false });

            var submission = db.Submissions.Where(sub => sub.Student == uid && sub.Assignment == assignment.AssignmentId).SingleOrDefault();

            Debug.WriteLine("-----------------------------------------------------------------------------------submission.Student GradeSubmission" + submission.Student);

            if (submission == null)
                return Json(new { success = false });


            submission.Score = (uint)score;
            db.Update(submission);
            db.SaveChanges();

            // updates grade
            UpdateGrade(student, thisClass);

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var getClasses = from cl in db.Classes
                             join p in db.Professors on cl.TaughtBy equals p.UId
                             join co in db.Courses on cl.Listing equals co.CatalogId
                             where p.UId == uid
                             select new
                             {
                                 subject = co.Department,
                                 number = co.Number,
                                 name = co.Name,
                                 season = cl.Season,
                                 year = cl.Year
                             };

            return Json(getClasses.ToArray());
        }


        void UpdateGrade(Student student, Class thisClass)
        {
            var enr = db.Enrolleds.Where(enr => enr.Student == student.UId && enr.Class == thisClass.ClassId).Single();

            thisClass.AssignmentCategories = db.AssignmentCategories.Where(ac => ac.InClass == thisClass.ClassId).ToList();

            Debug.WriteLine("-----------------------------------------------------------------------------------enr.Student " + enr.Student);
            Debug.WriteLine("-----------------------------------------------------------------------------------thisClass " + thisClass.ClassId);
            Debug.WriteLine("-----------------------------------------------------------------------------------CategoryId num " + thisClass.AssignmentCategories.Count);

            double allTotalUnscaled = thisClass.AssignmentCategories.Select(
                // assignment category weight * (sum)
                ac =>
                {
                    Debug.WriteLine("-----------------------------------------------------------------------------------ddddddddddddddddddddddddeee " + ac.Name);

                    ac.Assignments = db.Assignments.Where(ass => ass.Category == ac.CategoryId).ToList();
                    int earnedPoints = ac.Assignments.Select(ass =>
                    {
                        //ass.Submissions = db.Submissions.Where()
                        Debug.WriteLine("-----------------------------------------------------------------------------------Submissions count " + ass.Submissions.Count());
                        
                        var sub = db.Submissions.Where(sub => sub.Student == student.UId && ass.AssignmentId == sub.Assignment).SingleOrDefault();
                        //Debug.WriteLine("-----------------------------------------------------------------------------------Submissions" + sub.Score);

                        return sub == null ? 0 : (int)sub.Score;
                    }
                    ).Sum();
                    // Total points across all assignments
                    int totalPoints = ac.Assignments.Sum(ass => (int)ass.MaxPoints);
                    Debug.WriteLine("-----------------------------------------------------------------------------------totalPoints on one ca " + totalPoints);

                    double score = totalPoints == 0 ? 0.0 : (double)earnedPoints / (double)totalPoints;

                    
                   
                    double scaledScore = score * (double)ac.Weight;
                    Debug.WriteLine("-----------------------------------------------------------------------------------scaledScore on one ca " + scaledScore);

                    return scaledScore;
                }
            ).Sum();

            Debug.WriteLine("-----------------------------------------------------------------------------------allTotalUnscaled on one ca " + allTotalUnscaled);
            // IMPORTANT - there is no rule that assignment
            // category weights must sum to 100. Therefore, we have to re-scale
            // This takes the sum of all assignment category weights that are non empty
            var totalWeights = thisClass.AssignmentCategories.Select(ac =>
            {
                var totalPoints = ac.Assignments.Select(ass => (int)ass.MaxPoints).Sum();
                if (totalPoints == 0)
                    return 0;
                else
                    return (int)ac.Weight;
            }).Sum();
            // Avoids dividing by zero; in the case that all assignment weights
            // are zero, the total scaled score is 0.0 / 100.0
            double totalPercentage = totalWeights == 0 ? 100.0 : (double)allTotalUnscaled * (100 / (double)totalWeights);


            string letterGrade = "--";

            if (totalPercentage >= 92)
            {
                letterGrade = "A";
            }

            else if (totalPercentage >= 90)
            {
                letterGrade = "A-";
            }
            else if (totalPercentage >= 88)
            {
                letterGrade = "B+";
            }
            else if (totalPercentage >= 82)
            {
                letterGrade = "B";
            }
            else if (totalPercentage >= 80)
            {
                letterGrade = "B-";
            }
            else if (totalPercentage >= 78)
            {
                letterGrade = "C+";
            }
            else if (totalPercentage >= 72)
            {
                letterGrade = "C";
            }
            else if (totalPercentage >= 70)
            {
                letterGrade = "C-";
            }
            else if (totalPercentage >= 68)
            {
                letterGrade = "D+";
            }
            else if (totalPercentage >= 62)
            {
                letterGrade = "D";
            }
            else if (totalPercentage >= 60)
            {
                letterGrade = "D-";
            }
            else
            {
                letterGrade = "E";
            }

            enr.Grade = letterGrade;
            db.Update(enr);
            db.SaveChanges();

        }

    }
}