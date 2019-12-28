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
using System.Web.Helpers;
using System.Web;
using AuthenticationService.Models;
using AuthenticationService.Managers;
using System.Security.Claims;

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
            token.Value = jwtToken;
            HttpContext.Current.Response.Cookies.Add(token);
            return Ok(HttpStatusCode.Accepted);
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
    }
}