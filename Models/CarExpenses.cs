using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CarlistApi.Models
{
    public class CarExpenses
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int CarInformationId { get; set; }
        public string Expense { get; set; }
        public double Cost { get; set; }
        public System.DateTime CreatedTime { get; set; }
    }
}