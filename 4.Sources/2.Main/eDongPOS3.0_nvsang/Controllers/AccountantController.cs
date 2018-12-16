using ePOS3.eStoreWS;
using ePOS3.Models;
using ePOS3.Utils;
using Excel;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ePOS3.Controllers
{
    [Authorize]
    [AllowAnonymous]
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class AccountantController : Controller
    {
        private static int Max_PC = int.Parse(Convert.ToString(ConfigurationManager.AppSettings["Max_PC"]));
       
        #region Danh sach no
        [AllowAnonymous]
        public ActionResult DebtList()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.RPT_DEBTLIST_TITLE;
            ViewBag.TitleLeft = "Danh sách khách hàng";
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            ReportDebtListModel model = new ReportDebtListModel();
            List<SelectListItem> EVNList = new List<SelectListItem>();
            foreach (var item in Constant.EVN())
            {
                EVNList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            model.EVNList = EVNList;
            model.PCList = EVNList;
            List<SelectListItem> StatusList = new List<SelectListItem>();         
            foreach (var item in Constant.StatusAccountant())
            {
                StatusList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }          
            model.StatusList = StatusList;
            model.PCList = ePosDAO.GetListPC("1", 2, posAccount);            
            return View(model);
        }

        [HttpPost]
        public JsonResult SearchDebtForBank(string pcCode = "", string customer = "", string fromdate = "", string todate = "", int status = 1, int pagenum = 0, int pagesize = 50)
        {           
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });          
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {             
                string result = Validate.checkDebtForBank(pcCode, fromdate, todate);           
                if (string.IsNullOrEmpty(result))
                {
                    string[] data = JsonConvert.DeserializeObject<string[]>(pcCode);                    
                    string[] array = new string[data.Count()];                
                    Dictionary<string, string> dict_pc = new Dictionary<string, string>();
                    for (int i = 0; i < data.Count(); i++)
                    {
                        dict_pc.Add(data[i].Split('-')[0].Trim().ToUpper(), data[i].Split('-')[1].Trim().ToUpper());
                        array[i] = data[i].Split('-')[0].Trim().ToUpper();
                    }               
                    List<ObjReport> items = new List<ObjReport>();
                    responseEntity entity = new responseEntity();
                    decimal total_Bill = 0;
                    decimal total_Amout = 0;                  
                    if (array.Length > Max_PC)
                    {                        
                        int index = array.Length % Max_PC > 0 == true ? (array.Length / Max_PC) + 1 : (array.Length / Max_PC);
                       
                        for(int i = 0; i< index; i++)
                        {
                            var tem_array = array.Skip(Max_PC * i).Take(Max_PC).ToArray();
                            entity = ePosDAO.getReportForBank(tem_array, customer, fromdate, todate, posAccount);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && !string.IsNullOrEmpty(entity.outputZip))
                            {
                                items = ReadFile.FillDebtForBank(items, dict_pc, entity.outputZip, status, ref total_Bill, ref total_Amout);
                            }else
                            {                                
                                Logging.EPOSLogger.ErrorFormat("SearchDebtForBank => UserName: {0}, sessionId: {1}, Code: {2}, Error: {3}, index: {4}", posAccount.edong, posAccount.session, entity.code, entity.description, i);
                            }
                        }
                    }
                    else
                    {
                        entity = ePosDAO.getReportForBank(array, customer, fromdate, todate, posAccount);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0 && !string.IsNullOrEmpty(entity.outputZip))
                        {
                            items = ReadFile.FillDebtForBank(items, dict_pc, entity.outputZip, status, ref total_Bill, ref total_Amout);                            
                        }
                        else
                        {
                            Logging.AccountantLogger.ErrorFormat("SearchDebtForBank => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                    if (items.Count > 0)
                    {
                        List<ObjReport> _temp = new List<ObjReport>();
                        int index = (pagenum * pagesize) + 1;
                        string totalRecord = items.Count().ToString();
                        foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
                        {
                            ObjReport row = new ObjReport();
                            row.col_0 = totalRecord;
                            row.col_1 = index.ToString("N0");
                            row.col_2 = item.col_2;
                            row.col_3 = item.col_3;
                            row.col_4 = item.col_4;
                            row.col_6 = item.col_6;
                            row.col_5 = item.col_5;
                            index++;
                            _temp.Add(row);
                        }
                        int _PageLast = 0;
                        if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_DEBT) != null)
                        {
                            _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_DEBT));
                        }
                        ePOSSession.AddObject(Constant.PAGE_SIZE_LAST_DEBT, pagesize);
                        int countItem = _temp.Count();
                        int MaxItem = pagesize;
                        if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
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
                                _temp.Insert(0, row);
                            }
                        }
                        string date = DateTime.Now.ToString();
                        ePOSSession.AddObject(Constant.REPORT_DEBT + date, items);
                        return Json(new
                        {
                            Result = "SUCCESS",
                            Records = _temp,
                            total_bill = decimal.Parse(totalRecord).ToString("N0"),
                            totalAmount = total_Amout.ToString("N0"),
                            id = Constant.REPORT_DEBT + date,
                            fromdate = fromdate,
                            todate = todate
                        });
                    }
                    else
                    {
                        Logging.AccountantLogger.ErrorFormat("SearchDebtForBank => User: {0}, Code: {1}, Error: {2}, SessionId: {3}", posAccount.edong, "000", items.Count(), posAccount.session);
                        return Json(new { Result = "ERROR", Message = "Không tìm thấy  bản ghi nào thỏa mãn nghiệp vụ" });
                    }                 
                }
                else
                {
                    Logging.AccountantLogger.ErrorFormat("SearchDebtForBank => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, result, posAccount.session);
                    return Json(new { Result = "ERROR", Message = result });
                }
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("SearchDebtForBank => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [HttpPost]
        public JsonResult SumContainer_PageChange(string id, int pagenum = 0, int pagesize = 50)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(id) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                List<ObjReport> _temp = new List<ObjReport>();
                int index = (pagenum * pagesize) + 1;
                string totalRecord = items.Count().ToString();
                foreach (var item in items.Skip(pagesize * (pagenum)).Take(pagesize))
                {
                    ObjReport row = new ObjReport();
                    row.col_0 = totalRecord;
                    row.col_1 = index.ToString("N0");
                    row.col_2 = item.col_2;
                    row.col_3 = item.col_3;
                    row.col_4 = item.col_4;
                    row.col_6 = item.col_6;
                    row.col_5 = item.col_5;
                    index++;
                    _temp.Add(row);
                }
                int _PageLast = 0;
                if (ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_DEBT) != null)
                {
                    _PageLast = Convert.ToInt16(ePOSSession.GetObject(Constant.PAGE_SIZE_LAST_DEBT));
                }
                ePOSSession.AddObject(Constant.PAGE_SIZE_LAST_DEBT, pagesize);
                int countItem = _temp.Count();
                int MaxItem = pagesize;
                if (countItem > 0 && countItem < pagesize && pagenum > 0 && _PageLast >= pagesize)
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
                        _temp.Insert(0, row);
                    }
                }              
                return Json(new
                {
                    Result = "SUCCESS",
                    Records = _temp,
                    total_bill = decimal.Parse(totalRecord).ToString("N0") 
                });               
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("SumContainer_PageChange => UserName: {0}, sessionId: {1}, Error: {2}", posAccount.edong, posAccount.session, ex.Message);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }

        [AllowAnonymous]
        public ActionResult ExportDebtBank(string id, string fromdate, string todate, string bill, string amount)
        {
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                var dir = new DirectoryInfo(Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["file"])));
                ExcelPackage epk = ePOSReport.DebtBank(id, dir + "Temp_DebtBank.xlsx", fromdate, todate, bill, amount, posAccount);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;  filename=BCKHNo.xlsx");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Charset = "utf-8";
                Response.BinaryWrite(epk.GetAsByteArray());
                Response.End();
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("ExportDebtBank => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return RedirectToAction("Login", "ePOS", true);
            }
            return View();
        }

        [HttpPost]
        public JsonResult UploadFile_HN()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ePOSSession.Del(posAccount.session + ePOSSession.UPLOAD_PCHANOI);
                string uploadDir = Constant.UPLOAD_FOLDER + @"\" + posAccount.edong + @"\" + DateTime.Today.ToString("yyyyMMdd");
                if (!Directory.Exists(Server.MapPath(uploadDir)))
                    Directory.CreateDirectory(Server.MapPath(uploadDir));               
                var pic = System.Web.HttpContext.Current.Request.Files["MyFile"]; 
                List<FileExcel_HN> data = new List<FileExcel_HN>();
                if (pic.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(pic.FileName);                 
                    var _ext = Path.GetExtension(pic.FileName);                  
                    string fileLocation = Server.MapPath(uploadDir + @"\" + fileName);                   
                    if (System.IO.File.Exists(fileLocation))                   
                        System.IO.File.Delete(fileLocation); 
                    pic.SaveAs(fileLocation); 
                    data = ReadFile.ReadExcelNPOI_HN(fileLocation, 10, 0, posAccount);
                }
                ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_PCHANOI, data);
                return Json(new { Result = "SUCCESS", Records = data, Message = posAccount.session + ePOSSession.UPLOAD_PCHANOI });
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("UploadFile_HN => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }
           
        [HttpPost]
        public JsonResult UploadFile_SPC()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                ePOSSession.Del(posAccount.session + ePOSSession.UPLOAD_PCHANOI);
                string uploadDir = Constant.UPLOAD_FOLDER + @"\" + posAccount.edong + @"\" + DateTime.Today.ToString("yyyyMMdd");
                if (!Directory.Exists(Server.MapPath(uploadDir)))
                    Directory.CreateDirectory(Server.MapPath(uploadDir));
                var pic = System.Web.HttpContext.Current.Request.Files["MyFile"];
                List<FileExcel_HN> data = new List<FileExcel_HN>();
                if (pic.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(pic.FileName);
                    var _ext = Path.GetExtension(pic.FileName);
                    string fileLocation = Server.MapPath(uploadDir + @"\" + fileName);
                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);
                    pic.SaveAs(fileLocation);
                    data = ReadFile.ReadExcel_SPC(fileLocation, posAccount);

                }
                ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_PCHANOI, data);
                return Json(new { Result = "SUCCESS", Records = data, Message = posAccount.session + ePOSSession.UPLOAD_PCHANOI });
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("UploadFile_SPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InsertFile(string id, string pc)
        {
            if (!ePOSController.CheckSession(HttpContext) || ePOSSession.GetObject(id) == null)
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                responseEntity entity = new responseEntity();

                List<FileExcel_HN> items = (List<FileExcel_HN>)ePOSSession.GetObject(id);
                int i_success = 0;
                int i_error = 0;
                for(int i = 0; i< items.Count; i++)
                {
                    entity = ePosDAO.MergeReportForBank(items.ElementAt(i), posAccount);
                    if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                    {
                        items.ElementAt(i).Desc = "Thành công";
                        i_success++;
                    }
                    else
                    {
                        Logging.AccountantLogger.ErrorFormat("InsertFile => User: {0}, pc: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, pc, entity.code, entity.description, posAccount.session);
                        items.ElementAt(i).Desc = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code));
                        i_error++;
                    }
                }
                ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_PCHANOI, items);
                return Json(new { Result = "SUCCESS", Records = items, success = i_success, error = i_error });
            }
            catch (Exception ex)
            {
                Logging.AccountantLogger.ErrorFormat("InsertFile => User: {0}, pc: {1}, Error: {2}, Session: {3}", posAccount.edong, pc, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm")) });
            }
        }
        #endregion

        
    }
}