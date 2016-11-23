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

namespace CustomWebApi.Controllers
{
    [Authenticator]
    public class SystemController : ApiController
    {
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
        
        [HttpGet]
        [Route("kenticoapi/system/show-eventlog")]
        public HttpResponseMessage ShowEventlog()
        {
            try
            {
                ObjectQuery<EventLogInfo> events = EventLogProvider.GetEvents().OrderByDescending("EventTime").TopN(50); 
                List<Object> eventList = events.Select( 
                    eventLogInfo => new
                    {
                        EventCode = eventLogInfo.EventCode,
                        EventDescription = eventLogInfo.EventDescription,
                        EventTime = eventLogInfo.EventTime,
                        EventID = eventLogInfo.EventID,
                        EventMachineName = eventLogInfo.EventMachineName,
                        EventType = eventLogInfo.EventType,
                    }).ToList<Object>();
                return Request.CreateResponse(HttpStatusCode.OK, new { eventList = eventList });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }

        [HttpPost]
        [Route("kenticoapi/system/clear-cache")]
        public HttpResponseMessage ClearCache()
        {
            try
            {
                CacheHelper.ClearCache();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }   
        }

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

        [HttpGet]
        [Route("kenticoapi/system/show-general-information")]
        public HttpResponseMessage ShowGeneralInformation()
        {
            string siteName, siteDisplayName, siteDomainName, siteLastModified, licenseExpiration;
            //DateTime siteLastModified, licenseExpiration;

            try
            {
                siteName = SiteContext.CurrentSiteName;
                var site = SiteInfoProvider.GetSiteInfo(siteName);
                siteDisplayName = site.DisplayName;
                siteDomainName = site.DomainName;
                siteLastModified = site.SiteLastModified.ToShortDateString();
                licenseExpiration = LicenseHelper.ApplicationExpires.ToShortDateString();
                
                return Request.CreateResponse(HttpStatusCode.OK, new {
                    siteName = siteName,
                    siteDisplayName = siteDisplayName,
                    siteDomainName = siteDomainName,
                    siteLastModified = siteLastModified,
                    licenseExpiration = licenseExpiration,
                });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = e.Message });
            }
        }


        [HttpGet]
        [Route("kenticoapi/system/testaction/{id}")]
        public HttpResponseMessage TestAction(int id)
        {
            if (true) //was action succesfull?
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { id = id });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "Unable to restart!" });
            }
        }
    }
}