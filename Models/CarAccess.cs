using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CarlistApi.Models
{
    public class CarAccess
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int CarInformationId { get; set; }
        public Nullable<bool> Write { get; set; }
        public System.DateTime CreatedTime { get; set; }
    }
}