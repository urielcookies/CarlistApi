using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using CarlistApi.Models;

namespace CarlistApi.data
{
    public class CarlistDbContext : DbContext
    {
        public DbSet<CarInformation> CarInformation { get; set; }
        public DbSet<CarExpenses> CarExpenses { get; set; }
        public DbSet<CarStatus> CarStatus { get; set; }
        public DbSet<CarAccess> CarAccess { get; set; }
        public DbSet<UserAccounts> UserAccounts { get; set; }
        public DbSet<WebSubscriptions> WebSubscriptions { get; set; }
    }
}