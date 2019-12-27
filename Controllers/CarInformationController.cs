using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CarlistApi.data;
using CarlistApi.Models;

namespace CarlistApi.Controllers
{
    public class CarInformationController : ApiController
    {
        CarlistDbContext carlistDbContext = new CarlistDbContext();
        // GET: api/CarInformation/
        public IHttpActionResult Get()
        {
            var carlistInfo = carlistDbContext.CarInformation;
            return Ok(carlistInfo);
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
    }
}
