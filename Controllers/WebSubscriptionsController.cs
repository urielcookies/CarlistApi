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
    public class WebSubscriptionsController : ApiController
    {
        private CarlistDbContext db = new CarlistDbContext();

        // GET: api/WebSubscriptions
        public IQueryable<WebSubscriptions> GetWebSubscriptions()
        {
            return db.WebSubscriptions;
        }

        // GET: api/WebSubscriptions/5
        [ResponseType(typeof(WebSubscriptions))]
        public IHttpActionResult GetWebSubscriptions(int id)
        {
            WebSubscriptions webSubscriptions = db.WebSubscriptions.Find(id);
            if (webSubscriptions == null)
            {
                return NotFound();
            }

            return Ok(webSubscriptions);
        }

        // Post: api/WebSubscriptions/5
        [HttpPost]
        [Route("api/websubscriptions/check-subscription")]
        [ResponseType(typeof(WebSubscriptions))]
        public IHttpActionResult CheckSubscription(Subscription subscription)
        {
            if (Helper.isAuthorizedJWT())
            {
                var currentUser = Helper.currentUser();
                var webSubscription = db.WebSubscriptions.FirstOrDefault(w => w.UserAccountId == currentUser.Id);

                if (webSubscription == null)
                {
                    return NotFound();
                }

                if (webSubscription.Endpoint == subscription.endpoint)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }

            }
            {
                return BadRequest("Bad token");
            }
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/websubscriptions/insert-subscription")]
        [ResponseType(typeof(WebSubscriptions))]
        public IHttpActionResult InsertWebSubscription([FromBody]Subscription subscription)
        {
            if (Helper.isAuthorizedJWT())
            {
                var currentUser = Helper.currentUser();
                var subscriptionEntity = db.WebSubscriptions.FirstOrDefault(c => c.UserAccountId == currentUser.Id);

                if (subscriptionEntity != null)
                {
                    subscriptionEntity.Endpoint = subscription.endpoint;
                    subscriptionEntity.P256dh = subscription.keys.p256dh;
                    subscriptionEntity.Auth = subscription.keys.auth;

                    db.SaveChanges();
                } else
                {
                    var newSubscription = new WebSubscriptions
                    {
                        UserAccountId = currentUser.Id,
                        Endpoint = subscription.endpoint,
                        P256dh = subscription.keys.p256dh,
                        Auth = subscription.keys.auth,
                        CreatedTime = DateTime.UtcNow,
                    };

                    db.WebSubscriptions.Add(newSubscription);
                    db.SaveChanges();
                }
                return Ok(HttpStatusCode.Created);
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        // PUT: api/WebSubscriptions/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutWebSubscriptions(int id, WebSubscriptions webSubscriptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != webSubscriptions.Id)
            {
                return BadRequest();
            }

            db.Entry(webSubscriptions).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebSubscriptionsExists(id))
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

        // POST: api/WebSubscriptions
        [ResponseType(typeof(WebSubscriptions))]
        public IHttpActionResult PostWebSubscriptions(WebSubscriptions webSubscriptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.WebSubscriptions.Add(webSubscriptions);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = webSubscriptions.Id }, webSubscriptions);
        }

        // DELETE: api/WebSubscriptions/5
        [ResponseType(typeof(WebSubscriptions))]
        public IHttpActionResult DeleteWebSubscriptions(int id)
        {
            WebSubscriptions webSubscriptions = db.WebSubscriptions.Find(id);
            if (webSubscriptions == null)
            {
                return NotFound();
            }

            db.WebSubscriptions.Remove(webSubscriptions);
            db.SaveChanges();

            return Ok(webSubscriptions);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WebSubscriptionsExists(int id)
        {
            return db.WebSubscriptions.Count(e => e.Id == id) > 0;
        }

        public class Subscription
        {
            public string endpoint { get; set; }
            public SubscriptionKeys keys { get; set; }
        }

        public class SubscriptionKeys
        {
            public string p256dh { get; set; }
            public string auth { get; set; }
        }
    }
}