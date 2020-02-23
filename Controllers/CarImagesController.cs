using CarlistApi.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CarlistApi.Controllers
{
    public class CarImagesController : ApiController
    {
        [HttpGet]
        [Route("api/carimages/{carid}")]
        public IHttpActionResult GetCarImages(int carid)
        {
            if (!Helper.isAuthorizedJWT())
                return BadRequest("Bad token");

            var carId = carid.ToString();

            var account = new CloudStorageAccount(new StorageCredentials("4ever", "6rjyBoPAy19Ou2Co7uM9Sd8MtmUZldeoTomD1mhzeFCsFMvgS+rmY4AlPQzCAh/XF2/yY0OJbfdNWdIp1hbq1w=="), true);
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("cars");
            var blobList = container.ListBlobs(prefix: carId, useFlatBlobListing: true);

            String[] carImages = blobList.Select(element => Convert.ToString(element.Uri)).ToArray();

            return Ok(carImages);
        }
    }
}
