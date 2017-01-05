using CMS.Membership;
using CMS.SiteProvider;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CustomWebAPI.DAL;
using CustomWebApi.Filters;

namespace CustomWebApi.Controllers
{
    /// <summary>
    /// The controller checks if a user is authorized to fulfill a request
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class AuthenticationController : ApiController
    {
        /// <summary>
        /// Authenticates the current user.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the username and password
        /// </param>
        /// <returns>Appropriate HTTP message and if successful the access token</returns>
        [HttpPost]
        [Route("kenticoapi/authentication/authenticate-user")]
        public HttpResponseMessage AuthenticateUser([FromBody]JObject postData)
        {
            UserInfo userInfo = null;
            string username, password;

            //parsing of the postdata
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
                userInfo= AuthenticationHelper.AuthenticateUser(username, password, SiteContext.CurrentSiteName);             
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });

            }
            if (userInfo != null)
            {
                // If user is not a glbal Admin return Forbidden
                if(!userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { errorMessage = "You need to be a GLOBAL ADMINISTRATOR to enter the system" });
                }
                // Authentication was successful
                Token token = CreateToken(userInfo.UserID);
                //everything is OK, the token is also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { token = token });
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, new { errorMessage = "There is a problem with your username or password" });
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>Appropriate HTTP message and if successful the current user</returns>
        [Authorizator]
        [HttpGet]
        [Route("kenticoapi/authentication/get-current-user")]
        public HttpResponseMessage GetCurrentUser()
        {
            UserInfo user = null;
            try
            {
                // Attempts to log into the current site using a username and password
                user = (UserInfo) Request.Properties["LoggedUserInfo"];
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { errorMessage = e.Message });

            }
            if (user != null)
            {
                // Authentication was successful, the user is also returned
                return Request.CreateResponse(HttpStatusCode.OK, new {
                    UserID = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName });
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = "There is a problem with the current user." });
        }

        /// <summary>
        /// Signs the user out.
        /// </summary>
        /// <returns>Appropriate HTTP message</returns>
        [Authorizator]
        [HttpPost]
        [Route("kenticoapi/authentication/sign-out-user")]
        public HttpResponseMessage SignOutUser()
        {
            UserInfo user = null;
            try
            {
                //gets the currently logged in user
                user = (UserInfo)Request.Properties["LoggedUserInfo"];
                var request = Request;
                var headers = request.Headers;

                //checks if the header has the users valid access token
                if (headers.Contains("AccessToken"))
                {
                    string tokenCode = headers.GetValues("AccessToken").FirstOrDefault();
                    if (tokenCode != null)
                    {
                        //the token is present, the user is signed out
                        AuthenticationHelper.SignOut();
                        //the token is being removed from the database
                        using (var context = new DBContext())
                        {
                            context.Token.RemoveRange(context.Token.Where(tok => tok.Code == tokenCode));
                            context.SaveChanges();
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

            }
        }

        private Token CreateToken(int userID, int length = 32)
        {
            Token token;
            //the pseudo-random code is generated
            string code = CreateRandomCode(length);

            if (length < 0) length = 32;

            using (var context = new DBContext())
            {
                //the code is tested against the DB if it doesn't exist already
                while (GetTokenByCode(code) != null)
                {
                    code = CreateRandomCode(length);
                }
                token = new Token {
                    UserID = userID,
                    Code = code,
                    Expiration = DateTime.Now.AddMinutes(10)
                };
                context.Token.Add(token);
                context.SaveChanges();
            }
            return token;
        }

        private string CreateRandomCode(int length = 32)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        private Token GetTokenByCode(string code)
        {
            using (var context = new DBContext())
            {
                return context.Token.Where(token => token.Code == code).FirstOrDefault();
            }
        }
    }
}
