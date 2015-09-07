using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

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

        // POST: api/Enrollments/PostEnrollment
        [ResponseType(typeof(Preference))]
        public async Task<HttpResponseMessage> PostEnrollment()
        {
            string body = await Request.Content.ReadAsStringAsync();
            List<Enrollment> enrollments = JsonConvert.DeserializeObject<List<Enrollment>>(body);
                        
            DataTable table = new DataTable();
            
            using(var reader = ObjectReader.Create(enrollments)) {
                table.Load(reader);
            }
            table.Columns.Remove("Student");
            table.Columns.Remove("Session");
            table.Columns.Remove("EnrollmentId");

            string cnString = ConfigurationManager.ConnectionStrings["ArtDayConnection"].ConnectionString;
            using (SqlConnection destinationConnection = new SqlConnection(cnString))
            {
                destinationConnection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                {
                    bulkCopy.DestinationTableName = "Enrollment";
                    
                    SqlBulkCopyColumnMapping studentMap = new SqlBulkCopyColumnMapping("studentId", "StudentID");
                    bulkCopy.ColumnMappings.Add(studentMap);

                    SqlBulkCopyColumnMapping sessionMap = new SqlBulkCopyColumnMapping("sessionId", "SessionID");
                    bulkCopy.ColumnMappings.Add(sessionMap);

                    SqlBulkCopyColumnMapping periodMap = new SqlBulkCopyColumnMapping("period", "Period");
                    bulkCopy.ColumnMappings.Add(periodMap);
                    
                    try
                    {
                        bulkCopy.WriteToServer(table);
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                    }                    
                }
            }
            

            // this is very slow:
            // db.Enrollments.AddRange(enrollments);
            // await db.SaveChangesAsync();  // save changes once.  Hopefully this makes a single db call.

            return this.Request.CreateResponse(HttpStatusCode.Created);
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