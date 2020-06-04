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

            var carEntity = db.CarInformation.Any(ci => ci.Id == carid);
            if (!carEntity)
                return BadRequest("Car does not exit");

            var carStatus = db.CarStatus.FirstOrDefault(cs => cs.CarInformationId == carid);

            return Ok(carStatus);
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
        [HttpPut]
        [Route("api/carstatus")]
        [ResponseType(typeof(CarStatus))]
        public IHttpActionResult PutCarStatus(CarStatusObj carStatus)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var userHasCarPermission = Helper.UserHasCarPermission(carStatus.CarInformationId);
            if (userHasCarPermission == Helper.PermissionType.NONE)
                return BadRequest("User has no access");

            if (userHasCarPermission == Helper.PermissionType.READ)
                return BadRequest("User has no permission to change");

            var carEntity = db.CarStatus.FirstOrDefault(ci => ci.CarInformationId == carStatus.CarInformationId);
            
            if (carEntity == null)
                return BadRequest("Car does not exit");

            carEntity.Sold = carStatus.Sold;
            carEntity.PriceSold = carStatus.PriceSold;
            carEntity.YearSold = carStatus.YearSold;

            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        //// POST: api/CarStatus
        //[ResponseType(typeof(CarStatus))]
        //public IHttpActionResult PostCarStatus(CarStatus carStatus)
        //{
        //    if (!Helper.isAuthorizedJWT())
        //        return BadRequest("Bad token");

        //    var userHasCarPermission = Helper.UserHasCarPermission(carStatus.CarInformationId);
        //    if (userHasCarPermission == Helper.PermissionType.NONE)
        //        return BadRequest("User has no access");

        //    if (userHasCarPermission == Helper.PermissionType.READ)
        //        return BadRequest("User has no permission to post image");

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var carStatusEntity = db.CarStatus.FirstOrDefault(cs => carStatus.CarInformationId == cs.CarInformationId);
        //    carStatus.UserAccountId = Helper.currentUser().Id;
        //    if (carStatusEntity == null)
        //    {  
        //        carStatus.CreatedTime = DateTime.UtcNow;
        //        db.CarStatus.Add(carStatus);
        //    }
        //    else
        //    {
        //        carStatusEntity.Sold = carStatus.Sold;
        //        if (carStatus.Sold == false)
        //        {
        //            carStatusEntity.PriceSold = null;
        //            carStatusEntity.YearSold = null;
        //        }
        //        else
        //        {
        //            carStatusEntity.PriceSold = carStatus.PriceSold;
        //            carStatusEntity.YearSold = carStatus.YearSold;
        //        }
        //    }

        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = carStatus.Id }, carStatusEntity);
        //}

        // DELETE: api/CarStatus/5
        [HttpPut]
        [Route("api/carstatus/{carId}")]
        [ResponseType(typeof(CarStatus))]
        public IHttpActionResult DeleteCarStatus(int carId)
        {
            CarStatus carEntity = db.CarInformation.Find(carId);
            if (carEntity == null)
            {
                return NotFound();
            }

            db.CarStatus.Remove(carEntity);
            db.SaveChanges();

            return Ok(carEntity);
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
        public class CarStatusObj
        {
            public int CarInformationId { get; set; }
            public int PriceSold { get; set; }
            public bool Sold { get; set; }
            public short YearSold { get; set; }
        }
    }
}