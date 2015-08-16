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
    public class PreferencesController : ApiController
    {
        private ArtDayDBEntities db = new ArtDayDBEntities();

        // GET: api/Preferences
        public HttpResponseMessage GetPreferences()
        {
            // Doing this prevents circular reference from session -> preference -> session -> preference...
            var result = db.Preferences.Select(
                p => new
                {
                    id = p.PreferenceID,
                    rank = p.Rank,
                    student = p.StudentID,
                    session = p.SessionID                    
                }).ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, new { preferences = result });
        }

        // GET: api/Preferences/5
        [ResponseType(typeof(Preference))]
        public async Task<HttpResponseMessage> GetPreference(int id)
        {
            // Doing this prevents circular reference
            Preference pref = await db.Preferences.FindAsync(id);

            if (pref == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { message = "That preference does not exist." }, new JsonMediaTypeFormatter());
            }

            var prefResult = new
            {
                id = pref.PreferenceID,
                rank = pref.Rank,
                session = pref.SessionID,
                student = pref.StudentID
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, new { 
                preference = prefResult
            });
        }

        // PUT: api/Preferences/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutPreference(int id, Preference preference)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            preference.PreferenceID = id;

            db.Entry(preference).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PreferenceExists(id))
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

        // POST: api/Preferences
        [ResponseType(typeof(Preference))]
        public async Task<IHttpActionResult> PostPreference(Preference preference)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Preferences.Add(preference);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }

            var prefResult = new
            {
                id = preference.PreferenceID,
                rank = preference.Rank,
                session = preference.SessionID,
                student = preference.StudentID
            };

            return CreatedAtRoute("DefaultApi", new { id = preference.PreferenceID }, new { preference = prefResult });
        }

        // DELETE: api/Preferences/5
        [ResponseType(typeof(Preference))]
        public async Task<IHttpActionResult> DeletePreference(int id)
        {
            Preference preference = await db.Preferences.FindAsync(id);
            if (preference == null)
            {
                return NotFound();
            }

            db.Preferences.Remove(preference);
            await db.SaveChangesAsync();

            return Ok(preference);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PreferenceExists(int id)
        {
            return db.Preferences.Count(e => e.PreferenceID == id) > 0;
        }
    }
}