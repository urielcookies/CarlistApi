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
using CarlistApi.Utils;

namespace CarlistApi.Controllers
{
    public class CarStatusController : ApiController
    {
        private CarlistDbContext db = new CarlistDbContext();

        // GET: api/carstatus/{carid}
        [HttpGet]
        [Route("api/carstatus/{carid}")]
        public IHttpActionResult getCarStatus(int carid)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var entity = db.CarStatus.FirstOrDefault(cs => cs.CarInformationId == carid);
            if (entity == null)
                return BadRequest("Car does not exit");

            return Ok(entity);
        }

        // GET: api/CarStatus
        public IQueryable<CarStatus> GetCarStatus()
        {
            return db.CarStatus;
        }

        // GET: api/CarStatus/5
        [ResponseType(typeof(CarStatus))]
        public IHttpActionResult GetCarStatus(int id)
        {
            CarStatus carStatus = db.CarStatus.Find(id);
            if (carStatus == null)
            {
                return NotFound();
            }

            return Ok(carStatus);
        }

        // PUT: api/CarStatus/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCarStatus(int id, CarStatus carStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != carStatus.Id)
            {
                return BadRequest();
            }

            db.Entry(carStatus).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarStatusExists(id))
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

        // POST: api/CarStatus
        [ResponseType(typeof(CarStatus))]
        public IHttpActionResult PostCarStatus(CarStatus carStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.CarStatus.Add(carStatus);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = carStatus.Id }, carStatus);
        }

        // DELETE: api/CarStatus/5
        [ResponseType(typeof(CarStatus))]
        public IHttpActionResult DeleteCarStatus(int id)
        {
            CarStatus carStatus = db.CarStatus.Find(id);
            if (carStatus == null)
            {
                return NotFound();
            }

            db.CarStatus.Remove(carStatus);
            db.SaveChanges();

            return Ok(carStatus);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CarStatusExists(int id)
        {
            return db.CarStatus.Count(e => e.Id == id) > 0;
        }
    }
}