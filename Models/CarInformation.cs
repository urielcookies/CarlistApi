using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CarlistApi.Models
{
    public class CarInformation
    {
        public int Id { get; set; }
        public Nullable<short> Year { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public Nullable<double> Cost { get; set; }
        public Nullable<bool> CleanTitle { get; set; }
        public string Notes { get; set; }
        public int UserAccountId { get; set; }
        public string Partner { get; set; }
        public System.DateTime CreatedTime { get; set; }
    }
}