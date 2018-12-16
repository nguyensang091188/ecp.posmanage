using ePOS3.eStoreWS;
using ePOS3.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ePOS3.Controllers
{
    [Authorize]
    [AllowAnonymous]
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class ePOSController : Controller
    {

        private int Height = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Img_Height"]));
        private int Width = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Img_Width"]));
        private int Captcha_length = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Captcha_length"]));
        private string Bill_Error = Convert.ToString(ConfigurationManager.AppSettings["BILL_ERROR"]);

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public FileResult generateCaptcha()
        {
            MemoryStream stream = new MemoryStream();
            FontFamily family = new FontFamily("Arial");
            CaptchaImage img = new CaptchaImage(Width, Height, family);
            string text = img.CreateRandomText(Captcha_length);
            img.SetText(text);
            img.GenerateImage();
            img.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            Session["captchaText"] = text;
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Login(string user, string password, string captchar)
        {
            try
            {
                int index = 0;
                if (Session["captchaText"] != null)
                {
                    string img_Captcha = Session["captchaText"].ToString().Replace(" ", "");
                    string result = Validate.Login(user, password, captchar, img_Captcha, ref index);
                    if (string.IsNullOrEmpty(result))
                    {
                        responseEntity resEntity = ePosDAO.doLogin(user, password);
                        if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            ePosAccount posAccount = new ePosAccount
                            {
                                edong = resEntity.responseLoginEdong.account.edong,
                                phone = resEntity.responseLoginEdong.account.phone,
                                name = resEntity.responseLoginEdong.account.name,
                                birthday = resEntity.responseLoginEdong.account.birthday,
                                balance = resEntity.responseLoginEdong.account.balance,
                                address = resEntity.responseLoginEdong.account.address,
                                changedPIN = resEntity.responseLoginEdong.account.changedPIN,
                                email = resEntity.responseLoginEdong.account.email,
                                lockMoney = resEntity.responseLoginEdong.account.lockMoney,
                                status = resEntity.responseLoginEdong.account.status,
                                type = resEntity.responseLoginEdong.account.type,
                                verified = resEntity.responseLoginEdong.account.verified,
                                session = resEntity.responseLoginEdong.account.session,
                                IdNumber = resEntity.responseLoginEdong.account.idNumber,
                                IdNumberDate = resEntity.responseLoginEdong.account.idNumberDate,
                                IdNumberPlace = resEntity.responseLoginEdong.account.idNumberPlace,
                                parent = resEntity.responseLoginEdong.account.parentEdong,
                                parent_id = resEntity.responseLoginEdong.account.parentId.ToString(),
                                i_Cancel = string.IsNullOrEmpty(resEntity.responseLoginEdong.totalCancel) ? 0 : int.Parse(resEntity.responseLoginEdong.totalCancel),
                                i_Bill = string.IsNullOrEmpty(resEntity.responseLoginEdong.totalErr) ? 0 : int.Parse(resEntity.responseLoginEdong.totalErr),
                                IP_Mac = resEntity.responseLoginEdong.account.edong + " " + resEntity.responseLoginEdong.account.name + " " + ePosDAO.get_CPUID() + " " + ePosDAO.get_IPClient() + " " + ePosDAO.GetMacAddress()
                            };
                            Session.Clear();
                            var cookie = FormsAuthentication.GetAuthCookie(user, true);
                            var ticket = FormsAuthentication.Decrypt(cookie.Value);
                            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate,
                                ticket.Expiration, ticket.IsPersistent, JsonConvert.SerializeObject(posAccount), ticket.CookiePath);
                            var encTicket = FormsAuthentication.Encrypt(newTicket);
                            cookie.Value = encTicket;
                            cookie.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["Time"]));
                            Response.Cookies.Add(cookie);
                            List<ObjEVNPC> items = new List<ObjEVNPC>();
                            List<ObjEdong> childs = new List<ObjEdong>();
                            Dictionary<string, string> dict_child = new Dictionary<string, string>();
                            resEntity = ePosDAO.getAccount(posAccount.edong, posAccount);
                            Logging.LoginLogger.InfoFormat("Login=>getAccount user={0}, captcha={1}, resEntity={2}", user,captchar, JsonConvert.SerializeObject(resEntity));

                            if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                if (resEntity.responseLoginEdong.listEvnPC != null && resEntity.responseLoginEdong.listEvnPC.Count() > 0)
                                {
                                    foreach (var item in resEntity.responseLoginEdong.listEvnPC)
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
                                            status = item.status
                                        });
                                    }
                                }
                            }
                            else
                            {
                                Logging.LoginLogger.ErrorFormat("Login (Lấy thông tin điện lực) => User: {0}, Code: {1}, Error: {2}", user, resEntity.code, resEntity.description);
                            }
                            resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
                            Logging.LoginLogger.InfoFormat("Login=>getChildAcc user={0}, captcha={1}, resEntity={2}", user, captchar, JsonConvert.SerializeObject(resEntity));
                            if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                if (resEntity.listAccount != null && resEntity.listAccount.Count() > 0)
                                {
                                    foreach (var item in resEntity.listAccount)
                                    {
                                        if (!dict_child.ContainsKey(item.edong))
                                            dict_child.Add(item.edong, item.name);
                                        childs.Add(new ObjEdong
                                        {
                                            phoneNumber = item.edong,
                                            name = item.name,
                                            address = item.address,
                                            birthday = item.birthday,
                                            idNumber = item.idNumber,
                                            idNumberDate = item.idNumberDate,
                                            idNumberPlace = item.idNumberPlace,
                                            type = item.type.ToString(),
                                            email = item.email,
                                            status = item.status,
                                            parent = item.parentEdong,
                                            parent_id = item.parentId.ToString(),
                                            phone = item.phone,
                                            debtAmount = item.debt,
                                            DebtDate = item.strDebtDate
                                        });
                                    }
                                }
                            }
                            else
                            {
                                Logging.LoginLogger.ErrorFormat("Login=>getChildAcc (Lấy thông tin ví con) => User: {0}, Code: {1}, Error: {2}", user, resEntity.code, resEntity.description);
                            }
                            posAccount.dict_child = dict_child;
                            posAccount.EvnPC = items;
                            posAccount.Childs = childs;                            
                            ePOSSession.AddObject(posAccount.session, posAccount);
                            return Json(new { Result = "SUCCESS" });
                        }
                        else
                        {
                            Logging.LoginLogger.ErrorFormat("Login => User: {0}, Code: {1}, Error: {2}", user, resEntity.code, resEntity.description);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(resEntity.code)), index = index });
                        }
                    }
                    else
                    {
                        Logging.LoginLogger.ErrorFormat("Login => User: {0}, Error: {1}", user, result);
                        return Json(new { Result = "ERROR", Message = result, index = index });
                    }
                }
                else
                {
                    generateCaptcha();
                    return Json(new { Result = "ERROR", Message = Constant.ERROR_LOGIN_CAPTCHA_TIMEOUT, index = index });
                }
            }
            catch (Exception ex)
            {
                Logging.LoginLogger.ErrorFormat("Login => User: {0}, Error: {1}", user, ex.Message);
                return Json(new { Result = "CONNECTION", Message = Constant.ERROR_CONNECTION });
            }
        }

        [AllowAnonymous]
        public ActionResult Account()
        {
            if (!CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.INDEX_TITLE;
            ViewBag.TitleLeft = "Thông tin tài khoản";
            ViewBag.CancelRequest = posAccount.i_Cancel;
            ViewBag.BillError = posAccount.i_Bill;
            return View();
        }

        [HttpPost]
        public JsonResult Refresh()
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                long bill_error = 0;
                long cancel = 0;
                responseEntity entity = new responseEntity();
                entity = ePosDAO.requestTotalErr(posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listBill != null)
                    bill_error = entity.listBill.Count();
                else
                    Logging.EPOSLogger.ErrorFormat("Refresh(BillError) => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);

                entity = ePosDAO.getCancelRequest(string.Empty, string.Empty, Constant.STATUS_ONLINE, PhoneNumber.GetFirstDayOfMonth(), DateTime.Now.ToString("dd/MM/yyyy"), posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    cancel = entity.listTransCanRequest.Count();
                else
                    Logging.EPOSLogger.ErrorFormat("Refresh(Cancel) => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                return Json(new { Result = "SUCCESS", BillError = bill_error.ToString("N0"), CancelBill = cancel.ToString("N0") });
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.ErrorFormat("Refresh => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = Constant.ERROR_CONNECTION });
            }
        }
        public PartialViewResult _CancelRequest()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult DetailCancel()
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                Logging.EPOSLogger.InfoFormat("DetailCancel => User: {0}, status: 1 , fromdate: {1}, todate: {2}, Session: {3}", posAccount.edong, PhoneNumber.GetFirstDayOfMonth(), dateNow, posAccount.session);
                responseEntity entity = ePosDAO.getCancelRequest(string.Empty, string.Empty, Constant.STATUS_ONLINE, PhoneNumber.GetFirstDayOfMonth(), dateNow, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    foreach (var item in entity.listTransCanRequest)
                    {
                        ObjReport row = new ObjReport
                        {
                            col_1 = item.id.ToString(),
                            col_2 = item.eDong,
                            col_3 = item.customerCode,
                            col_4 = item.billId.ToString(),
                            col_5 = item.amount.ToString("N0"),
                            col_6 = item.strCreateDate,
                            col_7 = (from x in Constant.CancelRequestStatus() where x.Key == item.status.ToString() select x).FirstOrDefault().Value,
                            col_8 = item.reason,
                            col_9 = item.strBillingDate,
                            col_10 = item.status.ToString()
                        };
                        items.Add(row);
                    }
                    Logging.BillLogger.InfoFormat("DetailCancel: SUCCESS  items:{0}", JsonConvert.SerializeObject(items));
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.BillLogger.ErrorFormat("DetailCancel => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.BillLogger.ErrorFormat("DetailCancel => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = Constant.ERROR_CONNECTION });
            }
        }

        public PartialViewResult _BillError()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult DetailBill()
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            string dateNow = DateTime.Now.ToString("dd/MM/yyyy");
            try
            {
                responseEntity entity = ePosDAO.requestTotalErr(posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listBill != null)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    int i = 1;
                    foreach (var item in entity.listBill)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = item.customerCode;
                        row.col_2 = item.name;
                        row.col_3 = item.address;
                        row.col_4 = item.bookCmis;
                        row.col_5 = item.amount.ToString("N0");
                        row.col_6 = item.strTerm;
                        try
                        {
                            row.col_7 = (from x in Constant.PaymentResult() where x.Key == item.billingType select x).FirstOrDefault().Value;
                        }
                        catch
                        {
                            row.col_7 = "";
                        }
                        row.col_8 = item.billId.ToString();
                        row.col_9 = PhoneNumber.GetFirstDayOfMonth();
                        row.col_10 = DateTime.Now.ToString("dd/MM/yyyy");
                        row.col_11 = i++.ToString();
                        items.Add(row);
                    }
                    Logging.BillLogger.InfoFormat("DetailCancel: SUCCESS  items:{0}", JsonConvert.SerializeObject(items));
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.BillLogger.ErrorFormat("DetailBill => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.BillLogger.ErrorFormat("DetailBill => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = Constant.ERROR_CONNECTION });
            }
        }

        public ActionResult ChangePassword()
        {
            if (!CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.CHANGEPASS_TITLE;
            ViewBag.TitleLeft = "Đổi mật khẩu";
            return View();
        }

        [HttpPost]
        public JsonResult ChangePass(string OldPass, string NewPass, string ConfirmPass)
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                int index = 0;
                string result = Validate.ChagePass(OldPass, NewPass, ConfirmPass, ref index);
                if (string.IsNullOrEmpty(result))
                {
                    responseChangePIN changePin = ePosDAO.doChangePin(posAccount, OldPass, NewPass);
                    if (changePin.result)
                    {
                        Logging.AccountLogger.InfoFormat("ChangePass: {0}", Json(new { Result = "SUCCESS", Message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại" }).ToString());
                        return
                                Json(new { Result = "SUCCESS", Message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại" });
                    }
                    else
                    {
                        Logging.AccountLogger.ErrorFormat("ChangePass => User: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, changePin.responseCode, changePin.description);
                        return Json(new { Result = "ERROR", Message = changePin.description, index = index });
                    }
                }
                else
                {
                    Logging.AccountLogger.ErrorFormat("ChangePass => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result, index = index });
                }
            }
            catch (Exception ex)
            {
                Logging.AccountLogger.ErrorFormat("ChangePass => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "CONNECTION", Message = Constant.ERROR_CONNECTION });
            }
        }

        public ActionResult LogOff()
        {
            try
            {
                ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                ePosDAO.doLogout(posAccount);
                HttpCookie cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);
                Session.Abandon();
                Session.Clear();
                Session.RemoveAll();
                FormsAuthentication.SignOut();
                Logging.LoginLogger.InfoFormat("LogOff: {0}", Json(new { Result = "SUCCESS", Message = "Đăng xuất thành công" }).ToString());
            }
            catch (Exception ex)
            {
                Logging.LoginLogger.ErrorFormat("LogOff: {0}", Json(new { Result = "ERROR", Message = ex.Message }).ToString());

                HttpCookie cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);
                Session.Abandon();
                Session.Clear();
                Session.RemoveAll();
                FormsAuthentication.SignOut();
            }
            return RedirectToAction("Login", "ePOS");
        }

        [AllowAnonymous]
        public ActionResult LogOffWithoutRedirect()
        {
            try
            {
                ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                ePosDAO.doLogout(posAccount);
                HttpCookie cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);
                Session.Abandon();
                Session.Clear();
                Session.RemoveAll();
                FormsAuthentication.SignOut();
                Logging.LoginLogger.InfoFormat("LogOff: {0}", Json(new { Result = "SUCCESS", Message = "Đăng xuất thành công" }).ToString());
            }
            catch (Exception ex)
            {
                Logging.LoginLogger.ErrorFormat("LogOff: {0}", Json(new { Result = "ERROR", Message = ex.Message }).ToString());

                HttpCookie cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);
                Session.Abandon();
                Session.Clear();
                Session.RemoveAll();
                FormsAuthentication.SignOut();
            }
            return View();
        }

        public static List<SelectListItem> getEVN()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.EVN())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            Logging.AccountLogger.InfoFormat("getEVN: items: {0}",JsonConvert.SerializeObject(items) );
            return items;
        }

        public JsonResult getPCbyEVN(string id, string index)
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var item = ePosDAO.GetListPC(id, int.Parse(index), posAccount);
                if (item != null)
                {
                    Logging.AccountLogger.InfoFormat("getPCbyEVN: id={0}, index={1}, items: {2}",id,index, JsonConvert.SerializeObject(item));
                    return Json(new { Result = "OK", Data = item.ToList() });
                }
                else
                {
                    Logging.AccountLogger.InfoFormat("getPCbyEVN: id={0}, index={1}, items: {2}", id, index, "null");
                    return null;                   
                }
                    
            }
            catch (Exception ex)
            {
                Logging.AccountLogger.ErrorFormat("getPCbyEVN => id={0}, index={1}, UserName: {2}, SessionId: {3}, Error: {4}",id,index,posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult getBookCMISbyPC(string id)
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity resEntity = ePosDAO.getBookCMISbyPC(id, posAccount);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<SelectListItem> items = new List<SelectListItem>();
                    foreach (var item in (from i in resEntity.listBookCmis orderby i.bookCmis1 select i))
                    {
                        items.Add(new SelectListItem { Value = item.bookCmis1, Text = item.bookCmis1 });
                    }
                    Logging.AccountLogger.InfoFormat("getPCbyEVN => SUCCESS UserName: {0},pcCode={1}, items: {2}", posAccount.edong, id, JsonConvert.SerializeObject(items));
                    return Json(new { Result = "SUCCESS", Array = items });
                }
                else
                {
                    Logging.AccountLogger.ErrorFormat("getBookCMISbyPC => UserName: {0}, pcCode={1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong,id,resEntity.code, resEntity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(resEntity.code)) });
                }
            }
            catch (Exception ex)
            {
               Logging.AccountLogger.ErrorFormat("getBookCMISbyPC => UserName: {0}, pcCode={1} SessionId: {2}, Error: {3}", posAccount.edong,id, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult GetAccountChild(string type)
        {
            if (!CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (string.IsNullOrEmpty(type))
                {
                    ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                    if (tempPosAcc.Childs == null)
                    {
                        Logging.AccountLogger.ErrorFormat("GetAccountChild => User: {0}, Error: Ví quản lý không có ví con, Session: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Ví quản lý không có ví con" });
                    }
                    else
                    {
                        List<SelectListItem> items = new List<SelectListItem>();
                        foreach (var acc in tempPosAcc.Childs)
                        {
                            SelectListItem item = new SelectListItem
                            {
                                Value = acc.phoneNumber,
                                Text = acc.phoneNumber + " - " + acc.name
                            };
                            items.Add(item);
                        }
                        Logging.AccountLogger.InfoFormat("GetAccountChild => SUCCESS UserName: {0},type={1}, items: {2}", posAccount.edong, type, JsonConvert.SerializeObject(items));
                        return Json(new { Result = "SUCCESS", Array = items });
                    }
                }
                else
                {
                    responseEntity resEntity = ePosDAO.getChildAcc(type, Constant.CHILD_0, posAccount);
                    if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && resEntity.listAccount != null)
                    {
                        List<SelectListItem> items = new List<SelectListItem>();
                        foreach (var acc in resEntity.listAccount)
                        {
                            SelectListItem item = new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name };
                            items.Add(item);
                        }
                        return Json(new { Result = "SUCCESS", Array = items });
                    }
                    else
                    {
                        Logging.AccountLogger.ErrorFormat("GetAccountChild => User: {0}, type : {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong,type, resEntity.code, resEntity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Không lấy được danh sách ví TNV" });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.AccountLogger.ErrorFormat("GetAccountChild => UserName: {0}, type : {1}, SessionId: {2}, Error: {3}", posAccount.edong,type, posAccount.session, ex.Message);
                return Json(new { Result = "CONNECTION", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult CheckSession()
        {
            if (!CheckSession(HttpContext))
            {
               // Logging.AccountLogger.InfoFormat("CheckSession HttpContext={0}=>ERROR",JsonConvert.SerializeObject(HttpContext));
                return Json(new { Result = "ERROR" });
            }
            else

            {
                //Logging.AccountLogger.InfoFormat("CheckSession HttpContext={0}=>SUCCESS", JsonConvert.SerializeObject(HttpContext));
                return Json(new { Result = "SUCCESS" });
            }
                
        }

        public static bool CheckSession(HttpContextBase context)
        {
            bool sessionExisted = true;
            if (context.Request == null)
            {
                sessionExisted = false;
                //Logging.AccountLogger.InfoFormat("CheckSession HttpContext={0}=>false", JsonConvert.SerializeObject(context));
            }
               
            else if (context.Request.Cookies[".ASPXAUTH"] == null)
            {
                sessionExisted = false;
                //Logging.AccountLogger.InfoFormat("CheckSession HttpContext={0}=>false", JsonConvert.SerializeObject(context));
            }
                
            else
            {
                ePosAccount login = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(context.Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                if (ePOSSession.GetObject(login.session) == null)
                {
                    sessionExisted = false;
                    Logging.AccountLogger.InfoFormat("CheckSession HttpContext={0}=>false", JsonConvert.SerializeObject(ePOSSession.GetObject(login.session)));
                }
                   
            }
            if (!sessionExisted)
            {
                HttpCookie cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                context.Response.Cookies.Add(cookie);
                FormsAuthentication.SignOut();
                context.Session.Clear();
            }
            return sessionExisted;
        }
    }
}