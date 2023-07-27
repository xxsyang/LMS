using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            return Json(query.ToArray());
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
                            submissions = 0
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
            int weightCount = 0;

            if (catweight < 0 || catweight > 100)
            {
                categoryExists = true;
                return Json(new { success = false });
            }

            var getAssignmentCategories = from ac in db.AssignmentCategories
                        join cl in db.Classes on ac.InClass equals cl.ClassId
                        join co in db.Courses on cl.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && cl.Season == season && cl.Year == year
                        select new
                        {
                            weight = ac.Weight
                        };

            var assignmentCategories = getAssignmentCategories.ToArray();

            foreach(var ac in assignmentCategories){
                weightCount = weightCount + (int)ac.weight;
            }

            if(weightCount + catweight > 100)
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
            //Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + classID + "\n");

            var getCategoryID = from ac in db.AssignmentCategories
                                where ac.InClass == classID && ac.Name == category
                                select ac.CategoryId;

            uint categoryID = (uint)getCategoryID.FirstOrDefault();
            //Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + categoryID + "\n");


            var newAssignment = new Assignment
            {
                Category = categoryID,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint) asgpoints,
                Name = asgname
            };

            db.Assignments.Add(newAssignment);
            db.SaveChanges();

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
            //String subject
            var query = from sub in db.Submissions
                        join 


            return Json(null);
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
            return Json(new { success = false });
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


        
        /*******End code to modify********/
    }
}

