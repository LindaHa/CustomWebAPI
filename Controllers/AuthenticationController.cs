using CMS.Membership;
using CMS.SiteProvider;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CustomWebAPI.Controllers
{
    class AuthenticationController : ApiController
    {
        private static bool boolTrue = true;
        [HttpGet]
        [Route("kenticoapi/authentication/authenticate-user")]
        public HttpResponseMessage AuthenticateUser([FromBody]JObject postData)
        {
            UserInfo user = null;
            string username, password;

            try
            {
                username = postData["username"].ToObject<string>();
                password = postData["password"].ToObject<string>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }
            try
            {
                // Attempts to log into the current site using a username and password
                user = AuthenticationHelper.AuthenticateUser(username, password, SiteContext.CurrentSiteName);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

            }
            if (user != null)
            {
                // Authentication was successful
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = "There is a problem with your username or password" });
        }

        [HttpGet]
        [Route("kenticoapi/authentication/get-current-user")]
        public HttpResponseMessage GetCurrentUser()
        {
            UserInfo user = null;
            try
            {
                // Attempts to log into the current site using a username and password
                user = AuthenticationHelper.GetCurrentUser(out boolTrue);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

            }
            if (user != null)
            {
                // Authentication was successful
                return Request.CreateResponse(HttpStatusCode.OK, new { User = user });
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = "There is a problem with the current user." });
        }
    }
}
