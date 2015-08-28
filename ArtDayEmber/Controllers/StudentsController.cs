using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description; 
using ArtDayEmber;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using System.Web.Http.Results;
using System.IO;
using System.Net.Http.Formatting;

namespace ArtDayEmber.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class StudentsController : ApiController
    {
        private ArtDayDBEntities db = new ArtDayDBEntities();

        // GET: api/Students
        //public IQueryable<Student> GetStudents()
        public HttpResponseMessage GetStudents()
        {
            // Doing this prevents circular reference from student -> preference -> student -> preference...
            var result = db.Students.Select(
                s => new
                {
                    id = s.id,
                    firstName = s.firstName,
                    lastName = s.lastName,
                    grade = s.grade,
                    locked = s.locked,
                    preferences = s.Preferences.Select(p => p.PreferenceID).ToList(),
                    enrollments = s.Enrollments.Select(p => p.EnrollmentID).ToList()
                }).ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, new {students = result});
        }

        // GET: api/Students/5
        [ResponseType(typeof(Student))]
        public async Task<HttpResponseMessage> GetStudent(int id)
        {
            Student student = await db.Students.FindAsync(id);

            if (student == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { message = "That student does not exist." }, new JsonMediaTypeFormatter());
            }

            var result = new
            {
                id = student.id,
                firstName = student.firstName,
                lastName = student.lastName,
                grade = student.grade,
                locked = student.locked,
                preferences = student.Preferences.Select(p => p.PreferenceID).ToList(),
                enrollments = student.Enrollments.Select(p => p.EnrollmentID).ToList()
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, new { student = result });
        }

        // PUT: api/Students/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutStudent(int id, Student student)
        {
            // json body looks like this: 
            // {"student":{"firstname":"ISABELLA","lastname":"SIAMUNDO","grade":10,"locked":true}}

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ember by default doesn't include the id in the json request.
            student.id = id;

            db.Entry(student).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Students
        // This is the method called when importing a new set of students.
        // Only available to admins.  Should be called only once using the current student list
        // just before opening the site to pref setting.
        // Danger - will delete all existing students, prefs and enrollments.
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> PostStudent()
        {            
            // TODO: Uncomment sql commands before going live.
            // Before adding new students, first delete all enrollments, prefs and students.
            //db.Database.ExecuteSqlCommand("delete from preference");
            //db.Database.ExecuteSqlCommand("delete from enrollment");
            //db.Database.ExecuteSqlCommand("delete from student");

            string body = await Request.Content.ReadAsStringAsync();
            List<Student> students = JsonConvert.DeserializeObject<List<Student>>(body);

            foreach (var student in students)
            {
                db.Students.Add(student);
            }
            
            await db.SaveChangesAsync();  // save changes once.  Hopefully this makes a single db call.

            return this.Request.CreateResponse(HttpStatusCode.Created);
        }

        // DELETE: api/Students/5
        [ResponseType(typeof(Student))]
        public async Task<IHttpActionResult> DeleteStudent(int id)
        {
            Student student = await db.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            db.Students.Remove(student);
            await db.SaveChangesAsync();

            return Ok(student);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StudentExists(int id)
        {
            return db.Students.Count(e => e.id == id) > 0;
        }
    }
}