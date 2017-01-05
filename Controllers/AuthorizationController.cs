using CMS.DataEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CustomWebApi.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CustomWebApi.Controllers
{
    /// <summary>
    /// The controller to manage roles and permissions
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    [Authorizator]
    public class AuthorizationController : ApiController
    {
        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <returns>
        /// Appropriate HTTP message and if successful all Roles
        /// </returns>    
        [HttpGet]
        [Route("kenticoapi/authorization/get-roles")]
        public HttpResponseMessage GetRoles()
        {
            ObjectQuery<RoleInfo> roles;
            try
            {
                //the roles are got
                roles = RoleInfoProvider.GetRoles().OrderByDescending("RoleDisplayName");
                //puts the relevant information into a new object to represent the role
                List<Object> roleList = roles.Select(
                    roleInfo => new
                    {
                        RoleId = roleInfo.RoleID,
                        RoleName = roleInfo.RoleName,
                        RoleDisplayName = roleInfo.DisplayName
                    }).OrderBy(role => role.RoleDisplayName)
                    .ToList<Object>();
                //everything is OK, the roles are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { roleList = roleList });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Gets the role permissions by role ID.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns>
        /// Appropriate HTTP message and if successful all permissions of the given role
        /// </returns>
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
                //the relevant permissions are retrieved 
                List<Object> permissions = PermissionNameInfoProvider.GetPermissionNames()
                    .WhereIn("PermissionID", RolePermissionInfoProvider
                        .GetRolePermissions()
                        .Column("PermissionID")
                        .WhereEquals("RoleID", roleId))
                        .Select(
                            row => new
                            { //puts the relevant information into a new object to represent the permission
                                PermissionId = row.PermissionId,
                                PermissionName = row.PermissionName,
                                PermissionDisplayName = row.PermissionDisplayName,
                                PermissionDescription = row.PermissionDescription
                            }
                        )
                        .OrderBy(role => role.PermissionDisplayName)
                        .ToList<Object>();
                //everything is OK, the permissions are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { permissionList = permissions });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Deletes the role by ID.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns>Appropriate HTTP message</returns>
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });

                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "There's a problem with your role." });
        }

        /// <summary>
        /// Creates the new role.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the name and display name of the new role
        /// </param>
        /// <returns>
        /// Appropriate HTTP message and if successful the ID of the new role
        /// </returns>
        [HttpPost]
        [Route("kenticoapi/authorization/create-new-role")]
        public HttpResponseMessage CreateNewRole([FromBody]JObject postData)
        {
            // Creates a new role object
            RoleInfo newRole = new RoleInfo();
            string newRoleName, newDisplayName;

            //parsing of the postdata
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
            int roleId;
            // Verifies that the role is unique for the current site
            if (!RoleInfoProvider.RoleExists(newRole.RoleName, SiteContext.CurrentSiteName))
            {
                try
                {
                    // Saves the role to the database
                    RoleInfoProvider.SetRoleInfo(newRole);
                    roleId = RoleInfoProvider.GetRoleInfo(newRole.RoleName, SiteContext.CurrentSiteName).RoleID;
                }
                catch (CodeNameNotValidException e)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
                }
                //everything is OK, the ID of the new role are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { newRoleId = roleId });
            }
            else
            {
                // A role with the same name already exists on the site
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "A role with the same name already exists on the site" });
            }
        }

        /// <summary>
        /// Gets all permissions.
        /// </summary>
        /// <returns>
        /// Appropriate HTTP message and if successful all perissions
        /// </returns>
        [HttpGet]
        [Route("kenticoapi/authorization/get-all-permissions")]
        public HttpResponseMessage GetAllPermissions()
        {
            try
            {
                List<Object> permissions = PermissionNameInfoProvider.GetPermissionNames()
                    .Select( //the relevant permission information are retrieved into a new object
                            row => new
                            {
                                PermissionId = row.PermissionId,
                                PermissionName = row.PermissionName,
                                PermissionDisplayName = row.PermissionDisplayName,
                                PermissionDescription = row.PermissionDescription
                            }
                        )
                        .OrderBy(role => role.PermissionDisplayName)
                        .ToList<Object>();
                //everything is OK, the permissions are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { permissionList = permissions });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns>
        /// Appropriate HTTP message and if successful the role of the given ID
        /// </returns>
        [HttpGet]
        [Route("kenticoapi/authorization/get-role/{roleId}")]
        public HttpResponseMessage GetRole(int roleId)
        {
            try
            {
                List<Object> roles = RoleInfoProvider.GetRoles()
                .WhereEquals("RoleID", roleId)
                .Select(
                    row => new
                    { //puts the relevant information into a new object representing the role
                        RoleId = row.RoleID,
                        RoleName = row.RoleName,
                        RoleDisplayName = row.DisplayName
                    }
                ).ToList<Object>();
                if (roles.First() != null)
                {
                    //everything is OK, the role is also returned
                    return Request.CreateResponse(HttpStatusCode.OK, new { role = roles.First() });
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No role with the given roleId exists.");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Assigns the given permissions to the given roles.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the IDs of the roles and permissions.
        /// </param>       
        /// <returns>Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/authorization/assign-permissions-to-roles")]
        public HttpResponseMessage AssignPermissionsToRoles([FromBody]JObject postData)
        {
            int[] roleIds;
            int[] permissionIds;
            //the parsing of the postdata
            try
            {
                roleIds = postData["roleIds"].ToObject<int[]>();
                permissionIds = postData["permissionIds"].ToObject<int[]>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }
            
            if (roleIds != null && permissionIds != null)
            {
                RolePermissionInfo newRolePermission;

                //for every given role
                foreach (int roleId in roleIds)
                { //and every given permission
                    foreach (int permissionId in permissionIds)
                    {
                        newRolePermission = new RolePermissionInfo();
                        try
                        {   //join the role with the permission
                            newRolePermission.RoleID = roleId;
                            newRolePermission.PermissionID = permissionId;
                            RolePermissionInfoProvider.SetRolePermissionInfo(newRolePermission);
                            newRolePermission = null;

                        }
                        catch(Exception e)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
                        }
                    }
                }            
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "No roleIDs or no permissionIDs provided" });


        }

        /// <summary>
        /// Unassigns the given permissions from the given roles.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the IDs of the role and permissions.
        /// </param>
        /// <returns>Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/authorization/unassign-permissions-from-roles")]
        public HttpResponseMessage UnassignPermissionsFromRoles([FromBody]JObject postData)
        {
            int[] roleIds;
            int[] permissionIds;
            //parsing of the postdata
            try
            {
                roleIds = postData["roleIds"].ToObject<int[]>();
                permissionIds = postData["permissionIds"].ToObject<int[]>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }

            if (roleIds != null && permissionIds != null)
            { //for every given role
                foreach (int roleId in roleIds)
                { //and for every given permission
                    foreach (var permissionId in permissionIds)
                    {
                        try
                        {                            
                            // Gets the object representing the role-permission relationship
                            RolePermissionInfo deleteRolePermission = RolePermissionInfoProvider.GetRolePermissionInfo(roleId, permissionId);

                            if (deleteRolePermission != null)
                            {
                                // Removes the permission from the role
                                RolePermissionInfoProvider.DeleteRolePermissionInfo(deleteRolePermission);
                            }
                        }
                        catch (Exception e)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = "No roleIDs or no permissionIDs provided" });
        }
    }
}
