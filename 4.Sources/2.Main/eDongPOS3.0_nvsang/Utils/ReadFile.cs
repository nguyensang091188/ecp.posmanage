using ePOS3.eStoreWS;
using Excel;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;


namespace ePOS3.Utils
{
    public class ReadFile
    {
        private static string convertSo(object val)
        {
            string strVal = val.ToString();
            string.Join("", new String(strVal.Where(Char.IsDigit).ToArray()));
            if (strVal == "") { strVal = "0"; }
            return strVal;
        }

        public static DataXML readXmlBill(DataSet ds, ePosAccount posAccount, out string strError)
        {
            DataXML data = new DataXML();
            List<bill> bills = new List<bill>();
            List<customer> customers = new List<customer>();
            List<ObjReport> report = new List<ObjReport>();
            strError = "";
            try
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    if (string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()))
                    {
                        strError = "Mã khách hàng thanh toán trống";
                        break;
                    }
                    if (string.IsNullOrEmpty(dataRow["TEN_KHANG"].ToString()))
                    {
                        strError = "Tên khách hàng thanh toán trống";
                        break;
                    }
                    if (string.IsNullOrEmpty(dataRow["DCHI_KHANG"].ToString()))
                    {
                        strError = "Địa chỉ khách thanh toán trống";
                        break;
                    }
                    //if (string.IsNullOrEmpty(dataRow["ID_HDON"].ToString()))
                    if (string.IsNullOrEmpty(dataRow["MAHD"].ToString()))
                    {
                        strError = "Mã hóa đơn để trống";
                        break;
                    }
                    if (string.IsNullOrEmpty(dataRow["THANG_PS"].ToString()) || string.IsNullOrEmpty(dataRow["NAM_PS"].ToString()))
                    {
                        strError = "Tháng hóa đơn để trống";
                        break;
                    }
                    if (string.IsNullOrEmpty(dataRow["TIEN_NO"].ToString()))
                    {
                        strError = "Tổng tiền hóa đơn trống";
                        break;
                    }
                    if (string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()))
                    {
                        strError = "Sổ ghi chỉ số trống";
                        break;
                    }

                    bill b = new bill();
                    b.customerCode = string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()) == true ? "" : dataRow["MA_KHANG"].ToString();
                    //b.strBillId = string.IsNullOrEmpty(dataRow["ID_HDON"].ToString()) == true ? "0" : dataRow["ID_HDON"].ToString();
                    b.strBillId = string.IsNullOrEmpty(dataRow["MAHD"].ToString()) == true ? "0" : dataRow["MAHD"].ToString();
                    b.strTerm = string.IsNullOrEmpty(dataRow["THANG_PS"].ToString()) == true || string.IsNullOrEmpty(dataRow["NAM_PS"].ToString()) == true ?
                           DateTime.Now.ToString("dd/MM/yyyy") : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                    int.Parse(dataRow["THANG_PS"].ToString()),
                                    1).ToString("dd/MM/yyyy");
                    long amount = string.IsNullOrEmpty(dataRow["TIEN_NO"].ToString()) == true ? 0 : long.Parse(dataRow["TIEN_NO"].ToString());
                    long amountTax = string.IsNullOrEmpty(dataRow["THUE_NO"].ToString()) == true ? 0 : long.Parse(dataRow["THUE_NO"].ToString());
                    b.strAmount = (amount + amountTax).ToString();
                    b.period = string.IsNullOrEmpty(dataRow["KY_PS"].ToString()) == true ? "" : dataRow["KY_PS"].ToString();
                    b.strStatus = "0";
                    b.seri = string.IsNullOrEmpty(dataRow["SO_SERY"].ToString()) == true ? "" : dataRow["SO_SERY"].ToString();
                    b.pcCode = string.IsNullOrEmpty(dataRow["MA_DVIQLY"].ToString()) == true ? "" : dataRow["MA_DVIQLY"].ToString();
                    b.handoverCode = string.IsNullOrEmpty(dataRow["SO_BBGIAO"].ToString()) == true ? "" : dataRow["SO_BBGIAO"].ToString();
                    b.cashierCode = "";
                    b.bookCmis = string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()) == true ? "" : dataRow["MA_SOGCS"].ToString();
                    if (int.Parse(dataRow["THANG_PS"].ToString()) == 1)
                    {
                        b.strFromDate = string.IsNullOrEmpty(dataRow["NGAY_DKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                                : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                    int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[1]),
                                    int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                        b.strToDate = string.IsNullOrEmpty(dataRow["NGAY_CKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                            : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[1]),
                                int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        b.strFromDate = string.IsNullOrEmpty(dataRow["NGAY_DKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                               : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                   int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[1]),
                                   int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                        b.strToDate = string.IsNullOrEmpty(dataRow["NGAY_CKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                            : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[1]),
                                int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                    }
                    b.home = string.IsNullOrEmpty(dataRow["SO_HO"].ToString()) == true ? "" : dataRow["SO_HO"].ToString();
                    b.strTax = "0";
                    b.billNum = string.IsNullOrEmpty(dataRow["STT"].ToString()) == true ? "" : dataRow["STT"].ToString();
                    b.currency = "VND";
                    b.priceDetails = string.IsNullOrEmpty(dataRow["CTIET_GIA"].ToString()) == true ? "" : dataRow["CTIET_GIA"].ToString();
                    b.numeDetails = string.IsNullOrEmpty(dataRow["CTIET_DNTT"].ToString()) == true ? "" : dataRow["CTIET_DNTT"].ToString();
                    b.amountDetails = string.IsNullOrEmpty(dataRow["CTIET_TIEN"].ToString()) == true ? "" : dataRow["CTIET_TIEN"].ToString();
                    b.oldIndex = string.IsNullOrEmpty(dataRow["CS_DKY"].ToString()) == true ? "" : dataRow["CS_DKY"].ToString();
                    b.newIndex = string.IsNullOrEmpty(dataRow["CS_CKY"].ToString()) == true ? "" : dataRow["CS_CKY"].ToString();
                    b.nume = string.IsNullOrEmpty(dataRow["DIEN_TTHU"].ToString()) == true ? "" : dataRow["DIEN_TTHU"].ToString();
                    b.strAmountNotTax = string.IsNullOrEmpty(dataRow["TIEN_PSINH"].ToString()) == true ? "0" : dataRow["TIEN_PSINH"].ToString();
                    b.strAmountTax = string.IsNullOrEmpty(dataRow["THUE_PSINH"].ToString()) == true ? "0" : dataRow["THUE_PSINH"].ToString();
                    b.strMultiple = "0";
                    b.billType = string.IsNullOrEmpty(dataRow["LOAI_HDON"].ToString()) == true ? "" : dataRow["LOAI_HDON"].ToString();
                    b.typeIndex = string.IsNullOrEmpty(dataRow["LOAI_PSINH"].ToString()) == true ? "" : dataRow["LOAI_PSINH"].ToString();
                    b.groupTypeIndex = "";
                    bills.Add(b);

                    customer c = new customer();
                    c.pcCode = string.IsNullOrEmpty(dataRow["MA_DVIQLY"].ToString()) == true ? "" : dataRow["MA_DVIQLY"].ToString();
                    c.code = string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()) == true ? "" : dataRow["MA_KHANG"].ToString();
                    c.name = string.IsNullOrEmpty(dataRow["TEN_KHANG"].ToString()) == true ? "" : dataRow["TEN_KHANG"].ToString();
                    c.address = string.IsNullOrEmpty(dataRow["DCHI_KHANG"].ToString()) == true ? "" : dataRow["DCHI_KHANG"].ToString();
                    c.phoneByevn = string.IsNullOrEmpty(dataRow["DTHOAI_KHANG"].ToString()) == true ? "" : dataRow["DTHOAI_KHANG"].ToString();
                    c.bookCmis = string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()) == true ? "" : dataRow["MA_SOGCS"].ToString();
                    c.electricityMeter = string.IsNullOrEmpty(dataRow["SO_CTO"].ToString()) == true ? "" : dataRow["SO_CTO"].ToString();
                    c.team = string.IsNullOrEmpty(dataRow["MANHOM_KHANG"].ToString()) == true ? "" : dataRow["MANHOM_KHANG"].ToString();
                    c.strType = string.IsNullOrEmpty(dataRow["LOAI_KHANG"].ToString()) == true ? "0" : dataRow["LOAI_KHANG"].ToString();
                    customers.Add(c);

                    ObjReport r = new ObjReport();
                    r.col_1 = string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()) == true ? "" : dataRow["MA_SOGCS"].ToString();
                    r.col_2 = string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()) == true ? "" : dataRow["MA_KHANG"].ToString();
                    r.col_3 = string.IsNullOrEmpty(dataRow["TEN_KHANG"].ToString()) == true ? "" : dataRow["TEN_KHANG"].ToString();
                    r.col_4 = string.IsNullOrEmpty(dataRow["DCHI_KHANG"].ToString()) == true ? "" : dataRow["DCHI_KHANG"].ToString();
                    //r.col_5 = string.IsNullOrEmpty(dataRow["ID_HDON"].ToString()) == true ? "0" : dataRow["ID_HDON"].ToString();
                    r.col_5 = string.IsNullOrEmpty(dataRow["MAHD"].ToString()) == true ? "0" : dataRow["MAHD"].ToString();
                    r.col_6 = string.IsNullOrEmpty(dataRow["KY_PS"].ToString()) == true ? "" : dataRow["KY_PS"].ToString();
                    r.col_7 = string.IsNullOrEmpty(dataRow["DIEN_TTHU"].ToString()) == true ? "0" : long.Parse(dataRow["DIEN_TTHU"].ToString()).ToString("N0");
                    r.col_8 = amount.ToString("N0");
                    r.col_9 = amountTax.ToString("N0");
                    r.col_10 = (amount + amountTax).ToString("N0");
                    report.Add(r);

                    data.bill = bills;
                    data.customer = customers;
                    data.report = report;
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("readXmlBill => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, ex.Message, posAccount.session);
                strError = "Lỗi dữ liệu hóa đơn";
            }
            return data;
        }

        public static List<ObjLogViewer> ReaderLogFile(List<FileInfo> files, DateTime fromDate, DateTime toDate, ePosAccount posAccount)
        {
            int index = 1;
            DateTime dtime;
            List<ObjLogViewer> items = new List<ObjLogViewer>();
            foreach (var item in files)
            {
                if (item.LastWriteTime <= toDate.AddDays(1) && item.LastWriteTime >= fromDate)
                {
                    string[] lines = File.ReadAllLines(item.FullName);
                    foreach (var i in lines)
                    {
                        try
                        {
                            if (i.Trim().Length >= 10 && DateTime.TryParseExact(i.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtime))
                            {
                                if (dtime <= fromDate && dtime >= toDate)
                                    items.Add(new ObjLogViewer
                                    {
                                        No = index++,
                                        Date = DateTime.ParseExact(i.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                        DateLog = i.Substring(0, 10) + i.Substring(10, 13),
                                        Type = i.Substring(23, 6).Trim(),
                                        Content = i.Substring(29)
                                    });
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.ImportLogger.ErrorFormat("ReaderFileLog => UserName: {0}, Error:{1}, SessionId: {2}", posAccount.edong, ex.ToString(), posAccount.session);
                        }
                    }
                }
            }
            return items;
        }

        public static DataXML readBillPayment(DataSet ds, ePosAccount posAccount, out string strError)
        {
            DataXML data = new DataXML();
            List<bill> bills = new List<bill>();
            List<customer> customers = new List<customer>();
            List<transactionOff> transOff = new List<transactionOff>();
            strError = "";
            try
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    bool flag = true;
                    if (string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()))
                    {
                        flag = false;
                    }
                    if (string.IsNullOrEmpty(dataRow["TEN_KHANG"].ToString()))
                    {
                        flag = false;
                    }
                    if (string.IsNullOrEmpty(dataRow["DCHI_KHANG"].ToString()))
                    {
                        flag = false;
                    }
                    //if (string.IsNullOrEmpty(dataRow["ID_HDON"].ToString()))
                    if (string.IsNullOrEmpty(dataRow["MAHD"].ToString()))
                    {
                        flag = false;
                    }
                    if (string.IsNullOrEmpty(dataRow["THANG_PS"].ToString()) || string.IsNullOrEmpty(dataRow["NAM_PS"].ToString()))
                    {
                        flag = false;
                    }
                    if (string.IsNullOrEmpty(dataRow["TIEN_NO"].ToString()))
                    {
                        flag = false;
                    }
                    if (string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()))
                    {
                        flag = false;
                    }

                    if (flag)
                    {
                        bill b = new bill();
                        b.customerCode = dataRow["MA_KHANG"] == null ? "" : dataRow["MA_KHANG"].ToString();
                        b.strBillId = string.IsNullOrEmpty(dataRow["MAHD"].ToString()) == true ? "" : dataRow["MAHD"].ToString();
                        b.strTerm = string.IsNullOrEmpty(dataRow["THANG_PS"].ToString()) == true || string.IsNullOrEmpty(dataRow["NAM_PS"].ToString()) == true ?
                            DateTime.Now.ToString("dd/MM/yyyy") : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                int.Parse(dataRow["THANG_PS"].ToString()), 1).ToString("dd/MM/yyyy");
                        long amount = string.IsNullOrEmpty(dataRow["TIEN_NO"].ToString()) == true ? 0 : long.Parse(dataRow["TIEN_NO"].ToString());
                        long amountTax = string.IsNullOrEmpty(dataRow["THUE_NO"].ToString()) == true ? 0 : long.Parse(dataRow["THUE_NO"].ToString());
                        b.strAmount = (amount + amountTax).ToString();
                        b.period = string.IsNullOrEmpty(dataRow["KY_PS"].ToString()) == true ? "" : dataRow["KY_PS"].ToString();
                        b.strStatus = "0";
                        b.seri = string.IsNullOrEmpty(dataRow["SO_SERY"].ToString()) == true ? "" : dataRow["SO_SERY"].ToString();
                        b.pcCode = string.IsNullOrEmpty(dataRow["MA_DVIQLY"].ToString()) == true ? "" : ConvertPCCode(dataRow["MA_DVIQLY"].ToString());
                        b.handoverCode = string.IsNullOrEmpty(dataRow["SO_BBGIAO"].ToString()) == true ? "" : dataRow["SO_BBGIAO"].ToString();
                        b.cashierCode = "";
                        b.bookCmis = string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()) == true ? "" : dataRow["MA_SOGCS"].ToString();
                        if (int.Parse(dataRow["THANG_PS"].ToString()) == 1)
                        {
                            b.strFromDate = string.IsNullOrEmpty(dataRow["NGAY_DKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                                : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                    int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[1]),
                                    int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                            b.strToDate = string.IsNullOrEmpty(dataRow["NGAY_CKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                                : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                    int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[1]),
                                    int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            b.strFromDate = string.IsNullOrEmpty(dataRow["NGAY_DKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                               : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                   int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[1]),
                                   int.Parse(dataRow["NGAY_DKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                            b.strToDate = string.IsNullOrEmpty(dataRow["NGAY_CKY"].ToString()) == true ? DateTime.Now.ToString("dd/MM/yyyy")
                                : new DateTime(int.Parse(dataRow["NAM_PS"].ToString()),
                                    int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[1]),
                                    int.Parse(dataRow["NGAY_CKY"].ToString().Split('/')[0])).ToString("dd/MM/yyyy");
                        }
                        b.home = string.IsNullOrEmpty(dataRow["SO_HO"].ToString()) == true ? "" : dataRow["SO_HO"].ToString();
                        b.strTax = "0";
                        b.billNum = string.IsNullOrEmpty(dataRow["STT"].ToString()) == true ? "" : dataRow["STT"].ToString();
                        b.currency = "VND";
                        b.priceDetails = string.IsNullOrEmpty(dataRow["CTIET_GIA"].ToString()) == true ? "" : dataRow["CTIET_GIA"].ToString();
                        b.numeDetails = string.IsNullOrEmpty(dataRow["CTIET_DNTT"].ToString()) == true ? "" : dataRow["CTIET_DNTT"].ToString();
                        b.amountDetails = string.IsNullOrEmpty(dataRow["CTIET_TIEN"].ToString()) == true ? "" : dataRow["CTIET_TIEN"].ToString();
                        b.oldIndex = string.IsNullOrEmpty(dataRow["CS_DKY"].ToString()) == true ? "" : dataRow["CS_DKY"].ToString();
                        b.newIndex = string.IsNullOrEmpty(dataRow["CS_CKY"].ToString()) == true ? "" : dataRow["CS_CKY"].ToString();
                        b.nume = string.IsNullOrEmpty(dataRow["DIEN_TTHU"].ToString()) == true ? "" : dataRow["DIEN_TTHU"].ToString();
                        b.strAmountNotTax = string.IsNullOrEmpty(dataRow["TIEN_PSINH"].ToString()) == true ? "0" : dataRow["TIEN_PSINH"].ToString();
                        b.strAmountTax = string.IsNullOrEmpty(dataRow["THUE_PSINH"].ToString()) == true ? "0" : dataRow["THUE_PSINH"].ToString();
                        b.strMultiple = "0";
                        b.billType = string.IsNullOrEmpty(dataRow["LOAI_HDON"].ToString()) == true ? "" : dataRow["LOAI_HDON"].ToString();
                        b.typeIndex = string.IsNullOrEmpty(dataRow["LOAI_PSINH"].ToString()) == true ? "" : dataRow["LOAI_PSINH"].ToString();
                        b.groupTypeIndex = "";
                        bills.Add(b);

                        customer c = new customer();
                        c.pcCode = string.IsNullOrEmpty(dataRow["MA_DVIQLY"].ToString()) == true ? "" : ConvertPCCode(dataRow["MA_DVIQLY"].ToString());
                        c.code = string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()) == true ? "" : dataRow["MA_KHANG"].ToString();
                        c.name = string.IsNullOrEmpty(dataRow["TEN_KHANG"].ToString()) == true ? "" : dataRow["TEN_KHANG"].ToString();
                        c.address = string.IsNullOrEmpty(dataRow["DCHI_KHANG"].ToString()) == true ? "" : dataRow["DCHI_KHANG"].ToString();
                        c.phoneByevn = string.IsNullOrEmpty(dataRow["DTHOAI_KHANG"].ToString()) == true ? "" : dataRow["DTHOAI_KHANG"].ToString();
                        c.bookCmis = string.IsNullOrEmpty(dataRow["MA_SOGCS"].ToString()) == true ? "" : dataRow["MA_SOGCS"].ToString();
                        c.electricityMeter = string.IsNullOrEmpty(dataRow["SO_CTO"].ToString()) == true ? "" : dataRow["SO_CTO"].ToString();
                        c.team = string.IsNullOrEmpty(dataRow["MANHOM_KHANG"].ToString()) == true ? "" : dataRow["MANHOM_KHANG"].ToString();
                        c.strType = string.IsNullOrEmpty(dataRow["LOAI_KHANG"].ToString()) == true ? "0" : dataRow["LOAI_KHANG"].ToString();

                        customers.Add(c);

                        Random random = new Random();
                        var newAudit = 99999999 + random.Next(900000000);
                        transactionOff item = new transactionOff();
                        item.customerCode = string.IsNullOrEmpty(dataRow["MA_KHANG"].ToString()) == true ? "" : dataRow["MA_KHANG"].ToString();
                        item.pcCode = string.IsNullOrEmpty(dataRow["MA_DVIQLY"].ToString()) == true ? "" : ConvertPCCode(dataRow["MA_DVIQLY"].ToString());

                        item.strBillId = string.IsNullOrEmpty(dataRow["MAHD"].ToString()) == true ? "" : dataRow["MAHD"].ToString();
                        item.strAuditNumber = newAudit.ToString();
                        item.strAmount = (amount + amountTax).ToString();
                        item.cashierPay = posAccount.edong;
                        item.edong = posAccount.edong;
                        item.strOffType = "4";
                        transOff.Add(item);

                        data.bill = bills;
                        data.customer = customers;
                        data.transOff = transOff;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("readBillPayment => User: {0}, Error: {1}, SessionId: {2}", posAccount.edong, ex.Message, posAccount.session);
                strError = "Lỗi dữ liệu hóa đơn";
            }
            return data;
        }

        public static bool Check_DataFileImport(DataSet ds, ePosAccount posAccount)
        {
            try
            {
                long i = ds.Tables[0].Rows.Count;
                string expression = "(1 = 1)";
                if (i == ds.Tables[0].Rows.Count)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("Check_DataFile => User: {0}, Error:{1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return false;
            }
        }

        private static string ConvertPCCode(string PCCode)
        {
            if (PCCode.StartsWith("PE"))
                return PCCode.Substring(0, 4);
            else
                return PCCode;
        }

        public static List<ObjReport> ReportGeneral(List<ObjReport> items, responseReportCollectGeneral[] data, Dictionary<string, string> dict_acc, ePosAccount posAccount, int index)
        {
            foreach (var item in data.GroupBy(x => x.edong).Select(x => x).ToList())
            {
                ObjReport row = new ObjReport();
                row.col_1 = item.Key;
                row.col_2 = "0";
                row.col_3 = "0";
                row.col_4 = "0";
                row.col_5 = "0";
                row.col_6 = "0";
                row.col_7 = "0";
                row.col_8 = "0";
                row.col_9 = "0";
                row.col_10 = "0";
                row.col_11 = "0";
                row.col_12 = "0";
                row.col_13 = "0";
                row.col_14 = "0";
                row.col_15 = "0";
                row.col_21 = "0";
                row.col_22 = "0";
                row.col_23 = index + "";
                double Total_bill = 0;
                double Total_amout = 0;
                double Return_amount = 0;
                double Return_bill = 0;
              
                for (int i = 0; i < item.Count(); i++)
                {
                    switch (item.ElementAt(i).responseCode)
                    {
                        case "BILLING_TYPE_BILLING":
                            row.col_2 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_3 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Total_bill = Total_bill + long.Parse(item.ElementAt(i).totalBill);
                            Total_amout = Total_amout + long.Parse(item.ElementAt(i).totalAmount);
                            break;
                        case "BILLING_TYPE_OFF":
                            row.col_4 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_5 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Total_bill = Total_bill + long.Parse(item.ElementAt(i).totalBill);
                            Total_amout = Total_amout + long.Parse(item.ElementAt(i).totalAmount);
                            break;
                        case "BILLING_TYPE_SOURCE_OTHER":
                            row.col_6 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_7 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "BILLING_TYPE_EDONG_OTHER":
                            row.col_8 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_9 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "BILLING_TYPE_TIMEOUT":
                            row.col_10 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_11 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Total_bill = Total_bill + long.Parse(item.ElementAt(i).totalBill);
                            Total_amout = Total_amout + long.Parse(item.ElementAt(i).totalAmount);
                            break;
                        case "BILLING_TYPE_ERROR":
                            row.col_12 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_13 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "BILLING_TYPE_REVERT":
                            row.col_14 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_15 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "TKTD":
                            row.col_21 = long.Parse(item.ElementAt(i).totalBill).ToString("N0");
                            row.col_22 = long.Parse(item.ElementAt(i).totalAmount).ToString("N0");
                            break;
                        default:
                            break;
                    }
                }
                row.col_16 = (Return_bill).ToString("N0");
                row.col_17 = (Return_amount).ToString("N0");
                row.col_18 = (Total_bill).ToString("N0");
                row.col_19 = (Total_amout).ToString("N0");
                try
                {
                    row.col_20 = item.Key.CompareTo(posAccount.edong) == 0 ? posAccount.name : (from x in dict_acc where x.Key == item.Key select x).FirstOrDefault().Value;
                }
                catch (Exception ex)
                {
                    Logging.ImportLogger.ErrorFormat(" ReportGeneral => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                    row.col_20 = "";
                }              
                items.Add(row);
            }
            return items;
        }

        public static List<FileExcel_HN> ReadExcelNPOI_HN(string fileName, int rowIndex, int sheetIndex, ePosAccount posAccount)
        {
            List<FileExcel_HN> data = new List<FileExcel_HN>();
            try
            {          
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                HSSFWorkbook wb = new HSSFWorkbook(fs);
                ISheet sheet = wb.GetSheetAt(sheetIndex);           
                while (!string.IsNullOrEmpty(sheet.GetRow(rowIndex).GetCell(0).StringCellValue))
                {            
                    FileExcel_HN row = new FileExcel_HN();               
                    var nowRow = sheet.GetRow(rowIndex);
                    row.BookCMIS = nowRow.GetCell(0).StringCellValue;             
                    row.CustomerCode = nowRow.GetCell(1).StringCellValue;              
                    row.Name = nowRow.GetCell(2).StringCellValue;              
                    row.Address = nowRow.GetCell(3).StringCellValue;              
                    row.Amount = decimal.Parse(nowRow.GetCell(5).NumericCellValue.ToString());
                    row.strAmount = row.Amount.ToString("N0"); 
                    row.Term = nowRow.GetCell(4).StringCellValue.Substring(3, 7);                   
                    row.Time_Online = nowRow.GetCell(6).StringCellValue;
                    row.BillID = row.Amount.ToString() + nowRow.GetCell(4).StringCellValue.Substring(3, 2) + nowRow.GetCell(4).StringCellValue.Substring(8);
                    row.pcCode = row.CustomerCode.Substring(0, 4) + "00";
                    row.status = "1";
                    data.Add(row);
                    rowIndex++;
                }           
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("ReadExcelNPOI_HN => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return data;
        }
        
        public static List<FileExcel_HN> ReadExcel_SPC(string fileName, ePosAccount posAccount)
        {
            List<FileExcel_HN> data = new List<FileExcel_HN>();
            try
            {
                DataSet dsTemp = new DataSet();
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    dsTemp = reader.AsDataSet();
                }
                for (int i = 0; i < dsTemp.Tables[0].AsEnumerable().Skip(1).Count(); i++)
                {
                    if (!string.IsNullOrEmpty(dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[0].ToString()))
                    {
                        FileExcel_HN row = new FileExcel_HN();
                        row.BookCMIS = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[0].ToString();
                        row.CustomerCode = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[1].ToString();
                        row.Name = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[2].ToString();
                        row.Address = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[3].ToString();
                        row.Amount = decimal.Parse(Validate.ProcessReplace(dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[7].ToString(), false));
                        row.strAmount = row.Amount.ToString("N0");
                        string _temp = int.Parse(dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[5].ToString()) < 10 ? 
                            "0" + dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[5].ToString() : dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[5].ToString();                       
                        row.Term = _temp + "/" + dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[6].ToString();
                        row.Time_Online = DateTime.Now.ToString("dd/MM/yyyy");
                        row.BillID = row.Amount.ToString() + _temp + dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[6].ToString();                    
                        row.pcCode = row.CustomerCode.Substring(0, 2).CompareTo("PB") == 0 ? row.CustomerCode.Substring(0, 6) : row.CustomerCode.Substring(0, 4) + "00";
                        row.status = dsTemp.Tables[0].AsEnumerable().Skip(1).ElementAt(i)[8].ToString().CompareTo("Đã chấm") == 0 ? "1" : "0";
                        data.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("ReadExcel_SPC => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return data;
        }

        public static DataTable ReadExcelToDatatble(string fileName, string ReportType, int HeaderLine, int ColumnStart, ePosAccount posAccount)
        {
            System.Data.DataTable dataTable = new DataTable();
            Microsoft.Office.Interop.Excel.Application excel;           
            Microsoft.Office.Interop.Excel.Workbook excelworkBook ;           
            Microsoft.Office.Interop.Excel.Worksheet excelSheet;         
            Microsoft.Office.Interop.Excel.Range range;
            try
            {
                // Get Application object.
                excel = new Microsoft.Office.Interop.Excel.Application();
             
               // excel.Visible = true;
               // check tai day
               // excel.DisplayAlerts = true;
               
                // Creation a new Workbook
                excelworkBook = excel.Workbooks.Open(fileName);
              
                // Workk sheet
                excelSheet = (Microsoft.Office.Interop.Excel.Worksheet)
                                      excelworkBook.Worksheets.Item[1];             
                range = excelSheet.UsedRange;
              
                int cl = range.Columns.Count;
                // loop through each row and add values to our sheet              
                int rowcount = range.Rows.Count;             
                //create the header of table
                for (int j = ColumnStart; j <= cl; j++)
                {                  
                    dataTable.Columns.Add(Convert.ToString
                                         (range.Cells[HeaderLine, j].Value2), typeof(string));
                }
                //filling the table from  excel file 
                for (int i = HeaderLine + 1; i <= rowcount; i++)
                {
                    DataRow dr = dataTable.NewRow();
                    for (int j = ColumnStart; j <= cl; j++)
                    {
                        dr[j - ColumnStart] = Convert.ToString(range.Cells[i, j].Value2);
                    }
                    dataTable.Rows.InsertAt(dr, dataTable.Rows.Count + 1);
                }
                //now close the workbook and make the function return the data table            
                excelworkBook.Close();           
                excel.Quit();
                return dataTable;
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("UploadFile_HN => User: {0}, Session: {1} ", posAccount.edong, posAccount.session);
                return null;
            }
            finally
            {
                excelSheet = null;
                range = null;
                excelworkBook = null;
            }
        }

        public static List<ObjReport> ReportGeneralOrther(List<ObjReport> items, responseReportCollectGeneral[] data, Dictionary<string, string> dict_acc, ePosAccount posAccount)
        {
            foreach (var item in data.GroupBy(x => x.edong).Select(x => x).ToList())
            {
                ObjReport row = new ObjReport();
                row.col_1 = item.Key;
                row.col_2 = "0";
                row.col_3 = "0";
                row.col_4 = "0";
                row.col_5 = "0";
                row.col_6 = "0";
                row.col_7 = "0";
                row.col_8 = "0";
                row.col_9 = "0";
                row.col_10 = "0";
                row.col_11 = "0";
                row.col_12 = "0";
                row.col_13 = "0";
                row.col_14 = "0";
                row.col_15 = "0";
                row.col_21 = "0";
                row.col_22 = "0";                
                double Total_bill = 0;
                double Total_amout = 0;
                double Return_amount = 0;
                double Return_bill = 0;

                for (int i = 0; i < item.Count(); i++)
                {
                    switch (item.ElementAt(i).responseCode)
                    {
                        case "BILLING_TYPE_BILLING":
                            row.col_2 = item.ElementAt(i).totalBill;
                            row.col_3 = item.ElementAt(i).totalAmount;
                            Total_bill = Total_bill + long.Parse(item.ElementAt(i).totalBill);
                            Total_amout = Total_amout + long.Parse(item.ElementAt(i).totalAmount);
                            break;
                        case "BILLING_TYPE_OFF":
                            row.col_4 = item.ElementAt(i).totalBill;
                            row.col_5 = item.ElementAt(i).totalAmount;
                            Total_bill = Total_bill + long.Parse(item.ElementAt(i).totalBill);
                            Total_amout = Total_amout + long.Parse(item.ElementAt(i).totalAmount);
                            break;
                        case "BILLING_TYPE_SOURCE_OTHER":
                            row.col_6 = item.ElementAt(i).totalBill;
                            row.col_7 = item.ElementAt(i).totalAmount;
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "BILLING_TYPE_EDONG_OTHER":
                            row.col_8 = item.ElementAt(i).totalBill;
                            row.col_9 = item.ElementAt(i).totalAmount;
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "BILLING_TYPE_TIMEOUT":
                            row.col_10 = item.ElementAt(i).totalBill;
                            row.col_11 = item.ElementAt(i).totalAmount;
                            Total_bill = Total_bill + long.Parse(item.ElementAt(i).totalBill);
                            Total_amout = Total_amout + long.Parse(item.ElementAt(i).totalAmount);
                            break;
                        case "BILLING_TYPE_ERROR":
                            row.col_12 = item.ElementAt(i).totalBill;
                            row.col_13 = item.ElementAt(i).totalAmount;
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "BILLING_TYPE_REVERT":
                            row.col_14 = item.ElementAt(i).totalBill;
                            row.col_15 = item.ElementAt(i).totalAmount;
                            Return_amount = Return_amount + long.Parse(item.ElementAt(i).totalAmount);
                            Return_bill = Return_bill + long.Parse(item.ElementAt(i).totalBill);
                            break;
                        case "TKTD":
                            row.col_21 = item.ElementAt(i).totalBill;
                            row.col_22 = item.ElementAt(i).totalAmount;
                            break;
                        default:
                            break;
                    }
                }
                row.col_16 = Return_bill.ToString();
                row.col_17 = Return_amount.ToString();
                row.col_18 = Total_bill.ToString();
                row.col_19 = Total_amout.ToString();
                try
                {
                    row.col_20 = item.Key.CompareTo(posAccount.edong) == 0 ? posAccount.name : (from x in dict_acc where x.Key == item.Key select x).FirstOrDefault().Value;
                }
                catch (Exception ex)
                {
                    Logging.ImportLogger.ErrorFormat(" ReportGeneralOrther => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                    row.col_20 = "";
                }
                items.Add(row);
            }
            return items;
        }
        
        public static List<ObjReport> ReportGroupGeneral(List<ObjReport> items, List<ObjReport> _temp, string[] array_edong, ePosAccount posAccount, int index)
        {
            try
            {
                foreach (var item in array_edong)
                {
                    ObjReport data = new ObjReport();
                    foreach (var obj in (from x in _temp where x.col_1 == item select x))
                    {
                        data.col_1 = obj.col_1;
                        data.col_2 = string.IsNullOrEmpty(data.col_2) == true ? obj.col_2 : (long.Parse(data.col_2) + long.Parse(obj.col_2)).ToString();
                        data.col_3 = string.IsNullOrEmpty(data.col_3) == true ? obj.col_3 : (long.Parse(data.col_3) + long.Parse(obj.col_3)).ToString();
                        data.col_4 = string.IsNullOrEmpty(data.col_4) == true ? obj.col_4 : (long.Parse(data.col_4) + long.Parse(obj.col_4)).ToString();
                        data.col_5 = string.IsNullOrEmpty(data.col_5) == true ? obj.col_5 : (long.Parse(data.col_5) + long.Parse(obj.col_5)).ToString();
                        data.col_6 = string.IsNullOrEmpty(data.col_6) == true ? obj.col_6 : (long.Parse(data.col_6) + long.Parse(obj.col_6)).ToString();
                        data.col_7 = string.IsNullOrEmpty(data.col_7) == true ? obj.col_7 : (long.Parse(data.col_7) + long.Parse(obj.col_7)).ToString();
                        data.col_8 = string.IsNullOrEmpty(data.col_8) == true ? obj.col_8 : (long.Parse(data.col_8) + long.Parse(obj.col_8)).ToString();
                        data.col_9 = string.IsNullOrEmpty(data.col_9) == true ? obj.col_9 : (long.Parse(data.col_9) + long.Parse(obj.col_9)).ToString();
                        data.col_10 = string.IsNullOrEmpty(data.col_10) == true ? obj.col_10 : (long.Parse(data.col_10) + long.Parse(obj.col_10)).ToString();
                        data.col_11 = string.IsNullOrEmpty(data.col_11) == true ? obj.col_11 : (long.Parse(data.col_11) + long.Parse(obj.col_11)).ToString();
                        data.col_12 = string.IsNullOrEmpty(data.col_12) == true ? obj.col_12 : (long.Parse(data.col_12) + long.Parse(obj.col_12)).ToString();
                        data.col_13 = string.IsNullOrEmpty(data.col_13) == true ? obj.col_13 : (long.Parse(data.col_13) + long.Parse(obj.col_13)).ToString();
                        data.col_14 = string.IsNullOrEmpty(data.col_14) == true ? obj.col_14 : (long.Parse(data.col_14) + long.Parse(obj.col_14)).ToString();
                        data.col_15 = string.IsNullOrEmpty(data.col_15) == true ? obj.col_15 : (long.Parse(data.col_15) + long.Parse(obj.col_15)).ToString();
                        data.col_16 = string.IsNullOrEmpty(data.col_16) == true ? obj.col_16 : (long.Parse(data.col_16) + long.Parse(obj.col_16)).ToString();
                        data.col_17 = string.IsNullOrEmpty(data.col_17) == true ? obj.col_17 : (long.Parse(data.col_17) + long.Parse(obj.col_17)).ToString();
                        data.col_18 = string.IsNullOrEmpty(data.col_18) == true ? obj.col_18 : (long.Parse(data.col_18) + long.Parse(obj.col_18)).ToString();
                        data.col_19 = string.IsNullOrEmpty(data.col_19) == true ? obj.col_19 : (long.Parse(data.col_19) + long.Parse(obj.col_19)).ToString();
                        data.col_20 = obj.col_20;
                        data.col_21 = obj.col_21;
                        data.col_22 = obj.col_22;
                    }
                    if (data != null)
                        items.Add(new ObjReport {
                            col_1 = data.col_1,
                            col_2 = long.Parse(data.col_2).ToString("N0"),
                            col_3 = long.Parse(data.col_3).ToString("N0"),
                            col_4 = long.Parse(data.col_4).ToString("N0"),
                            col_5 = long.Parse(data.col_5).ToString("N0"),
                            col_6 = long.Parse(data.col_6).ToString("N0"),
                            col_7 = long.Parse(data.col_7).ToString("N0"),
                            col_8 = long.Parse(data.col_8).ToString("N0"),
                            col_9 = long.Parse(data.col_9).ToString("N0"),
                            col_10 = long.Parse(data.col_10).ToString("N0"),
                            col_11 = long.Parse(data.col_11).ToString("N0"),
                            col_12 = long.Parse(data.col_12).ToString("N0"),
                            col_13 = long.Parse(data.col_13).ToString("N0"),
                            col_14 = long.Parse(data.col_14).ToString("N0"),
                            col_15 = long.Parse(data.col_15).ToString("N0"),
                            col_16 = long.Parse(data.col_16).ToString("N0"),
                            col_17 = long.Parse(data.col_17).ToString("N0"),
                            col_18 = long.Parse(data.col_18).ToString("N0"),
                            col_19 = long.Parse(data.col_19).ToString("N0"),
                            col_20 = data.col_20,
                            col_21 = data.col_21,
                            col_22 = data.col_22,
                            col_23 = index + ""                            
                        });
                }
            }
            catch (Exception ex)
            {
                Logging.ImportLogger.ErrorFormat("ReportGroupGeneral => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
            }
            return items;
        }

        public static List<ObjReport> ReportDetailEVN_(List<ObjReport> items, bill[] data, string Upload_FromDate, string Upload_ToDate, ref decimal total_bill, ref decimal total_billTKTĐ, ref decimal total_amount, ref decimal total_amountTKTD)
        {
            List<ObjReport> temp_TKTD = new List<ObjReport>();
            List<ObjReport> temp_Bill = new List<ObjReport>();
            foreach (var item in (from x in data where x.billingType == Constant.TKTD select x))
            {
                temp_TKTD.Add(new ObjReport
                {
                    col_1 = item.customerCode,
                    col_2 = item.name,
                    col_3 = item.address,
                    col_4 = item.bookCmis,
                    col_5 = string.Empty,
                    col_6 = item.strTerm,
                    col_7 = (from x in Constant.PaymentResult() where x.Key == item.billingType select x).FirstOrDefault().Value,
                    col_8 = item.billId.ToString(),
                    col_9 = Upload_FromDate,
                    col_10 = Upload_ToDate,
                    col_12 = item.strFromDate,
                    col_13 = item.strToDate,
                    col_14 = item.amount.ToString("N0"),
                    col_15 = item.taxCode
                });
                total_billTKTĐ = total_billTKTĐ + 1;
                total_amountTKTD = total_amountTKTD + item.amount;
            }

            foreach (var item in (from x in data where x.billingType != Constant.TKTD select x))
            {
                temp_Bill.Add(new ObjReport
                {
                    col_1 = item.customerCode,
                    col_2 = item.name,
                    col_3 = item.address,
                    col_4 = item.bookCmis,
                    col_5 = item.amount.ToString("N0"),
                    col_6 = item.strTerm,
                    col_7 = (from x in Constant.PaymentResult() where x.Key == item.billingType select x).FirstOrDefault().Value,
                    col_8 = item.billId.ToString(),
                    col_9 = Upload_FromDate,
                    col_10 = Upload_ToDate,
                    col_12 = item.strFromDate,
                    col_13 = item.strToDate,
                    col_14 = string.Empty,
                    col_15 = item.taxCode
                });
                total_bill = total_bill + 1;
                total_amount = total_amount + item.amount;
            }

            var rightjoin = temp_Bill.GroupJoin(temp_TKTD, bill => bill.col_8, tktd => tktd.col_8, (bill, tktd) => new { bill, tktd })
                           .SelectMany(@t => @t.tktd.DefaultIfEmpty(), (@t, c) => new { @t, c })
                           .OrderBy(@t => @t.@t.bill.col_1)
                           .Select(@t => new ObjReport
                           {
                               col_1 = @t.@t.bill.col_1,
                               col_2 = @t.@t.bill.col_2,
                               col_3 = @t.@t.bill.col_3,
                               col_4 = @t.@t.bill.col_4,
                               col_5 = @t.@t.bill.col_5,
                               col_6 = @t.@t.bill.col_6,
                               col_7 = @t.@t.bill.col_7,
                               col_8 = @t.@t.bill.col_8,
                               col_9 = @t.@t.bill.col_9,
                               col_10 = @t.@t.bill.col_10,
                               col_11 = @t.@t.bill.col_11,
                               col_12 = @t.@t.bill.col_12,
                               col_13 = @t.@t.bill.col_13,
                               col_14 = @t.@t.bill.col_14,
                               col_15 = @t.@t.bill.col_15                              
                           });

            var leftjoin = 
                (from l in temp_TKTD
                 where !(from a in rightjoin select a.col_8).Contains(l.col_8)
                 select new ObjReport
                 {
                     col_1 = l.col_1,
                     col_2 = l.col_2,
                     col_3 = l.col_3,
                     col_4 = l.col_4,
                     col_5 = l.col_5,
                     col_6 = l.col_6,
                     col_7 = l.col_7,
                     col_8 = l.col_8,
                     col_9 = l.col_9,
                     col_10 = l.col_10,
                     col_11 = l.col_11,
                     col_12 = l.col_12,
                     col_13 = l.col_13,
                     col_14 = l.col_14,
                     col_15 = l.col_15
                 });
            var fulljion = leftjoin.Union(rightjoin).ToList();

            return null;
            //var rightjoin =
            //                temp.GroupJoin(listAutoBilling.autoBillingList, sup => sup.S_CUSTOMERCODE,
            //                    cust => cust.customerCode, (sup, cs) => new { sup, cs })
            //                    .SelectMany(@t => @t.cs.DefaultIfEmpty(), (@t, c) => new { @t, c })
            //                    .OrderBy(@t => @t.@t.sup.I_STT)
            //                    .Select(@t => new ObjTBL_ENV_CODE
            //                    {
            //                        S_CUSTOMERCODE = @t.@t.sup.S_CUSTOMERCODE,
            //                        S_CUSTOMERNAME = @t.@t.sup.S_CUSTOMERNAME,
            //                        I_FLAG = @t.c == null ? 0 : 1,
            //                        TBL_EVN_CODEID = @t.@t.sup.TBL_EVN_CODEID,
            //                        S_CUSTOMERADDRESS = @t.@t.sup.S_CUSTOMERADDRESS,
            //                        S_ID = @t.@t.sup.S_ID,
            //                        SMS_1 = @t.c == null ? false : @t.c.sms1,
            //                        SMS_2 = @t.c == null ? false : @t.c.sms2,
            //                        SMS_3 = @t.c == null ? false : @t.c.sms3
            //                    });

            //var leftjoin =
            //    (from l in listAutoBilling.autoBillingList
            //     where !(from a in rightjoin select a.S_CUSTOMERCODE).Contains(l.customerCode)
            //     select new ObjTBL_ENV_CODE
            //     {
            //         S_CUSTOMERCODE = l.customerCode,
            //         S_CUSTOMERNAME = l.customerName,
            //         I_FLAG = 1,
            //         SMS_1 = l.sms1,
            //         SMS_2 = l.sms2,
            //         SMS_3 = l.sms3,
            //         S_ID = CryptorEngine.Encrypt("1," + l.customerCode + "," + l.customerName, false, PrivateKey, PublicKey)
            //     });
            //var fulljoin = leftjoin.Union(rightjoin).ToList();




            //var temp = temp_Bill.Join(temp_TKTD, bill => bill.col_1, tktd => tktd.col_8, (bill, tktd) => new ObjReport
            //{
            //    col_1 = bill.col_1,
            //    col_2 = bill.col_2,
            //    col_3 = bill.col_3,
            //    col_4 = bill.col_4,
            //    col_5 = bill.col_5,
            //    col_6 = bill.col_6,
            //    col_7 = bill.col_7,
            //    col_8 = bill.col_8,
            //    col_9 = bill.col_9,
            //    col_10 = bill.col_10,             
            //    col_12 = bill.col_12,
            //    col_13 = bill.col_13,
            //    col_14 = tktd.col_2,
            //    col_15 = bill.col_15
            //});
            //return temp.OrderBy(c => c.col_1).ThenByDescending(x => x.col_12).ToList();
        }


        public static List<ObjReport> ReportDetailEVN(List<ObjReport> items, bill[] data, string Upload_FromDate, string Upload_ToDate, ref decimal total_bill, ref decimal total_billTKTĐ, ref decimal total_amount, ref decimal total_amountTKTD)
        {
            List<ObjReport> temp_TKTD = new List<ObjReport>();
            List<ObjReport> temp_Bill = new List<ObjReport>();
            foreach (var item in (from x in data where x.billingType == Constant.TKTD select x))
            {
                temp_TKTD.Add(new ObjReport
                {
                    col_1 = item.customerCode,
                    col_2 = item.name,
                    col_3 = item.address,
                    col_4 = item.bookCmis,
                    col_5 = string.Empty,
                    col_6 = item.strTerm,
                    col_7 = (from x in Constant.PaymentResult() where x.Key == item.billingType select x).FirstOrDefault().Value,
                    col_8 = item.billId.ToString(),
                    col_9 = Upload_FromDate,
                    col_10 = Upload_ToDate,
                    col_12 = item.strFromDate,
                    col_13 = item.strToDate,
                    col_14 = item.amount.ToString("N0"),
                    col_15 = item.taxCode
                });
                total_billTKTĐ = total_billTKTĐ + 1;
                total_amountTKTD = total_amountTKTD + item.amount;
            }

            foreach (var item in (from x in data where x.billingType != Constant.TKTD select x))
            {
                temp_Bill.Add(new ObjReport
                {
                    col_1 = item.customerCode,
                    col_2 = item.name,
                    col_3 = item.address,
                    col_4 = item.bookCmis,
                    col_5 = item.amount.ToString("N0"),
                    col_6 = item.strTerm,
                    col_7 = (from x in Constant.PaymentResult() where x.Key == item.billingType select x).FirstOrDefault().Value,
                    col_8 = item.billId.ToString(),
                    col_9 = Upload_FromDate,
                    col_10 = Upload_ToDate,
                    col_12 = item.strFromDate,
                    col_13 = item.strToDate,
                    col_14 = string.Empty,
                    col_15 = item.taxCode
                });
                total_bill = total_bill + 1;
                total_amount = total_amount + item.amount;
            }
            
            for (int i = 0; i < data.Count(); i++)
            {
                ObjReport row = new ObjReport();
                row.col_1 = data.ElementAt(i).customerCode;
                row.col_2 = data.ElementAt(i).name;
                row.col_3 = data.ElementAt(i).address;
                row.col_4 = data.ElementAt(i).bookCmis;
                row.col_6 = data.ElementAt(i).strTerm;
                try
                {
                    row.col_5 = data.ElementAt(i).billingType.CompareTo("TKTD") == 0 ? "" : data.ElementAt(i).amount.ToString("N0");
                }
                catch
                {
                    row.col_5 = "";                 
                }
                try
                {
                    row.col_7 = (from x in Constant.PaymentResult() where x.Key == data.ElementAt(i).billingType select x).FirstOrDefault().Value;                   
                }
                catch 
                {
                    row.col_7 = "";                    
                }
                try
                {
                    row.col_14 = data.ElementAt(i).billingType.CompareTo("TKTD") == 0 ? data.ElementAt(i).amount.ToString("N0") : "";
                }
                catch 
                {
                    row.col_14 = "";
                }
                row.col_8 = data.ElementAt(i).billId.ToString();
                row.col_9 = Upload_FromDate;
                row.col_10 = Upload_ToDate;
                row.col_11 = i.ToString();
                row.col_12 = data.ElementAt(i).strFromDate;
                row.col_13 = data.ElementAt(i).strToDate;
                row.col_15 = data.ElementAt(i).taxCode;               
                items.Add(row);
            }
            return items.OrderBy(c => c.col_1).ThenByDescending(x => x.col_12).ToList();
        }

        public static List<ObjReport> ReportDetailWater(string pcCode, List<ObjReport> items, transaction[] data, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");
                row.col_6 = "Tiền nước";
                row.col_7 = (from x in Constant.Provider_Water() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value; ;
                row.col_8 = data.ElementAt(i).customerCode;               
                row.col_10 = data.ElementAt(i).customerName;
                row.col_11 = data.ElementAt(i).customerAddress;
                row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true? "0": data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }                
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }
                if (string.IsNullOrEmpty(data.ElementAt(i).resever2))
                {
                    total_bill = total_bill + 0;
                    row.col_5 = "";
                }
                else
                {
                    total_bill = data.ElementAt(i).bridgeCode.CompareTo("NCC0505") == 0 ? total_bill + 1 : total_bill + decimal.Parse(data.ElementAt(i).resever2);
                    row.col_5 = data.ElementAt(i).bridgeCode.CompareTo("NCC0505") == 0?"1": data.ElementAt(i).resever2;
                }                    
                total = total + (string.IsNullOrEmpty(data.ElementAt(i).amount) == true? 0: decimal.Parse(data.ElementAt(i).amount));
                items.Add(row);
            }
            return items.ToList();
        }

        public static List<ObjReport> ReportDetailVTV(string pcCode, List<ObjReport> items, transaction[] data, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");
                row.col_6 = "Truyền hình";
                row.col_7 = (from x in Constant.Provider_VTV() where x.Key == data.ElementAt(i).bridgeCode select x ).FirstOrDefault().Value;
                row.col_8 = data.ElementAt(i).customerCode;
                row.col_10 = data.ElementAt(i).customerName;
                row.col_11 = data.ElementAt(i).customerAddress;
                row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? "0" : data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }
                row.col_5 = "1";
                total_bill = total_bill + 1;

                //if (string.IsNullOrEmpty(data.ElementAt(i).resever2))
                //{
                //    total_bill = total_bill + 0;
                //    row.col_5 = "";
                //}
                //else
                //{
                //    total_bill = total_bill + decimal.Parse(data.ElementAt(i).resever2);
                //    row.col_5 = data.ElementAt(i).resever2;
                //}
                total = total + (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount));
                items.Add(row);
            }
            return items.ToList();
        }

        public static List<ObjReport> ReportTopup(string pcCode, List<ObjReport> items, transaction[] data, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");

                if (data.ElementAt(i).type.CompareTo(Constant.TOPUP_PREPAID) == 0)
                    try
                    {
                        row.col_6 = "Di động trả trước";
                        row.col_7 = (from x in Constant.TopupPrePaid() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value;
                    }
                    catch (Exception)
                    {
                        row.col_6 = "";
                        row.col_7 = "";
                    }
                else 
                    try
                    {
                        row.col_6 = "Di động trả sau";
                        row.col_7 = (from x in Constant.TopupPostPaid() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value;
                    }
                    catch (Exception)
                    {
                        row.col_6 = "";
                        row.col_7 = "";
                    }
               
               // row.col_7 =  data.ElementAt(i).bridgeCode;
                row.col_8 = data.ElementAt(i).resever1;
                row.col_10 = "";
                row.col_11 = "";
                row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? "0" : data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }
                row.col_5 = "1";
                total_bill = total_bill + 1;
                total = total +
                    (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount)) -
                    (string.IsNullOrEmpty(data.ElementAt(i).resever11) == true? 0: decimal.Parse(data.ElementAt(i).resever11));
                    

                items.Add(row);
            }
            return items.ToList();
        }

        public static List<ObjReport> ReportBuyCard(string pcCode, List<ObjReport> items, transaction[] data, string service, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");
                row.col_6 = service;
                row.col_7 = (from x in Constant.TopupPrePaid() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value;
                row.col_8 = "";
                row.col_10 = "";
                row.col_11 = "";
                row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? "0" : data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }

                row.col_5 = "1";
                total_bill = total_bill +1;
                //total = total + (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount));
                total = total +
                   (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount)) -
                   (string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? 0 : decimal.Parse(data.ElementAt(i).resever11));
                items.Add(row);
            }
            return items.ToList();
        }
        
        public static List<ObjReport> ReportCardGame(string pcCode, List<ObjReport> items, transaction[] data, string service, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");
                row.col_6 = service;
                row.col_7 = (from x in Constant.CardGame() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value;
                row.col_8 = "";
                row.col_10 = "";
                row.col_11 = "";
                row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? "0" : data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }

                row.col_5 = "1";
                total_bill = total_bill + 1;
                //total = total + (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount));
                total = total +
                   (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount)) -
                   (string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? 0 : decimal.Parse(data.ElementAt(i).resever11));
                items.Add(row);
            }
            return items.ToList();
        }

        public static List<ObjReport> ReportCardData(string pcCode, List<ObjReport> items, transaction[] data, string service, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");
                row.col_6 = service;
                row.col_7 = (from x in Constant.CardData() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value;
                row.col_8 = "";
                row.col_10 = "";
                row.col_11 = "";
                row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? "0" : data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }

                row.col_5 = "1";
                total_bill = total_bill + 1;
                //total = total + (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount));
                total = total +
                   (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount)) -
                   (string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? 0 : decimal.Parse(data.ElementAt(i).resever11));
                items.Add(row);
            }
            return items.ToList();
        }

        public static List<ObjReport> ReportFinance(string pcCode, List<ObjReport> items, transaction[] data, string service, ePosAccount posAccount, ref decimal total, ref decimal total_bill)
        {
            List<ObjEdong> edongs = posAccount.Childs;
            int index = items.Count > 0 == true ? items.Count : 0;
            for (int i = 0; i < data.Count(); i++)
            {
                index++;
                ObjReport row = new ObjReport();
                row.col_1 = index.ToString();
                row.col_2 = data.ElementAt(i).responseTime;
                row.col_3 = data.ElementAt(i).edong;
                row.col_4 = string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? "0" : decimal.Parse(data.ElementAt(i).amount).ToString("N0");
                row.col_6 = service;
                row.col_7 = (from x in Constant.Provider_Finance() where x.Key == data.ElementAt(i).bridgeCode select x).FirstOrDefault().Value;
                row.col_8 = "";
                row.col_10 = "";
                row.col_11 = "";
                row.col_13 = "";//chiết khẩu bỏ
                //row.col_13 = string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? "0" : data.ElementAt(i).resever11;
                try
                {
                    row.col_12 = (from x in posAccount.dict_child where x.Key == data.ElementAt(i).edong select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_12 = "";
                }
                switch (data.ElementAt(i).billingType)
                {
                    case "BILLING_TYPE_BILLING":
                        row.col_9 = "Thành công";
                        break;
                    case "BILLING_TYPE_OFF":
                        row.col_9 = "Đang giữ tại ECPAY";
                        break;
                    case "BILLING_TYPE_SOURCE_OTHER":
                        row.col_9 = "Thanh toán bởi ví khác";
                        break;
                    case "BILLING_TYPE_EDONG_OTHER":
                        row.col_9 = "Thanh toán bởi nguồn khác";
                        break;
                    case "BILLING_TYPE_TIMEOUT":
                        row.col_9 = "Giao dịch nghi ngờ";
                        break;
                    case "BILLING_TYPE_ERROR":
                        row.col_9 = "Thanh toán lỗi";
                        break;
                    case "BILLING_TYPE_REVERT":
                        row.col_9 = "Hủy hóa đơn thành công";
                        break;
                    default:
                        break;
                }

                row.col_5 = "1";
                total_bill = total_bill + 1;
                total = total + (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount));
                //try
                //{
                //    total = total +
                //   (string.IsNullOrEmpty(data.ElementAt(i).amount) == true ? 0 : decimal.Parse(data.ElementAt(i).amount)) -
                //   (string.IsNullOrEmpty(data.ElementAt(i).resever11) == true ? 0 : decimal.Parse(data.ElementAt(i).resever11));
                //}catch
                //{
                //    total = 0;
                //}
                
                items.Add(row);
            }
            return items.ToList();
        }

        public static List<ObjReport> FillDataBillHanding(List<ObjReport> items, string outputZipField, ref decimal total_amount, ref int total_bill)
        {
           
            List<PendingReportItem> data = JsonConvert.DeserializeObject<List<PendingReportItem>>(CompressUtil.DecryptBase64(outputZipField), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            foreach (var item in data)
            {
                total_bill = total_bill + 1;
                total_amount = total_amount + item.bill.amount;
                ObjReport row = new ObjReport();
                row.col_1 = item.cashierPay;
                row.col_2 = item.strRequestDate;
                row.col_3 = item.bill.customerCode;
                row.col_4 = item.bill.strTerm; //kỳ
                row.col_5 = item.bill.amount.ToString("N0");
                try
                {
                    row.col_6 = (from x in Constant.BillHandlingType() where x.Key == item.offType.ToString() select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_6 = "";
                }
                row.col_7 = item.strWorkDate;
                row.col_8 = item.id.ToString();
                try
                {
                    row.col_9 = string.IsNullOrEmpty(item.bill.pcCodeExt.ToString()) == true ? string.Empty : item.bill.pcCodeExt.ToString();
                }
                catch (Exception)
                {

                    row.col_9 ="";
                }
               
                row.col_10 = item.offType.ToString();
                try
                {
                    row.col_11 = (from x in Constant.BillHandlingStatus() where x.Key == item.status.ToString() select x).FirstOrDefault().Value;
                }
                catch (Exception)
                {
                    row.col_11 = "";
                }
                row.col_12 = item.status.ToString();
                row.col_13 = item.description;
                row.col_14 = item.bill.amount.ToString("N0");
                row.col_15 = item.bill.billId.ToString();                
                row.col_16 = item.responseCode;
                row.col_17 = item.bill.name;
                row.col_18 = item.bill.address;
                row.col_19 = item.bill.inning;
                row.col_20 = item.providerCode;
                row.col_21 = item.bill.inning;
                items.Add(row);
            }
            return items;
        }        

        public static List<ObjReport> FillDebtForBank(List<ObjReport> items, Dictionary<string, string> dict_pc, string outputZip, int status, ref decimal totalBill, ref decimal totalAmount)
        {
            var _data = status == -1 ? JsonConvert.DeserializeObject<List<DebtForBankObject>>(CompressUtil.DecryptBase64(outputZip), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) :
                (from x in JsonConvert.DeserializeObject<List<DebtForBankObject>>(CompressUtil.DecryptBase64(outputZip), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) where x.status == status select x);
            if (_data.Count() > 0)
            {
                foreach (var item in _data)
                {
                    ObjReport row = new ObjReport();                 
                    row.col_2 = dict_pc.ContainsKey(item.pcCodeExt) == true ?
                        (from x in dict_pc where x.Key == item.pcCodeExt select x).FirstOrDefault().Value : "";
                    row.col_3 = item.code;
                    row.col_4 = item.name;
                    row.col_5 = item.amount.ToString("N0");
                    totalAmount = totalAmount + item.amount;
                    row.col_6 = item.status == 1 ? "Đã thanh toán" : "Chưa thanh toán";
                    items.Add(row);
                }
                totalBill = totalBill + _data.Count();
            }
            return items;
        }

    }

   
}