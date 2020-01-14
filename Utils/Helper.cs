using AuthenticationService.Managers;
using AuthenticationService.Models;
using CarlistApi.data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;
using CarlistApi.Models;

namespace CarlistApi.Utils
{
    public class Helper
    {
        public bool isAuthorized(CarlistDbContext db)
        {

            HttpCookie cookie = HttpContext.Current.Request.Cookies["token"];
            if (cookie != null)
            {
                var tokenCookie = HttpContext.Current.Request.Cookies["token"].Value;
                var jwtToken = new JwtSecurityToken(tokenCookie);
                var tokenEmail = jwtToken.Claims.First(c => c.Type == "email").Value;

                IAuthContainerModel model = GetJWTContainerModel(tokenEmail);
                IAuthService authService = new JWTService(model.SecretKey);

                List<Claim> claims = authService.GetTokenClaims(tokenCookie).ToList();
                var email = claims.FirstOrDefault(e => e.Type.Equals(ClaimTypes.Email)).Value;
                var user = db.UserAccounts.SingleOrDefault(ua => ua.Email == email);

                if (user != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            {
                return false;
            }
        }

        public UserAccounts currentUser(CarlistDbContext db)
        {
            var tokenCookie = HttpContext.Current.Request.Cookies["token"].Value;
            var jwtToken = new JwtSecurityToken(tokenCookie);
            var tokenEmail = jwtToken.Claims.First(c => c.Type == "email").Value;

            IAuthContainerModel model = GetJWTContainerModel(tokenEmail);
            IAuthService authService = new JWTService(model.SecretKey);

            List<Claim> claims = authService.GetTokenClaims(tokenCookie).ToList();
            var email = claims.FirstOrDefault(e => e.Type.Equals(ClaimTypes.Email)).Value;
            var user = db.UserAccounts.SingleOrDefault(ua => ua.Email == email);
            return user;
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