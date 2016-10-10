using CMS.DataEngine;
using CMS.Membership;
using CMS.Modules;
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

namespace CustomWebApi
{
    public class AuthorizationController : ApiController
    {
        [HttpGet]
        [Route("kenticoapi/authorization/get-permissions/{roleId}")]
        public HttpResponseMessage GetRolePermissions(int roleId = 0)
        {

            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "Invalid roleId" });     
           
            try
            {
                List<Object> permissions = PermissionNameInfoProvider.GetPermissionNames()
                    .WhereIn("PermissionID", RolePermissionInfoProvider
                        .GetRolePermissions()
                        .Column("PermissionID")
                        .WhereEquals("RoleID", roleId))
                        .Select(
                            row => new
                            {
                                PermissionId = row.PermissionId,
                                PermissionName = row.PermissionName,
                                PermissionDisplayName = row.PermissionDisplayName,
                                PermissionDescription = row.PermissionDescription
                            }
                        ).ToList<Object>();

                return Request.CreateResponse(HttpStatusCode.OK, new { permissionList = permissions });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }
    }
}
