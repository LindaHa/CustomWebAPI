using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CustomWebApi;

using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using CMS.MembershipProvider;
using CMS.Membership;
using Newtonsoft.Json.Linq;

namespace CustomWebApi
{
    public class UsersController : ApiController
    {
        [HttpGet]
        [Route("kenticoapi/users")]
        public HttpResponseMessage GetAllUsers()
        {
            CMSRoleProvider cmsRoleProvider = new CMSRoleProvider();
            try
            {
                DataSet users = UserInfoProvider.GetFullUsers("", "UserID ASC");
                List<Object> usersList = users.Tables[0].AsEnumerable().Select(
                        dataRow => new
                        {
                            UserId = dataRow.Field<int>("userid"),
                            FirstName = dataRow.Field<string>("firstname"),
                            Surname = dataRow.Field<string>("lastname"),
                            Email = dataRow.Field<string>("email"),
                            Username = dataRow.Field<string>("username"),
                            //UsrName = dataRow.Field<string>("nickname"),
                            Roles = cmsRoleProvider.GetRolesForUser(dataRow.Field<string>("username")),
                        }).ToList<Object>();
                return Request.CreateResponse(HttpStatusCode.OK, new { usersList = usersList });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        [HttpPost]
        [Route("kenticoapi/users/remove-users-from-roles")]
        public HttpResponseMessage RemoveUsersFromRoles([FromBody]JObject postData)
        {
            string[] usernames, roleNames;
            try
            {
                usernames = postData["usernames"].ToObject<string[]>();
                roleNames = postData["roleNames"].ToObject<string[]>();
            }catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }
            CMSRoleProvider cmsRoleProvider = new CMSRoleProvider();
            try
            {
                cmsRoleProvider.RemoveUsersFromRoles(usernames, roleNames);
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }
    }
}