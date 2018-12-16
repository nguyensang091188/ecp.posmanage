namespace ePOS3.Controllers
{
    using ePOS3.eStoreWS;
    using ePOS3.Models;
    using ePOS3.Utils;
    using Newtonsoft.Json;
    using OfficeOpenXml;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    [Authorize]
    [AllowAnonymous]
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class ReportController : Controller
    {
        private static int Array_Acc = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Array_Acc"]));
        private static int Max_Array = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Max_Array"]));
        private static string HN = Convert.ToString(ConfigurationManager.AppSettings["ROOT_EDONG_HN"]);
        private static string HCM = Convert.ToString(ConfigurationManager.AppSettings["ROOT_EDONG_HCM"]);
        private static string NPC = Convert.ToString(ConfigurationManager.AppSettings["ROOT_EDONG_NPC"]);
        private static string CPC = Convert.ToString(ConfigurationManager.AppSettings["ROOT_EDONG_CPC"]);
        private static string SPC = Convert.ToString(ConfigurationManager.AppSettings["ROOT_EDONG_SPC"]);

        //KHAI BAO VI TONG
        private static string VI_TONG_ECPAY = Convert.ToString(ConfigurationManager.AppSettings["VI_TONG_ECPAY"]);
        private static string VI_TONG_VIETTEL = Convert.ToString(ConfigurationManager.AppSettings["VI_TONG_VIETTEL"]);
        private static string VI_TONG_BAO_HIEM = Convert.ToString(ConfigurationManager.AppSettings["VI_TONG_BAO_HIEM"]);

        #region Báo cáo tổng hợp       
        public ActionResult PointCollection()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.RPTPOINTCOLLECTION_TITLE;
            ViewBag.TitleLeft = "Tổng hợp";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ReportPointCollectionModel model = new ReportPointCollectionModel();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ReportLogger.ErrorFormat("PointCollection => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            Dictionary<string, string> temp_dict = new Dictionary<string, string>();
            foreach (var item in Constant.TopupPrePaid())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.TopupPostPaid())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.Provider_VTV())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            PCList.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            foreach (var item in Constant.Provider_Water())
            {
                PCList.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
            }
            foreach (var item in temp_dict)
            {
                PCList.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
            }
            foreach (var item in Constant.CardData())
            {
                PCList.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
            }
            foreach (var item in Constant.CardGame())
            {
                PCList.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
            }
            if (tempPosAcc.Childs == null)
            {
                Logging.ReportLogger.ErrorFormat("PointCollection => UserName: {0}, Error: Ví quản lý chưa có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> ParentList = new List<SelectListItem>();
            ParentList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            if (posAccount.type != -1)
            {
                ParentList.Add(new SelectListItem { Value = posAccount.edong, Text = posAccount.edong + " - " + posAccount.name });
                foreach (var item in (from x in tempPosAcc.Childs where x.parent == posAccount.edong select x))
                {
                    ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            else
            {
                foreach (var item in tempPosAcc.Childs)
                {
                    ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            List<SelectListItem> ChildList = new List<SelectListItem>();
            foreach (var item in tempPosAcc.Childs)
            {
                ChildList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
            }
            List<SelectListItem> StatusAccList = new List<SelectListItem>();
            foreach (var item in Constant.StatusAcc())
            {
                StatusAccList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }

            List<SelectListItem> Services = new List<SelectListItem>();
            foreach (var item in Constant.ECPayService())
            {
                Services.Add(new SelectListItem { Value = item.Key.ToString(), Text = item.Value });
            }
            model.PCList = PCList;
            model.ParentList = ParentList;
            model.Services = Services;
            model.StatusAccList = StatusAccList;
            model.ChildList = ChildList;
            return View(model);
        }

        [HttpPost]
        public JsonResult ChangePC(int service, int index)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            try
            {
                List<SelectListItem> items = new List<SelectListItem>();
                if (index == 0)
                {
                    if (service == 0)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        if (tempPosAcc.EvnPC != null)
                        {
                            foreach (var item in tempPosAcc.EvnPC)
                            {
                                items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                            }
                        }
                        foreach (var item in Constant.Provider_Water())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                        Dictionary<string, string> temp_dict = new Dictionary<string, string>();
                        foreach (var item in Constant.TopupPrePaid())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.TopupPostPaid())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.Provider_VTV())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.CardData())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.CardGame())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.Provider_Finance())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in temp_dict)
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 1)
                    {
                        if (tempPosAcc.EvnPC == null)
                            return Json(new { Result = "ERROR", Message = "Phiên làm việc không tồn tại" });
                        else
                        {
                            items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                            foreach (var item in tempPosAcc.EvnPC)
                            {
                                items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                            }
                        }
                    }
                    else if (service == 2)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.Provider_Water())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 3)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.TopupPrePaid())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }

                    }
                    else if (service == 4)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.TopupPostPaid())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 5)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.TopupPrePaid())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 6)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.Provider_VTV())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 7)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.CardData())
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                    }
                    else if (service == 9)//finance
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.Provider_Finance())
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                    }
                    else
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                        foreach (var item in Constant.CardGame())
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                    }

                }
                else
                {
                    if (service == 0)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        Dictionary<string, string> temp_dict = new Dictionary<string, string>();
                        foreach (var item in Constant.TopupPrePaid())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.TopupPostPaid())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.CardData())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.CardGame())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.Provider_Finance())
                        {
                            if (!temp_dict.ContainsKey(item.Key))
                                temp_dict.Add(item.Key, item.Value);
                        }
                        foreach (var item in Constant.Provider_Water())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                        foreach (var item in Constant.Provider_VTV())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                        foreach (var item in temp_dict)
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 1)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.Provider_Water())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 2 || service == 4)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.TopupPrePaid())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 3)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.TopupPostPaid())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 5)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.Provider_VTV())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 6)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.CardData())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else if (service == 9)
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.Provider_Finance())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                    else
                    {
                        items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
                        foreach (var item in Constant.CardGame())
                        {
                            items.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
                        }
                    }
                }
                return Json(new { Result = "SUCCESS", Array = items });
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ChangePC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchPointCollection(int option = 0, string pcCode = "", string pcName = "", string Account = "", string ListAcc = "",
            string Pay_FromDate = "", string Pay_ToDate = "", string Upload_FromDate = "", string Upload_ToDate = "", string status_Acc = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string[] provider_water = PhoneNumber.ProcessProvider(Constant.Provider_Water());
                string[] provider_vtv = PhoneNumber.ProcessProvider(Constant.Provider_VTV());
                string[] provider_postpaid = PhoneNumber.ProcessProvider(Constant.TopupPostPaid());
                string[] provider_prepaid = PhoneNumber.ProcessProvider(Constant.TopupPrePaid());
                string[] provider_carddata = PhoneNumber.ProcessProvider(Constant.CardData());
                string[] provider_cardgame = PhoneNumber.ProcessProvider(Constant.CardGame());
                string[] provider_finance = PhoneNumber.ProcessProvider(Constant.Provider_Finance());

                if (option == 0)//tat ca
                {
                    string result = Validate.check_ReportGeneral(ListAcc, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                        if (string.IsNullOrEmpty(result))
                        {
                            List<ObjReport> items = new List<ObjReport>();
                            Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                            if (string.IsNullOrEmpty(ListAcc))
                            {
                                dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                            }
                            else
                            {
                                foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                                {
                                    if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                        dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                                }
                            }
                            string[] array = new string[dict_acc.Count()];
                            for (int i = 0; i < dict_acc.Count(); i++)
                            {
                                array[i] = dict_acc.ElementAt(i).Key;
                            }
                            string date = DateTime.Now.ToString();
                            if (array.Length > Array_Acc)
                            {
                                if (string.IsNullOrEmpty(pcCode))
                                {
                                    int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                    for (int i = 0; i < index; i++)
                                    {
                                        var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                        responseEntity entity = ePosDAO.getReportGeneral(pcCode, temp_Array, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 1);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, i);

                                        entity = ePosDAO.getReportGeneral_Water(provider_water, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 2);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "WATER");

                                        entity = ePosDAO.getReportGeneral_Water(provider_vtv, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 6);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "VTVCAB");

                                        entity = ePosDAO.getReportGeneral_Water(provider_postpaid, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);

                                        entity = ePosDAO.getReportGeneral_Water(provider_prepaid, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);

                                        entity = ePosDAO.getReportGeneral_Water(provider_prepaid, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);

                                        entity = ePosDAO.getReportGeneral_Water(provider_carddata, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 7);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "DATA CARD");

                                        entity = ePosDAO.getReportGeneral_Water(provider_cardgame, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 8);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "CARD GAME");
                                        //tai chinh
                                        entity = ePosDAO.getReportGeneral_Water(provider_finance, temp_Array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                    }
                                }
                                else
                                {
                                    if (pcCode.StartsWith("P"))
                                    {
                                        int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                        for (int i = 0; i < index; i++)
                                        {
                                            var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                            responseEntity entity = ePosDAO.getReportGeneral(pcCode, temp_Array, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 1);
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, i);
                                        }
                                    }
                                    else
                                    {
                                        string[] _array = new string[1];
                                        _array[0] = pcCode.Trim();
                                        if (Constant.Provider_Water().ContainsKey(pcCode) || Constant.Provider_VTV().ContainsKey(pcCode))
                                        {

                                            int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                            for (int i = 0; i < index; i++)
                                            {
                                                var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                                {
                                                    if (entity.listReportGeneral != null)
                                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, Constant.Provider_Water().ContainsKey(pcCode) == true ? 2 : 6);
                                                }
                                                else
                                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "Water");
                                            }
                                        }
                                        else if (Constant.TopupPostPaid().ContainsKey(pcCode))
                                        {
                                            int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                            for (int i = 0; i < index; i++)
                                            {
                                                var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                                {
                                                    if (entity.listReportGeneral != null)
                                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                                }
                                                else
                                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);
                                            }
                                        }
                                        else if (Constant.TopupPrePaid().ContainsKey(pcCode))
                                        {
                                            int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                            for (int i = 0; i < index; i++)
                                            {
                                                var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                                {
                                                    if (entity.listReportGeneral != null)
                                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                                }
                                                else
                                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);
                                                entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                                {
                                                    if (entity.listReportGeneral != null)
                                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                                }
                                                else
                                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                            }
                                        }
                                        else if (Constant.CardData().ContainsKey(pcCode) || Constant.CardGame().ContainsKey(pcCode))
                                        {
                                            int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                            for (int i = 0; i < index; i++)
                                            {
                                                var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                                {
                                                    if (entity.listReportGeneral != null)
                                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, Constant.CardData().ContainsKey(pcCode) == true ? 7 : 8);
                                                }
                                                else
                                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                            }
                                        }
                                        //tai chinh
                                        else if (Constant.Provider_Finance().ContainsKey(pcCode))
                                        {
                                            int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                            for (int i = 0; i < index; i++)
                                            {
                                                var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                                {
                                                    if (entity.listReportGeneral != null)
                                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                                }
                                                else
                                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(pcCode))
                                {
                                    responseEntity entity = ePosDAO.getReportGeneral(pcCode, array, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                        {
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 1);
                                        }
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);

                                    entity = ePosDAO.getReportGeneral_Water(provider_water, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 2);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "Water");

                                    entity = ePosDAO.getReportGeneral_Water(provider_vtv, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 6);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "VTVCab");

                                    entity = ePosDAO.getReportGeneral_Water(provider_postpaid, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);

                                    entity = ePosDAO.getReportGeneral_Water(provider_prepaid, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);

                                    entity = ePosDAO.getReportGeneral_Water(provider_prepaid, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);

                                    entity = ePosDAO.getReportGeneral_Water(provider_carddata, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 7);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "DATA CARD");

                                    entity = ePosDAO.getReportGeneral_Water(provider_cardgame, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 8);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "CARD GAME");
                                    //tai chinh
                                    entity = ePosDAO.getReportGeneral_Water(provider_finance, array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                                }
                                else
                                {
                                    if (pcCode.StartsWith("P"))
                                    {
                                        responseEntity entity = ePosDAO.getReportGeneral(pcCode, array, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            if (entity.listReportGeneral != null)
                                            {
                                                items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 1);
                                            }
                                        }
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                                    }
                                    else
                                    {
                                        string[] _array = new string[1];
                                        _array[0] = pcCode.Trim();
                                        if (Constant.Provider_Water().ContainsKey(pcCode) || Constant.Provider_VTV().ContainsKey(pcCode))
                                        {
                                            responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                {
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, Constant.Provider_Water().ContainsKey(pcCode) == true ? 2 : 6);
                                                }
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.Provider_Water().ContainsKey(pcCode) == true ? "Water" : "VTVCab");
                                        }
                                        if (Constant.TopupPostPaid().ContainsKey(pcCode))
                                        {
                                            responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                {
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                                }
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);
                                        }
                                        if (Constant.TopupPrePaid().ContainsKey(pcCode))
                                        {
                                            responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                {
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                                }
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);
                                            entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                {
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                                }
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                        }
                                        else if (Constant.CardData().ContainsKey(pcCode) || Constant.CardGame().ContainsKey(pcCode))
                                        {
                                            responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                {
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, Constant.CardData().ContainsKey(pcCode) == true ? 7 : 8);
                                                }
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                        }

                                        if (Constant.Provider_Finance().ContainsKey(pcCode))
                                        {
                                            responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            {
                                                if (entity.listReportGeneral != null)
                                                {
                                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                                }
                                            }
                                            else
                                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                                        }
                                    }
                                }
                            }
                            if (items.Count > 0)
                            {

                                var temp = (from t in items orderby t.col_1 select t);
                                List<ObjReport> data = new List<ObjReport>();
                                for (int i = 0; i < temp.Count(); i++)
                                {
                                    if (i == 0)
                                        data.Add(temp.ElementAt(i));
                                    else
                                    {
                                        if (temp.ElementAt(i - 1).col_1.CompareTo(temp.ElementAt(i).col_1) == 0)
                                        {
                                            ObjReport temp_data = temp.ElementAt(i);
                                            temp_data.col_1 = "";
                                            temp_data.col_20 = "";
                                            data.Add(temp_data);
                                        }
                                        else
                                            data.Add(temp.ElementAt(i));
                                    }
                                }
                                ePOSSession.AddObject(Constant.REPORT_SUM + date, data);
                                return Json(new
                                {
                                    Result = "SUCCESS",
                                    Records = data,
                                    id = Constant.REPORT_SUM + date,
                                    service = "Tất cả",
                                    pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                    fromdate = Upload_FromDate,
                                    todate = Upload_ToDate
                                });
                            }
                            else
                                return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                        else
                        {
                            Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                            return Json(new { Result = "ERROR", Message = result });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 1) // Tiền điện
                {
                    string result = Validate.check_ReportGeneral(ListAcc, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        if (array.Length > Array_Acc)
                        {
                            List<ObjReport> items = new List<ObjReport>();
                            int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                            for (int i = 0; i < index; i++)
                            {
                                var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                responseEntity entity = ePosDAO.getReportGeneral(pcCode, temp_Array, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 1);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, i);
                                }
                            }
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_SUM + date, service = "Tiền điện", pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1], fromdate = Upload_FromDate, todate = Upload_ToDate });
                        }
                        else
                        {
                            responseEntity entity = ePosDAO.getReportGeneral(pcCode, array, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                List<ObjReport> items = new List<ObjReport>();
                                if (entity.listReportGeneral != null)
                                {
                                    items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 1);
                                }
                                ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                                return Json(new
                                {
                                    Result = "SUCCESS",
                                    Records = items,
                                    id = Constant.REPORT_SUM + date,
                                    service = "Tiền điện",
                                    pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                    fromdate = Upload_FromDate,
                                    todate = Upload_ToDate
                                });
                            }
                            else
                            {
                                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 2) // Tiên nuoc
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();

                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_water, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 2);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "Water");
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_water, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 2);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "Water");
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 2);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "Water");
                                    }
                                }
                            }
                            else
                            {

                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                    {
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 2);
                                    }
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "Water");
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Tiền nước",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 3) //  Di động trả trước
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_prepaid, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_prepaid, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                    {
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                    }
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_PREPAID, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                    {
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 3);
                                    }
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Di động trả trước",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 4) //Di động trả sau
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_postpaid, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_postpaid, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                    {
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                    }
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.TOPUP_POSTPAID, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                    {
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 4);
                                    }
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Di động trả sau",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 5) // Mua ma the
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_prepaid, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_prepaid, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 5);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                }
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Mua mã thẻ",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 6)
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_vtv, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 6);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "VTV");

                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_vtv, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 6);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "VTV");

                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 6);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "VTV");
                                    }
                                }
                            }
                            else
                            {

                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, "", posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                    {
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 6);
                                    }
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "VTV");
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Truyền hình",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 7) // the data
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_carddata, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 7);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_carddata, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 7);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 7);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 7);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Mã thẻ data 3G - 4G",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else if (option == 9) // tai chinh
                {
                    string result = Validate.check_ReportFinance(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();

                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_finance, temp_Array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_finance, array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);

                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                }
                            }
                            else
                            {

                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.FINANCE, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 9);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Tài chính",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else // the game 
                {
                    string result = Validate.check_ReportWater(ListAcc, Upload_FromDate, Upload_ToDate, 0);
                    if (string.IsNullOrEmpty(result))
                    {
                        Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                        if (string.IsNullOrEmpty(ListAcc))
                        {
                            dict_acc.Add(Account.Split('-')[0].Trim().ToUpper(), Account.Split('-')[1].Trim());
                        }
                        else
                        {
                            foreach (var item in JsonConvert.DeserializeObject<string[]>(ListAcc))
                            {
                                if (!dict_acc.ContainsKey(item.Split('-')[0].Trim().ToUpper()))
                                    dict_acc.Add(item.Split('-')[0].Trim().ToUpper(), item.Split('-')[1].Trim());
                            }
                        }
                        string[] array = new string[dict_acc.Count()];
                        for (int i = 0; i < dict_acc.Count(); i++)
                        {
                            array[i] = dict_acc.ElementAt(i).Key;
                        }
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();

                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(provider_cardgame, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 8);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportGeneral_Water(provider_cardgame, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 8);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);

                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = array.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportGeneral_Water(_array, temp_Array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        if (entity.listReportGeneral != null)
                                            items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 8);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                }
                            }
                            else
                            {

                                responseEntity entity = ePosDAO.getReportGeneral_Water(_array, array, Upload_FromDate, Upload_ToDate, Constant.BUY_CARD, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    if (entity.listReportGeneral != null)
                                        items = ReadFile.ReportGeneral(items, entity.listReportGeneral, dict_acc, posAccount, 8);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                            }
                        }
                        if (items.Count() > 0)
                        {
                            ePOSSession.AddObject(Constant.REPORT_SUM + date, items);
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Records = items,
                                id = Constant.REPORT_SUM + date,
                                service = "Mã thẻ game",
                                pc = string.IsNullOrEmpty(pcCode) == true ? "Tất cả" : pcName.Split('-')[1],
                                fromdate = Upload_FromDate,
                                todate = Upload_ToDate
                            });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                        }
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchPointCollection => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _DetailError()
        {
            return PartialView();
        }

        public PartialViewResult _DetailReportEVN()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ReportDetailEVNModel model = new ReportDetailEVNModel();
            List<SelectListItem> PCList = new List<SelectListItem>();
            if (tempPosAcc.EvnPC == null)
                Logging.ReportLogger.ErrorFormat("_DetailReportEVN => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
            else
            {
                PCList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                foreach (var item in tempPosAcc.EvnPC)
                {
                    PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                }
            }
            model.PCList = PCList;
            List<SelectListItem> ParentList = new List<SelectListItem>();
            List<SelectListItem> ChildList = new List<SelectListItem>();
            if (tempPosAcc.Childs == null)
                Logging.ReportLogger.ErrorFormat("_DetailReportEVN => UserName: {0}, Error: Ví quản lý chưa có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
            else
            {
                ParentList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                if (posAccount.type != -1)
                {
                    ParentList.Add(new SelectListItem { Value = posAccount.edong, Text = posAccount.edong + " - " + posAccount.name });
                    foreach (var item in (from x in tempPosAcc.Childs where x.parent == posAccount.edong select x))
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                else
                {
                    foreach (var item in tempPosAcc.Childs)
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                foreach (var item in tempPosAcc.Childs)
                {
                    ChildList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            model.ParentList = ParentList;
            model.ChildList = ChildList;
            List<SelectListItem> StatusDetailList = new List<SelectListItem>();
            StatusDetailList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.PaymentStatus())
            {
                StatusDetailList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            List<SelectListItem> StatusAccList = new List<SelectListItem>();
            foreach (var item in Constant.StatusAcc())
            {
                StatusAccList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.StatusDetailList = StatusDetailList;
            model.StatusAccList = StatusAccList;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult SearchDetailEVN(string pcCode = "", string Account = "", string ListAcc = "", string customer = "",
            string Pay_FromDate = "", string Pay_ToDate = "", string Upload_FromDate = "",
            string Upload_ToDate = "", string BookCMIS = "", string status = "", string status_Acc = "", int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_ReportGeneral(ListAcc, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, 1);
                if (string.IsNullOrEmpty(result))
                {
                    string[] array = string.IsNullOrEmpty(ListAcc) == true ? Account.Split(';') : JsonConvert.DeserializeObject<string[]>(ListAcc);
                    string[] temp_aray = new string[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        temp_aray[i] = array[i].Split('-')[0].Trim();
                    }
                    string date = DateTime.Now.ToString();
                    List<ObjReport> items = new List<ObjReport>();


                    decimal total_bill = 0;
                    decimal total_billTKTĐ = 0;
                    decimal total_amount = 0;
                    decimal total_amountTKTD = 0;



                    if (array.Length > Array_Acc)
                    {
                        int index = 0;
                        if (array.Length % Array_Acc > 0)
                            index = (array.Length / Array_Acc) + 1;
                        else
                            index = (array.Length / Array_Acc);
                        for (int i = 0; i < index; i++)
                        {
                            var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                            responseEntity entity = ePosDAO.getReportDetail(pcCode, temp_Array, customer, BookCMIS, status, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                items = ReadFile.ReportDetailEVN(items, entity.listBill, Upload_FromDate, Upload_ToDate, ref total_bill, ref total_billTKTĐ, ref total_amount, ref total_amountTKTD);
                                //items = ReadFile.ReportDetailEVN_(items, entity.listBill, Upload_FromDate, Upload_ToDate, ref total_bill, ref total_billTKTĐ, ref total_amount, ref total_amountTKTD);
                            }
                            else
                            {
                                Logging.ReportLogger.ErrorFormat("SearchDetailEVN => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, i);
                            }
                        }
                    }
                    else
                    {
                        responseEntity entity = ePosDAO.getReportDetail(pcCode, temp_aray, customer, BookCMIS, status, Pay_FromDate, Pay_ToDate, Upload_FromDate, Upload_ToDate, status_Acc, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            items = ReadFile.ReportDetailEVN(items, entity.listBill, Upload_FromDate, Upload_ToDate, ref total_bill, ref total_billTKTĐ, ref total_amount, ref total_amountTKTD);
                            //items = ReadFile.ReportDetailEVN_(items, entity.listBill, Upload_FromDate, Upload_ToDate, ref total_bill, ref total_billTKTĐ, ref total_amount, ref total_amountTKTD);
                        }
                        else
                        {
                            Logging.ReportLogger.ErrorFormat("SearchDetailEVN => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), total_bill = 0, total_amount = 0 });
                        }
                    }
                    if (items.Count() == 0)
                    {
                        return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ", total_bill = 0, total_amount = 0 });
                    }
                    else
                    {
                        ePOSSession.AddObject(Constant.REPORT_DETAIL + date, items);
                        List<ObjReport> data = new List<ObjReport>();
                        string totalRecord = items.Count.ToString();
                        foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = item.col_1;
                            row.col_2 = item.col_2;
                            row.col_3 = item.col_3;
                            row.col_4 = item.col_4;
                            row.col_6 = item.col_6;
                            row.col_5 = item.col_5;
                            row.col_7 = item.col_7;
                            row.col_14 = item.col_14;
                            row.col_8 = item.col_8;
                            row.col_9 = item.col_9;
                            row.col_10 = item.col_10;
                            row.col_11 = item.col_11;
                            row.col_12 = item.col_12;
                            row.col_13 = item.col_13;
                            row.col_15 = item.col_15;
                            row.col_16 = totalRecord;
                            data.Add(row);
                        }
                        int _PageLast = 0;
                        if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST) != null)
                        {
                            _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST));
                        }
                        ePOSSession.AddObject(Constant.PAGE_SIZE_LAST, pagesize);
                        //int countItem = items.Count();
                        int countItem = data.Count();
                        int MaxItem = pagesize;
                        if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize/*Không xử lý khi kích cỡ trang hiện tại lớn hơn trang trước đó*/)
                        {
                            for (int i = 0; i < MaxItem; i++)
                            {
                                ObjReport row = new ObjReport();
                                row.col_1 = i.ToString();
                                row.col_2 = i.ToString();
                                row.col_3 = i.ToString();
                                row.col_4 = i.ToString();
                                row.col_5 = i.ToString();
                                row.col_6 = i.ToString();
                                row.col_7 = i.ToString();
                                row.col_8 = i.ToString();
                                row.col_9 = i.ToString();
                                row.col_10 = i.ToString();
                                row.col_11 = i.ToString();
                                row.col_12 = i.ToString();
                                row.col_13 = i.ToString();
                                row.col_14 = i.ToString();
                                row.col_15 = i.ToString();
                                row.col_16 = totalRecord;
                                data.Insert(0, row);
                            }
                        }
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = data,
                            id = Constant.REPORT_DETAIL + date,
                            total_bill = total_bill.ToString("N0"),
                            total_billTKTĐ = total_billTKTĐ.ToString("N0"),
                            total_amount = total_amount.ToString("N0"),
                            total_amountTKTD = total_amountTKTD.ToString("N0")
                        });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchDetailEVN => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result, total_bill = 0, total_amount = 0, total_billTKTĐ = 0, total_amountTKTD = 0 });
                }

            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchDetailEVN => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), total_bill = 0, total_amount = 0 });
            }
        }

        public PartialViewResult _DetailHistoryBill(string bill, string customer, string amount)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ViewBag.BillId = bill;
                ViewBag.Customer = customer;
                ViewBag.Amount = amount.Replace(",", "").Replace(".", "");
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("_DetailHistoryBill => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
            }
            return PartialView();
        }

        [HttpPost]
        public JsonResult HistoryBill(string bill, string customer, string amount)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getHistoryBill(bill, customer, amount.Replace(",", "").Replace(".", ""), posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    for (int i = 0; i < entity.listBill.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_5 = i.ToString();
                        row.col_1 = entity.listBill.ElementAt(i).strFromDate;
                        row.col_2 = entity.listBill.ElementAt(i).edong; // thanh toan
                        if (i == 0)
                        {
                            row.col_3 = "";
                            row.col_4 = (from x in Constant.BillingType() where x.Key == entity.listBill.ElementAt(i).billingType select x).FirstOrDefault().Value;
                        }
                        else
                        {
                            row.col_3 = (from x in Constant.BillingType() where x.Key == entity.listBill.ElementAt(i - 1).billingType select x).FirstOrDefault().Value;
                            row.col_4 = (from x in Constant.BillingType() where x.Key == entity.listBill.ElementAt(i).billingType select x).FirstOrDefault().Value;
                        }
                        row.col_5 = entity.listBill.ElementAt(i).cashierPay; // so vi thuc hien                       
                        if (entity.listBill.ElementAt(i).status == 2)
                            row.col_6 = entity.listBill.ElementAt(i).billingType.CompareTo("TIMEOUT") == 0 ? "Đang chờ xử lý Time Out" :
                                (from x in Constant.BillStatus() where x.Key == entity.listBill.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                        else
                            row.col_6 = (from x in Constant.BillStatus() where x.Key == entity.listBill.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("HistoryBill => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("HistoryBill => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _DetailReportWater()
        {
            ReportDetailWater model = new ReportDetailWater();
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            List<SelectListItem> ParentList = new List<SelectListItem>();
            List<SelectListItem> ChildList = new List<SelectListItem>();
            List<SelectListItem> ServiceList = new List<SelectListItem>();
            foreach (var item in Constant.DetailService())
            {
                ServiceList.Add(new SelectListItem { Value = item.Key.ToString(), Text = item.Value });
            }
            Dictionary<string, string> temp_dict = new Dictionary<string, string>();
            foreach (var item in Constant.TopupPrePaid())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.TopupPostPaid())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.Provider_VTV())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.CardData())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.CardGame())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            foreach (var item in Constant.Provider_Finance())
            {
                if (!temp_dict.ContainsKey(item.Key))
                    temp_dict.Add(item.Key, item.Value);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            PCList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.Provider_Water())
            {
                PCList.Add(new SelectListItem { Value = item.Key.ToString(), Text = item.Key + " - " + item.Value });
            }
            foreach (var item in temp_dict)
            {
                PCList.Add(new SelectListItem { Value = item.Key, Text = item.Key + " - " + item.Value });
            }
            if (tempPosAcc.Childs == null)
                Logging.ReportLogger.ErrorFormat("_DetailReportWater => UserName: {0}, Error: Ví quản lý chưa có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
            else
            {
                ParentList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                if (posAccount.type != -1)
                {
                    ParentList.Add(new SelectListItem { Value = posAccount.edong, Text = posAccount.edong + " - " + posAccount.name });
                    foreach (var item in (from x in tempPosAcc.Childs where x.parent == posAccount.edong select x))
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                else
                {
                    foreach (var item in tempPosAcc.Childs)
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                foreach (var item in tempPosAcc.Childs)
                {
                    ChildList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            List<SelectListItem> StatusList = new List<SelectListItem>();
            StatusList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.ServiceStatus())
            {
                StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.PCList = PCList;
            model.ServiceList = ServiceList;
            model.ParentList = ParentList;
            model.ChildList = ChildList;
            model.StatusList = StatusList;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult SearchDetail_Water(int option = 0, string pcCode = "", string pcName = "", string account = "", string edong = "", string customer = "", string fromdate = "", string todate = "", string status = "", int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount temp_posAccount = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            try
            {
                string[] provider_water = PhoneNumber.ProcessProvider(Constant.Provider_Water());
                string[] provider_vtv = PhoneNumber.ProcessProvider(Constant.Provider_VTV());
                string[] provider_postpaid = PhoneNumber.ProcessProvider(Constant.TopupPostPaid());
                string[] provider_prepaid = PhoneNumber.ProcessProvider(Constant.TopupPrePaid());
                string[] provider_carddata = PhoneNumber.ProcessProvider(Constant.CardData());
                string[] provider_cardgame = PhoneNumber.ProcessProvider(Constant.CardGame());
                string[] provider_finance = PhoneNumber.ProcessProvider(Constant.Provider_Finance());

                string result = Validate.check_ReportWater(edong, fromdate, todate, 1);
                if (string.IsNullOrEmpty(result))
                {
                    string[] array = string.IsNullOrEmpty(edong) == true ? account.Split(';') : JsonConvert.DeserializeObject<string[]>(edong);
                    string[] temp_aray = new string[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        temp_aray[i] = array[i].Split('-')[0].Trim();
                    }
                    string date = DateTime.Now.ToString();
                    decimal total = 0;
                    decimal total_bill = 0;
                    List<ObjReport> items = new List<ObjReport>();
                    string s_service = "Tất cả";
                    if (option == 0) // all
                    {
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_water, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "Water");

                                    entity = ePosDAO.getReportDetail_Water(provider_postpaid, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);

                                    entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);

                                    entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Mua mã thẻ", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);

                                    entity = ePosDAO.getReportDetail_Water(provider_vtv, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "VTV");

                                    entity = ePosDAO.getReportDetail_Water(provider_carddata, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportCardData(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "DATA CARD");

                                    entity = ePosDAO.getReportDetail_Water(provider_cardgame, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportCardGame(pcCode, items, entity.listTransaction, "Thẻ Game", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "GAME CARD");
                                    //tai chinh
                                    entity = ePosDAO.getReportDetail_Water(provider_finance, "", temp_Array, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_water, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "Water");

                                entity = ePosDAO.getReportDetail_Water(provider_postpaid, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);

                                entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);

                                entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Mua mã thẻ", temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);

                                entity = ePosDAO.getReportDetail_Water(provider_vtv, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "VTV");

                                entity = ePosDAO.getReportDetail_Water(provider_carddata, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportCardData(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "DATA CARD");

                                entity = ePosDAO.getReportDetail_Water(provider_cardgame, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportCardGame(pcCode, items, entity.listTransaction, "Thẻ Game", temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "GAME CARD");

                                //tai chinh
                                entity = ePosDAO.getReportDetail_Water(provider_finance, "", temp_aray, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                if (Constant.Provider_Water().ContainsKey(pcCode) || Constant.Provider_VTV().ContainsKey(pcCode))
                                {
                                    for (int i = 0; i < index; i++)
                                    {
                                        var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                        responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            if (Constant.Provider_Water().ContainsKey(pcCode))
                                                items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                            else
                                                items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.Provider_VTV().ContainsKey(pcCode) == true ? "VTV" : "Water");
                                    }
                                }
                                else if (Constant.TopupPostPaid().ContainsKey(pcCode))
                                {
                                    for (int i = 0; i < index; i++)
                                    {
                                        var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                        responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);
                                    }
                                }
                                else if (Constant.TopupPrePaid().ContainsKey(pcCode))
                                {
                                    for (int i = 0; i < index; i++)
                                    {
                                        var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                        responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);

                                        entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);

                                    }
                                }
                                else if (Constant.CardData().ContainsKey(pcCode) || Constant.CardGame().ContainsKey(pcCode))
                                {
                                    for (int i = 0; i < index; i++)
                                    {
                                        var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                        responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            if (Constant.CardData().ContainsKey(pcCode))
                                                items = ReadFile.ReportCardData(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                            else
                                                items = ReadFile.ReportCardGame(pcCode, items, entity.listTransaction, "Thẻ game", temp_posAccount, ref total, ref total_bill);
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.CardGame().ContainsKey(pcCode) == true ? "GAME CARD" : "DATA CARD");
                                    }
                                }
                                else if (Constant.Provider_Finance().ContainsKey(pcCode))
                                {
                                    for (int i = 0; i < index; i++)
                                    {
                                        var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                        responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                            items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                        else
                                            Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                    }
                                }
                            }
                            else
                            {
                                if (Constant.Provider_Water().ContainsKey(pcCode) || Constant.Provider_VTV().ContainsKey(pcCode))
                                {
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        if (Constant.Provider_Water().ContainsKey(pcCode))
                                            items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                        else
                                            items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.Provider_VTV().ContainsKey(pcCode) == true ? "VTV" : "Water");
                                }
                                else if (Constant.TopupPostPaid().ContainsKey(pcCode))
                                {
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);
                                }
                                else if (Constant.TopupPrePaid().ContainsKey(pcCode))
                                {
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);

                                    entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                }
                                else if (Constant.CardData().ContainsKey(pcCode) || Constant.CardGame().ContainsKey(pcCode))
                                {
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        if (Constant.CardData().ContainsKey(pcCode))
                                            items = ReadFile.ReportCardData(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                        else
                                            items = ReadFile.ReportCardGame(pcCode, items, entity.listTransaction, "Thẻ game", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.CardData().ContainsKey(pcCode) == true ? "DATA CARD" : "GAME CARD");
                                }
                                else if (Constant.Provider_Finance().ContainsKey(pcCode))
                                {
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                                }
                            }
                        }
                    }
                    else if (option == 5) // truyền hình
                    {
                        s_service = "Truyền hình";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_vtv, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "VTV");
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_vtv, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "VTV");
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "VTV");
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportDetailVTV(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "VTV");
                                }
                            }
                        }
                    }
                    else if (option == 6)
                    {
                        s_service = "Thẻ data";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_carddata, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        items = ReadFile.ReportCardData(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                    else
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "DATA CARD");
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_carddata, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    items = ReadFile.ReportCardData(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                else
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "DATA CARD");
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "DATA CARD");
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Thẻ data", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "DATA CARD");
                                }
                            }
                        }
                    }
                    else if (option == 7)
                    {
                        s_service = "Thẻ game";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_cardgame, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportCardGame(pcCode, items, entity.listTransaction, "Thẻ game", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "GAME CARD");
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_cardgame, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportCardGame(pcCode, items, entity.listTransaction, "Thẻ game", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Thẻ game", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "GAME CARD");
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Thẻ game", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "GAME CARD");
                                }
                            }
                        }
                    }
                    else if (option == 4)
                    {
                        s_service = "Mua mã thẻ";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Mua mã thẻ", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Mua mã thẻ", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Mua mã thẻ", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.BUY_CARD);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.BUY_CARD, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportBuyCard(pcCode, items, entity.listTransaction, "Mua mã thẻ", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.BUY_CARD);
                                }
                            }
                        }
                    }
                    else if (option == 1)
                    {
                        s_service = "Tiền nước";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_water, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "Tiền nước");
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_water, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "Tiền nước");
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, "", status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, "Tiền nước");
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, "", status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportDetailWater(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, "Tiền nước");
                                }
                            }
                        }
                    }
                    else if (option == 2)
                    {
                        s_service = "Di động trả trước";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_prepaid, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_PREPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_PREPAID, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_PREPAID);
                                }
                            }
                        }
                    }
                    else if (option == 9)
                    {
                        s_service = "Tài chính";
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_finance, "", temp_Array, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_finance, "", temp_aray, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.FINANCE);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportFinance(pcCode, items, entity.listTransaction, "Tài chính", temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.FINANCE);
                                }
                            }
                        }
                    }
                    else
                    {
                        s_service = "Di động trả sau"; ;
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(provider_postpaid, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(provider_postpaid, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);
                                }
                            }
                        }
                        else
                        {
                            string[] _array = new string[1];
                            _array[0] = pcCode.Trim();
                            if (array.Length > Array_Acc)
                            {
                                int index = array.Length % Array_Acc > 0 == true ? (array.Length / Array_Acc) + 1 : (array.Length / Array_Acc);
                                for (int i = 0; i < index; i++)
                                {
                                    var temp_Array = temp_aray.Skip(Array_Acc * i).Take(Array_Acc).ToArray();
                                    responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_Array, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                    }
                                    else
                                    {
                                        Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}, BillingType: {5}", posAccount.edong, posAccount.session, entity.code, entity.description, i, Constant.TOPUP_POSTPAID);
                                    }
                                }
                            }
                            else
                            {
                                responseEntity entity = ePosDAO.getReportDetail_Water(_array, "", temp_aray, customer, fromdate, todate, Constant.TOPUP_POSTPAID, status, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.ReportTopup(pcCode, items, entity.listTransaction, temp_posAccount, ref total, ref total_bill);
                                }
                                else
                                {
                                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, Constant.TOPUP_POSTPAID);
                                }
                            }
                        }
                    }

                    if (items.Count() == 0)
                    {
                        return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ", total_bill = 0, total_amount = 0 });
                    }
                    else
                    {
                        ePOSSession.AddObject(Constant.REPORT_DETAIL_OTHER + date, items);
                        List<ObjReport> data = new List<ObjReport>();
                        string totalRecord = items.Count.ToString();
                        foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
                        {
                            ObjReport row = new ObjReport();
                            row.col_0 = totalRecord;
                            row.col_1 = item.col_1;
                            row.col_2 = item.col_2;
                            row.col_3 = item.col_3;
                            row.col_4 = item.col_4;
                            row.col_6 = item.col_6;
                            row.col_5 = item.col_5;
                            row.col_7 = item.col_7;
                            row.col_8 = item.col_8;
                            row.col_9 = item.col_9;
                            row.col_10 = item.col_10;
                            row.col_11 = item.col_11;
                            row.col_12 = item.col_12;
                            row.col_13 = item.col_13;
                            row.col_14 = item.col_14;
                            data.Add(row);
                        }
                        int _PageLast = 0;
                        if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_ORTHER) != null)
                        {
                            _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_ORTHER));
                        }
                        ePOSSession.AddObject(Constant.PAGE_SIZE_LAST_ORTHER, pagesize);
                        int countItem = data.Count();
                        int MaxItem = pagesize;
                        if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize/*Không xử lý khi kích cỡ trang hiện tại lớn hơn trang trước đó*/)
                        {
                            for (int i = 0; i < MaxItem; i++)
                            {
                                ObjReport row = new ObjReport();
                                row.col_0 = totalRecord;
                                row.col_1 = i.ToString();
                                row.col_2 = i.ToString();
                                row.col_3 = i.ToString();
                                row.col_4 = i.ToString();
                                row.col_5 = i.ToString();
                                row.col_6 = i.ToString();
                                row.col_7 = i.ToString();
                                row.col_8 = i.ToString();
                                row.col_9 = i.ToString();
                                row.col_10 = i.ToString();
                                row.col_11 = i.ToString();
                                row.col_12 = i.ToString();
                                row.col_13 = i.ToString();
                                row.col_14 = i.ToString();
                                data.Insert(0, row);
                            }
                        }
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = data,
                            service = s_service,
                            pc = string.IsNullOrEmpty(pcCode.Trim()) == true ? "Tất cả" : pcName.Split('-')[1],
                            id = Constant.REPORT_DETAIL_OTHER + date,
                            total_bill = total_bill.ToString("N0"),
                            total_tran = items.Count.ToString("N0"),
                            total_amount = total.ToString("N0"),
                            fromdate = fromdate,
                            todate = todate
                            //pc = pcCode
                        });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchDetail_Water => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), total_bill = 0, total_amount = 0 });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportCollectionPoint(string id, string service, string pc, string fromdate, string todate)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.Sumary(id, dir + "Temp_BCdiemthutonghop.xlsx", fromdate, todate, service, pc, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=BCdiemthutonghop.xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportCollectionPoint => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExportDetailEVN(string id)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.SummaryDetail(id, dir + "Temp_DetailEVN.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=BCdiemthuchitiet.xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportDetailEVN => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExportDetailWater(string id, string fromdate, string todate, string total_trans, string total_bill, string total_amount, string pc, string service)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.WaterDetail(id, dir + "Temp_WaterDetail.xlsx", fromdate, todate, total_trans, total_bill, total_amount, pc, service, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=Baocaochitiet_DVK.xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportDetailWater => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }
        #endregion

        #region Bao cao vi tiền mat

        public ActionResult EdongCash()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.RPTEDONGCASH_TITLE;
            ViewBag.TitleLeft = "Ví và tiền mặt";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ReportEDongCashModel model = new ReportEDongCashModel();
            try
            {
                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (tempPosAcc.Childs == null)
                {
                    Logging.ReportLogger.ErrorFormat("PointCollection => UserName: {0}, Error: Ví quản lý chưa có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
                    return RedirectToAction("Login", "ePOS", true);
                }
                List<SelectListItem> ParentList = new List<SelectListItem>();
                ParentList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                if (posAccount.type != -1)
                {
                    ParentList.Add(new SelectListItem { Value = posAccount.edong, Text = posAccount.edong + " - " + posAccount.name });
                    foreach (var item in (from x in tempPosAcc.Childs where x.parent == posAccount.edong select x))
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                else
                {
                    foreach (var item in tempPosAcc.Childs)
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                model.AccountList = ParentList;

                List<SelectListItem> AccAssignList = new List<SelectListItem>();
                foreach (var item in tempPosAcc.Childs)
                {
                    AccAssignList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
                model.AccAssignList = AccAssignList;

                List<SelectListItem> WalletList = new List<SelectListItem>();
                WalletList.Add(new SelectListItem { Value = HN, Text = "Tổng CTY Điện lực HÀ NỘI" });
                WalletList.Add(new SelectListItem { Value = HCM, Text = "Tổng CTY Điện lực HỒ CHÍ MINH" });
                WalletList.Add(new SelectListItem { Value = NPC, Text = "Tổng CTY Điện lực miền Bắc" });
                WalletList.Add(new SelectListItem { Value = CPC, Text = "Tổng CTY Điện lực miền Trung" });
                WalletList.Add(new SelectListItem { Value = SPC, Text = "Tổng CTY Điện lực miền Nam" });
                model.WalletList = WalletList;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ReportEdongCash => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchEDongCash(string wallet = "", string Account = "", string ListAcc = "", string FromDate = "", string ToDate = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_EDongCash(ListAcc, FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    Logging.ReportLogger.InfoFormat("SearchEDongCash => User: {0}, FromDate: {1}, ToDate: {2}, ListAcc: {3}, wallet: {4}, SessionId: {5}", posAccount.edong, FromDate, ToDate, ListAcc, wallet, posAccount.session);
                    string[] array = string.IsNullOrEmpty(ListAcc) == true ? Account.Split(';') : JsonConvert.DeserializeObject<string[]>(ListAcc);
                    string[] temp_aray = new string[array.Length];
                    for (int i = 0; i < array.Length; i++)
                        temp_aray[i] = array[i].Split('-')[0].Trim();

                    responseEntity entity = ePosDAO.GetReportEdongCash(wallet, temp_aray, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                        {
                            long col3 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).debt) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).debt);
                            long col4 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).availabilityAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).availabilityAmount);
                            long col7 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).rechargeParent) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).rechargeParent);
                            long col6 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).withdrawalParent) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).withdrawalParent);
                            long col8 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).susscessPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).susscessPay);
                            long col9 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                            long col11 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).addTran) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).addTran);
                            long col12 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).subTran) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).subTran);
                            ObjReport row = new ObjReport();
                            row.col_1 = entity.listResponseReportEdong.ElementAt(i).edong;
                            row.col_2 = entity.listResponseReportEdong.ElementAt(i).nameEdong;
                            row.col_3 = col3.ToString("N0");
                            row.col_4 = col4.ToString("N0");
                            row.col_5 = (col3 - col4).ToString("N0");
                            row.col_6 = col6.ToString("N0");
                            row.col_7 = col7.ToString("N0");
                            row.col_8 = col8.ToString("N0");
                            row.col_9 = col9.ToString("N0");
                            row.col_10 = (col8 + col9).ToString("N0");
                            row.col_11 = col11.ToString("N0");
                            row.col_12 = col12.ToString("N0");
                            row.col_13 = (col3 + col6 - col7).ToString("N0");
                            row.col_14 = (col4 + col6 - col7 - (col8 + col9) + col11 - col12).ToString("N0");
                            row.col_15 = ((col3 + col6 - col7) - (col4 + col6 - col7 - (col8 + col9) + col11 - col12)).ToString("N0");
                            items.Add(row);
                        }
                        ePOSSession.AddObject(Constant.REPORT_CASH + date, items);
                        return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_CASH + date });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchEDongCash => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchEDongCash => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchEDongCash => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchEDongCashDetail(string wallet = "", string account = "", string name = "", string FromDate = "", string ToDate = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_EDongCash(account, FromDate, ToDate);

                if (string.IsNullOrEmpty(result))
                {
                    Logging.ReportLogger.InfoFormat("SearchEDongCashDetail => User: {0}, FromDate: {1}, ToDate: {2}, ListAcc: {3}, wallet: {4}, SessionId: {5}", posAccount.edong, FromDate, ToDate, account, wallet, posAccount.session);
                    responseEntity entity = ePosDAO.GetReportEdongCashDetail(wallet, account, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        ObjCashDetail detail = new ObjCashDetail();
                        long temp_debt = 0;
                        long temp_amount = 0;

                        if (entity.listResponseReportEdong.Count() == 1)
                        {
                            ObjReport row = new ObjReport();
                            int i = 0;
                            temp_debt = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).debt) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).debt);
                            temp_amount = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).availabilityAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).availabilityAmount);

                            detail.debt_old = temp_debt.ToString("N0");
                            detail.Amount_old = temp_amount.ToString("N0");
                            detail.cash_old = (temp_debt - temp_amount).ToString("N0");

                            detail.debt_new = "0";
                            detail.Amount_new = "0";
                            detail.cash_new = "0";

                            long col3 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).rechargeParent) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).rechargeParent);
                            long col2 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).withdrawalParent) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).withdrawalParent);
                            long col4 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).susscessPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).susscessPay);
                            long col5 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                            long col6 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).addTran) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).addTran);
                            long col7 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).subTran) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).subTran);
                            long col9 = temp_debt + col2 - col3;
                            temp_debt = col9;
                            long col10 = temp_amount + col2 - col3 - (col4 + col5 + col6 + col7);
                            temp_amount = col10;
                            long col11 = col9 - col10;
                            row.col_1 = entity.listResponseReportEdong.ElementAt(i).reportDate;
                            row.col_2 = col2.ToString("N0");
                            row.col_3 = col3.ToString("N0");
                            row.col_4 = col4.ToString("N0");
                            row.col_5 = col5.ToString("N0");
                            row.col_6 = col6.ToString("N0");
                            row.col_7 = col7.ToString("N0");
                            row.col_8 = (col4 + col5 + col6 + col7).ToString("N0");
                            row.col_9 = col9.ToString("N0");
                            row.col_10 = col10.ToString("N0");
                            row.col_11 = col11.ToString("N0");
                            if (i == entity.listResponseReportEdong.Count() - 1)
                            {
                                detail.debt_new = col9.ToString("N0");
                                detail.Amount_new = col10.ToString("N0");
                                detail.cash_new = col11.ToString("N0");
                            }
                            items.Add(row);
                        }
                        else
                        {
                            for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                            {
                                ObjReport row = new ObjReport();
                                if (i == 0)
                                {
                                    temp_debt = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).debt) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).debt);
                                    temp_amount = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).availabilityAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).availabilityAmount);

                                    detail.debt_old = temp_debt.ToString("N0");
                                    detail.Amount_old = temp_amount.ToString("N0");
                                    detail.cash_old = (temp_debt - temp_amount).ToString("N0");

                                    detail.debt_new = "0";
                                    detail.Amount_new = "0";
                                    detail.cash_new = "0";
                                }
                                else
                                {
                                    long col3 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).rechargeParent) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).rechargeParent);
                                    long col2 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).withdrawalParent) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).withdrawalParent);
                                    long col4 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).susscessPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).susscessPay);
                                    long col5 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                                    long col6 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).addTran) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).addTran);
                                    long col7 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).subTran) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).subTran);
                                    long col9 = temp_debt + col2 - col3;
                                    temp_debt = col9;
                                    long col10 = temp_amount + col2 - col3 - (col4 + col5 + col6 + col7);
                                    temp_amount = col10;
                                    long col11 = col9 - col10;
                                    row.col_1 = entity.listResponseReportEdong.ElementAt(i).reportDate;
                                    row.col_2 = col2.ToString("N0");
                                    row.col_3 = col3.ToString("N0");
                                    row.col_4 = col4.ToString("N0");
                                    row.col_5 = col5.ToString("N0");
                                    row.col_6 = col6.ToString("N0");
                                    row.col_7 = col7.ToString("N0");
                                    row.col_8 = (col4 + col5 + col6 + col7).ToString("N0");
                                    row.col_9 = col9.ToString("N0");
                                    row.col_10 = col10.ToString("N0");
                                    row.col_11 = col11.ToString("N0");
                                    if (i == entity.listResponseReportEdong.Count() - 1)
                                    {
                                        detail.debt_new = col9.ToString("N0");
                                        detail.Amount_new = col10.ToString("N0");
                                        detail.cash_new = col11.ToString("N0");
                                    }
                                    items.Add(row);
                                }

                            }
                        }
                        detail.items = items;
                        ePOSSession.AddObject(Constant.REPORT_CASHDETAIL + date, detail);
                        return Json(new { Result = "SUCCESS", Records = detail, id = Constant.REPORT_CASHDETAIL + date, account = name });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchEDongCashDetail => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchEDongCashDetail => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchEDongCashDetail => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchEDongWallet(string Account = "", string ListAcc = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (string.IsNullOrEmpty(ListAcc))
                    return Json(new { Result = "ERROR", Message = "Vui lòng chọn thông tin VÍ TNV" });
                Logging.ReportLogger.InfoFormat("SearchEDongWallet => User: {0}, ListAcc: {1}, Session: {2}", posAccount.edong, ListAcc, posAccount.session);

                string[] array_acc = JsonConvert.DeserializeObject<string[]>(ListAcc);
                string[] array_key = new string[array_acc.Length];
                Dictionary<string, string> dict_acc = new Dictionary<string, string>();
                for (int i = 0; i < array_acc.Length; i++)
                {
                    array_key[i] = array_acc[i].Split('-')[0].Trim();
                    dict_acc.Add(array_acc[i].Split('-')[0].Trim(), array_acc[i].Split('-')[1].Trim());
                }
                List<ObjReport> items = new List<ObjReport>();
                int index = 0;
                if (array_key.Length % Max_Array > 0)
                    index = (array_key.Length / Max_Array) + 1;
                else
                    index = (array_key.Length / Max_Array);
                for (int i = 0; i < index; i++)
                {
                    var temp_Array = array_key.Skip(Max_Array * i).Take(Max_Array).ToArray();
                    responseEntity entity = ePosDAO.GetReportEdongWallet(temp_Array, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        items = ReportEDongWallet(items, entity.listResponseReportEdong, dict_acc);

                    else
                        Logging.ReportLogger.ErrorFormat("SearchEDongWallet => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, i);
                }
                if (items.Count == 0)
                    return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                else
                {
                    string date = DateTime.Now.ToString();
                    ePOSSession.AddObject(Constant.REPORT_WALLET + date, items);
                    return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_WALLET + date });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchEDongWallet => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        private static List<ObjReport> ReportEDongWallet(List<ObjReport> items, responseReportEdong[] data, Dictionary<string, string> dict_acc)
        {
            for (int i = 0; i < data.Count(); i++)
            {
                long balance = string.IsNullOrEmpty(data.ElementAt(i).balance) == true ? 0 : long.Parse(data.ElementAt(i).balance);
                long waitpay = string.IsNullOrEmpty(data.ElementAt(i).waitPay) == true ? 0 : long.Parse(data.ElementAt(i).waitPay);
                long lockmoney = string.IsNullOrEmpty(data.ElementAt(i).lockAmount) == true ? 0 : long.Parse(data.ElementAt(i).lockAmount);
                ObjReport row = new ObjReport();
                row.col_1 = data.ElementAt(i).edong;
                row.col_2 = (from x in dict_acc where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                row.col_3 = balance.ToString("N0");
                row.col_4 = waitpay.ToString("N0");
                row.col_5 = lockmoney.ToString("N0");
                row.col_6 = (balance - waitpay - lockmoney).ToString("N0");
                items.Add(row);
            }
            return items;
        }

        [AllowAnonymous]
        public ActionResult ExportEDongCash(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.CashWallet(id, dir + "Temp_SumCashWallet.xlsx", posAccount);
                //var workbook = ePOSReport.CashWallet(id, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=BCTonghopvivatienmat.xlsx", posAccount.edong));
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportEDongCash => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult ExportEDongCashDetail(string id, string account)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));

                var workbook = ePOSReport.CashWalletDetail(id, dir + "Temp_DetailCashWallet.xlsx", account, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=BCChitietvivatienmat.xlsx", posAccount.edong));

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportEDongCashDetail => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExportEDongWallet(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.EDongWallet(id, dir + "Temp_Wallet_Account.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=BCTaikhoanvi.xlsx", posAccount.edong));

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportEDongWallet => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        #endregion

        #region Bao cao ton       
        public ActionResult CheckDebt()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.RPTCHECKDEBT_TITLE;
            ViewBag.TitleLeft = "Báo cáo tồn";
            CheckDebtModel model = new CheckDebtModel();
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ReportLogger.ErrorFormat("CheckDebt => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            List<SelectListItem> EDongList = new List<SelectListItem>();
            if (posAccount.type != -1)
            {
                EDongList.Add(new SelectListItem { Value = posAccount.edong, Text = posAccount.edong + " - " + posAccount.name });
                foreach (var item in tempPosAcc.Childs)
                {
                    EDongList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            else
            {
                foreach (var item in tempPosAcc.Childs)
                {
                    EDongList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            model.EdongList = EDongList;
            model.PCList = PCList;
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchCheckDebt(string Deb_Id, string Account, string FromDate, string ToDate)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_ReportDebt(Deb_Id, FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.getCheckStock(Deb_Id, Account, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        List<ObjCheckStock> items = new List<ObjCheckStock>();
                        for (int i = 0; i < entity.listCheckStockRequests.Count(); i++)
                        {

                            ObjCheckStock item = new ObjCheckStock();
                            item.edong = entity.listCheckStockRequests.ElementAt(i).edong;
                            item.email = entity.listCheckStockRequests.ElementAt(i).email;
                            item.description = entity.listCheckStockRequests.ElementAt(i).description;
                            item.fileInputPath = string.IsNullOrEmpty(entity.listCheckStockRequests.ElementAt(i).fileInputPath) == true ? null : entity.listCheckStockRequests.ElementAt(i).fileInputPath;
                            item.fileOutputPath = string.IsNullOrEmpty(entity.listCheckStockRequests.ElementAt(i).fileOutputPath) == true ? null : entity.listCheckStockRequests.ElementAt(i).fileOutputPath;
                            item.requestId = entity.listCheckStockRequests.ElementAt(i).requestId;
                            item.createDate = entity.listCheckStockRequests.ElementAt(i).strCreatedDate;
                            item.status = (from x in Constant.CheckStatus() where x.Key == entity.listCheckStockRequests.ElementAt(i).status select x).FirstOrDefault().Value;
                            items.Add(item);
                        }
                        return Json(new { Result = "SUCCESS", Records = items });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchCheckDebt => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchCheckDebt => User: {0}, Error: {1}, SessionId: {3}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchCheckDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }


        [HttpPost]
        public JsonResult AddCheckDebt(string type, string pc, string edong, string email, string code)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_AddDebt(type, email, code);
                if (string.IsNullOrEmpty(result))
                {
                    Logging.ReportLogger.InfoFormat("AddCheckDebt => UserName: {0}, type: {1}, pc: {2}, edong: {3}, email: {4}, code: {5}, SessionId: {6}",
                        posAccount.edong, type, pc, edong, email, code, posAccount.session);
                    responseEntity entity = ePosDAO.doCheckStockRequest(type, pc, edong, email, code, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        return Json(new { Result = "SUCCESS", Message = "Gửi yêu cầu thành công" });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("AddCheckDebt => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        if (entity.code.CompareTo("0017") == 0)
                        {
                            try
                            {
                                string msg = string.Empty;
                                foreach (var item in entity.listStringResult)
                                {
                                    msg = string.IsNullOrEmpty(msg) ? item : msg + ";" + item;
                                }
                                return Json(new { Result = "ERROR", Message = "Mã khách hàng không có thông tin: " + msg });
                            }
                            catch (Exception ex)
                            {
                                Logging.ReportLogger.ErrorFormat("AddCheckDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                        else if (entity.code.CompareTo("2071") == 0)
                        {
                            try
                            {
                                string msg = string.Empty;
                                foreach (var item in entity.listStringResult)
                                {
                                    msg = string.IsNullOrEmpty(msg) ? item : msg + ";" + item;
                                }
                                return Json(new { Result = "ERROR", Message = "Sổ ghi chỉ số chưa được thiết lập: " + msg });
                            }
                            catch (Exception ex)
                            {
                                Logging.ReportLogger.ErrorFormat("AddCheckDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                        else
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("AddCheckDebt => UserName: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("AddCheckDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }


        [AllowAnonymous]
        public ActionResult ExportCheckDebtIn(string filePath, string edong, string createDate, string requestId)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<Hashtable> items = new List<Hashtable>();
                responseEntity entity = ePosDAO.DownloadDataFromFilePath(JsonConvert.DeserializeObject<string>(filePath), Constant.STATUS_OFFLINE, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listBill.Count(); i++)
                    {
                        Hashtable table = new Hashtable();
                        table.Add("STT", i + 1);
                        table.Add("customer", entity.listBill.ElementAt(i).code);
                        table.Add("GSC", string.IsNullOrEmpty(entity.listBill.ElementAt(i).bookCmis) == true ? string.Empty : entity.listBill.ElementAt(i).bookCmis);
                        table.Add("DVQL", string.IsNullOrEmpty(entity.listBill.ElementAt(i).pcCode) == true ? string.Empty : entity.listBill.ElementAt(i).pcCode);
                        items.Add(table);
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("ExportCheckDebtIn => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                }
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.InStock(items, JsonConvert.DeserializeObject<string>(edong), JsonConvert.DeserializeObject<string>(createDate), dir + "Temp_InStock.xlsx");
                // var workbook = ePOSReport.InStock(items, JsonConvert.DeserializeObject<string>(edong), JsonConvert.DeserializeObject<string>(createDate));
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument."
                                    + "spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=DataRequestStock_" + JsonConvert.DeserializeObject<string>(edong) + "_" + JsonConvert.DeserializeObject<string>(requestId) + ".xlsx", posAccount.edong));
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportCheckDebtIn => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExportCheckDebtOut(string filePath, string edong, string createDate, string requestId)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<Hashtable> items = new List<Hashtable>();
                responseEntity entity = ePosDAO.DownloadDataFromFilePath(JsonConvert.DeserializeObject<string>(filePath), Constant.STATUS_ONLINE, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    int index = 1;
                    for (int i = 0; i < entity.listBill.Count(); i++)
                    {
                        Hashtable table = new Hashtable();
                        table.Add("STT", i + 1);
                        table.Add("customer", entity.listBill.ElementAt(i).code);
                        table.Add("name", entity.listBill.ElementAt(i).name);
                        table.Add("address", entity.listBill.ElementAt(i).address);
                        table.Add("bookCMIS", entity.listBill.ElementAt(i).bookCmis);
                        table.Add("pc", entity.listBill.ElementAt(i).pcCode);
                        table.Add("phoneByevn", string.IsNullOrEmpty(entity.listBill.ElementAt(i).phoneByevn) == true ? "" : entity.listBill.ElementAt(i).phoneByevn);
                        table.Add("phoneByecp", string.IsNullOrEmpty(entity.listBill.ElementAt(i).phoneByecp) == true ? "" : entity.listBill.ElementAt(i).phoneByecp);
                        table.Add("Term", string.IsNullOrEmpty(entity.listBill.ElementAt(i).strTerm) == true ? "" : "'" + entity.listBill.ElementAt(i).strTerm);
                        table.Add("Amount", entity.listBill.ElementAt(i).amount.ToString("N0"));
                        table.Add("Description", entity.listBill.ElementAt(i).description);
                        table.Add("ElectricityMeter", entity.listBill.ElementAt(i).electricityMeter);
                        items.Add(table);
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("ExportCheckDebtOut => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                }
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.OutStock(items, JsonConvert.DeserializeObject<string>(edong), JsonConvert.DeserializeObject<string>(createDate), dir + "Temp_OutStock.xlsx");
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument."
                                 + "spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=BCkiemtraton.xlsx", posAccount.edong));

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportCheckDebtOut => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }
        #endregion

        #region Giao thu SPC
        public ActionResult DeliverySPC()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.RPT_DELIVERYSPC_TITLE;
            ViewBag.TitleLeft = "Giao thu SPC";
            DeliverySPCModel model = new DeliverySPCModel();
            List<SelectListItem> months = new List<SelectListItem>();
            months.Add(new SelectListItem { Value = "-1", Text = "ALL" });
            for (int i = 1; i <= 12; i++)
            {
                if (DateTime.Today.Month == i)
                    months.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = true });
                else
                    months.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            model.MonthList = months;
            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 2000; i <= 2050; i++)
            {
                if (DateTime.Today.Year == i)
                    years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = true });
                else
                    years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
            model.YearList = years;
            List<SelectListItem> pcCodes = new List<SelectListItem>();
            responseEntity entity = ePosDAO.getPCbyId("4", posAccount);
            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO.Count() > 0)
            {
                foreach (var item in entity.listEvnPcBO)
                {
                    pcCodes.Add(new SelectListItem { Value = item.code, Text = item.code + " - " + item.shortName });
                }
            }
            else
                Logging.ReportLogger.ErrorFormat("DeliverySPC => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
            model.PCList = pcCodes;
            return View(model);
        }

        [HttpPost]
        public JsonResult SumDeliverySPC(string Branch = "", string GCSCode = "ALL", string Account = "ALL",
           string Month = "-1", string Year = "2000", int SoBBBG = -1)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                Logging.ReportLogger.InfoFormat("SumDeliverySPC => User: {0}, Branch: {1}, GCSCode: {2}, Account: {3}, Month: {4}, Year: {5}, SoBBBG: {6}, Session: {7}",
                    posAccount.edong, Branch, GCSCode, Account, Month, Year, SoBBBG, posAccount.session);
                GCSCode = GCSCode == "" ? "ALL" : GCSCode;
                Account = Account == "" ? "ALL" : Account;
                responseEntity entity = ePosDAO.SumDeliverySPC(Branch, SoBBBG.ToString(), GCSCode, Month, Year, Account, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    string date = DateTime.Now.ToString();
                    string strBranch = "";
                    string strAccount = "";
                    string strFormDate = "";
                    string strTodate = "";
                    Dictionary<DateTime, string> dicNgayGiao = new Dictionary<DateTime, string>();
                    List<ObjDeliverySummuryReport> reports = new List<ObjDeliverySummuryReport>();
                    foreach (var item in entity.listBCTHopGTMobile)
                    {
                        ObjDeliverySummuryReport row = new ObjDeliverySummuryReport();
                        row.S_MA_DVIQLY = item.MA_DVIQLY == null ? "" : item.MA_DVIQLY;
                        row.N_PERIOD = item.KY == null ? 0 : Convert.ToInt32(item.KY);
                        row.N_MONTH = item.THANG == null ? 0 : Convert.ToInt32(item.THANG);
                        row.N_YEAR = item.NAM == null ? 0 : Convert.ToInt32(item.NAM);
                        row.S_ID_REPORT = item.SO_BBGIAO == null ? 0 : Convert.ToInt32(item.SO_BBGIAO);
                        row.S_TEN_TNGAN = item.TEN_TNGAN == null ? "" : item.TEN_TNGAN;
                        row.S_MA_TNGAN = item.MA_TNGAN == null ? "" : item.MA_TNGAN;
                        strAccount = strAccount == "" ? item.MA_TNGAN : "";
                        row.S_GCS_CODE = item.MA_SOGCS == null ? "" : item.MA_SOGCS;
                        row.S_DELIVERY_DATE = row.D_DELIVERY_DATE.ToString("yyyy-MM-dd");
                        string sNgayGiao = item.NGAY_GIAO == null ? "" : item.NGAY_GIAO;
                        if (sNgayGiao.Length >= 10)
                        {
                            DateTime dtNgayGiao = Convert.ToDateTime(sNgayGiao);
                            sNgayGiao = Convert.ToDateTime(sNgayGiao).ToString("dd/MM/yyyy");
                            if (!dicNgayGiao.ContainsKey(dtNgayGiao.Date))
                            {
                                dicNgayGiao.Add(dtNgayGiao.Date, sNgayGiao);
                            }
                        }
                        row.S_DELIVERY_DATE = sNgayGiao;
                        row.N_HC_BILL_AMOUNT = item.SO_TIEN_TD == null ? 0 : Convert.ToDecimal(item.SO_TIEN_TD);
                        row.N_VC_BILL_AMOUNT = item.SO_TIEN_VC == null ? 0 : Convert.ToDecimal(item.SO_TIEN_VC);
                        row.N_HC_BILL_SUM = item.SO_HDON_TD == null ? 0 : Convert.ToInt32(item.SO_HDON_TD);
                        row.N_VC_BILL_SUM = item.SO_HDON_VC == null ? 0 : Convert.ToInt32(item.SO_HDON_VC);
                        row.N_HC_BILL_VAT = item.TIEN_GTGT_TD == null ? 0 : Convert.ToDecimal(item.TIEN_GTGT_TD);
                        row.N_VC_BILL_VAT = item.TIEN_GTGT_VC == null ? 0 : Convert.ToDecimal(item.TIEN_GTGT_VC);
                        row.N_AMOUNT_SUM = item.TONG_TIEN == null ? 0 : Convert.ToDecimal(item.TONG_TIEN);
                        row.S_MONTH_YEAR = row.N_MONTH + "/" + row.N_YEAR;
                        row.S_MA_TNV = strAccount;
                        row.N_SOBBBG = SoBBBG;
                        reports.Add(row);
                    }

                    Dictionary<int, List<ObjDeliverySummuryReport>> dicIdReport = new Dictionary<int, List<ObjDeliverySummuryReport>>();
                    foreach (var item in reports)
                    {
                        if (!dicIdReport.ContainsKey(item.S_ID_REPORT))
                        {
                            dicIdReport.Add(item.S_ID_REPORT, new List<ObjDeliverySummuryReport>() { item });
                        }
                        else
                        {
                            dicIdReport[item.S_ID_REPORT].Add(item);
                        }
                    }

                    List<ObjDeliverySummuryReport> lstView = new List<ObjDeliverySummuryReport>();

                    foreach (var k in dicIdReport)
                    {
                        lstView.AddRange(k.Value);
                    }
                    Dictionary<string, List<ObjDeliverySummuryReport>> dicAcountName = new Dictionary<string, List<ObjDeliverySummuryReport>>();
                    foreach (var item in lstView)
                    {
                        if (!dicAcountName.ContainsKey(item.S_TEN_TNGAN))
                        {
                            dicAcountName.Add(item.S_TEN_TNGAN, new List<ObjDeliverySummuryReport>() { item });
                        }
                        else
                        {
                            dicAcountName[item.S_TEN_TNGAN].Add(item);
                        }
                    }
                    lstView = new List<ObjDeliverySummuryReport>();
                    foreach (var k in dicAcountName)
                    {
                        lstView.AddRange(k.Value);
                    }

                    List<ObjDeliverySummuryReport> lstRes = new List<ObjDeliverySummuryReport>();

                    foreach (var item in lstView)
                    {
                        var iteRes = new ObjDeliverySummuryReport();
                        iteRes.S_ID_REPORT = item.S_ID_REPORT;
                        iteRes.D_DELIVERY_DATE = item.D_DELIVERY_DATE;
                        iteRes.S_DELIVERY_DATE = item.S_DELIVERY_DATE;
                        iteRes.D_DELIVERYED_DATE = item.D_DELIVERYED_DATE;
                        iteRes.S_MA_TNGAN = item.S_MA_TNGAN;
                        iteRes.S_TEN_TNGAN = item.S_TEN_TNGAN;
                        iteRes.S_GCS_CODE = item.S_GCS_CODE;
                        iteRes.N_CUSTOMER_SUM = item.N_CUSTOMER_SUM;
                        iteRes.S_MONTH_YEAR = item.S_MONTH_YEAR;
                        iteRes.N_PERIOD = item.N_PERIOD;
                        iteRes.N_MONTH = item.N_MONTH;
                        iteRes.N_YEAR = item.N_YEAR;
                        iteRes.N_PAGE_REPORT = item.N_PAGE_REPORT;
                        iteRes.N_HC_BILL_SUM = item.N_HC_BILL_SUM;
                        iteRes.N_VC_BILL_SUM = item.N_VC_BILL_SUM;

                        iteRes.N_HC_BILL_AMOUNT = item.N_HC_BILL_AMOUNT;
                        iteRes.N_VC_BILL_AMOUNT = item.N_VC_BILL_AMOUNT;
                        iteRes.N_HC_BILL_VAT = item.N_HC_BILL_VAT;
                        iteRes.N_VC_BILL_VAT = item.N_VC_BILL_VAT;
                        iteRes.N_AMOUNT_SUM = item.N_AMOUNT_SUM;
                        iteRes.S_MA_TNV = item.S_MA_TNGAN;

                        lstRes.Add(iteRes);

                    }
                    List<string> lstIpReport = new List<string>();
                    List<string> lstIpAcountName = new List<string>();

                    int sumHC = 0;
                    int sumVC = 0;

                    decimal sumAcount = 0;
                    decimal tienHC = 0;
                    decimal tienVC = 0;
                    decimal thueHC = 0;
                    decimal thueVC = 0;
                    if (reports != null)
                    {
                        sumHC = reports.Sum(p => p.N_HC_BILL_SUM);
                        sumVC = reports.Sum(p => p.N_VC_BILL_SUM);
                        sumAcount = reports.Sum(p => p.N_AMOUNT_SUM);

                        tienHC = reports.Sum(p => p.N_HC_BILL_AMOUNT);
                        tienVC = reports.Sum(p => p.N_VC_BILL_AMOUNT);
                        thueHC = reports.Sum(p => p.N_HC_BILL_VAT);
                        thueVC = reports.Sum(p => p.N_VC_BILL_VAT);
                    }

                    if (Month != "-1")
                    {
                        DateTime dtsearch = new DateTime(Convert.ToInt16(Year), Convert.ToInt16(Month), 1);
                        strFormDate = dtsearch.ToString("dd-MM-yyyy");
                        strTodate = dtsearch.AddMonths(1).AddDays(-1).ToString("dd-MM-yyyy");
                    }
                    else
                    {
                        if (dicNgayGiao.Count > 0)
                        {
                            var fromDate = dicNgayGiao.Min(p => p.Key);
                            var toDate = dicNgayGiao.Max(p => p.Key);
                            strFormDate = dicNgayGiao[fromDate];
                            strTodate = dicNgayGiao[toDate];
                        }
                    }

                    strAccount = Account == "ALL" ? " Tất cả" : Account;
                    ObjReportDeliverySummuryCommon objParam = new ObjReportDeliverySummuryCommon()
                    {
                        PCCode = Branch,
                        Account = strAccount,
                        BillCount = sumHC + sumVC,
                        BillAmount = sumAcount,
                        AmountHC = tienHC,
                        AmountVC = tienVC,
                        VATHC = thueHC,
                        VATVC = thueVC,
                        FromDate = strFormDate,
                        ToDate = strTodate
                    };
                    ePOSSession.AddObject(ePOSSession.DELIVERY_REPORT + date, lstRes);
                    ePOSSession.AddObject(ePOSSession.DELIVERY_COMMON + date, objParam);
                    return Json(new { Result = "SUCCESS", Records = lstView, id = ePOSSession.DELIVERY_REPORT + date, obj = ePOSSession.DELIVERY_COMMON + date });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SumDeliverySPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SumDeliverySPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult DetailDeliverySPC(string BranchDetail = "", string GCSCodeDetail = "", string AccountDetail = "",
            string MonthDetail = "-1", string YearDetail = "0", int SoBBBGDetail = -1, string strTenTNV = "", string strMaTNV = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                Dictionary<string, List<ObjDeliveryDetailReport>> dicHoaDonTNV = new Dictionary<string, List<ObjDeliveryDetailReport>>();
                List<ObjDeliveryDetailReport> reports = new List<ObjDeliveryDetailReport>();
                Logging.ReportLogger.InfoFormat("DetailDeliverySPC => User: {0}, BranchDetail: {1}, SoBBBGDetail: {2}, GCSCodeDetail: {3}, MonthDetail: {4}, YearDetail: {5}, AccountDetail: {6}, Session: {7}",
                    posAccount.edong, BranchDetail, SoBBBGDetail, MonthDetail, YearDetail, AccountDetail, posAccount.session);
                GCSCodeDetail = GCSCodeDetail == "" ? "ALL" : GCSCodeDetail;
                AccountDetail = AccountDetail == "" ? "ALL" : AccountDetail;
                responseEntity entity = ePosDAO.DetailDeliverySPC(BranchDetail, SoBBBGDetail.ToString(), GCSCodeDetail, MonthDetail, YearDetail, AccountDetail, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {

                    string date = DateTime.Now.ToString();
                    string strEmpCode = "";
                    string strEmpName = "";
                    string strPc = "";
                    string sAccount = "";
                    string sAccountName = "";
                    int i = 1;

                    Dictionary<DateTime, string> dicNgayGiao = new Dictionary<DateTime, string>();
                    foreach (var item in entity.listBCCTietGTMobile)
                    {
                        var row = new ObjDeliveryDetailReport();
                        row.S_MA_DVIQLY = item.MA_DVIQLY == null ? "" : item.MA_DVIQLY;
                        if (strPc == "") { strPc = row.S_MA_DVIQLY; }
                        row.S_EDONG_ACCOUNT = item.MA_TNGAN == null ? "" : item.MA_TNGAN;
                        row.S_EDONG_ACCOUNT_NAME = item.MA_TNGAN == null ? "" : item.MA_TNGAN;
                        if (row.S_EDONG_ACCOUNT == "")
                        {
                            row.S_EDONG_ACCOUNT = strMaTNV;
                        }
                        if (row.S_EDONG_ACCOUNT_NAME == "")
                        {
                            row.S_EDONG_ACCOUNT_NAME = strTenTNV;
                        }
                        if (sAccount == "" || sAccountName == "")
                        {
                            sAccount = row.S_EDONG_ACCOUNT;
                            sAccountName = row.S_EDONG_ACCOUNT_NAME;
                        }
                        DateTime dateFrom = new DateTime(1900, 01, 01);
                        row.S_DELIVERY_DATE = "";
                        if (dateFrom.Year != 1900)
                        {
                            row.S_DELIVERY_DATE = dateFrom.ToString("yyyy-MM-dd");
                        }
                        string sNgayGiao = item.NGAY_GIAO == null ? "" : item.NGAY_GIAO;
                        if (sNgayGiao.Length >= 10)
                        {
                            DateTime dtNgayGiao = Convert.ToDateTime(sNgayGiao);
                            sNgayGiao = Convert.ToDateTime(sNgayGiao).ToString("dd/MM/yyyy");
                            if (!dicNgayGiao.ContainsKey(dtNgayGiao.Date))
                            {
                                dicNgayGiao.Add(dtNgayGiao.Date, sNgayGiao);
                            }
                        }
                        row.S_ID_REPORT = item.SO_BBGIAO == null ? 0 : Convert.ToInt32(item.SO_BBGIAO);
                        row.S_SO_HDON = item.ID_HDON == null ? "" : item.ID_HDON;
                        row.S_CUSTOMER_CODE = item.MA_KHANG == null ? "" : item.MA_KHANG;
                        row.S_CUSTOMER_NAME = item.TEN_KHANG == null ? "" : item.TEN_KHANG;
                        row.S_SERI_ID = item.SO_SERY == null ? "" : item.SO_SERY;
                        row.S_GCS_CODE = item.MA_SOGCS == null ? "" : item.MA_SOGCS;
                        row.S_DELIVERY_DATE = sNgayGiao;
                        row.S_PERIOD_YEAR = item.THANGNAM == null ? "" : item.THANGNAM;
                        row.S_TYPE = item.LOAI_HDON == null ? "" : item.LOAI_HDON;
                        row.N_AMOUNT_SUM = item.SO_TIEN == null ? 0 : Convert.ToDecimal(item.SO_TIEN);
                        row.S_AREA = item.DANHSO == null ? "" : item.DANHSO;
                        row.N_ID = i;
                        reports.Add(row);
                        i += 1;
                    }
                    string strFormDate = "";
                    string strTodate = "";
                    if (MonthDetail != "-1")
                    {
                        DateTime dtsearch = new DateTime(Convert.ToInt16(YearDetail), Convert.ToInt16(MonthDetail), 1);
                        strFormDate = dtsearch.ToString("dd-MM-yyyy");
                        strTodate = dtsearch.AddMonths(1).AddDays(-1).ToString("dd-MM-yyyy");
                    }
                    else
                    {
                        if (dicNgayGiao.Count > 0)
                        {
                            var fromDate = dicNgayGiao.Min(p => p.Key);
                            var toDate = dicNgayGiao.Max(p => p.Key);
                            strFormDate = dicNgayGiao[fromDate];
                            strTodate = dicNgayGiao[toDate];
                        }
                    }
                    var lstMaTNV = reports.Select(p => p.S_EDONG_ACCOUNT).Distinct().ToList();

                    foreach (var item in lstMaTNV)
                    {
                        if (!dicHoaDonTNV.ContainsKey(item))
                        {
                            var listHoaDon = reports.FindAll(p => p.S_EDONG_ACCOUNT == item);
                            dicHoaDonTNV.Add(item, listHoaDon);
                        }
                    }
                    ObjReportDeliveryDetailCommon objParam = new ObjReportDeliveryDetailCommon()
                    {
                        AccountCode = sAccount,
                        AccountName = sAccountName,
                        EmpCode = strEmpCode,
                        EmpName = strEmpName,
                        PCCode = strPc,
                        FromDate = strFormDate,
                        ToDate = strTodate
                    };
                    ePOSSession.AddObject(ePOSSession.DELIVERY_REPORT_DETAIL + date, dicHoaDonTNV);
                    ePOSSession.AddObject(ePOSSession.DELIVERY_COMMON_DETAIL + date, objParam);
                    return Json(new { Result = "SUCCESS", Records = reports, id = ePOSSession.DELIVERY_REPORT_DETAIL + date, obj = ePOSSession.DELIVERY_COMMON_DETAIL + date });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("DetailDeliverySPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DetailDeliverySPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExpSumDelivery(string id, string obj)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjDeliverySummuryReport> listDelivery = (List<ObjDeliverySummuryReport>)ePOSSession.GetObject(id);
                ObjReportDeliverySummuryCommon common = (ObjReportDeliverySummuryCommon)ePOSSession.GetObject(obj);
                var workbook = ePOSReport.SumDelivery(listDelivery, common, posAccount);

                DateTime dt = DateTime.Now;
                string strFileName = "SPC_BaoCaoTongHopGiaoThu" + dt.ToString("yyyyMMdd") + "_" + dt.ToString("HHmmss");
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=" + strFileName + ".xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExpSumDelivery => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExpDetailDelivery(string id, string obj)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                Dictionary<string, List<ObjDeliveryDetailReport>> dicHoaDonTNV = (Dictionary<string, List<ObjDeliveryDetailReport>>)ePOSSession.GetObject(id);
                ObjReportDeliveryDetailCommon common = (ObjReportDeliveryDetailCommon)ePOSSession.GetObject(obj);
                var workbook = ePOSReport.DetailDelivery(dicHoaDonTNV, common, posAccount);
                DateTime dt = DateTime.Now;
                string strFileName = "SPC_BaoCaoChiTietGiaoThu" + dt.ToString("yyyyMMdd") + "_" + dt.ToString("HHmmss");
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=" + strFileName + ".xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExpDetailDelivery => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }
        #endregion

        #region Gach no SPC
        public ActionResult DebtReliefSPC()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.RPT_DEBTRELIEFSPC_TITLE;
            ViewBag.TitleLeft = "Gạch nợ SPC";
            DeliverySPCModel model = new DeliverySPCModel();
            List<SelectListItem> months = new List<SelectListItem>();
            months.Add(new SelectListItem { Value = "-1", Text = "ALL" });
            for (int i = 1; i <= 12; i++)
            {
                if (DateTime.Today.Month == i)
                    months.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = true });
                else
                    months.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            model.MonthList = months;
            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 2000; i <= 2050; i++)
            {
                if (DateTime.Today.Year == i)
                    years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = true });
                else
                    years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
            model.YearList = years;
            List<SelectListItem> pcCodes = new List<SelectListItem>();
            responseEntity entity = ePosDAO.getPCbyId("4", posAccount);
            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO.Count() > 0)
            {
                foreach (var item in entity.listEvnPcBO)
                {
                    pcCodes.Add(new SelectListItem { Value = item.code, Text = item.code + " - " + item.shortName });
                }
            }
            else
                Logging.ReportLogger.ErrorFormat("DeliverySPC => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
            model.PCList = pcCodes;
            return View(model);
        }

        [HttpPost]
        public JsonResult DebtReliefSPCSum(string madviqly = "", int sobbgiao = 0, string manvien = "", string mathungan = "", string ngaygiao = "", int thang = 0, int nam = 0)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjDebtReliefSummuryReport> reports = new List<ObjDebtReliefSummuryReport>();
                if (sobbgiao == 0)
                {
                    return Json(new { Result = "ERROR", Message = "Chưa chọn số biên bản bàn giao. Vui lòng kiểm tra lại" });
                }
                if (string.IsNullOrEmpty(manvien))
                {
                    manvien = "ALL";
                }
                if (string.IsNullOrEmpty(mathungan))
                {
                    mathungan = "ALL";
                }
                if (string.IsNullOrEmpty(ngaygiao))
                {
                    ngaygiao = "ALL";
                }
                string strBranch = string.Empty;
                if (!string.IsNullOrEmpty(madviqly))
                {
                    strBranch = madviqly;
                }
                int i = 0;
                responseEntity entity = ePosDAO.DebtReliefSPCSum(madviqly, sobbgiao.ToString(), manvien, mathungan, ngaygiao, thang.ToString(), nam.ToString(), posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    string date = DateTime.Now.ToString();
                    foreach (var item in entity.listBCTHop_GNMobile)
                    {
                        var row = new ObjDebtReliefSummuryReport();
                        DateTime dateFrom = new DateTime(1900, 01, 01);
                        bool isNgayGiao = DateTime.TryParseExact(item.NGAY_GIAO, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom);

                        row.ID = ++i;
                        row.MA_DVIQLY = item.MA_DVIQLY == null ? string.Empty : item.MA_DVIQLY.Split('-')[0];
                        row.TEN_DVIQLY = item.MA_DVIQLY == null ? string.Empty : item.MA_DVIQLY.Split('-')[1];
                        row.D_NGAY_GIAO = dateFrom;
                        row.SO_BBGIAO = item.SO_BBGIAO;
                        row.TEN_TNGAN = item.TEN_TNGAN;
                        row.NVIEN_GIAO = item.NVIEN_GIAO;
                        row.NGAY_GIAO = item.NGAY_GIAO;
                        row.SO_HDON_GIAO = Convert.ToInt32(item.SO_HDON_GIAO);
                        row.SO_TIEN_GIAO = Convert.ToDecimal(item.SO_TIEN_GIAO);
                        row.SO_THUE_GIAO = Convert.ToDecimal(item.SO_THUE_GIAO);
                        row.SO_TIEN_GIAO_TONG = row.SO_TIEN_GIAO + row.SO_THUE_GIAO;

                        row.SO_HDON_THU_DUOC = Convert.ToInt32(item.SO_HDON_THU_DUOC);
                        row.SO_TIEN_THU_DUOC = Convert.ToDecimal(item.SO_TIEN_THU_DUOC);
                        row.SO_GTGT_THU_DUOC = Convert.ToDecimal(item.SO_GTGT_THU_DUOC);
                        row.SO_TIEN_THU_DUOC_TONG = row.SO_TIEN_THU_DUOC + row.SO_GTGT_THU_DUOC;

                        row.TRANG_THAI = item.TRANG_THAI;

                        row.SO_HDON_TON = row.SO_HDON_GIAO - row.SO_HDON_THU_DUOC;
                        row.SO_TIEN_TON = row.SO_TIEN_GIAO - row.SO_TIEN_THU_DUOC;
                        row.SO_GTGT_TON = row.SO_THUE_GIAO - row.SO_GTGT_THU_DUOC;
                        row.SO_TIEN_TON_TONG = row.SO_TIEN_TON + row.SO_GTGT_TON;

                        row.PC_CODE_COMMON = strBranch;
                        reports.Add(row);
                    }
                    ePOSSession.AddObject(ePOSSession.DEBTRELIEF_REPORT_SUM + date, reports);
                    return Json(new { Result = "SUCCESS", Records = reports, id = ePOSSession.DEBTRELIEF_REPORT_SUM + date });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("DebtReliefSPCSum => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DebtReliefSPCSum => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult DebtReliefSPCDetail(string madviqly = "", string sobbgiao = "", string manvien = "", string mathungan = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (string.IsNullOrEmpty(sobbgiao) || sobbgiao == "0")
                {
                    return Json(new { Result = "ERROR", Message = "Chưa chọn số biên bản bàn giao. Vui lòng kiểm tra lại" });
                }
                Logging.ReportLogger.InfoFormat("DebtReliefSPCDetail => User: {0}, madviqly: {1}, sobbgiao: {2}, manvien: {3}, mathungan: {4}, Session: {5}",
                    posAccount.edong, madviqly, sobbgiao, manvien, mathungan, posAccount.session);
                responseEntity entity = ePosDAO.DebtReliefSPCDetail(madviqly, sobbgiao, manvien, mathungan, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjDebtReliefDetailReport> reports = new List<ObjDebtReliefDetailReport>();
                    string date = DateTime.Now.ToString();
                    string strEmpCode = string.Empty;
                    string strEmpName = string.Empty;
                    string strPc = string.Empty;

                    string _nvien = manvien;
                    string _thungan = mathungan;
                    int i = 1;

                    int TongHDGiao = 0;
                    decimal TongTienGiao = 0;
                    decimal TongTienCham = 0;
                    foreach (var item in entity.listBCCTietGNMobile)
                    {
                        ObjDebtReliefDetailReport row = new ObjDebtReliefDetailReport();
                        row.MA_KHANG = item.MA_KHANG;
                        row.TEN_KHANG = item.TEN_KHANG;
                        row.DIA_CHI = item.DIA_CHI;
                        row.DANH_SO = item.DANH_SO;
                        row.MA_SOGCS = item.MA_SOGCS;
                        row.ID_HDON = item.ID_HDON;
                        row.KYTHANGNAM = item.KYTHANGNAM;
                        try
                        {
                            row.SO_TIEN = item.SO_TIEN == null ? 0 : Convert.ToDecimal(item.SO_TIEN);
                            row.TIEN_GTGT = item.TIEN_GTGT == null ? 0 : Convert.ToDecimal(item.TIEN_GTGT);
                            row.SO_TIEN_CHAM = item.SO_TIEN_CHAM == null ? 0 : Convert.ToDecimal(item.SO_TIEN_CHAM);
                            row.TIEN_GTGT_CHAM = item.TIEN_GTGT_CHAM == null ? 0 : Convert.ToDecimal(item.TIEN_GTGT_CHAM);
                        }
                        catch (Exception ex)
                        {
                            Logging.ReportLogger.ErrorFormat("DebtReliefSPCDetail => User: {0}, Error for Customer: {1}, Error: {2}, Session: {3}", posAccount.edong, item.MA_KHANG, ex.Message, posAccount.session);
                        }
                        row.TRANG_THAI = item.TRANG_THAI;
                        row.DON_VI = item.DON_VI;
                        row.TTIN_BBGIAO = item.SO_BBGIAO;


                        if (!string.IsNullOrEmpty(item.DON_VI))
                        {
                            row.MA_DON_VI = item.DON_VI.Split('-')[0];
                        }


                        if (!string.IsNullOrEmpty(row.TTIN_BBGIAO))
                        {
                            row.SO_BBGIAO = row.TTIN_BBGIAO.Split('-')[0];
                        }

                        row.SO_TIEN_GIAO_VAT = row.SO_TIEN + row.TIEN_GTGT;
                        row.SO_TIEN_CHAM_VAT = row.SO_TIEN_CHAM + row.TIEN_GTGT_CHAM;

                        if (!string.IsNullOrEmpty(_nvien))
                        {
                            string[] arrEmp = _nvien.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrEmp.ToList().Count >= 2)
                            {
                                row.MA_NVIEN_GIAO = arrEmp[0];
                                row.TEN_NVIEN_GIAO = arrEmp[1];
                            }
                        }

                        if (!string.IsNullOrEmpty(_thungan))
                        {
                            string[] arrEmp = _thungan.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrEmp.ToList().Count >= 2)
                            {
                                row.MA_TNGAN = arrEmp[0];
                                row.TEN_TNGAN = arrEmp[1];
                            }
                        }

                        row.NGAY_GIAO = item.SO_BBGIAO.Split('-')[1];
                        DateTime dateFrom = new DateTime(1900, 01, 01);
                        bool isNgayGiao = DateTime.TryParseExact(row.NGAY_GIAO, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom);

                        row.D_NGAY_GIAO = dateFrom;
                        row.ID = i++;

                        TongHDGiao++;
                        TongTienGiao += row.SO_TIEN_GIAO_VAT;
                        TongTienCham += row.SO_TIEN_CHAM_VAT;
                        reports.Add(row);
                    }
                    ePOSSession.AddObject(ePOSSession.DEBT_RELIEF_REPORT_DETAIL + date, reports);
                    return Json(new { Result = "SUCCESS", Records = reports, id = ePOSSession.DEBT_RELIEF_REPORT_DETAIL + date });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("DebtReliefSPCDetail => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
                return null;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DebtReliefSPCDetail => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExpDebtReliefSPCSum(string id)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjDebtReliefSummuryReport> reports = (List<ObjDebtReliefSummuryReport>)ePOSSession.GetObject(id);
                var workbook = ePOSReport.SumDebtReliefSPC(reports, posAccount);
                DateTime dt = DateTime.Now;
                string strFileName = "SPC_BCTongHopGachNo" + dt.ToString("yyyyMMdd") + "_" + dt.ToString("HHmmss");
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=" + strFileName + ".xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExpDebtReliefSPCSum => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ExpDebtReliefSPCDetail(string id)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjDebtReliefDetailReport> reports = (List<ObjDebtReliefDetailReport>)ePOSSession.GetObject(id);

                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.DetailDebtReliefSPC(reports, dir + "Temp_Detail_DebtReliefSPC.xlsx", posAccount);

                // var workbook = ePOSReport.DetailDebtReliefSPC(reports, posAccount);
                DateTime dt = DateTime.Now;
                string strFileName = "SPC_BCChiTietGachNo" + dt.ToString("yyyyMMdd") + "_" + dt.ToString("HHmmss");
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=" + strFileName + ".xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExpDebtReliefSPCDetail => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }
        #endregion

        #region Hạn muc NPC
        public ActionResult WarningAssign()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.RPT_WARNINASSIGN_NPC_TITLE;
            ViewBag.TitleLeft = "Hạn mức";
            ReportPointCollectionModel model = new ReportPointCollectionModel();
            model.PCList = ePosDAO.GetListPC("5", 2, posAccount);
            return View(model);
        }

        [HttpPost]
        public JsonResult WarningAssign_NPC()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.requestHanMucNPC(posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    foreach (var item in entity.thongtin)
                    {
                        ObjReport row = new ObjReport
                        {
                            col_1 = item.maDV,
                            col_2 = item.name,
                            col_3 = decimal.Parse(item.tiencoc).ToString("N0"),
                            col_4 = decimal.Parse(item.truyVan).ToString("N0"),
                            col_5 = decimal.Parse(item.tienChuyen).ToString("N0"),
                            col_6 = item.ngay,
                            col_7 = decimal.Parse(item.tile).ToString("N0")
                        };
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("WarningAssign_NPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
                //responseEntity entity = ePosDAO.reportWarningAssign(posAccount);
                //if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                //{
                //    List<ObjReport> items = new List<ObjReport>();
                //    long total = 0;
                //    foreach (var item in entity.listReportOverLimitNPC)
                //    {
                //        items.Add(new ObjReport
                //        {
                //            col_1 = item.pcName,
                //            col_2 = item.limitMoney.ToString("N0"),
                //            col_3 = item.amcdrMoney.ToString("N0"),
                //            col_4 = item.dotSumMoney.ToString("N0"),
                //            col_5 = item.amountAssign.ToString("N0"),
                //            col_6 = (item.limitMoney - item.amcdrMoney - item.amountAssign - item.dotSumMoney).ToString("N0")
                //        });
                //    }
                //    return Json(new { Result = "SUCCESS", Records = items });

                //}
                //else
                //{
                //    Logging.ReportLogger.ErrorFormat("WarningAssign_NPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                //    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                //}
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("WarningAssign_NPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult WarningAssign_CPC(string pc)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.requestHanMucCPC(pc, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", day = DateTime.Now.ToString("dd/MM/yyyy"), amount_1 = decimal.Parse(entity.hanMuc.SOTIEN).ToString("N0"), amount_2 = decimal.Parse(entity.hanMuc.SO_TIEN1).ToString("N0") });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("WarningAssign_CPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
                return null;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("WarningAssign_CPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        #endregion

        #region Dinh danh the
        public ActionResult CardIdentifier()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ViewBag.Title = Constant.RPT_CARDIDENTIFER_TITLE;
            ViewBag.TitleLeft = "Định danh thẻ";
            CardIdentifierModel model = new CardIdentifierModel();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ReportLogger.ErrorFormat("CardIdentifier => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in (from x in tempPosAcc.EvnPC where !(from a in Constant.EVN() select a.Key).Contains(x.pcId.ToString()) select x))
            {
                items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = items;
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchCardIdentifier(string pc, string customer, string cardcode, int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = null;
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.reportCardNo(pc, customer, cardcode, pagenum, pagesize, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        List<ObjReport> items = new List<ObjReport>();

                        int index = pagenum + 1;
                        string totalRecord = entity.totalBill.ToString();
                        for (int i = 0; i < entity.listCustomer.Count(); i++)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = entity.listCustomer.ElementAt(i).code;
                            row.col_2 = entity.listCustomer.ElementAt(i).cardNo;
                            row.col_3 = entity.listCustomer.ElementAt(i).name;
                            row.col_4 = entity.listCustomer.ElementAt(i).address;
                            row.col_5 = entity.listCustomer.ElementAt(i).phoneByevn;
                            row.col_6 = entity.listCustomer.ElementAt(i).phoneByecp;
                            row.col_7 = entity.listCustomer.ElementAt(i).bookCmis;
                            row.col_8 = index.ToString();
                            row.col_9 = totalRecord;
                            index++;
                            items.Add(row);
                        }

                        int _PageLast = 0;
                        if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_CARD) != null)
                        {
                            _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_CARD));
                        }
                        ePOSSession.AddObject(Constant.PAGE_SIZE_LAST_CARD, pagesize);
                        int countItem = items.Count();
                        int MaxItem = pagesize;
                        if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize/*Không xử lý khi kích cỡ trang hiện tại lớn hơn trang trước đó*/)
                        {
                            for (int i = 0; i < MaxItem; i++)
                            {
                                ObjReport row = new ObjReport();
                                row.col_1 = i.ToString();
                                row.col_2 = i.ToString();
                                row.col_3 = i.ToString();
                                row.col_4 = i.ToString();
                                row.col_5 = i.ToString();
                                row.col_6 = i.ToString();
                                row.col_7 = i.ToString();
                                row.col_8 = i.ToString();
                                row.col_9 = totalRecord;
                                items.Insert(0, row);
                            }
                        }
                        return Json(new { Result = "SUCCESS", Records = items, total_bill = totalRecord });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchCardIdentifier => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchCardIdentifier => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchCardIdentifier => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult CheckSession(string pc, string customer, string cardcode, int pagenum, int pagesize)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.reportCardNo(pc, customer, cardcode, pagenum, pagesize, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    string date = DateTime.Now.ToString();
                    List<ObjReport> items = new List<ObjReport>();
                    for (int i = 0; i < entity.listCustomer.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listCustomer.ElementAt(i).code;
                        row.col_2 = entity.listCustomer.ElementAt(i).cardNo;
                        row.col_3 = entity.listCustomer.ElementAt(i).name;
                        row.col_4 = entity.listCustomer.ElementAt(i).address;
                        row.col_5 = entity.listCustomer.ElementAt(i).phoneByevn;
                        row.col_6 = entity.listCustomer.ElementAt(i).phoneByecp;
                        row.col_7 = entity.listCustomer.ElementAt(i).bookCmis;
                        items.Add(row);
                    }
                    ePOSSession.AddObject(ePOSSession.CARDIDENTIFIER + date, items);
                    return Json(new { Result = "SUCCESS", Message = ePOSSession.CARDIDENTIFIER + date });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("CheckSession => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("CheckSession => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportCardIdentifier(string id)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.CardIdentifier(id, dir + "Temp_CardIdentifier.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=BCdinhdanhthe.xlsx");
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportCardIdentifier => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            return View();
        }
        #endregion

        #region Lich su giao dich

        public ActionResult HistoryTranfer()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.RPT_HISTORYTRANFER_TITLE;
            ViewBag.TitleLeft = "Lịch sử giao dịch";
            return View();
        }

        [HttpPost]
        public JsonResult SearchHistoryCust(string customer)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                Logging.ReportLogger.InfoFormat("SearchHistoryCust => User: {0}, customer: {1}, Session: {2}", posAccount.edong, customer, posAccount.session);
                responseEntity entity = ePosDAO.getHistoryBill(string.Empty, customer.Trim().ToUpper(), string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    if (entity.listBill != null)
                    {
                        foreach (var item in entity.listBill)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = item.billId.ToString();
                            row.col_2 = item.customerCode;
                            row.col_3 = item.name;
                            row.col_4 = item.address;
                            row.col_5 = item.amount.ToString("N0");
                            row.col_6 = item.strTerm;
                            row.col_7 = (from x in Constant.BillStatus() where x.Key == item.status.ToString() select x).FirstOrDefault().Value;
                            items.Add(row);
                        }
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchHistoryCust => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }

            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchHistoryCust => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _DetailHistory(string BillId, string Customer, string Amount, string Period)
        {
            ViewBag.BillId = BillId;
            ViewBag.Customer = Customer;
            ViewBag.Amount = Amount;
            ViewBag.Period = Period;
            return PartialView();
        }

        [HttpPost]
        public JsonResult DetailHistoryCust(string customer, string billID, string amount)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getHistoryBill(billID, customer, amount.Replace(",", "").Replace(".", ""), posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    for (int i = 0; i < entity.listBill.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_5 = i.ToString();
                        row.col_1 = entity.listBill.ElementAt(i).strFromDate;
                        row.col_2 = entity.listBill.ElementAt(i).edong; // thanh toan
                        if (i == 0)
                        {
                            row.col_3 = "";
                            row.col_4 = (from x in Constant.BillingType() where x.Key == entity.listBill.ElementAt(i).billingType select x).FirstOrDefault().Value;
                        }
                        else
                        {
                            row.col_3 = (from x in Constant.BillingType() where x.Key == entity.listBill.ElementAt(i - 1).billingType select x).FirstOrDefault().Value;
                            row.col_4 = (from x in Constant.BillingType() where x.Key == entity.listBill.ElementAt(i).billingType select x).FirstOrDefault().Value;
                        }
                        row.col_5 = entity.listBill.ElementAt(i).cashierPay; // so vi thuc hien         
                        if (entity.listBill.ElementAt(i).status == 2)
                            row.col_6 = entity.listBill.ElementAt(i).billingType.CompareTo("TIMEOUT") == 0 ? "Đang chờ xử lý Time Out" :
                                (from x in Constant.BillStatus() where x.Key == entity.listBill.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                        else
                            row.col_6 = (from x in Constant.BillStatus() where x.Key == entity.listBill.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("DetailHistoryCust => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("DetailHistoryCust => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        #endregion

        #region TDTT
        [AllowAnonymous]
        public ActionResult PrepaidElectricity()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.RPTEVNPREPAID_TITLE;
            ViewBag.TitleLeft = "Tiền điện trả trước";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ReportEDongCashModel model = new ReportEDongCashModel();
            try
            {
                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (tempPosAcc.Childs == null)
                {
                    Logging.ReportLogger.ErrorFormat("PrepaidElectricity => UserName: {0}, Error: Ví quản lý chưa có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
                    return RedirectToAction("Login", "ePOS", true);
                }
                List<SelectListItem> AccAssignList = new List<SelectListItem>();
                foreach (var item in (from x in tempPosAcc.Childs where x.type == "4" select x))
                {
                    AccAssignList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
                model.AccAssignList = AccAssignList;
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("PrepaidElectricity => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchPrepaidElectricity(string ListAcc, string FromDate, string ToDate)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_EDongCash(ListAcc, FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                    Logging.ReportLogger.InfoFormat("SearchPrepaidElectricity => User: {0}, FromDate: {1}, ToDate: {2}, ListAcc: {3}, SessionId: {4}", posAccount.edong, FromDate, ToDate, ListAcc, posAccount.session);
                    string[] array = JsonConvert.DeserializeObject<string[]>(ListAcc);
                    string[] temp_aray = new string[array.Length];
                    for (int i = 0; i < array.Length; i++)
                        temp_aray[i] = array[i].Split('-')[0].Trim();
                    responseEntity entity = ePosDAO.requestReportTDTTGeneral(temp_aray, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        for (int i = 0; i < entity.listReportTDTT.Count(); i++)
                        {
                            long col3 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).firstBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).firstBalance);
                            long col4 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).lastBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).lastBalance);
                            long col5 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountBuy) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountBuy);
                            long col6 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountSale) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountSale);

                            ObjReport row = new ObjReport();
                            row.col_1 = entity.listReportTDTT.ElementAt(i).edong;
                            row.col_2 = entity.listReportTDTT.ElementAt(i).edongName;
                            row.col_3 = col3.ToString("N0");
                            row.col_4 = col4.ToString("N0");
                            row.col_5 = col5.ToString("N0");
                            row.col_6 = col6.ToString("N0");
                            items.Add(row);
                        }
                        ePOSSession.AddObject(Constant.REPORT_PREPAIDEVN + date, items);
                        return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_PREPAIDEVN + date });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchPrepaidElectricity => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchPrepaidElectricity => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchPrepaidElectricity => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchDetailPrepaidElectricity(string edong, string name, string FromDate, string ToDate)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_EDongCash(edong, FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                    Logging.ReportLogger.InfoFormat("SearchDetailPrepaidElectricity => User: {0}, FromDate: {1}, ToDate: {2}, edong: {3}, SessionId: {4}", posAccount.edong, FromDate, ToDate, edong, posAccount.session);

                    string[] array = new string[1];
                    array[0] = edong;
                    responseEntity entity = ePosDAO.requestReportTDTTDetail(array, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        ObjCashDetail detail = new ObjCashDetail();
                        long temp_amount = 0;

                        if (entity.listReportTDTT.Count() == 1)
                        {
                            ObjReport row = new ObjReport();
                            int i = 0;
                            temp_amount = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).firstBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).firstBalance);
                            detail.Amount_old = temp_amount.ToString("N0");
                            detail.Amount_new = temp_amount.ToString("N0");

                            long col2 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).firstBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).firstBalance);
                            long col3 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountBuy) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountBuy);
                            long col4 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountSale) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountSale);

                            row.col_1 = entity.listReportTDTT.ElementAt(i).syntheticDate;
                            row.col_2 = col2.ToString("N0");
                            row.col_3 = col3.ToString("N0");
                            row.col_4 = col4.ToString("N0");
                            items.Add(row);
                        }
                        else
                        {
                            for (int i = 0; i < entity.listReportTDTT.Count(); i++)
                            {
                                ObjReport row = new ObjReport();
                                if (i == 0)
                                {
                                    temp_amount = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).firstBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).firstBalance);
                                    detail.Amount_old = temp_amount.ToString("N0");
                                    detail.Amount_new = "0";

                                    long col2 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).firstBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).firstBalance);
                                    long col3 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountBuy) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountBuy);
                                    long col4 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountSale) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountSale);
                                    row.col_1 = entity.listReportTDTT.ElementAt(i).syntheticDate;
                                    row.col_2 = col2.ToString("N0");
                                    row.col_3 = col3.ToString("N0");
                                    row.col_4 = col4.ToString("N0");
                                    items.Add(row);
                                }
                                else
                                {
                                    long col2 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).firstBalance) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).firstBalance);
                                    long col3 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountBuy) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountBuy);
                                    long col4 = string.IsNullOrEmpty(entity.listReportTDTT.ElementAt(i).amountSale) == true ? 0 : long.Parse(entity.listReportTDTT.ElementAt(i).amountSale);
                                    row.col_1 = entity.listReportTDTT.ElementAt(i).syntheticDate;
                                    row.col_2 = col2.ToString("N0");
                                    row.col_3 = col3.ToString("N0");
                                    row.col_4 = col4.ToString("N0");
                                    if (i == entity.listResponseReportEdong.Count() - 1)
                                    {
                                        detail.Amount_new = col2.ToString("N0");
                                    }
                                    items.Add(row);
                                }
                            }
                        }
                        detail.items = items;
                        ePOSSession.AddObject(Constant.REPORT_PREPAIDEVN_DETAIL + date, detail);
                        return Json(new { Result = "SUCCESS", Records = detail, id = Constant.REPORT_PREPAIDEVN_DETAIL + date, account = name });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchDetailPrepaidElectricity => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchDetailPrepaidElectricity => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchDetailPrepaidElectricity => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportPrepaidElectricity(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.PrepaidElectricity(id, dir + "Temp_SumPrepaidElectricity.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=BCTonghopvivatienmat.xlsx", posAccount.edong));
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportPrepaidElectricity => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult ExportDetailPrepaidElectricity(string id, string account)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));

                var workbook = ePOSReport.PrepaidElectricityDetail(id, dir + "Temp_DetailPrepaidElectricity.xlsx", account, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=BCChitietvivatienmat.xlsx", posAccount.edong));

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportDetailPrepaidElectricity => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        #endregion


        #region Bao cao doanh so
        public ActionResult EdongSales()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.RPTEDONGSALES_TITLE;
            ViewBag.TitleLeft = "Doanh số";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ReportEDongSalesModel model = new ReportEDongSalesModel();
            try
            {

                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (tempPosAcc.Childs == null)
                {
                    Logging.ReportLogger.ErrorFormat("EdongSales => UserName: {0}, Error: Ví quản lý chưa có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
                    return RedirectToAction("Login", "ePOS", true);
                }

                if (posAccount.type != -1 && posAccount.type != 5)
                {
                    List<SelectListItem> WalletList = new List<SelectListItem>();
                    
                    if (posAccount.edong.StartsWith("PD"))
                    {
                        WalletList.Add(new SelectListItem { Value = "PD", Text = "Hà Nội" });
                    }
                    else if (posAccount.edong.StartsWith("PE"))
                    {
                        WalletList.Add(new SelectListItem { Value = "PE", Text = "Hồ Chí Minh" });
                    }
                    else if (posAccount.edong.StartsWith("PA"))
                    {
                        WalletList.Add(new SelectListItem { Value = "PA", Text = "Miền Bắc" });
                    }
                    else if (posAccount.edong.StartsWith("PC"))
                    {
                        WalletList.Add(new SelectListItem { Value = "PC", Text = "Miền Trung" });
                    }
                    else if (posAccount.edong.StartsWith("PB"))
                    {
                        WalletList.Add(new SelectListItem { Value = "PB", Text = "Miền Nam" });
                    }else
                    {
                        WalletList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                    }
                    model.WalletList = WalletList;
                }
                else
                {
                    //chi nhanh KD
                    List<SelectListItem> WalletList = new List<SelectListItem>();
                    WalletList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                    WalletList.Add(new SelectListItem { Value = "PD", Text = "Hà Nội" });
                    WalletList.Add(new SelectListItem { Value = "PE", Text = "Hồ Chí Minh" });
                    WalletList.Add(new SelectListItem { Value = "PA", Text = "Miền Bắc" });
                    WalletList.Add(new SelectListItem { Value = "PC", Text = "Miền Trung" });
                    WalletList.Add(new SelectListItem { Value = "PB", Text = "Miền Nam" });
                    model.WalletList = WalletList;
                }


                //vi quan ly
                List<SelectListItem> ParentList = new List<SelectListItem>();
                ParentList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                if (posAccount.type != -1)
                {
                    ParentList.Add(new SelectListItem { Value = posAccount.edong, Text = posAccount.edong + " - " + posAccount.name });
                    foreach (var item in (from x in tempPosAcc.Childs where x.parent == posAccount.edong select x))
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                else
                {
                    foreach (var item in tempPosAcc.Childs)
                    {
                        ParentList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                    }
                }
                model.AccountList = ParentList;
                //vi con
                List<SelectListItem> AccAssignList = new List<SelectListItem>();
                AccAssignList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                foreach (var item in tempPosAcc.Childs)
                {
                    AccAssignList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
                model.AccAssignList = AccAssignList;

                ViewBag.FromDate = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.ToDate = DateTime.Now.ToString("dd/MM/yyyy");
                ViewBag.FromDateBalance = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
                ViewBag.ToDateBalance = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");

            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ReportEdongSales => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchEdongSales(string wallet = "", string Account = "", string ListAcc = "", string FromDate = "", string ToDate = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_EDongSales(ListAcc, FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    Logging.ReportLogger.InfoFormat("SearchEdongSales => User: {0}, FromDate: {1}, ToDate: {2}, ListAcc: {3}, wallet: {4}, SessionId: {5}", posAccount.edong, FromDate, ToDate, ListAcc, wallet, posAccount.session);
                    //string[] array = string.IsNullOrEmpty(ListAcc) == true ? Account.Split(';') : JsonConvert.DeserializeObject<string[]>(ListAcc);
                    //string[] temp_aray = new string[array.Length];
                    //for (int i = 0; i < array.Length; i++)
                    //    temp_aray[i] = array[i].Split('-')[0].Trim();
                    string parent = Account;
                    string account = ListAcc;
                    responseEntity entity = ePosDAO.GetReportEdongSales(wallet, parent, account, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        ObjReport row = new ObjReport();
                        long sum_col3 = 0; long sum_col4 = 0; long sum_col5 = 0; long sum_col6 = 0; long sum_col7 = 0; long sum_col8 = 0;
                        long sum_col9 = 0; long sum_col10 = 0; long sum_col11 = 0; long sum_col12 = 0; long sum_col13 = 0; long sum_col14 = 0;
                        long sum_col15 = 0; long sum_col16 = 0; long sum_col17 = 0; long sum_col18 = 0; long sum_col19 = 0;
                        for (int i = 0; i < entity.listReportSaleCashier.Count(); i++)
                        {
                            long col3 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransEvn.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransEvn;
                            long col4 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountEvn.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountEvn;
                            long col5 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransRefundEvn.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransRefundEvn;
                            long col6 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountRefundEvn.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountRefundEvn;
                            long col7 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransGateEvn.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransGateEvn;
                            long col8 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountGateEvn.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountGateEvn;
                            long col9 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransVtvCab.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransVtvCab;
                            long col10 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountVtvCab.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountVtvCab;
                            long col11 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransWaco.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransWaco;
                            long col12 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountWaco.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountWaco;
                            long col13 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransTopup.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransTopup;
                            long col14 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountTopup.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountTopup;
                            long col15 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransFinance.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransFinance;
                            long col16 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountFinance.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountFinance;
                            long col17 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalTransRefundVAS.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalTransRefundVAS;
                            long col18 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).totalAmountRefundVAS.ToString()) == true ? 0 : (long)entity.listReportSaleCashier.ElementAt(i).totalAmountRefundVAS;

                            long col19 = (col4 - col6) + (col10 + col12 + col14 + col16 - col18);

                            sum_col3 += col3;
                            sum_col4 += col4;
                            sum_col5 += col5;
                            sum_col6 += col6;
                            sum_col7 += col7;
                            sum_col8 += col8;
                            sum_col9 += col9;
                            sum_col10 += col10;
                            sum_col11 += col11;
                            sum_col12 += col12;
                            sum_col13 += col13;
                            sum_col14 += col14;
                            sum_col15 += col15;
                            sum_col16 += col16;
                            sum_col17 += col17;
                            sum_col18 += col18;
                            sum_col19 += col19;
                            //long col12 = string.IsNullOrEmpty(entity.listReportSaleCashier.ElementAt(i).subTran) == true ? 0 : long.Parse(entity.listReportSaleCashier.ElementAt(i).subTran);
                            row = new ObjReport();
                            row.col_1 = entity.listReportSaleCashier.ElementAt(i).edong;
                            row.col_2 = entity.listReportSaleCashier.ElementAt(i).edongName;
                            row.col_3 = col3.ToString("N0");
                            row.col_4 = col4.ToString("N0");
                            row.col_5 = col5.ToString("N0");
                            row.col_6 = col6.ToString("N0");
                            row.col_7 = col7.ToString("N0");
                            row.col_8 = col8.ToString("N0");
                            row.col_9 = col9.ToString("N0");
                            row.col_10 = col10.ToString("N0");
                            row.col_11 = col11.ToString("N0");
                            row.col_12 = col12.ToString("N0");
                            row.col_13 = col13.ToString("N0");
                            row.col_14 = col14.ToString("N0");
                            row.col_15 = col15.ToString("N0");
                            row.col_16 = col16.ToString("N0");
                            row.col_17 = col17.ToString("N0");
                            row.col_18 = col18.ToString("N0");
                            row.col_19 = col19.ToString("N0");
                            items.Add(row);
                        }
                        if (entity.listReportSaleCashier != null && entity.listReportSaleCashier.Count() > 0)
                        {
                            row = new ObjReport();
                            row.col_1 = "TỔNG CỘNG";
                            row.col_2 = "";
                            row.col_3 = sum_col3.ToString("N0");
                            row.col_4 = sum_col4.ToString("N0");
                            row.col_5 = sum_col5.ToString("N0");
                            row.col_6 = sum_col6.ToString("N0");
                            row.col_7 = sum_col7.ToString("N0");
                            row.col_8 = sum_col8.ToString("N0");
                            row.col_9 = sum_col9.ToString("N0");
                            row.col_10 = sum_col10.ToString("N0");
                            row.col_11 = sum_col11.ToString("N0");
                            row.col_12 = sum_col12.ToString("N0");
                            row.col_13 = sum_col13.ToString("N0");
                            row.col_14 = sum_col14.ToString("N0");
                            row.col_15 = sum_col15.ToString("N0");
                            row.col_16 = sum_col16.ToString("N0");
                            row.col_17 = sum_col17.ToString("N0");
                            row.col_18 = sum_col18.ToString("N0");
                            row.col_19 = sum_col19.ToString("N0");
                            //row.col_1 = @"<strong>TỔNG CỘNG</strong>";
                            //row.col_2 = "";
                            //row.col_3 = @"<strong>" + sum_col3.ToString("N0") + "</strong>";
                            //row.col_4 = @"<strong>" + sum_col4.ToString("N0") + "</strong>";
                            //row.col_5 = @"<strong>" + sum_col5.ToString("N0") + "</strong>";
                            //row.col_6 = @"<strong>" + sum_col6.ToString("N0") + "</strong>";
                            //row.col_7 = @"<strong>" + sum_col7.ToString("N0") + "</strong>";
                            //row.col_8 = @"<strong>" + sum_col8.ToString("N0") + "</strong>";
                            //row.col_9 = @"<strong>" + sum_col9.ToString("N0") + "</strong>";
                            //row.col_10 = @"<strong>" + sum_col10.ToString("N0") + "</strong>";
                            //row.col_11 = @"<strong>" + sum_col11.ToString("N0") + "</strong>";
                            //row.col_12 = @"<strong>" + sum_col12.ToString("N0") + "</strong>";
                            //row.col_13 = @"<strong>" + sum_col13.ToString("N0") + "</strong>";
                            //row.col_14 = @"<strong>" + sum_col14.ToString("N0") + "</strong>";
                            //row.col_15 = @"<strong>" + sum_col15.ToString("N0") + "</strong>";
                            //row.col_16 = @"<strong>" + sum_col16.ToString("N0") + "</strong>";
                            //row.col_17 = @"<strong>" + sum_col17.ToString("N0") + "</strong>";
                            //row.col_18 = @"<strong>" + sum_col18.ToString("N0") + "</strong>";
                            //row.col_19 = @"<strong>" + sum_col19.ToString("N0") + "</strong>";
                            items.Add(row);

                        }
                        ePOSSession.AddObject(Constant.REPORT_SALES + date, items);
                        return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_SALES + date });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchEdongSales => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchEdongSales => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchEdongSales => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        //so du vi
        [HttpPost]
        public JsonResult SearchEdongBalance(string wallet = "", string Account = "", string ListAcc = "", string FromDate = "", string ToDate = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_EDongSales(ListAcc, FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    Logging.ReportLogger.InfoFormat("SearchEdongBalance => User: {0}, FromDate: {1}, ToDate: {2}, ListAcc: {3}, wallet: {4}, SessionId: {5}", posAccount.edong, FromDate, ToDate, ListAcc, wallet, posAccount.session);
                    //string[] array = string.IsNullOrEmpty(ListAcc) == true ? Account.Split(';') : JsonConvert.DeserializeObject<string[]>(ListAcc);
                    //string[] temp_aray = new string[array.Length];
                    //for (int i = 0; i < array.Length; i++)
                    //    temp_aray[i] = array[i].Split('-')[0].Trim();
                    string parent = Account;
                    string account = ListAcc;
                    responseEntity entity = ePosDAO.GetReportEdongBalance(wallet, parent, account, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        ObjReport row = new ObjReport();
                        long sum_col3 = 0; long sum_col4 = 0; long sum_col5 = 0; long sum_col6 = 0; long sum_col7 = 0; long sum_col8 = 0;
                        long sum_col9 = 0; long sum_col10 = 0; long sum_col11 = 0; long sum_col12 = 0;
                        for (int i = 0; i < entity.listBalanceEdongBO.Count(); i++)
                        {
                            //no vi tong dau ky
                            long col3 = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).switchEdongDebtFirst.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).switchEdongDebtFirst;
                            long edongBalanceFirst = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongBalanceFirst.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongBalanceFirst;
                            long edongBalanceLockFirst = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongBalanceLockFirst.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongBalanceLockFirst;
                            long edongBalanceWaitFirst = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongBalanceWaitFirst.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongBalanceWaitFirst;
                            //so du kha dung dau ky
                            long col4 = edongBalanceFirst - edongBalanceLockFirst - edongBalanceWaitFirst;
                            //tien ton dau ky
                            long col5 = col3 - col4;//string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongStockFirst.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongStockFirst;

                            long col6 = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).advanceMoney.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).advanceMoney;
                            long col7 = 0;//string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).refundSwitchEdongECP.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).refundSwitchEdongECP;
                            long col8 = 0;// string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).refundPTI.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).refundPTI;
                            long col9 = 0;// string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).refundVTEL.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).refundVTEL;
                            if (entity.listBalanceEdongBO.ElementAt(i).edongMap != null && entity.listBalanceEdongBO.ElementAt(i).edongMap.Count() > 0)
                            {
                                var listedongMap = entity.listBalanceEdongBO.ElementAt(i).edongMap;
                                for (int j = 0; j < listedongMap.Count(); j++)
                                {
                                    string col_key = string.IsNullOrEmpty(listedongMap.ElementAt(j).key.ToString()) == true ? "" : listedongMap.ElementAt(j).key.ToString();
                                    long col_val = string.IsNullOrEmpty(listedongMap.ElementAt(j).value.ToString()) == true ? 0 : (long)listedongMap.ElementAt(j).value;
                                    if(col_key.ToUpper().Trim() == VI_TONG_ECPAY)
                                    {
                                        col7 += col_val;
                                    }else if (col_key.ToUpper().Trim() == VI_TONG_BAO_HIEM)
                                    {
                                        col8 += col_val;
                                    }else if (col_key.ToUpper().Trim() == VI_TONG_VIETTEL)
                                    {
                                        col9 += col_val;
                                    }
                                }
                            }
                            //hoan vi: chi tru Hoan vi (ECP + PTI + VTEL)
                            //long col10 = (col3 + col6) - (col7 + col8 + col9); //string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).switchEdongDebtLast.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).switchEdongDebtLast;
                            //hoan vi: chi tru Hoan vi (ECP)
                            long col10 = (col3 + col6) - (col7); //string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).switchEdongDebtLast.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).switchEdongDebtLast;
                            long edongBalanceLast = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongBalanceLast.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongBalanceLast;
                            long edongBalanceLockLast = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongBalanceLockLast.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongBalanceLockLast;
                            long edongBalanceWaitLast = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongBalanceWaitLast.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongBalanceWaitLast;
                            //so du kha dung cuoi ky
                            long col11  = edongBalanceLast - edongBalanceLockLast - edongBalanceWaitLast;
                            long col12 = col10 - col11;//string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).edongStockLast.ToString()) == true ? 0 : (long)entity.listBalanceEdongBO.ElementAt(i).edongStockLast;

                            sum_col3 += col3;
                            sum_col4 += col4;
                            sum_col5 += col5;
                            sum_col6 += col6;
                            sum_col7 += col7;
                            sum_col8 += col8;
                            sum_col9 += col9;
                            sum_col10 += col10;
                            sum_col11 += col11;
                            sum_col12 += col12;

                            //long col12 = string.IsNullOrEmpty(entity.listBalanceEdongBO.ElementAt(i).subTran) == true ? 0 : long.Parse(entity.listBalanceEdongBO.ElementAt(i).subTran);
                            row = new ObjReport();
                            row.col_1 = entity.listBalanceEdongBO.ElementAt(i).edong;
                            row.col_2 = entity.listBalanceEdongBO.ElementAt(i).edongName;
                            row.col_3 = col3.ToString("N0");
                            row.col_4 = col4.ToString("N0");
                            row.col_5 = col5.ToString("N0");
                            row.col_6 = col6.ToString("N0");
                            row.col_7 = col7.ToString("N0");
                            row.col_8 = col8.ToString("N0");
                            row.col_9 = col9.ToString("N0");
                            row.col_10 = col10.ToString("N0");
                            row.col_11 = col11.ToString("N0");
                            row.col_12 = col12.ToString("N0");
                            items.Add(row);
                        }
                        if (entity.listBalanceEdongBO != null && entity.listBalanceEdongBO.Count() > 0)
                        {
                            row = new ObjReport();
                            row.col_1 = "TỔNG CỘNG";
                            row.col_2 = "";
                            row.col_3 = sum_col3.ToString("N0");
                            row.col_4 = sum_col4.ToString("N0");
                            row.col_5 = sum_col5.ToString("N0");
                            row.col_6 = sum_col6.ToString("N0");
                            row.col_7 = sum_col7.ToString("N0");
                            row.col_8 = sum_col8.ToString("N0");
                            row.col_9 = sum_col9.ToString("N0");
                            row.col_10 = sum_col10.ToString("N0");
                            row.col_11 = sum_col11.ToString("N0");
                            row.col_12 = sum_col12.ToString("N0");
                            items.Add(row);
                        }
                        ePOSSession.AddObject(Constant.REPORT_BALANCE + date, items);
                        return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_BALANCE + date });
                    }
                    else
                    {
                        Logging.ReportLogger.ErrorFormat("SearchEdongBalance => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ReportLogger.ErrorFormat("SearchEdongBalance => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("SearchEdongBalance => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportEDongSales(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.EdongSales(id, dir + "Temp_EdongSales.xlsx", posAccount);
                //var workbook = ePOSReport.CashWallet(id, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=Doanhso_{0}_{1}.xlsx", posAccount.edong, DateTime.Now.ToString("ddMMyyyy")));
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportEDongSales => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }

            return View();
        }
        [AllowAnonymous]
        public ActionResult ExportEDongBalance(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.EdongBalance(id, dir + "Temp_EdongBalance.xlsx", posAccount);
                //var workbook = ePOSReport.CashWallet(id, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=Bao cao vi {0}_{1}.xlsx", posAccount.edong, DateTime.Now.ToString("ddMMyyyy")));
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                    memoryStream.Close();
                }
                context.Response.End();
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("ExportEDongBalance => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }

            return View();
        }
        #endregion



    }
}
