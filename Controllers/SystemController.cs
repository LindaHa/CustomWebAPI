using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CustomWebApi;

// All of the rest is WebAPI thing, so no Kentico stuff in here
namespace CustomWebApi
{
    public class SystemController : ApiController
    {
        [HttpGet]
        [Route("kenticoapi/system/restart-server")]
        public HttpResponseMessage RestartServer()
        {

            if (true) //was reboot succesful?
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { error_message = "Unable to restart!" });
            }
        }

        [HttpGet]
        [Route("kenticoapi/system/testaction/{id}")]
        public HttpResponseMessage TestAction(int id)
        {
            if (true) //was action succesfull?
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { id = id });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { error_message = "Unable to restart!" });
            }
        }
    }
}