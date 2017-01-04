using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using CMS.MembershipProvider;
using CMS.Membership;
using Newtonsoft.Json.Linq;
using CustomWebApi.Filters;

namespace CustomWebApi.Controllers
{
    [Authorizator]
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
                        })
                        .ToList<Object>();
                return Request.CreateResponse(HttpStatusCode.OK, new { usersList = usersList});
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
            string siteName;
            try
            {
                usernames = postData["usernames"].ToObject<string[]>();
                roleNames = postData["roleNames"].ToObject<string[]>();
                siteName = postData["siteName"].ToObject<string>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }
            string check = CheckIfUsersAndRolesExist(usernames, roleNames, siteName);
            if (check != "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = check });

            }

            UserInfo user;
            RoleInfo role;
            for (int i = 0; i < usernames.Length; i++)
            {
                user = UserInfoProvider.GetUserInfo(usernames[i]);
                for (int j = 0; j < roleNames.Length; j++)
                {
                    bool checkGlobalRoles = true;
                    bool checkMembership = true;

                    // Checks whether the user is assigned to a role with the "Rolename" code name
                    if (user.IsInRole(roleNames[j], siteName, checkGlobalRoles, checkMembership))
                    {
                        // Removes the user from the role
                        try
                        {
                            role = RoleInfoProvider.GetRoleInfo(roleNames[i], siteName, true);
                            UserInfoProvider.RemoveUserFromRole(user.UserID, role.RoleID);
                        } catch (Exception e)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
                        }

                    }
                }               
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { });            
        }

        [HttpPost]
        [Route("kenticoapi/users/edit-user")]
        public HttpResponseMessage EditUser([FromBody]JObject postData)
        {
            string username, firstName, surname;
            try
            {
                username = postData["username"].ToObject<string>();
                firstName = postData["firstName"].ToObject<string>(); 
                surname = postData["surname"].ToObject<string>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
            try
            {
                UserInfo updateUser = UserInfoProvider.GetUserInfo(username);
                if (updateUser != null)
                {
                    // Updates the user's properties
                    updateUser.FirstName = firstName;
                    updateUser.LastName = surname;

                    // Saves the changes
                    UserInfoProvider.SetUserInfo(updateUser);
                    return Request.CreateResponse(HttpStatusCode.OK, new { user = updateUser });

                }
            } catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

            }
            return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "User is null" });

        }

        [HttpPost]
        [Route("kenticoapi/users/add-users-to-roles")]
        public HttpResponseMessage AddUsersToRoles([FromBody]JObject postData)
        {
            string[] usernames;
            int[] roleIds;
            string siteName;
            try
            {
                usernames = postData["usernames"].ToObject<string[]>();
                roleIds = postData["roleIds"].ToObject<int[]>();
                siteName = postData["siteName"].ToObject<string>();
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }

            string checkUsers = CheckIfUsersAndRolesExist(usernames, new string[0], siteName);
            string checkRoles = AreRoleIdsValid(roleIds);
            if (checkUsers != "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = checkUsers });
            }
            if (checkRoles != "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = checkRoles });
            }

            UserInfo user;
            for (int i = 0; i < usernames.Length; i++)
            {
                user = UserInfoProvider.GetUserInfo(usernames[i]);
                for (int j = 0; j < roleIds.Length; j++)
                {
                    try
                    { 
                        UserInfoProvider.AddUserToRole(user.UserID, roleIds[j]);
                    } catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
                    }
                    
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { });
        }

        private string CheckIfUsersAndRolesExist(string[] usernames, string[] roleNames, string siteName)
        {
            UserInfo user;
            //Checks if all usernames are valid
            for (int i = 0; i < usernames.Length; i++)
            {
                user = UserInfoProvider.GetUserInfo(usernames[i]);
                if (user == null)
                {
                    return "invalid username: " + usernames[i];
                }
            }

            RoleInfo role;
            //Checks if all roles are valid
            for (int i = 0; i < roleNames.Length; i++)
            {
                role = RoleInfoProvider.GetRoleInfo(roleNames[i], siteName, true);
                if (role == null)
                {
                   return "invalid roleName: " + roleNames[i];
                }
            }
            return "";
        }

        private string AreRoleIdsValid(int[] roleIds)
        {
            
            for (int i = 0; i < roleIds.Length; i++)
            {
                if(RoleInfoProvider.GetRoleInfo(roleIds[i]) == null)
                {
                    return "invalid roleId: " + roleIds[i];
                }      
            }
            return "";
        }

    }    
}