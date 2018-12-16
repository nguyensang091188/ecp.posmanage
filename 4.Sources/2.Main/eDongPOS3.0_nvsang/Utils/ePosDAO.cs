using ePOS3.Entities.RequestObject;
using ePOS3.eStoreWS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ePOS3.Utils
{
    public class ePosDAO
    {
        private static string privateKey = Convert.ToString(ConfigurationManager.AppSettings["privateKey"]);
        private static string publicKey = Convert.ToString(ConfigurationManager.AppSettings["publicKey"]);
        private static string time_service = Convert.ToString(ConfigurationManager.AppSettings["Time_service"]);
        public static string get_IPClient(bool GetLan = false)
        {
            string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.UserHostAddress;
            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                GetLan = true;
                visitorIPAddress = string.Empty;
            }
            if (GetLan)
            {
                if (string.IsNullOrEmpty(visitorIPAddress))
                {
                    string stringHostName = Dns.GetHostName();
                    IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                    IPAddress[] arrIpAddress = ipHostEntries.AddressList;
                    try
                    {
                        visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                    }
                    catch
                    {
                        try
                        {
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            try
                            {
                                arrIpAddress = Dns.GetHostAddresses(stringHostName);
                                visitorIPAddress = arrIpAddress[0].ToString();
                            }
                            catch
                            {
                                visitorIPAddress = "127.0.0.1";
                            }
                        }
                    }
                }
            }
            return visitorIPAddress;
        }

        public static string get_CPUID()
        {
            string cpuID = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if (cpuID == "")
                {
                    cpuID = mo.Properties["processorID"].Value.ToString();

                }
            }
            return cpuID;
        }

        public static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {

                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }
            return macAddress;
        }
        public static responseEntity doLogin(string user, string pass)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                Logging.LoginLogger.InfoFormat("Login => User: {0}, Ext: {1}", user, user.Trim() + " " + get_CPUID() + " " + get_IPClient() + " " + GetMacAddress());
                EStoreManagerClient eStore = new EStoreManagerClient();
                string date_call = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                requestEntity entity = new requestEntity
                {
                    phoneNumber = user.Trim(),
                    pin = CryptorEngine.Encrypt_CBC(CryptorEngine.Validate_Pass(pass.Trim()), false, privateKey, publicKey),
                    createBy = user.Trim() + " " + get_IPClient() + " " + GetMacAddress()
                };
                Logging.LoginLogger.InfoFormat("Login (Request) Call: {2} => User: {0} \r\n Msg: {1}", user, JsonConvert.SerializeObject(entity), date_call);
                resEntity = eStore.doLogin(entity);
                Logging.LoginLogger.InfoFormat("Login (Response) Call: {2} => User: {0} \r\n Msg: {1}", user, JsonConvert.SerializeObject(resEntity), date_call);
            }
            catch (Exception ex)
            {
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                Logging.LoginLogger.ErrorFormat("Login => User:{0}, code:{1},description:{2}, error:{3}", user, resEntity.code, resEntity.description, ex.Message);

            }
            return resEntity;
        }

        public static responseEntity getCancelRequest(string edong, string customer, string status, string fromdate, string todate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity();
                string date_call = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                if (posAccount.type == -1)
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.edong = edong.Trim();
                    entity.customerCode = customer.Trim();
                    entity.strStatus = status.Trim();
                    entity.strFromDate = fromdate.Trim();
                    entity.strToDate = todate.Trim();
                    entity.phoneNumber = string.Empty;
                }
                else
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.edong = edong.Trim();
                    entity.customerCode = customer.Trim();
                    entity.strStatus = status.Trim();
                    entity.strFromDate = fromdate.Trim();
                    entity.strToDate = todate.Trim();
                    entity.phoneNumber = posAccount.edong;
                }
                Logging.ManagementLogger.InfoFormat("getCancelRequest (Request) Call {3} => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session, date_call);
                resEntity = eStore.requestTransactionCancellation(entity);
                Logging.ManagementLogger.InfoFormat("getCancelRequest (Response) Call {3} => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session, date_call);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getCancelRequest => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity requestTotalErr(ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, edong = posAccount.edong };
                Logging.ManagementLogger.InfoFormat("requestTotalErr (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, BuildMessage.GetInfor(new object[] { entity }, string.Empty, "requestTotalErr"), posAccount.session);
                resEntity = eStore.requestTotalErr(entity);
                Logging.ManagementLogger.InfoFormat("requestTotalErr (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, BuildMessage.GetInfor(new object[] { resEntity }, string.Empty, "requestTotalErr"), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("requestTotalErr => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseChangePIN doChangePin(ePosAccount posAccount, string oldPIN, string newPIN)
        {
            responseChangePIN changePin = new responseChangePIN();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    phoneNumber = posAccount.edong,
                    oldPin = CryptorEngine.Encrypt_CBC(CryptorEngine.Validate_Pass(oldPIN.Trim()), false, privateKey, publicKey),
                    newPin = CryptorEngine.Encrypt_CBC(CryptorEngine.Validate_Pass(newPIN.Trim()), false, privateKey, publicKey),
                    session = posAccount.session,
                    createBy = posAccount.IP_Mac,
                    retypePin = CryptorEngine.Encrypt_CBC(CryptorEngine.Validate_Pass(newPIN.Trim()), false, privateKey, publicKey)
                };
                Logging.AccountLogger.InfoFormat("doChangePin (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                responseEntity resEntity = eStore.doChangePin(entity);
                Logging.AccountLogger.InfoFormat("doChangePin (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                changePin = resEntity.responseChangePIN;
                changePin.description = ConvertResponseCode.GetResponseDescription(int.Parse(resEntity.code));
                Logging.AccountLogger.InfoFormat("ChangePin => User: {0}, Code: {1}, Description: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
            }
            catch (Exception ex)
            {
                changePin.responseCode = Constant.CONNECTION_ERROR_CODE;
                changePin.description = Constant.CONNECTION_ERROR_DESC;
                changePin.result = false;
                Logging.AccountLogger.ErrorFormat("ChangePin => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return changePin;
        }

        public static responseEntity doLogout(ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            EStoreManagerClient eStore = new EStoreManagerClient();
            requestEntity entity = new requestEntity
            {
                edong = posAccount.edong,
                session = posAccount.session,
                createBy = posAccount.IP_Mac
            };
            Logging.LoginLogger.InfoFormat("doLogout (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
            resEntity = eStore.doLogout(entity);
            Logging.LoginLogger.InfoFormat("doLogout (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
            return resEntity;
        }

        public static responseEntity GetPCByParent(string parent, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    evnPcBO = new evnPcBO { strParentId = parent },
                    createBy = posAccount.IP_Mac
                };
                Logging.ManagementLogger.InfoFormat("GetListPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestEvnPcByParentId(entity);
                Logging.ManagementLogger.InfoFormat("GetListPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetPCByParent => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static List<SelectListItem> getDays (int index)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = " ", Text = " " });
            for(int i = 0; i< index; i++)
            {
                items.Add(new SelectListItem { Value = i + 1 + "", Text = i + 1 + "" });               
            }
            return items;
        }

        public static List<SelectListItem> GetListPC(string parentid, int index, ePosAccount posAccount)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            responseEntity resEntity = new responseEntity();
            try
            {
                string temp = parentid;
                Logging.ManagementLogger.InfoFormat("GetListPC (Request) => User: {0}, parentid: {1}, index: {2},  posAccount: {3}, Session: {4}",
                posAccount.edong, parentid, index, JsonConvert.SerializeObject(posAccount), posAccount.session);
                resEntity = GetPCByParent(parentid, posAccount);
                Logging.ManagementLogger.InfoFormat("GetListPC (Response) => User: {0} \r\n GetPCByParent: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && resEntity.listEvnPcBO.Count() > 0)
                {

                    if (index == 1)
                    {
                        items.Add(new SelectListItem { Value = " ", Text = Constant.NULL });
                        foreach (var item in (from i in resEntity.listEvnPcBO where i.pcId != long.Parse(parentid) && i.level == 2 select i))
                        {
                            items.Add(new SelectListItem { Value = item.pcId.ToString(), Text = item.ext + " - " + item.shortName });
                        }
                    }
                    else if (index == 2 || index == 3)
                    {
                        foreach (var item in (from i in resEntity.listEvnPcBO where i.pcId != long.Parse(parentid) select i))
                        {
                            items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                        }
                    }                   
                    else
                    {
                        items.Add(new SelectListItem { Value = " ", Text = Constant.NULL });
                        foreach (var item in (from i in resEntity.listEvnPcBO where i.pcId != long.Parse(parentid) select i))
                        {
                            items.Add(new SelectListItem { Value = item.pcId.ToString(), Text = item.ext + " - " + item.shortName });
                        }
                    }

                    Logging.ManagementLogger.InfoFormat("GetListPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}", posAccount.edong, JsonConvert.SerializeObject(items), posAccount.session);

                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("GetListPC => User: {0}, Code: {1} Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetListPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return items;
        }

        public static responseEntity getBookCMISbyPC(string pcCode, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode.Trim() };
                Logging.ManagementLogger.InfoFormat("getBookCMISbyPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestListBookCmisByPcCode(entity);
                Logging.ManagementLogger.InfoFormat("getBookCMISbyPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getBookCMISbyPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getChildAcc(string edong, int index, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                Logging.ManagementLogger.InfoFormat("getChildAcc => UserName: {0}, edong: {1}, index: {2}, SessionId: {3}", posAccount.edong, edong, index, posAccount.session);
                requestEntity entity = new requestEntity();
                if (string.IsNullOrEmpty(edong))
                {
                    entity.phoneNumber = posAccount.edong; entity.typeRequest = index; entity.createBy = posAccount.IP_Mac;
                }
                else
                {
                    entity.phoneNumber = edong.Trim(); entity.typeRequest = index; entity.createBy = posAccount.IP_Mac;
                }
                Logging.ManagementLogger.InfoFormat("getChildAcc (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestSubAccountInfo(entity);
                if(resEntity!=null && resEntity.code==Constant.SUCCESS_CODE)
                {
                    ePOSSession.AddObject(posAccount.session + ePOSSession.LIST_EDONG, resEntity.listAccount);
                }
                Logging.ManagementLogger.InfoFormat("getChildAcc (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getChildAcc => User: {0}, edong: {1}, index: {2}, Error: {3}, Session: {4}", posAccount.edong, edong, index, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
            }
            return resEntity;
        }
        
        public static responseEntity getChildAccFromSession(ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {                    
                Logging.ManagementLogger.InfoFormat("getChildAccFromSession (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(posAccount), posAccount.session);

                var listAccount = ePOSSession.GetObject(posAccount.session + ePOSSession.LIST_EDONG);
                if (listAccount != null)
                {
                    resEntity.code = Constant.SUCCESS_CODE;
                    resEntity.description = "Lấy danh sách ví con thành công";
                    resEntity.listAccount = (account[]) listAccount;
                }
                else
                {
                    resEntity.code = Constant.ERROR_CONNECTION;
                    resEntity.description = "Không lấy được danh sách ví con thành công";
                    resEntity.listAccount = null;
                }
                Logging.ManagementLogger.InfoFormat("getChildAccFromSession (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getChildAccFromSession => User: {0}, posAccount: {1}, Error: {2}, Session: {3}", posAccount.edong, ex.Message,JsonConvert.SerializeObject(posAccount), posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
            }
            return resEntity;
        }

        public static responseEntity doInsertBill(ePosAccount posAccount, DataXML items)
        {
            responseEntity responseEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity temp = new requestEntity();
                temp.listBill = items.bill.ToArray();
                temp.listCustomer = items.customer.ToArray();
                temp.createBy = posAccount.IP_Mac;
                Logging.ImportLogger.InfoFormat("doInsertBill (Request)=> User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, BuildMessage.GetInfor(new object[] { temp }, string.Empty, "doMergeCustomerOrBillInfo"), posAccount.session);
                responseEntity = eStore.doMergeCustomerOrBillInfo(new requestEntity
                {
                    listBill = items.bill.ToArray(),
                    listCustomer = items.customer.ToArray(),
                    createBy = posAccount.IP_Mac
                });
                Logging.ImportLogger.InfoFormat("doInsertBill (Response)=> User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(responseEntity), posAccount.session);
                Logging.ImportLogger.InfoFormat("InsertBill => User: {0}, Code: {1}, Description: {2}, Session: {3}",
                    posAccount.edong, responseEntity.code, responseEntity.description, posAccount.session);
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("InsertBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                responseEntity.code = Constant.CONNECTION_ERROR_CODE;
                responseEntity.description = Constant.CONNECTION_ERROR_DESC;
            }
            return responseEntity;
        }

        public static responseEntity TransactionOff(transactionOff transactionOff, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, edong = posAccount.edong, transactionOff = transactionOff };
                Logging.ImportLogger.InfoFormat("TransactionOff (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, BuildMessage.GetInfor(new object[] { entity }, string.Empty, "doMergeTransactionOff"), posAccount.session);
                resEntity = eStore.doMergeTransactionOff(entity);
                Logging.ImportLogger.InfoFormat("TransactionOff (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, BuildMessage.GetInfor(new object[] { resEntity }, string.Empty, "doMergeTransactionOff"), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("TransactionOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity mergeAccountBookcmisMapping(string edong, string bookCMIS, string pcCode, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    edong = edong.Trim(),
                    bookcmis = bookCMIS.Trim(),
                    pcCode = pcCode.Trim(),
                    strStatus = status.Trim(),
                    createBy = posAccount.IP_Mac
                };
                Logging.ManagementLogger.InfoFormat("mergeAccountBookcmisMapping (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeAccountBookcmisMapping(entity);
                Logging.ManagementLogger.InfoFormat("mergeAccountBookcmisMapping (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("mergeAccountBookcmisMapping => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getReportGeneral(string pcCode, string[] edong,
            string FromDateCollect, string ToDateCollect, string FromDateDebt, string ToDateDebt, string status_Acc, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity();
                entity.createBy = posAccount.IP_Mac;
                entity.pcCode = pcCode.Trim();
                entity.arrEdong = edong;
                entity.strFromDateCollect = FromDateCollect.Trim();
                entity.strToDateCollect = ToDateCollect.Trim();
                entity.strFromDateDebt = FromDateDebt.Trim();
                entity.strToDateDebt = ToDateDebt.Trim();
                entity.typeOff = status_Acc;
                entity.session = posAccount.session;
                Logging.ReportLogger.InfoFormat("getReportGeneral (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportGeneral(entity);
                Logging.ReportLogger.InfoFormat("getReportGeneral (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("getReportGeneral => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetEvnpcBlocking(string pcCode, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode.Trim() };
                Logging.ManagementLogger.InfoFormat("GetEvnpcBlocking (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestEvnpcBlocking(entity);
                Logging.ManagementLogger.InfoFormat("GetEvnpcBlocking (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetEvnpcBlocking => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getReportDetail(string pcCode, string[] edong, string customer, string bookCMIS, string status,
            string FromDateCollect, string ToDateCollect, string FromDateDebt, string ToDateDebt, string status_Acc, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity();

                if (string.IsNullOrEmpty(status))
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.pcCode = pcCode = string.IsNullOrEmpty(customer) == true ? pcCode.Trim() : "";
                    entity.customerCode = customer.Trim();
                    entity.arrEdong = edong;
                    entity.strFromDateCollect = FromDateCollect.Trim();
                    entity.strToDateCollect = ToDateCollect.Trim();
                    entity.strFromDateDebt = FromDateDebt.Trim();
                    entity.strToDateDebt = ToDateDebt.Trim();
                    entity.bookcmis = string.IsNullOrEmpty(customer) == true ? bookCMIS.Trim() : "";
                    entity.typeOff = status_Acc;
                    entity.session = posAccount.session;
                }
                else
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.pcCode = pcCode = string.IsNullOrEmpty(customer) == true ? pcCode.Trim() : "";
                    entity.customerCode = customer.Trim();
                    entity.arrEdong = edong;
                    entity.strFromDateCollect = FromDateCollect.Trim();
                    entity.strToDateCollect = ToDateCollect.Trim();
                    entity.strFromDateDebt = FromDateDebt.Trim();
                    entity.strToDateDebt = ToDateDebt.Trim();
                    entity.bookcmis = string.IsNullOrEmpty(customer) == true ? bookCMIS.Trim() : "";
                    entity.strStatus = status.Trim();
                    entity.typeOff = status_Acc;
                    entity.session = posAccount.session;
                }
                Logging.ReportLogger.InfoFormat("getReportDetail (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportDetail(entity);
                Logging.ReportLogger.InfoFormat("getReportDetail (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {

                Logging.ReportLogger.ErrorFormat("getReportDetail => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getHistoryBill(string billId, string customer, string amount, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                Logging.SupportLogger.InfoFormat("getHistoryBill => User: {0}, bill: {1}, customer: {2}, amount: {3}, Session: {4}", posAccount.edong, billId, customer, amount, posAccount.session);
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.edong,
                    customerCode = customer.Trim(),
                    strBillId = billId.Trim(),
                    strAmount = amount.Trim()
                };
                Logging.SupportLogger.InfoFormat("getHistoryBill (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestHistoryBill(entity);
                Logging.SupportLogger.InfoFormat("getHistoryBill (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getHistoryBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity requestReportTDTTGeneral(string[] account, string fromdate, string todate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity {
                    createBy = posAccount.IP_Mac,
                    edongParent = posAccount.type == 6? posAccount.edong: string.Empty,
                    arrEdong = account,
                    strFromDate = fromdate,
                    strToDate = todate
                };
                Logging.ReportLogger.InfoFormat("requestReportTDTTGeneral (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportTDTTGeneral(entity);
                Logging.ReportLogger.InfoFormat("requestReportTDTTGeneral (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("requestReportTDTTGeneral => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity requestReportTDTTDetail(string[] account, string fromdate, string todate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    edongParent = posAccount.type == 6? posAccount.edong: string.Empty,
                    arrEdong = account,
                    strFromDate = fromdate,
                    strToDate = todate
                };
                Logging.ReportLogger.InfoFormat("requestReportTDTTDetail (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportTDTTDetail(entity);
                Logging.ReportLogger.InfoFormat("requestReportTDTTDetail (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("requestReportTDTTDetail => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetReportEdongCash(string parent, string[] account, string FromDate, string ToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();               
                requestEntity entity = new requestEntity
                {
                    strFromDate = FromDate.Trim(),
                    strToDate = ToDate.Trim(),
                    arrEdong = account,
                    edongParent = parent.Trim(),
                    createBy = posAccount.IP_Mac
                };
                Logging.ReportLogger.InfoFormat("GetReportEdongCash (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportEdong(entity);
                Logging.ReportLogger.InfoFormat("GetReportEdongCash (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("GerReportEdongCash => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetReportEdongCashDetail(string parent, string account, string FromDate, string ToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    strFromDate = FromDate.Trim(),
                    strToDate = ToDate.Trim(),
                    edong = account.Trim(),
                    edongParent = parent.Trim(),
                    createBy = posAccount.IP_Mac
                };
                Logging.ReportLogger.InfoFormat("GetReportEdongCashDetail (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportEdongDetail(entity);
                Logging.ReportLogger.InfoFormat("GetReportEdongCashDetail (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("GetReportEdongCashDetail => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetReportEdongWallet(string[] account, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    arrEdong = account
                };
                Logging.ReportLogger.InfoFormat("GetReportEdongWallet (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestEdong(entity);
                Logging.ReportLogger.InfoFormat("GetReportEdongWallet (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("GetReportEdongWallet => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetReportEdongSales(string pcCode,string parent, string account, string FromDate, string ToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    pcCode = pcCode.Trim(),
                    edongParent = parent.Trim(),
                    edong = account.Trim(),
                    strFromDate = FromDate.Trim(),
                    strToDate = ToDate.Trim()
                };
                Logging.ReportLogger.InfoFormat("GetReportEdongSales (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.reportSalesCashier(entity);
                Logging.ReportLogger.InfoFormat("GetReportEdongSales (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("GerReportEdongCash => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        public static responseEntity GetReportEdongBalance(string pcCode, string parent, string account, string FromDate, string ToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    pcCode = pcCode.Trim(),
                    edongParent = parent.Trim(),
                    edong = account.Trim(),
                    strFromDate = FromDate.Trim(),
                    strToDate = ToDate.Trim()
                };
                Logging.ReportLogger.InfoFormat("GetReportEdongBalance (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.reportBalanceEdong(entity);
                Logging.ReportLogger.InfoFormat("GetReportEdongBalance (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("GetReportEdongBalance => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getCheckStock(string requestId, string edong, string FromDate, string ToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    checkStockRequest = new checkStockRequest { strRequestId = requestId.Trim(), edong = edong.Trim(), strFromDate = FromDate.Trim(), strToDate = ToDate.Trim() }
                };
                Logging.ReportLogger.InfoFormat("getCheckStock (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestCheckStockRequest(entity);
                Logging.ReportLogger.InfoFormat("getCheckStock (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("doCheckStockRequest => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity doCheckStockRequest(string type, string pc, string edong, string email, string code, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity();
                if (int.Parse(type) == 1)
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.checkStockRequest = new checkStockRequest
                    {
                        edong = edong.Trim(),
                        pcCode = pc.Trim(),
                        email = email.Trim(),
                        listCustomerCode = PhoneNumber.ProcessCustomerGroup(code)
                    };
                }
                else
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.checkStockRequest = new checkStockRequest
                    {
                        edong = edong.Trim(),
                        pcCode = pc.Trim(),
                        email = email.Trim(),
                        listBookcmis = PhoneNumber.ProcessCustomerGroup(code)
                    };
                }
                Logging.ReportLogger.InfoFormat("doCheckStockRequest (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doCheckStockRequest(entity);
                Logging.ReportLogger.InfoFormat("doCheckStockRequest (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("doCheckStockRequest => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity DownloadDataFromFilePath(string filePath, string typeFile, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    filePath = filePath.Trim(),
                    fileType = typeFile.Trim()
                };
                Logging.ReportLogger.InfoFormat("DownloadDataFromFilePath (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestDownloadDataFromFilePath(entity);
                Logging.ReportLogger.InfoFormat("DownloadDataFromFilePath (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("getBillFromFilePath => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getPCbyId(string id, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    evnPcBO = new evnPcBO { strParentId = id.Trim() }
                };
                Logging.ManagementLogger.InfoFormat("getPCbyId (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestEvnPcByParentId(entity);
                Logging.ManagementLogger.InfoFormat("getPCbyId (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getPCbyId => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity SumDeliverySPC(string pcCode, string delivery, string bookcode, string month, string year, string account, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;

                Logging.ReportLogger.InfoFormat("SumDeliverySPC (Request) => User: {0} \r\n , pcCode: {1}, delivery: {2}, bookcode: {3}, month: {4}, year: {5}, account: {6}, posAccount: {7} \r\n Session: {8}",
                   posAccount.edong, pcCode, JsonConvert.SerializeObject(resEntity), posAccount.session);
                resEntity = eStore.BC_THop_GTMobile(pcCode.Trim(), delivery.Trim(), bookcode.Trim(), month.Trim(), year.Trim(), account.Trim());
                Logging.ReportLogger.InfoFormat("SumDeliverySPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SumDeliverySPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity DetailDeliverySPC(string pcCode, string delivery, string bookcode, string month, string year, string account, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;

                Logging.ReportLogger.InfoFormat("DetailDeliverySPC (Request) => User: {0} \r\n , pcCode: {1}, delivery: {2}, bookcode: {3}, month: {4}, year: {5}, account: {6}, posAccount: {7} \r\n Session: {8}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                resEntity = eStore.BC_CTiet_GTMobile(pcCode.Trim(), delivery.Trim(), bookcode.Trim(), month.Trim(), year.Trim(), account.Trim());
                Logging.ReportLogger.InfoFormat("DetailDeliverySPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DetailDeliverySPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity DebtReliefSPCSum(string madviqly, string sobbgiao, string manvien, string mathungan,
            string ngaygiao, string thang, string nam, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;

                resEntity = eStore.BC_THop_GNMobile(madviqly.Trim(), sobbgiao.Trim(), manvien.Trim(), mathungan.Trim(), ngaygiao.Trim(), thang.Trim(), nam.Trim());
                Logging.ReportLogger.InfoFormat("DebtReliefSPCSum (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, BuildMessage.GetInfor(new object[] { resEntity }, string.Empty, "BC_THop_GNMobile"), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DebtReliefSPCSum => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity DebtReliefSPCDetail(string madviqly, string sobbgiao, string manvien, string mathungan, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;

                Logging.ReportLogger.InfoFormat("DebtReliefSPCDetail (Request) => User: {0} \r\n madviqly: {1}, sobbgiao: {2}, manvien: {3}, mathungan: {4}, posAccount: {5} \r\n Session: {6}",
                   posAccount.edong, madviqly, sobbgiao, manvien, mathungan, JsonConvert.SerializeObject(posAccount), posAccount.session);
                resEntity = eStore.BC_CTiet_GNMobile(madviqly.Trim(), sobbgiao.Trim());
                Logging.ReportLogger.InfoFormat("DebtReliefSPCDetail (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DebtReliefSPCSum => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity reportWarningAssign(ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, edong = posAccount.edong };
                Logging.ReportLogger.InfoFormat("reportWarningAssign (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(posAccount), posAccount.session);
                resEntity = eStore.reportWarningAssign(entity);
                Logging.ReportLogger.InfoFormat("reportWarningAssign (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("reportWarningAssign => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity requestHanMucCPC(string pc, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, edong = posAccount.edong, pcCode = pc };
                Logging.ReportLogger.InfoFormat("requestHanMucCPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(posAccount), posAccount.session);
                resEntity = eStore.requestHanMucCPC(entity);
                Logging.ReportLogger.InfoFormat("requestHanMucCPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("requestHanMucCPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity requestHanMucNPC(ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, edong = posAccount.edong };
                Logging.ReportLogger.InfoFormat("requestHanMucNPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(posAccount), posAccount.session);
                resEntity = eStore.requestHanMucNPC(entity);
                Logging.ReportLogger.InfoFormat("requestHanMucNPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("requestHanMucNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        
        public static responseEntity reportCardNo(string pc, string custormer, string cardcode, int jgStartIndex, int jgPageSize, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;

                requestEntity entity = new requestEntity();
                entity.createBy = posAccount.IP_Mac;
                entity.pcCode = pc;
                entity.customerCode = custormer;
                entity.cardNo = cardcode;
                entity.id = jgStartIndex.ToString();
                entity.rowid = jgPageSize.ToString();
                Logging.ReportLogger.InfoFormat("MergeEvnPcTime (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.reportCardNo(entity);
                Logging.ReportLogger.InfoFormat("MergeEvnPcTime (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("reportCardNo => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity MergeEvnpcBlocking(string id, string pccode, string FromDate, string ToDate, string WorkDate, string reason, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, rowid = id.Trim(), pcCode = pccode.Trim(), strFromDate = FromDate.Trim(), strToDate = ToDate.Trim(), strWorkDate = WorkDate.Trim(), reason = reason };
                Logging.SupportLogger.InfoFormat("MergeEvnpcBlocking (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeEvnpcBlocking(entity);
                Logging.SupportLogger.InfoFormat("MergeEvnpcBlocking (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("MergeEvnpcBlocking => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        public static responseEntity getAssignHCM(string pcCode, string date, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode, strWorkDate = date };
                Logging.ManagementLogger.InfoFormat("getAssignHCM (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAssignHCM(entity);
                Logging.ManagementLogger.InfoFormat("doTransferMoney (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getAssignHCM => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetAssignBookCMIS(string pcCode, string BookCMIS, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode.Trim(), bookcmis = BookCMIS.Trim() };
                Logging.ManagementLogger.InfoFormat("GetAssignBookCMIS (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAssign(entity);
                Logging.ManagementLogger.InfoFormat("GetAssignBookCMIS (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetAssignBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getHPAssignInfo(string status, string pcCode, string strRequestId, string strFromDate, string strToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, strStatus = status.Trim(), pcCode = pcCode.Trim(), strRequestId = strRequestId.Trim(), strFromDate = strFromDate.Trim(), strToDate = strToDate.Trim() };
                Logging.ManagementLogger.InfoFormat("getHPAssignInfo (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestHPAssignInfo(entity);
                Logging.ManagementLogger.InfoFormat("getHPAssignInfo (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("requestHPAssignInfo => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getKHAssignInfo(string status, string pcCode, string strRequestId, string strFromDate, string strToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, strStatus = status.Trim(), pcCode = pcCode.Trim(), strRequestId = strRequestId.Trim(), strFromDate = strFromDate.Trim(), strToDate = strToDate.Trim() };
                Logging.ManagementLogger.InfoFormat("getKHAssignInfo (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestKHAssignInfo(entity);
                Logging.ManagementLogger.InfoFormat("getKHAssignInfo (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getKHAssignInfo => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity doTransferMoney(string edong, string amount, string desc, ePosAccount posAccount, bool flag)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity();
                if (flag)
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.edong = edong.Trim();
                    entity.strAmount = amount.Trim().Replace(",", "").Replace(".", "");
                    entity.edongParent = posAccount.edong;
                    entity.reason = string.IsNullOrEmpty(desc) == true ? string.Empty : desc.Trim();
                }
                else
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.edong = posAccount.edong;
                    entity.strAmount = amount.Trim().Replace(",", "").Replace(".", "");
                    entity.edongParent = edong.Trim();
                    entity.reason = string.IsNullOrEmpty(desc) == true ? string.Empty : desc.Trim();
                }
                Logging.ManagementLogger.InfoFormat("doTransferMoney (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doTransferMoney(entity);
                Logging.ManagementLogger.InfoFormat("doTransferMoney (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doTransferMoney => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
            return resEntity;
        }

        public static responseEntity getTransactionOff(string pcCode, string edong, string customer, string type, string status, string fromdate, string todate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    edong = edong.Trim(),
                    pcCode = pcCode.Trim(),
                    customerCode = customer.Trim(),
                    strStatus = status.Trim(),
                    strFromDate = fromdate.Trim(),
                    strToDate = todate.Trim(),
                    typeOff = type.Trim()
                };
                Logging.PushLogger.InfoFormat("getTransactionOff (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestTransactionOff(entity);
                Logging.PushLogger.InfoFormat("getTransactionOff (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getTransactionOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity UpdateTransactionOffByID(string RequestId, string status, string workDate, string Code, string desc, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    strRequestId = RequestId.Trim(),
                    strStatus = string.IsNullOrEmpty(status) == true ? string.Empty : status.Trim(),
                    strWorkDate = string.IsNullOrEmpty(workDate) == true ? string.Empty : workDate.Trim(),
                    strCode = string.IsNullOrEmpty(Code) == true ? string.Empty : Code.Trim(),
                    reason = desc

                };
                Logging.PushLogger.InfoFormat("UpdateTransactionOffByID (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doUpdateTransactionOffById(entity);
                Logging.PushLogger.InfoFormat("UpdateTransactionOffByID (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.PushLogger.ErrorFormat("UpdateTransactionOffByID => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        
        public static responseEntity TotalAmountTransactionOff(string Fromdate, string Todate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, strFromDate = Fromdate, strToDate = Todate };
                Logging.ManagementLogger.InfoFormat("TotalAmountTransactionOff (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                       posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestTotalAmountTransactionOff(entity);
                Logging.ManagementLogger.InfoFormat("TotalAmountTransactionOff (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("TotalAmountTransactionOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getTransferSurvive(string pcCode, string bookCMIS, string edong, string FromDate, string ToDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, edong = edong.Trim(), arrBookCmis = PhoneNumber.ProcessCustomerGroup(bookCMIS), pcCode = pcCode.Trim(), strFromDate = FromDate.Trim(), strToDate = ToDate.Trim() };
                Logging.ManagementLogger.InfoFormat("getTransferSurvive (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestBookcmisAccountMomentary(entity);
                Logging.ManagementLogger.InfoFormat("getTransferSurvive (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getTransferSurvive => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getBookCMISbyPC(string pcCode, string[] bookCMIS, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode.Trim(), arrBookCmis = bookCMIS };
                Logging.ManagementLogger.InfoFormat("getBookCMISbyPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                 posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestListBookCmisByPcCode(entity);
                Logging.ManagementLogger.InfoFormat("getBookCMISbyPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getBookCMISbyPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity AddTransferSurvive(string pcCode, string[] bookCMIS, string edong, string date, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode.Trim(), edong = edong.Trim(), arrBookCmis = bookCMIS, strWorkDate = date.Trim() };
                Logging.ManagementLogger.InfoFormat("AddTransferSurvive (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doInsertBookcmisAccountMomentary(entity);
                Logging.ManagementLogger.InfoFormat("AddTransferSurvive (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("AddTransferSurvive => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity DeleteBookcmisAccountMomentary(string id, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, id = id.Trim() };
                Logging.ManagementLogger.InfoFormat("DeleteBookcmisAccountMomentary (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doDeleteBookcmisAccountMonemtary(entity);
                Logging.ManagementLogger.InfoFormat("DeleteBookcmisAccountMomentary (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("DeleteBookcmisAccountMomentary => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity doCheckTrans(string fromdate, string traceNumber, string auditnumber, string billingdate, string edong, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    strAuditNumber = auditnumber,
                    requestDate = fromdate.Trim(),
                    edong = edong
                };
                Logging.ManagementLogger.InfoFormat("doCheckTrans (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doCheckTrans(entity);
                Logging.ManagementLogger.InfoFormat("doCheckTrans (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doCheckTrans => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity ApproveTransactionCancellation(string requestId, string status, string desc, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, strRequestId = requestId.Trim(), strStatus = string.IsNullOrEmpty(status) == true ? "0" : status.Trim(), edong = posAccount.edong, reason = string.IsNullOrEmpty(desc) == true ? desc : desc.Trim() };
                Logging.ManagementLogger.InfoFormat("ApproveTransactionCancellation (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doApproveTransactionCancellation(entity);
                Logging.ManagementLogger.InfoFormat("ApproveTransactionCancellation (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("ApproveTransactionCancellation => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static List<SelectListItem> getPCMapEdong(string edong, ePosAccount posAccount)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            try
            {
                if (string.IsNullOrEmpty(edong) || edong.CompareTo(posAccount.edong) == 0)
                {
                    ePosAccount temp = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                    foreach (var item in temp.EvnPC)
                    {
                        items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                    }
                }
                else
                {
                    EStoreManagerClient eStore = new EStoreManagerClient();
                    requestEntity entity = new requestEntity
                    {
                        phoneNumber = edong.Trim(),
                        createBy = posAccount.IP_Mac
                    };
                    Logging.ManagementLogger.InfoFormat("getPCMapEdong (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                       posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                    responseEntity resEntity = eStore.requestPCByAccountMapping(entity);
                    Logging.ManagementLogger.InfoFormat("getPCMapEdong (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                        posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                    if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && resEntity.listEvnPcBO != null)
                    {
                        foreach (var item in resEntity.listEvnPcBO)
                        {
                            items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                        }
                    }
                    else
                        Logging.ManagementLogger.ErrorFormat("GetListPC => User: {0}, Code: {1} Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getPCMapEdong => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return items;
        }

        public static responseEntity MergeBookCMIS(string id, string pccode, string bookCMIS, string day, string day_released, string status, string email, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity();
                if (string.IsNullOrEmpty(id))
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.pcCode = pccode.Trim();
                    entity.bookcmis = string.IsNullOrEmpty(bookCMIS.Trim()) == true ? string.Empty : bookCMIS.Trim();
                    entity.inningDate = day.Trim();
                    entity.strStatus = status.Trim();
                    entity.email = string.IsNullOrEmpty(email.Trim()) == true ? string.Empty : email.Trim();
                    entity.strWorkDate = day_released.Trim();
                }
                else
                {
                    entity.createBy = posAccount.IP_Mac;
                    entity.id = id.Trim();
                    entity.pcCode = pccode.Trim();
                    entity.bookcmis = string.IsNullOrEmpty(bookCMIS.Trim()) == true ? string.Empty : bookCMIS.Trim();
                    entity.inningDate = day.Trim();
                    entity.strStatus = status.Trim();
                    entity.email = string.IsNullOrEmpty(email.Trim()) == true ? string.Empty : email.Trim();
                    entity.strWorkDate = day_released.Trim();
                }
                Logging.ManagementLogger.InfoFormat("MergeBookCMIS (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeBookCmis(entity);
                Logging.ManagementLogger.InfoFormat("MergeBookCMIS (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("MergeBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetPCMapEdong(string edong, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity();
                if (string.IsNullOrEmpty(edong))
                {
                    entity.phoneNumber = posAccount.edong; entity.createBy = posAccount.IP_Mac;
                }
                else
                {
                    entity.phoneNumber = edong.Trim(); entity.createBy = posAccount.IP_Mac;
                }
                Logging.ManagementLogger.InfoFormat("GetPCMapEdong (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestPCByAccountMapping(entity);
                Logging.ManagementLogger.InfoFormat("GetPCMapEdong (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetPCMapEdong => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetBookCMIS(string pccode, string[] bookCMIS, string fromdate, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pccode.Trim(), arrBookCmis = bookCMIS, strFromDate = fromdate.Trim(), strStatus = string.IsNullOrEmpty(status) == true ? status : status.Trim() };
                Logging.ManagementLogger.InfoFormat("GetBookCMIS (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestBookCmis(entity);
                Logging.ManagementLogger.InfoFormat("GetBookCMIS (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getAssignBillLog(string pcCode, string[] BookCMIS, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    arrBookCmis = BookCMIS,
                    pcCode = pcCode.Trim(),
                    strStatus = status.Trim()
                };
                Logging.ManagementLogger.InfoFormat("getAssignBillLog (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAssignBillLog(entity);
                Logging.ManagementLogger.InfoFormat("getAssignBillLog (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getAssignBillLog => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity UpdateAssignBillLog(string pcCode, string bookCMIS, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    pcCode = pcCode.Trim(),
                    strStatus = status.Trim(),
                    bookcmis = bookCMIS.Trim()
                };
                Logging.ManagementLogger.InfoFormat("UpdateAssignBillLog (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportAssign(entity);

                Logging.ManagementLogger.InfoFormat("UpdateAssignBillLog (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAssignBillLog => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getAccount(string edong, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    phoneNumber = edong.Trim()
                };
                Logging.AccountLogger.InfoFormat("getAccount (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAccountInfo(entity);
                Logging.AccountLogger.InfoFormat("getAccount (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.AccountLogger.ErrorFormat("getAccount => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity mergeMappingAccountParent(string edong, string parent, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {

                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    edong = edong.Trim(),
                    edongParent = parent.Trim(),
                    strStatus = status.Trim(),
                    createBy = posAccount.IP_Mac
                };
                Logging.ManagementLogger.InfoFormat(" mergeMappingAccountParent (Request) => User: {0} \r\n Msg: {1} \r\n Sesion: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeMappingAccountParent(entity);
                Logging.ManagementLogger.InfoFormat(" mergeMappingAccountParent (Response) => User: {0} \r\n Msg: {1} \r\n Sesion: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("mergeMappingAccountParent => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity mergeMappigAccountEVNPC(string edong, string pccode, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    edong = edong.Trim(),
                    pcCode = pccode.Trim(),
                    strStatus = status.Trim(),
                    createBy = posAccount.IP_Mac
                };
                Logging.ManagementLogger.InfoFormat("mergeMappigAccountEVNPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeMappingAccountEVNPC(entity);
                Logging.ManagementLogger.InfoFormat("mergeMappigAccountEVNPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("mergeMappigAccountEVNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getBookCMISMapping(string edong, string pcCode, string bookcmis, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    edong = edong.Trim(),
                    bookcmis = bookcmis.Trim(),
                    pcCode = pcCode.Trim(),
                    strStatus = status.Trim(),
                    createBy = posAccount.IP_Mac
                };
                Logging.ManagementLogger.InfoFormat("getBookCMISMapping (Request) User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAccountBookcmisMapping(entity);
                Logging.ManagementLogger.InfoFormat("getBookCMISMapping (Response) User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getBookCMISMapping => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity UpdateAccount(string edong, string name, string idNumber, string idNumberPlace, string idNumberDate,
            string address, string type, string email, string phone, string debtdate, string debtamount, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    account = new account
                    {
                        edong = edong.Trim(),
                        name = name.Trim(),
                        idNumber = idNumber.Trim(),
                        idNumberDate = idNumberDate.Trim(),
                        idNumberPlace = idNumberPlace.Trim(),
                        address = address.Trim(),
                        strType = type.Trim(),
                        email = email.Trim(),
                        phone = phone.Trim()
                    },
                    strWorkDate = debtdate.Trim(),
                    strAmount = debtamount.Trim()
                };
                Logging.AccountLogger.InfoFormat("UpdateAccount (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doUpdateAccount(entity);
                Logging.AccountLogger.InfoFormat("UpdateAccount (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.AccountLogger.ErrorFormat("UpdateAccount => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity AddEditPC(string address, string code, string ext, string fullName, string shortName, string mailTo,
           string mailCc, string level, string parentId, string phone1, string phone2, string status, string tax, string cardEVN, string providerCode, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    evnPcBO = new evnPcBO
                    {
                        address = string.IsNullOrEmpty(address) == true ? "" : address.Trim(),
                        code = string.IsNullOrEmpty(code) == true ? "" : code.Trim(),
                        ext = string.IsNullOrEmpty(ext) == true ? "" : ext.Trim(),
                        fullName = string.IsNullOrEmpty(fullName) == true ? "" : fullName.Trim(),
                        shortName = string.IsNullOrEmpty(shortName) == true ? "" : shortName.Trim(),
                        mailTo = string.IsNullOrEmpty(mailTo) == true ? "" : mailTo.Trim(),
                        mailCc = string.IsNullOrEmpty(mailCc) == true ? "" : mailCc.Trim(),
                        strLevel = level.Trim(),
                        strParentId = parentId.Trim(),
                        phone1 = string.IsNullOrEmpty(phone1) == true ? "" : phone1.Trim(),
                        phone2 = string.IsNullOrEmpty(phone2) == true ? "" : phone2.Trim(),
                        strStatus = status.Trim(),
                        taxCode = tax.Trim(),
                        cardPrefix = cardEVN.Trim(),
                        providerCode = providerCode.Trim()
                    }
                };
                Logging.SupportLogger.InfoFormat("AddEditPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeEVNPCInfo(entity);
                Logging.SupportLogger.InfoFormat("AddEditPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("AddEditPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetEdongMapPC(string pcId, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    evnPcBO = new evnPcBO { strPcId = pcId.Trim() }
                };
                Logging.SupportLogger.InfoFormat("GetEdongMapPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAccountByPcMapping(entity);
                Logging.SupportLogger.InfoFormat("GetEdongMapPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("GetEdongMapPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetListPC(string ParentCode, string Name, string Code, string Tax,
            string Address, string Phone, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                Logging.SupportLogger.InfoFormat("GetListPC => User: {0}, ParentCode: {1}, Name: {2}, Code: {3}, Tax: {4}, Address: {5}, Phone: {6}, Session: {7}",
                    posAccount.edong, ParentCode, Name, Code, Tax, Address, Phone, posAccount.session);
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    evnPcBO = new evnPcBO { strPcId = ParentCode.Trim(), fullName = Name.Trim(), address = Address.Trim(), code = Code.Trim(), phone1 = Phone.Trim(), taxCode = Tax.Trim() }
                };
                Logging.SupportLogger.InfoFormat("GetListPC (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestEVNPCInfo(entity);
                Logging.SupportLogger.InfoFormat("GetListPC (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("GetListPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity LockUnlockAcc(string edong, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    account = new account { edong = edong.Trim(), status = status.Trim() }
                };
                Logging.SupportLogger.InfoFormat("LockUnlockAcc (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doUpdateAccount(entity);
                Logging.SupportLogger.InfoFormat("LockUnlockAcc (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("LockUnlockAcc => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity LogoutAccount(string edong, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, account = new account { edong = edong.Trim(), strLogoutTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") } };
                Logging.SupportLogger.InfoFormat("LogoutAccount (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doUpdateAccount(entity);
                Logging.SupportLogger.InfoFormat("LogoutAccount (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("LogoutAccount => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity GetCustomerBill(string pccode, string customer, string name, string address, string billId, string bookCMIS, string status,
            string from_date, string to_date, string amount_from, string amount_to, string phone, string month, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    pcCode = pccode.Trim(),
                    //customerCode = customer.Trim(),
                    customerName = name.Trim(),
                    customerAddress = address.Trim(),
                    strBillId = billId.Trim(),
                    bookcmis = bookCMIS.Trim(),
                    strStatus = status.Trim(),
                    strFromDate = from_date.Trim(),
                    strToDate = to_date.Trim(),
                    strFromAmount = amount_from.Replace(",", "").Trim(),
                    strToAmount = amount_to.Replace(",", "").Trim(),
                    customerPhone = phone.Trim(),
                    strTerm = month.Trim()
                };

                // Logging.DaoLogger.InfoFormat("GetCustomerBill =>User: {0}, Error: {1}, Session: {2}, pccode: {3}, customer: {4}, name: {5}, address: {6}, billId: {7}, bookCMIS: {8}, status: {9}, from_date: {10}, to_date: {11}, amount_from: {12}, amount_to: {13}, phone: {14}, month: {15}, posAccount: {16}",
                //posAccount.edong, posAccount.session, pccode, customer, name, address, billId, bookCMIS, status, from_date, to_date, amount_from, amount_to, phone, month, JsonConvert.SerializeObject(posAccount));

                Logging.SupportLogger.InfoFormat("GetCustomerBill (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestListCustomerBill(entity);
                Logging.SupportLogger.InfoFormat("GetCustomerBill (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("GetCustomerBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        public static responseEntity GetCustomerBillEVN(string[] customer, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    arrCustomerCode = customer
                };

                // Logging.DaoLogger.InfoFormat("GetCustomerBillEVN =>User: {0}, Error: {1}, Session: {2}, pccode: {3}, customer: {4}, name: {5}, address: {6}, billId: {7}, bookCMIS: {8}, status: {9}, from_date: {10}, to_date: {11}, amount_from: {12}, amount_to: {13}, phone: {14}, month: {15}, posAccount: {16}",
                //posAccount.edong, posAccount.session, pccode, customer, name, address, billId, bookCMIS, status, from_date, to_date, amount_from, amount_to, phone, month, JsonConvert.SerializeObject(posAccount));

                Logging.SupportLogger.InfoFormat("GetCustomerBillEVN (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestBillFromEvn(entity);
                Logging.SupportLogger.InfoFormat("GetCustomerBillEVN (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("GetCustomerBillEVN => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        public static responseEntity EvnpcTime(string pcCode, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, pcCode = pcCode };
                Logging.SupportLogger.InfoFormat("EvnpcTime (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestEvnpcTime(entity);
                Logging.SupportLogger.InfoFormat("EvnpcTime (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("EvnpcTime => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity MergeEvnPcTime(string id, string pccode, string offFlag, string offWork, string cdrTime, string open,
            string type, string checkCdr, string checkHoliday, string checkKeep, string status, string cdrSat, string cdrSun,
            string offGcs, string offMax, string dayNotOff, string notWorkSat, string notWorkSun, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity();
                entity.createBy = posAccount.IP_Mac;
                entity.evnPcTime = new evnPcTime
                {
                    strEvnPcId = id,
                    pcCodeExt = pccode,
                    strOffFlag = offFlag,
                    strOffWork = offWork,
                    cdrTime = cdrTime,
                    strOpen = open,
                    strType = type,
                    strCheckCdr = checkCdr,
                    strCheckHoliday = checkHoliday,
                    strCheckKeep = checkKeep,
                    strStatus = status,
                    cdrSat = cdrSat,
                    cdrSun = cdrSun,
                    offGcs = offGcs,
                    offMax = offMax,
                    dayNotOff = dayNotOff,
                    notWorkSat = notWorkSat,
                    notWorkSun = notWorkSun
                };
                Logging.SupportLogger.InfoFormat("MergeEvnPcTime (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeEvnPcTime(entity);
                Logging.SupportLogger.InfoFormat("MergeEvnPcTime (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("MergeEvnPcTime => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }
        
        public static responseEntity getParams(string type, string code, string name, string strStatus, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { @params = new @params { type = type.Trim(), code = code.Trim(), name = name.Trim(), strStatus = strStatus.Trim() }, createBy = posAccount.IP_Mac };
                Logging.SupportLogger.InfoFormat("getParams (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestListParams(entity);
                Logging.SupportLogger.InfoFormat("getParams (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), string.Empty, posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getParams => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity MergeParam(string type, string code, string name, string strStatus, string value, string valueAlt, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    @params = new @params { type = type.Trim(), code = code.Trim(), name = name.Trim(), strStatus = strStatus.Trim(), value = value.Trim(), valueAlt = valueAlt.Trim() }
                };
                Logging.SupportLogger.InfoFormat("MergeParam (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeParams(entity);
                Logging.SupportLogger.InfoFormat("MergeParam (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("MergeParam => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static string InitSystem(string message, ePosAccount posAccount)
        {
            try
            {
                Logging.SupportLogger.InfoFormat("InitSystem => User: {0}, message: {1}, Session: {2}", posAccount.edong, message, posAccount.edong);
                EStoreManagerClient eStore = new EStoreManagerClient();
                return eStore.initSystem(message);
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("InitSystem => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return null;
            }
        }

        public static responseEntity UpdateTransactionOff(string RequestId, string pcCode, string edong, string status,
            string strFromDate, string strToDate, string workDate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    strRequestId = RequestId.Trim(),
                    pcCode = string.IsNullOrEmpty(pcCode) == true ? string.Empty : pcCode.Trim(),
                    edong = string.IsNullOrEmpty(edong) == true ? string.Empty : edong.Trim(),
                    strStatus = string.IsNullOrEmpty(status) == true ? string.Empty : status.Trim(),
                    strFromDate = string.IsNullOrEmpty(strFromDate) == true ? string.Empty : strFromDate.Trim(),
                    strToDate = string.IsNullOrEmpty(strToDate) == true ? string.Empty : strToDate.Trim(),
                    strWorkDate = string.IsNullOrEmpty(workDate) == true ? string.Empty : workDate.Trim()
                };
                Logging.SupportLogger.InfoFormat("UpdateTransactionOff (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                  posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doUpdateTransactionOff(entity);
                Logging.SupportLogger.InfoFormat("UpdateTransactionOff (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("UpdateTransactionOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getListJobConfig(string code, string name, string desc, string sClass, string status, string dayOfMonth, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    jobConfig = new jobConfig { code = code.Trim(), name = name.Trim(), description = desc.Trim(), sClass = sClass.Trim(), dayOfMonth = dayOfMonth.Trim(), status = status.Trim() }
                };
                Logging.SupportLogger.InfoFormat("getListJobConfig (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestListJobConfig(entity);
                Logging.SupportLogger.InfoFormat("getListJobConfig (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getListJobConfig => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity MergeJobConfig(string code, string name, string desc, string sClass, string status, string dayOfMonth, string allowFrom,
            string allowTo, string repeat, string interval, string intervalUnit, string poolSize, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    jobConfig = new jobConfig
                    {
                        code = code.Trim(),
                        name = name.Trim(),
                        description = desc.Trim(),
                        sClass = sClass.Trim(),
                        dayOfMonth = dayOfMonth.Trim(),
                        status = status.Trim(),
                        allowFrom = allowFrom.Trim(),
                        allowTo = allowTo.Trim(),
                        repeat = repeat.Trim(),
                        interval = interval.Trim(),
                        intervalUnit = intervalUnit.Trim(),
                        poolSize = poolSize.Trim()
                    }
                };
                Logging.SupportLogger.InfoFormat("MergeJobConfig (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeJobConfig(entity);
                Logging.SupportLogger.InfoFormat("MergeJobConfig (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("MergeJobConfig => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getReportGeneral_Water(string[] pcCode, string[] edong, string FromDate, string ToDate, string billingType, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, arrProviderCode = pcCode, arrEdong = edong, strFromDate = FromDate, strToDate = ToDate, billingType = billingType };
                Logging.ReportLogger.InfoFormat("getReportGeneral_Water (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.reportWacoGeneral(entity);
                Logging.ReportLogger.InfoFormat("getReportGeneral_Water (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("getReportGeneral_Water => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getReportDetail_Water(string[] pcCode, string parent_edong, string[] edong, string customer, string FromDate, string ToDate, string BillingType, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, arrProviderCode = pcCode, edongParent = parent_edong, arrEdong = edong, strFromDate = FromDate, strToDate = ToDate, customerCode = customer, billingType = BillingType, strStatus = status };
                Logging.ReportLogger.InfoFormat("getReportDetail_Water (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.reportWacoDetail(entity);
                Logging.ReportLogger.InfoFormat("getReportDetail_Water (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("getReportDetail_Water => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity AddAgent_API(string name, string desc, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    strCode = string.IsNullOrEmpty(name.Trim()) == true ? string.Empty : name.Trim(),
                    strName = string.IsNullOrEmpty(desc.Trim()) == true ? string.Empty : desc.Trim()
                };
                Logging.SupportLogger.InfoFormat("AddAgent_API (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeService(entity);
                Logging.SupportLogger.InfoFormat("AddAgent_API (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("AddAgent_API => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getAgent(ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, strStatus = Constant.STATUS_ONLINE };
                Logging.SupportLogger.InfoFormat("getAgent (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestAgent(entity);
                Logging.SupportLogger.InfoFormat("GetEvnpcBlocking (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getAgent => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getRole(string agent, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, id = agent };
                Logging.SupportLogger.InfoFormat("getRole (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestRole(entity);
                Logging.SupportLogger.InfoFormat("getRole (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getRole => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity UpdateRole(string agent, string role, string status, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, strRequestId = agent.Trim(), id = role.Trim(), strStatus = status.Trim() };
                Logging.SupportLogger.InfoFormat("UpdateRole (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                   posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeRole(entity);
                Logging.SupportLogger.InfoFormat("UpdateRole (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("UpdateRole => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity getReportForBank(string[] pcCode, string customer, string fromdate, string todate, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();
                //var time = new TimeSpan(0, int.Parse(time_service), 0);
                //eStore.Endpoint.Binding.CloseTimeout = time;
                //eStore.Endpoint.Binding.OpenTimeout = time;
                //eStore.Endpoint.Binding.ReceiveTimeout = time;
                //eStore.Endpoint.Binding.SendTimeout = time;
                requestEntity entity = new requestEntity { createBy = posAccount.IP_Mac, arrPcCodeExt = pcCode, customerCode = customer, strFromDate = fromdate, strToDate = todate };
                Logging.AccountantLogger.InfoFormat("getReportForBank (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.requestReportForBank(entity);
                Logging.AccountantLogger.InfoFormat("getReportForBank (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("getReportForBank => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }

        public static responseEntity MergeReportForBank(FileExcel_HN item, ePosAccount posAccount)
        {
            responseEntity resEntity = new responseEntity();
            try
            {
                EStoreManagerClient eStore = new EStoreManagerClient();               
                requestEntity entity = new requestEntity
                {
                    createBy = posAccount.IP_Mac,
                    bill = new bill
                    {
                        customerCode = item.CustomerCode,
                        name = item.Name,
                        address = item.Address,
                        strBillId = item.BillID,
                        strAmount = item.Amount.ToString(),
                        strStatus = item.status,
                        pcCode = item.pcCode,
                        strTerm = item.Term,
                        dateChanged = item.Time_Online
                    }
                };
                Logging.AccountantLogger.InfoFormat("MergeReportForBank (Request) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(entity), posAccount.session);
                resEntity = eStore.doMergeReportForBank(entity);
                Logging.AccountantLogger.InfoFormat("MergeReportForBank (Response) => User: {0} \r\n Msg: {1} \r\n Session: {2}",
                    posAccount.edong, JsonConvert.SerializeObject(resEntity), posAccount.session);
                return resEntity;
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("MergeReportForBank => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                resEntity.code = Constant.CONNECTION_ERROR_CODE;
                resEntity.description = Constant.CONNECTION_ERROR_DESC;
                return resEntity;
            }
        }               
    }       
}
