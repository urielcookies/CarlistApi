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
using WebPush;
using System.Web;
using System.Web.Script.Serialization;

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
            if (Helper.isAuthorized())
            {
                var currentUser = Helper.currentUser();

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
        [HttpPut]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/carexpenses/{id}")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult PutCarExpenses(int id, CarExpenses carExpenses)
        {
            if (Helper.isAuthorized())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != carExpenses.Id)
                {
                    return BadRequest();
                }

                var currentUser = Helper.currentUser();
                var entity = db.CarExpenses.FirstOrDefault(ci => ci.Id == id);

                // is owner
                var hasAccess = db.CarInformation
                    .Any(c => c.Id == entity.CarInformationId && c.UserAccountId == currentUser.Id);

                // has permissions
                if (!hasAccess)
                {
                    hasAccess = db.CarAccess
                        .Any(c => c.CarInformationId == entity.CarInformationId && c.UserAccountId == currentUser.Id);
                }

                if (hasAccess)
                {
                    // var entity = db.CarExpenses.FirstOrDefault(ci => ci.Id == id);

                    entity.Expense = carExpenses.Expense;
                    entity.Cost = carExpenses.Cost;

                    // Throws at errors - need to look into it more (maybe just bring all clas sprops from client and nor modify them like here above)
                    // db.Entry(carExpenses).State = EntityState.Modified;

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
                else
                {
                    return BadRequest("You have no access to this car or the car does not exist");
                }
            }
            {
                return BadRequest("Bad token");
            }
        }

        // POST: api/CarExpenses
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/carexpenses")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult PostCarExpenses(CarExpenses carExpenses)
        {
            if (Helper.isAuthorized())
            {
                var currentUser = Helper.currentUser();

                // is owner
                var hasAccess = db.CarInformation
                    .Any(c => c.Id == carExpenses.CarInformationId && c.UserAccountId == currentUser.Id);
                var url = !hasAccess
                    ? $"home/mycarlist/{currentUser.Id}/{carExpenses.CarInformationId}/expenses"
                    : $"home/carlist/{currentUser.Id}/{carExpenses.CarInformationId}/expenses";

                // has permissions
                if (!hasAccess)
                {
                    hasAccess = db.CarAccess
                        .Any(c => c.CarInformationId == carExpenses.CarInformationId && c.UserAccountId == currentUser.Id);
                }

                if (hasAccess)
                {
                    var newExpense = new CarExpenses
                    {
                        UserAccountId = currentUser.Id,
                        CarInformationId = carExpenses.CarInformationId,
                        Expense = carExpenses.Expense,
                        Cost = carExpenses.Cost,
                        CreatedTime = DateTime.UtcNow,
                    };

                    var ownerAndAccessors = OwnerAndAccessors(currentUser.Id, carExpenses.CarInformationId, db);
                    var currentCar = db.CarInformation.FirstOrDefault(c => c.Id == newExpense.CarInformationId);
                    var notificationData = new { 
                        title = "New Expense Created",
                        body = $"{currentUser.Username} added {newExpense.Expense} of ${newExpense.Cost} to {currentCar.Year} {currentCar.Brand} {currentCar.Model}",
                        url = url
                    };
                    var json = new JavaScriptSerializer().Serialize(notificationData);

                    foreach (int userId in ownerAndAccessors)
                    {
                        var userSubscription = db.WebSubscriptions.FirstOrDefault(user => user.UserAccountId == userId);
                        if (userSubscription != null && userSubscription.UserAccountId != currentUser.Id)
                        {
                            var subscription = new SubscriptionKeys
                            {
                                Endpoint = userSubscription.Endpoint,
                                P256dh = userSubscription.P256dh,
                                Auth = userSubscription.Auth,
                                Subject = "mailto:urielcookies@outlook.com",
                                PublicKey = "BGtbGS02vyTs8DEeNMU-qkk06y8G_hftexcb9ckqBd8F4bolTd7E5FKhcM7JSOqL-TiVOP-lmxXLB5MjnQDEVeA",
                                PrivateKey = "qyNJkPc4vmlVRnkX3Mh5rbagxtyQzdsAzyllnqG46X0",
                                PayLoad = json
                            };
                            SendNotification(subscription);
                        }
                    }

                    db.CarExpenses.Add(newExpense);
                    db.SaveChanges();
                    return StatusCode(HttpStatusCode.Created);
                }
                else
                {
                    return BadRequest("You have no access to this car or the car does not exist");
                }
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
            if (Helper.isAuthorized())
            {
                var currentUser = Helper.currentUser();
                var carInformationId = db.CarExpenses.FirstOrDefault(c => c.Id == id).CarInformationId;

                // is owner
                var hasAccess = db.CarInformation
                    .Any(c => c.Id == carInformationId && c.UserAccountId == currentUser.Id);

                // has permissions
                if (!hasAccess)
                {
                    hasAccess = db.CarAccess
                        .Any(c => c.CarInformationId == carInformationId && c.UserAccountId == currentUser.Id);
                }

                if (hasAccess)
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
                else
                {
                    return BadRequest("You have no access to this car or the car does not exist");
                }
            }
            else
            {
                return BadRequest("Bad token");
            }
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

        private List<int> OwnerAndAccessors(int currentUserId, int carInformationId, CarlistDbContext db)
        {
            var currentCar = db.CarInformation.FirstOrDefault(c => c.Id == carInformationId);
            
            var notifyOwner = currentCar.UserAccountId != currentUserId;

            var notifyAccessors = db.CarAccess
                .Where(u => u.CarInformationId == currentCar.Id)
                .Select(c => c.UserAccountId).ToList();

            if (notifyOwner)
            {
                notifyAccessors.Add(currentCar.UserAccountId);
            }

            return notifyAccessors;
        }

        private void SendNotification(SubscriptionKeys subscriptionKeys)
        {
            var webPushClient = new WebPushClient();
            try
            {
                var pushSubscription = new PushSubscription(
                   subscriptionKeys.Endpoint,
                   subscriptionKeys.P256dh,
                   subscriptionKeys.Auth
                );

                var vapidDetails = new VapidDetails(
                   subscriptionKeys.Subject,
                   subscriptionKeys.PublicKey,
                   subscriptionKeys.PrivateKey
                );

                webPushClient.SendNotification(pushSubscription, subscriptionKeys.PayLoad, vapidDetails);
            }
            catch (WebPushException exception)
            {
                Console.WriteLine("Http STATUS code" + exception.StatusCode);
            }


        }
        public class SubscriptionKeys
        {
            public string Endpoint { get; set; }
            public string P256dh { get; set; }
            public string Auth { get; set; }
            public string Subject { get; set; }
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
            public string PayLoad { get; set; }
        }
    }
}