using ePOS3.Entities.RequestObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ePOS3.Utils
{
    public class ePosRestFulDAO
    {
        private static string WEB_MANAGER = Convert.ToString(ConfigurationManager.AppSettings["WEB_MANAGER"]);
        private static string WEB_PASS = Convert.ToString(ConfigurationManager.AppSettings["WEB_PASS"]);
        private static int WEB_RSA_KEYSIZE = Convert.ToInt32(ConfigurationManager.AppSettings["WEB_RSA_KEYSIZE"]);
        private static int WEB_BYTE_ENCRYPT = Convert.ToInt32(ConfigurationManager.AppSettings["WEB_BYTE_ENCRYPT"]);
        private static string WEB_PUBLICKEY = Convert.ToString(ConfigurationManager.AppSettings["WEB_PUBLICKEY"]);
        private static string WEB_PRIVATEKEY = Convert.ToString(ConfigurationManager.AppSettings["WEB_PRIVATEKEY"]);
        private static string WEB_URL_BASE = Convert.ToString(ConfigurationManager.AppSettings["WEB_URL_BASE"]);
        private static string WEB_URL_LOG_BASE = Convert.ToString(ConfigurationManager.AppSettings["WEB_URL_LOG_BASE"]);

        public static long RandomNumber(int Length)
        {
            string allowedChars = "0123456789";
            char[] chars = new char[Length];
            Random rd = new Random();

            for (int i = 0; i < Length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return long.Parse(new string(chars));
        }

        public static ObjResponse Init(string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);
                    
                    string requestUri = string.Format(WEB_URL_BASE + "init?edong={0}&auditNumber={1}", posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("Init (Request) => User: {0} \r\n Url: {1} \r\n Session: {2}", posAccount.edong, requestUri, posAccount.session);
                    HttpResponseMessage resMessage = client.GetAsync(requestUri).Result;
                    Logging.ePosResFulLogger.InfoFormat("Init (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công"                           
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("Init => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("Init => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }           
        }

        public static ObjResponse getAllAPI(string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    //pass_RSA = pass_RSA + "sdfs";
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string requestUri = string.Format(WEB_URL_BASE + "api?edong={0}&auditNumber={1}", posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("getAllAPI (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, requestUri, posAccount.session);
                    HttpResponseMessage resMessage = client.GetAsync(requestUri).Result;
                    Logging.ePosResFulLogger.InfoFormat("getAllAPI (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)// || resMessage.IsSuccessStatusCode
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công",
                            objAPIs = JsonConvert.DeserializeObject<List<ObjAPI>>(resMessage.Content.ReadAsStringAsync().Result)
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("getAllAPI => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("getAllAPI => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }           
        }
        
        public static ObjResponse getAPIbyPage(ObjAPI objAPI, int pagenumber, int pagesize, string sortOrder, string sortColumn, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string bodyMsg = JsonConvert.SerializeObject(objAPI);
                    string requestUri = string.Format(WEB_URL_BASE + "api/getPage?edong={0}&auditNumber={1}&page={2}&size={3}", posAccount.edong, RandomNumber(9), pagenumber, pagesize);
                    Logging.ePosResFulLogger.InfoFormat("getAPIbyPage (Request) => User: {0} \r\n Url: {1} \r\n Msg: {2} \r\n Session: {3}", posAccount.edong, requestUri, bodyMsg, posAccount.session);
                    HttpResponseMessage resMessage = client.PostAsync(requestUri, new StringContent(bodyMsg, UTF8Encoding.UTF8, "application/json")).Result;
                    Logging.ePosResFulLogger.InfoFormat("getAPIbyPage (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công",
                            objPageAPI = JsonConvert.DeserializeObject<ObjPageAPIResponse>(resMessage.Content.ReadAsStringAsync().Result)
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("getAPIbyPage => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("getAPIbyPage => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }
        }

        public static ObjResponse getAPIbyID(int id, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string requestUri = string.Format(WEB_URL_BASE + "api/{0}?edong={1}&auditNumber={2}", id, posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("getAPIbyID (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, requestUri, posAccount.session);
                    HttpResponseMessage resMessage = client.GetAsync(requestUri).Result;
                    Logging.ePosResFulLogger.InfoFormat("getAPIbyID (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công",
                            ObjAPI = JsonConvert.DeserializeObject<ObjAPI>(resMessage.Content.ReadAsStringAsync().Result)
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("getAPIbyID => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("getAPIbyID => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }
        }

        public static ObjResponse createAPI(ObjAPI objAPI, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string bodyMsg = JsonConvert.SerializeObject(objAPI);
                    string requestUri = string.Format(WEB_URL_BASE + "api?edong={0}&auditNumber={1}", posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("createAPI (Request) => User: {0} \r\n Url: {1} \r\n Msg: {2} \r\n Session: {3}", posAccount.edong, requestUri, bodyMsg, posAccount.session);
                    HttpResponseMessage resMessage = client.PostAsync(requestUri, new StringContent(bodyMsg, UTF8Encoding.UTF8, "application/json")).Result;
                    Logging.ePosResFulLogger.InfoFormat("createAPI (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thêm mới thành công"
                        };                      
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("createAPI => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("createAPI => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }
        }

        public static ObjResponse updateAPI(ObjAPI objAPI, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string bodyMsg = JsonConvert.SerializeObject(objAPI);
                    string requestUri = string.Format(WEB_URL_BASE + "api/{0}?edong={1}&auditNumber={2}", objAPI.id, posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("updateAPI (Request) => User: {0} \r\n Url: {1} \r\n Msg: {2} \r\n Session: {3}", posAccount.edong, requestUri, bodyMsg, posAccount.session);
                    HttpResponseMessage resMessage = client.PutAsync(requestUri, new StringContent(bodyMsg, UTF8Encoding.UTF8, "application/json")).Result;
                    Logging.ePosResFulLogger.InfoFormat("updateAPI (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công"
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("updateAPI => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("updateAPI => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }
        }

        public static ObjResponse getAllAgent(string privateKey, ePosAccount posAccount)
        {
            try
            {
                using(var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string requestUri = string.Format(WEB_URL_BASE + "agent?edong={0}&auditNumber={1}", posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("getAllAgent (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, requestUri, posAccount.session);
                    HttpResponseMessage resMessage = client.GetAsync(requestUri).Result;
                    Logging.ePosResFulLogger.InfoFormat("getAllAgent (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công",
                            objAgents = JsonConvert.DeserializeObject<List<ObjAgent>>(resMessage.Content.ReadAsStringAsync().Result)
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("getAllAgent => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("getAllAgent => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }            
        }

        public static ObjResponse getAllRole(string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string requestUri = string.Format(WEB_URL_BASE + "roles?edong={0}&auditNumber={1}", posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("getAllRole (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, requestUri, posAccount.session);
                    HttpResponseMessage resMessage = client.GetAsync(requestUri).Result;
                    Logging.ePosResFulLogger.InfoFormat("getAllRole (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {                       
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công",
                            objRoles = JsonConvert.DeserializeObject<List<ObjRole>>(resMessage.Content.ReadAsStringAsync().Result)
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("getAllAPI => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("getAllRole => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }            
        }

        public static ObjResponse createRole(ObjRole objRole, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string bodyMsg = JsonConvert.SerializeObject(objRole);
                    string requestUri = string.Format(WEB_URL_BASE + "roles?edong={0}&auditNumber={1}", posAccount.edong, RandomNumber(9));

                    Logging.ePosResFulLogger.InfoFormat("createRole (Request) => User: {0} \r\n Url: {1} \r\n Msg: {2} \r\n Session: {3}", posAccount.edong, requestUri, bodyMsg, posAccount.session);
                    HttpResponseMessage resMessage = client.PostAsync(requestUri, new StringContent(bodyMsg, UTF8Encoding.UTF8, "application/json")).Result;
                    Logging.ePosResFulLogger.InfoFormat("createRole (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công"
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("createRole => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("createRole => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }           
        }

        public static ObjResponse updateRole(ObjRole objRole, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string bodyMsg = JsonConvert.SerializeObject(objRole);
                    string requestUri = string.Format(WEB_URL_BASE + "roles/{0}?edong={1}&auditNumber={2}", objRole.id, posAccount.edong, RandomNumber(9));
                    Logging.ePosResFulLogger.InfoFormat("updateRole (Request) => User: {0} \r\n Url: {1} \r\n Msg: {2} \r\n Session: {3}", posAccount.edong, requestUri, bodyMsg, posAccount.session);
                    HttpResponseMessage resMessage = client.PutAsync(requestUri, new StringContent(bodyMsg, UTF8Encoding.UTF8, "application/json")).Result;
                    Logging.ePosResFulLogger.InfoFormat("updateRole (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new ObjResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công"
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("updateRole => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new ObjResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("updateRole => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new ObjResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }           
        }

        public static SchedulerResponse getViewLogbyPage(ObjLogSearchRequest objViewLogE, int pagenumber, int pagesize, string sortOrder, string sortColumn, string privateKey, ePosAccount posAccount)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    WEB_MANAGER = "ADMIN";
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("agent", WEB_MANAGER);
                    string pass_RSA = CryptorEngine.Encrypt_RSA(WEB_PASS, WEB_RSA_KEYSIZE, privateKey, WEB_PASS.Length);
                    client.DefaultRequestHeaders.Add("password", pass_RSA);

                    string bodyMsg = JsonConvert.SerializeObject(objViewLogE);
                    string requestUri = string.Format(WEB_URL_LOG_BASE + "log/getPage?edong={0}&auditNumber={1}&page={2}&size={3}", posAccount.edong, RandomNumber(9), pagenumber, pagesize);
                    Logging.ePosResFulLogger.InfoFormat("getViewLogbyPage (Request) => User: {0} \r\n Url: {1} \r\n Msg: {2} \r\n Session: {3}", posAccount.edong, requestUri, bodyMsg, posAccount.session);
                    HttpResponseMessage resMessage = client.PostAsync(requestUri, new StringContent(bodyMsg, UTF8Encoding.UTF8, "application/json")).Result;
                    Logging.ePosResFulLogger.InfoFormat("getViewLogbyPage (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resMessage), posAccount.session);
                    int statuscode = (int)resMessage.StatusCode;
                    if (resMessage.IsSuccessStatusCode)
                    {
                        return new SchedulerResponse
                        {
                            code = Constant.SUCCESS_CODE,
                            msg = "Thành công",
                            objPageLogSearch = JsonConvert.DeserializeObject<ObjPageLogSearchResponse>(resMessage.Content.ReadAsStringAsync().Result)
                        };
                    }
                    else
                    {
                        Logging.ePosResFulLogger.ErrorFormat("getAPIbyPage => User: {0}, Error: {1}, Msg: {2}, Session: {3}", posAccount.edong, statuscode.ToString(), ConvertResponseCode.GetResponseDescriptionRestFull(statuscode), posAccount.session);
                        return new SchedulerResponse
                        {
                            code = statuscode.ToString(),
                            msg = ConvertResponseCode.GetResponseDescriptionRestFull(statuscode)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ePosResFulLogger.ErrorFormat("getAPIbyPage => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return new SchedulerResponse { code = HttpStatusCode.RequestTimeout.ToString(), msg = Constant.CONNECTION_ERROR_DESC };
            }
        }
    }
}