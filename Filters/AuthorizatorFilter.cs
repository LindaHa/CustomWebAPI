using CMS.Membership;
using CustomWebAPI.DAL;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CustomWebApi.Filters
{
    public class Authorizator : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;
            Token token = null;
            UserInfo userInfo = null;

            if (headers.Contains("AccessToken"))
            {
                string tokenCode = headers.GetValues("AccessToken").FirstOrDefault();
                if (tokenCode != null)
                {
                    using (var context = new DBContext())
                    {
                        context.Token.RemoveRange(context.Token.Where(tok => tok.Expiration <= DateTime.Now));
                        token = context.Token.Where(tok => tok.Code == tokenCode).FirstOrDefault();
                        if (token != null)
                        {
                            token.Expiration = DateTime.Now.AddMinutes(10);
                        }
                        context.SaveChanges();
                    }
                }
            }
            if (token != null)
            {
                try
                {
                    userInfo = UserInfoProvider.GetUserInfo(token.UserID);
                    actionContext.Request.Properties.Add("LoggedUserInfo", userInfo);
                }
                catch (Exception) { }
            }
            if (userInfo == null || !userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }
}
