using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Enums;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from Dept in db.Departments
                        select new
                        {
                            name = Dept.Name,
                            subject = Dept.Subject

                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()

        {
            var query = from c in db.Courses
                        join d in db.Departments on c.Department equals d.Subject
                        select new
                        {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = db.Courses.Where(course => course.Department == d.Subject)
                                        .Select(course => new
                                        {
                                            number = course.Number,
                                            cname = course.Name
                                        }).ToArray()

                        };

            return Json(query.ToArray());
            
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {

            var query = from co in db.Courses
                        join cl in db.Classes on co.CatalogId equals cl.Listing
                        join p in db.Professors on cl.TaughtBy equals p.UId
                        where co.Department == subject && co.Number == number
                        select new
                        {
                            season = cl.Season,
                            year = cl.Year,
                            location = cl.Location,
                            start = cl.StartTime,
                            end = cl.EndTime,
                            fname = p.FName,
                            lname = p.LName
                        };
                        
            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from assign in db.Assignments
                        join ac in db.AssignmentCategories on assign.Category equals ac.CategoryId
                        join c in db.Classes on ac.InClass equals c.ClassId
                        join co in db.Courses on c.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && c.Season == season && c.Year == year && ac.Name == category && assign.Name == asgname
                        select assign.Contents;
                        
            return Content(query.FirstOrDefault() ?? "");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query = from sub in db.Submissions
                        join assign in db.Assignments on sub.Assignment equals assign.AssignmentId
                        join ac in db.AssignmentCategories on assign.Category equals ac.CategoryId
                        join c in db.Classes on ac.InClass equals c.ClassId
                        join co in db.Courses on c.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && c.Season == season && c.Year == year && ac.Name == category && assign.Name == asgname && sub.Student == uid

                        select sub.SubmissionContents;

            return Content(query.FirstOrDefault() ?? "");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            Debug.WriteLine("Get userInfo Connection Successful");
            bool exist = false;

            var getStudentID = from s in db.Students
                               select s.UId;

            var getProfID = from p in db.Professors
                            select p.UId;

            var getAdminID = from a in db.Administrators
                             select a.UId;


            if (getStudentID.ToArray().Contains(uid))
            {
                exist = true;
                var qurey = from s in db.Students
                            where s.UId == uid
                            select new
                            {
                                fname = s.FName,
                                lname = s.LName,
                                uid = s.UId,
                                Department = s.Major
                            };
                return Json(qurey.First());
            }

            if (getProfID.ToArray().Contains(uid))
            {
                exist = true;
                var qurey = from p in db.Professors
                            where p.UId == uid
                            select new
                            {
                                fname = p.FName,
                                lname = p.LName,
                                uid = p.UId,
                                Department = p.WorksIn
                            };
                return Json(qurey.First());
            }

            if (getStudentID.ToArray().Contains(uid))
            {
                exist = true;
                var qurey = from a in db.Administrators
                            where a.UId == uid
                            select new
                            {
                                fname = a.FName,
                                lname = a.LName,
                                uid = a.UId
                            };
                return Json(qurey.First());
            }

            return Json(new { success = exist });
        }


        /*******End code to modify********/
    }
}

