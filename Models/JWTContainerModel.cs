using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Models
{
    public class JWTContainerModel : IAuthContainerModel
    {
        #region Public Methods
        public int ExpireMinutes { get; set; } = 525600; // 1 Year.
        public string SecretKey { get; set; } = "EVERf3JzWbTSP8GEXMvrzpTyURIELRMwRyYwSVkfHRzNDsgycAjwHqchyJGARCIA"; // This secret key should be moved to some configurations outter server.
        public string SecurityAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256Signature;

        public Claim[] Claims { get; set; }
        #endregion
    }
}