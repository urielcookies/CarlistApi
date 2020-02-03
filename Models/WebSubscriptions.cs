using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarlistApi.Models
{
    public class WebSubscriptions
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
        public System.DateTime CreatedTime { get; set; }
    }
}