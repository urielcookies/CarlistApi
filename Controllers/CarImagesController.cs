using CarlistApi.Utils;
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

            return Ok(carid);
        }
    }
}
