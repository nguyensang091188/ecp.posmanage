using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ePOS3.Utils
{
    public class ePOSSession
    {
        public static String UPLOAD_XMLBILL = "FileXmlBill";
        public static String UPLOAD_XMLBILLPAYMENT = "FileXmlBillPayment";
        public static String UPLOAD_PCHANOI = "FilePCHANOI";
        public static String UPLOAD_EXCEL = "FileExcel";
        public static String UPLOAD_CONTROLDEBT = "FileControlDebt";
        public static String UPLOAD_CONTROLDEBTNPC = "FileControlDebtNPC";
        public static String UPLOAD_WITHDRAW = "FileWithdraw";
        public static String UPLOAD_BOOKCMIS = "FileBookCMIS";
        public static String UPLOAD_CONTROLDEBTGCS = "FileControlDebtGCS";
        public static String CASH_AND_EDONG_SUM = "CashAndEDongSum";
        public static String CASH_AND_EDONG_DETAIL = "CashAndEDongDetail";
        public static String GENERALONOFF = "GeneralOnOff";
        public static String GENERALONOFF_DETAIL = "GeneralOnOffDetail";
        public static String DELIVERY_REPORT = "DeliveryReport";
        public static String DELIVERY_COMMON = "DeliveryCommon";
        public static String DELIVERY_REPORT_DETAIL = "DeliveryReportDetail";
        public static String DELIVERY_COMMON_DETAIL = "DeliveryCommonDetail";
        public static String DEBTRELIEF_REPORT_SUM = "SumDebtReliefReport";
        public static String DEBT_RELIEF_REPORT_DETAIL = "DetailDebtReliefReport";
        public static String LIST_BOOKCMIS = "ListBookCMIS";
        public static String ACCOUNT_CANCELREQUEST = "CANCELREQUEST";
        public static String ACCOUNT_BILLERROR = "BILL_ERROR";
        public static String LIST_EDONG = "LIST_EDONG";
        public static String CARDIDENTIFIER = "CardIdentifier";

        public static String SESSION_NOT_EXISTED = "201";
        public static String SESSION_TIMEOUT = "202";
        public static String SESSION_INVALID = "203";

        /// <param name="strSessionName">Session</param>
        /// <param name="strValue">Session</param>
        public static void AddObject(string strSessionName, object objValue)
        {
            HttpContext.Current.Session[strSessionName] = objValue;
            HttpContext.Current.Session.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["Time"]);
        }


        public static void AddObject(string strSessionName, object objValue, int iExpires)
        {
            HttpContext.Current.Session[strSessionName] = objValue;
            HttpContext.Current.Session.Timeout = iExpires;
        }

        /// Session
        /// <summary>
        /// Session
        /// </summary>
        /// <param name="strSessionName">Session</param>
        /// <returns>Session</returns>
        public static object GetObject(string strSessionName)
        {
            if (HttpContext.Current == null && HttpContext.Current.Session[strSessionName] == null)
            {
                return null;
            }
            else
            {
                return HttpContext.Current.Session[strSessionName];
            }
        }

        /// Session
        /// <summary>
        /// Session
        /// </summary>
        /// <param name="strSessionName">Session</param>
        /// <returns>Session</returns>
        //public static string GetObject(string strSessionName)
        //{
        //    if (HttpContext.Current.Session[strSessionName] == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return HttpContext.Current.Session[strSessionName].ToString();
        //    }
        //}


        /// Session
        /// <summary>
        /// Session
        /// </summary>
        /// <param name="strSessionName">Session</param>
        public static void Del(string strSessionName)
        {
            HttpContext.Current.Session[strSessionName] = null;
        }
    }
}