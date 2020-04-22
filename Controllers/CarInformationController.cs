using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using AuthenticationService.Managers;
using AuthenticationService.Models;
using CarlistApi.data;
using CarlistApi.Models;
using CarlistApi.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CarlistApi.Controllers
{
    public class CarInformationController : ApiController
    {
        CarlistDbContext carlistDbContext = new CarlistDbContext();

        // check token and check for email match in database to allow access
        // GET: api/CarInformation/
        public IHttpActionResult Get()
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var currentUser = Helper.currentUser();
            var usersCars = carlistDbContext.CarInformation
                .Where(m => currentUser.Id == m.UserAccountId);
            
            return Ok(usersCars);
        }

        // GET: api/CarInformation/
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/carinformation/access-other-cars")]
        public IHttpActionResult GetAccessCars()
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var currentUser = Helper.currentUser();
            var queryCarIds = carlistDbContext.CarAccess
                .Where(s => s.UserAccountId == currentUser.Id)
                .Select(x => x.CarInformationId); ;

            var permittedCars = carlistDbContext.CarInformation
                    .Where(m => queryCarIds.Contains(m.Id));

            return Ok(permittedCars);

        }

        // GET: api/CarAccess
        [HttpGet]
        [Route("api/carinformation/get-users-cars/{userId}")]
        [ResponseType(typeof(CarInformation))]
        public IHttpActionResult GetUsersCars(int userId)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var currentUser = Helper.currentUser();

            var queryCarIds = carlistDbContext.CarInformation
                .Where(s => s.UserAccountId == userId)
                .Select(x => x.Id);

            var permittedCarsIds = carlistDbContext.CarAccess
                .Where(m => m.UserAccountId == currentUser.Id && queryCarIds.Contains(m.CarInformationId))
                .Select(x => x.CarInformationId);

            var permittedCars = carlistDbContext.CarInformation
                .Where(s => permittedCarsIds.Contains(s.Id));

            return Ok(permittedCars);
        }

        // GET: api/CarAccess
        [HttpGet]
        [Route("api/carinformation/get-other-carinfo/{carId}")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult GetOtherCarInfo(int carId)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");
        
            var currentUser = Helper.currentUser();

            var queryCarIds = carlistDbContext.CarAccess
                .Where(s => s.UserAccountId == currentUser.Id)
                .Select(x => x.CarInformationId);

            var carInfo = carlistDbContext.CarInformation
                .FirstOrDefault(m => queryCarIds.Contains(m.Id) && m.Id == carId);

            return Ok(carInfo);
        }

        // GET: api/CarAccess
        [HttpGet]
        [Route("api/carinformation/get-carinfo/{carId}")]
        [ResponseType(typeof(CarExpenses))]
        public IHttpActionResult GetCarInfo(int carId)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var carInfo = carlistDbContext.CarInformation.FirstOrDefault(m => m.Id == carId);

            return Ok(carInfo);
        }

        // GET: api/CarInformation/5
        public IHttpActionResult Get(int id)
        {
            var car = carlistDbContext.CarInformation.Find(id);
            if (car == null)
            {
                return BadRequest("No record found on this id...");
            }
            return Ok(car);
        }

        // GET: api/CarInformation/test
        [HttpGet]
        [Route("api/carinformation/test/{id}")]
        public IHttpActionResult Test(int id)
        {
            return Ok(id);
        }

        // POST: api/CarInformation
        public IHttpActionResult Post([FromBody]CarInformation carInfo)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            // Check for validation on db fields
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUser = Helper.currentUser();

            carInfo.UserAccountId = currentUser.Id;
            carInfo.CreatedTime = DateTime.UtcNow;

            carlistDbContext.CarInformation.Add(carInfo);
            carlistDbContext.SaveChanges();

            var newCarStatus = new CarStatus();
            newCarStatus.UserAccountId = currentUser.Id;
            newCarStatus.CarInformationId = carInfo.Id;
            newCarStatus.Sold = false;
            newCarStatus.CreatedTime = DateTime.UtcNow;

            carlistDbContext.CarStatus.Add(newCarStatus);
            carlistDbContext.SaveChanges();

            return Ok(carInfo.Id);
        }

        // PUT: api/CarInformation/5
        public IHttpActionResult Put(int id, [FromBody]CarInformation carInfo)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var entity = carlistDbContext.CarInformation.FirstOrDefault(ci => ci.Id == id);

            if (entity == null)
            {
                return BadRequest("No record found on this id...");
            }

            // Check for validation on db fields
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = Helper.currentUser();

            // is owner
            var hasAccess = carlistDbContext.CarInformation
                .Any(c => c.Id == entity.Id && c.UserAccountId == currentUser.Id);

            // has permissions
            if (!hasAccess)
            {
                hasAccess = carlistDbContext.CarAccess
                    .Any(c => c.CarInformationId == entity.Id && c.UserAccountId == currentUser.Id && c.Write == true);
            }

            if (!hasAccess)
                return BadRequest("No permission to edit");

            entity.Year = carInfo.Year;
            entity.Brand = carInfo.Brand;
            entity.Model = carInfo.Model;
            entity.Cost = carInfo.Cost;
            entity.CleanTitle = carInfo.CleanTitle;
            entity.Notes = carInfo.Notes;

            carlistDbContext.SaveChanges();
            return Ok(HttpStatusCode.NoContent);
        }

        // DELETE: api/CarInformation/5
        public IHttpActionResult Delete(int id)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var userHasCarPermission = Helper.UserHasCarPermission(id);
            if (userHasCarPermission != Helper.PermissionType.OWNER)
                return BadRequest("User needs to be owner to give car access");

            var carInfo = carlistDbContext.CarInformation.Find(id);
            if (carInfo == null)
                return BadRequest("Record does not exist");

            // Delete All Car Expenses
            IQueryable allCarExpenses = carlistDbContext.CarExpenses
                .Where(s => s.CarInformationId == id);
            foreach (CarExpenses carExpense in allCarExpenses)
            {
                carlistDbContext.CarExpenses.Remove(carExpense);
            }

            // Delete All Car Access
            IQueryable allCarAccess = carlistDbContext.CarAccess
                .Where(s => s.CarInformationId == id);
            foreach (CarAccess carAccess in allCarAccess)
            {
                carlistDbContext.CarAccess.Remove(carAccess);
            }

            // Delete Car Status
            var carStatus = carlistDbContext.CarStatus
                .FirstOrDefault(cs => cs.CarInformationId == id);
            carlistDbContext.CarStatus.Remove(carStatus);

            // Delete All Images
            var account = new CloudStorageAccount(new StorageCredentials("4ever", "6rjyBoPAy19Ou2Co7uM9Sd8MtmUZldeoTomD1mhzeFCsFMvgS+rmY4AlPQzCAh/XF2/yY0OJbfdNWdIp1hbq1w=="), true);
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("cars");

            var carId = id.ToString();
            var blobList = container
                .ListBlobs(useFlatBlobListing: true)
                .OfType<ICloudBlob>()
                .Where(b => b.Name.StartsWith($"{carId}/"));

            string[] carImages = blobList.Select(
                element => Convert.ToString(element.Uri).Replace($"https://4ever.blob.core.windows.net/cars/", "")
                ).ToArray();

            foreach (var carImage in carImages)
            {
                container.GetBlockBlobReference(carImage).DeleteIfExists();
            }

            carlistDbContext.CarInformation.Remove(carInfo);
            carlistDbContext.SaveChanges();
            return Ok(HttpStatusCode.OK);
        }
    }
}
