using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarlistApi.Models
{
    public class UserAccounts
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public System.DateTime CreatedTime { get; set; }
    }
}