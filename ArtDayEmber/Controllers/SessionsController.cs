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

namespace ArtDayEmber.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class SessionsController : ApiController
    {
        private ArtDayDBEntities db = new ArtDayDBEntities();

        // GET: api/Sessions
        public HttpResponseMessage GetSessions()
        {
            // Doing this prevents circular reference from session -> preference -> session -> preference...
            var result = db.Sessions.Select(
                s => new
                {
                    id = s.id,
                    capacity = s.capacity,
                    description = s.description,
                    instructorName = s.instructorName,
                    sessionName = s.sessionName,
                    location = s.location,
                    imageUrl = s.imageUrl,
                    preferences = s.Preferences.Select(p => p.PreferenceID).ToList()
                }).ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, new { sessions = result });
        }

        // GET: api/Sessions/5
        [ResponseType(typeof(Session))]
        public HttpResponseMessage GetSession(int id)
        {
            // Doing this prevents circular reference from session -> preference -> session -> preference...
            var result = db.Sessions.Select(
                s => new
                {
                    id = s.id,
                    capacity = s.capacity,
                    description = s.description,
                    instructorName = s.instructorName,
                    sessionName = s.sessionName,
                    location = s.location,
                    imageUrl = s.imageUrl,
                    preferences = s.Preferences.Select(p => p.PreferenceID).ToList()
                }).Where(s => s.id == id);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { session = result });
        }

        // PUT: api/Sessions/5
        // Updates an existing session
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutSession(int id, Session session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ember passes in id with the URL, and sets the session id to 0. Fixing that here.
            session.id = id;

            db.Entry(session).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionExists(id))
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

        // POST: api/Sessions
        // Creates a new session
        [ResponseType(typeof(Session))]
        public async Task<HttpResponseMessage> PostSession(Session session)
        {
            if (!ModelState.IsValid)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            // Ember expects the POST result to contain json in an array - so we use a list.
            
            var temp = new {
                id = session.id,
                sessionName = session.sessionName,
                instructorName = session.instructorName,
                capacity = session.capacity,
                description = session.description,
                location = session.location,
                imageUrl = session.imageUrl
                };
            
            return this.Request.CreateResponse(HttpStatusCode.Created, new { session = temp });
        }

        // DELETE: api/Sessions/5
        [ResponseType(typeof(Session))]
        public async Task<IHttpActionResult> DeleteSession(int id)
        {
            Session session = await db.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            db.Sessions.Remove(session);
            await db.SaveChangesAsync();

            return Ok(session);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SessionExists(int id)
        {
            return db.Sessions.Count(e => e.id == id) > 0;
        }
    }
}