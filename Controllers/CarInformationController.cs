﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using AuthenticationService.Managers;
using AuthenticationService.Models;
using CarlistApi.data;
using CarlistApi.Models;
using CarlistApi.Utils;

namespace CarlistApi.Controllers
{
    public class CarInformationController : ApiController
    {
        CarlistDbContext carlistDbContext = new CarlistDbContext();
        // check token and check for email match in database to allow access
        // GET: api/CarInformation/
        public IHttpActionResult Get()
        {
            var utils = new Helper();
            if (utils.isAuthorized(carlistDbContext))
            {
                List<object> usersCars = new List<object>();

                // Need to retrieve own users cars instead of all
                foreach (var carRow in carlistDbContext.CarInformation)
                {
                    usersCars.Add(carRow);
                }

                return Ok(usersCars);
            } else
            {
                return BadRequest("Bad token");
            }
        }

        // GET: api/CarInformation/
        [Route("api/carinformation/access-other-cars")]
        public IHttpActionResult GetAccessCars()
        {
            var utils = new Helper();
            var currentUser = utils.currentUser(carlistDbContext);
            if (utils.isAuthorized(carlistDbContext))
            {
                var queryCarIds = carlistDbContext.CarAccess
                   .Where(s => s.UserAccountId == currentUser.Id).Select(x => x.CarInformationId); ;

                var permittedCars = carlistDbContext.CarInformation.Where(m => queryCarIds.Contains(m.Id));

                return Ok(permittedCars);
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        // GET: api/CarInformation/5
        public IHttpActionResult Get(int id)
        {
            var car = carlistDbContext.CarInformation.Find(id);
            if (car == null)
            {
                return BadRequest("No record found on this id...");
            }
            return Ok(car);
        }

        // GET: api/CarInformation/test
        [HttpGet]
        [Route("api/carinformation/test/{id}")]
        public IHttpActionResult Test(int id)
        {
            return Ok(id);
        }

        // POST: api/CarInformation
        public IHttpActionResult Post([FromBody]CarInformation carInfo)
        {
            // Check for validation on db fields
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            carlistDbContext.CarInformation.Add(carInfo);
            carlistDbContext.SaveChanges();
            return StatusCode(HttpStatusCode.Created);
        }

        // PUT: api/CarInformation/5
        public IHttpActionResult Put(int id, [FromBody]CarInformation carInfo)
        {
            var entity = carlistDbContext.CarInformation.FirstOrDefault(ci => ci.Id == id);
            if (entity == null)
            {
                return BadRequest("No record found on this id...");
            }

            // Check for validation on db fields
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entity.Partner = carInfo.Partner;
            entity.Year = carInfo.Year;
            entity.Brand = carInfo.Brand;
            entity.Model = carInfo.Model;
            entity.Cost = carInfo.Cost;
            entity.CleanTitle = carInfo.CleanTitle;
            entity.Notes = carInfo.Notes;

            carlistDbContext.SaveChanges();
            return Ok("Record updated sucessfully...");
        }

        // DELETE: api/CarInformation/5
        public IHttpActionResult Delete(int id)
        {
            var carInfo = carlistDbContext.CarInformation.Find(id);
            if (carInfo == null)
            {
                return BadRequest("Record does not exist");
            }
            carlistDbContext.CarInformation.Remove(carInfo);
            carlistDbContext.SaveChanges();
            return Ok("Record deleted");
        }

        private static JWTContainerModel GetJWTContainerModel(string email)
        {
            return new JWTContainerModel()
            {
                Claims = new Claim[]
                {
                    new Claim(ClaimTypes.Email, email)
                }
            };
        }
    }
}
