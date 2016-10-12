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
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "Invalid roleId" });
            }
           
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

        [HttpPost]
        [Route("kenticoapi/authorization/delete-role/{roleId}")]
        public HttpResponseMessage DeleteRole(int roleId = 0)
        {
            if (roleId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "Invalid roleId" });
            }

            RoleInfo deleteRole = new RoleInfo();
            try
            {
                // Gets the role
                deleteRole = RoleInfoProvider.GetRoleInfo(roleId);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

            }

            if (deleteRole != null)
            {
                try
                {
                    // Deletes the role
                    RoleInfoProvider.DeleteRoleInfo(deleteRole);
                    return Request.CreateResponse(HttpStatusCode.OK, new { });

                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

                }
            }
            return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "There's a problem with your role." });
        }

        [HttpPost]
        [Route("kenticoapi/authorization/create-new-role/{newRoleName}/{newDisplayName}")]
        public HttpResponseMessage CreateNewRole([FromBody]JObject postData)
        {
            // Creates a new role object
            RoleInfo newRole = new RoleInfo();
            string newRoleName, newDisplayName;

            try
            {
                newRoleName = postData["roleName"].ToObject<string>();
                newDisplayName = postData["roleDisplayName"].ToObject<string>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }

            // Sets the role properties
            newRole.RoleName = newRoleName;
            newRole.DisplayName = newDisplayName;
            newRole.SiteID = SiteContext.CurrentSiteID;

            // Verifies that the role is unique for the current site
            if (!RoleInfoProvider.RoleExists(newRole.RoleName, SiteContext.CurrentSiteName))
            {
                try
                {
                    // Saves the role to the database
                    RoleInfoProvider.SetRoleInfo(newRole);
                } catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { newRole = newRole });
            }
            else
            {
                // A role with the same name already exists on the site
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "A role with the same name already exists on the site" });
            }
        }
    }
}
