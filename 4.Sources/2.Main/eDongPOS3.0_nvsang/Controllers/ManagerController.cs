using ePOS3.eStoreWS;
using ePOS3.Models;
using ePOS3.Utils;
using Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using MoreLinq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Collections;
using Amib.Threading;

namespace ePOS3.Controllers
{
    [Authorize]
    [AllowAnonymous]
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class ManagerController : Controller
    {
        private int Height = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Img_Height"]));
        private int Width = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Img_Width"]));
        private int Captcha_length = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Captcha_length"]));
        private int thread = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["thread"]));
        #region Quan ly vi
        public ActionResult Account()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.MANAGERACCOUNT_TITLE;
            ViewBag.TitleLeft = "Quản lý ví";
            ManagerAccModel model = new ManagerAccModel();
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = Constant.ALL, Value = "" });
            items.Add(new SelectListItem { Text = "Đang hoạt động", Value = Constant.STATUS_ONLINE });
            items.Add(new SelectListItem { Text = "Ngừng hoạt động", Value = Constant.STATUS_OFFLINE });
            model.StatusList = items;
            return View(model);
        }

        [HttpPost]
        public JsonResult GetTreeAcc(string edong)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (ePOSSession.GetObject(Constant.MANAGER_EDONG + posAccount.session) == null)
                {
                    List<ObjEdong> items = new List<ObjEdong>();
                    if (posAccount.type != -1)
                    {
                        items.Add(new ObjEdong
                        {
                            phoneNumber = posAccount.edong,
                            name = posAccount.name,
                            address = posAccount.address,
                            email = posAccount.email,
                            status = Constant.STATUS_ONLINE,
                            idNumber = posAccount.IdNumber,
                            idNumberDate = posAccount.IdNumberDate,
                            idNumberPlace = posAccount.IdNumberPlace,
                            type = posAccount.type.ToString(),
                            parent = posAccount.parent,
                            parent_id = posAccount.parent_id,
                            phone = posAccount.phone
                        });
                    }
                    responseEntity resEntity = ePosDAO.getChildAcc(edong, Constant.CHILD_0, posAccount);
                    if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        foreach (var item in resEntity.listAccount)
                        {
                            items.Add(new ObjEdong
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
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("GetTreeAcc => Username: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                    }
                    ePOSSession.AddObject(Constant.MANAGER_EDONG + posAccount.session, items);
                    return Json(new { Result = "OK", Records = items.DistinctBy(x => x.phoneNumber).ToList() });
                }
                else
                {
                    List<ObjEdong> items = (List<ObjEdong>)ePOSSession.GetObject(Constant.MANAGER_EDONG + posAccount.session);
                    return Json(new { Result = "OK", Records = items.DistinctBy(x => x.phoneNumber).ToList() });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetTreeAcc => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SearchTreeAcc(string edong, string status, string level)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjEdong> items = new List<ObjEdong>();
                if (posAccount.type != -1)
                    items.Add(new ObjEdong
                    {
                        phoneNumber = posAccount.edong,
                        name = posAccount.name,
                        address = posAccount.address,
                        email = posAccount.email,
                        status = Constant.STATUS_ONLINE,
                        idNumber = posAccount.IdNumber,
                        idNumberDate = posAccount.IdNumberDate,
                        idNumberPlace = posAccount.IdNumberPlace,
                        type = posAccount.type.ToString(),
                        parent = posAccount.parent,
                        parent_id = posAccount.parent_id,
                        phone = posAccount.phone
                    });
                responseEntity resEntity = ePosDAO.getChildAcc(edong, int.Parse(level), posAccount);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && resEntity.listAccount != null)
                {
                    if (string.IsNullOrEmpty(status))
                        foreach (var item in (from i in resEntity.listAccount where i.edong != posAccount.edong select i))
                        {
                            items.Add(new ObjEdong
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
                    else
                        foreach (var item in (from i in resEntity.listAccount where i.status == status && i.edong != posAccount.edong select i))
                        {
                            items.Add(new ObjEdong
                            {
                                phoneNumber = item.edong,
                                name = item.name,
                                address = item.address,
                                birthday = item.birthday,
                                email = item.email,
                                idNumber = item.idNumber,
                                idNumberDate = item.idNumberDate,
                                idNumberPlace = item.idNumberPlace,
                                status = item.status,
                                type = item.type.ToString(),
                                parent = item.parentEdong,
                                parent_id = item.parentId.ToString(),
                                phone = item.phone,
                                debtAmount = item.debt,
                                DebtDate = item.strDebtDate
                            });
                        }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("SearchTreeAcc => Username: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                }
                return Json(new { Result = "OK", Records = items.DistinctBy(x => x.phoneNumber).ToList() });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchTreeAcc => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult DelMergeAccount(string child, string parent, string datasource)
        {
            List<ObjEdong> items = JsonConvert.DeserializeObject<List<ObjEdong>>(datasource);
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));

            try
            {
                responseEntity resEntity = ePosDAO.mergeMappingAccountParent(child, parent, Constant.STATUS_OFFLINE, posAccount);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.RemoveAll(x => x.phoneNumber == child);
                    items.RemoveAll(x => x.parent == child);
                    return Json(new { Result = "SUCCESS", Message = "Thành công", Records = items });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("DelMergeAccount => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}",
                        posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = resEntity.description });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("DelMergeAccount => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditAccount(string _id, string _name, string _idnumber, string _idnumberplace, string _idnumberdate,
           string _address, string _email, string _type, string _phone, string _debtamount, string _debtdate)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            EditAccountModel model = new EditAccountModel();
            List<SelectListItem> TypeAccount = new List<SelectListItem>();
            if (posAccount.type == -1)
                foreach (var x in Constant.TypeAccount())
                {
                    TypeAccount.Add(new SelectListItem { Value = x.Key, Text = x.Value });
                }
            else
                foreach (var x in (from i in Constant.TypeAccount() where i.Key != "-1" select i))
                {
                    TypeAccount.Add(new SelectListItem { Value = x.Key, Text = x.Value });
                }
            model.TypeList = TypeAccount;
            ViewBag.account = _id;
            model.type = _type;
            ViewBag.name = _name;
            ViewBag.idnumber = _idnumber;
            ViewBag.idnumberplace = _idnumberplace;
            ViewBag.idnumberdate = _idnumberdate;
            ViewBag.address = _address;
            ViewBag.email = _email;
            ViewBag.phone = _phone;
            ViewBag.debtamount = _debtamount;
            ViewBag.debtdate = _debtdate;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateAcc(string edong, string name, string address, string idNumber,
            string idNumberDate, string idNumberPlace, string email, string type, string phone, string debtdate, string debtamount, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            List<ObjEdong> items = JsonConvert.DeserializeObject<List<ObjEdong>>(datasource);
            try
            {
                string result = Validate.check_UpdateAcc(email, debtdate);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.UpdateAccount(edong, name, idNumber, idNumberPlace, idNumberDate,
                        address, type, email, phone, debtdate, debtamount.Replace(",", "").Trim(), posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        for (int i = 0; i < items.Count(); i++)
                        {
                            if (items.ElementAt(i).phoneNumber == edong)
                            {
                                items.ElementAt(i).name = name.Trim();
                                items.ElementAt(i).address = address.Trim();
                                items.ElementAt(i).idNumber = idNumber.Trim();
                                items.ElementAt(i).idNumberDate = idNumberDate.Trim();
                                items.ElementAt(i).idNumberPlace = idNumberPlace.Trim();
                                items.ElementAt(i).email = email.Trim();
                                items.ElementAt(i).type = type;
                                items.ElementAt(i).phone = phone;
                                items.ElementAt(i).debtAmount = string.IsNullOrEmpty(debtamount) == true ? string.Empty : long.Parse(debtamount.Replace(",", "").Trim()).ToString("N0");
                                items.ElementAt(i).DebtDate = debtdate;
                                break;
                            }
                        }
                        return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("UpdateAcc => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateAcc => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAcc => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _MapEdong(string edong)
        {
            ViewBag.parent = edong;
            return PartialView();
        }

        [HttpPost]
        public JsonResult SearchAccount(string edong)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getAccount(edong, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    //if(entity.responseLoginEdong.reponseCode.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.responseLoginEdong.account.status == Constant.STATUS_ONLINE)
                    if (entity.responseLoginEdong.reponseCode.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        List<ObjEdong> items = new List<ObjEdong>();
                        items.Add(new ObjEdong
                        {
                            phoneNumber = entity.responseLoginEdong.account.edong,
                            address = entity.responseLoginEdong.account.address,
                            name = entity.responseLoginEdong.account.name,
                            email = entity.responseLoginEdong.account.email,
                            idNumber = entity.responseLoginEdong.account.idNumber,
                            parent = entity.responseLoginEdong.account.parentEdong,
                            status = string.IsNullOrEmpty(entity.responseLoginEdong.account.parentEdong) == true ? "0" : "1"
                        });
                        return Json(new { Result = "SUCCESS", Records = items });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("SearchAccount => UserName: {0}, SessionId: {1}, , status: {4}, Code: {2}, Error: {3}",
                            posAccount.edong, posAccount.session, entity.responseLoginEdong.reponseCode,
                            entity.responseLoginEdong.description, entity.responseLoginEdong.account.status);
                        //return Json(new { Result = "ERROR", Message = entity.responseLoginEdong.account.status.CompareTo(Constant.STATUS_OFFLINE) == 0 ? 
                        //    "Tài khoản ví edong không có quyền" :  ConvertResponseCode.GetResponseDescription(int.Parse(entity.responseLoginEdong.reponseCode))});
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.responseLoginEdong.reponseCode)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("SearchAccount => UserName: {0}, SessionId: {1}, Code: {2}, Error: {3}",
                        posAccount.edong, posAccount.session, entity.code, entity.description);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchAccount => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult MergeAccount(string child, string parent, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            List<ObjEdong> items = JsonConvert.DeserializeObject<List<ObjEdong>>(datasource);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity resEntity = ePosDAO.mergeMappingAccountParent(child, parent, Constant.STATUS_ONLINE, posAccount);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", Message = "Gán ví thành công", Records = items.Select(c => { c.parent = parent; c.status = "1"; return c; }).ToList() });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("MergeAccount => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}",
                        posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(resEntity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("MergeAccount => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _ListPC(string edong)
        {
            ViewBag.parent = edong;
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetPCByEdong(string edong, string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (edong.CompareTo(posAccount.edong) == 0)
                {
                    ePosAccount tempAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                    return Json(new { Result = "SUCCESS", Records = tempAcc.EvnPC });
                }
                else
                {
                    responseEntity entity = ePosDAO.GetPCMapEdong(edong, posAccount);
                    List<ObjEVNPC> data = new List<ObjEVNPC>();
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO != null)
                    {
                        if (string.IsNullOrEmpty(pcCode))
                        {
                            for (int i = 0; i < entity.listEvnPcBO.Count(); i++)
                            {
                                data.Add(new ObjEVNPC
                                {
                                    address = entity.listEvnPcBO.ElementAt(i).address,
                                    code = entity.listEvnPcBO.ElementAt(i).code,
                                    dateChanged = entity.listEvnPcBO.ElementAt(i).dateChanged.ToString("dd-MM-yyyy"),
                                    dateCreated = entity.listEvnPcBO.ElementAt(i).dateCreated.ToString("dd-MM-yyyy"),
                                    ext = entity.listEvnPcBO.ElementAt(i).ext,
                                    fullName = entity.listEvnPcBO.ElementAt(i).fullName,
                                    level = entity.listEvnPcBO.ElementAt(i).level,
                                    mailCc = entity.listEvnPcBO.ElementAt(i).mailCc,
                                    mailTo = entity.listEvnPcBO.ElementAt(i).mailTo,
                                    parentId = entity.listEvnPcBO.ElementAt(i).parentId,
                                    pcId = entity.listEvnPcBO.ElementAt(i).pcId,
                                    phone1 = entity.listEvnPcBO.ElementAt(i).phone1,
                                    phone2 = entity.listEvnPcBO.ElementAt(i).phone2,
                                    shortName = entity.listEvnPcBO.ElementAt(i).shortName,
                                    status = entity.listEvnPcBO.ElementAt(i).status
                                });
                            }
                        }
                        else
                        {
                            if (entity.listEvnPcBO != null)
                            {
                                return Json(new { Result = "SUCCESS", Records = (from i in entity.listEvnPcBO where i.ext.Contains(pcCode.Trim().ToUpper()) select i).ToList() });
                            }
                        }
                        return Json(new { Result = "SUCCESS", Records = data });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("GetPCByEdong => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                        return Json(new { Result = "ERROR", Message = entity.code.CompareTo("000") == 0 ? "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" : ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetPCByEdong => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult DelMappingEVNPC(string pccode, string edong, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });

            List<ObjEVNPC> items = JsonConvert.DeserializeObject<List<ObjEVNPC>>(datasource);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity resEntity = ePosDAO.mergeMappigAccountEVNPC(edong, pccode, Constant.STATUS_OFFLINE, posAccount);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.RemoveAll(x => x.ext == pccode);
                    return Json(new { Result = "SUCCESS", Message = "Xóa điện lực thành công", Records = items });
                }

                else
                {
                    Logging.ManagementLogger.ErrorFormat("DelMappingEVNPC => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}",
                        posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = resEntity.description });
                }
            }
            catch (Exception ex)
            {

                Logging.ManagementLogger.ErrorFormat("DelMappingEVNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _MappingEVNPC(string edong)
        {
            ViewBag.parent = edong;
            return PartialView();
        }

        [HttpPost]
        public JsonResult SearchEVNPCByEdong(string edong, string pccode, string pcname)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjEVNPC> items = new List<ObjEVNPC>();
                List<ObjEVNPC> evn_parent = new List<ObjEVNPC>();
                List<ObjEVNPC> evn_child = new List<ObjEVNPC>();
                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (tempPosAcc.EvnPC == null)
                {
                    Logging.ManagementLogger.ErrorFormat("SearchEVNPCByEdong => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = "Ví quản lý chưa có danh sách điện lực" });
                }
                else
                {
                    if (edong.CompareTo(posAccount.edong) != 0)
                    {
                        responseEntity Entity_child = ePosDAO.GetPCMapEdong(edong, posAccount);
                        if (Entity_child.code.CompareTo(Constant.SUCCESS_CODE) == 0 && Entity_child.listEvnPcBO != null)
                        {
                            foreach (var item in (from i in Entity_child.listEvnPcBO where i.status == int.Parse(Constant.STATUS_ONLINE) select i))
                            {
                                evn_child.Add(new ObjEVNPC
                                {
                                    address = item.address,
                                    code = item.code,
                                    ext = item.ext,
                                    fullName = item.fullName,
                                    shortName = item.shortName,
                                    pcId = item.pcId
                                });
                            }
                            items = (from p in tempPosAcc.EvnPC where !(from c in evn_child select c.pcId).Contains(p.pcId) select p).ToList();
                        }
                        else
                        {
                            items = tempPosAcc.EvnPC;
                        }
                    }
                    else
                        items = tempPosAcc.EvnPC;
                    if (string.IsNullOrEmpty(pccode))
                        items = string.IsNullOrEmpty(pcname) == true ? items : (from p in items where p.fullName.Contains(pcname) select p).ToList();
                    else
                        //items = (from p in items where  p.ext == pccode select p).ToList();
                        items = items.Where(p => p.ext.StartsWith(pccode.ToUpper())).ToList();
                    return Json(new { Result = "SUCCESS", Records = items });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchEVNPCByEdong => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult MappingEVNPC(string pccode, string edong, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            List<ObjEVNPC> items = JsonConvert.DeserializeObject<List<ObjEVNPC>>(datasource);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.mergeMappigAccountEVNPC(edong, pccode, Constant.STATUS_ONLINE, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {

                    return Json(new { Result = "SUCCESS", Message = "Gán điện lực thành công", Records = items.Where(x => x.ext != pccode).ToList() });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("MappingEVNPC => UserName: {0}, Code: {1}, Error: {2},  sessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = entity.description });
                }

            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("MappingEVNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult MappingAllEVNPC(string pccode, string edong, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            List<ObjEVNPC> items = JsonConvert.DeserializeObject<List<ObjEVNPC>>(datasource);
            List<ObjEVNPC> temp = JsonConvert.DeserializeObject<List<ObjEVNPC>>(datasource);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (string.IsNullOrEmpty(pccode.Substring(1, pccode.Length - 2)))
                    return Json(new { Result = "ERROR", Message = "Bạn phải chọn ít nhất một điện lực để gán" });
                int i_success = 0;
                int i_error = 0;
                foreach (string item in pccode.Substring(1, pccode.Length - 2).Split(','))
                {
                    string code = temp.ElementAt(int.Parse(item)).ext;
                    responseEntity entity = ePosDAO.mergeMappigAccountEVNPC(edong, code, Constant.STATUS_ONLINE, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        i_success++;
                        items = items.Where(x => x.code != temp.ElementAt(int.Parse(item)).code).ToList();
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("MappingAllEVNPC => UserName: {0}, Code: {1}, Error: {2},  sessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        i_error++;
                    }
                }
                return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết gán điện lực cho ví: {0}. <br> - Thành công: {1} PC. <br> - Không thành công: {2} PC", edong, i_success, i_error), Records = items });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("MappingAllEVNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _ListBookCMIS(string edong)
        {
            ViewBag.parent = edong;
            MapBookCMISModel model = new MapBookCMISModel();
            List<SelectListItem> items = new List<SelectListItem>();
            if (!ePOSController.CheckSession(HttpContext))
            {
                model.PCList = items;
                model.StatusCMISList = items;
            }
            else
            {
                ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                model.PCList = ePosDAO.getPCMapEdong(edong, posAccount);
                items.Add(new SelectListItem { Value = "", Text = Constant.ALL });
                items.Add(new SelectListItem { Value = Constant.STATUS_ONLINE, Text = "Đang giao thu" });
                items.Add(new SelectListItem { Value = Constant.STATUS_OFFLINE, Text = "Ngừng giao thu" });
                model.StatusCMISList = items;
            }
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult SearchBooCMIS(string edong, string bookcmis, string pcCode, string pcName, string status)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjBookCMIMS> items = new List<ObjBookCMIMS>();
                responseEntity entity = ePosDAO.getBookCMISMapping(edong, pcCode, bookcmis, status, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {

                    for (int i = 0; i < entity.listAccountBookcmisMapping.Count(); i++)
                    {
                        items.Add(new ObjBookCMIMS
                        {
                            pcCode = entity.listAccountBookcmisMapping.ElementAt(i).pcCode,
                            edong = entity.listAccountBookcmisMapping.ElementAt(i).edong,
                            bookCMIS = entity.listAccountBookcmisMapping.ElementAt(i).bookcmis,
                            pcName = pcName.Split('-')[1].TrimStart(),
                            status = entity.listAccountBookcmisMapping.ElementAt(i).status.ToString()
                        });
                    }
                    return Json(new { Result = "SUCCESS", Message = "Lấy thông tin thành công", Records = items });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("SearchBooCMIS => UserName: {0}, edong: {1}, code: {2}, Error: {3}, SessionId: {4}", posAccount.edong, edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = entity.listAccountBookcmisMapping == null ? "Ví edong không có danh sách giao thu" : ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }

            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchBooCMIS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UpadetBookCMIS(string edong, string bookCMIS, string pcCode, string status, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            List<ObjBookCMIMS> items = JsonConvert.DeserializeObject<List<ObjBookCMIMS>>(datasource);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string temp = string.Empty;
                temp = status.CompareTo(Constant.STATUS_ONLINE) == 0 ? Constant.STATUS_OFFLINE : Constant.STATUS_ONLINE;
                responseEntity resEntity = ePosDAO.mergeAccountBookcmisMapping(edong, bookCMIS, pcCode, temp, posAccount);
                if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.Where(s => s.bookCMIS == bookCMIS).ToList().ForEach(i => i.status = temp);
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật trạng thái thành công", Records = items });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpadetBookCMIS => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}",
                        posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(resEntity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpadetBookCMIS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _MapBookCMIS(string edong)
        {
            ViewBag.parent = edong;
            List<SelectListItem> items = new List<SelectListItem>();
            MapBookCMISModel model = new MapBookCMISModel();
            model.EDongCMIS = edong;
            if (!ePOSController.CheckSession(HttpContext))
            {
                model.PCList = items;
                model.StatusCMISList = items;

            }
            else
            {
                ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                model.PCList = ePosDAO.getPCMapEdong(edong, posAccount);
                model.type = "1";
            }
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult AddMapBookCMIS(string pccode, string bookcmis, string edong)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_AddMapBookCMIS(bookcmis);
                if (string.IsNullOrEmpty(result))
                {
                    string[] array = PhoneNumber.ProcessCustomerGroup(bookcmis);
                    string msg_success = string.Empty;
                    string msg_error = string.Empty;
                    foreach (var item in array)
                    {
                        responseEntity entity = ePosDAO.mergeAccountBookcmisMapping(edong, item.Trim().ToUpper(), pccode, Constant.STATUS_ONLINE, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            msg_success = string.IsNullOrEmpty(msg_success) == true ? "- Mã quyển gán thành công: " + item : msg_success + ", " + item;
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("AddMapBookCMIS => UserName: {0}, type: {1}, BookCMIS: {2}, PCCode: {3}, Code: {4}, Error: {5}, SessionId: {6}", posAccount.edong, "", item, pccode, entity.code, entity.description, posAccount.session);
                            msg_error = string.IsNullOrEmpty(msg_error) == true ? "- Mã quyển gán lỗi: " + item : msg_error + ", " + item;
                        }
                    }
                    if (string.IsNullOrEmpty(msg_error) && string.IsNullOrEmpty(msg_success))
                        return Json(new { Result = "ERROR", Message = "Gán sổ giao thu lỗi" });
                    else
                    {
                        string msg = string.IsNullOrEmpty(msg_success) == true ? string.Empty : msg_success;
                        if (string.IsNullOrEmpty(msg))
                            msg = string.IsNullOrEmpty(msg_error) == true ? string.Empty : msg_error;
                        else
                            msg = string.IsNullOrEmpty(msg_error) == true ? msg : msg + "<br>" + msg_error;
                        return Json(new { Result = "SUCCESS", Message = string.Format("Kết quả gán giao thu: <br>{0}", msg) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("AddMapBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("AddMapBookCMIS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        #endregion

        #region Quan ly SGCS
        public ActionResult BookCMIS()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.MANAGER_BOOKCMIS_TITLE;
            ViewBag.TitleLeft = "Sổ ghi chỉ số";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            Logging.ManagementLogger.InfoFormat("BookCMIS => User: {0}, Msg: Vào Tab BookCMIS, Session: {1}", posAccount.edong, posAccount.session);
            BoockCMISModel model = new BoockCMISModel();
            model.CorporationList = ePOSController.getEVN();
            List<SelectListItem> days = new List<SelectListItem>();
            days.Add(new SelectListItem { Value = "", Text = "--Chọn ngày--" });
            for (int i = 1; i <= 31; i++)
            {
                if (DateTime.Today.Day == i)
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                else
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
            model.DayList = days;
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.StatusBookCMIS())
            {
                status.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.StatusCMISList = status;
            List<SelectListItem> items = new List<SelectListItem>();
            if (posAccount.type == -1)
            {
                model.Corporation = "1";
                responseEntity entity = ePosDAO.getPCbyId(model.Corporation, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO.Count() > 0)
                {
                    foreach (var item in (from p in entity.listEvnPcBO where p.status == int.Parse(Constant.STATUS_ONLINE) && p.pcId != long.Parse(model.Corporation) select p))
                    {
                        items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                    }
                }
                else
                    Logging.ManagementLogger.ErrorFormat("BookCMIS => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
            }
            else
            {
                items = ePosDAO.getPCMapEdong(string.Empty, posAccount);
            }
            model.PCList = items;
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchDowloadBookCMIS(string pcCode = "", string day = "", string status = "", string bookCMIS = "", int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                //if (bookCMIS.Trim().Length < 3) 12548904155118
                //    return Json(new { Result = "ERROR", Message = "Sổ GCS tối thiểu phải 3 ký tự!" });               
                responseEntity entity = ePosDAO.GetBookCMIS(pcCode, PhoneNumber.ProcessCustomerGroup(bookCMIS.Trim()), day, status, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> rows = new List<ObjReport>();
                    for (int i = 0; i < entity.listBookCmis.Count(); i++)
                    {
                        rows.Add(new ObjReport
                        {
                            col_1 = (i + 1).ToString(),
                            col_2 = entity.listBookCmis.ElementAt(i).bookCmis1,
                            col_3 = entity.listBookCmis.ElementAt(i).strCreateDate,
                            col_4 = entity.listBookCmis.ElementAt(i).inningDate,
                            col_5 = entity.listBookCmis.ElementAt(i).email,
                            col_6 = (from x in Constant.StatusBookCMIS() where x.Key == entity.listBookCmis.ElementAt(i).strStatus select x).FirstOrDefault().Value,
                            col_7 = entity.listBookCmis.ElementAt(i).pcCodeExt,
                            col_8 = entity.listBookCmis.ElementAt(i).id,
                            col_9 = entity.listBookCmis.ElementAt(i).strStatus,
                            col_10 = pcCode,
                            col_11 = entity.listBookCmis.ElementAt(i).countBill.ToString("N0"),
                            col_12 = entity.listBookCmis.ElementAt(i).sumAmount.ToString("N0"),
                            col_13 = entity.listBookCmis.ElementAt(i).countCustomer.ToString("N0"),
                            col_14 = entity.listBookCmis.ElementAt(i).issueDate
                        });
                    }
                    string date = DateTime.Now.ToString();
                    ePOSSession.AddObject(Constant.EDIT_BOOKCMIS + date, rows);
                    return Json(new { Result = "SUCCESS", Records = rows, id = Constant.EDIT_BOOKCMIS + date });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("SearchBookCMIS => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchBookCMIS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddBookCMIS()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            AddBookCISModel model = new AddBookCISModel();
            model.CorporationList = ePOSController.getEVN();
            List<SelectListItem> items = new List<SelectListItem>();
            model.Add_Status = Constant.STATUS_ONLINE;
            if (posAccount.type == -1)
            {
                ViewBag.check = 1;
                model.Add_Corporation = "1";
                responseEntity entity = ePosDAO.getPCbyId(model.Add_Corporation, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO.Count() > 0)
                {
                    foreach (var item in (from p in entity.listEvnPcBO where p.status == int.Parse(Constant.STATUS_ONLINE) && p.pcId != long.Parse(model.Add_Corporation) select p))
                    {
                        items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                    }
                }
                else
                    Logging.ManagementLogger.ErrorFormat("AddBookCMIS => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
            }
            else
            {
                ViewBag.check = 0;
                items = ePosDAO.getPCMapEdong(string.Empty, posAccount);
            }
            List<SelectListItem> days = new List<SelectListItem>();
            days.Add(new SelectListItem { Value = "", Text = "--Chọn ngày--" });
            for (int i = 1; i <= 31; i++)
            {
                if (DateTime.Today.Day == i)
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = true });
                else
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
            model.DayList = days;
            model.PCList = items;
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult doAddBookCMIS(string pcCode, string bookCMIS, string day, string day_released, string status, string email)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            int index = 0;
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    string[] array_email = email.Split(';');
                    if (array_email.Length > 8)
                        return Json(new { Result = "ERROR", Message = "Số lượng Email quá lớn" });
                    foreach (var item in email.Split(';'))
                    {
                        if (!Validate.Validate_Email(item))
                            return Json(new { Result = "ERROR", Message = "Email không đúng định dạng", index = 1 });
                    }
                }
                if (PhoneNumber.ProcessCustomerGroup(bookCMIS.Trim()).Length > 20)
                    return Json(new { Result = "ERROR", Message = "Số lượng sổ GCS lớn hơn 20" });
                string msg_success = string.Empty;
                string msg_error = string.Empty;
                foreach (var item in PhoneNumber.ProcessCustomerGroup(bookCMIS.Trim()))
                {
                    responseEntity entity = ePosDAO.MergeBookCMIS(string.Empty, pcCode, item, day, day_released, status, email, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        msg_success = string.IsNullOrEmpty(msg_success) == true ? "- Mã quyển thêm thành công: " + item : msg_success + ", " + item;
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("doAddBookCMIS => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        msg_error = string.IsNullOrEmpty(msg_error) == true ? "- Mã quyển thêm lỗi: " + item : msg_error + ", " + item;
                    }
                }
                if (string.IsNullOrEmpty(msg_error) && string.IsNullOrEmpty(msg_success))
                    return Json(new { Result = "ERROR", Message = "Thêm mới lỗi", index = index });
                else
                {
                    string msg = string.IsNullOrEmpty(msg_success) == true ? string.Empty : msg_success;
                    if (string.IsNullOrEmpty(msg))
                        msg = string.IsNullOrEmpty(msg_error) == true ? string.Empty : msg_error;
                    else
                        msg = string.IsNullOrEmpty(msg_error) == true ? msg : msg + "<br>" + msg_error;
                    return Json(new { Result = "SUCCESS", Message = string.Format("Kết quả thêm mới : <br>{0}", msg) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doAddBookCMIS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = index });
            }
        }

        public PartialViewResult _EditBookCMIS(string id, string key)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            AddBookCISModel model = new AddBookCISModel();
            try
            {
                List<ObjReport> rows = (List<ObjReport>)ePOSSession.GetObject(id);

                var data = (from a in rows where a.col_1 == key select a).FirstOrDefault();
                model.CorporationList = ePOSController.getEVN();
                List<SelectListItem> days = new List<SelectListItem>();
                days.Add(new SelectListItem { Value = "", Text = "--Chọn ngày--" });
                for (int i = 1; i <= 31; i++)
                {
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
                model.DayList = days;
                model.Add_Day = data.col_4;
                model.Add_Released = data.col_14;
                List<SelectListItem> pclist = new List<SelectListItem>();
                ViewBag.check = 0;
                responseEntity entity = ePosDAO.GetPCMapEdong(string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && entity.listEvnPcBO != null)
                {
                    foreach (var item in entity.listEvnPcBO)
                    {
                        if (item.ext == data.col_7)
                            pclist.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName, Selected = true });
                        else
                            pclist.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("EditBookCMIS => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                }
                model.PCList = pclist;

                ViewBag.BookCMIS = data.col_2;
                ViewBag.status = string.IsNullOrEmpty(data.col_9) == true ? 0 : int.Parse(data.col_9);
                ViewBag.Email = data.col_5;
                ViewBag.Sysdate = data.col_3;
                ViewBag.id = data.col_8;
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("EditBookCMIS => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult doEditBookCMIS(string id, string pcCode, string bookCMIS, string day, string day_released, string status, string email, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
            int index = 0;
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    string[] array_email = email.Split(';');
                    if (array_email.Length > 8)
                        return Json(new { Result = "ERROR", Message = "Số lượng Email quá lớn" });
                    foreach (var item in array_email)
                    {
                        if (!Validate.Validate_Email(item))
                            return Json(new { Result = "ERROR", Message = "Email không đúng định dạng", index = 1 });
                    }
                }
                responseEntity entity = ePosDAO.MergeBookCMIS(id, pcCode, bookCMIS, day, day_released, status, email, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items.ElementAt(i).col_8.CompareTo(id) == 0)
                        {
                            items.ElementAt(i).col_2 = bookCMIS;
                            items.ElementAt(i).col_4 = day;
                            items.ElementAt(i).col_5 = email;
                            items.ElementAt(i).col_6 = (from x in Constant.StatusBookCMIS() where x.Key == status select x).FirstOrDefault().Value;
                            items.ElementAt(i).col_7 = pcCode;
                            items.ElementAt(i).col_9 = status;
                            index = i;
                            break;
                        }
                    }
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật thông tin thành công", Records = items, index = index });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("doEditBookCMIS => User: {0}, Code: {1}, Error: {2}, Session: {4}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)), index = index });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doEditBookCMIS => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")), index = index });
            }
        }

        [HttpPost]
        public JsonResult SearchAssignBillLog(string pccode = "", string bookCMIS = "")
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (string.IsNullOrEmpty(bookCMIS))
                {
                    Logging.ManagementLogger.ErrorFormat("SearchAssignBillLog => User: {0}, Error: BookCMIS null, Session: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = "Sổ ghi chỉ số không được để trống" });
                }
                string[] array = pccode.Split('-');
                responseEntity entity = ePosDAO.getAssignBillLog(array[0].Trim(), PhoneNumber.ProcessCustomerGroup(bookCMIS.Trim()), "0", posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> rows = new List<ObjReport>();
                    if (entity.listBookCmis != null)
                        for (int i = 0; i < entity.listBookCmis.Count(); i++)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = (i + 1).ToString();
                            row.col_2 = array[1].Trim();
                            row.col_3 = array[0].Trim();
                            row.col_4 = entity.listBookCmis.ElementAt(i).bookCmis1;
                            row.col_5 = entity.listBookCmis.ElementAt(i).strStatus;
                            row.col_7 = entity.listBookCmis.ElementAt(i).countBill.ToString("N0");
                            row.col_9 = entity.listBookCmis.ElementAt(i).strStartDate;
                            row.col_10 = entity.listBookCmis.ElementAt(i).strEndDate;
                            row.col_11 = entity.listBookCmis.ElementAt(i).orderQueue.ToString("N0");
                            row.col_12 = entity.listBookCmis.ElementAt(i).status.ToString();
                            rows.Add(row);
                        }
                    return Json(new { Result = "SUCCESS", Records = rows });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("SearchAssignBillLog => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchAssignBillLog => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UpdateAssignBillLog(string pcCode, string bookCMIS, string status, string index, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                responseEntity entity = ePosDAO.UpdateAssignBillLog(pcCode, bookCMIS, status, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listBookCmis.Count(); i++)
                    {
                        items.ElementAt(int.Parse(index)).col_9 = entity.listBookCmis.ElementAt(i).strStartDate;
                        items.ElementAt(int.Parse(index)).col_10 = entity.listBookCmis.ElementAt(i).strEndDate;
                        items.ElementAt(int.Parse(index)).col_11 = entity.listBookCmis.ElementAt(i).orderQueue.ToString("N0");
                        items.ElementAt(int.Parse(index)).col_12 = entity.listBookCmis.ElementAt(i).status.ToString();
                    }
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật thông tin thành công", Records = items });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateAssignBillLog => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAssignBillLog => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult AddFileBookCMIS()
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
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_EXCEL, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_EXCEL });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("AddFileBookCMIS => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("AddFileBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InsertDataExcel()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (ePOSSession.GetObject(posAccount.session + ePOSSession.UPLOAD_EXCEL) != null)
                {
                    DataSet ds = (DataSet)ePOSSession.GetObject(posAccount.session + ePOSSession.UPLOAD_EXCEL);
                    List<ObjReport> rows = new List<ObjReport>();
                    for (int i = 0; i < ds.Tables[0].AsEnumerable().Skip(1).Count(); i++)
                    {
                        Logging.ManagementLogger.InfoFormat("InsertDataExcel => User: {0}, index: {1}, Session: {2}", posAccount.edong, i, posAccount.session);

                        bool flag = true;
                        ObjReport row = new ObjReport();
                        row.col_1 = (i + 1).ToString();
                        row.col_2 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[1].ToString().Trim();//pccode
                        row.col_3 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString().Trim();//bookcmis
                        row.col_4 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[3].ToString().Trim();// day ghi chi so
                        row.col_5 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[4].ToString().Trim();//ngay pha hanh                      
                        row.col_6 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[5].ToString().Trim();//trang thai
                        row.col_7 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[6].ToString().Trim();// email

                        if (string.IsNullOrEmpty(row.col_2))
                        {
                            row.col_8 = "Mã công ty điện lực để trống";
                            flag = false;
                        }
                        else if (string.IsNullOrEmpty(row.col_3))
                        {
                            row.col_8 = "Sổ GCS để trống";
                            flag = false;
                        }
                        else if (string.IsNullOrEmpty(row.col_4))
                        {
                            row.col_8 = "Ngày GCS để trống";
                            flag = false;
                        }
                        try
                        {
                            if (int.Parse(row.col_4) < 1 || int.Parse(row.col_4) > 31)
                            {
                                row.col_8 = "Ngày GCS phải nằm trong khoảng 1 đến 31";
                                flag = false;
                            }
                        }
                        catch
                        {
                            row.col_8 = "Ngày GCS không đúng định dạng";
                            flag = false;
                        }
                        try
                        {
                            if (int.Parse(row.col_5) < 1 || int.Parse(row.col_5) > 31)
                            {
                                row.col_8 = "Ngày phát hành phải nằm trong khoảng 1 đến 31";
                                flag = false;
                            }
                        }
                        catch
                        {
                            row.col_8 = "Ngày phát hành không đúng định dạng";
                            flag = false;
                        }
                        if (string.IsNullOrEmpty(row.col_6))
                        {
                            row.col_8 = "Trạng thái để trống";
                            flag = false;
                        }
                        try
                        {
                            int status = int.Parse(row.col_6);
                            if (status < 0 || status > 1)
                            {
                                row.col_8 = "Trạng thái phải là 0 hoặc 1";
                                flag = false;
                            }
                        }
                        catch
                        {
                            row.col_8 = "Trạng thái không đúng định dạng";
                            flag = false;
                        }
                        if (!string.IsNullOrEmpty(row.col_7))
                        {
                            foreach (var item in row.col_7.Split(';'))
                            {
                                if (!Validate.Validate_Email(item))
                                {
                                    row.col_8 = "Email không đúng định dạng";
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            string success = string.Empty;
                            string error = string.Empty;
                            foreach (var item in PhoneNumber.ProcessCustomerGroup(row.col_3.Trim()))
                            {
                                responseEntity entity = ePosDAO.MergeBookCMIS(string.Empty, row.col_2, item, row.col_4, row.col_5, row.col_6, row.col_7, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    success = string.IsNullOrEmpty(success) == true ? "Thành công: " + item : success + ", " + item;
                                else
                                {
                                    Logging.ManagementLogger.ErrorFormat("InsertDataExcel => User: {0}, BookCMIS: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, item, entity.code, entity.description, posAccount.session);
                                    error = string.IsNullOrEmpty(error) == true ? "Lỗi: " + item : error + ", " + item;
                                }
                            }
                            if (string.IsNullOrEmpty(success))
                                row.col_8 = error;
                            else
                                row.col_8 = success + ". " + error;
                        }
                        rows.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = rows });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("InsertDataExcel => User: {0}, Error: Phiên làm việc không tồn tại, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("InsertDataExcel => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }
        #endregion

        #region Yêu cầu huy
        public ActionResult CancelRequest()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.CANCELREQUEST_TITLE;
            ViewBag.TitleLeft = "Yêu cầu hủy";
            CancelRequestModel model = new CancelRequestModel();
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ManagementLogger.ErrorFormat("CancelRequest => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            List<SelectListItem> EDongList = new List<SelectListItem>();
            responseEntity resEntity = ePosDAO.getChildAccFromSession(posAccount);

            //responseEntity resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
            EDongList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && resEntity.listAccount != null)
            {
                foreach (var acc in resEntity.listAccount)
                {
                    EDongList.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
                }
                ePOSSession.AddObject(ePOSSession.LIST_EDONG, resEntity.listAccount);
            }
            else
            {
                Logging.ManagementLogger.ErrorFormat("CancelRequest => UserName: {0}, Error: Không có danh sách ví con, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> StatusList = new List<SelectListItem>();
            StatusList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.CancelRequestStatus())
            {
                StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.PCList = PCList;
            model.EdongList = EDongList;
            model.StatusList = StatusList;
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchCancelRequest(string edong, string customer, string status, string fromdate, string todate)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(ePOSSession.LIST_EDONG) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_CancelRequest(fromdate, todate);
                if (string.IsNullOrEmpty(result))
                {
                    account[] array = (account[])ePOSSession.GetObject(ePOSSession.LIST_EDONG);
                    responseEntity entity = ePosDAO.getCancelRequest(edong, customer, status, fromdate + " 00:00:00", todate + " 23:59:59", posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        List<ObjReport> items = new List<ObjReport>();
                        for (int i = 0; i < entity.listTransCanRequest.Count(); i++)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = entity.listTransCanRequest.ElementAt(i).id.ToString();
                            row.col_2 = entity.listTransCanRequest.ElementAt(i).eDong;
                            row.col_3 = entity.listTransCanRequest.ElementAt(i).customerCode;
                            row.col_4 = entity.listTransCanRequest.ElementAt(i).billId.ToString();
                            row.col_5 = entity.listTransCanRequest.ElementAt(i).amount.ToString("N0");
                            row.col_6 = entity.listTransCanRequest.ElementAt(i).strCreateDate;
                            row.col_7 = (from x in Constant.CancelRequestStatus() where x.Key == entity.listTransCanRequest.ElementAt(i).status.ToString() select x).FirstOrDefault().Value;
                            row.col_8 = entity.listTransCanRequest.ElementAt(i).reason;
                            row.col_9 = entity.listTransCanRequest.ElementAt(i).requestDate;
                            row.col_10 = entity.listTransCanRequest.ElementAt(i).status.ToString();
                            //if()
                            //row.col_11 = entity.listTransCanRequest.ElementAt(i).eDong.CompareTo(posAccount.edong) == 0 ? posAccount.name :
                            //    (from x in array where x.edong == entity.listTransCanRequest.ElementAt(i).eDong select x).FirstOrDefault().name;
                            row.col_12 = entity.listTransCanRequest.ElementAt(i).traceNumber.ToString("N0");
                            row.col_13 = entity.listTransCanRequest.ElementAt(i).billingType;
                            row.col_14 = entity.listTransCanRequest.ElementAt(i).auditNumber + "";
                            row.col_15 = entity.listTransCanRequest.ElementAt(i).strBillingDate;
                            items.Add(row);
                        }
                        Logging.ManagementLogger.InfoFormat("SearchCancelRequest => User: {0}, Step 4, Session: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "SUCCESS", Records = items });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("SearchCancelRequest => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("SearchCancelRequest => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchCancelRequest => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult GetInfoRequest(string fromdate, string tracenumber, string auditnumber, string billingdate, string edong)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.doCheckTrans(fromdate.Split(' ')[0], tracenumber.Replace(".", "").Replace(",", ""), auditnumber, billingdate.Split(' ')[0], edong, posAccount);
                if (entity.code.CompareTo("2006") == 0 || entity.code.CompareTo("9999 ") == 0)
                {
                    Logging.ManagementLogger.ErrorFormat("GetInfoRequest => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
                else
                {
                    string status = string.Empty;
                    switch (entity.code)
                    {
                        case "BILLING": status = "Thanh toán thành công"; break;
                        case "OFF": status = "Giao dịch đang được giữ"; break;
                        case "EDONG_OTHER": status = "Hóa đơn được thanh toán bởi ví khác"; break;
                        case "SOURCE_OTHER": status = "Hóa đơn được thanh toán bởi đối tác khác"; break;
                        case "TIMEOUT": status = "Timeout-Giao dịch nghi ngờ"; break;
                        case "ERROR": status = "Thanh toán lỗi"; break;
                        default: status = "Hủy hóa đơn"; break;
                    }
                    if (entity.code.CompareTo("BILLING") == 0)
                        return Json(new { Result = "SUCCESS", check = 1, status = status });
                    else if (entity.code.CompareTo("OFF") == 0 || entity.code.CompareTo("TIMEOUT") == 0)
                        return Json(new { Result = "SUCCESS", check = 2, status = status });
                    else if (entity.code.CompareTo("REVERT") == 0)
                        return Json(new { Result = "ERROR", Message = "Hóa đơn đã bị hủy" });
                    else
                        return Json(new { Result = "SUCCESS", check = 0, status = status });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetInfoRequest => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditCancelRequest(string id, string index, string check, string status)
        {
            CancelRequestModel model = new CancelRequestModel();
            List<SelectListItem> items = new List<SelectListItem>();
            if (int.Parse(check) == 1)
            {
                items.Add(new SelectListItem { Value = "4", Text = "Chuyển sang ĐVKT" });
                //items.Add(new SelectListItem { Value = "3", Text = "Từ chối" });
            }
            else if (int.Parse(check) == 2)
            {
                items.Add(new SelectListItem { Value = "0", Text = "Đồng ý" });
                //items.Add(new SelectListItem { Value = "3", Text = "Từ chối" });
            }
            //else
            //{
            //    items.Add(new SelectListItem { Value = "3", Text = "Từ chối" });
            //}
            model.DecidedList = items;
            ViewBag.idRequest = id;
            ViewBag.Index = index;
            ViewBag.status = Server.UrlDecode(status);
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult UpdateCancelRequest(int index, string id, string status, string desc, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                responseEntity entity = ePosDAO.ApproveTransactionCancellation(id, status, desc, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.RemoveAt(index);
                    return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items, id = items.Count() - 1 > index == true ? items.Count() - 1 : index });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateCancelRequest => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateCancelRequest => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        #endregion

        #region Chuyen ton
        public ActionResult TransferSurvive()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.TRANFERSURVIVE_TITLE;
            ViewBag.TitleLeft = "Chuyển tồn";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            TransferSurviveModel model = new TransferSurviveModel();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ManagementLogger.ErrorFormat("TransferSurvive => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = PCList;
            model.PCCode = PCList.ElementAt(0).Value;
            responseEntity resEntity = new responseEntity();
            List<SelectListItem> AccList = new List<SelectListItem>();
            AccList.Add(new SelectListItem { Value = "", Text = "--Chọn ví TNV--" });

            resEntity = ePosDAO.getChildAccFromSession(posAccount);
            //resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
            if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
            {
                foreach (var acc in resEntity.listAccount)
                {
                    AccList.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
                }
            }
            else
            {
                Logging.ManagementLogger.ErrorFormat("TransferSurvive => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            model.AccList = AccList;
            model.FromDate = PhoneNumber.GetFirstDayOfMonth();
            model.ToDate = DateTime.Now.ToString("dd/MM/yyyy");
            return View(model);
        }

        [HttpPost]
        public JsonResult getTransferSurvive(string pcCode, string bookCMIS, string edong, string FromDate, string ToDate)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_TransferSurvive(FromDate, ToDate);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.getTransferSurvive(pcCode, bookCMIS, edong, FromDate, ToDate, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        List<ObjReport> items = new List<ObjReport>();
                        int i = 1;
                        foreach (var item in entity.listBookcmisAccountMomentary)
                        {
                            ObjReport row = new ObjReport();
                            row.col_1 = i++.ToString();
                            row.col_2 = item.pcName;
                            row.col_3 = item.bookCmis;
                            row.col_4 = item.edong;
                            row.col_5 = item.strDateExpired;
                            row.col_6 = item.totalCustomer.ToString("N0");
                            row.col_7 = item.totalBacklogBill.ToString("N0");
                            row.col_8 = item.pcCode;
                            row.col_9 = item.id.ToString();
                            items.Add(row);
                        }
                        return Json(new { Result = "SUCCESS", Records = items });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("getTransferSurvive => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("getTransferSurvive => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getTransferSurvive => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddTransferSurvive()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            TransferSurviveModel model = new TransferSurviveModel();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ManagementLogger.ErrorFormat("TransferSurvive => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);

            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = PCList;
            List<SelectListItem> AccList = new List<SelectListItem>();
            AccList.Add(new SelectListItem { Value = "", Text = "--Chọn ví TNV--" });
            responseEntity resEntity = ePosDAO.getChildAccFromSession(posAccount);
            //responseEntity resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
            if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
            {
                foreach (var acc in resEntity.listAccount)
                {
                    AccList.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
                }
            }
            else
            {
                Logging.ManagementLogger.ErrorFormat("TransferSurvive => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);

            }
            model.AccList = AccList;
            ViewBag.date = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
            return PartialView(model);
        }

        public JsonResult getInfoBookCMIS(string pcCode, string bookCMIS)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getBookCMISbyPC(pcCode, PhoneNumber.ProcessCustomerGroup(bookCMIS), posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    int index = 0;
                    int count_bill = 0;
                    if (entity.listBookCmis == null)
                    {
                        return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào" });
                    }
                    foreach (var item in (from i in entity.listBookCmis orderby i.bookCmis1 select i))
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = (++index).ToString();
                        row.col_2 = item.bookCmis1;
                        row.col_3 = item.countCustomer.ToString("N0");
                        row.col_4 = item.countBill.ToString("N0");
                        count_bill = count_bill + item.countBill;
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items, count = count_bill, pc = pcCode });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("getInfoBookCMIS => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getInfoBookCMIS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult doAddTransferSurvive(string pcCode, string data, string source, string account, string date)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_AddTransferSurvive(account, date, data);
                if (string.IsNullOrEmpty(result))
                {
                    List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(source);
                    var data_array = data.Substring(1, data.Length - 2).Split(',');
                    string temp_book = string.Empty;
                    foreach (string i in data_array)
                    {
                        if (int.Parse(items.ElementAt(int.Parse(i)).col_4) != 0)
                            temp_book = string.IsNullOrEmpty(temp_book) == true ? items.ElementAt(int.Parse(i)).col_2 : temp_book + "," + items.ElementAt(int.Parse(i)).col_2;
                    }
                    if (!string.IsNullOrEmpty(temp_book))
                    {
                        responseEntity entity = ePosDAO.AddTransferSurvive(pcCode, PhoneNumber.ProcessCustomerGroup(temp_book), account, date, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            return Json(new { Result = "SUCCESS", Message = "Thêm mới bản ghi thành công" });
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("doAddTransferSurvive => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("doAddTransferSurvive => User: {0}, Error: Không có hóa đơn chuyển tồn!, Session: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Không có hóa đơn chuyển tồn!" });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("doAddTransferSurvive => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doAddTransferSurvive => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult doDelTransferSurvive(string datasource, string id, int index)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                responseEntity entity = ePosDAO.DeleteBookcmisAccountMomentary(id, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.RemoveAt(index);
                    return Json(new { Result = "SUCCESS", Message = "Xóa bản ghi thành công", Records = items });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("doDelTransferSurvive => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doDelTransferSurvive => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        #endregion

        #region Khanh Hoa
        [AllowAnonymous]
        public ActionResult EVNKH()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.CONTROLDEBT_TITLE;
            ViewBag.TitleLeft = "Khánh Hòa";
            return View();
        }

        [HttpPost]
        public JsonResult SearchEVNKH(string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ePosAccount tempAccount = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (string.IsNullOrEmpty(pcCode))
                {
                    List<ObjEVNPC> items = new List<ObjEVNPC>();
                    items = (from x in tempAccount.EvnPC where x.ext.StartsWith("PQ") select x).OrderBy(c => c.ext).ToList();
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    if (pcCode.ToUpper().StartsWith("PQ"))
                    {
                        List<ObjEVNPC> items = new List<ObjEVNPC>();
                        items = (from x in tempAccount.EvnPC where x.ext.StartsWith(pcCode.ToUpper()) select x).ToList();
                        return Json(new { Result = "SUCCESS", Records = items });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("SearchEVNHP => User: {0}, Error: Mã điện lực không đúng định dạng, Session: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Mã điện lực không đúng. Vui lòng kiểm tra lại" });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchEVNKH => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult CheckEVNKH(int status, string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getKHAssignInfo(status.ToString(), pcCode, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", Message = "Thực hiện nghiệp vụ thành công" });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("CheckEVNKH => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("CheckEVNKH => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _HistoryEVNKH(string pc)
        {
            ViewBag.pcCode = pc;
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetHistoryEVNKH(string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = new List<ObjReport>();
                responseEntity entity = ePosDAO.getKHAssignInfo("3", pcCode, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listAssignBillLog.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listAssignBillLog.ElementAt(i).id.ToString();
                        row.col_2 = entity.listAssignBillLog.ElementAt(i).pcCode;
                        row.col_3 = entity.listAssignBillLog.ElementAt(i).cashierCode;
                        row.col_4 = entity.listAssignBillLog.ElementAt(i).totalbill.ToString("N0");
                        row.col_5 = entity.listAssignBillLog.ElementAt(i).totalAmount.ToString("N0");
                        row.col_6 = entity.listAssignBillLog.ElementAt(i).strStartDate;
                        items.Add(row);
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("GetHistoryEVNKH => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                }
                return Json(new { Result = "SUCCESS", Records = items });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetHistoryEVNKH => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _HistoryDetailEVNKH(string pc, string id)
        {
            ViewBag.pc = pc;
            ViewBag.id = id;
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetDetailHistoryKH(string id, string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = new List<ObjReport>();
                responseEntity entity = ePosDAO.getHPAssignInfo("4", pcCode, id, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listAssignBillLog.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listAssignBillLog.ElementAt(i).id.ToString();
                        row.col_2 = entity.listAssignBillLog.ElementAt(i).pcCode;
                        row.col_3 = entity.listAssignBillLog.ElementAt(i).cashierCode;
                        row.col_4 = entity.listAssignBillLog.ElementAt(i).totalbill.ToString("N0");
                        row.col_5 = entity.listAssignBillLog.ElementAt(i).totalAmount.ToString("N0");
                        row.col_6 = entity.listAssignBillLog.ElementAt(i).strStartDate;
                        items.Add(row);
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("GetDetailHistoryKH => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                }
                return Json(new { Result = "SUCCESS", Records = items });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetDetailHistoryKH => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        #endregion

        #region Hai Phong
        [AllowAnonymous]
        public ActionResult EVNHP()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.CONTROLDEBT_TITLE;
            ViewBag.TitleLeft = "Hải Phòng";
            return View();
        }

        [HttpPost]
        public JsonResult SearchEVNHP(string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            if (ePOSSession.GetObject(posAccount.session) == null)
            {
                Logging.ManagementLogger.ErrorFormat("SearchEVNHP => User: {0}, Error: Phiên làm việc không tồn tại, Session: {1}", posAccount.edong, posAccount.session);
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            }
            try
            {
                ePosAccount tempAccount = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (string.IsNullOrEmpty(pcCode))
                {
                    List<ObjEVNPC> items = new List<ObjEVNPC>();
                    items = (from x in tempAccount.EvnPC where x.ext.StartsWith("PH") select x).OrderBy(c => c.ext).ToList();
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    if (pcCode.ToUpper().StartsWith("PH"))
                    {
                        List<ObjEVNPC> items = new List<ObjEVNPC>();
                        items = (from x in tempAccount.EvnPC where x.ext.StartsWith(pcCode.ToUpper()) select x).ToList();
                        return Json(new { Result = "SUCCESS", Records = items });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("SearchEVNHP => User: {0}, Error: Mã điện lực không đúng định dạng, Session: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Mã điện lực không đúng. Vui lòng kiểm tra lại" });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchEVNHP => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult CheckEVNHP(int status, string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getHPAssignInfo(status.ToString(), pcCode, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", Message = "Thực hiện nghiệp vụ thành công" });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("CheckEVNHP => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("CheckEVNHP => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _HistoryEVNHP(string pc)
        {
            ViewBag.pcCode = pc;
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetHistoryEVNHP(string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = new List<ObjReport>();
                responseEntity entity = ePosDAO.getHPAssignInfo("3", pcCode, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listAssignBillLog.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listAssignBillLog.ElementAt(i).id.ToString();
                        row.col_2 = entity.listAssignBillLog.ElementAt(i).pcCode;
                        row.col_3 = entity.listAssignBillLog.ElementAt(i).cashierCode;
                        row.col_4 = entity.listAssignBillLog.ElementAt(i).totalbill.ToString("N0");
                        row.col_5 = entity.listAssignBillLog.ElementAt(i).totalAmount.ToString("N0");
                        row.col_6 = entity.listAssignBillLog.ElementAt(i).strStartDate;
                        items.Add(row);
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("GetHistoryEVNHP => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                }
                return Json(new { Result = "SUCCESS", Records = items });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetHistoryEVNHP => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _HistoryDetailEVNHP(string pc, string id)
        {
            ViewBag.pc = pc;
            ViewBag.id = id;
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetDetailHistoryHP(string id, string pcCode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = new List<ObjReport>();
                responseEntity entity = ePosDAO.getHPAssignInfo("4", pcCode, id, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    for (int i = 0; i < entity.listAssignBillLog.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listAssignBillLog.ElementAt(i).id.ToString();
                        row.col_2 = entity.listAssignBillLog.ElementAt(i).pcCode;
                        row.col_3 = entity.listAssignBillLog.ElementAt(i).cashierCode;
                        row.col_4 = entity.listAssignBillLog.ElementAt(i).totalbill.ToString("N0");
                        row.col_5 = entity.listAssignBillLog.ElementAt(i).totalAmount.ToString("N0");
                        row.col_6 = entity.listAssignBillLog.ElementAt(i).strStartDate;
                        items.Add(row);
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("GetDetailHistoryHP => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                }
                return Json(new { Result = "SUCCESS", Records = items });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetDetailHistoryHP => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        #endregion

        #region Ho Chi Minh
        [AllowAnonymous]
        public ActionResult EVNHCM()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.CONTROLDEBT_TITLE;
            ViewBag.TitleLeft = "Hồ Chí Minh";
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ControlDebtModel model = new ControlDebtModel();
            if (tempPosAcc.EvnPC == null || (from x in tempPosAcc.EvnPC where x.ext.StartsWith("PE") select x) == null)
            {
                Logging.ManagementLogger.ErrorFormat("EVNHCM => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in (from x in tempPosAcc.EvnPC where x.ext.StartsWith("PE") select x).ToList())
            {
                items.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = items;
            return View(model);
        }

        [HttpPost]
        public JsonResult searchAssignHCM(string pcCode, string date)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.getAssignHCM(pcCode, date, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    List<ObjReport> items = new List<ObjReport>();
                    for (int i = 0; i < entity.listDanhSach.Count(); i++)
                    {
                        ObjReport row = new ObjReport();
                        row.col_1 = entity.listDanhSach.ElementAt(i).MA_DVIQLY;
                        row.col_2 = entity.listDanhSach.ElementAt(i).MA_SOGCS;
                        row.col_3 = entity.listDanhSach.ElementAt(i).SOLUONG_HDON;
                        items.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = items });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("searchAssignHCM => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("searchAssignHCM => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult GetAssignBookCMIS(string pcCode, string bookCMIS)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = ePosDAO.GetAssignBookCMIS(pcCode, bookCMIS, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    return Json(new { Result = "SUCCESS", Message = "Tải dữ liệu thành công: <br> - Tổng số hóa đơn: " + entity.totalBill.ToString("N0") + "<br> - Tổng tiền: " + entity.totalAmount.ToString("N0") });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("GetAssignBookCMIS => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = "Tải dữ liệu không thành công ở thời điểm này" });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("GetAssignBookCMIS => User: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        #endregion

        //#region Hóa đơn đang xử lý chấm nợ
        //[AllowAnonymous]
        //public ActionResult BillHandling()
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return RedirectToAction("Login", "ePOS", true);
        //    ViewBag.Title = Constant.BILLHANDLING_TITLE;
        //    ViewBag.TitleLeft = "Hóa đơn đang XLCN";
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
        //    BillHandlingModel model = new BillHandlingModel();
        //    if (tempPosAcc.EvnPC == null)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("BillHandling => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
        //        return RedirectToAction("Login", "ePOS", true);
        //    }
        //    List<SelectListItem> PCList = new List<SelectListItem>();
        //    PCList.Add(new SelectListItem { Value = "", Text = "--Chọn điện lực--" });
        //    //foreach (var item in (tempPosAcc.EvnPC.Where(x => !Constant.EVN().Any(p=> p.Key == x.pcId.ToString()))))
        //    foreach (var item in tempPosAcc.EvnPC)
        //    {
        //        PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
        //    }
        //    model.PCList = PCList;
        //    List<SelectListItem> AccList = new List<SelectListItem>();
        //    AccList.Add(new SelectListItem { Value = "", Text = "--Chọn ví TNV--" });
        //    //NPHAN 2018-04-10
        //    //responseEntity resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
        //    responseEntity resEntity = ePosDAO.getChildAccFromSession(posAccount);
        //    if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //    {
        //        foreach (var acc in resEntity.listAccount)
        //        {
        //            AccList.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
        //        }
        //    }
        //    else
        //    {
        //        Logging.ManagementLogger.ErrorFormat("BillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
        //        return RedirectToAction("Login", "ePOS", true);
        //    }
        //    model.AccList = AccList;
        //    List<SelectListItem> TypeList = new List<SelectListItem>();
        //    TypeList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
        //    foreach (var item in Constant.BillHandlingType())
        //    {
        //        TypeList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.TypeList = TypeList;
        //    List<SelectListItem> StatusList = new List<SelectListItem>();
        //    StatusList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
        //    foreach (var item in Constant.BillHandlingStatus())
        //    {
        //        StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.StatusList = StatusList;
        //    model.Todate = model.Fromdate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
        //    ViewBag.DateTime = DateTime.Now.ToString("HH:mm:ss");
        //    return View(model);
        //}

        //[HttpPost]
        //public JsonResult SearchBillHandling (string pcCode, string account, string customer, string type, string status, string fromdate, string fromtime, string todate, string totime,
        //    int pagenum = 0, int pagesize = 50)
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        string result = Validate.check_SearchBillHandling(pcCode, customer, fromdate, fromtime, todate, totime);
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
        //            List<ObjReport> items = new List<ObjReport>();
        //            decimal total_amount = 0;
        //            string date = DateTime.Now.ToString();
        //            int total_bill = 0;
        //            if (!string.IsNullOrEmpty(customer))
        //            {
        //                responseEntity entity = ePosDAO.getTransactionOff(string.Empty, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                {                           
        //                    List<PendingReportItem> temp = JsonConvert.DeserializeObject<List<PendingReportItem>>(CompressUtil.DecryptBase64(entity.outputZip), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //                    items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        //                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //                }
        //                else
        //                {
        //                    Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //                }
        //            }
        //            else if(!string.IsNullOrEmpty(pcCode))
        //            {
        //                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
        //                List<ObjEVNPC> data = tempPosAcc.EvnPC;
        //                var pc_parent = (from x in tempPosAcc.EvnPC where x.ext == pcCode select x).FirstOrDefault();
        //                var pc_child = pcCode.CompareTo("PA") == 0 ?
        //                    tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId && !x.ext.Contains("PH")).ToList() :
        //                    tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId).ToList();
        //                if (pc_child == null || pc_child.Count() == 0)
        //                {
        //                    responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                    {                                
        //                        items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        //                        ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //                    }
        //                    else
        //                    {
        //                        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (var pc in pc_child)
        //                    {
        //                        decimal temp_total_amount = 0;
        //                        int temp_total_bill = 0;
        //                        responseEntity entity = ePosDAO.getTransactionOff(pc.ext, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                        {                                    
        //                            items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref temp_total_amount, ref temp_total_bill);
        //                            total_amount = total_amount + temp_total_amount;
        //                            total_bill = total_bill + temp_total_bill;
        //                        }
        //                        else
        //                        {                                   
        //                            Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, pc: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, pc.ext, entity.code, entity.description, posAccount.session);
        //                        }
        //                    }
        //                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //                }
        //            }
        //            else
        //            {
        //                responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                {                            
        //                    items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        //                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //                }
        //                else
        //                {
        //                    Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //                }
        //            }
        //            if (items.Count > 0)
        //            {
        //                List<ObjReport> data = new List<ObjReport>();
        //                foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
        //                {
        //                    ObjReport row = new ObjReport();
        //                    row.col_1 = item.col_1;
        //                    row.col_2 = item.col_2;
        //                    row.col_3 = item.col_3;
        //                    row.col_4 = item.col_4;
        //                    row.col_5 = item.col_5;
        //                    row.col_6 = item.col_6;
        //                    row.col_7 = item.col_7;
        //                    row.col_8 = item.col_8;
        //                    row.col_9 = item.col_9;
        //                    row.col_10 = item.col_10;
        //                    row.col_11 = item.col_11;
        //                    row.col_12 = item.col_12;
        //                    row.col_13 = item.col_13;
        //                    row.col_14 = item.col_14;
        //                    row.col_15 = item.col_15;
        //                    row.col_16 = item.col_16;
        //                    row.col_17 = item.col_17;
        //                    row.col_18 = item.col_18;
        //                    row.col_19 = item.col_19;
        //                    row.col_20 = item.col_20;
        //                    row.col_0 = total_bill.ToString();
        //                    data.Add(row);
        //                }
        //                int _PageLast = 0;
        //                if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST) != null)
        //                {
        //                    _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST));
        //                }
        //                ePOSSession.AddObject(Constant.PAGE_SIZE_LAST, pagesize);
        //                int countItem = data.Count();
        //                if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
        //                {
        //                    for (int i = 0; i < pagesize; i++)
        //                    {
        //                        ObjReport row = new ObjReport();
        //                        row.col_1 = string.Empty;
        //                        row.col_2 = string.Empty;
        //                        row.col_3 = string.Empty;
        //                        row.col_4 = string.Empty;
        //                        row.col_5 = string.Empty;
        //                        row.col_6 = string.Empty;
        //                        row.col_7 = string.Empty;
        //                        row.col_8 = string.Empty;
        //                        row.col_9 = string.Empty;
        //                        row.col_10 = string.Empty;
        //                        row.col_11 = string.Empty;
        //                        row.col_12 = string.Empty;
        //                        row.col_13 = string.Empty;
        //                        row.col_14 = string.Empty;
        //                        row.col_15 = string.Empty;
        //                        row.col_16 = string.Empty;
        //                        row.col_17 = string.Empty;
        //                        row.col_18 = string.Empty;
        //                        row.col_19 = string.Empty;
        //                        row.col_20 = string.Empty;
        //                        row.col_0 = total_bill.ToString();
        //                        data.Insert(0, row);
        //                    }
        //                }
        //                return Json(new
        //                {
        //                    Result = "SUCCESS",
        //                    Records = data,
        //                    amount = total_amount.ToString("N0"),
        //                    total_bill = total_bill.ToString("N0"),
        //                    Message = Constant.REPORT_BILLHANDLING + date,
        //                    pagesize = pagesize                           
        //                });
        //            }
        //            else
        //            {
        //                return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
        //            }
        //        }
        //        else
        //        {                   
        //            Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = result });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }            
        //}

        ////[HttpPost]
        ////public JsonResult SearchBillHandling(string pcCode, string account, string customer, string type, string status, string fromdate, string fromtime, string todate,
        ////    string totime, string curentpage, string CustomerChoice, int pagenum = 0, int pagesize = 50)
        ////{
        ////    if (!ePOSController.CheckSession(HttpContext))
        ////        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        ////    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        ////    try
        ////    {
        ////        string result = Validate.check_SearchBillHandling(pcCode, customer, fromdate, fromtime, todate, totime);
        ////        if (string.IsNullOrEmpty(result))
        ////        {
        ////            Dictionary<string, string> dcustomerPage = new Dictionary<string, string>();
        ////            string _CustomerSelected = string.Empty;
        ////            if (ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED) != null)
        ////            {
        ////                dcustomerPage = (Dictionary<string, string>)ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
        ////                if (string.IsNullOrEmpty(curentpage))
        ////                {
        ////                    if (!dcustomerPage.ContainsKey(pagenum.ToString()) && !string.IsNullOrEmpty(CustomerChoice))
        ////                    {
        ////                        Dictionary<string, string> dCustomerChoiced = new Dictionary<string, string>();
        ////                        string[] arr = CustomerChoice.Split(';');
        ////                        if (arr != null && arr.Length > 0)
        ////                        {
        ////                            for (int i = 0; i < arr.Length; i++)
        ////                            {
        ////                                if (!string.IsNullOrEmpty(arr[i]))
        ////                                    if (!dCustomerChoiced.ContainsKey(arr[i]))
        ////                                        dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
        ////                            }
        ////                        }
        ////                        dcustomerPage.Add(pagenum.ToString(), string.Join(";", dCustomerChoiced.Select(p => p.Key)));
        ////                    }
        ////                    else
        ////                    {
        ////                        if (!string.IsNullOrEmpty(CustomerChoice))
        ////                        {
        ////                            Dictionary<string, string> dCustomerChoiced = new Dictionary<string, string>();
        ////                            string[] arr = CustomerChoice.Split(';');
        ////                            if (arr != null && arr.Length > 0)
        ////                            {
        ////                                for (int i = 0; i < arr.Length; i++)
        ////                                {
        ////                                    if (!string.IsNullOrEmpty(arr[i]))
        ////                                        if (!dCustomerChoiced.ContainsKey(arr[i]))
        ////                                            dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
        ////                                }
        ////                            }
        ////                            if (dcustomerPage.ContainsKey(pagenum.ToString()))
        ////                                dcustomerPage[pagenum.ToString()] = string.Join(";", dCustomerChoiced.Select(p => p.Key));
        ////                            else
        ////                                dcustomerPage.Add(pagenum.ToString(), string.Join(";", dCustomerChoiced.Select(p => p.Key)));
        ////                            ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
        ////                        }                                
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    if (!string.IsNullOrEmpty(curentpage))
        ////                    {
        ////                        if (!dcustomerPage.ContainsKey(curentpage))
        ////                        {
        ////                            dcustomerPage.Add(curentpage, CustomerChoice);
        ////                        }
        ////                    }
        ////                    if (dcustomerPage != null && dcustomerPage.Count > 0)
        ////                    {
        ////                        ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
        ////                    }
        ////                } 
        ////            }
        ////            else
        ////            {
        ////                Dictionary<string, string> dChoice = new Dictionary<string, string>();
        ////                if (!string.IsNullOrEmpty(CustomerChoice))
        ////                {
        ////                    string[] arr = CustomerChoice.Split(';');
        ////                    if (arr != null && arr.Length > 0)
        ////                    {
        ////                        for (int i = 0; i < arr.Length; i++)
        ////                        {
        ////                            if (!string.IsNullOrEmpty(arr[i]))
        ////                                dChoice.Add(arr[i].ToString(), arr[i]);
        ////                        }
        ////                    }
        ////                }
        ////                if (dChoice != null && dChoice.Count() > 0)
        ////                    _CustomerSelected = string.Join(";", dChoice.Select(p => p.Key));
        ////                dcustomerPage.Add(pagenum.ToString(), _CustomerSelected);
        ////                ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
        ////            }
        ////            if (dcustomerPage != null)
        ////                foreach (var item in dcustomerPage)
        ////                    if (!string.IsNullOrEmpty(item.Value))
        ////                        _CustomerSelected = string.IsNullOrEmpty(_CustomerSelected) == true ? item.Value : _CustomerSelected + "," + item.Value;
        ////            List<ObjReport> items = new List<ObjReport>();
        ////            decimal total_amount = 0;
        ////            int total_bill = 0;
        ////            string date = DateTime.Now.ToString();
        ////            int check = 0;
        ////            string error_msg = string.Empty;
        ////            if (!string.IsNullOrEmpty(customer))
        ////            {
        ////                responseEntity entity = ePosDAO.getTransactionOff(string.Empty, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);

        ////                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        ////                {

        ////                    // string temp = CompressUtil.DecryptBase64(entity.outputZip);
        ////                    List<PendingReportItem> temp = JsonConvert.DeserializeObject<List<PendingReportItem>>(CompressUtil.DecryptBase64(entity.outputZip), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        ////                    items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        ////                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        ////                }
        ////                else
        ////                {
        ////                    Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        ////                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        ////                }

        ////            }                    
        ////            else if(!string.IsNullOrEmpty(pcCode))
        ////            {
        ////                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
        ////                List<ObjEVNPC> data = tempPosAcc.EvnPC;
        ////                var pc_parent = (from x in tempPosAcc.EvnPC where x.ext == pcCode select x).FirstOrDefault();
        ////                var pc_child = pcCode.CompareTo("PA") == 0 ?
        ////                    tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId && !x.ext.Contains("PH")).ToList() :
        ////                    tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId).ToList();
        ////                if(pc_child == null || pc_child.Count() == 0)
        ////                {
        ////                    responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        ////                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        ////                    {
        ////                        //items = ReadFile.FillDataBillHanding(items, entity.listTransactionOff, ref total_amount, ref total_bill);
        ////                        items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        ////                        ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        ////                    }
        ////                    else
        ////                    {
        ////                        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        ////                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    foreach(var pc in pc_child)
        ////                    {
        ////                        decimal temp_total_amount = 0;
        ////                        int temp_total_bill = 0;
        ////                        responseEntity entity = ePosDAO.getTransactionOff(pc.ext, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        ////                       if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        ////                        {
        ////                            //items = ReadFile.FillDataBillHanding(items, entity.listTransactionOff, ref temp_total_amount, ref temp_total_bill);
        ////                            items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref temp_total_amount, ref temp_total_bill);
        ////                            total_amount = total_amount + temp_total_amount;
        ////                            total_bill = total_bill + temp_total_bill;
        ////                        }
        ////                        else
        ////                        {
        ////                            check = 1;
        ////                            error_msg = string.IsNullOrEmpty(error_msg) == true ? pc.ext : error_msg + "; " + pc.ext;
        ////                            Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, pc: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, pc.ext, entity.code, entity.description, posAccount.session);
        ////                        }
        ////                    }
        ////                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        ////                }
        ////            }else
        ////            {
        ////                responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        ////                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        ////                {
        ////                     //items = ReadFile.FillDataBillHanding(items, entity.listTransactionOff, ref total_amount, ref total_bill);
        ////                    items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        ////                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        ////                }
        ////                else
        ////                {
        ////                    Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        ////                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        ////                }
        ////            }
        ////            if (items.Count > 0)
        ////            {
        ////                List<ObjReport> data = new List<ObjReport>();                                          
        ////                foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
        ////                {
        ////                    ObjReport row = new ObjReport();
        ////                    row.col_1 = item.col_1;
        ////                    row.col_2 = item.col_2;
        ////                    row.col_3 = item.col_3;
        ////                    row.col_4 = item.col_4;
        ////                    row.col_5 = item.col_5;                           
        ////                    row.col_6 = item.col_6;
        ////                    row.col_7 = item.col_7;
        ////                    row.col_8 = item.col_8;
        ////                    row.col_9 = item.col_9;
        ////                    row.col_10 = item.col_10;
        ////                    row.col_11 = item.col_11;
        ////                    row.col_12 = item.col_12;
        ////                    row.col_13 = item.col_13;
        ////                    row.col_14 = item.col_14;
        ////                    row.col_15 = item.col_15;
        ////                    row.col_16 = item.col_16;
        ////                    row.col_17 = item.col_17;
        ////                    row.col_18 = item.col_18;
        ////                    row.col_19 = item.col_19;
        ////                    row.col_20 = item.col_20;
        ////                    row.col_0 = total_bill.ToString();
        ////                    data.Add(row);
        ////                }
        ////                int _PageLast = 0;
        ////                if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST) != null)
        ////                {
        ////                    _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST));
        ////                }
        ////                ePOSSession.AddObject(Constant.PAGE_SIZE_LAST, pagesize);
        ////                int countItem = data.Count();
        ////                if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
        ////                {
        ////                    for (int i = 0; i < pagesize; i++)
        ////                    {
        ////                        ObjReport row = new ObjReport();
        ////                        row.col_1 = i.ToString();
        ////                        row.col_2 = i.ToString();
        ////                        row.col_3 = i.ToString();
        ////                        row.col_4 = i.ToString();
        ////                        row.col_5 = i.ToString();
        ////                        row.col_6 = i.ToString();
        ////                        row.col_7 = i.ToString();
        ////                        row.col_8 = i.ToString();
        ////                        row.col_9 = i.ToString();
        ////                        row.col_10 = i.ToString();
        ////                        row.col_11 = i.ToString();
        ////                        row.col_12 = i.ToString();
        ////                        row.col_13 = i.ToString();
        ////                        row.col_14 = i.ToString();
        ////                        row.col_15 = i.ToString();
        ////                        row.col_16 = i.ToString();
        ////                        row.col_17 = i.ToString();
        ////                        row.col_18 = i.ToString();
        ////                        row.col_19 = i.ToString();
        ////                        row.col_20 = i.ToString();
        ////                        row.col_0 = total_bill.ToString();
        ////                        data.Insert(0, row);
        ////                    }
        ////                }
        ////                return Json(new { Result = "SUCCESS", Records = data, amount = total_amount.ToString("N0"), total_bill = total_bill.ToString("N0"),
        ////                    Message = Constant.REPORT_BILLHANDLING + date, CustomerSelected = _CustomerSelected, check = check,
        ////                    ErrorMsg = string.IsNullOrEmpty(error_msg) == true? string.Empty : "Hóa đơn đang xử lý: " + error_msg +". Không lấy được thông tin"});
        ////            }
        ////            else
        ////            {
        ////                return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
        ////            }
        ////        }
        ////        else
        ////        {
        ////            List<ObjReport> items = new List<ObjReport>();
        ////            Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
        ////            return Json(new { Result = "ERROR", Message = result });
        ////        }                       
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        ////        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        ////    }
        ////}        

        ////[HttpPost]
        ////public JsonResult BillHandlingContainer_PageChange(string id, string CustomerChoice, int pagesize_old = 0, int pagenum = 0, int pagesize = 50)
        ////{
        ////    if (!ePOSController.CheckSession(HttpContext))
        ////        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        ////    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        ////    try
        ////    {
        ////        if(ePOSSession.GetObject(id) == null)
        ////        {
        ////            Logging.ManagementLogger.ErrorFormat("BillHandlingContainer_PageChange => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, "Phiên làm việc không tồn tại");
        ////            return Json(new { Result = "ERROR", Message = "Phiên làm việc không tồn tại" });
        ////        }
        ////        else
        ////        {
        ////            List<ObjReport> data = new List<ObjReport>();
        ////            if (pagesize_old != pagesize)
        ////            {                       
        ////                ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
        ////                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
        ////                long total_bill = items.Count();
        ////                foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
        ////                {
        ////                    ObjReport row = new ObjReport();
        ////                    row.col_1 = item.col_1;
        ////                    row.col_2 = item.col_2;
        ////                    row.col_3 = item.col_3;
        ////                    row.col_4 = item.col_4;
        ////                    row.col_5 = item.col_5;
        ////                    row.col_6 = item.col_6;
        ////                    row.col_7 = item.col_7;
        ////                    row.col_8 = item.col_8;
        ////                    row.col_9 = item.col_9;
        ////                    row.col_10 = item.col_10;
        ////                    row.col_11 = item.col_11;
        ////                    row.col_12 = item.col_12;
        ////                    row.col_13 = item.col_13;
        ////                    row.col_14 = item.col_14;
        ////                    row.col_15 = item.col_15;
        ////                    row.col_16 = item.col_16;
        ////                    row.col_17 = item.col_17;
        ////                    row.col_18 = item.col_18;
        ////                    row.col_19 = item.col_19;
        ////                    row.col_20 = item.col_20;
        ////                    row.col_0 = total_bill.ToString();
        ////                    data.Add(row);
        ////                }
        ////                int _PageLast = 0;
        ////                if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST) != null)
        ////                {
        ////                    _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST));
        ////                }
        ////                ePOSSession.AddObject(Constant.PAGE_SIZE_LAST, pagesize);
        ////                int countItem = data.Count();
        ////                if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
        ////                {
        ////                    for (int i = 0; i < pagesize; i++)
        ////                    {
        ////                        ObjReport row = new ObjReport();
        ////                        row.col_1 = string.Empty;
        ////                        row.col_2 = string.Empty;
        ////                        row.col_3 = string.Empty;
        ////                        row.col_4 = string.Empty;
        ////                        row.col_5 = string.Empty;
        ////                        row.col_6 = string.Empty;
        ////                        row.col_7 = string.Empty;
        ////                        row.col_8 = string.Empty;
        ////                        row.col_9 = string.Empty;
        ////                        row.col_10 = string.Empty;
        ////                        row.col_11 = string.Empty;
        ////                        row.col_12 = string.Empty;
        ////                        row.col_13 = string.Empty;
        ////                        row.col_14 = string.Empty;
        ////                        row.col_15 = string.Empty;
        ////                        row.col_16 = string.Empty;
        ////                        row.col_17 = string.Empty;
        ////                        row.col_18 = string.Empty;
        ////                        row.col_19 = string.Empty;
        ////                        row.col_20 = string.Empty;
        ////                        row.col_0 = total_bill.ToString();
        ////                        data.Insert(0, row);
        ////                    }
        ////                }
        ////                return Json(new
        ////                {
        ////                    Result = "SUCCESS",
        ////                    Records = data,
        ////                    total_bill = total_bill.ToString("N0"),
        ////                    CustomerSelected = string.Empty
        ////                });
        ////            }
        ////            else
        ////            {
        ////                Dictionary<string, string> dcustomerPage = new Dictionary<string, string>();
        ////                string _CustomerSelected = string.Empty;
        ////                if (ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED) != null)
        ////                {
        ////                    dcustomerPage = (Dictionary<string, string>)ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
        ////                    if (string.IsNullOrEmpty(curentpage))
        ////                    {
        ////                        if (!dcustomerPage.ContainsKey(pagenum.ToString()) && !string.IsNullOrEmpty(CustomerChoice))
        ////                        {
        ////                            Dictionary<string, string> dCustomerChoiced = new Dictionary<string, string>();
        ////                            string[] arr = CustomerChoice.Split(';');
        ////                            if (arr != null && arr.Length > 0)
        ////                            {
        ////                                for (int i = 0; i < arr.Length; i++)
        ////                                {
        ////                                    if (!string.IsNullOrEmpty(arr[i]))
        ////                                        if (!dCustomerChoiced.ContainsKey(arr[i]))
        ////                                            dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
        ////                                }
        ////                            }
        ////                            dcustomerPage.Add(pagenum.ToString(), string.Join(";", dCustomerChoiced.Select(p => p.Key)));
        ////                        }
        ////                        else
        ////                        {
        ////                            if (!string.IsNullOrEmpty(CustomerChoice))
        ////                            {
        ////                                Dictionary<string, string> dCustomerChoiced = new Dictionary<string, string>();
        ////                                string[] arr = CustomerChoice.Split(';');
        ////                                if (arr != null && arr.Length > 0)
        ////                                {
        ////                                    for (int i = 0; i < arr.Length; i++)
        ////                                    {
        ////                                        if (!string.IsNullOrEmpty(arr[i]))
        ////                                            if (!dCustomerChoiced.ContainsKey(arr[i]))
        ////                                                dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
        ////                                    }
        ////                                }
        ////                                if (dcustomerPage.ContainsKey(pagenum.ToString()))
        ////                                    dcustomerPage[pagenum.ToString()] = string.Join(";", dCustomerChoiced.Select(p => p.Key));
        ////                                else
        ////                                    dcustomerPage.Add(pagenum.ToString(), string.Join(";", dCustomerChoiced.Select(p => p.Key)));
        ////                                ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
        ////                            }else
        ////                            {
        ////                                if (!string.IsNullOrEmpty(curentpage))
        ////                                {
        ////                                    if (!dcustomerPage.ContainsKey(curentpage))
        ////                                    {
        ////                                        dcustomerPage.Add(curentpage, CustomerChoice);
        ////                                    }
        ////                                }
        ////                                if (dcustomerPage != null && dcustomerPage.Count > 0)
        ////                                {
        ////                                    ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
        ////                                }
        ////                            }
        ////                        }
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    Dictionary<string, string> dChoice = new Dictionary<string, string>();
        ////                    if (!string.IsNullOrEmpty(CustomerChoice))
        ////                    {
        ////                        string[] arr = CustomerChoice.Split(';');
        ////                        if (arr != null && arr.Length > 0)
        ////                        {
        ////                            for (int i = 0; i < arr.Length; i++)
        ////                            {
        ////                                if (!string.IsNullOrEmpty(arr[i]))
        ////                                    dChoice.Add(arr[i].ToString(), arr[i]);
        ////                            }
        ////                        }
        ////                    }
        ////                    if (dChoice != null && dChoice.Count() > 0)
        ////                        _CustomerSelected = string.Join(";", dChoice.Select(p => p.Key));
        ////                    if (!string.IsNullOrEmpty(_CustomerSelected))
        ////                    {
        ////                        dcustomerPage.Add(pagenum.ToString(), _CustomerSelected);
        ////                        ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
        ////                    }                            
        ////                }
        ////                if (dcustomerPage != null)
        ////                    foreach (var item in dcustomerPage)
        ////                        if (!string.IsNullOrEmpty(item.Value))
        ////                            _CustomerSelected = string.IsNullOrEmpty(_CustomerSelected) == true ? item.Value : _CustomerSelected + "," + item.Value;

        ////                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
        ////                long total_bill = items.Count();
        ////                foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
        ////                {
        ////                    ObjReport row = new ObjReport();
        ////                    row.col_1 = item.col_1;
        ////                    row.col_2 = item.col_2;
        ////                    row.col_3 = item.col_3;
        ////                    row.col_4 = item.col_4;
        ////                    row.col_5 = item.col_5;
        ////                    row.col_6 = item.col_6;
        ////                    row.col_7 = item.col_7;
        ////                    row.col_8 = item.col_8;
        ////                    row.col_9 = item.col_9;
        ////                    row.col_10 = item.col_10;
        ////                    row.col_11 = item.col_11;
        ////                    row.col_12 = item.col_12;
        ////                    row.col_13 = item.col_13;
        ////                    row.col_14 = item.col_14;
        ////                    row.col_15 = item.col_15;
        ////                    row.col_16 = item.col_16;
        ////                    row.col_17 = item.col_17;
        ////                    row.col_18 = item.col_18;
        ////                    row.col_19 = item.col_19;
        ////                    row.col_20 = item.col_20;
        ////                    row.col_0 = total_bill.ToString();
        ////                    data.Add(row);
        ////                }
        ////                int _PageLast = 0;
        ////                if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST) != null)
        ////                {
        ////                    _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST));
        ////                }
        ////                ePOSSession.AddObject(Constant.PAGE_SIZE_LAST, pagesize);
        ////                int countItem = data.Count();
        ////                if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
        ////                {
        ////                    for (int i = 0; i < pagesize; i++)
        ////                    {
        ////                        ObjReport row = new ObjReport();
        ////                        row.col_1 = string.Empty;
        ////                        row.col_2 = string.Empty;
        ////                        row.col_3 = string.Empty;
        ////                        row.col_4 = string.Empty;
        ////                        row.col_5 = string.Empty;
        ////                        row.col_6 = string.Empty;
        ////                        row.col_7 = string.Empty;
        ////                        row.col_8 = string.Empty;
        ////                        row.col_9 = string.Empty;
        ////                        row.col_10 = string.Empty;
        ////                        row.col_11 = string.Empty;
        ////                        row.col_12 = string.Empty;
        ////                        row.col_13 = string.Empty;
        ////                        row.col_14 = string.Empty;
        ////                        row.col_15 = string.Empty;
        ////                        row.col_16 = string.Empty;
        ////                        row.col_17 = string.Empty;
        ////                        row.col_18 = string.Empty;
        ////                        row.col_19 = string.Empty;
        ////                        row.col_20 = string.Empty;
        ////                        row.col_0 = total_bill.ToString();
        ////                        data.Insert(0, row);
        ////                    }
        ////                }
        ////                return Json(new
        ////                {
        ////                    Result = "SUCCESS",
        ////                    Records = data,
        ////                    total_bill = total_bill.ToString("N0"),
        ////                    CustomerSelected = _CustomerSelected
        ////                });
        ////            } 
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Logging.ManagementLogger.ErrorFormat("BillHandlingContainer_PageChange => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        ////        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        ////    }           
        ////}

        //[HttpPost]
        //public JsonResult BillHandlingContainer_PageChange(string id, string CustomerChoice, int pagenum = 0, int pagesize = 50)
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        if (ePOSSession.GetObject(id) == null)
        //        {
        //            Logging.ManagementLogger.ErrorFormat("BillHandlingContainer_PageChange => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, "Phiên làm việc không tồn tại");
        //            return Json(new { Result = "ERROR", Message = "Phiên làm việc không tồn tại" });
        //        }else
        //        {
        //            List<ObjReport> data = new List<ObjReport>();
        //            List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
        //            long total_bill = items.Count();
        //            foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
        //            {
        //                ObjReport row = new ObjReport();
        //                row.col_1 = item.col_1;
        //                row.col_2 = item.col_2;
        //                row.col_3 = item.col_3;
        //                row.col_4 = item.col_4;
        //                row.col_5 = item.col_5;
        //                row.col_6 = item.col_6;
        //                row.col_7 = item.col_7;
        //                row.col_8 = item.col_8;
        //                row.col_9 = item.col_9;
        //                row.col_10 = item.col_10;
        //                row.col_11 = item.col_11;
        //                row.col_12 = item.col_12;
        //                row.col_13 = item.col_13;
        //                row.col_14 = item.col_14;
        //                row.col_15 = item.col_15;
        //                row.col_16 = item.col_16;
        //                row.col_17 = item.col_17;
        //                row.col_18 = item.col_18;
        //                row.col_19 = item.col_19;
        //                row.col_20 = item.col_20;
        //                row.col_0 = total_bill.ToString();
        //                data.Add(row);
        //            }

        //            int _PageLast = 0;
        //            if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST) != null)
        //            {
        //                _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST));
        //            }
        //            ePOSSession.AddObject(Constant.PAGE_SIZE_LAST, pagesize);
        //            int countItem = data.Count();
        //            if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
        //            {
        //                for (int i = 0; i < pagesize; i++)
        //                {
        //                    ObjReport row = new ObjReport();
        //                    row.col_1 = string.Empty;
        //                    row.col_2 = string.Empty;
        //                    row.col_3 = string.Empty;
        //                    row.col_4 = string.Empty;
        //                    row.col_5 = string.Empty;
        //                    row.col_6 = string.Empty;
        //                    row.col_7 = string.Empty;
        //                    row.col_8 = string.Empty;
        //                    row.col_9 = string.Empty;
        //                    row.col_10 = string.Empty;
        //                    row.col_11 = string.Empty;
        //                    row.col_12 = string.Empty;
        //                    row.col_13 = string.Empty;
        //                    row.col_14 = string.Empty;
        //                    row.col_15 = string.Empty;
        //                    row.col_16 = string.Empty;
        //                    row.col_17 = string.Empty;
        //                    row.col_18 = string.Empty;
        //                    row.col_19 = string.Empty;
        //                    row.col_20 = string.Empty;
        //                    row.col_0 = total_bill.ToString();
        //                    data.Insert(0, row);
        //                }
        //            }                   
        //            return Json(new {
        //                Result = "SUCCESS",
        //                Records = data,
        //                total_bill = total_bill.ToString("N0"),
        //                CustomerSelected = CustomerChoice
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("BillHandlingContainer_PageChange => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[HttpPost]
        //public JsonResult DelBildHandling(string id, string datasource, string curentpage)
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {               
        //        List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        var item = (from x in data where x.col_8 == id select x).FirstOrDefault();               
        //        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, "3", string.Empty, string.Empty, string.Empty, posAccount);
        //        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //        {
        //            data.Remove(item);
        //            ePOSSession.AddObject(datasource, data);
        //            return Json(new { Result = "SUCCESS", Message = "Xóa bản ghi thành công" });
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //        }                
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("DelBildHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[HttpPost]
        //public JsonResult UpdateStatusTransOff(string id, string datasource, string curentpage)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        var item = (from x in data where x.col_8 == id select x).FirstOrDefault();               
        //        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
        //        if (entity.code.CompareTo(Constant.SUCCESS_CODE)==0)
        //        {
        //            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", id = id });
        //        }else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("DelBildHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[HttpPost]
        //public JsonResult UpdateAllStatusTransOff(string datasource)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        int i_success = 0;
        //        int i_error = 0;
        //        int index = 0;
        //        foreach (var item in items)
        //        {
        //            Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, item.col_8, posAccount.session);
        //            responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
        //            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //            {
        //                Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
        //                items.ElementAt(index).col_11 = (from x in Constant.BillHandlingStatus() where x.Key == Constant.LEVEL_PC_2 select x).FirstOrDefault().Value;
        //                items.ElementAt(index).col_12 = Constant.LEVEL_PC_2;
        //                i_success++;
        //            }
        //            else
        //            {
        //                Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
        //                i_error++;
        //            }
        //            index++;
        //        }
        //        ePOSSession.AddObject(datasource, items);                  
        //        return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thay đổi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });

        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //public PartialViewResult _EditBillHandling(string id, int index)
        //{
        //    EditBillHandlingModel model = new EditBillHandlingModel();

        //    List<SelectListItem> StatusList = new List<SelectListItem>();
        //    StatusList.Add(new SelectListItem { Value = "", Text = "-- Chọn trạng thái --" });
        //    foreach (var item in Constant.BillHandlingStatus())
        //    {
        //        StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.StatusList = StatusList;
        //    List<SelectListItem> CodeList = new List<SelectListItem>();
        //    CodeList.Add(new SelectListItem { Value = "", Text = "-- Chọn mã lỗi --" });
        //    foreach (var item in Constant.BillHandlingCode())
        //    {
        //        CodeList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.CodeList = CodeList;
        //    ViewBag.id = id;
        //    return PartialView(model);
        //}

        //public JsonResult EditBillHandling(string id, string datasource, string status, string code)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        var item = (from x in data where x.col_8 == id select x).FirstOrDefault();
        //        Logging.ManagementLogger.ErrorFormat("EditBillHandling => User: {0}, id: {1}, datasource: {2}, status: {3}, code: {4}, Session: 53}", posAccount.edong, id, datasource,status,code, posAccount.session);
        //        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, status, string.Empty, code, posAccount.edong + " update tay", posAccount);
        //        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //        {
        //            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", id = id });
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("EditBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //        }               
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("EditBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //public PartialViewResult _UpdateTimeBillHanding(string id, int index)
        //{
        //    ViewBag.id = id;
        //    ViewBag.index = index;
        //    string[] array = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
        //    ViewBag.date = array[0];
        //    ViewBag.time = array[1];
        //    return PartialView();
        //}

        ////Cập nhật từng bản ghi
        //[HttpPost]
        //public JsonResult UpdateTransOff(string id, string datasource, string date, string time)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
        //        string result = Validate.checkTimeTransOff(datetime);
        //        if(string.IsNullOrEmpty(result))
        //        {
        //            List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //            var item = (from x in data where x.col_8 == id select x).FirstOrDefault();
        //            Logging.ManagementLogger.InfoFormat("UpdateTransOff (Request) => User: {0}, RequestId: {1}, status: {2}, workDate: {3}, Code: {4}, Description: {5}, Session: {6}", posAccount.edong, item.col_8, string.Empty, date + " " + time, string.Empty, string.Empty, posAccount.session);

        //            responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);

        //            Logging.ManagementLogger.InfoFormat("UpdateTransOff (Response) => User: {0}, RequestId: {1}, status: {2}, workDate: {3}, Code: {4}, Description: {5}, Result: {7}, Session: {6}", posAccount.edong, item.col_8, string.Empty, date + " " + time, string.Empty, string.Empty, JsonConvert.SerializeObject(entity),posAccount.session);

        //            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //            {
        //                return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", id = id });
        //            }
        //            else
        //            {
        //                Logging.ManagementLogger.ErrorFormat("UpdateTransOff => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //            }
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("UpdateTransOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = result });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("UpdateTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        ////Cập nhật tất cả hóa đơn
        //[HttpPost]
        //public JsonResult UpdateAllTransOff(string CustomerChoice, string datasource, string curentpage, string date, string time)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        int i_success = 0;
        //        int i_error = 0;
        //        string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
        //        string result = Validate.checkTimeTransOff(datetime);
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //            int index = 0;
        //            foreach (var item in items)
        //            {
        //                Logging.ManagementLogger.InfoFormat("UpdateAllTransOff => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, item.col_8, posAccount.session);
        //                responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);
        //                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                {
        //                    Logging.ManagementLogger.InfoFormat("UpdateAllTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
        //                    i_success++;                            
        //                }
        //                else
        //                {
        //                    Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
        //                    i_error++;
        //                }
        //            }
        //            ePOSSession.Del(datasource);
        //            return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thay đổi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });                                      
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = result });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[AllowAnonymous]
        //public ActionResult ExportBillHandling(string id)
        //{
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        var workbook = ePOSReport.BillHandling(id, posAccount);
        //        HttpContext context = System.Web.HttpContext.Current;
        //        context.Response.Clear();
        //        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        context.Response.AddHeader("content-disposition", "attachment;filename=HDdangxulychamno.xlsx");
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            workbook.SaveAs(memoryStream);
        //            memoryStream.WriteTo(context.Response.OutputStream);
        //            memoryStream.Close();
        //        }
        //        context.Response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("ExportBillHandling => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
        //    }
        //    return View();
        //}

        //[AllowAnonymous]
        //public ActionResult ExportReconciliation(string id, string pc)
        //{
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
        //        var workbook = ePOSReport.Reconciliation(id, pc, dir + "Temp_Reconciliation.xlsx", posAccount);
        //        HttpContext context = System.Web.HttpContext.Current;
        //        context.Response.Clear();
        //        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        context.Response.AddHeader("content-disposition", "attachment;filename=HDdangxulychamno.xlsx");
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            workbook.SaveAs(memoryStream);
        //            memoryStream.WriteTo(context.Response.OutputStream);
        //            memoryStream.Close();
        //        }
        //        context.Response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("ExportReconciliation => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
        //    }
        //    return View();
        //}

        //public PartialViewResult _TreeHandling()
        //{
        //    ViewBag.DateTime = DateTime.Now.ToString("HH:mm:ss");
        //    return PartialView();
        //}

        //[HttpPost]
        //public JsonResult getTreeHandling(string Fromdate, string TimeFrom, string Todate, string TimeTo)
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        string result = Validate.checkTreeHandling(Fromdate, TimeFrom, Todate, TimeTo);
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            string strFrom = string.IsNullOrEmpty(Fromdate) == true ? Fromdate : 
        //                string.IsNullOrEmpty(TimeFrom) == true ? Fromdate.Trim() + " 00:00:00" : Fromdate.Trim() + " " + TimeFrom.Trim();
        //            string strTo = string.IsNullOrEmpty(Todate) == true ? Todate :
        //                string.IsNullOrEmpty(TimeTo) == true ? Todate.Trim() + " 00:00:00" : Todate.Trim() + " " + TimeTo.Trim();
        //            responseEntity entity = ePosDAO.TotalAmountTransactionOff(strFrom, strTo, posAccount);
        //            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //            {
        //                string date = DateTime.Now.ToString();
        //                List<ObjReport> items = new List<ObjReport>();
        //                decimal total_bill = 0;
        //                decimal total_amount = 0;

        //                foreach (var item in Constant.BillHandling())
        //                {
        //                    decimal bill = 0;
        //                    decimal amount = 0;
        //                    ObjReport row_parent = new ObjReport();
        //                    row_parent.col_1 = item.Key.Trim();
        //                    row_parent.col_2 = item.Value;
        //                    foreach (var data in (from x in entity.listEvnPcBO where x.providerCode == item.Key.Trim() select x))
        //                    {
        //                        ObjReport row_child = new ObjReport();
        //                        row_child.col_1 = data.ext;
        //                        row_child.col_2 = data.fullName;
        //                        row_child.col_3 = data.totalTransaction.ToString("N0");
        //                        row_child.col_4 = data.totalAmount.ToString("N0");
        //                        row_child.col_5 = data.providerCode;                             
        //                        bill = bill + data.totalTransaction;
        //                        amount = amount + data.totalAmount;
        //                        items.Add(row_child);
        //                    }
        //                    row_parent.col_3 = bill.ToString("N0");
        //                    row_parent.col_4 = amount.ToString("N0");
        //                    row_parent.col_5 = "0";                         
        //                    total_bill = total_bill + bill;
        //                    total_amount = total_amount + amount;
        //                    items.Add(row_parent);
        //                }                        
        //                ePOSSession.AddObject(Constant.REPORT_TOTAL_BILLHANDLING + date, items);
        //                return Json(new { Result = "SUCCESS", Records = items, id = Constant.REPORT_TOTAL_BILLHANDLING + date,
        //                    total = total_bill.ToString("N0"), amount = total_amount.ToString("N0"),
        //                    fromdate = Fromdate,
        //                    todate = Todate
        //                });
        //            }
        //            else
        //            {
        //                Logging.ManagementLogger.ErrorFormat("getTreeHandling => UserName: {0}, sessionId: {1}, Code: {2} Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
        //                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //            }
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("getTreeHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
        //            return Json(new { Result = "ERROR", Message = result });
        //        }                
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("getTreeHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[AllowAnonymous]
        //public ActionResult ExportTotal(string id, string total, string amount, string fromdate, string todate)
        //{
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        //var workbook = ePOSReport.TotalBillHandling(id, total, amount, fromdate, todate, posAccount);
        //        var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
        //        var workbook = ePOSReport.TotalBillHandling(id, total, amount, fromdate, todate, dir + "Temp_BillHandling.xlsx", posAccount);
        //        HttpContext context = System.Web.HttpContext.Current;
        //        context.Response.Clear();
        //        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        context.Response.AddHeader("content-disposition", "attachment;filename=THHDdangxulychamno.xlsx");
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            workbook.SaveAs(memoryStream);
        //            memoryStream.WriteTo(context.Response.OutputStream);
        //            memoryStream.Close();
        //        }
        //        context.Response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("ExportReconciliation => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
        //    }
        //    return View();
        //}

        //#endregion

        #region Hóa đơn đang xử lý chấm nợ

        [AllowAnonymous]
        public ActionResult BillHandling_2()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.BILLHANDLING_TITLE;
            ViewBag.TitleLeft = "Hóa đơn đang XLCN";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            BillHandlingModel model = new BillHandlingModel();
            if (tempPosAcc.EvnPC == null)
            {
                Logging.ManagementLogger.ErrorFormat("BillHandling => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            PCList.Add(new SelectListItem { Value = "", Text = "--Chọn điện lực--" });
            //foreach (var item in (tempPosAcc.EvnPC.Where(x => !Constant.EVN().Any(p=> p.Key == x.pcId.ToString()))))
            foreach (var item in tempPosAcc.EvnPC)
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = PCList;
            List<SelectListItem> AccList = new List<SelectListItem>();
            AccList.Add(new SelectListItem { Value = "", Text = "--Chọn ví TNV--" });
            //NPHAN 2018-04-10
            //responseEntity resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
            responseEntity resEntity = ePosDAO.getChildAccFromSession(posAccount);
            if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
            {
                foreach (var acc in resEntity.listAccount)
                {
                    AccList.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
                }
            }
            else
            {
                Logging.ManagementLogger.ErrorFormat("BillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            model.AccList = AccList;
            List<SelectListItem> TypeList = new List<SelectListItem>();
            TypeList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.BillHandlingType())
            {
                TypeList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.TypeList = TypeList;
            List<SelectListItem> StatusList = new List<SelectListItem>();
            StatusList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
            foreach (var item in Constant.BillHandlingStatus())
            {
                StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.StatusList = StatusList;
            model.Todate = model.Fromdate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
            ViewBag.DateTime = DateTime.Now.ToString("HH:mm:ss");
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchBillHandling_2(int index, string pcCode, string account, string customer, string type, string status, string fromdate, string fromtime, string todate,
            string totime, string id, string curentpage, string CustomerChoice, int jtPageSize = 0, int jtStartIndex = 500)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (index == 0)
                {
                    string result = Validate.check_SearchBillHandling(pcCode, customer, fromdate, fromtime, todate, totime);
                    if (string.IsNullOrEmpty(result))
                    {
                        // xử lý checkbox
                        Dictionary<string, string> dcustomerPage = new Dictionary<string, string>();
                        string _CustomerSelected = string.Empty;
                        if (ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED) != null)
                        {
                            dcustomerPage = (Dictionary<string, string>)ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);

                            if (!string.IsNullOrEmpty(curentpage))
                            {
                                if (!dcustomerPage.ContainsKey(curentpage) && !string.IsNullOrEmpty(CustomerChoice))
                                {
                                    Dictionary<string, string> dCustomerChoiced = new Dictionary<string, string>();
                                    string[] arr = CustomerChoice.Split(';');
                                    if (arr != null && arr.Length > 0)
                                    {
                                        for (int i = 0; i < arr.Length; i++)
                                        {
                                            if (!string.IsNullOrEmpty(arr[i]))
                                            {
                                                if (!dCustomerChoiced.ContainsKey(arr[i]))
                                                    dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
                                            }
                                        }
                                    }

                                    dcustomerPage.Add(curentpage, string.Join(";", dCustomerChoiced.Select(p => p.Key)));
                                }
                                else
                                {
                                    Dictionary<string, string> dCustomerChoiced = new Dictionary<string, string>();
                                    string[] arr = CustomerChoice.Split(';');
                                    if (arr != null && arr.Length > 0)
                                    {
                                        for (int i = 0; i < arr.Length; i++)
                                        {
                                            if (!string.IsNullOrEmpty(arr[i]))
                                            {
                                                if (!dCustomerChoiced.ContainsKey(arr[i]))
                                                    dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
                                            }
                                        }
                                    }
                                    if (dcustomerPage.ContainsKey(curentpage))
                                        dcustomerPage[curentpage] = string.Join(";", dCustomerChoiced.Select(p => p.Key));
                                    else
                                        dcustomerPage.Add(curentpage, string.Join(";", dCustomerChoiced.Select(p => p.Key)));
                                }
                                ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(curentpage))
                                {
                                    if (!dcustomerPage.ContainsKey(curentpage))
                                    {
                                        dcustomerPage.Add(curentpage, CustomerChoice);
                                    }
                                }
                                if (dcustomerPage != null && dcustomerPage.Count > 0)
                                {
                                    ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
                                }
                            }

                            // đẩy toàn bộ chọn ra ngoài
                            if (dcustomerPage != null)
                            {
                                foreach (var item in dcustomerPage)
                                {
                                    if (!string.IsNullOrEmpty(item.Value))
                                        _CustomerSelected = string.IsNullOrEmpty(_CustomerSelected) == true ? item.Value : _CustomerSelected + "," + item.Value;
                                }
                            }
                        }
                        else
                        {
                            Dictionary<string, string> dcustomer = new Dictionary<string, string>();

                            if (ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO_CHOICED) != null)
                            {

                                var dCustomerChoiced = (Dictionary<string, string>)ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO_CHOICED);
                                var lstCusChoiced = new List<string>();

                                if (dCustomerChoiced != null && dCustomerChoiced.Count() > 0)
                                {
                                    //Lưu thông tin mã chọn vào cache                   
                                    if (!string.IsNullOrEmpty(CustomerChoice))
                                    {
                                        string[] arr = CustomerChoice.Split(';');
                                        if (arr != null && arr.Length > 0)
                                        {
                                            for (int i = 0; i < arr.Length; i++)
                                            {
                                                if (!string.IsNullOrEmpty(arr[i]))
                                                {
                                                    if (!dCustomerChoiced.ContainsKey(arr[i]))
                                                        dCustomerChoiced.Add(arr[i].ToString(), arr[i]);
                                                }
                                            }
                                        }
                                    }
                                    //Lấy list các mã KH đã chọn để hiển thị selected trên Jtable
                                    if (dCustomerChoiced != null && dCustomerChoiced.Count() > 0)
                                    {
                                        _CustomerSelected = string.Join(";", dCustomerChoiced.Select(p => p.Key));
                                    }
                                }
                            }
                            else
                            {
                                Dictionary<string, string> dChoice = null;
                                //Lưu thông tin mã chọn vào cache                   
                                if (!string.IsNullOrEmpty(CustomerChoice))
                                {
                                    dChoice = new Dictionary<string, string>();
                                    string[] arr = CustomerChoice.Split(';');
                                    if (arr != null && arr.Length > 0)
                                    {
                                        for (int i = 0; i < arr.Length; i++)
                                        {
                                            if (!string.IsNullOrEmpty(arr[i]))
                                            {
                                                if (!dChoice.ContainsKey(arr[i]))
                                                    dChoice.Add(arr[i].ToString(), arr[i]);
                                            }
                                        }
                                    }
                                }
                                //Lấy list các mã KH đã chọn để hiển thị selected trên Jtable
                                if (dChoice != null && dChoice.Count() > 0)
                                {
                                    _CustomerSelected = string.Join(";", dChoice.Select(p => p.Key));
                                }
                            }
                            dcustomerPage.Add(curentpage, _CustomerSelected);
                            ePOSSession.AddObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED, dcustomerPage);
                        }
                        List<ObjReport> items = new List<ObjReport>();
                        decimal total_amount = 0;
                        int total_bill = 0;
                        string date = DateTime.Now.ToString();
                        if (!string.IsNullOrEmpty(customer))
                        {
                            responseEntity entity = ePosDAO.getTransactionOff(string.Empty, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                List<PendingReportItem> temp = JsonConvert.DeserializeObject<List<PendingReportItem>>(CompressUtil.DecryptBase64(entity.outputZip), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                                items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
                                ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
                            }
                            else
                            {
                                Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                        else if (!string.IsNullOrEmpty(pcCode))
                        {
                            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                            List<ObjEVNPC> data = tempPosAcc.EvnPC;
                            var pc_parent = (from x in tempPosAcc.EvnPC where x.ext == pcCode select x).FirstOrDefault();
                            var pc_child = pcCode.CompareTo("PA") == 0 ?
                                tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId && !x.ext.Contains("PH")).ToList() :
                                tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId).ToList();
                            if (pc_child == null || pc_child.Count() == 0)
                            {
                                responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                {
                                    items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
                                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
                                }
                                else
                                {
                                    Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                                }
                            }
                            else
                            {
                                foreach (var pc in pc_child)
                                {
                                    decimal temp_total_amount = 0;
                                    int temp_total_bill = 0;
                                    responseEntity entity = ePosDAO.getTransactionOff(pc.ext, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
                                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    {
                                        items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref temp_total_amount, ref temp_total_bill);
                                        total_amount = total_amount + temp_total_amount;
                                        total_bill = total_bill + temp_total_bill;
                                    }
                                    else
                                    {
                                        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, pc: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, pc.ext, entity.code, entity.description, posAccount.session);
                                    }
                                }
                                ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
                            }
                        }
                        else
                        {
                            responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
                                ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
                            }
                            else
                            {
                                Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }

                        return Json(new
                        {
                            Result = "OK",
                            Records = items.Skip(jtStartIndex).Take(jtPageSize),
                            status = status,
                            CustomerSelected = _CustomerSelected,
                            TotalRecordCount = total_bill,
                            total = total_bill.ToString("N0"),
                            amount = total_amount.ToString("N0"),
                            pc = pcCode,
                            id = Constant.REPORT_BILLHANDLING + date,
                            datefrom = fromdate.Trim() + " " + fromtime.Trim(),
                            dateto = todate.Trim() + " " + totime.Trim()
                        });
                    }
                    else
                    {
                        List<ObjReport> items = new List<ObjReport>();
                        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                        //return Json(new { Result = "OK", Records = items, TotalRecordCount = 0, amount = 0, CustomerSelected = "", pc = pcCode });
                        return Json(new { Result = "ERROR", Message = result });
                    }
                }
                else
                {
                    List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                    int total_bill = 0;
                    decimal total_amount = 0;
                    foreach (var item in items)
                    {
                        total_bill++;
                        total_amount = total_amount + decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    }
                    return Json(new
                    {
                        Result = "OK",
                        Records = items.Skip(jtStartIndex).Take(jtPageSize),
                        status = status,
                        CustomerSelected = string.Empty,
                        TotalRecordCount = total_bill,
                        total = total_bill.ToString("N0"),
                        amount = total_amount.ToString("N0"),
                        pc = pcCode,
                        id = id,
                        datefrom = fromdate.Trim() + " " + fromtime.Trim(),
                        dateto = todate.Trim() + " " + totime.Trim()
                    });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult DetailBillHandling(string id, string index)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(id) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var items = (List<ObjReport>)ePOSSession.GetObject(id);
                var item = (from x in items where x.col_8 == index select x).FirstOrDefault();
                Hashtable hash = new Hashtable();
                hash.Add("name", item.col_17);
                hash.Add("address", item.col_18);
                hash.Add("reason", item.col_13);
                hash.Add("responseCode", item.col_16);
                hash.Add("pcCode", item.col_20);
                hash.Add("inning", item.col_21);
                List<Hashtable> data = new List<Hashtable>();
                data.Add(hash);
                return Json(new { Result = "OK", Records = data, TotalRecordCount = data.Count().ToString("N0") });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("DetailBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult DelBildHandling(string datasource, string index, int total_row)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
                var item = (from x in data where x.col_8 == index select x).FirstOrDefault();
                if (item != null)
                {
                    responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, "3", string.Empty, string.Empty, string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        data.Remove(item);
                        ePOSSession.AddObject(datasource, data);
                        int total_page = 0;
                        if (data.Count() % total_row == 0)
                            total_page = data.Count() / total_row;
                        else
                            total_page = (data.Count() / total_row) + 1;
                        var objResult = new
                        {
                            Result = "SUCCESS",
                            Message = "Xóa bản ghi thành công",
                            total_page = total_page
                        };
                        var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;
                        return jsonResult;
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Error: {1}, Session: {2}", posAccount.edong, "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ", posAccount.session);
                    return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("DelBildHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UpdateStatusTransOff(string datasource, string index)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
                for (int i = 0; i < data.Count(); i++)
                {
                    ObjReport item = data.ElementAt(i);
                    if (item.col_8.CompareTo(index) == 0)
                    {
                        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            data.ElementAt(i).col_11 = (from x in Constant.BillHandlingStatus() where x.Key == Constant.LEVEL_PC_2 select x).FirstOrDefault().Value;
                            data.ElementAt(i).col_12 = Constant.LEVEL_PC_2;
                            ePOSSession.AddObject(datasource, data);
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công" });
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                }
                return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi thỏa mãn nghiệp vụ" });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateStatusTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UpdateAllStatusTransOff(string datasource, string id, string choicepage, string curentpage)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {

                string temp_choice = string.Empty;
                // lay cache
                Dictionary<string, string> dcustomerPage = new Dictionary<string, string>();
                if (ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED) != null)
                {
                    dcustomerPage = (Dictionary<string, string>)ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                    if (!dcustomerPage.ContainsKey(curentpage))
                    { // khong chưa
                        foreach (var item in dcustomerPage)
                        {
                            if (!string.IsNullOrEmpty(item.Value))
                                temp_choice = string.IsNullOrEmpty(temp_choice) ? item.Value : temp_choice + ";" + item.Value;
                        }
                        temp_choice = string.IsNullOrEmpty(temp_choice) ? choicepage : temp_choice + ";" + choicepage;
                    }
                    else
                    {
                        foreach (var item in dcustomerPage)
                        {
                            if (item.Key.CompareTo(curentpage) != 0)
                                temp_choice = string.IsNullOrEmpty(temp_choice) ? item.Value : temp_choice + ";" + item.Value;
                        }
                        temp_choice = string.IsNullOrEmpty(temp_choice) ? choicepage : temp_choice + ";" + choicepage;
                    }
                }
                else
                {
                    temp_choice = choicepage;
                }

                string[] array_choice = temp_choice.Split(';');
                List<ObjReport> items = (List<ObjReport>)(ePOSSession.GetObject(datasource));
                Dictionary<string, string> dict_choice = new Dictionary<string, string>();
                int i_success = 0;
                int i_error = 0;

                if (array_choice != null && array_choice.Length > 0)
                {
                    for (int i = 0; i < array_choice.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array_choice[i]))
                        {
                            if (!dict_choice.ContainsKey(array_choice[i]))
                                dict_choice.Add(array_choice[i].ToString(), array_choice[i]);
                        }
                    }
                }

                for (int i = 0; i < items.Count(); i++)
                {
                    var temp = items.ElementAt(i);
                    string temp_key = temp.col_8;
                    if (dict_choice.ContainsKey(temp_key))
                    {
                        Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, i, items.ElementAt(i).col_8, posAccount.session);
                        responseEntity entity = ePosDAO.UpdateTransactionOffByID(temp.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
                        //responseEntity entity = ePosDAO.UpdateTransactionOffByID(items.ElementAt(i).col_8, items.ElementAt(i).col_9, items.ElementAt(i).col_1, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, i, items.ElementAt(i).col_8, entity.code, entity.description, posAccount.session);
                            items.ElementAt(i).col_11 = (from x in Constant.BillHandlingStatus() where x.Key == Constant.LEVEL_PC_2 select x).FirstOrDefault().Value;
                            items.ElementAt(i).col_12 = Constant.LEVEL_PC_2;
                            i_success++;
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, i, items.ElementAt(i).col_8, entity.code, entity.description, posAccount.session);
                            i_error++;
                        }
                    }
                }
                ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                ePOSSession.AddObject(datasource, items);
                //ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + datasource, items);
                return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thay đổi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UpdateAllStatusTransOff_2(string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = (List<ObjReport>)(ePOSSession.GetObject(datasource));
                int index = 0;
                int i_success = 0;
                int i_error = 0;
                foreach (var item in items)
                {
                    Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff_2 => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, item.col_8, posAccount.session);
                    responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        items.ElementAt(index).col_11 = (from x in Constant.BillHandlingStatus() where x.Key == Constant.LEVEL_PC_2 select x).FirstOrDefault().Value;
                        items.ElementAt(index).col_12 = Constant.LEVEL_PC_2;
                        i_success++;
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff_2 => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
                        i_error++;
                    }
                    index++;
                }
                ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                ePOSSession.AddObject(datasource, items);
                return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thay đổi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff_2 => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _UpdateTimeBillHanding(string id)
        {
            ViewBag.id = id;
            string[] array = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
            ViewBag.date = array[0];
            ViewBag.time = array[1];
            return PartialView();
        }

        [HttpPost]
        public JsonResult UpdateTransOff(int id, string datasource, string date, string time)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
                string result = Validate.checkTimeTransOff(datetime, string.Empty, 0);
                if (string.IsNullOrEmpty(result))
                {
                    List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
                    for (int i = 0; i < data.Count(); i++)
                    {
                        var item = data.ElementAt(i);
                        if (item.col_8 == id.ToString())
                        {
                            Logging.ManagementLogger.InfoFormat("UpdateTransOff (Request) => User: {0}, RequestId: {1}, status: {2}, workDate: {3}, Code: {4}, Description: {5}, Session: {6}", posAccount.edong, item.col_8, string.Empty, date + " " + time, string.Empty, string.Empty, posAccount.session);
                            responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);
                            Logging.ManagementLogger.InfoFormat("UpdateTransOff (Response) => User: {0}, RequestId: {1}, status: {2}, workDate: {3}, Code: {4}, Description: {5}, Result: {7}, Session: {6}", posAccount.edong, item.col_8, string.Empty, date + " " + time, string.Empty, string.Empty, JsonConvert.SerializeObject(entity), posAccount.session);

                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                data.ElementAt(i).col_7 = datetime;
                                ePOSSession.AddObject(datasource, data);
                                return Json(new { Result = "SUCCESS", Message = "Thay đổi lịch chấm nợ thành công" });

                            }
                            else
                            {
                                Logging.ManagementLogger.ErrorFormat("UpdateTransOff => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                    }
                    return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi thỏa mãn nghiệp vụ" });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateTransOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }

            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _UpdateTimeAllBillHandling(string id)
        {
            ViewBag.id = id;
            string[] array = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
            ViewBag.date = array[0];
            ViewBag.time = array[1];
            return PartialView();
        }

        [HttpPost]
        public JsonResult UpdateAllTransOff(string choicepage, string datasource, string date, string time, string curentpage, int total_row)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
                string result = Validate.checkTimeTransOff(datetime, choicepage, 1);
                if (string.IsNullOrEmpty(result))
                {

                    string temp_choice = string.Empty;
                    // lay cache
                    Dictionary<string, string> dcustomerPage = new Dictionary<string, string>();
                    if (ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED) != null)
                    {
                        dcustomerPage = (Dictionary<string, string>)ePOSSession.GetObject(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                        if (!dcustomerPage.ContainsKey(curentpage))
                        { // khong chưa
                            foreach (var item in dcustomerPage)
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                    temp_choice = string.IsNullOrEmpty(temp_choice) ? item.Value : temp_choice + ";" + item.Value;
                            }
                            temp_choice = string.IsNullOrEmpty(temp_choice) ? choicepage : temp_choice + ";" + choicepage;
                        }
                        else
                        {
                            foreach (var item in dcustomerPage)
                            {
                                if (item.Key.CompareTo(curentpage) != 0)
                                    temp_choice = string.IsNullOrEmpty(temp_choice) ? item.Value : temp_choice + ";" + item.Value;
                            }
                            temp_choice = string.IsNullOrEmpty(temp_choice) ? choicepage : temp_choice + ";" + choicepage;
                        }
                    }
                    else
                    {
                        temp_choice = choicepage;
                    }
                    string[] array_choice = temp_choice.Split(';');

                    // thực hiện cập nhật
                    List<ObjReport> items = (List<ObjReport>)(ePOSSession.GetObject(datasource));
                    Dictionary<string, string> dict_choice = new Dictionary<string, string>();
                    int i_success = 0;
                    int i_error = 0;

                    if (array_choice != null && array_choice.Length > 0)
                    {
                        for (int i = 0; i < array_choice.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(array_choice[i]))
                            {
                                if (!dict_choice.ContainsKey(array_choice[i]))
                                    dict_choice.Add(array_choice[i].ToString(), array_choice[i]);
                            }
                        }
                    }
                    for (int i = 0; i < items.Count(); i++)
                    {
                        string key_temp = items.ElementAt(i).col_8;
                        if (dict_choice.ContainsKey(key_temp))
                        {
                            var temp = items.ElementAt(i);
                            Logging.ManagementLogger.InfoFormat("UpdateTransOffAll => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, i, items.ElementAt(i).col_8, posAccount.session);
                            responseEntity entity = ePosDAO.UpdateTransactionOffByID(temp.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                Logging.ManagementLogger.InfoFormat("UpdateTransOffAll => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, i, items.ElementAt(i).col_8, entity.code, entity.description, posAccount.session);
                                i_success++;
                                items.ElementAt(i).col_7 = date;
                            }
                            else
                            {
                                Logging.ManagementLogger.ErrorFormat("UpdateTransOffAll => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, i, items.ElementAt(i).col_8, entity.code, entity.description, posAccount.session);
                                i_error++;
                            }
                        }
                    }
                    ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                    //ePOSSession.Del(datasource);
                    ePOSSession.AddObject(datasource, items);
                    int total_page = 0;
                    if (items.Count() % total_row == 0)
                        total_page = items.Count() / total_row;
                    else
                        total_page = (items.Count() / total_row) + 1;


                    return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thay đổi lịch chấm nợ: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error), total_page = total_page });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _UpdateTimeAllBillHandling_2()
        {
            string[] array = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
            ViewBag.date = array[0];
            ViewBag.time = array[1];
            return PartialView();
        }

        [HttpPost]
        public JsonResult UpdateAllTransOff_2(string date, string time, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
                string result = Validate.checkTimeTransOff(datetime, string.Empty, 0);
                if (string.IsNullOrEmpty(result))
                {
                    List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(datasource);
                    int index = 0;
                    int i_success = 0;
                    int i_error = 0;
                    foreach (var item in items)
                    {
                        Logging.ManagementLogger.InfoFormat("UpdateAllTransOff_2 => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, item.col_8, posAccount.session);
                        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            Logging.ManagementLogger.InfoFormat("UpdateAllTransOff_2 => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
                            i_success++;
                            items.ElementAt(index).col_7 = date;
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff_2 => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
                            i_error++;
                        }
                        index++;
                    }
                    ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                    ePOSSession.AddObject(datasource, items);
                    return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thay đổi lịch chấm nợ: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff_2 => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff_2 => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        [HttpPost]
        public JsonResult UpdateAllTransOff_3(string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                //string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
                string result = "";
                if (string.IsNullOrEmpty(result))
                {
                    List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(datasource);
                    int index = 0;
                    int i_success = 0;
                    int i_error = 0;
                    foreach (var item in items)
                    {
                        //kiem tra trang thai
                        //chi  kiem tra trang thai !=1(hoan thanh) !=3(chua bi huy)
                        if (item.col_3.StartsWith("PD") && item.col_12 != "1" && item.col_12 != "3")
                        {
                            string status = "";
                            string[] _pcCode = new string[1];
                            _pcCode[0] = item.col_20.Trim();
                            string[] _account = new string[1];
                            _account[0] = item.col_1.Trim();
                            string customer = item.col_3;
                            string[] arrequestdate = item.col_2.Split(' ')[0].Split('-');
                            string txdate = arrequestdate[2] + "/" + arrequestdate[1] + "/" + arrequestdate[0];
                            string fromdate = txdate;
                            string todate = txdate;
                            Logging.ManagementLogger.InfoFormat("UpdateAllTransOff_3:getReportDetail_Water => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, item.col_8, posAccount.session);
                            responseEntity entity_check = ePosDAO.getReportDetail_Water(_pcCode, "", _account, customer, fromdate, todate, Constant.FINANCE, status, posAccount);
                            if (entity_check.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                //string resever2 = entity_check.listTransaction.FirstOrDefault().resever2;
                                for(int i=0; i< entity_check.listTransaction.Count(); i++)
                                {
                                    string resever2 = entity_check.listTransaction[i].resever2;
                                    if (resever2 == item.col_15)//transactionOff.bill.billId = s_reserve_2 
                                    {
                                        //update trang thai hoan thanh
                                        Logging.ManagementLogger.InfoFormat("UpdateAllTransOff_3 => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, item.col_8, posAccount.session);
                                        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, "1", string.Empty, entity_check.code, posAccount.edong + " thuc hien dong bo tay", posAccount);
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            Logging.ManagementLogger.InfoFormat("UpdateAllTransOff_3 => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
                                            i_success++;
                                        }
                                        else
                                        {
                                            Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff_3 => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, item.col_8, entity.code, entity.description, posAccount.session);
                                            i_error++;
                                        }
                                    }
                                }

                                

                            }
                            else
                            {
                                Logging.ReportLogger.ErrorFormat("UpdateAllTransOff_3 SearchDetail_Water => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, BillingType: {4}", posAccount.edong, posAccount.session, entity_check.code, entity_check.description, Constant.FINANCE);
                            }
                        }
                        index++;
                    }
                    //ePOSSession.Del(posAccount.session + Constant.CUSTOMER_INFO__PAGE_CHOICED);
                    //ePOSSession.AddObject(datasource, items);
                    return Json(new { Result = "SUCCESS", Message = string.Format("Đồng bộ: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff_3 => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff_3 => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditBillHandling(int index)
        {
            EditBillHandlingModel model = new EditBillHandlingModel();

            List<SelectListItem> StatusList = new List<SelectListItem>();
            StatusList.Add(new SelectListItem { Value = "", Text = "-- Chọn trạng thái --" });
            foreach (var item in Constant.BillHandlingStatus())
            {
                StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.StatusList = StatusList;
            List<SelectListItem> CodeList = new List<SelectListItem>();
            CodeList.Add(new SelectListItem { Value = "", Text = "-- Chọn mã lỗi --" });
            foreach (var item in Constant.BillHandlingCode())
            {
                CodeList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.CodeList = CodeList;
            ViewBag.index = index;
            return PartialView(model);
        }

        public JsonResult EditBillHandling(int index, string datasource, string status, string code)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
                for (int i = 0; i < data.Count(); i++)
                {
                    var item = data.ElementAt(i);
                    if (item.col_8 == index.ToString())
                    {
                        Logging.ManagementLogger.InfoFormat("EditBillHandling => User: {0}, id: {1}, datasource: {2}, status: {3}, code: {4}, Session: 53}", posAccount.edong, index, datasource, status, code, posAccount.session);
                        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, status, string.Empty, code, posAccount.edong + " update tay", posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            data.ElementAt(i).col_20 = code;
                            data.ElementAt(i).col_11 = status;
                            ePOSSession.AddObject(datasource, data);
                            return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công" });
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("EditBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                }
                return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi thỏa mãn nghiệp vụ" });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("EditBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        //[AllowAnonymous]
        //public ActionResult BillHandling()
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return RedirectToAction("Login", "ePOS", true);
        //    ViewBag.Title = Constant.BILLHANDLING_TITLE;
        //    ViewBag.TitleLeft = "Hóa đơn đang XLCN";
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
        //    BillHandlingModel model = new BillHandlingModel();
        //    if (tempPosAcc.EvnPC == null)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("BillHandling => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
        //        return RedirectToAction("Login", "ePOS", true);
        //    }
        //    List<SelectListItem> PCList = new List<SelectListItem>();
        //    PCList.Add(new SelectListItem { Value = "", Text = "--Chọn điện lực--" });
        //    //foreach (var item in (tempPosAcc.EvnPC.Where(x => !Constant.EVN().Any(p=> p.Key == x.pcId.ToString()))))
        //    foreach (var item in tempPosAcc.EvnPC)
        //    {
        //        PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
        //    }
        //    model.PCList = PCList;
        //    List<SelectListItem> AccList = new List<SelectListItem>();
        //    AccList.Add(new SelectListItem { Value = "", Text = "--Chọn ví TNV--" });
        //    //NPHAN 2018-04-10
        //    //responseEntity resEntity = ePosDAO.getChildAcc(string.Empty, Constant.CHILD_0, posAccount);
        //    responseEntity resEntity = ePosDAO.getChildAccFromSession(posAccount);
        //    if (resEntity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //    {
        //        foreach (var acc in resEntity.listAccount)
        //        {
        //            AccList.Add(new SelectListItem { Value = acc.edong, Text = acc.edong + " - " + acc.name });
        //        }
        //    }
        //    else
        //    {
        //        Logging.ManagementLogger.ErrorFormat("BillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, resEntity.code, resEntity.description, posAccount.session);
        //        return RedirectToAction("Login", "ePOS", true);
        //    }
        //    model.AccList = AccList;
        //    List<SelectListItem> TypeList = new List<SelectListItem>();
        //    TypeList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
        //    foreach (var item in Constant.BillHandlingType())
        //    {
        //        TypeList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.TypeList = TypeList;
        //    List<SelectListItem> StatusList = new List<SelectListItem>();
        //    StatusList.Add(new SelectListItem { Value = "", Text = Constant.ALL });
        //    foreach (var item in Constant.BillHandlingStatus())
        //    {
        //        StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.StatusList = StatusList;
        //    model.Todate = model.Fromdate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
        //    ViewBag.DateTime = DateTime.Now.ToString("HH:mm:ss");
        //    return View(model);
        //}

        //[HttpPost]
        //public JsonResult SearchBillHandling(string pcCode, string account, string customer, string type, string status, string fromdate, string fromtime, string todate,
        //    string totime)
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> items = new List<ObjReport>();
        //        decimal total_amount = 0;
        //        int total_bill = 0;
        //        string date = DateTime.Now.ToString();
        //        if (!string.IsNullOrEmpty(customer))
        //        {
        //            responseEntity entity = ePosDAO.getTransactionOff(string.Empty, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //            {
        //                List<PendingReportItem> temp = JsonConvert.DeserializeObject<List<PendingReportItem>>(CompressUtil.DecryptBase64(entity.outputZip), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //                items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        //                ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //            }
        //            else
        //            {
        //                Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //            }
        //        }
        //        else if (!string.IsNullOrEmpty(pcCode))
        //        {
        //            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
        //            List<ObjEVNPC> data = tempPosAcc.EvnPC;
        //            var pc_parent = (from x in tempPosAcc.EvnPC where x.ext == pcCode select x).FirstOrDefault();
        //            var pc_child = pcCode.CompareTo("PA") == 0 ?
        //                tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId && !x.ext.Contains("PH")).ToList() :
        //                tempPosAcc.EvnPC.Where(x => x.parentId == pc_parent.pcId).ToList();
        //            if (pc_child == null || pc_child.Count() == 0)
        //            {
        //                responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                {
        //                    items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        //                    ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //                }
        //                else
        //                {
        //                    Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //                }
        //            }
        //            else
        //            {
        //                foreach (var pc in pc_child)
        //                {
        //                    decimal temp_total_amount = 0;
        //                    int temp_total_bill = 0;
        //                    responseEntity entity = ePosDAO.getTransactionOff(pc.ext, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                    {
        //                        items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref temp_total_amount, ref temp_total_bill);
        //                        total_amount = total_amount + temp_total_amount;
        //                        total_bill = total_bill + temp_total_bill;
        //                    }
        //                    else
        //                    {
        //                        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, pc: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, pc.ext, entity.code, entity.description, posAccount.session);
        //                    }
        //                }
        //                ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //            }
        //        }
        //        else
        //        {
        //            responseEntity entity = ePosDAO.getTransactionOff(pcCode, account, customer, type, status, fromdate.Trim() + " " + fromtime.Trim(), todate.Trim() + " " + totime.Trim(), posAccount);
        //            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //            {
        //                items = ReadFile.FillDataBillHanding(items, entity.outputZip, ref total_amount, ref total_bill);
        //                ePOSSession.AddObject(Constant.REPORT_BILLHANDLING + date, items);
        //            }
        //            else
        //            {
        //                Logging.ManagementLogger.ErrorFormat("SearchBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //            }
        //        }
        //        if (items.Count() == 0)
        //            return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ", amount = 0, total_bill = 0 });
        //        else
        //        {
        //            var data = new
        //            {
        //                Result = "SUCCESS",
        //                Records = items,
        //                amount = total_amount.ToString("N0"),
        //                total_bill = total_bill.ToString("N0"),
        //                Message = Constant.REPORT_BILLHANDLING + date
        //            };

        //            var jsonResult = Json(data, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("SearchBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[HttpPost]
        //public JsonResult DelBildHandling(int id, string datasource, string curentpage)
        //{
        //    if (!ePOSController.CheckSession(HttpContext))
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        var item = data.ElementAt(id);
        //        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, "3", string.Empty, string.Empty, string.Empty, posAccount);
        //        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //        {
        //            data.Remove(item);
        //            ePOSSession.AddObject(datasource, data);
        //            var objResult = new
        //            {
        //                Result = "SUCCESS",
        //                Records = data,
        //                Message = "Xóa bản ghi thành công"
        //            };
        //            var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("DelBildHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[HttpPost]
        //public JsonResult UpdateStatusTransOff(int index, string datasource, string curentpage)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        var item = data.ElementAt(index);
        //        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
        //        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //        {
        //            data.ElementAt(index).col_11 = (from x in Constant.BillHandlingStatus() where x.Key == Constant.LEVEL_PC_2 select x).FirstOrDefault().Value;
        //            data.ElementAt(index).col_12 = Constant.LEVEL_PC_2;
        //            ePOSSession.AddObject(datasource, data);
        //            var objResult = new
        //            {
        //                Result = "SUCCESS",
        //                Records = data,
        //                Message = "Cập nhật bản ghi thành công",
        //                index = index
        //            };
        //            var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("DelBildHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("DelBildHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //[HttpPost]
        //public JsonResult UpdateAllStatusTransOff(string CustomerChoice, string datasource, int pagenum, int pagesize)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(CustomerChoice))
        //        {
        //            int i_success = 0;
        //            int i_error = 0;
        //            List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //            foreach (var item in CustomerChoice.Split(',').Skip(pagesize * (pagenum)).Take(pagesize))
        //            {

        //                int index = int.Parse(item);
        //                var temp = data.ElementAt(index);
        //                Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, temp.col_8, posAccount.session);
        //                responseEntity entity = ePosDAO.UpdateTransactionOffByID(temp.col_8, Constant.LEVEL_PC_2, string.Empty, string.Empty, string.Empty, posAccount);
        //                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                {
        //                    Logging.ManagementLogger.InfoFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, temp.col_8, entity.code, entity.description, posAccount.session);
        //                    data.ElementAt(index).col_11 = (from x in Constant.BillHandlingStatus() where x.Key == Constant.LEVEL_PC_2 select x).FirstOrDefault().Value;
        //                    data.ElementAt(index).col_12 = Constant.LEVEL_PC_2;
        //                    i_success++;
        //                }
        //                else
        //                {
        //                    Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, temp.col_8, entity.code, entity.description, posAccount.session);
        //                    i_error++;
        //                }
        //            }
        //            ePOSSession.AddObject(datasource, data);
        //            var objResult = new
        //            {
        //                Result = "SUCCESS",
        //                Records = data,
        //                Message = string.Format("Chi tiết thay đổi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error)
        //            };
        //            var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => UserName: {0}, sessionId: {1}, Error: Không có bản ghi nào được chọn", posAccount.edong, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = "Bạn phải chọn ít nhất một bản ghi để cập nhật" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("UpdateAllStatusTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //public PartialViewResult _EditBillHandling(int index)
        //{
        //    EditBillHandlingModel model = new EditBillHandlingModel();

        //    List<SelectListItem> StatusList = new List<SelectListItem>();
        //    StatusList.Add(new SelectListItem { Value = "", Text = "-- Chọn trạng thái --" });
        //    foreach (var item in Constant.BillHandlingStatus())
        //    {
        //        StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.StatusList = StatusList;
        //    List<SelectListItem> CodeList = new List<SelectListItem>();
        //    CodeList.Add(new SelectListItem { Value = "", Text = "-- Chọn mã lỗi --" });
        //    foreach (var item in Constant.BillHandlingCode())
        //    {
        //        CodeList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
        //    }
        //    model.CodeList = CodeList;
        //    ViewBag.index = index;
        //    return PartialView(model);
        //}

        //public JsonResult EditBillHandling(int index, string datasource, string status, string code)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //        //var item = (from x in data where x.col_8 == id select x).FirstOrDefault();
        //        var item = data.ElementAt(index);
        //        Logging.ManagementLogger.ErrorFormat("EditBillHandling => User: {0}, id: {1}, datasource: {2}, status: {3}, code: {4}, Session: 53}", posAccount.edong, index, datasource, status, code, posAccount.session);
        //        responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, status, string.Empty, code, posAccount.edong + " update tay", posAccount);
        //        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //        {
        //            data.ElementAt(index).col_20 = code;
        //            data.ElementAt(index).col_11 = status;
        //            ePOSSession.AddObject(datasource, data);
        //            var objResult = new
        //            {
        //                Result = "SUCCESS",
        //                Records = data,
        //                Message = "Cập nhật bản ghi thành công",
        //                index = index
        //            };
        //            var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //            //return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", index = index });
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("EditBillHandling => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("EditBillHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //public PartialViewResult _UpdateTimeBillHanding(string id)
        //{
        //    ViewBag.id = id;
        //    string[] array = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
        //    ViewBag.date = array[0];
        //    ViewBag.time = array[1];
        //    return PartialView();
        //}

        //[HttpPost]
        //public JsonResult UpdateTransOff(int id, string datasource, string date, string time)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
        //        string result = Validate.checkTimeTransOff(datetime, string.Empty, 0);
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //            //var item = (from x in data where x.col_8 == id select x).FirstOrDefault();
        //            var item = data.ElementAt(id);
        //            Logging.ManagementLogger.InfoFormat("UpdateTransOff (Request) => User: {0}, RequestId: {1}, status: {2}, workDate: {3}, Code: {4}, Description: {5}, Session: {6}", posAccount.edong, item.col_8, string.Empty, date + " " + time, string.Empty, string.Empty, posAccount.session);
        //            responseEntity entity = ePosDAO.UpdateTransactionOffByID(item.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);
        //            Logging.ManagementLogger.InfoFormat("UpdateTransOff (Response) => User: {0}, RequestId: {1}, status: {2}, workDate: {3}, Code: {4}, Description: {5}, Result: {7}, Session: {6}", posAccount.edong, item.col_8, string.Empty, date + " " + time, string.Empty, string.Empty, JsonConvert.SerializeObject(entity), posAccount.session);

        //            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //            {
        //                data.ElementAt(id).col_7 = datetime;
        //                ePOSSession.AddObject(datasource, data);
        //                var objResult = new
        //                {
        //                    Result = "SUCCESS",
        //                    Records = data,
        //                    Message = "Cập nhật bản ghi thành công",
        //                    index = id
        //                };
        //                var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
        //                jsonResult.MaxJsonLength = int.MaxValue;
        //                return jsonResult;

        //            }
        //            else
        //            {
        //                Logging.ManagementLogger.ErrorFormat("UpdateTransOff => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
        //                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
        //            }
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("UpdateTransOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = result });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("UpdateTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        //public PartialViewResult _OptionPushBillHandling()
        //{
        //    return PartialView();
        //}


        //public PartialViewResult _UpdateTimeAllBillHandling(string id, int pagenum, int pagesize)
        //{
        //    ViewBag.id = id;
        //    ViewBag.pagesize = pagesize;
        //    ViewBag.pagenum = pagenum;
        //    string[] array = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Split(' ');
        //    ViewBag.date = array[0];
        //    ViewBag.time = array[1];
        //    return PartialView();
        //}

        //[HttpPost]
        //public JsonResult UpdateAllTransOff(string CustomerChoice, string datasource, int pagenum, int pagesize, string date, string time)
        //{
        //    if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(datasource) == null)
        //        return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        string datetime = string.IsNullOrEmpty(time) == true ? date + " 00:00:00" : date + " " + time;
        //        string result = Validate.checkTimeTransOff(datetime, CustomerChoice, 1);
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            List<ObjReport> data = (List<ObjReport>)ePOSSession.GetObject(datasource);
        //            int i_success = 0;
        //            int i_error = 0;
        //            foreach (var item in CustomerChoice.Split(',').Skip(pagesize * (pagenum)).Take(pagesize))
        //            {
        //                int index = int.Parse(item);
        //                var temp = data.ElementAt(index);
        //                Logging.ManagementLogger.InfoFormat("UpdateAllTransOff => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, index, temp.col_8, posAccount.session);
        //                responseEntity entity = ePosDAO.UpdateTransactionOffByID(temp.col_8, string.Empty, datetime, string.Empty, string.Empty, posAccount);
        //                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
        //                {
        //                    Logging.ManagementLogger.InfoFormat("UpdateAllTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, temp.col_8, entity.code, entity.description, posAccount.session);
        //                    i_success++;
        //                    data.ElementAt(index).col_7 = date;
        //                }
        //                else
        //                {
        //                    Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => User: {0}, Item: {1}, RequestId: {2}, Code: {3}, Error: {4}, Session: {5}", posAccount.edong, index, temp.col_8, entity.code, entity.description, posAccount.session);
        //                    i_error++;
        //                }
        //            }
        //            ePOSSession.AddObject(datasource, data);
        //            var objResult = new
        //            {
        //                Result = "SUCCESS",
        //                Records = data,
        //                Message = string.Format("Chi tiết thay đổi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error)
        //            };
        //            var jsonResult = Json(objResult, JsonRequestBehavior.AllowGet);
        //            jsonResult.MaxJsonLength = int.MaxValue;
        //            return jsonResult;
        //        }
        //        else
        //        {
        //            Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
        //            return Json(new { Result = "ERROR", Message = result });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("UpdateAllTransOff => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
        //        return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
        //    }
        //}

        [AllowAnonymous]
        public ActionResult ExportBillHandling(string id, string pc, string datefrom, string dateto)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.BillHandling(id, pc, datefrom, dateto, dir + "Temp_HDDXLCN.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=HDdangxulychamno.xlsx");
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
                Logging.ManagementLogger.ErrorFormat("ExportBillHandling => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        //[AllowAnonymous]
        //public ActionResult ExportBillHandling(string id)
        //{
        //    ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
        //    try
        //    {
        //        var workbook = ePOSReport.BillHandling(id, posAccount);
        //        HttpContext context = System.Web.HttpContext.Current;
        //        context.Response.Clear();
        //        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        context.Response.AddHeader("content-disposition", "attachment;filename=HDdangxulychamno.xlsx");
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            workbook.SaveAs(memoryStream);
        //            memoryStream.WriteTo(context.Response.OutputStream);
        //            memoryStream.Close();
        //        }
        //        context.Response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.ManagementLogger.ErrorFormat("ExportBillHandling => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
        //    }
        //    return View();
        //}

        [AllowAnonymous]
        public ActionResult ExportReconciliation(string id, string pc)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.Reconciliation(id, pc, dir + "Temp_Reconciliation.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=HDdangxulychamno.xlsx");
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
                Logging.ManagementLogger.ErrorFormat("ExportReconciliation => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        public PartialViewResult _TreeHandling()
        {
            ViewBag.DateTime = DateTime.Now.ToString("HH:mm:ss");
            return PartialView();
        }

        [HttpPost]
        public JsonResult getTreeHandling(string Fromdate, string TimeFrom, string Todate, string TimeTo)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.checkTreeHandling(Fromdate, TimeFrom, Todate, TimeTo);
                if (string.IsNullOrEmpty(result))
                {
                    string strFrom = string.IsNullOrEmpty(Fromdate) == true ? Fromdate :
                        string.IsNullOrEmpty(TimeFrom) == true ? Fromdate.Trim() + " 00:00:00" : Fromdate.Trim() + " " + TimeFrom.Trim();
                    string strTo = string.IsNullOrEmpty(Todate) == true ? Todate :
                        string.IsNullOrEmpty(TimeTo) == true ? Todate.Trim() + " 00:00:00" : Todate.Trim() + " " + TimeTo.Trim();
                    responseEntity entity = ePosDAO.TotalAmountTransactionOff(strFrom, strTo, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string date = DateTime.Now.ToString();
                        List<ObjReport> items = new List<ObjReport>();
                        decimal total_bill = 0;
                        decimal total_amount = 0;

                        foreach (var item in Constant.BillHandling())
                        {
                            decimal bill = 0;
                            decimal amount = 0;
                            ObjReport row_parent = new ObjReport();
                            row_parent.col_1 = item.Key.Trim();
                            row_parent.col_2 = item.Value;
                            foreach (var data in (from x in entity.listEvnPcBO where x.providerCode == item.Key.Trim() select x))
                            {
                                ObjReport row_child = new ObjReport();
                                row_child.col_1 = data.ext;
                                row_child.col_2 = data.fullName;
                                row_child.col_3 = data.totalTransaction.ToString("N0");
                                row_child.col_4 = data.totalAmount.ToString("N0");
                                row_child.col_5 = data.providerCode;
                                bill = bill + data.totalTransaction;
                                amount = amount + data.totalAmount;
                                items.Add(row_child);
                            }
                            row_parent.col_3 = bill.ToString("N0");
                            row_parent.col_4 = amount.ToString("N0");
                            row_parent.col_5 = "0";
                            total_bill = total_bill + bill;
                            total_amount = total_amount + amount;
                            items.Add(row_parent);
                        }
                        ePOSSession.AddObject(Constant.REPORT_TOTAL_BILLHANDLING + date, items);
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = items,
                            id = Constant.REPORT_TOTAL_BILLHANDLING + date,
                            total = total_bill.ToString("N0"),
                            amount = total_amount.ToString("N0"),
                            fromdate = Fromdate,
                            todate = Todate
                        });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("getTreeHandling => UserName: {0}, sessionId: {1}, Code: {2} Error: {3}", posAccount.edong, posAccount.session, entity.code, entity.description);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("getTreeHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getTreeHandling => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportTotal(string id, string total, string amount, string fromdate, string todate)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                //var workbook = ePOSReport.TotalBillHandling(id, total, amount, fromdate, todate, posAccount);
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                var workbook = ePOSReport.TotalBillHandling(id, total, amount, fromdate, todate, dir + "Temp_BillHandling.xlsx", posAccount);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.Clear();
                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AddHeader("content-disposition", "attachment;filename=THHDdangxulychamno.xlsx");
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
                Logging.ManagementLogger.ErrorFormat("ExportReconciliation => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return View();
        }

        #endregion

        #region Han muc vi quay
        public ActionResult Withdraw_CashWallet()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ViewBag.Title = Constant.CONTROLDEBT_TITLE;
            ViewBag.TitleLeft = "Hạn mức ví quầy";
            WithdrawModel model = new WithdrawModel();
            List<SelectListItem> AccList = new List<SelectListItem>();
            if (tempPosAcc.Childs == null)
            {
                Logging.ManagementLogger.ErrorFormat("Withdraw => User: {0}, Error: Ví quản lý chưa có danh sách ví con , Session: {1}", posAccount.edong, posAccount.session);

            }
            else
            {
                AccList.Add(new SelectListItem { Value = "", Text = "--Chọn ví TNV--" });
                foreach (var item in tempPosAcc.Childs)
                {
                    AccList.Add(new SelectListItem { Value = item.phoneNumber, Text = item.phoneNumber + " - " + item.name });
                }
            }
            model.AccList = AccList;
            return View(model);
        }

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
            Session["captchaWithdraw"] = text;
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }

        [HttpPost]
        public JsonResult getBalanceByAcc(string acc)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (tempPosAcc == null)
                {
                    Logging.ManagementLogger.ErrorFormat("getBalanceByAcc => UserName: {0}, sessionId: {1} Error: Phiên làm việc không tồn tại", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = "Phiên làm việc không tồn tại" });
                }
                else
                {
                    if (string.IsNullOrEmpty(acc))
                    {
                        Logging.ManagementLogger.ErrorFormat("getBalanceByAcc => UserName: {0}, sessionId: {1}, Error: Acount null", posAccount.edong, posAccount.session);
                        return Json(new { Result = "SUCCESS_1", Message = "Lấy số dư thành công", balance = 0 });
                    }
                    else
                    {
                        string[] array = { acc };
                        responseEntity entity = ePosDAO.GetReportEdongWallet(array, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            long balance = 0;
                            long waitpay = 0;
                            long lockpay = 0;
                            for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                            {
                                lockpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).lockAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).lockAmount);
                                balance = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).balance) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).balance);
                                waitpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                            }
                            return Json(new
                            {
                                Result = "SUCCESS",
                                Message = "Lấy số dư thành công",
                                balance = (balance - lockpay - waitpay).ToString("N0"),
                                balanceAcc = balance.ToString("N0"),
                                lockpay = lockpay.ToString("N0"),
                                waitpay = waitpay.ToString("N0")
                            });
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("getBalanceByAcc => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getBalanceByAcc => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult getBalanceByAcc_Cash(string acc)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                if (tempPosAcc == null)
                {
                    Logging.ManagementLogger.ErrorFormat("getBalanceByAcc_Cash => UserName: {0}, sessionId: {1} Error: Phiên làm việc không tồn tại", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = "Phiên làm việc không tồn tại" });
                }
                else
                {
                    var item = (from x in tempPosAcc.Childs where x.phoneNumber == acc select x).FirstOrDefault();
                    responseEntity entity = new responseEntity();
                    int check = 0;
                    decimal multiplesOf = 0;
                    if (int.Parse(item.type) == 4)
                    {

                        entity = ePosDAO.getParams(Constant.GROUP, Constant.CODE, "", "1", posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            var y = (from x in entity.listParams select x).FirstOrDefault();
                            check = 1;
                            multiplesOf = string.IsNullOrEmpty(y.value) == true ? 0 : decimal.Parse(y.value);
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("getBalanceByAcc => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                    string[] array = { acc };
                    entity = ePosDAO.GetReportEdongWallet(array, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        long balance = 0;
                        long waitpay = 0;
                        long lockpay = 0;
                        for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                        {
                            lockpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).lockAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).lockAmount);
                            balance = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).balance) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).balance);
                            waitpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                        }
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Message = "Lấy số dư thành công",
                            balance = (balance - lockpay - waitpay).ToString("N0"),
                            balanceAcc = balance.ToString("N0"),
                            lockpay = lockpay.ToString("N0"),
                            waitpay = waitpay.ToString("N0"),
                            check = check,
                            multiplesOf = multiplesOf,
                        });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("getBalanceByAcc => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("getBalanceByAcc_Cash => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult TransferMoney(string edong, string balance, string amount, string desc, string captchar)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));

            try
            {
                string temp_captchar = Session["captchaWithdraw"].ToString().Replace(" ", "");
                string result = Validate.check_TransferMoney(edong, balance, amount, desc, captchar, temp_captchar, posAccount);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.doTransferMoney(edong, amount, desc, posAccount, true);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string temp_balance = string.Empty;
                        string temp_lookmany = string.Empty;
                        get_Banlance(HttpContext, posAccount, ref temp_balance, ref temp_lookmany);
                        //ePosAccount temp_posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                        return Json(new { Result = "SUCCESS", Message = "Thu hồi thành công", balance = temp_balance, lockMoney = temp_lookmany });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("TransferMoney => UserName: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("TransferMoney => UserName: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("TransferMoney => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public static void get_Banlance(HttpContextBase context, ePosAccount posAccount, ref string balance, ref string lookmany)
        {
            try
            {
                ePosAccount temp_posAccount = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                string[] array = { posAccount.edong };
                responseEntity entity = ePosDAO.GetReportEdongWallet(array, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    decimal temp_balance = 0;
                    decimal waitpay = 0;
                    decimal lockpay = 0;
                    for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                    {
                        lockpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).lockAmount) == true ? 0 : decimal.Parse(entity.listResponseReportEdong.ElementAt(i).lockAmount);
                        temp_balance = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).balance) == true ? 0 : decimal.Parse(entity.listResponseReportEdong.ElementAt(i).balance);
                        waitpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : decimal.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                    }
                    posAccount.balance = (temp_balance - lockpay - waitpay);
                    posAccount.lockMoney = lockpay;
                    temp_posAccount.balance = (temp_balance - lockpay - waitpay);
                    temp_posAccount.lockMoney = lockpay;
                    balance = (temp_balance - lockpay - waitpay).ToString("N0");
                    lookmany = lockpay.ToString("N0");
                    HttpCookie cookie = new HttpCookie(".ASPXAUTH");
                    cookie = FormsAuthentication.GetAuthCookie(posAccount.edong, true);
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate,
                        ticket.Expiration,
                        ticket.IsPersistent, JsonConvert.SerializeObject(posAccount), ticket.CookiePath);
                    var encTicket = FormsAuthentication.Encrypt(newTicket);
                    cookie.Value = encTicket;
                    cookie.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["Time"]));
                    context.Response.Cookies.Add(cookie);
                    ePOSSession.Del(posAccount.session);
                    ePOSSession.AddObject(posAccount.session, temp_posAccount);
                }
                else
                    Logging.ManagementLogger.ErrorFormat("get_Banlance => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("get_Banlance => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
        }

        [HttpPost]
        public JsonResult UploadFileWithdraw()
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

                    List<ObjReport> rows = new List<ObjReport>();
                    if (dsTemp.Tables[0].AsEnumerable().Skip(1).Count() <= 20)
                    {
                        Dictionary<string, ObjReport> dict = new Dictionary<string, ObjReport>();
                        for (int i = 0; i < dsTemp.Tables[0].AsEnumerable().Skip(1).Count(); i++)
                        {
                            Logging.ManagementLogger.InfoFormat("ReaderFileExcel => User: {0}, index: {1}, Session: {2}", posAccount.edong, i, posAccount.session);
                            ObjReport row = new ObjReport();
                            row.col_1 = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[0].ToString().Trim();
                            row.col_2 = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[1].ToString().Trim();// so vi                           
                            row.col_5 = string.IsNullOrEmpty(dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString().Trim()) == true ? string.Empty :
                                decimal.Parse(dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString().Trim()).ToString("N0");//so tien thu hoi                       
                            row.col_6 = "Thu hồi hạn mức Ví quầy";
                            row.col_8 = "true";
                            if (string.IsNullOrEmpty(row.col_2))
                            {
                                row.col_7 = "Số ví TNV để trống";
                                row.col_8 = "false";
                            }
                            else if (string.IsNullOrEmpty(row.col_5.Trim()))
                            {
                                row.col_7 = "Sổ tiền thu hồi để trống";
                                row.col_8 = "false";
                            }
                            if (row.col_8.CompareTo("true") == 0)
                                dict.Add(row.col_2, row);
                            rows.Add(row);
                        }
                        if (dict.Count() > 0)
                        {
                            string[] array = new string[dict.Count()];
                            for (int i = 0; i < dict.Count(); i++)
                                array[i] = dict.ElementAt(i).Key;
                            responseEntity entity = ePosDAO.GetReportEdongWallet(array, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                long balance = 0;
                                long waitpay = 0;
                                long lockpay = 0;
                                for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                                {
                                    balance = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).balance) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).balance);
                                    waitpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                                    lockpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).lockAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).lockAmount);
                                    long temp_balance = balance - lockpay - waitpay;
                                    for (int j = 0; j < rows.Count(); j++)
                                    {
                                        if (rows.ElementAt(j).col_2.CompareTo(entity.listResponseReportEdong.ElementAt(i).edong) == 0)
                                        {
                                            rows.ElementAt(j).col_3 = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).nameEdong) == true ? string.Empty : entity.listResponseReportEdong.ElementAt(i).nameEdong;
                                            rows.ElementAt(j).col_4 = temp_balance.ToString("N0");

                                            long amount = long.Parse(rows.ElementAt(j).col_5.Replace(",", "").Replace(".", ""));
                                            if (temp_balance <= 0)
                                            {
                                                rows.ElementAt(j).col_7 = "Số dư khả dụng phải > 0";
                                                rows.ElementAt(j).col_8 = "false";
                                            }
                                            if (amount > temp_balance)
                                            {
                                                rows.ElementAt(j).col_7 = "Số tiền thu hồi không được > Số dư khả dụng";
                                                rows.ElementAt(j).col_8 = "false";
                                            }
                                        }
                                    }
                                }
                                ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_WITHDRAW, rows);
                                return Json(new { Result = "SUCCESS", Records = rows, Message = posAccount.session + ePOSSession.UPLOAD_WITHDRAW });
                            }
                            else
                            {
                                Logging.ManagementLogger.ErrorFormat("ReaderFileExcel => User: {0}, Code: {1}, Error: {2}, Session: {2}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("ReaderFileExcel => User: {0}, Error: Dữ liệu File Trống, Session: {1}", posAccount.edong, posAccount.session);
                            return Json(new { Result = "ERROR", Message = "File dữ liệu để trống" });
                        }
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("ReaderFileExcel => User: {0}, Error: Số lượng bản ghi vượt hạn mức, Session: {1}", posAccount.edong, posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Số lượng bản ghi lớn hơn 20" });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UploadFileDebtNPC => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UploadFileDebtNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InsertFileWithdraw(string id)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(id) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            responseEntity entity = new responseEntity();
            try
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                for (int i = 0; i < items.Count(); i++)
                {
                    if (bool.Parse(items.ElementAt(i).col_8))
                    {
                        entity = ePosDAO.doTransferMoney(items.ElementAt(i).col_2, items.ElementAt(i).col_5, items.ElementAt(i).col_6, posAccount, true);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            items.ElementAt(i).col_7 = "Thành công";
                        }
                        else
                        {
                            Logging.ManagementLogger.ErrorFormat("InsertFileWithdraw => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            items.ElementAt(i).col_7 = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code));
                        }
                    }
                }
                string temp_balance = string.Empty;
                string temp_lookmany = string.Empty;
                get_Banlance(HttpContext, posAccount, ref temp_balance, ref temp_lookmany);
                ePosAccount temp_posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                return Json(new { Result = "SUCCESS", Records = items, balance = temp_balance, lockMoney = temp_lookmany });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("InsertFileWithdraw => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult CashMoney(string edong, string amount)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_CashMoney(edong, amount, posAccount);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.doTransferMoney(edong, amount, "Ứng ví tổng", posAccount, false);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        string temp_balance = string.Empty;
                        string temp_lookmany = string.Empty;
                        get_Banlance(HttpContext, posAccount, ref temp_balance, ref temp_lookmany);
                        ePosAccount temp_posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
                        return Json(new { Result = "SUCCESS", Message = "Ứng ví tổng thành công", balance = temp_balance, lockMoney = temp_lookmany });
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("CashMoney => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("CashMoney => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("CashMoney => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        #endregion

        #region Kiểm soát chấm nợ NPC
        [AllowAnonymous]
        public ActionResult ControlDebtNPC()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = Constant.CONTROLDEBT_TITLE;
            ViewBag.TitleLeft = "Chấm nợ NPC";
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
                Logging.ManagementLogger.ErrorFormat("ControlDebtNPC => UserName: {0}, Error: Ví quản lý chưa có danh sách điện lực, SessionId: {1}", posAccount.edong, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            List<SelectListItem> PCList = new List<SelectListItem>();
            PCList.Add(new SelectListItem { Value = "", Text = "--Chọn điện lực--" });
            foreach (var item in (from x in tempPosAcc.EvnPC where x.ext.StartsWith("PA") || x.ext.StartsWith("PH") select x))
            {
                PCList.Add(new SelectListItem { Value = item.ext, Text = item.ext + " - " + item.shortName });
            }
            model.PCList = PCList;
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchControlDebtNPC(string pccode)
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
                            Logging.ManagementLogger.ErrorFormat("SearchControlDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}, pcCode: {3}", posAccount.edong, posAccount.session, ex.Message, item.pcCode);
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
                    Logging.ManagementLogger.ErrorFormat("SearchControlDebtNPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("SearchControlDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddControlDebtNPC()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            ControlDebtModel model = new ControlDebtModel();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in (from x in tempPosAcc.EvnPC where x.ext.StartsWith("PA") || x.ext.StartsWith("PH") orderby x.ext select x).ToList())
            {
                items.Add(new SelectListItem { Value = item.ext, Text = item.ext + "-" + item.shortName });
            }
            model.PCList = items;
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
        public JsonResult AddDebtNPC(string pccode, string date_start, string time_start, string date_end, string time_end, string date_slow, string time_slow)
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
                        Logging.ManagementLogger.ErrorFormat("AddDebtNPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("AddDebtNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("AddDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditControlDebtNPC(string id, string index)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ObjReport data = JsonConvert.DeserializeObject<ObjReport>(id);
            ViewBag.pcCode = data.col_1;
            ViewBag.pcName = data.col_2;
            ViewBag.day1 = data.col_3;
            ViewBag.day2 = data.col_4;
            ViewBag.day3 = data.col_5;
            ViewBag.index = index;
            return PartialView();
        }

        [HttpPost]
        public JsonResult EditDebtNPC(string index, string pccode, string date_start, string time_start, string date_end, string time_end, string date_slow, string time_slow, string datasource)
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
                        Logging.ManagementLogger.ErrorFormat("EditDebtNPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("EditDebtNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("EditDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult DelDebtNPC(string id, string datasource)
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
                    Logging.ManagementLogger.ErrorFormat("DelDebtNPC => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("DelDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult DelAllDebtNPC(string bill, string datasource)
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
                    Logging.ManagementLogger.InfoFormat("DelAllDebtNPC => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, i, item.col_6, posAccount.session);
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
                        Logging.ManagementLogger.ErrorFormat("DelAllDebtNPC => User: {0}, Item: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, i, entity.code, entity.description, posAccount.session);
                        i_error++;
                    }
                }
                return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết xóa bản ghi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error), Records = temps });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("DelAllDebtNPC => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult UploadFileDebtNPC()
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
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_CONTROLDEBTNPC, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_CONTROLDEBTNPC });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UploadFileDebtNPC => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UploadFileDebtNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InsertFileContrlDebtNPC(string id)
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
                        Logging.ManagementLogger.InfoFormat("InsertFileContrlDebtNPC => User: {0}, index: {1}, Session: {2}", posAccount.edong, i, posAccount.session);
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
                                Logging.ManagementLogger.ErrorFormat("InsertFileContrlDebtNPC => User: {0}, index: {1}, Code: {2}, Error: {3} Session: {4}", posAccount.edong, i, entity.code, entity.description, posAccount.session);
                                row.col_6 = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code));
                            }
                        }
                        rows.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = rows, Message = "Đọc File thành công" });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("InsertFileContrlDebtNPC => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("InsertFileContrlDebtNPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }
        #endregion

        #region Kiểm soát chấm nợ GCS
        [AllowAnonymous]
        public ActionResult ControlDebtGCS()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ViewBag.Title = "CHẤM NỢ GCS";
            ViewBag.TitleLeft = "Chấm nợ GCS";
            ControlDebtModel model = new ControlDebtModel();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in Constant.EVN())
            {
                items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.CorporationList = items;
            model.PCList = ePosDAO.GetListPC("1", 2, posAccount);
            return View(model);
        }

        public JsonResult Search_ControlDebtGCS(string pccode)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                string result = Validate.check_ControlDebtGCS(pccode);
                if (string.IsNullOrEmpty(result))
                {
                    Dictionary<string, string> dict_pc = new Dictionary<string, string>();
                    List<ObjReport> data = new List<ObjReport>();
                    foreach (var item in JsonConvert.DeserializeObject<string[]>(pccode))
                    {
                        responseEntity entity = ePosDAO.EvnpcTime(item.Split('-')[0].Trim().ToUpper(), posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            for (int i = 0; i < entity.listEvnPcTime.Count(); i++)
                            {
                                if (entity.listEvnPcTime.ElementAt(i).checkKeep == 1)
                                {
                                    ObjReport items = new ObjReport();
                                    items.col_1 = entity.listEvnPcTime.ElementAt(i).pcCodeExt;
                                    items.col_2 = entity.listEvnPcTime.ElementAt(i).shortName;
                                    items.col_3 = entity.listEvnPcTime.ElementAt(i).offGcs;
                                    items.col_4 = entity.listEvnPcTime.ElementAt(i).offMax;
                                    items.col_5 = entity.listEvnPcTime.ElementAt(i).dayNotOff;
                                    items.col_6 = entity.listEvnPcTime.ElementAt(i).notWorkSat;
                                    items.col_7 = entity.listEvnPcTime.ElementAt(i).notWorkSun;
                                    items.col_8 = entity.listEvnPcTime.ElementAt(i).evnPcId.ToString();
                                    items.col_9 = entity.listEvnPcTime.ElementAt(i).regionId.ToString();
                                    data.Add(items);
                                }
                            }
                        }
                        else
                            Logging.ManagementLogger.ErrorFormat("Search_ControlDebtGCS => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, pc: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, item);
                    }
                    if (data.Count > 0)
                    {
                        string date = DateTime.Now.ToString();
                        ePOSSession.AddObject(Constant.CONTROL_DEBTGCS + date, data);
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = data.DistinctBy(x => x.col_1),
                            id = Constant.CONTROL_DEBTGCS + date
                        });
                    }
                    else
                        return Json(new { Result = "ERROR", Message = "Không tìm thấy bản ghi nào thỏa mãn nghiệp vụ" });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("Search_ControlDebtGCS => UserName: {0}, sessionId: {1}, Code: {2}", posAccount.edong, posAccount.session, result);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("Search_ControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _AddControlDebtGCS()
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            AddControlDebtModel model = new AddControlDebtModel();
            try
            {
                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var item in Constant.EVN())
                {
                    items.Add(new SelectListItem { Value = item.Key, Text = item.Value });
                }
                model.CorporationList = items;
                model.PCList = ePosDAO.GetListPC("1", 2, posAccount);
                model.Day_1_List = ePosDAO.getDays(20);
                model.Day_2_List = ePosDAO.getDays(20);
                model.Day_3_List = ePosDAO.getDays(31);
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("_AddControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
            }
            return PartialView(model);
        }

        public JsonResult doAddControlDebtGCS(string pc, string day_1, string day_2, string day_3, string check_T7, string check_CN)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                //string strCheckKeep = string.Empty;
                //if (string.IsNullOrEmpty(day_1) && string.IsNullOrEmpty(day_1) && string.IsNullOrEmpty(day_1))
                //    strCheckKeep = "0";
                //else
                //    strCheckKeep = "1";
                string result = Validate.check_ControlDebtGCS(pc, day_1, day_2, day_3);
                if (string.IsNullOrEmpty(result))
                {
                    int i_success = 0;
                    int i_error = 0;
                    foreach (var item in JsonConvert.DeserializeObject<string[]>(pc))
                    {
                        responseEntity entity = ePosDAO.MergeEvnPcTime(string.Empty, item, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "1", string.Empty, string.Empty, string.Empty,
                            day_1, day_2, day_3, check_T7.CompareTo("true") == 0 ? "1" : "0", check_CN.CompareTo("true") == 0 ? "1" : "0", posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            i_success++;
                        else
                        {
                            Logging.SupportLogger.ErrorFormat("doAddControlDebtGCS => User: {0}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            i_error++;
                        }
                    }
                    return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết thêm mới: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error) });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doAddControlDebtGCS => User: {0}, Code: {2}, Session: {3}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doAddControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public PartialViewResult _EditControlDebtGCS(string id, string index)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            AddControlDebtModel model = new AddControlDebtModel();
            try
            {
                ObjReport data = JsonConvert.DeserializeObject<ObjReport>(id);
                ViewBag.PCName = data.col_1 + " - " + data.col_2;
                ViewBag.Corporation = (from x in Constant.EVN() where x.Key == data.col_9 select x).FirstOrDefault().Value;
                ViewBag.index = index;
                model.Add_Day_1 = data.col_3;
                model.Add_Day_2 = data.col_4;
                model.Add_Day_3 = data.col_5;
                ViewBag.checkT7 = data.col_6;
                ViewBag.checkCN = data.col_7;
                ViewBag.PCId = data.col_8;
                model.Day_1_List = ePosDAO.getDays(20);
                model.Day_2_List = ePosDAO.getDays(20);
                model.Day_3_List = ePosDAO.getDays(31);
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("_AddControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
            }
            return PartialView(model);
        }

        public JsonResult doEitControlDebtGCS(string index, string pc, string day_1, string day_2, string day_3, string check_T7, string check_CN, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                string result = Validate.check_ControlDebtGCS(pc, day_1, day_2, day_3);
                if (string.IsNullOrEmpty(result))
                {
                    responseEntity entity = ePosDAO.MergeEvnPcTime(pc, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "1", string.Empty, string.Empty, string.Empty,
                        day_1, day_2, day_3, check_T7.CompareTo("true") == 0 ? "1" : "0", check_CN.CompareTo("true") == 0 ? "1" : "0", posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        items.ElementAt(int.Parse(index)).col_3 = day_1;
                        items.ElementAt(int.Parse(index)).col_4 = day_2;
                        items.ElementAt(int.Parse(index)).col_5 = day_3;
                        items.ElementAt(int.Parse(index)).col_6 = check_T7.CompareTo("true") == 0 ? "1" : "0";
                        items.ElementAt(int.Parse(index)).col_7 = check_CN.CompareTo("true") == 0 ? "1" : "0";
                        return Json(new { Result = "SUCCESS", Message = "Cập nhật bản ghi thành công", Records = items, index = index });
                    }
                    else
                    {
                        Logging.SupportLogger.ErrorFormat("doEitControlDebtGCS => User: {0}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, entity.code, entity.description, posAccount.session);
                        return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                    }
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doEitControlDebtGCS => User: {0}, Code: {2}, Session: {3}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doEitControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult doDelControlDebtGCS(string id, string datasource)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = JsonConvert.DeserializeObject<List<ObjReport>>(datasource);
                ObjReport item = items.ElementAt(int.Parse(id));
                responseEntity entity = ePosDAO.MergeEvnPcTime(item.col_8, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "0", string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    items.RemoveAt(int.Parse(id));
                    return Json(new { Result = "SUCCESS", Message = "Xóa bản ghi thành công", Records = items });
                }
                else
                {
                    Logging.SupportLogger.ErrorFormat("doDelControlDebtGCS => User: {0}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, entity.code, entity.description, posAccount.session);
                    return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doDelControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult doDelAllControlDebtGCS(string id, string datasource)
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
                foreach (string i in id.Substring(1, id.Length - 2).Split(','))
                {
                    ObjReport item = items.ElementAt(int.Parse(i));
                    Logging.ManagementLogger.InfoFormat("doDelAllControlDebtGCS => User: {0}, Item: {1}, RequestId: {2}, Session: {3}", posAccount.edong, i, item.col_1, posAccount.session);
                    responseEntity entity = ePosDAO.MergeEvnPcTime(item.col_8, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "0", string.Empty, string.Empty, string.Empty,
                        string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        foreach (var x in temps)
                        {
                            if (x.col_8 == item.col_8)
                            {
                                temps.Remove(x);
                                break;
                            }
                        }
                        i_success++;
                    }
                    else
                    {
                        Logging.ManagementLogger.ErrorFormat("doDelAllControlDebtGCS => User: {0}, Item: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, i, entity.code, entity.description, posAccount.session);
                        i_error++;
                    }
                }
                return Json(new { Result = "SUCCESS", Message = string.Format("Chi tiết xóa bản ghi: <br> - Thành công: {0} bản ghi. <br> - Không thành công: {1} bản ghi", i_success, i_error), Records = temps });
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("doDelAllControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        public JsonResult UploadFileControlDebtGCS()
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
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_CONTROLDEBTGCS, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_CONTROLDEBTGCS });
                }
                else
                {
                    Logging.ManagementLogger.ErrorFormat("UploadFileControlDebtGCS => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("UploadFileControlDebtGCS => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        public JsonResult InsertFileControlDebtGCS(string id)
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
                        bool flag = true;
                        ObjReport row = new ObjReport();
                        row.col_1 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[0].ToString().Trim();
                        row.col_2 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[1].ToString().Trim();
                        row.col_3 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString().Trim();
                        row.col_4 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[3].ToString().Trim();
                        row.col_5 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[4].ToString().Trim();
                        row.col_6 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[5].ToString().Trim();
                        row.col_7 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[6].ToString().Trim();
                        if (string.IsNullOrEmpty(row.col_1))
                        {
                            row.col_8 = "Mã điện lực để trống";
                            flag = false;
                        }
                        if (string.IsNullOrEmpty(row.col_3))
                        {
                            row.col_8 = "Số ngày giữ tính từ phiên GCS để trống";
                            flag = false;
                        }
                        try
                        {
                            int day_1 = int.Parse(row.col_3);
                            if (day_1 <= 0 || day_1 >= 21)
                            {
                                row.col_8 = "Số ngày giữ tính từ phiên GCS không đúng định dạng";
                                flag = false;
                            }
                        }
                        catch
                        {
                            row.col_8 = "Số ngày giữ tính từ phiên GCS không đúng định dạng";
                            flag = false;
                        }

                        if (string.IsNullOrEmpty(row.col_4))
                        {
                            row.col_8 = "Số ngày giữ tính từ ngày thu để trống";
                            flag = false;
                        }
                        try
                        {
                            int day_2 = int.Parse(row.col_4);
                            if (day_2 <= 0 || day_2 >= 21)
                            {
                                row.col_8 = "Số ngày giữ tính từ ngày thu không đúng định dạng";
                                flag = false;
                            }
                        }
                        catch
                        {
                            row.col_8 = "Số ngày giữ tính từ ngày thu không đúng định dạng";
                            flag = false;
                        }

                        if (string.IsNullOrEmpty(row.col_5))
                        {
                            row.col_8 = "Ngày bắt đầu không giữ để trống";
                            flag = false;
                        }
                        try
                        {
                            int day_3 = int.Parse(row.col_5);
                            if (day_3 <= 0 || day_3 >= 32)
                            {
                                row.col_8 = "Ngày bắt đầu không giữ không đúng định dạng";
                                flag = false;
                            }
                        }
                        catch
                        {
                            row.col_8 = "Ngày bắt đầu không giữ không đúng định dạng";
                            flag = false;
                        }
                        if (string.IsNullOrEmpty(row.col_6))
                        {
                            row.col_8 = "Chấm nợ EVN thứ 7 để trống";
                            flag = false;
                        }
                        if (row.col_6.CompareTo("0") != 0 && row.col_6.CompareTo("1") != 0)
                        {
                            row.col_8 = "Chấm nợ EVN thứ 7 không đúng định dạng";
                            flag = false;
                        }
                        if (string.IsNullOrEmpty(row.col_7))
                        {
                            row.col_8 = "Chấm nợ EVN CN để trống";
                            flag = false;
                        }
                        if (row.col_7.CompareTo("0") != 0 && row.col_7.CompareTo("1") != 0)
                        {
                            row.col_8 = "Chấm nợ EVN CN không đúng định dạng";
                            flag = false;
                        }
                        if (int.Parse(row.col_4) >= int.Parse(row.col_3))
                        {
                            row.col_8 = "Số ngày giữ sau ngày thu phải nhỏ hơn số ngày giữ sau phiên GCS";
                            flag = false;
                        }
                        if (int.Parse(row.col_5) <= int.Parse(row.col_3))
                        {
                            row.col_8 = "Ngày bắt đầu không giữ phải lớn hơn số ngày giữ sau phiên GCS";
                            flag = false;
                        }
                        if (flag)
                        {
                            responseEntity entity = ePosDAO.MergeEvnPcTime(string.Empty, row.col_1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "1", string.Empty, string.Empty, string.Empty,
                               row.col_3, row.col_4, row.col_5, row.col_6, row.col_7, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                row.col_8 = "Thành công";
                            else
                            {
                                Logging.SupportLogger.ErrorFormat("InsertFileControlDebtGCS => User: {0}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                row.col_8 = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code));
                            }
                        }
                        rows.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = rows });
                }
                else
                {
                    Logging.ImportLogger.ErrorFormat("InsertFileControlDebtGCS => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.ManagementLogger.ErrorFormat("InsertFileControlDebtGCS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        #endregion
    }
}