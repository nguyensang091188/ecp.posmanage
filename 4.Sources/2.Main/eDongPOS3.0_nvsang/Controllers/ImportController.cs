using ePOS3.eStoreWS;
using ePOS3.Utils;
using Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    public class ImportController : Controller
    {
        private static string file = Convert.ToString(ConfigurationManager.AppSettings["file"]);

        #region hoa don giao thu
        [AllowAnonymous]
        public ActionResult Bill()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.IMPORTDATA_TITLE;         
            ViewBag.TitleLeft = "Hóa đơn giao thu";
            return View();
        }

        [HttpPost]
        public JsonResult UploadFileBill()
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
                    dsTemp.ReadXmlSchema(fileLocation);
                    dsTemp.ReadXml(fileLocation);
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_XMLBILL, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_XMLBILL });
                }
                else
                {
                    Logging.ImportLogger.ErrorFormat("UploadFileBill => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("UploadFileBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InserDataBill(string id)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if(ePOSSession.GetObject(id)!=null)
                {
                    DataXML items = new DataXML();
                    string strError = string.Empty;
                    items = ReadFile.readXmlBill((DataSet)ePOSSession.GetObject(id), posAccount, out strError);
                    if (string.IsNullOrEmpty(strError))
                    {
                        Logging.ImportLogger.InfoFormat("InsertDataBill => User: {0}, Msg = Bắt đầu đẩy file, Session: {1}", posAccount.edong, posAccount.session);
                        responseEntity entity = ePosDAO.doInsertBill(posAccount, items);
                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                        {
                            return Json(new { Result = "SUCCESS", Message = "Import file dữ liệu thành công", Records = items.report });
                        }
                        else
                        {
                            Logging.ImportLogger.ErrorFormat("InsertDataBill => User: {0}, Code: {1}, Error: {2}, Sessiong: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                            return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                        }
                    }
                    else
                    {
                        Logging.ImportLogger.ErrorFormat("InsertDataBill => User: {0}, Error: {1}, Sessiong: {2}", posAccount.edong, strError, posAccount.session);
                        return Json(new { Result = "ERROR", Message = strError });
                    }
                }
                else
                {
                    Logging.ImportLogger.ErrorFormat("InsertDataBill => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("InsertDataBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }        
        #endregion

        #region cham no
        [AllowAnonymous]
        public ActionResult EVNBillPayment()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.IMPORTBILLPAYMENT_TITLE;
            ViewBag.TitleLeft = "Chấm nợ";
            return View();
        }

        [HttpPost]
        public JsonResult UploadFileEVNBill()
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
                if (pic.ContentLength > 0) {
                    var fileName = Path.GetFileName(pic.FileName);
                    var _ext = Path.GetExtension(pic.FileName);
                    string fileLocation = Server.MapPath(uploadDir + @"\" + fileName);
                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);
                    pic.SaveAs(fileLocation);
                    DataSet dsTemp = new DataSet();
                    dsTemp.ReadXmlSchema(fileLocation);
                    dsTemp.ReadXml(fileLocation);
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_XMLBILLPAYMENT, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_XMLBILLPAYMENT, pathFile = fileLocation });
                }
                else
                {                    
                    Logging.ImportLogger.ErrorFormat("UploadFileEVNBill => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("UploadFileEVNBill => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }
        
        [HttpPost]
        public JsonResult InsertBillPayment(string id, string pathFile)
        {
            if (!ePOSController.CheckSession(HttpContext))
                return Json(new { redirectUrl = Url.Action("Login", "ePOS"), isRedirect = true });
            ePosAccount posAccount = (ePosAccount)JsonConvert.DeserializeObject(FormsAuthentication.Decrypt(Request.Cookies[".ASPXAUTH"].Value).UserData, typeof(ePosAccount));
            try
            {
                if (ePOSSession.GetObject(id) != null)
                {
                    DataSet ds = (DataSet)ePOSSession.GetObject(id);
                    if (!ReadFile.Check_DataFileImport(ds, posAccount))
                    {
                        System.IO.File.Delete(pathFile);
                        return Json(new { Result = "ERROR", Message = "Sai nội dung file dữ liệu" });
                    }
                    else
                    {
                        string strError = string.Empty;
                        DataXML items = ReadFile.readBillPayment(ds, posAccount, out strError);
                        if (string.IsNullOrEmpty(strError))
                        {
                            Logging.BillLogger.InfoFormat("InsertBillPayment => User: {0}, Msg = Bắt đầu đẩy file, Session: {1}", posAccount.edong, posAccount.session);
                            responseEntity entity = ePosDAO.doInsertBill(posAccount, items);
                            if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                            {
                                Logging.BillLogger.InfoFormat("InsertBillPayment => User: {0}, Msg = Bắt đầu đẩy file, Session: {1}", posAccount.edong, posAccount.session);
                                List<ObjReport> data = new List<ObjReport>();
                                long temp_amount = 0;
                                long amount_error = 0;
                                long temp_bill = 0;
                                long bill_error = 0;
                                for (int i = 0; i < items.transOff.Count(); i++)
                                {
                                    ObjReport row = new ObjReport();
                                    entity = ePosDAO.TransactionOff(items.transOff.ElementAt(i), posAccount);
                                    row.col_1 = items.transOff.ElementAt(i).strBillId;
                                    row.col_2 = items.transOff.ElementAt(i).customerCode;
                                    row.col_3 = items.transOff.ElementAt(i).pcCode;
                                    row.col_4 = string.IsNullOrEmpty(items.transOff.ElementAt(i).strAmount) == true ? "0" : long.Parse(items.transOff.ElementAt(i).strAmount).ToString("N0");
                                    try
                                    {
                                        row.col_5 = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code));
                                        if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                        {
                                            temp_amount = temp_amount + (string.IsNullOrEmpty(items.transOff.ElementAt(i).strAmount) == true ? 0 : long.Parse(items.transOff.ElementAt(i).strAmount));
                                            temp_bill = temp_bill + 1;
                                        }
                                        else
                                        {
                                            amount_error = amount_error + (string.IsNullOrEmpty(items.transOff.ElementAt(i).strAmount) == true ? 0 : long.Parse(items.transOff.ElementAt(i).strAmount));
                                            bill_error = bill_error + 1;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logging.BillLogger.ErrorFormat("InsertBillPayment => User: {0}, Bill: {1}, Error: {2}, Session: {3}", posAccount.edong, items.transOff.ElementAt(i).strBillId, ex.Message, posAccount.session);
                                        row.col_5 = "Lỗi hệ thống";
                                        amount_error = amount_error + (string.IsNullOrEmpty(items.transOff.ElementAt(i).strAmount) == true ? 0 : long.Parse(items.transOff.ElementAt(i).strAmount));
                                        bill_error = bill_error + 1;
                                    }
                                    data.Add(row);
                                }
                                ePOSSession.Del(id);
                                return Json(new
                                {
                                    Result = "SUCCESS",
                                    Message = "Import File chấm nợ thành công",
                                    Records = data,
                                    amount_error = amount_error.ToString("N0"),
                                    bill_error = bill_error.ToString("N0"),
                                    amount = temp_amount.ToString("N0"),
                                    bill = temp_bill.ToString("N0")
                                });
                            }
                            else
                            {
                                Logging.ImportLogger.ErrorFormat("InsertBillPayment => User: {0}, Code: {1}, Error: {2}, Session: {3}", posAccount.edong, entity.code, entity.description, posAccount.session);
                                ePOSSession.Del(id);
                                return Json(new { Result = "ERROR", Message = ConvertResponseCode.GetResponseDescription(int.Parse(entity.code)) });
                            }
                        }
                        else
                        {
                            ePOSSession.Del(id);
                            Logging.ImportLogger.ErrorFormat("InsertBillPayment => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                            return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                        }
                    }
                }
                else
                {
                    Logging.ImportLogger.ErrorFormat("InsertBillPayment => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("InsertBillPayment => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }        
        #endregion

        #region FileBookCMIS
        [AllowAnonymous]
        public ActionResult MapBookCMIS()
        {
            if (!ePOSController.CheckSession(HttpContext))
                return RedirectToAction("Login", "ePOS", true);
            ViewBag.Title = Constant.IMPORTBOOKCMIS_TITLE;
            ViewBag.TitleLeft = "Gán giao thu";
            return View();
        }

        [HttpPost]
        public JsonResult UploadFileBookCMIS()
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
                    ePOSSession.AddObject(posAccount.session + ePOSSession.UPLOAD_BOOKCMIS, dsTemp);
                    return Json(new { Result = "SUCCESS", Message = posAccount.session + ePOSSession.UPLOAD_BOOKCMIS });
                }else
                {
                    Logging.ImportLogger.ErrorFormat("UploadFileBookCMIS => User: {0}, Error: Lỗi đọc file", posAccount.edong);
                    return Json(new { Result = "ERROR", Message = "Đọc file lỗi" });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("UploadFileBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }

        [HttpPost]
        public JsonResult InsertFileBookCMIS(string id)
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

                        row.col_1 = (i + 1).ToString();
                        row.col_2 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[0].ToString().Trim();//account
                        row.col_3 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[1].ToString().Trim();//pccode
                        row.col_4 = ds.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString().Trim();// bookcmis

                        if (string.IsNullOrEmpty(row.col_2))
                        {
                            row.col_5 = "Ví gán để trống";
                            flag = false;
                        }
                        if (string.IsNullOrEmpty(row.col_3))
                        {
                            row.col_5 = "Mã điện lực để trống";
                            flag = false;
                        }
                        if (string.IsNullOrEmpty(row.col_4))
                        {
                            row.col_5 = "Sổ GCS để trống";
                            flag = false;
                        }
                        if (flag)
                        {
                            Logging.ImportLogger.InfoFormat("InsertFileBookCMIS => User: {0}, pcCode: {1}, Account: {2}, BookCMIS: {3}, session: {4}", posAccount.edong, row.col_3, row.col_2, row.col_4, posAccount.session);
                            string success = string.Empty;
                            string error = string.Empty;
                            foreach (var item in PhoneNumber.ProcessCustomerGroup(row.col_4.Trim()))
                            {
                                responseEntity entity = ePosDAO.mergeAccountBookcmisMapping(row.col_2.Trim(), item, row.col_3.Trim(), Constant.STATUS_ONLINE, posAccount);
                                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                                    success = string.IsNullOrEmpty(success) == true ? "Thành công: " + item : success + ", " + item;
                                else
                                {
                                    Logging.ImportLogger.ErrorFormat("InsertFileBookCMIS => User: {0}, BookCMIS: {1}, Code: {2}, Error: {3}, Session: {4}", posAccount.edong, item, entity.code, entity.description, posAccount.session);
                                    error = string.IsNullOrEmpty(error) == true ? "Lỗi: " + item : error + ", " + item;
                                }
                            }
                            if (string.IsNullOrEmpty(success))
                                row.col_5 = error;
                            else
                                row.col_5 = success + ". " + error;
                        }
                        rows.Add(row);
                    }
                    return Json(new { Result = "SUCCESS", Records = rows });
                }
                else
                {
                    Logging.ImportLogger.ErrorFormat("InsertFileBookCMIS => User: {0}, Error: Phiên làm việc không tồn tai, Sessiong: {1}", posAccount.edong, posAccount.session);
                    return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("InsertFileBookCMIS => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return Json(new { Result = "ERROR", Message = Constant.CONNECTION_ERROR_DESC });
            }
        }       
        #endregion

        #region FileTemplate
        [AllowAnonymous]
        public FileResult Download(string filename)
        {
            var dir = new DirectoryInfo(Server.MapPath(file));

            return File(dir + filename, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }
        #endregion
    }
}