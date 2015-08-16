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
using System.Net.Http.Formatting;

namespace ArtDayEmber.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class EnrollmentsController : ApiController
    {
        private ArtDayDBEntities db = new ArtDayDBEntities();

        // GET: api/Enrollments
        public HttpResponseMessage GetEnrollments()
        {
            // Doing this prevents circular reference from session -> preference -> session -> preference...
            var result = db.Enrollments.Select(
                e => new
                {
                    id = e.EnrollmentID,
                    period = e.Period,
                    student = e.StudentID,
                    session = e.SessionID
                }).ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, new { enrollments = result });
        }

        // GET: api/Enrollments/5
        [ResponseType(typeof(Preference))]
        public async Task<HttpResponseMessage> GetEnrollment(int id)
        {
            // Doing this prevents circular reference
            Enrollment enrollment = await db.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { message = "That enrollment does not exist." }, new JsonMediaTypeFormatter());
            }

            var result = new
            {
                id = enrollment.EnrollmentID,
                period = enrollment.Period,
                session = enrollment.SessionID,
                student = enrollment.StudentID
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                enrollment = result
            });
        }

        // PUT: api/Enrollments/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEnrollment(int id, Enrollment enrollment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            enrollment.EnrollmentID = id;

            db.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentExists(id))
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

        // POST: api/Enrollments
        [ResponseType(typeof(Preference))]
        public async Task<IHttpActionResult> PostEnrollment(Enrollment enrollment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Enrollments.Add(enrollment);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }

            var enrollResult = new
            {
                id = enrollment.EnrollmentID,
                period = enrollment.Period,
                session = enrollment.SessionID,
                student = enrollment.StudentID
            };

            return CreatedAtRoute("DefaultApi", new { id = enrollment.EnrollmentID }, new { enrollment = enrollResult });
        }

        // DELETE: api/Enrollments/5
        [ResponseType(typeof(Preference))]
        public async Task<IHttpActionResult> DeleteEnrollment(int id)
        {
            Enrollment enrollment = await db.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            db.Enrollments.Remove(enrollment);
            await db.SaveChangesAsync();

            return Ok(enrollment);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EnrollmentExists(int id)
        {
            return db.Enrollments.Count(e => e.EnrollmentID == id) > 0;
        }
    }
}