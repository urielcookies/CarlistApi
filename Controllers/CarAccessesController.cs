using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using CarlistApi.Models;
using CarlistApi.data;
using System.Web.Http.Cors;
using CarlistApi.Utils;

namespace CarlistApi.Controllers
{
    public class CarAccessesController : ApiController
    {
        private CarlistDbContext db = new CarlistDbContext();

        // GET: api/CarAccesses
        public IQueryable<CarAccess> GetCarAccess()
        {
            return db.CarAccess;
        }

        // GET: api/CarAccess
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/caraccess/getusernames")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult GetUsersNames()
        {
            if (Helper.isAuthorizedJWT())
            {
                var currentUser = Helper.currentUser();
                var queryCarIds = db.CarAccess
                   .Where(s => s.UserAccountId == currentUser.Id)
                   .Select(x => x.CarInformationId);

                var permittedCars = db.CarInformation
                    .Where(m => queryCarIds.Contains(m.Id))
                    .Select(x => x.UserAccountId)
                    .Distinct()
                    .ToList();

                var users = db.UserAccounts
                    .Where(m => permittedCars.Contains(m.Id))
                    .Select(x => new { x.Id, x.Username, x.Email});

                return Ok(users);
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        // POST: api/CarAccesses/giveaccess
        [HttpPost]
        [Route("api/caraccess/give-car-access")]
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult GiveCarAccess(int id)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            return Ok("Check if owner then give permissions based on read/write");
        }

        // GET: api/CarAccesses/5
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult GetCarAccess(int id)
        {
            CarAccess carAccess = db.CarAccess.Find(id);
            if (carAccess == null)
            {
                return NotFound();
            }

            return Ok(carAccess);
        }

        [HttpGet]
        [Route("api/caraccess/get-permissions/{carId}")]
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult FetchPermissions(int carid)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var carEntity = db.CarInformation.Any(ci => ci.Id == carid);
            if (!carEntity)
                return BadRequest("Car does not exit");

            var userPermissionRank = Helper.UserHasCarPermission(carid);
            var userHasWritePermissions = Helper.PermissionType.OWNER == userPermissionRank
                || Helper.PermissionType.WRITE == userPermissionRank;

            return Ok(userHasWritePermissions);
        }

        // PUT: api/CarAccesses/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCarAccess(int id, CarAccess carAccess)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != carAccess.Id)
            {
                return BadRequest();
            }

            db.Entry(carAccess).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarAccessExists(id))
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

        // POST: api/CarAccesses
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult PostCarAccess(CarAccess carAccess)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.CarAccess.Add(carAccess);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = carAccess.Id }, carAccess);
        }

        // DELETE: api/CarAccesses/5
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult DeleteCarAccess(int id)
        {
            CarAccess carAccess = db.CarAccess.Find(id);
            if (carAccess == null)
            {
                return NotFound();
            }

            db.CarAccess.Remove(carAccess);
            db.SaveChanges();

            return Ok(carAccess);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CarAccessExists(int id)
        {
            return db.CarAccess.Count(e => e.Id == id) > 0;
        }
    }
}