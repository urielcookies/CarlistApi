using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using CarlistApi.Models;
using CarlistApi.data;
using System.Web.Helpers;
using System.Web;
using AuthenticationService.Models;
using AuthenticationService.Managers;
using System.Security.Claims;
using System.Web.Http.Cors;
using CarlistApi.Utils;
using WebPush;
using Newtonsoft.Json;

namespace CarlistApi.Controllers
{
    public class UserAccountsController : ApiController
    {
        private CarlistDbContext db = new CarlistDbContext();

        // GET: api/UserAccounts
        public IQueryable<UserAccounts> GetUserAccounts()
        {
            return db.UserAccounts;
        }

        // GET: api/UserAccounts/5
        [ResponseType(typeof(UserAccounts))]
        public IHttpActionResult GetUserAccounts(int id)
        {
            UserAccounts userAccounts = db.UserAccounts.Find(id);
            if (userAccounts == null)
            {
                return NotFound();
            }

            return Ok(userAccounts);
        }

        // PUT: api/UserAccounts/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserAccounts(int id, UserAccounts userAccounts)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userAccounts.Id)
            {
                return BadRequest();
            }

            db.Entry(userAccounts).State = EntityState.Modified;

            try
            {
                //db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAccountsExists(id))
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

        // POST: api/useraccounts/register
        [Route("api/useraccounts/register")]
        public IHttpActionResult Register(UserAccounts userAccounts)
        {
            var username = userAccounts.Username;
            var email = userAccounts.Email;

            if (!IsValidEmail(email))
            {
                return BadRequest("Fix email format");
            }

            var isNewEmail = db.UserAccounts.Any(s => s.Email == email);
            var isNewUsername = db.UserAccounts.Any(s => s.Username == username);

            if (isNewEmail)
            {
                return BadRequest("This email is taking already");
            }
            else if (isNewUsername)
            {
                return BadRequest("This username is taking already");
            }

            var newUser = new UserAccounts
            {
                Username = username,
                Email = email,
                Password = Crypto.HashPassword(userAccounts.Password),
                CreatedTime = DateTime.UtcNow,
            };

            db.UserAccounts.Add(newUser);
            db.SaveChanges();

            return Ok(HttpStatusCode.Created);
        }

        // POST: api/useraccounts/login
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/useraccounts/login")]
        public IHttpActionResult Login(UserAccounts userAccounts)
        {
            var email = userAccounts.Email;
            var password = userAccounts.Password;

            var user = db.UserAccounts.SingleOrDefault(ua => ua.Email == email);
            if (user == null)
            {
                return BadRequest("Email or password are incorrect");
            }

            var verified = Crypto.VerifyHashedPassword(user.Password, password);
            if (!verified)
            {
                return BadRequest("Email or password are incorrect");
            }

            IAuthContainerModel model = GetJWTContainerModel(email);
            IAuthService authService = new JWTService(model.SecretKey);
            string jwtToken = authService.GenerateToken(model);

            HttpCookie token = new HttpCookie("token");
            token.HttpOnly = true;
            token.Value = jwtToken;
            token.SameSite = (SameSiteMode)(1);
            token.Domain = Request.RequestUri.Host;
            HttpContext.Current.Response.Cookies.Add(token);
            return Ok(jwtToken);
        }

        [HttpPut]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/useraccounts/changePassword")]
        [ResponseType(typeof(UserAccounts))]
        public IHttpActionResult ChangePassword([FromBody]UserInfo passwords)
        {
            var utils = new Helper();
            if (utils.isAuthorized(db))
            {
                var currentUser = utils.currentUser(db);
                var currentPassword = passwords.currentPassword;
                var newPassword = passwords.newPassword;

                var verified = Crypto.VerifyHashedPassword(currentUser.Password, currentPassword);
                if (verified)
                {
                    currentUser.Password = Crypto.HashPassword(newPassword);
                    db.SaveChanges();
                    return Ok(HttpStatusCode.OK);
                }
                else
                {
                    return BadRequest("Could not verify password");
                }
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/useraccounts/getuserinfo")]
        [ResponseType(typeof(UserAccounts))]
        public IHttpActionResult GetUserInfo()
        {
            var utils = new Helper();
            if (utils.isAuthorized(db))
            {
                var currentUser = utils.currentUser(db);
                var publicAccountInfo = new PublicAccountInfo
                {
                    Id = currentUser.Id,
                    Username = currentUser.Username,
                    Email = currentUser.Email,
                    CreatedTime = currentUser.CreatedTime,
                };
                return Ok(publicAccountInfo);
            }
            else
            {
                return BadRequest("Bad token");
            }
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/useraccounts/test-push-next")]
        [ResponseType(typeof(UserAccounts))]
        public IHttpActionResult testPusdddh([FromBody]Subscription subscription)
        {
            var webPushClient = new WebPushClient();
            try
            {

                var pushSubscription = new PushSubscription(
                    "https://fcm.googleapis.com/fcm/send/cwMpSClYJ-g:APA91bGHMkUqztHG2AtSNW-aGSWgWIaUsjbRwy2dPVmOPBTwhidvnEqUJdcJaH5T7Ud4dozWkBncyBzFsrGG7t7-z5-u90UgEZ7qJCpefr048h7NeLuSy6RQXqiP7KVBAwsqHPV178Dm",
                    "BIDddyYtaVpA2FwTa - edvrtbRIm8ZeMmhZ3t7qkGGLWpqfcybhxnisKK99uc2QvQSC4wX6f3pgiFKtuNaeq1k10",
                    "l18IyjqdkI0DVI9dUkyYKQ"
                );


                var vapidDetails = new VapidDetails(
                    "mailto:urielcookies@outlook.com",
                    "BGtbGS02vyTs8DEeNMU-qkk06y8G_hftexcb9ckqBd8F4bolTd7E5FKhcM7JSOqL-TiVOP-lmxXLB5MjnQDEVeA",
                    "qyNJkPc4vmlVRnkX3Mh5rbagxtyQzdsAzyllnqG46X0"
                );

                //var payloadzzz = new { title = "WAT UP" };
                //var payloadx = new JavaScriptSerializer().Serialize(payloadzzz);

                webPushClient.SendNotification(pushSubscription, "", vapidDetails);
            }
            catch (WebPushException exception)
            {
                Console.WriteLine("Http STATUS code" + exception.StatusCode);
            }

            return Ok("Greetings Humans");
        }

        // POST: api/UserAccounts
        [ResponseType(typeof(UserAccounts))]
        public IHttpActionResult PostUserAccounts(UserAccounts userAccounts)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.UserAccounts.Add(userAccounts);
            //db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = userAccounts.Id }, userAccounts);
        }

        // DELETE: api/UserAccounts/5
        [ResponseType(typeof(UserAccounts))]
        public IHttpActionResult DeleteUserAccounts(int id)
        {
            UserAccounts userAccounts = db.UserAccounts.Find(id);
            if (userAccounts == null)
            {
                return NotFound();
            }

            db.UserAccounts.Remove(userAccounts);
            //db.SaveChanges();

            return Ok(userAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserAccountsExists(int id)
        {
            return db.UserAccounts.Count(e => e.Id == id) > 0;
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
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

        public class UserInfo
        {
            public String currentPassword { get; set; }
            public String newPassword { get; set; }
        }

        public class PublicAccountInfo
        {
            public int Id { get; set; }
            public String Username { get; set; }
            public String Email { get; set; }
            public DateTime CreatedTime { get; set; }
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