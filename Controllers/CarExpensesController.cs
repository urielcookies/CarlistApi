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
using System.Web.Http.Cors;

namespace CarlistApi.Controllers
{
    public class CarExpensesController : ApiController
    {
        private CarlistDbContext db = new CarlistDbContext();

        // GET: api/CarExpenses
        public IQueryable<CarExpenses> GetCarExpenses()
        {
            return db.CarExpenses;
        } 

        // GET: api/CarExpenses/
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/carexpenses/{carInformationId}")]
        public IHttpActionResult GetSingleCarExpenses(int carInformationId)
        {
            var utils = new Helper();
            if (utils.isAuthorized(db))
            {
                var currentUser = utils.currentUser(db);

                var hasAccess = db.CarInformation
                    .Any(c => c.Id == carInformationId && c.UserAccountId == currentUser.Id);

                if(!hasAccess)
                {
                    hasAccess = db.CarAccess
                        .Any(c => c.CarInformationId == carInformationId && c.UserAccountId == currentUser.Id);
                }

                
                if (hasAccess)
                {
                    IQueryable expenseList = db.CarExpenses.Where(s => s.CarInformationId == carInformationId);
                    return Ok(expenseList);
                } else
                {
                    return BadRequest("You have no access to this car or the car does not exist");
                }
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        // GET: api/CarExpenses/5
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult GetCarExpenses(int id)
        {
            CarExpenses carExpenses = db.CarExpenses.Find(id);
            if (carExpenses == null)
            {
                return NotFound();
            }

            return Ok(carExpenses);
        }

        // PUT: api/CarExpenses/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCarExpenses(int id, CarExpenses carExpenses)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != carExpenses.Id)
            {
                return BadRequest();
            }

            db.Entry(carExpenses).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExpensesExists(id))
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

        // POST: api/CarExpenses
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/carexpenses")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult PostCarExpenses(CarExpenses carExpenses)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            var utils = new Helper();
            if (utils.isAuthorized(db))
            {
                var currentUser = utils.currentUser(db);
                var newExpense = new CarExpenses
                {
                    UserAccountId = currentUser.Id,
                    CarInformationId = carExpenses.CarInformationId,
                    Expense = carExpenses.Expense,
                    Cost = carExpenses.Cost,
                    CreatedTime = DateTime.UtcNow,
                };

                db.CarExpenses.Add(newExpense);
                db.SaveChanges();
                return StatusCode(HttpStatusCode.Created);
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        // DELETE: api/CarExpenses/5
        [HttpDelete]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/carexpenses/{id}")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult DeleteCarExpenses(int id)
        {
            CarExpenses carExpenses = db.CarExpenses.Find(id);
            if (carExpenses == null)
            {
                return NotFound();
            }

            db.CarExpenses.Remove(carExpenses);
            db.SaveChanges();

            return Ok(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CarExpensesExists(int id)
        {
            return db.CarExpenses.Count(e => e.Id == id) > 0;
        }
    }
}