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
using CMS.Membership;

namespace CustomWebApi
{
    public class UsersController : ApiController
    {
        [HttpGet]
        [Route("kenticoapi/users")]
        public HttpResponseMessage GetAllUsers()
        {
            try{
                DataSet users = UserInfoProvider.GetFullUsers("", "UserID ASC");
                List<Object> eventList = users.Tables[0].AsEnumerable().Select(
                        dataRow => new {
                            UserId = dataRow.Field<int>("userid"),
                            FirstName = dataRow.Field<string>("firstname"),
                            Surname = dataRow.Field<string>("lastname"),
                            Email = dataRow.Field<string>("email"),
                        }).ToList<Object>();
                return Request.CreateResponse(HttpStatusCode.OK, new { eventList = eventList });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }
    }
}