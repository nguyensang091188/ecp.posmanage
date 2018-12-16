using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ePOS3.Entities.RequestObject
{
   
    public class ObjResponse
    {
        public string code { get; set; }
        public string msg { get; set; }
        public ObjAPI ObjAPI { get; set; }

        public ObjPageAPIResponse objPageAPI { get; set; }

        public List<ObjAPI> objAPIs { get; set; }

        public List<ObjAgent> objAgents { get; set; }

        public List<ObjRole> objRoles { get; set; }
    }

    public class Response
    {
        public string responseCode { get; set; }
        public string description { get; set; }
    }


    public class ObjAPI
    {
        public int id { get; set; }
        public string url { get; set; }
        public int status { get; set; }
        public string method { get; set; }
        public string name { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class ObjPageAPIResponse
    {
        public bool last { get; set; }
        public int totalPages { get; set; }
        public int totalElements { get; set; }
        public string sort { get; set; }
        public bool first { get; set; }
        public int numberOfElements { get; set; }
        public int size { get; set; }
        public int number { get; set; }
        public List<ObjAPI> content { get; set; }
    }

    public class ObjAgent
    {
        public int id { get; set; }
        public string code { get; set; }
        public string password { get; set; }
        public string publicKey { get; set; }
        public string privateKey { get; set;}
        public string salt { get; set; }
        public string desc { get; set; }
        public string publicKeyOwn { get; set; }
        public int status { get; set; }        
        public string publicKeyCrypt { get; set; }
        public string privateKeyCrypt { get; set; }
    }

    public class ObjRole
    {
        public int id { get; set; }//role
        public int agentId { get; set; }
        public int apiId { get; set; }
        public int status { get; set; }
        public string createdAt { get; set; }
   
    }

    public class SchedulerResponse
    {
        public string code { get; set; }
        public string msg { get; set; }
        public ObjPageLogSearchResponse objPageLogSearch { get; set; }
    }

    public class ObjLogSearchModel
    {
        public string application { get; set; }
        public List<SelectListItem> ApplicationList { get; set; }
        public string fromDate { get; set; }
        public long logId { get; set; }
        public long maxDuration { get; set; }
        public string method { get; set; }
        public long minDuration { get; set; }
        public string request { get; set; }
        public string response { get; set; }
        public string status { get; set; }
        public string toDate { get; set; }
        public string type { get; set; }
        public List<SelectListItem> TypeList { get; set; }
    }
    public class ObjLogSearchRequest
    {
        public string application { get; set; }
        public string type { get; set; }
        public string logId { get; set; }
        public string method { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
    }
    public class ObjLogSearch
    {
        public long logId { get; set; }
        public string application { get; set; }
        public string method { get; set; }
        public string request { get; set; }
        public string response { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public long duration { get; set; }
        public string status { get; set; }
        public string logPath { get; set; }
    }

    public class ObjPageLogSearchResponse
    {
        public bool last { get; set; }
        public int totalPages { get; set; }
        public int totalElements { get; set; }
        public string sort { get; set; }
        public bool first { get; set; }
        public int numberOfElements { get; set; }
        public int size { get; set; }
        public int number { get; set; }
        public List<ObjLogSearch> content { get; set; }
    }
}