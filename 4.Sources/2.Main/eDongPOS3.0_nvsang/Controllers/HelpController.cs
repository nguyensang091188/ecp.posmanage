using ePOS3.Entities.RequestObject;
using ePOS3.eStoreWS;
using ePOS3.Models;
using ePOS3.Utils;
using Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ePOS3.Controllers
{
    [Authorize]
    [AllowAnonymous]
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class HelpController : Controller
    {
        //private static string logCashier = Convert.ToString(ConfigurationManager.AppSettings["logCashier"]);
        //private static string logStore_15 = Convert.ToString(ConfigurationManager.AppSettings["logStore"]);
        //private static string logStore_18 = Convert.ToString(ConfigurationManager.AppSettings["logStore"]);
        //private static string logStore_30 = Convert.ToString(ConfigurationManager.AppSettings["logStore"]);

        private static string WEB_PUBLICKEY = Convert.ToString(ConfigurationManager.AppSettings["WEB_PUBLICKEY"]);
        private static string WEB_PRIVATEKEY = Convert.ToString(ConfigurationManager.AppSettings["WEB_PRIVATEKEY"]);

        #region Quan lý PC
        public ActionResult EVNPC()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            //check quyen hotro
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.MANAGERPC_TITLE;
            ViewBag.TitleLeft = "Quản lý PC";
            
            ManagerPCModel model = new ManagerPCModel();
            model.Providers = ePOSController.getEVN();
            model.PCList = new List<SelectListItem>();
            Logging.SupportLogger.InfoFormat("EVNPC model={0}", JsonConvert.SerializeObject(model));
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchEVNPC(string provider = "", string ParentPCCode = "", string Name = "", string Code = "", string Tax = "",
            string Address = "", string phoneNumber = "")
        {
            Logging.SupportLogger.InfoFormat("SearchEVNPC RQ: provider={0}, ParentPCCode={1}, Name={2}, Code={3}, Tax={4}", provider, ParentPCCode, Name, Code, Tax);
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            Logging.SupportLogger.InfoFormat("SearchEVNPC RQ: provider={0}, ParentPCCode={1}, Name={2}, Code={3}, Tax={4}, posAccount={5}", provider, ParentPCCode, Name, Code, Tax, JsonConvert.SerializeObject(posAccount));
            try
            {
                responseEntity entity = new responseEntity();
                if (int.Parse(provider) == Constant.EVN_HANOI || int.Parse(provider) == Constant.EVN_HCM)
                {
                    if (!string.IsNullOrEmpty(Code) || !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Tax) || !string.IsNullOrEmpty(Address) || !string.IsNullOrEmpty(phoneNumber))
                        entity = ePosDAO.GetListPC(string.Empty, Name, Code, Tax, Address, phoneNumber, posAccount);
                    else
                        entity = ePosDAO.getPCbyId(provider, posAccount);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Code) || !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Tax) || !string.IsNullOrEmpty(Address) || !string.IsNullOrEmpty(phoneNumber))
                        entity = ePosDAO.GetListPC(string.Empty, Name, Code, Tax, Address, phoneNumber, posAccount);
                    else
                    {
                        string pcid = string.IsNullOrEmpty(ParentPCCode.Trim()) == true ? provider : ParentPCCode;
                        entity = ePosDAO.getPCbyId(pcid, posAccount);
                    }
                }
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO.Count() > 0)
                {
                    List<ObjEVNPC> items = new List<ObjEVNPC>();
                    foreach (var item in entity.listEvnPcBO)
                    {
                        items.Add(new ObjEVNPC
                        {
                            address = item.address,
                            code = item.code,
                            dateChanged = item.dateChanged.ToString("dd-MM-yyyy"),
                            dateCreated = item.dateCreated.ToString("dd-MM-yyyy"),
                            ext = item.ext,
                            fullName = item.fullName,
                            level = item.level,
                            mailCc = item.mailCc,
                            mailTo = item.mailTo,
                            parentId = item.parentId,
                            pcId = item.pcId,
                            phone1 = item.phone1,
                            phone2 = item.phone2,
                            shortName = item.shortName,
                            status = item.status,
                            tax = item.taxCode,
                            cardEVN = item.cardPrefix,
                            providerCode = item.providerCode
                        });
                    }
                    items = items.OrderBy(p => p.code).ToList();
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchEVNPC => UserName: {0}, SessionId: {1}, Code: {2}, Error: {3}",
                        posAccount.edong, posAccount.session, entity.code, entity.description);
                    return Json(new { Result = "ERROR", Message = entity.description });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchEVNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddEVNPC()
        {
            EVNPCModel model = new EVNPCModel();
            model.Corporations = ePOSController.getEVN();
            List<SelectListItem> items = new List<SelectListItem>();
            model.Status = Constant.STATUS_ONLINE;
            model.PCList = items;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult AddPC(string evn, string parentid, string parentname, string code, string fullname, string shortname,
           string ext, string address, string phone1, string phone2, string mailTo, string mailCc, string status, string tax, string cardEVN, string providerCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string parten = string.Empty;
                int index = 0;
                if (int.Parse(evn) == Constant.EVN_HANOI || int.Parse(evn) == Constant.EVN_HCM)
                    parten = int.Parse(evn) == Constant.EVN_HANOI ? "PD" : "PE";
                else
                    parten = string.IsNullOrEmpty(parentid.Trim()) == true ? string.Empty : parentname.Split('-')[0].Trim();
                string result = Validate.check_MergeEVNPC(parten, code.Trim(), ext.Trim(), fullname.Trim(),
                    shortname.Trim(), mailCc, mailTo, phone1, phone2, address,
                    tax, cardEVN, providerCode, ref index);
                if (string.IsNullOrEmpty(result))
                {
                    string level = string.Empty;
                    string parent = string.Empty;
                    if (int.Parse(evn) == Constant.EVN_HANOI || int.Parse(evn) == Constant.EVN_HCM)
                    {
                        parent = evn;
                        level = Constant.LEVEL_PC_3;
                    }
                    else
                    {
                        parent = string.IsNullOrEmpty(parentid.Trim()) == true ? evn : parentid;
                        level = string.IsNullOrEmpty(parentid.Trim()) == true ? Constant.LEVEL_PC_2 : Constant.LEVEL_PC_3;
                    }
                    responseEntity entity = ePosDAO.AddEditPC(address.Trim(), code.Trim(), ext.Trim(), fullname.Trim(), shortname.Trim(),
                        mailTo, mailCc, level, parent, phone1, phone2, status, tax, cardEVN, providerCode, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        return Json(new { Result = "SUCCESS", Message = "Thành công" });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("AddPC => UserName: {0}, Code: {1}, Error: {2}, sessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = 0 });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("AddPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result, index = index });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("AddPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = 0 });
            }
        }


        public PartialViewResult _EditEVNPC(string id, string index)
        {
            ObjEVNPC data = JsonConvert.DeserializeObject<ObjEVNPC>(id);
            ViewBag.Check = 0;
            ViewBag.Corporations = 0;
            ViewBag.pcId = data.pcId;
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            EVNPCModel model = new EVNPCModel();
            List<SelectListItem> items = new List<SelectListItem>();
            model.Corporations = ePOSController.getEVN();
            if (data.pcId == Constant.EVN_HANOI || data.pcId == Constant.EVN_HCM || data.pcId == Constant.EVN_NPC ||
                data.pcId == Constant.EVN_SPC || data.pcId == Constant.EVN_CPC)
            {
                model.PCList = items;
            }
            else
            {
                if (data.parentId == Constant.EVN_HANOI || data.parentId == Constant.EVN_HCM)
                {
                    model.PCList = items;
                    model.Corporation = data.parentId.ToString();
                }
                else if (data.parentId == Constant.EVN_NPC || data.parentId == Constant.EVN_CPC || data.parentId == Constant.EVN_SPC)
                {
                    model.Corporation = data.parentId.ToString();
                    model.PCList = ePosDAO.GetListPC(data.parentId.ToString(), 1, posAccount);
                    model.PCCode = "";
                }
                else
                {
                    responseEntity entity = ePosDAO.getPCbyId(data.parentId.ToString(), posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) != 0 || entity.listEvnPcBO == null)
                    {
                        Logging.SupportLogger.ErrorFormat("EditEVNPC => UserName: {0}, parentId: {1}, Error: Lỗi kết nối, SessionId: {2}", posAccount.edong, data.parentId, posAccount.session);
                        model.PCList = items;
                        ViewBag.Check = 1;
                        return PartialView(model);
                    }
                    else
                    {
                        evnPcBO ParentBO = entity.listEvnPcBO.ElementAt(0);
                        model.Corporation = ParentBO.parentId.ToString();
                        model.PCList = ePosDAO.GetListPC(ParentBO.parentId.ToString(), 1, posAccount);
                        model.PCCode = data.parentId.ToString();
                    }
                }
            }
            ViewBag.CodePC = data.code;
            ViewBag.FullName = data.fullName;
            ViewBag.ShortName = data.shortName;
            ViewBag.Ext = data.ext;
            ViewBag.Phone1 = data.phone1;
            ViewBag.Phone2 = data.phone2;
            ViewBag.MailTo = data.mailTo;
            ViewBag.MailCC = data.mailCc;
            ViewBag.AddressPC = data.address;
            ViewBag.TaxCode = data.tax;
            ViewBag.ProviderCode = data.providerCode;
            ViewBag.CardEVN = data.cardEVN;
            ViewBag.status = data.status;
            ViewBag.index = index;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult EditPC(int id_edit, string pcId, string evn, string parentid, string parentname, string code, string fullname, string shortname,
            string ext, string address, string phone1, string phone2, string mailTo, string mailCc, string status, string tax, string cardEVN, string providerCode, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            List<ObjEVNPC> items = JsonConvert.DeserializeObject<List<ObjEVNPC>>(datasource);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                int index = 0;
                if (int.Parse(pcId) == Constant.EVN_HANOI || int.Parse(pcId) == Constant.EVN_HCM || int.Parse(pcId) == Constant.EVN_NPC ||
                    int.Parse(pcId) == Constant.EVN_SPC || int.Parse(pcId) == Constant.EVN_CPC)
                {
                    string result = Validate.check_MergeEVNPC(string.Empty, code.Trim(), ext.Trim(), fullname.Trim(), shortname.Trim(),
                        mailCc, mailTo, phone1, phone2, address, tax, cardEVN, providerCode, ref index);
                    if (string.IsNullOrEmpty(result))
                    {
                        responseEntity entity = ePosDAO.AddEditPC(address.Trim(), code.Trim(), ext.Trim(), fullname.Trim(), shortname.Trim(),
                            mailTo, mailCc, Constant.LEVEL_PC_1, "0", phone1, phone2, status, tax, cardEVN, providerCode, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            items.ElementAt(id_edit).address = address;
                            items.ElementAt(id_edit).ext = ext;
                            items.ElementAt(id_edit).fullName = fullname;
                            items.ElementAt(id_edit).shortName = shortname;
                            items.ElementAt(id_edit).mailTo = mailTo;
                            items.ElementAt(id_edit).mailCc = mailCc;
                            items.ElementAt(id_edit).level = long.Parse(Constant.LEVEL_PC_1);
                            //items.ElementAt(id_edit).parentId = 0;
                            items.ElementAt(id_edit).phone1 = phone1;
                            items.ElementAt(id_edit).phone2 = phone2;
                            items.ElementAt(id_edit).status = long.Parse(status);
                            items.ElementAt(id_edit).tax = tax;
                            items.ElementAt(id_edit).cardEVN = cardEVN;
                            items.ElementAt(id_edit).providerCode = providerCode;
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật thông tin PC thành công", Records = items, index = id_edit });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("EditPC => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = 0 });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("EditPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result, index = index });
                    }
                }
                else
                {
                    string parten = string.Empty;
                    if (int.Parse(evn) == Constant.EVN_HANOI || int.Parse(evn) == Constant.EVN_HCM)
                        parten = int.Parse(evn) == Constant.EVN_HANOI ? "PD" : "PE";
                    else
                        parten = string.IsNullOrEmpty(parentid.Trim()) == true ? string.Empty : parentname.Split('-')[0].Trim();
                    string result = Validate.check_MergeEVNPC(parten, code.Trim(), ext.Trim(), fullname.Trim(), shortname.Trim(),
                        mailCc, mailTo, phone1, phone2, address, tax, cardEVN, providerCode, ref index);
                    if (string.IsNullOrEmpty(result))
                    {
                        string level = string.Empty;
                        string parent = string.Empty;
                        if (int.Parse(evn) == Constant.EVN_HANOI || int.Parse(evn) == Constant.EVN_HCM)
                        {
                            parent = evn;
                            level = Constant.LEVEL_PC_3;
                        }
                        else
                        {
                            parent = string.IsNullOrEmpty(parentid.Trim()) == true ? evn : parentid;
                            level = string.IsNullOrEmpty(parentid.Trim()) == true ? Constant.LEVEL_PC_2 : Constant.LEVEL_PC_3;
                        }
                        responseEntity entity = ePosDAO.AddEditPC(address, code, ext, fullname, shortname,
                            mailTo, mailCc, level, parent, phone1, phone2, status, tax, cardEVN, providerCode, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            items.ElementAt(id_edit).address = address;
                            items.ElementAt(id_edit).ext = ext;
                            items.ElementAt(id_edit).fullName = fullname;
                            items.ElementAt(id_edit).shortName = shortname;
                            items.ElementAt(id_edit).mailTo = mailTo;
                            items.ElementAt(id_edit).mailCc = mailCc;
                            items.ElementAt(id_edit).level = long.Parse(Constant.LEVEL_PC_1);
                            //items.ElementAt(id_edit).parentId = 0;
                            items.ElementAt(id_edit).phone1 = phone1;
                            items.ElementAt(id_edit).phone2 = phone2;
                            items.ElementAt(id_edit).status = long.Parse(status);
                            items.ElementAt(id_edit).tax = tax;
                            items.ElementAt(id_edit).cardEVN = cardEVN;
                            items.ElementAt(id_edit).providerCode = providerCode;
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật thông tin PC thành công", Records = items, index = id_edit });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("EditPC => UserName: {0}, Code: {1}, Error: {2}, sessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = 0 });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("EditPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                        return Json(new { Result = "ERROR", Message = result, index = index });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("EditPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = 0 });
            }
        }

        public PartialViewResult _ListAccByPC(string pc)
        {
            ViewBag.pc = pc;
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetEdongMapPC(string pccode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.GetEdongMapPC(pccode, posAccount);
                List<ObjReport> data = new List<ObjReport>();
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listAccount.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listAccount.ElementAt(i).edong;
                        row.col_2 = entity.listAccount.ElementAt(i).name;
                        row.col_3 = entity.listAccount.ElementAt(i).address;
                        row.col_4 = entity.listAccount.ElementAt(i).email;
                        row.col_5 = entity.listAccount.ElementAt(i).status;
                        row.col_6 = i.ToString();
                        data.Add(row);
                    }
                }
                else
                    Logging.SupportLogger.ErrorFormat("GetEdongMapPC => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                return Json(new { Result = "SUCCESS", Records = data });
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("GetEdongMapPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        #endregion

        #region Quan ly dang nhap/dang xuat
        public ActionResult Account()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.HELP_ACCOUNT_TITLE;
            ViewBag.TitleLeft = "Đăng nhập/đăng xuất";
            
            return View();
        }

        [HttpPost]
        public JsonResult SearchAccount(string account)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getAccount(account, posAccount);
                List<ObjReport> rows = new List<ObjReport>();

                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    ObjReport row = new ObjReport();
                    row.col_1 = "1";
                    row.col_2 = entity.responseLoginEdong.account.edong;
                    row.col_3 = entity.responseLoginEdong.account.name;
                    if (string.IsNullOrEmpty(entity.responseLoginEdong.account.strLoginTime))
                    {
                        row.col_4 = "Đăng xuất";
                        row.col_8 = Constant.STATUS_OFFLINE;
                    }
                    else if (string.IsNullOrEmpty(entity.responseLoginEdong.account.strLogoutTime))
                    {
                        row.col_4 = "Đăng nhập";
                        row.col_8 = Constant.STATUS_ONLINE;
                    }
                    else
                    {
                        DateTime dtLogin = DateTime.MinValue;
                        DateTime dtLogout = DateTime.MinValue;
                        if (!DateTime.TryParseExact(entity.responseLoginEdong.account.strLoginTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtLogin))
                        {
                            row.col_4 = "Đăng xuất";
                            row.col_8 = Constant.STATUS_OFFLINE;
                        }
                        if (!DateTime.TryParseExact(entity.responseLoginEdong.account.strLogoutTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dtLogout))
                        {
                            row.col_4 = "Đăng nhập";
                            row.col_8 = Constant.STATUS_ONLINE;
                        }
                        if (dtLogin < dtLogout)
                        {
                            row.col_4 = "Đăng xuất";
                            row.col_8 = Constant.STATUS_OFFLINE;
                        }
                        else
                        {
                            row.col_4 = "Đăng nhập";
                            row.col_8 = Constant.STATUS_ONLINE;
                        }
                    }
                    row.col_5 = entity.responseLoginEdong.account.mac;
                    row.col_6 = entity.responseLoginEdong.account.strLoginTime;
                    row.col_7 = entity.responseLoginEdong.account.strLogoutTime;
                    row.col_9 = entity.responseLoginEdong.account.status;
                    rows.Add(row);

                    return Json(new { Result = "SUCCESS", Records = rows });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchAccount => User: {0}, Account: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, account, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult Lock_Unlock(string id, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                ObjReport data = items.ElementAt(int.Parse(id));
                string status_temp = int.Parse(data.col_9) == 1 ? Constant.STATUS_OFFLINE : Constant.STATUS_ONLINE;
                Logging.SupportLogger.InfoFormat("Lock_Unlock => User: {0}, edong: {1}, status: {2}, Session: {3}", posAccount.edong, data.col_2, status_temp, posAccount.session);
                responseEntity entity = ePosDAO.LockUnlockAcc(data.col_2, status_temp, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.ElementAt(int.Parse(id)).col_9 = status_temp;
                    return Json(new
                    {
                        Result = "SUCCESS",
                        Records = items,
                        index = int.Parse(id),
                        Message = int.Parse(status_temp) == 0 ? "Khóa tài khoản thành " + data.col_2 + " thành công" : "Mở khóa tài khoản " + data.col_2 + " thành công"
                    });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("Lock_Unlock => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("Lock_Unlock => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult Logout(string id, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                ObjReport data = items.ElementAt(int.Parse(id));
                Logging.SupportLogger.InfoFormat("Logout => User: {0}, edong: {1}, Session: {2}", posAccount.edong, data.col_2, posAccount.session);
                responseEntity entity = ePosDAO.LogoutAccount(data.col_2, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.ElementAt(int.Parse(id)).col_4 = "Đăng xuất";
                    items.ElementAt(int.Parse(id)).col_8 = Constant.STATUS_OFFLINE;
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items, index = int.Parse(id) });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("Logout => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("Logout => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        #endregion

        #region Tìm hóa đơn trong kho
        public ActionResult SearchBill()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.HELP_SEARCHBILL_TITLE;
            ViewBag.TitleLeft = "Quản lý hóa đơn";
            
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            SearchBillModel model = new SearchBillModel();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.SupportLogger.ErrorFormat("SearchBill => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.pcList = PCList;
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
            foreach (var item in Constant.BillStatus())
            {
                status.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.statusList = status;

            List<SelectListItem> status_evn = new List<SelectListItem>();
            status_evn.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
            foreach (var item in Constant.BillEVNStatus())
            {
                status_evn.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.statusEVNList = status_evn;

            return View(model);
        }

        [HttpPost]
        public JsonResult SearchBillForStore(string pccode, string customer, string name, string address, string billId, string bookCMIS, string status,
            string from_date, string to_date, string amount_from, string amount_to, string phone, string month)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_SearchBill(customer, name, address, billId, bookCMIS, from_date, to_date, month, amount_from, amount_to);
                if (string.IsNullOrEmpty(result))
                {
                    if (!string.IsNullOrEmpty(customer))
                    {
                        pccode = string.Empty;
                        bookCMIS = string.Empty;
                    }
                    responseEntity entity = new responseEntity();
                    if (string.IsNullOrEmpty(customer))
                        entity = ePosDAO.GetCustomerBill(pccode, customer, name, address, billId, bookCMIS, status,
                            from_date, to_date, amount_from, amount_to, phone, month, posAccount);
                    else
                        entity = ePosDAO.getHistoryBill(string.Empty, customer.Trim().ToUpper(), string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listBill != null)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        for (int i = 0; i < entity.listBill.Count(); i++)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = entity.listBill.ElementAt(i).bookCmis;
                            row.col_2 = entity.listBill.ElementAt(i).customerCode;
                            row.col_3 = entity.listBill.ElementAt(i).name;
                            row.col_4 = entity.listBill.ElementAt(i).address;
                            row.col_5 = entity.listBill.ElementAt(i).phoneByevn;
                            row.col_6 = entity.listBill.ElementAt(i).billId.ToString();
                            row.col_7 = entity.listBill.ElementAt(i).term.ToString("MM/yyyy");
                            if (entity.listBill.ElementAt(i).status == 2)
                                row.col_8 = entity.listBill.ElementAt(i).billingType.CompareTo("TIMEOUT") == 0 ? "Đang chờ xử lý Time Out" :
                                    (from x in Constant.BillStatus() where x.Key == entity.listBill.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                            else
                                row.col_8 = (from x in Constant.BillStatus() where x.Key == entity.listBill.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                            row.col_9 = string.IsNullOrEmpty(entity.listBill.ElementAt(i).nume) == true ? "0" : entity.listBill.ElementAt(i).nume;
                            row.col_10 = entity.listBill.ElementAt(i).amountNotTax.ToString("N0");
                            row.col_11 = entity.listBill.ElementAt(i).amountTax.ToString("N0");
                            row.col_12 = entity.listBill.ElementAt(i).amount.ToString("N0");
                            items.Add(row);
                        }
                        ePOSSession.AddObject(Constant.REPORT_BILL, entity.listBill);
                        ePOSSession.AddObject(Constant.REPORT_BILL + date, items);
                        return Json(new { Result = "SUCCESS", Message = "Tim kiếm thành công", id = Constant.REPORT_BILL + date, Records = items });
                    }
                    else
                    {
                        if (entity.listBill == null)
                        {
                            Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào." });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchBillForEVN(string customer, string status = "0")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = "";// Validate.check_SearchBill(customer, name, address, billId, bookCMIS, from_date, to_date, month, amount_from, amount_to);
                if (string.IsNullOrEmpty(result))
                {

                    responseEntity entity = new responseEntity();
                    if (string.IsNullOrEmpty(customer.Trim()))
                    {
                        return Json(new { Result = "ERROR", Message = "Vui lòng nhập Mã khách hàng hoặc Mã thẻ" });
                    }
                    customer = customer.Trim().ToUpper();
                    string[] lstcustomer;
                    if (customer.Contains('\n'))
                        lstcustomer = customer.Split('\n');
                    else
                        lstcustomer = new[] { customer };

                    if(lstcustomer.Length>100)
                        return Json(new { Result = "ERROR", Message = "Tra cứu tối đa 100 khách hàng" });

                    entity = ePosDAO.GetCustomerBillEVN(lstcustomer, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listCustomer != null)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        string CustomerCode = "";

                        for (int i = 0; i < entity.listCustomer.Count(); i++)
                        {
                            CustomerCode = entity.listCustomer.ElementAt(i).code;
                            if ((string.IsNullOrEmpty(status) || status =="0" || status=="1") && entity.listCustomer.ElementAt(i).listBill != null && entity.listCustomer.ElementAt(i).listBill.Count() > 0)
                            {
                                for (int j = 0; j < entity.listCustomer.ElementAt(i).listBill.Count(); j++)
                                {
                                    ObjReport row = new ObjReport();
                                    row.col_1 = entity.listCustomer.ElementAt(i).bookCmis;
                                    row.col_2 = entity.listCustomer.ElementAt(i).code;
                                    row.col_11 = entity.listCustomer.ElementAt(i).cardNo;
                                    row.col_3 = entity.listCustomer.ElementAt(i).name;
                                    row.col_4 = entity.listCustomer.ElementAt(i).address;
                                    row.col_5 = entity.listCustomer.ElementAt(i).phoneByevn;
                                    row.col_6 = entity.listCustomer.ElementAt(i).phoneByecp;
                                    row.col_7 = entity.listCustomer.ElementAt(i).countBill.ToString();
                                    if (j > 0 && CustomerCode == entity.listCustomer.ElementAt(i).listBill.ElementAt(j).customerCode)
                                    {
                                        row.col_1 = "";
                                        row.col_2 = "";
                                        row.col_11 = "";
                                        row.col_3 = "";
                                        row.col_4 = "";
                                        row.col_5 = "";
                                        row.col_6 = "";

                                    }
                                    row.col_8 = entity.listCustomer.ElementAt(i).countBill > 0 ? "Còn nợ" : "Hết nợ";
                                    row.col_9 = entity.listCustomer.ElementAt(i).listBill.ElementAt(j).strTerm;
                                    row.col_10 = entity.listCustomer.ElementAt(i).listBill.ElementAt(j).amount.ToString("N0");
                                    items.Add(row);
                                }
                                items.OrderBy(x => x.col_9);
                            }
                            else if((string.IsNullOrEmpty(status) || status == "0" || status == "2") && (entity.listCustomer.ElementAt(i).listBill == null || entity.listCustomer.ElementAt(i).listBill.Count() <= 0))
                            {
                                ObjReport row = new ObjReport();
                                row.col_1 = entity.listCustomer.ElementAt(i).bookCmis;
                                row.col_2 = entity.listCustomer.ElementAt(i).code;
                                row.col_11 = entity.listCustomer.ElementAt(i).cardNo;
                                row.col_3 = entity.listCustomer.ElementAt(i).name;
                                row.col_4 = entity.listCustomer.ElementAt(i).address;
                                row.col_5 = entity.listCustomer.ElementAt(i).phoneByevn;
                                row.col_6 = entity.listCustomer.ElementAt(i).phoneByecp;
                                row.col_7 = entity.listCustomer.ElementAt(i).countBill.ToString();
                                row.col_8 = entity.listCustomer.ElementAt(i).countBill > 0 ? "Còn nợ" : "Hết nợ";
                                row.col_9 = "";
                                row.col_10 = "";
                                items.Add(row);
                            }

                        }
                        
                        ePOSSession.AddObject(Constant.REPORT_BILL_EVN, entity.listCustomer);
                        ePOSSession.AddObject(Constant.REPORT_BILL_EVN + date, items);
                        if(items.Count>0)
                        {
                            return Json(new { Result = "SUCCESS", Message = "Tìm kiếm thành công", id = Constant.REPORT_BILL_EVN + date, Records = items });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào." });
                        }
                        
                    }
                    else if (entity.code.CompareTo(Constant.PA_SUCCESS_CODE) == 0 && entity.listCustomer != null)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        string CustomerCode = "";

                        for (int i = 0; i < entity.listCustomer.Count(); i++)
                        {
                            CustomerCode = entity.listCustomer.ElementAt(i).code;
                            if ((string.IsNullOrEmpty(status) || status == "0" || status == "1") && entity.listCustomer.ElementAt(i).listBill != null && entity.listCustomer.ElementAt(i).listBill.Count() > 0)
                            {
                                for (int j = 0; j < entity.listCustomer.ElementAt(i).listBill.Count(); j++)
                                {
                                    ObjReport row = new ObjReport();
                                    row.col_1 = entity.listCustomer.ElementAt(i).bookCmis;
                                    row.col_2 = entity.listCustomer.ElementAt(i).code;
                                    row.col_11 = entity.listCustomer.ElementAt(i).cardNo;
                                    row.col_3 = entity.listCustomer.ElementAt(i).name;
                                    row.col_4 = entity.listCustomer.ElementAt(i).address;
                                    row.col_5 = entity.listCustomer.ElementAt(i).phoneByevn;
                                    row.col_6 = entity.listCustomer.ElementAt(i).phoneByecp;
                                    row.col_7 = entity.listCustomer.ElementAt(i).countBill.ToString();
                                    if (j > 0 && CustomerCode == entity.listCustomer.ElementAt(i).listBill.ElementAt(j).customerCode)
                                    {
                                        row.col_1 = "";
                                        row.col_2 = "";
                                        row.col_11 = "";
                                        row.col_3 = "";
                                        row.col_4 = "";
                                        row.col_5 = "";
                                        row.col_6 = "";

                                    }
                                    row.col_8 = entity.listCustomer.ElementAt(i).countBill > 0 ? "Còn nợ" : "Hết nợ";
                                    row.col_9 = entity.listCustomer.ElementAt(i).listBill.ElementAt(j).strTerm;
                                    row.col_10 = entity.listCustomer.ElementAt(i).listBill.ElementAt(j).amount.ToString("N0");
                                    items.Add(row);
                                }
                                items.OrderBy(x => x.col_9);
                            }
                            else if ((string.IsNullOrEmpty(status) || status == "0" || status == "2") && (entity.listCustomer.ElementAt(i).listBill == null || entity.listCustomer.ElementAt(i).listBill.Count() <= 0))
                            {
                                ObjReport row = new ObjReport();
                                row.col_1 = entity.listCustomer.ElementAt(i).bookCmis;
                                row.col_2 = entity.listCustomer.ElementAt(i).code;
                                row.col_11 = entity.listCustomer.ElementAt(i).cardNo;
                                row.col_3 = entity.listCustomer.ElementAt(i).name;
                                row.col_4 = entity.listCustomer.ElementAt(i).address;
                                row.col_5 = entity.listCustomer.ElementAt(i).phoneByevn;
                                row.col_6 = entity.listCustomer.ElementAt(i).phoneByecp;
                                row.col_7 = entity.listCustomer.ElementAt(i).countBill.ToString();
                                row.col_8 = entity.listCustomer.ElementAt(i).countBill > 0 ? "Còn nợ" : "Hết nợ";
                                row.col_9 = "";
                                row.col_10 = "";
                                items.Add(row);
                            }

                        }
                        ePOSSession.AddObject(Constant.REPORT_BILL_EVN, entity.listCustomer);
                        ePOSSession.AddObject(Constant.REPORT_BILL_EVN + date, items);
                        if (items.Count > 0)
                        {
                            return Json(new { Result = "SUCCESS", Message = "Không hỗ trợ tra cứu khách hàng thuộc miền Bắc", id = Constant.REPORT_BILL_EVN + date, Records = items, isMessage = "1" });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = "Không hỗ trợ tra cứu khách hàng thuộc miền Bắc"});
                        }
                        
                    }
                    else
                    {
                        if (entity.code.CompareTo(Constant.PA_SUCCESS_CODE) == 0 && entity.listCustomer == null)
                            {
                            Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = "Không hỗ trợ tra cứu khách hàng thuộc miền Bắc." });
                        }
                        else if (entity.listCustomer == null)
                        {
                            Logging.SupportLogger.ErrorFormat("SearchBillForStore => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào." });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("SearchBillForEVN => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchBillForEVN => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchBillForEVN => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _DetailSearchBill(int row)
        {
            ViewBag.id = row;
            return PartialView();
        }

        [HttpPost]
        public JsonResult DetailBill(int row)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = new List<ObjReport>();
                if (ePOSSession.GetObject(Constant.REPORT_BILL) != null)
                {
                    bill[] data = (bill[])ePOSSession.GetObject(Constant.REPORT_BILL);
                    bill item = data[row];
                    items.Add(new ObjReport { col_1 = "amount", col_2 = item.amount.ToString("N0") });
                    items.Add(new ObjReport { col_1 = "amountDetails", col_2 = string.IsNullOrEmpty(item.amountDetails) == true ? string.Empty : item.amountDetails });
                    //items.Add(new ObjReport { col_1 = "amountPs", col_2 = item.a })

                    items.Add(new ObjReport { col_1 = "billId", col_2 = item.billId.ToString() });
                    items.Add(new ObjReport { col_1 = "billingType", col_2 = string.IsNullOrEmpty(item.billingType) == true ? string.Empty : item.billingType });
                    items.Add(new ObjReport { col_1 = "billNum", col_2 = string.IsNullOrEmpty(item.billNum) == true ? string.Empty : item.billNum });
                    items.Add(new ObjReport { col_1 = "currency", col_2 = string.IsNullOrEmpty(item.currency) == true ? string.Empty : item.currency });
                    items.Add(new ObjReport { col_1 = "customerAddress", col_2 = string.IsNullOrEmpty(item.address) == true ? string.Empty : item.address });
                    items.Add(new ObjReport { col_1 = "customerCode", col_2 = string.IsNullOrEmpty(item.customerCode) == true ? string.Empty : item.customerCode });
                    items.Add(new ObjReport { col_1 = "customerName", col_2 = string.IsNullOrEmpty(item.name) == true ? string.Empty : item.name });
                    //items.Add(new ObjReport { col_1 = "customerPhone", col_2 = item.billId.ToString() });

                    items.Add(new ObjReport { col_1 = "electricityMeter", col_2 = string.IsNullOrEmpty(item.electricityMeter) == true ? string.Empty : item.electricityMeter });
                    items.Add(new ObjReport { col_1 = "fromDate", col_2 = item.fromDate.ToString("dd/MM/yyyy HH:mm:ss") });
                    items.Add(new ObjReport { col_1 = "home", col_2 = string.IsNullOrEmpty(item.home) == true ? string.Empty : item.home });
                    items.Add(new ObjReport { col_1 = "inning", col_2 = string.IsNullOrEmpty(item.inning) == true ? string.Empty : item.inning });
                    //items.Add(new ObjReport { col_1 = "month", col_2 = item. });
                    //items.Add(new ObjReport { col_1 = "monthHT", col_2 = item. });

                    items.Add(new ObjReport { col_1 = "multiple", col_2 = item.multiple.ToString("N0") });
                    items.Add(new ObjReport { col_1 = "newInds", col_2 = string.IsNullOrEmpty(item.newIndex) == true ? string.Empty : item.newIndex });
                    items.Add(new ObjReport { col_1 = "nume", col_2 = string.IsNullOrEmpty(item.nume) == true ? string.Empty : item.nume });
                    items.Add(new ObjReport { col_1 = "numeDetails", col_2 = string.IsNullOrEmpty(item.numeDetails) == true ? string.Empty : item.numeDetails });
                    items.Add(new ObjReport { col_1 = "oldInds", col_2 = string.IsNullOrEmpty(item.oldIndex) == true ? string.Empty : item.oldIndex });
                    items.Add(new ObjReport { col_1 = "pcCode", col_2 = string.IsNullOrEmpty(item.pcCode) == true ? string.Empty : item.pcCode });
                    //items.Add(new ObjReport { col_1 = "pcName", col_2 = item. });

                    items.Add(new ObjReport { col_1 = "period", col_2 = string.IsNullOrEmpty(item.period) == true ? string.Empty : item.period });
                    items.Add(new ObjReport { col_1 = "priceDetails", col_2 = string.IsNullOrEmpty(item.priceDetails) == true ? string.Empty : item.priceDetails });
                    items.Add(new ObjReport { col_1 = "station", col_2 = string.IsNullOrEmpty(item.station) == true ? string.Empty : item.station });
                    items.Add(new ObjReport { col_1 = "status", col_2 = item.status.ToString("N0") });
                    items.Add(new ObjReport { col_1 = "tax", col_2 = item.tax.ToString("N0") });
                    items.Add(new ObjReport { col_1 = "taxCode", col_2 = string.IsNullOrEmpty(item.taxCode) == true ? string.Empty : item.taxCode });
                    //items.Add(new ObjReport { col_1 = "taxPs", col_2 = item. });

                    items.Add(new ObjReport { col_1 = "term", col_2 = item.term.ToString("MM/yyyy") });
                    items.Add(new ObjReport { col_1 = "toDate", col_2 = item.toDate.ToString("dd/MM/yyyy HH:mm:ss") });
                    items.Add(new ObjReport { col_1 = "trade", col_2 = string.IsNullOrEmpty(item.trade) == true ? string.Empty : item.trade });
                    items.Add(new ObjReport { col_1 = "cashierCode", col_2 = string.IsNullOrEmpty(item.cashierCode) == true ? string.Empty : item.cashierCode });
                    items.Add(new ObjReport { col_1 = "edong", col_2 = string.IsNullOrEmpty(item.edong) == true ? string.Empty : item.edong });

                    items.Add(new ObjReport { col_1 = "createdDate", col_2 = string.IsNullOrEmpty(item.createdDate) == true ? string.Empty : item.createdDate });
                    items.Add(new ObjReport { col_1 = "idChanged", col_2 = item.idChanged.ToString() });
                    items.Add(new ObjReport { col_1 = "dateChanged", col_2 = string.IsNullOrEmpty(item.dateChanged) == true ? string.Empty : item.dateChanged });
                    items.Add(new ObjReport { col_1 = "billingType", col_2 = string.IsNullOrEmpty(item.billingType) == true ? string.Empty : item.billingType });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("DetailBill => User: {0}, Error: Phiên làm việc không tồn tại, Session: {1}", posAccount.edong, posAccount.session);
                }
                return Json(new { Result = "SUCCESS", Records = items });
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("DetailBill => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportBill(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.Bill(id, dir + "Temp_Bill.xlsx", posAccount);
                //var workbook = ePOSReport.Bill(id, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=DanhSachHoaDon.xlsx", posAccount.edong));
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
                Logging.SupportLogger.ErrorFormat("ExportBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult ExportBillEVN(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.BillEVN(id, dir + "Temp_Bill_EVN.xlsx", posAccount);
                //var workbook = ePOSReport.Bill(id, posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", string.Format("attachment;filename=DanhSachKhachHangHoaDon.xlsx", posAccount.edong));
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
                Logging.SupportLogger.ErrorFormat("ExportBillEVN => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }
        #endregion

        #region Thiet lap he thong
        public ActionResult SetParam()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            
            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.HELP_SETPARAM_TITLE;
            ViewBag.TitleLeft = "Thiết lập hệ thống";
            SetParamModel model = new SetParamModel();
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
            foreach (var item in Constant.ParamStatus())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.CorporationList = ePOSController.getEVN();
            model.Corporation = "1";
            try
            {
                List<SelectListItem> PCList = new List<SelectListItem>();
                PCList = ePosDAO.GetListPC(model.Corporation, 3, posAccount);
                model.PCList = PCList;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SetParam => UserName: {0}, SessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return RedirectToAction("Login", "ePOS", true);
            }
            model.StatusList = items;
            model.status = "0";
            model.agent = "0";

            return View(model);
        }

        [HttpPost]
        public JsonResult getParam(string code)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getParams(string.Empty, Constant.ONOFF_CREDIT, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    foreach (var item in entity.listParams)
                    {
                        ObjReport row = new ObjReport();
                        if (string.IsNullOrEmpty(item.valueAlt))
                        {
                            row.col_1 = item.type;
                            row.col_2 = item.value;
                            row.col_3 = string.Empty;
                            row.col_5 = item.value.CompareTo(Constant.STATUS_OFFLINE) == 0 ? "false" : "true";
                        }
                        else
                        {
                            row.col_1 = item.type;
                            row.col_2 = item.value;
                            row.col_3 = item.valueAlt.Trim();
                            row.col_5 = item.value.CompareTo(Constant.STATUS_OFFLINE) == 0 ? "false" : "true";
                        }
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("getParam => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getParam => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditOffCredit(string evn, string value)
        {
            DateTime dt = DateTime.Now.AddMinutes(20);
            ViewBag.date = dt.ToString("dd/MM/yyyy");
            ViewBag.time = dt.ToString("HH:mm:ss");
            ViewBag.evn = evn;
            ViewBag.value = value;
            return PartialView();
        }

        public PartialViewResult _EditOnCredit(string evn, string value)
        {
            ViewBag.evn = evn;
            ViewBag.value = value;
            return PartialView();
        }

        public JsonResult UpdateOnOffCredit(string evn, string value, string date, string time)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = new responseEntity();
                if (value.CompareTo("true") == 0)
                {
                    entity = ePosDAO.MergeParam(evn, Constant.ONOFF_CREDIT, string.Empty, string.Empty, Constant.STATUS_ONLINE, string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string init = ePosDAO.InitSystem(Constant.STATUS_ONLINE, posAccount);
                        if (!string.IsNullOrEmpty(init))
                        {
                            Logging.SupportLogger.InfoFormat("UpdateOnOffCredit => User: {0}, evn: {1}, init: {2}, Session: {3}", posAccount.edong, evn, init, posAccount.session);
                            return Json(new { Result = "SUCCESS", Message = evn + " cập nhật thông tin thành công" });
                        }
                        else
                        {
                            return Json(new { Result = "ERROR", Message = evn + " cập nhật thông tin lỗi." });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("UpdateOnOffCredit => User: {0}, evn: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, evn, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    string datetime = string.IsNullOrEmpty(date) == true ? date + " 00:00:00" : date + " " + time;
                    string check = Validate.check_UpdateOnOffCredit(datetime);
                    if (string.IsNullOrEmpty(check))
                    {
                        entity = ePosDAO.MergeParam(evn, Constant.ONOFF_CREDIT, string.Empty, string.Empty, Constant.STATUS_OFFLINE, datetime, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            string requestid = (from x in Constant.OnOffCredit() where x.Key == evn select x).FirstOrDefault().Value;
                            entity = ePosDAO.UpdateTransactionOff(requestid, string.Empty, string.Empty, "2", string.Empty, string.Empty, datetime, posAccount);

                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                string init = ePosDAO.InitSystem(Constant.STATUS_ONLINE, posAccount);
                                if (!string.IsNullOrEmpty(init))
                                {
                                    Logging.SupportLogger.InfoFormat("UpdateOnOffCredit => User: {0}, evn: {1}, init: {2}, Session: {3}", posAccount.edong, evn, init, posAccount.session);
                                    return Json(new { Result = "SUCCESS", Message = evn + " cập nhật thông tin thành công" });
                                }
                                else
                                {
                                    return Json(new { Result = "ERROR", Message = evn + " cập nhật thông tin lỗi." });
                                }
                            }
                            else
                            {
                                Logging.SupportLogger.ErrorFormat("UpdateOnOffCredit => User: {0}, evn: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, evn, entity.code, entity.description, posAccount.session);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("UpdateOnOffCredit => User: {0}, evn: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, evn, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("UpdateOnOffCredit => User: {0}, evn: {1}, Error: {2}, Session: {3}", posAccount.edong, evn, check, posAccount.session);
                        return Json(new { Result = "ERROR", Message = check });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("UpdateOnOffCredit => UserName: {0}, evn: {1}, Error: {2}, sessionId: {3}", posAccount.edong, evn, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchJobCredit(string code, string name, string desc, string Subjects, string day, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (!string.IsNullOrEmpty(day))
                    foreach (var item in PhoneNumber.ProcessCustomerGroup(day))
                    {
                        if (int.Parse(item) <= 0 || int.Parse(item) > 32)
                            return Json(new { Result = "ERROR", Message = "Ngày chạy không hợp lệ", index = 1 });
                    }
                responseEntity entity = ePosDAO.getListJobConfig(code.Trim(), name.Trim(), desc.Trim(), Subjects.Trim(), status.Trim(), day.Trim(), posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    int i = 1;
                    if (entity.listJobConfig != null)
                        foreach (var item in entity.listJobConfig)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = i++.ToString();
                            row.col_2 = item.code;
                            row.col_3 = item.name;
                            row.col_4 = item.description;
                            row.col_5 = item.dayOfMonth;
                            row.col_6 = item.status.CompareTo(Constant.STATUS_ONLINE) == 0 ? Constant.ONLINE : Constant.OFFLINE;
                            row.col_7 = item.allowFrom;
                            row.col_8 = item.allowTo;
                            row.col_9 = item.repeat;
                            row.col_10 = item.interval;
                            row.col_11 = item.intervalUnit;
                            row.col_12 = item.poolSize;
                            row.col_13 = item.sClass;
                            row.col_14 = item.status;
                            items.Add(row);
                        }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchJobCredit => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = 0 });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchJobCredit => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = 0 });
            }
        }

        [HttpPost]
        public JsonResult doChangeStatus(string index, string datasource, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                string temp_status = status.CompareTo("true") == 0 ? Constant.STATUS_ONLINE : Constant.STATUS_OFFLINE;
                responseEntity entity = ePosDAO.MergeJobConfig(items.ElementAt(int.Parse(index)).col_2, string.Empty, string.Empty, items.ElementAt(int.Parse(index)).col_13,
                    temp_status, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.ElementAt(int.Parse(index)).col_6 = temp_status.CompareTo(Constant.STATUS_ONLINE) == 0 ? Constant.ONLINE : Constant.OFFLINE;
                    items.ElementAt(int.Parse(index)).col_14 = temp_status;
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật trạng thái thành công", Records = items, index = int.Parse(index) });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doChangeStatus => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = int.Parse(index) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doChangeStatus => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = int.Parse(index) });
            }
        }

        public PartialViewResult _AddJob()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            AddJobModel model = new AddJobModel();
            model.Add_JobRepeat = Constant.STATUS_ONLINE;
            model.Add_JobStatus = Constant.STATUS_ONLINE;
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.UnitRepeats())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.UnitRepeatList = items;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult doAddJob(string code, string name, string desc, string subject, string day, string repeat,
            string TimeRepeat, string UnitRepeat, string NumberProcess, string TimeStart, string TimeEnd, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            int i_check = 0;
            try
            {
                string check = Validate.check_Job(code, name, desc, subject, day, ref i_check);
                if (string.IsNullOrEmpty(check))
                {
                    responseEntity entity = ePosDAO.MergeJobConfig(code, name, desc, subject, status, day, TimeStart, TimeEnd, repeat, TimeRepeat, UnitRepeat, NumberProcess, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        return Json(new { Result = "SUCCESS", Message = "Thêm mới tiến trình thành công." });
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doAddJob => User: {0}, Code: {1}, Error: {2}, Session: {2}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = i_check });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doAddJob => User: {0}, Error: {1}, Session: {2}", posAccount.edong, check, posAccount.session);
                    return Json(new { Result = "ERROR", Message = check, index = i_check });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doAddJob => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", index = i_check, Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditJob(string id, string index)
        {
            ObjReport data = JsonConvert.DeserializeObject<ObjReport>(id);
            AddJobModel model = new AddJobModel();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.UnitRepeats())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.UnitRepeatList = items;
            model.Add_JobUnitRepeat = data.col_11;
            ViewBag.Add_JobCode = data.col_2;
            ViewBag.Add_JobName = data.col_3;
            ViewBag.Add_JobDesc = data.col_4;
            ViewBag.Add_JobSubject = data.col_13;
            ViewBag.Add_JobDay = data.col_5;
            ViewBag.Add_JobRepeat = string.IsNullOrEmpty(data.col_9) == true ? 0 : int.Parse(data.col_9);
            ViewBag.Add_JobTimeRepeat = data.col_10;
            ViewBag.Add_JobNumProcess = data.col_12;
            ViewBag.Add_JobTimeStart = data.col_7;
            ViewBag.Add_JobTimeEnd = data.col_8;
            ViewBag.Add_JobStatus = string.IsNullOrEmpty(data.col_14) == true ? 0 : int.Parse(data.col_14);
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult SearchParam(string group, string code, string desc, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getParams(group, code, desc, status, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    int index = 1;
                    List<ObjReport> items = new List<ObjReport>();
                    foreach (var item in entity.listParams)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = index++.ToString();
                        row.col_2 = item.type;
                        row.col_3 = item.code;
                        row.col_4 = item.value;
                        row.col_5 = item.name;
                        row.col_6 = item.valueAlt;
                        row.col_7 = item.status == 0 ? Constant.OFFLINE : Constant.ONLINE;
                        row.col_8 = item.status.ToString();
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchParam => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchJobCredit => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddParam()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult doAddParam(string group, string code, string desc, string value, string value_ext, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            int index = 0;
            try
            {
                string check = Validate.check_Param(group, code, value, ref index);
                if (string.IsNullOrEmpty(check))
                {
                    responseEntity entity = ePosDAO.MergeParam(group, code, desc, status, value, value_ext, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string init = ePosDAO.InitSystem(Constant.STATUS_ONLINE, posAccount);
                        if (!string.IsNullOrEmpty(init))
                            return Json(new { Result = "SUCCESS", Message = "Thêm mới bản ghi thành công" });
                        else
                            return Json(new { Result = "ERROR", Message = "Thêm mới bản ghi không thành công" });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doAddParam => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = index });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doAddParam => User: {0}, Error: {1}, Session: {2}", posAccount.edong, check, posAccount.session);
                    return Json(new { Result = "ERROR", Message = check, index = index });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doAddParam => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", index = index, Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public PartialViewResult _EditParam(string id, string index)
        {
            ObjReport data = JsonConvert.DeserializeObject<ObjReport>(HttpUtility.UrlDecode(id));
            ViewBag.ParamGroup = data.col_2;
            ViewBag.ParamCode = data.col_3;
            ViewBag.ParamValue = data.col_4;
            ViewBag.ParamDesc = data.col_5;
            ViewBag.ParamValueExt = data.col_6;
            ViewBag.ParamStatus = data.col_8;
            ViewBag.index = index;
            return PartialView();
        }

        [HttpPost]
        public JsonResult doEditParam(string index, string datasource, string group, string code, string desc, string value, string value_ext, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            int i_check = 0;
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                string check = Validate.check_Param(group, code, value, ref i_check);
                if (string.IsNullOrEmpty(check))
                {
                    responseEntity entity = ePosDAO.MergeParam(group, code, desc, status, value, value_ext, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string init = ePosDAO.InitSystem(Constant.STATUS_ONLINE, posAccount);
                        if (!string.IsNullOrEmpty(init))
                        {
                            items.ElementAt(int.Parse(index)).col_2 = group;
                            items.ElementAt(int.Parse(index)).col_3 = code;
                            items.ElementAt(int.Parse(index)).col_4 = value;
                            items.ElementAt(int.Parse(index)).col_5 = desc;
                            items.ElementAt(int.Parse(index)).col_6 = value_ext;
                            items.ElementAt(int.Parse(index)).col_7 = int.Parse(status) == 0 ? Constant.OFFLINE : Constant.ONLINE;
                            items.ElementAt(int.Parse(index)).col_8 = status;
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", index = index, Records = items });
                        }
                        else
                            return Json(new { Result = "ERROR", Message = "Cập nhật bản ghi không thành công", index = index });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doEditParam => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = index });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doEditParam => User: {0}, Error: {1}, Session: {2}", posAccount.edong, check, posAccount.session);
                    return Json(new { Result = "ERROR", Message = check, index = index });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doEditParam => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", index = index, Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult AddAgent_API(string name, string desc)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.AddAgent_API(name, desc, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", Message = "Thêm mới bản ghi thành công" });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("AddAgent_API => Username: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("AddAgent_API => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult getAgent()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getAgent(posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<TreeView> items = new List<TreeView>();
                    foreach (var item in entity.listAgent)
                    {
                        TreeView tree = new TreeView();
                        tree.id = item.id;
                        tree.parentid = 0;
                        tree.value = item.id + "";
                        tree.text = item.code;
                        items.Add(tree);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("getAgent => UserName: {0}, sessionId: {1}, Error: {2}, Code: {3}", posAccount.edong, posAccount.session, entity.description, entity.code);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getAgent => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult getAPIByAgent(string agent)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getRole(agent, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    for (int i = 0; i < entity.listRole.Length; i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = (i + 1).ToString();
                        row.col_2 = entity.listRole.ElementAt(i).agentId.ToString();
                        row.col_3 = entity.listRole.ElementAt(i).serviceId.ToString();
                        row.col_4 = entity.listRole.ElementAt(i).nameService;
                        row.col_5 = entity.listRole.ElementAt(i).status.ToString();
                        row.col_6 = entity.listRole.ElementAt(i).codeService;
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("getAPIByAgent => UserName: {0}, sessionId: {1}, Error: {2}, Code: {3}", posAccount.edong, posAccount.session, entity.description, entity.code);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getAPIByAgent => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult doChangeAPIforAgent(string index, string datasource, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                string temp_status = status.CompareTo("true") == 0 ? Constant.STATUS_OFFLINE : Constant.STATUS_ONLINE;
                responseEntity entity = ePosDAO.UpdateRole(items.ElementAt(int.Parse(index)).col_2, items.ElementAt(int.Parse(index)).col_3, temp_status, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    string init = ePosDAO.InitSystem(Constant.STATUS_ONLINE, posAccount);
                    if (!string.IsNullOrEmpty(init))
                    {
                        items.ElementAt(int.Parse(index)).col_5 = temp_status;
                        return Json(new { Result = "SUCCESS", Message = "Cập nhật trạng thái thành công", Records = items, index = int.Parse(index) });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doChangeAPIforAgent => UserName: {0}, Error: Cập nhật trạng thái không thành công, SessionId: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Cập nhật trạng thái không thành công", index = int.Parse(index) });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doChangeAPIforAgent => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = int.Parse(index) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doChangeAPIforAgent => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = int.Parse(index) });
            }
        }

        #endregion

        #region Dịch vu tài chính
        [AllowAnonymous]
        public ActionResult FinancialServices()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);
            

            ViewBag.Title = Constant.HELP_SETFINANCIAL_TITLE;
            ViewBag.TitleLeft = "Thiết lập QLDV Tài chính";

            SearchAPIModel model = new SearchAPIModel();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.APIMethod("1"))
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.APIMethodList = items;

            return View(model);
        }

        [HttpPost]
        public JsonResult SearchAPI(string name = "", string method = "", string url = "", int status = 0, int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ObjAPI objAPI = new ObjAPI
                {
                    name = name.Trim(),
                    method = method.Trim(),
                    url = url.Trim(),
                    status = status
                };
                ObjResponse objResponse = ePosRestFulDAO.getAPIbyPage(objAPI, pagenum, pagesize, "DESC", string.Empty, Server.MapPath(WEB_PUBLICKEY), posAccount);
                if (objResponse.code == Constant.SUCCESS_CODE)
                {
                    ObjPageAPIResponse objPageAPIResponse = objResponse.objPageAPI;
                    List<ObjReport> data = new List<ObjReport>();
                    foreach (var item in objPageAPIResponse.content)
                    {
                        ObjReport row = new ObjReport();
                        row.col_0 = objPageAPIResponse.totalElements.ToString();
                        row.col_1 = item.id.ToString();
                        row.col_2 = item.name;
                        row.col_3 = item.method;
                        row.col_4 = item.url;
                        row.col_5 = item.status.ToString();
                        row.col_6 = item.updatedAt;
                        row.col_7 = item.createdAt;
                        data.Add(row);
                    }
                    if (data != null && data.Count > 0)
                    {
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = data,
                            total = objPageAPIResponse.totalElements
                        });

                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("SearchAPI => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, "");
                        return Json(new { Result = "ERROR", Message = string.Format("{0}", "Không tìm thấy dữ liệu") });
                    }

                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchAPI => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                    return Json(new { Result = "ERROR", Message = objResponse.msg });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchAPI => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddAPI()
        {
            AddAPIModel model = new AddAPIModel();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.APIMethod())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.APIMethodList = items;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult doAdd_API(string name, string method, string url, int status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string check = Validate.checkAPI(name, method, url);
                if (string.IsNullOrEmpty(check))
                {
                    ObjAPI objAPI = new ObjAPI { method = method, name = name, url = url, status = status };
                    ObjResponse objResponse = ePosRestFulDAO.createAPI(objAPI, Server.MapPath(WEB_PUBLICKEY), posAccount);
                    if (objResponse.code == Constant.SUCCESS_CODE)
                    {
                        objResponse = ePosRestFulDAO.Init(Server.MapPath(WEB_PUBLICKEY), posAccount);
                        if (objResponse.code == Constant.SUCCESS_CODE)
                        {
                            return Json(new { Result = "SUCCESS", Message = "Thêm mới bản ghi thành công" });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                            return Json(new { Result = "ERROR", Message = objResponse.msg });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                        return Json(new { Result = "ERROR", Message = objResponse.msg });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Error: {1}, Session: {2}", posAccount.edong, check, posAccount.session);
                    return Json(new { Result = "ERROR", Message = check });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doAdd_API => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditAPI(string id, int index)
        {
            ObjReport data = JsonConvert.DeserializeObject<ObjReport>(id);
            ViewBag.name = data.col_2;
            ViewBag.method = data.col_3;
            ViewBag.url = data.col_4;
            ViewBag.status = data.col_5;
            ViewBag.id = data.col_1;
            ViewBag.index = index;

            AddAPIModel model = new AddAPIModel();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.APIMethod())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.APIMethodList = items;
            model.Add_APIMethod = data.col_3;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult doEit_API(int id, string name, string method, string url, int status, int index, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string check = Validate.checkAPI(name, method, url);
                if (string.IsNullOrEmpty(check))
                {
                    List<ObjReport> data = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                    ObjAPI objAPI = new ObjAPI { method = method, name = name, url = url, status = status, id = id };
                    ObjResponse objResponse = ePosRestFulDAO.updateAPI(objAPI, Server.MapPath(WEB_PUBLICKEY), posAccount);
                    if (objResponse.code == Constant.SUCCESS_CODE)
                    {
                        objResponse = ePosRestFulDAO.Init(Server.MapPath(WEB_PUBLICKEY), posAccount);
                        if (objResponse.code == Constant.SUCCESS_CODE)
                        {
                            data.ElementAt(index).col_2 = name;
                            data.ElementAt(index).col_3 = method;
                            data.ElementAt(index).col_4 = url;
                            data.ElementAt(index).col_5 = status.ToString();
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = data });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                            return Json(new { Result = "ERROR", Message = objResponse.msg });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                        return Json(new { Result = "ERROR", Message = objResponse.msg });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doEit_API => User: {0}, Error: {1}, Session: {2}", posAccount.edong, check, posAccount.session);
                    return Json(new { Result = "ERROR", Message = check });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doEit_API => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult doDel_API(int index, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> data = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                ObjAPI objAPI = new ObjAPI { status = 0, id = int.Parse(data.ElementAt(index).col_1) };
                ObjResponse objResponse = ePosRestFulDAO.updateAPI(objAPI, Server.MapPath(WEB_PUBLICKEY), posAccount);
                if (objResponse.code == Constant.SUCCESS_CODE)
                {
                    objResponse = ePosRestFulDAO.Init(Server.MapPath(WEB_PUBLICKEY), posAccount);
                    if (objResponse.code == Constant.SUCCESS_CODE)
                    {
                        data.RemoveAt(index);
                        return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = data });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                        return Json(new { Result = "ERROR", Message = objResponse.msg });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doAdd_API => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                    return Json(new { Result = "ERROR", Message = objResponse.msg });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doDel_API => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult getAgentFinancial()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ObjResponse objResponse = ePosRestFulDAO.getAllAgent(Server.MapPath(WEB_PUBLICKEY), posAccount);
                if (objResponse.code == Constant.SUCCESS_CODE)
                {
                    List<TreeView> items = new List<TreeView>();
                    foreach (var item in objResponse.objAgents)
                    {
                        TreeView tree = new TreeView();
                        tree.id = item.id;
                        tree.parentid = 0;
                        tree.value = item.id + "";
                        tree.text = item.code;
                        items.Add(tree);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("getAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                    return Json(new { Result = "ERROR", Message = objResponse.msg });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getAgentFinancial => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult getAPIByAgentFinancial(int agent)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ObjResponse objResponse = ePosRestFulDAO.getAllRole(Server.MapPath(WEB_PUBLICKEY), posAccount);
                if (objResponse.code == Constant.SUCCESS_CODE)
                {
                    var data = (from x in objResponse.objRoles where x.agentId == agent select x).ToList();
                    objResponse = new ObjResponse();
                    objResponse = ePosRestFulDAO.getAllAPI(Server.MapPath(WEB_PUBLICKEY), posAccount);
                    if (objResponse.code == Constant.SUCCESS_CODE)
                    {
                        var data_api = (from x in objResponse.objAPIs where x.status == Constant.STATUS_1 select x).ToList();
                        List<ObjReport> dataAPI = new List<ObjReport>();
                        foreach (var item in data_api)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = item.id.ToString();//set apiId
                            row.col_2 = item.name;
                            row.col_3 = item.url;
                            row.col_8 = item.method;

                            if (data.Any(x => x.apiId == item.id))
                            {
                                var check = (from x in data where x.apiId == item.id select x).FirstOrDefault();
                                row.col_4 = check.status.ToString();
                                row.col_5 = Constant.STATUS_ONLINE;
                                row.col_7 = check.id.ToString();//exist id api-role =>update roles
                            }
                            else
                            {
                                row.col_4 = Constant.STATUS_OFFLINE;
                                row.col_5 = Constant.STATUS_OFFLINE;
                                row.col_7 = "0";//not exist id api-role => add roles
                            }
                            row.col_6 = agent.ToString();

                            dataAPI.Add(row);
                        }
                        return Json(new { Result = "SUCCESS", Records = dataAPI });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("getAPIByAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                        return Json(new { Result = "ERROR", Message = objResponse.msg });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("getAPIByAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                    return Json(new { Result = "ERROR", Message = objResponse.msg });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("getAPIByAgentFinancial => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult doChangeAPIforAgentFinancial(int index, string datasource, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                ObjReport item = items.ElementAt(index);
                ObjRole data = new ObjRole
                {
                    apiId = int.Parse(item.col_1),
                    agentId = int.Parse(item.col_6),
                    status = status.CompareTo("true") == 0 ? Constant.STATUS_0 : Constant.STATUS_1
                };
                if (item.col_7 == "0")
                {
                    ObjResponse objResponse = ePosRestFulDAO.createRole(data, Server.MapPath(WEB_PUBLICKEY), posAccount);
                    if (objResponse.code == Constant.SUCCESS_CODE)
                    {
                        objResponse = ePosRestFulDAO.Init(Server.MapPath(WEB_PUBLICKEY), posAccount);
                        if (objResponse.code == Constant.SUCCESS_CODE)
                        {
                            items.ElementAt(index).col_4 = status.CompareTo("true") == 0 ? Constant.STATUS_OFFLINE : Constant.STATUS_ONLINE;
                            items.ElementAt(index).col_5 = Constant.STATUS_ONLINE;

                            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items, index = index });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("doChangeAPIforAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                            return Json(new { Result = "ERROR", Message = objResponse.msg });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doChangeAPIforAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                        return Json(new { Result = "ERROR", Message = objResponse.msg });
                    }
                }
                else
                {
                    data.id = int.Parse(item.col_7);
                    ObjResponse objResponse = ePosRestFulDAO.updateRole(data, Server.MapPath(WEB_PUBLICKEY), posAccount);
                    if (objResponse.code == Constant.SUCCESS_CODE)
                    {
                        objResponse = ePosRestFulDAO.Init(Server.MapPath(WEB_PUBLICKEY), posAccount);
                        if (objResponse.code == Constant.SUCCESS_CODE)
                        {
                            items.ElementAt(index).col_4 = status.CompareTo("true") == 0 ? Constant.STATUS_OFFLINE : Constant.STATUS_ONLINE;
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items, index = index });
                        }
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("doChangeAPIforAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                            return Json(new { Result = "ERROR", Message = objResponse.msg });
                        }
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doChangeAPIforAgentFinancial => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                        return Json(new { Result = "ERROR", Message = objResponse.msg });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doChangeAPIforAgentFinancial => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = index });
            }
        }

        #endregion

        #region Kiem soat cham no
        public ActionResult ControlDebt()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.CONTROLDEBT_TITLE;
            ViewBag.TitleLeft = "Kiểm soát chấm nợ";
            ControlDebtModel model = new ControlDebtModel();
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = "", Text = Constant.ALL, Selected = true });
            foreach (var item in Constant.ParamStatus())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            if (tempPosAcc.EvnPC == null)
            {
                Logging.SupportLogger.ErrorFormat("ControlDebtNPC => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            PCList.Add(new SelectListItem { Value = "", Text = "--Chọn điện lực--" });
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = PCList;
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchControlDebt(string pccode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (ePOSSession.GetObject(posAccount.session) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            try
            {
                responseEntity entity = ePosDAO.GetEvnpcBlocking(pccode, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 || entity.listEvnpcBlockingBO != null)
                {
                    ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                    List<ObjReport> items = new List<ObjReport>();
                    foreach (var item in entity.listEvnpcBlockingBO)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = item.pcCode;
                        try
                        {
                            row.col_2 = (from i in tempPosAcc.EvnPC where i.ext == item.pcCode select i).FirstOrDefault().shortName;
                        }
                        catch (Exception ex)
                        {
                            Logging.SupportLogger.ErrorFormat("SearchControlDebt => UserName: {0}, sessionId: {1}, Error: {2}, pcCode: {3}", posAccount.edong, posAccount.session, ex.Message, item.pcCode);
                            row.col_2 = "";
                        }
                        row.col_3 = item.strDateFrom;
                        row.col_4 = item.strDateTo;
                        row.col_5 = item.strDateWork;
                        row.col_6 = item.rowid;
                        row.col_7 = item.evnpcId.ToString();
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchControlDebt => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchControlDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddControlDebt()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ControlDebtModel model = new ControlDebtModel();
            //List<SelectListItem> items = new List<SelectListItem>();
            //foreach (var item in (from x in tempPosAcc.EvnPC where x.ext.StartsWith("PA") || x.ext.StartsWith("PH") orderby x.ext select x).ToList())
            //{
            //    items.Add(new SelectListItem { Value = item.ext, Text = item.ext + "-" + item.shortName });
            //}
            //model.PCList = items;           
            model.CorporationList = ePOSController.getEVN();
            model.Corporation = "1";
            model.PCList = ePosDAO.GetListPC("1", 3, posAccount);
            DateTime dt = DateTime.Now;
            string[] temp = dt.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
            ViewBag.day1 = ViewBag.day2 = temp[0];
            ViewBag.time1 = temp[1];
            ViewBag.time2 = "23:59:59";
            ViewBag.day3 = dt.AddDays(1).ToString("dd/MM/yyyy");
            ViewBag.time3 = "01:00:00";
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult AddDebt(string pccode, string date_start, string time_start, string date_end, string time_end, string date_slow, string time_slow)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string FromDate = string.IsNullOrEmpty(time_start) == true ? date_start + " 00:00:00" : date_start + " " + time_start;
                string ToDate = string.IsNullOrEmpty(time_end) == true ? date_end + " 00:00:00" : date_end + " " + time_end;
                string WorkDate = string.IsNullOrEmpty(time_slow) == true ? date_slow + " 00:00:00" : date_slow + " " + time_slow;
                string result = Validate.check_MergeEvnpcBlocking(FromDate, ToDate, WorkDate);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.MergeEvnpcBlocking(string.Empty, pccode, FromDate, ToDate, WorkDate, string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        return Json(new { Result = "SUCCESS", Message = "Thêm mới bản ghi thành công" });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("AddDebt => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("AddDebt => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("AddDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditControlDebt(string id, string index)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));

            ObjReport data = JsonConvert.DeserializeObject<ObjReport>(HttpUtility.UrlDecode(id));
            ViewBag.pcCode = data.col_1;
            ViewBag.pcName = data.col_2;
            ViewBag.day1 = data.col_3;
            ViewBag.day2 = data.col_4;
            if (string.IsNullOrEmpty(data.col_5))
            {
                data.col_5 = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy") + " 01:00:00";
            }
            ViewBag.day3 = data.col_5;
            ViewBag.index = index;
            return PartialView();
        }

        [HttpPost]
        public JsonResult EditDebt(string index, string pccode, string date_start, string time_start, string date_end, string time_end, string date_slow, string time_slow, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                string FromDate = string.IsNullOrEmpty(time_start) == true ? date_start + " 00:00:00" : date_start + " " + time_start;
                string ToDate = string.IsNullOrEmpty(time_end) == true ? date_end + " 00:00:00" : date_end + " " + time_end;
                string WorkDate = string.IsNullOrEmpty(time_slow) == true ? date_slow + " 00:00:00" : date_slow + " " + time_slow;
                string result = Validate.check_MergeEvnpcBlocking(FromDate, ToDate, WorkDate);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.MergeEvnpcBlocking(items.ElementAt(int.Parse(index)).col_6, pccode, FromDate, ToDate, WorkDate, string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        items.ElementAt(int.Parse(index)).col_3 = FromDate;
                        items.ElementAt(int.Parse(index)).col_4 = ToDate;
                        items.ElementAt(int.Parse(index)).col_5 = WorkDate;
                        return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items, index = index });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("EditDebt => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("EditDebt => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("EditDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult DelAllDebt(string bill, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                List<ObjReport> temps = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                int i_success = 0;
                int i_error = 0;
                foreach (string i in bill.Substring(1, bill.Length - 2).Split(','))
                {
                    ObjReport item = items.ElementAt(int.Parse(i));
                    Logging.SupportLogger.InfoFormat("DelAllDebt => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, i, item.col_6, posAccount.session);
                    responseEntity entity = ePosDAO.MergeEvnpcBlocking(item.col_6, item.col_1, item.col_3, item.col_4, item.col_5, "DELETE", posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        foreach (var x in temps)
                        {
                            if (x.col_6 == item.col_6)
                            {
                                temps.Remove(x);
                                break;
                            }
                        }
                        i_success++;
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("DelAllDebt => User: {0}, Item: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, i, entity.code, entity.description, posAccount.session);
                        i_error++;
                    }
                }
                return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết xóa bản ghi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error), Records = temps });
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("DelAllDebt => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult DelDebt(string id, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                ObjReport item = items.ElementAt(int.Parse(id));
                responseEntity entity = ePosDAO.MergeEvnpcBlocking(item.col_6, item.col_1, item.col_3, item.col_4, item.col_5, "DELETE", posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.RemoveAt(int.Parse(id));
                    return Json(new { Result = "SUCCESS", Message = "Xóa bản ghi thành công", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("DelDebtNPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("DelDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UploadFileDebt()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string uploadDir = Constant.UPLOAD_FOLDER + @"\" + posAccount.edong + @"\" + DateTime.Today.ToString("yyyyMMdd");
                if (!Directory.Exists(Server.MapPath(uploadDir)))
                    Directory.CreateDirectory(Server.MapPath(uploadDir));
                var pic = System.Web.HttpContext.Current.Request.Files["MyFile"];
                if (pic.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(pic.FileName);
                    var _ext = Path.GetExtension(pic.FileName);
                    string fileLocation = Server.MapPath(uploadDir + @"\" + fileName);
                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);
                    pic.SaveAs(fileLocation);
                    DataSet dsTemp = new DataSet();
                    using (var stream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        IExcelDataReader reader = null;
                        if (_ext == ".xls")
                        {
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);

                        }
                        else if (_ext == ".xlsx")
                        {
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        dsTemp = reader.AsDataSet();
                    }
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_CONTROLDEBT, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_CONTROLDEBT });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("UploadFileDebt => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("UploadFileDebt => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InsertFileContrlDebt(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (ePOSSession.GetObject(id) != null)
                {
                    DataSet ds = (DataSet)ePOSSession.GetObject(id);
                    List<ObjReport> rows = new List<ObjReport>();
                    for (int i = 0; i < ds.Tables[0].AsEnumerable().Skip(1).Count(); i++)
                    {
                        Logging.SupportLogger.InfoFormat("InsertFileContrlDebt => User: {0}, index: {1}, Session: {2}", posAccount.edong, i, posAccount.session);
                        bool flag = true;
                        ObjReport row = new ObjReport();
                        row.col_1 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[0].ToString().Trim();//pccode
                        row.col_2 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[1].ToString().Trim();//pcname
                        row.col_3 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString().Trim();// ngay bat dau
                        row.col_4 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[3].ToString().Trim();//ngay ket thuc                      
                        row.col_5 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[4].ToString().Trim();//ngay cham

                        if (string.IsNullOrEmpty(row.col_1))
                        {
                            row.col_6 = "Mã công ty điện lực để trống";
                            flag = false;
                        }
                        else              //!row.col_1 .StartsWith("PA") || !row.col_1.StartsWith("PH"))
                        {
                            if (row.col_1.StartsWith("PA") || row.col_1.StartsWith("PH"))
                                flag = true;
                            else
                            {
                                row.col_6 = "Mã công ty điện lực không đúng định dạng";
                                flag = false;
                            }
                        }
                        if (string.IsNullOrEmpty(row.col_2))
                        {
                            row.col_6 = "Tên công ty điện lực để trống";
                            flag = false;
                        }
                        else if (string.IsNullOrEmpty(row.col_3))
                        {
                            row.col_6 = "Ngày bắt đầu để trống";
                            flag = false;
                        }
                        else if (string.IsNullOrEmpty(row.col_4))
                        {
                            row.col_6 = "Ngày kết thúc để trống";
                            flag = false;
                        }
                        else if (string.IsNullOrEmpty(row.col_5))
                        {
                            row.col_6 = "Ngày chấm để trống";
                            flag = false;
                        }
                        else
                        {
                            string result = Validate.check_MergeEvnpcBlocking(row.col_3, row.col_4, row.col_5);
                            if (!string.IsNullOrEmpty(result))
                            {
                                row.col_6 = result;
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            responseEntity entity = ePosDAO.MergeEvnpcBlocking(string.Empty, row.col_1, row.col_3, row.col_4, row.col_5, string.Empty, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                row.col_6 = "Thành công";
                            }
                            else
                            {
                                Logging.SupportLogger.ErrorFormat("InsertFileContrlDebt => User: {0}, index: {1}, Code: {2}, Error: {3} Session: {4}", posAccount.edong, i, entity.code, entity.description, posAccount.session);
                                row.col_6 = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code));
                            }
                        }
                        rows.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = rows });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("InsertFileContrlDebt => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("InsertFileContrlDebt => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }
        #endregion

        #region Quản lý đối soát
        [AllowAnonymous]
        public ActionResult JobControl()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.JOBCONTROL_TITILE;
            ViewBag.TitleLeft = "Quản lý đối soát";
            
            SetParamModel model = new SetParamModel();
            model.CorporationList = ePOSController.getEVN();
            model.Corporation = "1";
            try
            {
                List<SelectListItem> PCList = new List<SelectListItem>();
                PCList = ePosDAO.GetListPC(model.Corporation, 3, posAccount);
                model.PCList = PCList;
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("JobControl => UserName: {0}, SessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return RedirectToAction("Login", "ePOS", true);
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchJobControl(string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.EvnpcTime(pcCode, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    for (int i = 0; i < entity.listEvnPcTime.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listEvnPcTime.ElementAt(i).evnPcId.ToString();
                        row.col_2 = entity.listEvnPcTime.ElementAt(i).pcCodeExt;
                        row.col_3 = entity.listEvnPcTime.ElementAt(i).offFlag.ToString();
                        row.col_4 = entity.listEvnPcTime.ElementAt(i).strOffWork;
                        row.col_5 = entity.listEvnPcTime.ElementAt(i).cdrTime;
                        row.col_6 = entity.listEvnPcTime.ElementAt(i).strOpen;
                        row.col_7 = entity.listEvnPcTime.ElementAt(i).type.ToString();
                        row.col_8 = entity.listEvnPcTime.ElementAt(i).checkCdr.ToString();
                        row.col_9 = entity.listEvnPcTime.ElementAt(i).checkHoliday.ToString();
                        row.col_10 = entity.listEvnPcTime.ElementAt(i).checkKeep.ToString();
                        row.col_11 = entity.listEvnPcTime.ElementAt(i).status.ToString();
                        row.col_12 = entity.listEvnPcTime.ElementAt(i).cdrSat;
                        row.col_13 = entity.listEvnPcTime.ElementAt(i).cdrSun;
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchJobControl => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchJobControl => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddJobControl()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            EditJobControlModel model = new EditJobControlModel();
            List<SelectListItem> items = new List<SelectListItem>();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.SupportLogger.ErrorFormat("_AddJobControl => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
            }
            else
            {
                foreach (var item in tempPosAcc.EvnPC)
                {
                    items.Add(new SelectListItem { Value = item.pcId.ToString(), Text = item.ext + " - " + item.shortName });
                }
            }
            //model.status = model.offFlag = model.type = model.checkCdr = model.checkHoliday = model.checkKeep = "1";
            model.PCList = items;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult doAdJobControl(string id, string offFlag, string offWork_date, string offWork_time, string cdrTime, string open,
            string type, string checkCdr, string checkHoliday, string checkKeep, string status, string cdrSat, string cdrSun)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string offWork = string.IsNullOrEmpty(offWork_time) == true ? offWork_date.Trim() + " 00:00:00" : offWork_date.Trim() + " " + offWork_time.Trim();
                responseEntity entity = ePosDAO.MergeEvnPcTime(id, string.Empty, offFlag, offWork, cdrTime.Trim(), open, type, checkCdr, checkHoliday, checkKeep, status, cdrSat.Trim(), cdrSun.Trim(),
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", Message = "Thêm mới bản ghi thành công" });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doAdJobControl => User: {0}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }

            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doAdJobControl => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditJobControl(string id, string index)
        {
            ObjReport data = JsonConvert.DeserializeObject<ObjReport>(id);
            ViewBag.cdrSat = data.col_12;
            ViewBag.cdrSun = data.col_13;
            ViewBag.cdrTime = data.col_5;
            ViewBag.checkCdr = string.IsNullOrEmpty(data.col_8) == true ? 0 : int.Parse(data.col_8);
            ViewBag.checkHoliday = string.IsNullOrEmpty(data.col_9) == true ? 0 : int.Parse(data.col_9);
            ViewBag.checkKeep = string.IsNullOrEmpty(data.col_10) == true ? 0 : int.Parse(data.col_10);
            ViewBag.offFlag = string.IsNullOrEmpty(data.col_3) == true ? 0 : int.Parse(data.col_3);
            ViewBag.offWork = data.col_4;
            ViewBag.open = data.col_6;
            ViewBag.status = string.IsNullOrEmpty(data.col_11) == true ? 0 : int.Parse(data.col_11);
            ViewBag.type = string.IsNullOrEmpty(data.col_7) == true ? 0 : int.Parse(data.col_7);
            ViewBag.id = data.col_1;
            ViewBag.index = index;
            ViewBag.pcName = data.col_2;
            return PartialView();
        }

        [HttpPost]
        public JsonResult doEditJobControl(string index, string id, string offFlag, string offWork_date, string offWork_time, string cdrTime, string open,
            string type, string checkCdr, string checkHoliday, string checkKeep, string status, string cdrSat, string cdrSun, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                string offWork = string.IsNullOrEmpty(offWork_time) == true ? offWork_date.Trim() + " 00:00:00" : offWork_date.Trim() + " " + offWork_time.Trim();
                responseEntity entity = ePosDAO.MergeEvnPcTime(id, string.Empty, offFlag, offWork, cdrTime, open, type, checkCdr, checkHoliday, checkKeep, status, cdrSat, cdrSun,
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    int i = int.Parse(index);
                    items.ElementAt(i).col_3 = offFlag;
                    items.ElementAt(i).col_4 = offWork;
                    items.ElementAt(i).col_5 = cdrTime;
                    items.ElementAt(i).col_6 = open;
                    items.ElementAt(i).col_7 = type;
                    items.ElementAt(i).col_8 = checkCdr;
                    items.ElementAt(i).col_9 = checkHoliday;
                    items.ElementAt(i).col_10 = checkKeep;
                    items.ElementAt(i).col_11 = status;
                    items.ElementAt(i).col_12 = cdrSat;
                    items.ElementAt(i).col_13 = cdrSun;
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật trạng thái thành công", Records = items, index = i });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doEditJobControl => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("doEditJobControl => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", index = index, Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        #endregion

        #region Tra log
        public ActionResult LogView()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ViewBag.Title = Constant.HELP_LOGVIEW_TITLE;
            ViewBag.TitleLeft = "Tra log";
            
            LogViewModel model = new LogViewModel();
            List<SelectListItem> parentlist = new List<SelectListItem>();
            List<SelectListItem> acclist = new List<SelectListItem>();
            try
            {
                responseEntity entity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_1, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listAccount != null)
                {
                    foreach (var acc in entity.listAccount)
                    {
                        parentlist.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
                    }
                    acclist.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                    acclist.Add(new SelectListItem { Value = parentlist[0].Value, Text = parentlist[0].Text });
                    entity = ePosDAO.getChildAcc(parentlist[0].Value, Constant.CHILD_1, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listAccount != null)
                    {
                        foreach (var acc in entity.listAccount)
                        {
                            acclist.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
                        }
                    }
                    else
                        Logging.SupportLogger.ErrorFormat("LogView => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                }
                else
                    Logging.SupportLogger.ErrorFormat("LogView => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("LogView => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            model.ParentList = parentlist;
            model.AccountList = acclist;

            List<SelectListItem> ServerList = new List<SelectListItem>();
            //ServerList.Add(new SelectListItem { Value = logStore_15, Text = "Server 15", Selected = true });
            //ServerList.Add(new SelectListItem { Value = logStore_30, Text = "Server 30" });
            //ServerList.Add(new SelectListItem { Value = logStore_18, Text = "Server 18" });
            model.ServerList = ServerList;
            return View(model);
        }
        public ActionResult LogViewE()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.HELP_LOGVIEW_TITLE;
            ViewBag.TitleLeft = "Tra log";

            //check quyen hotro
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (posAccount.type != -1)
                return RedirectToAction("Login", "ePOS", true);

            ObjLogSearchModel model = new ObjLogSearchModel();
            List<SelectListItem> applicationlist = new List<SelectListItem>();
            foreach (var item in Constant.ApplicationList("1"))
            {
                applicationlist.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.ApplicationList = applicationlist;

            List<SelectListItem> typelist = new List<SelectListItem>();
            foreach (var item in Constant.LOGType("1"))
            {
                typelist.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.TypeList = typelist;
            ViewBag.FromDate = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.ToDate = DateTime.Now.ToString("dd/MM/yyyy");

            return View(model);
        }
        [HttpPost]
        public JsonResult SearchLogViewE(string application = "", string type = "", string logId = "", string method = "", string fromDate = "", string toDate = "", int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (fromDate.Length == 10) fromDate = fromDate.Trim() + " 00:00:00";
                if (toDate.Length == 10) toDate = toDate.Trim() + " 23:59:59";
                ObjLogSearchRequest objLogSearch = new ObjLogSearchRequest
                {
                    application = application.Trim(),
                    type = type.Trim(),
                    logId = logId,
                    method = method.Trim(),
                    fromDate = fromDate.Trim(),
                    toDate = toDate.Trim()
                };
                SchedulerResponse objResponse = ePosRestFulDAO.getViewLogbyPage(objLogSearch, pagenum, pagesize, "DESC", string.Empty, Server.MapPath(WEB_PUBLICKEY), posAccount);
                if (objResponse.code == Constant.SUCCESS_CODE)
                {
                    ObjPageLogSearchResponse objPageLogSearch = objResponse.objPageLogSearch;
                    List<ObjLogSearch> data = new List<ObjLogSearch>();
                    data = objPageLogSearch.content;
                    if (data != null && data.Count > 0)
                    {
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = data,
                            total = objPageLogSearch.totalElements
                        });

                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("SearchLogViewE => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, "");
                        return Json(new { Result = "ERROR", Message = string.Format("{0}", "Không tìm thấy dữ liệu") });
                    }

                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("SearchLogViewE => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, objResponse.code, objResponse.msg, posAccount.session);
                    return Json(new { Result = "ERROR", Message = objResponse.msg });
                }
            }
            catch (Exception ex)
            {
                Logging.SupportLogger.ErrorFormat("SearchLogViewE => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public PartialViewResult _DetailLogViewE(string request, string response)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ViewBag.request = HttpUtility.UrlDecode(request);
                ViewBag.response = HttpUtility.UrlDecode(response);
            }
            catch (Exception ex)
            {
                Logging.ReportLogger.ErrorFormat("_DetailLogViewE => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
            }
            return PartialView();
        }

        #endregion
    }
}