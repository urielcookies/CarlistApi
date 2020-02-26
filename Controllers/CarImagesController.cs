using CarlistApi.data;
using CarlistApi.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace CarlistApi.Controllers
{
    public class CarImagesController : ApiController
    {
        private CarlistDbContext db = new CarlistDbContext();

        [HttpGet]
        [Route("api/carimages/{carid}")]
        public IHttpActionResult GetCarImages(int carid)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var carEntity = db.CarInformation.Any(ci => ci.Id == carid);
            if (!carEntity)
                return BadRequest("Car does not exit");

            var userHasCarPermission = Helper.UserHasCarPermission(carid);
            if (userHasCarPermission == Helper.PermissionType.NONE)
                return BadRequest("User has no access");

            var carId = carid.ToString();

            var account = new CloudStorageAccount(new StorageCredentials("4ever", "6rjyBoPAy19Ou2Co7uM9Sd8MtmUZldeoTomD1mhzeFCsFMvgS+rmY4AlPQzCAh/XF2/yY0OJbfdNWdIp1hbq1w=="), true);
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("cars");
            var blobList = container.ListBlobs(prefix: carId, useFlatBlobListing: true);

            String[] carImages = blobList.Select(element => Convert.ToString(element.Uri)).ToArray();

            return Ok(carImages);
        }

        [HttpPost]
        [Route("api/carimages/{carid}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostCarImagesAsync(int carid)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var carEntity = db.CarInformation.Any(ci => ci.Id == carid);
            if (!carEntity)
                return BadRequest("Car does not exit");

            var userHasCarPermission = Helper.UserHasCarPermission(carid);
            if (userHasCarPermission == Helper.PermissionType.NONE)
                return BadRequest("User has no access");

            if (userHasCarPermission == Helper.PermissionType.READ)
                return BadRequest("User has no permission to post image");

            var httpRequest = HttpContext.Current.Request;

            var areJpeg = true;
            var isFileToLarge = false;
            foreach (string file in httpRequest.Files)
            {
                var postedFile = httpRequest.Files[file];
                if (postedFile.ContentType != "image/jpeg")
                {
                    areJpeg = false;
                };
                if (postedFile.ContentLength > 50000000)
                {
                    isFileToLarge = true;
                }
            }

            if (!areJpeg)
            {
                return BadRequest("Images needs to be jpeg format");
            }

            if (isFileToLarge)
            {
                return BadRequest("Images needs to be smaller than 50MB");
            }

            var account = new CloudStorageAccount(new StorageCredentials("4ever", "6rjyBoPAy19Ou2Co7uM9Sd8MtmUZldeoTomD1mhzeFCsFMvgS+rmY4AlPQzCAh/XF2/yY0OJbfdNWdIp1hbq1w=="), true);
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("cars");

            var carId = carid.ToString();

            // Check last index
            var blobList = container.ListBlobs(prefix: carId, useFlatBlobListing: true);

            string[] carImages = blobList.Select(
                element => Convert.ToString(element.Uri).Replace($"https://4ever.blob.core.windows.net/cars/{carId}/", "")
                ).ToArray();


            var counter = 0;
            foreach (var carImage in carImages)
            {
                counter = int.Parse(carImage.Replace(".jpg", ""));
            }

            if (counter != 0)
            {
                counter++;
            }

            foreach (string file in httpRequest.Files)
            {
                var postedFile = httpRequest.Files[file];
                var imageName = counter.ToString();
                CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference($"{carId}/{imageName}.jpg");
                cloudBlockBlob.Properties.ContentType = postedFile.ContentType;
                await cloudBlockBlob.UploadFromStreamAsync(postedFile.InputStream);
                counter++;
            }

            return Ok(counter);
        }
    }
}
