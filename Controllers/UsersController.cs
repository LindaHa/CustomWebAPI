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
    /// <summary>
    /// The controller to manage users
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    [Authorizator]
    public class UsersController : ApiController
    {
        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns> Appropriate HTTP message and if successful returns all users</returns>
        [HttpGet]
        [Route("kenticoapi/users")]
        public HttpResponseMessage GetAllUsers()
        {
            CMSRoleProvider cmsRoleProvider = new CMSRoleProvider();
            try
            {  
                //gets all users ordered depending on their IDs ascending
                DataSet users = UserInfoProvider.GetFullUsers("", "UserID ASC");                
                List<Object> usersList = users.Tables[0].AsEnumerable().Select(
                        dataRow => new
                        {   //puts the relevant information into a new object to represent the user
                            UserId = dataRow.Field<int>("userid"),
                            FirstName = dataRow.Field<string>("firstname"),
                            Surname = dataRow.Field<string>("lastname"),
                            Email = dataRow.Field<string>("email"),
                            Username = dataRow.Field<string>("username"),
                            //UsrName = dataRow.Field<string>("nickname"),
                            Roles = cmsRoleProvider.GetRolesForUser(dataRow.Field<string>("username")),
                        })
                        .ToList<Object>();
                //everything is OK, the users are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { usersList = usersList});
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Removes the users from roles.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the usernames, rolenames and the name of the current site.
        /// </param>
        /// <returns> Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/users/remove-users-from-roles")]
        public HttpResponseMessage RemoveUsersFromRoles([FromBody]JObject postData)
        {
            string[] usernames, roleNames;
            string siteName;
            //parsing postdata
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
            //checks if the given usernames and role names are valid on the given site
            string check = CheckIfUsersAndRolesExist(usernames, roleNames, siteName);
            if (check != "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = check });
            }

            UserInfo user;
            RoleInfo role;
            //for all usernames
            for (int i = 0; i < usernames.Length; i++)
            {   //gets the user according to the username
                user = UserInfoProvider.GetUserInfo(usernames[i]);
                //and for all role names
                for (int j = 0; j < roleNames.Length; j++)
                {   
                    //all global and membership roles will be checked
                    bool checkGlobalRoles = true;
                    bool checkMembership = true;

                    // Checks whether the user is assigned to a role with the role name 
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

        /// <summary>
        /// Edits the user.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the username, the first and last name of the user.
        /// </param>
        /// <returns> Appropriate HTTP message and if successful the updated user</returns>
        [HttpPost]
        [Route("kenticoapi/users/edit-user")]
        public HttpResponseMessage EditUser([FromBody]JObject postData)
        {
            string username, firstName, surname;
            //parsing postdata
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
                //gets the user by username
                UserInfo updateUser = UserInfoProvider.GetUserInfo(username);
                if (updateUser != null)
                {
                    // Updates the user's properties
                    updateUser.FirstName = firstName;
                    updateUser.LastName = surname;

                    // Saves the changes
                    UserInfoProvider.SetUserInfo(updateUser);
                    //everything is OK, the updated user is also returned
                    return Request.CreateResponse(HttpStatusCode.OK, new { user = updateUser });

                }
            } catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });

            }
            return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "User is null" });

        }

        /// <summary>
        /// Adds the given users to the given roles.
        /// </summary>
        /// <param name="postData">
        /// The post data contain the usernames, role IDs and the name of the current site.
        /// </param>
        /// <returns> Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/users/add-users-to-roles")]
        public HttpResponseMessage AddUsersToRoles([FromBody]JObject postData)
        {
            string[] usernames;
            int[] roleIds;
            string siteName;
            //parsing postdata
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

            //checks if usernames on the site name are valid
            string checkUsers = CheckIfUsersAndRolesExist(usernames, new string[0], siteName);
            //checks if role are valid
            string checkRoles = AreRoleIdsValid(roleIds);
            //the errorMessage contains which username or rolename is invalid
            if (checkUsers != "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = checkUsers });
            }
            if (checkRoles != "")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = checkRoles });
            }

            UserInfo user;
            //for each username
            for (int i = 0; i < usernames.Length; i++)
            {
                //gets the user by ID
                user = UserInfoProvider.GetUserInfo(usernames[i]);
                //for every role ID
                for (int j = 0; j < roleIds.Length; j++)
                {
                    //assign user to role
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