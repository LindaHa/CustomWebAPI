using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CustomWebApi;

using CMS.Helpers;
using CMS.Base;
    
namespace CustomWebApi
{
    public class SystemController : ApiController
    {
        [HttpPut]
        [Route("kenticoapi/system/restart-server")]
        public HttpResponseMessage RestartServer()
        {

            if (SystemHelper.RestartApplication(SystemContext.WebApplicationPhysicalPath)) //was reboot succesful?
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {path = SystemContext.WebApplicationPhysicalPath });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { error_message = "Unable to restart:" + SystemContext.WebApplicationPhysicalPath });
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