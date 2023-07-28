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

            Debug.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++" + query.FirstOrDefault().fname);

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
            //int weightCount = 0;

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

            var newAssignmentCategory = new AssignmentCategory
            {
                Name = category,
                InClass = classID,
                Weight = (uint)catweight,

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

            var getCategoryID = from ac in db.AssignmentCategories
                                where ac.InClass == classID && ac.Name == category
                                select ac.CategoryId;

            uint categoryID = (uint)getCategoryID.FirstOrDefault();

            var newAssignment = new Assignment
            {
                Category = categoryID,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Name = asgname
            };

            db.Assignments.Add(newAssignment);
            db.SaveChanges();

            //////////////update grade part
            var getStudentList = from en in db.Enrolleds
                                 where en.Class == classID
                                 select en;

            foreach(var student in getStudentList)
            {
                AutoGradeClass(subject, num, season, year, student.Student, classID);
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

            var getAssignmentId = from sub in db.Submissions
                                  join ass in db.Assignments on sub.Assignment equals ass.AssignmentId
                                  join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                                  join cl in db.Classes on ac.InClass equals cl.ClassId
                                  join co in db.Courses on cl.Listing equals co.CatalogId
                                  join stu in db.Students on sub.Student equals stu.UId
                                  where co.Department == subject && co.Number == num
                                                                 && cl.Season == season
                                                                 && cl.Year == year && ac.Name == category && ass.Name == asgname && sub.Student == uid
                                  select new
                                  {
                                      id = sub.Assignment
                                  };

            uint assignmentID = getAssignmentId.FirstOrDefault().id;

            var submission = db.Submissions.FirstOrDefault(s => s.Assignment == assignmentID && s.Student == uid);

            if (submission == null)
            {
                submission.Score = 0;
            }

            submission.Score = (uint)score;
            db.SaveChanges();

            //////////////update grade part

            var getClassID = from co in db.Courses
                             join cl in db.Classes on co.CatalogId equals cl.Listing
                             where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                             select cl.ClassId;

            uint classID = (uint)getClassID.FirstOrDefault();

            var getStudentList = from en in db.Enrolleds
                                 where en.Class == classID
                                 select en;

            getStudentList.ToList();

            foreach (var student in getStudentList)
            {
                AutoGradeClass(subject, num, season, year, student.Student, classID);
            }



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


        /// <summary>
        /// Returns a double score for one Assignment Category, if there is no assginments in Category, then return -1.0
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <returns>A double score number or -1.0(if there is no assginments in Category)</returns>
        public double AutoGradeAssignmentsInCategory(string subject, int num, string season, int year, string category, string uid)
        {
            uint scoreOnEachAssCategory = 0;
            uint maxScore = 0;

            var checkIsAssign = from ass in db.Assignments
                                join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                                join cl in db.Classes on ac.InClass equals cl.ClassId
                                join co in db.Courses on cl.Listing equals co.CatalogId
                                where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category
                                select ass.Category;


            if (!checkIsAssign.Any())
            {
                return -1.0;
            }

            var getOneAssignmentCategoryScore = from sub in db.Submissions
                                                join ass in db.Assignments on sub.Assignment equals ass.AssignmentId
                                                join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                                                join cl in db.Classes on ac.InClass equals cl.ClassId
                                                join co in db.Courses on cl.Listing equals co.CatalogId
                                                join stu in db.Students on sub.Student equals stu.UId
                                                where co.Department == subject && co.Number == num && cl.Season == season
                                                      && cl.Year == year && ac.Name == category && sub.Student == uid
                                                select new
                                                {
                                                    score = sub.Score,
                                                };

            getOneAssignmentCategoryScore.ToList();


            var getMaxScores = from ass in db.Assignments
                               join ac in db.AssignmentCategories on ass.Category equals ac.CategoryId
                               join cl in db.Classes on ac.InClass equals cl.ClassId
                               join co in db.Courses on cl.Listing equals co.CatalogId
                               where co.Department == subject && co.Number == num && cl.Season == season
                                     && cl.Year == year && ac.Name == category
                               select new
                               {
                                   maxScore = ass.MaxPoints,
                               };

            getMaxScores.ToList();

            foreach (var eachMaxscore in getMaxScores)
            {
                maxScore = maxScore + eachMaxscore.maxScore;
            }

            foreach (var eachSubScore in getOneAssignmentCategoryScore)
            {
                scoreOnEachAssCategory = scoreOnEachAssCategory + eachSubScore.score;
            }

            Double percentage = scoreOnEachAssCategory / maxScore;
            double roundedResult = Math.Round(percentage, 3);

            return roundedResult;
        }

        /// <summary>
        /// Returns a double score for one Assignment Category
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <returns>A double score number</returns>
        public IActionResult AutoGradeClass(string subject, int num, string season, int year, string uid, uint classID)
        {
            double percentageTimesWeight = 0.0;
            double sum0fpercentageTimesWeight = 0.0;

            var getAssignmentCategoryWeight = (from ac in db.AssignmentCategories
                                              join cl in db.Classes on ac.InClass equals cl.ClassId
                                              join co in db.Courses on cl.Listing equals co.CatalogId
                                              where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                                              select new
                                              {
                                                  acName = ac.Name,
                                                  weights = ac.Weight
                                              }).ToArray();


            foreach (var eachAssignmentCategory in getAssignmentCategoryWeight)
            {
                Debug.WriteLine("eachAssignmentCategory-------------------------------------------------------------------------------------" + eachAssignmentCategory.acName);
                double categoryScore = AutoGradeAssignmentsInCategory(subject, num, season, year, eachAssignmentCategory.acName, uid);

                if (categoryScore == -1.0)
                {
                    continue;
                }
                percentageTimesWeight = categoryScore * eachAssignmentCategory.weights;
                sum0fpercentageTimesWeight = sum0fpercentageTimesWeight + percentageTimesWeight;
            }

            double scalingFactor = 100 / sum0fpercentageTimesWeight;

            double totalPercentage = sum0fpercentageTimesWeight * scalingFactor;

            String grade = ConvertToLetterGrade(totalPercentage);

            var getGrade = from en in db.Enrolleds
                           where en.Student == uid && en.Class == classID
                           select en.Grade;


            var autoGrade = db.Enrolleds.FirstOrDefault(e => e.Student == uid && e.Class == classID);

            autoGrade.Grade = grade;
            db.SaveChanges();


            return Json(new { success = true });
        }

        private static string ConvertToLetterGrade(double totalPercentage)
        {
            Console.WriteLine("Calculating grade.");
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

            return letterGrade;

        }
    }
}
/*******End code to modify********/
