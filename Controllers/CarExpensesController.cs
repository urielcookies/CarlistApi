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
using CarlistApi.Utils;

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
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult PostCarExpenses(CarExpenses carExpenses)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var utils = new Helper();
            var currentUser = utils.currentUser(db);

            if (utils.isAuthorized(db))
            {
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

            return Ok(carExpenses);
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