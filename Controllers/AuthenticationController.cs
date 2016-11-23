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
    public class AuthenticationController : ApiController
    {
        [HttpPost]
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
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });

            }
            if (user != null)
            {

                // Authentication was successful
                Token token = CreateToken(user.UserID);
                return Request.CreateResponse(HttpStatusCode.OK, new { token = token });
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, new { errorMessage = "There is a problem with your username or password" });
        }

        [Authenticator]
        [HttpGet]
        [Route("kenticoapi/authentication/get-current-user")]
        public HttpResponseMessage GetCurrentUser()
        {
            UserInfo user = null;
            try
            {
                //bool boolTrue = true;
                // Attempts to log into the current site using a username and password
                //user = AuthenticationHelper.GetCurrentUser(out boolTrue);
                user = (UserInfo) Request.Properties["LoggedUserInfo"];
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { errorMessage = e.Message });

            }
            if (user != null)
            {
                // Authentication was successful
                return Request.CreateResponse(HttpStatusCode.OK, new {
                    UserID = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName });
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, new { errorMessage = "There is a problem with the current user." });
        }

        [Authenticator]
        [HttpPost]
        [Route("kenticoapi/authentication/sign-out-user")]
        public HttpResponseMessage SignOutUser()
        {
            UserInfo user = null;
            try
            {
                user = (UserInfo)Request.Properties["LoggedUserInfo"];
                var request = Request;
                var headers = request.Headers;

                if (headers.Contains("AccessToken"))
                {
                    string tokenCode = headers.GetValues("AccessToken").FirstOrDefault();
                    if (tokenCode != null)
                    {
                        AuthenticationHelper.SignOut();
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
            string code = CreateRandomCode(length);

            using (var context = new DBContext())
            {
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
