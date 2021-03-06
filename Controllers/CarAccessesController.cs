﻿using System;
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

        // POST: api/caraccess/give-car-access
        [HttpPost]
        [Route("api/caraccess/give-car-access")]
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult GiveCarAccesses(GiveCarAccess giveCarAccess)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var userHasCarPermission = Helper.UserHasCarPermission(giveCarAccess.CarInformationId);
            if (userHasCarPermission != Helper.PermissionType.OWNER)
                return BadRequest("User needs to be owner to give car access");

            if (!Helper.isAuthorizedJWT())
                return BadRequest("Access exists for this user");

            var userId = db.UserAccounts
                .FirstOrDefault(ua => ua.Username.ToLower() == giveCarAccess.Username.ToLower()).Id;

            var carAccessEntity = db.CarAccess.Any(ca => ca.UserAccountId == userId);
            if (!carAccessEntity)
                return BadRequest("Access exists for this user");

            var carAccess = new CarAccess();
            carAccess.UserAccountId = userId;
            carAccess.CarInformationId = giveCarAccess.CarInformationId;
            carAccess.Write = giveCarAccess.Write;
            carAccess.CreatedTime = DateTime.UtcNow;

            db.CarAccess.Add(carAccess);
            db.SaveChanges();

            return Ok(carAccess);
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

        // GET: api/caraccess/get-permissions/5}
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

            // db.CarAccess.Add(carAccess);
            // db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = carAccess.Id }, carAccess);
        }

        // GET: api/caraccess/get-all-car-permissions
        [HttpGet]
        [Route("api/caraccess/get-all-car-permissions/{carId}")]
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult GetAllUserCarPermissions(int carId)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var carEntity = db.CarInformation.Any(ci => ci.Id == carId);
            if (!carEntity)
                return BadRequest("Car does not exit");

            var userHasCarPermission = Helper.UserHasCarPermission(carId);
            if (userHasCarPermission != Helper.PermissionType.OWNER)
                return BadRequest("User needs to be owner to give car access");

            var permittedUsers = db.CarAccess.Where(s => s.CarInformationId == carId);
            var allUserAccounts = db.UserAccounts.Select(x => 
                new { 
                    id = x.Id,
                    username = x.Username,
                    email = x.Email,
                }
            );

            Dictionary<int, object> userObject = new Dictionary<int, object>();
  
            foreach (var _user in allUserAccounts)
            {
                userObject.Add(_user.id, _user);
            }

            IList<UserInfo> subjects = new List<UserInfo>();
            dynamic _userObject = userObject;
            foreach (var user in permittedUsers)
            {

                var userInfo = new UserInfo();
                userInfo.UserId = user.Id;
                userInfo.Username = _userObject[user.UserAccountId].username;
                userInfo.Email = _userObject[user.UserAccountId].email;
                userInfo.Write = user.Write;
                userInfo.CreatedTime = user.CreatedTime;

                subjects.Add(userInfo);
            }


            return Ok(subjects);
        }

        // PUT: api/CarStatus/5
        [HttpPut]
        [Route("api/caraccess")]
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult PutCarStatus(CarAccessObj carAccess)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var userHasCarPermission = Helper.UserHasCarPermission(carAccess.CarInformationId);
            if (userHasCarPermission == Helper.PermissionType.NONE)
                return BadRequest("User has no access");

            if (userHasCarPermission == Helper.PermissionType.READ)
                return BadRequest("User has no permission to change");

            var carEntity = db.CarAccess.FirstOrDefault(ci => ci.CarInformationId == carAccess.CarInformationId);

            if (carEntity == null)
                return BadRequest("Car does not exit");

            carEntity.Write = carAccess.WriteBool;

            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/CarAccesses/5
        [ResponseType(typeof(CarAccess))]
        public IHttpActionResult DeleteCarAccess(int carId) // replace caraccess with postobject of userId and carInfoId
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            CarAccess carAccess = db.CarAccess.FirstOrDefault(cs => cs.CarInformationId == carId);
            if (carAccess == null)
                return BadRequest("Car does not exit");

            var userHasCarPermission = Helper.UserHasCarPermission(carId);
            if (userHasCarPermission != Helper.PermissionType.OWNER)
                return BadRequest("User needs to be owner to give car access");

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

        public class GiveCarAccess
        {
            public string Username { get; set; }
            public int CarInformationId { get; set; }
            public bool Write { get; set; }
        }

        public class UserInfo
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public Nullable<bool> Write { get; set; }
            public System.DateTime CreatedTime { get; set; }

        }

        public class CarAccessObj
        {
            public int CarInformationId { get; set; }
            public bool WriteBool { get; set; }
        }
    }
}