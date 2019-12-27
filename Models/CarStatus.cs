using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CarlistApi.Models
{
    public class CarStatus
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int CarInformationId { get; set; }
        public bool Sold { get; set; }
        public double PriceSold { get; set; }
        public short YearSold { get; set; }
        public System.DateTime CreatedTime { get; set; }
    }
}