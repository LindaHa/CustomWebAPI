using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CustomWebApi;

using CMS.Helpers;
using CMS.Base;
using CMS.EventLog;
using CMS.DataEngine;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace CustomWebApi
{
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
            ObjectQuery<EventLogInfo> eventsQuery = EventLogProvider.GetEvents();
            if (eventsQuery != null)
            {
                DataSet results = eventsQuery.OrderByDescending("EventTime").TopN(50).Execute();
                if (results != null)
                {
                    List<Object> eventList = results.Tables[0].AsEnumerable().Select(
                        dataRow => new {
                            EventCode = dataRow.Field<string>("EventCode"),
                            EventDescription = dataRow.Field<string>("EventDescription"),
                            EventTime = dataRow.Field<DateTime>("EventTime"),
                            EventID = dataRow.Field<int>("EventID"),
                            EventMachineName = dataRow.Field<string>("EventMachineName"),
                            EventType = dataRow.Field<string>("EventType"),
                        }).ToList<Object>();
                    return Request.CreateResponse(HttpStatusCode.OK, new { eventList = eventList });
                }
            }
            return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new { errorMessage = "Unable to retrieve the eventlog." });
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