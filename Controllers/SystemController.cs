using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CMS.Helpers;
using CMS.Base;
using CMS.EventLog;
using CMS.DataEngine;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using CustomWebApi.Filters;
using CMS.SiteProvider;
using CMS.LicenseProvider;
using CMS.Synchronization;

namespace CustomWebApi.Controllers
{
    /// <summary>
    /// The controller to manage system related tasks
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    //[Authorizator]
    public class SystemController : ApiController
    {
        /// <summary>
        /// Restarts the server.
        /// </summary>
        /// <returns>Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/system/restart-server")]
        public HttpResponseMessage RestartServer()
        {   
            if (SystemHelper.RestartApplication(SystemContext.WebApplicationPhysicalPath)) //was reboot succesful?
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {path = SystemContext.WebApplicationPhysicalPath });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "Unable to restart:" + SystemContext.WebApplicationPhysicalPath });
            }
        }

        /// <summary>
        /// Shows the eventlog.
        /// </summary>
        /// <returns>
        /// Appropriate HTTP message and if successful event code, description, time, ID, machine name and type
        /// </returns>
        [HttpGet]
        [Route("kenticoapi/system/show-eventlog")]
        public HttpResponseMessage ShowEventlog()
        {
            try
            {   //gets the newest 50 events
                ObjectQuery<EventLogInfo> events = EventLogProvider.GetEvents().OrderByDescending("EventTime").TopN(50); 
                List<Object> eventList = events.Select( 
                    eventLogInfo => new
                    {   
                        //puts the relevant information into a new object to represent the eventlog
                        EventCode = eventLogInfo.EventCode,
                        EventDescription = eventLogInfo.EventDescription,
                        EventTime = eventLogInfo.EventTime,
                        EventID = eventLogInfo.EventID,
                        EventMachineName = eventLogInfo.EventMachineName,
                        EventType = eventLogInfo.EventType,
                    }).ToList<Object>();
                //everything is OK, the event information are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new { eventList = eventList });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <returns>Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/system/clear-cache")]
        public HttpResponseMessage ClearCache()
        {
            try
            {   //clears the cache memory
                CacheHelper.ClearCache();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }   
        }

        /// <summary>
        /// Cleans unused memory.
        /// </summary>
        /// <returns>Appropriate HTTP message</returns>
        [HttpPost]
        [Route("kenticoapi/system/clean-unused-memory")]
        public HttpResponseMessage CleanUnusedMemory()
        {
            try
            {
                // Collect the memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        /// <summary>
        /// Shows general information
        /// </summary>
        /// <returns>
        /// Appropriate HTTP message and if successful general information
        /// </returns>
        [HttpGet]
        [Route("kenticoapi/system/show-general-information")]
        public HttpResponseMessage ShowGeneralInformation()
        {
            string serverName = "", url = "", lastModified = "", lastStart = "";
            SiteInfo site;
            string siteName = "", siteDomainName = "", siteLastModified = "", licenseExpiration = "";
            ServerInfo serverInfo;
            long virtualMemory, workingPeak;

            try
            {
                
                siteName = SiteContext.CurrentSiteName;
                serverName = StagingTaskInfoProvider.ServerName;
                site = SiteContext.CurrentSite;
                if (site != null)
                {
                    siteDomainName = site.DomainName;
                    siteLastModified = site.SiteLastModified.ToShortDateString();
                    serverInfo = ServerInfoProvider.GetServerInfo(serverName, site.SiteID);
                    if(serverInfo != null)  lastModified = serverInfo.ServerLastModified.ToShortDateString();
                }
                
            
                lastStart = CMSApplication.ApplicationStart.ToShortDateString();

                virtualMemory = SystemHelper.GetVirtualMemorySize();
                workingPeak = SystemHelper.GetPeakWorkingSetSize();

                licenseExpiration = LicenseHelper.ApplicationExpires.ToShortDateString();

                //everything is OK, general information are also returned
                return Request.CreateResponse(HttpStatusCode.OK, new {
                    serverName = serverName,
                    serverURL = url,
                    serverLastModified = lastModified,
                    serverLastStart = lastStart,
                    virtualMemory = virtualMemory,
                    workingPeak = workingPeak,
                    siteName = siteName,
                    siteDomainName = siteDomainName,
                    siteLastModified = siteLastModified,
                    licenseExpiration = licenseExpiration,
                });
            }
            catch (NullReferenceException e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { errorMessage = e.Message });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }
    }
}