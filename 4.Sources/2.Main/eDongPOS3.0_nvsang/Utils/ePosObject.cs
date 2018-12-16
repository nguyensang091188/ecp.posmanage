using ePOS3.eStoreWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ePOS3.Utils
{
    public class ePosAccount
    {
        public string edong { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public string session { get; set; }
        public string phone { get; set; }
        public decimal balance { get; set; }
        public decimal lockMoney { get; set; }
        public bool changedPIN { get; set; }
        public int verified { get; set; }
        public string status { get; set; }
        public long type { get; set; }
        public string IdNumber { get; set; }
        public string IdNumberPlace { get; set; }
        public string IdNumberDate { get; set; }
        public string parent { get; set; }
        public string parent_id { get; set; }

        public string IP_Mac { get; set; }
        public List<ObjEVNPC> EvnPC { get; set; }
        public List<ObjEdong> Childs { get; set; }
        public int i_Cancel { get; set; }
        public int i_Bill { get; set; }
        public Dictionary<string, string> dict_child { get; set; }
    }

    public class ObjEVNPC
    {
        public string address { get; set; }
        public string code { get; set; }
        public string dateChanged { get; set; }
        public string dateCreated { get; set; }
        public string ext { get; set; }
        public string fullName { get; set; }
        public string shortName { get; set; }
        public string mailTo { get; set; }
        public string mailCc { get; set; }
        public long level { get; set; }
        public long parentId { get; set; }
        public long pcId { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public long status { get; set; }
        public string parentName { get; set; }
        public string edong { get; set; }
        public string tax { get; set; }
        public string cardEVN { get; set; }
        public string providerCode { get; set; }
    }

    public class ObjEdong
    {
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string birthday { get; set; }
        public string email { get; set; }
        public string idNumber { get; set; }
        public string idNumberDate { get; set; }
        public string idNumberPlace { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string parent { get; set; }
        public string parent_id { get; set; }
        public string phone { get; set; }
        public string DebtDate { get; set; }
        public string debtAmount { get; set; }
    }

    public class TreeView
    {
        public long id { get; set; }
        public int parentid { get; set; }
        public string text { get; set; }
        public string value { get; set; }
    }

    public class ObjReport
    {
        public string col_0 { get; set; }
        public string col_1 { get; set; }
        public string col_2 { get; set; }
        public string col_3 { get; set; }
        public string col_4 { get; set; }
        public string col_5 { get; set; }
        public string col_6 { get; set; }
        public string col_7 { get; set; }
        public string col_8 { get; set; }
        public string col_9 { get; set; }
        public string col_10 { get; set; }
        public string col_11 { get; set; }
        public string col_12 { get; set; }
        public string col_13 { get; set; }
        public string col_14 { get; set; }
        public string col_15 { get; set; }
        public string col_16 { get; set; }
        public string col_17 { get; set; }
        public string col_18 { get; set; }
        public string col_19 { get; set; }
        public string col_20 { get; set; }
        public string col_21 { get; set; }
        public string col_22 { get; set; }
        public string col_23 { get; set; }
        public string col_24 { get; set; }

    }

    public class DataXML
    {
        public List<bill> bill { get; set; }
        public List<customer> customer { get; set; }
        public List<ObjReport> report { get; set; }
        public List<transactionOff> transOff { get; set; }
    }

    public class ObjLogViewer
    {
        public long No { get; set; }
        public DateTime Date { get; set; }
        public string DateLog { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }

    public class ObjCashDetail
    {
        public string debt_old { get; set; }
        public string Amount_old { get; set; }
        public string cash_old { get; set; }
        public string debt_new { get; set; }
        public string Amount_new { get; set; }
        public string cash_new { get; set; }
        public List<ObjReport> items { get; set; }
    }
    
    public class ObjCheckStock
    {
        public long requestId { get; set; }
        public string edong { get; set; }
        public string email { get; set; }
        public string fileInputPath { get; set; }
        public string fileOutputPath { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string createDate { get; set; }
    }

    public class ObjDeliverySummuryReport
    {
        public int S_ID_REPORT { get; set; }
        public DateTime D_DELIVERY_DATE { get; set; }
        public string S_DELIVERY_DATE { get; set; }
        public DateTime D_DELIVERYED_DATE { get; set; }
        public string S_MA_TNGAN { get; set; }
        public string S_TEN_TNGAN { get; set; }
        public string S_GCS_CODE { get; set; }
        public string S_CUSTOMER_ADDRESS { get; set; }
        public int N_CUSTOMER_SUM { get; set; }
        public string S_MONTH_YEAR { get; set; }
        public int N_PERIOD { get; set; }
        public int N_MONTH { get; set; }
        public int N_YEAR { get; set; }
        public int N_PAGE_REPORT { get; set; }
        public int N_HC_BILL_SUM { get; set; }
        public int N_VC_BILL_SUM { get; set; }
        public decimal N_HC_BILL_AMOUNT { get; set; }
        public decimal N_VC_BILL_AMOUNT { get; set; }
        public decimal N_HC_BILL_VAT { get; set; }
        public decimal N_VC_BILL_VAT { get; set; }
        public decimal N_AMOUNT_SUM { get; set; }
        public string S_MA_DVIQLY { get; set; }

        public int N_SOBBBG { get; set; }

        public string S_MA_TNV { get; set; }

    }

    public class ObjReportDeliverySummuryCommon
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PCCode { get; set; }
        public string Account { get; set; }
        public int BillCount { get; set; }
        public decimal BillAmount { get; set; }
        public decimal AmountHC { get; set; }
        public decimal AmountVC { get; set; }
        public decimal VATHC { get; set; }
        public decimal VATVC { get; set; }
    }

    public class ObjDeliveryDetailReport
    {
        public int N_ID { get; set; }
        public int S_ID_REPORT { get; set; }
        public DateTime D_DELIVERY_DATE { get; set; }
        public string S_DELIVERY_DATE { get; set; }
        public DateTime D_DELIVERYED_DATE { get; set; }
        public string S_EDONG_ACCOUNT { get; set; }
        public string S_EDONG_ACCOUNT_NAME { get; set; }
        public string S_CUSTOMER_CODE { get; set; }
        public string S_CUSTOMER_NAME { get; set; }

        public string S_SERI_ID { get; set; }
        public string S_GCS_CODE { get; set; }
        public string S_AREA { get; set; }
        public string S_PERIOD_YEAR { get; set; }
        public string S_TYPE { get; set; }
        public decimal N_AMOUNT_SUM { get; set; }
        public string S_SO_HDON { get; set; }
        public string S_MA_DVIQLY { get; set; }
    }
    public class ObjReportDeliveryDetailCommon
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PCCode { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public int BillCount { get; set; }
        public decimal BillAmount { get; set; }

    }
    public class ObjDebtReliefDetailReport
    {
        public int ID { get; set; }
        public string MA_KHANG { get; set; }
        public string TEN_KHANG { get; set; }
        public string DIA_CHI { get; set; }
        public string DANH_SO { get; set; }
        public string MA_SOGCS { get; set; }
        public string ID_HDON { get; set; }
        public string KYTHANGNAM { get; set; }
        public decimal SO_TIEN { get; set; }
        public decimal TIEN_GTGT { get; set; }
        public decimal SO_TIEN_CHAM { get; set; }
        public decimal TIEN_GTGT_CHAM { get; set; }
        public string TRANG_THAI { get; set; }
        public string DON_VI { get; set; }
        public string SO_BBGIAO { get; set; }
        public string TTIN_BBGIAO { get; set; }

        public decimal SO_TIEN_GIAO_VAT { get; set; }
        public decimal SO_TIEN_CHAM_VAT { get; set; }
        public string TEN_TNGAN { get; set; }
        public string MA_TNGAN { get; set; }
        public string MA_NVIEN_GIAO { get; set; }
        public string TEN_NVIEN_GIAO { get; set; }

        public string PC_CODE_COMMON { get; set; }

        public string MA_DON_VI { get; set; }
        public DateTime D_NGAY_GIAO { get; set; }
        public string NGAY_GIAO { get; set; }

    }

    public class ObjDebtReliefSummuryReport
    {
        public int ID { get; set; }
        public string MA_DVIQLY { get; set; }
        public string TEN_DVIQLY { get; set; }
        public string SO_BBGIAO { get; set; }
        public string TEN_TNGAN { get; set; }
        public string NVIEN_GIAO { get; set; }
        public DateTime D_NGAY_GIAO { get; set; }
        public string NGAY_GIAO { get; set; }
        public int SO_HDON_GIAO { get; set; }
        public decimal SO_TIEN_GIAO { get; set; }
        public decimal SO_THUE_GIAO { get; set; }
        public decimal SO_TIEN_GIAO_TONG { get; set; }
        public int SO_HDON_THU_DUOC { get; set; }
        public decimal SO_TIEN_THU_DUOC { get; set; }
        public decimal SO_GTGT_THU_DUOC { get; set; }
        public decimal SO_TIEN_THU_DUOC_TONG { get; set; }
        public string TRANG_THAI { get; set; }

        public int SO_HDON_TON { get; set; }
        public decimal SO_TIEN_TON { get; set; }
        public decimal SO_GTGT_TON { get; set; }
        public decimal SO_TIEN_TON_TONG { get; set; }
        public string PC_CODE_COMMON { get; set; }
    }

    public class ObjBookCMIMS
    {
        public string edong { get; set; }
        public string bookCMIS { get; set; }
        public string pcCode { get; set; }
        public string status { get; set; }
        public string pcName { get; set; }
    }

    public class BillItem
    {
        public string customerCode { get; set; }
        public object customerPayCode { get; set; }
        public long billId { get; set; }
        public object term { get; set; }
        public string strTerm { get; set; }
        public decimal amount { get; set; }
        public string period { get; set; }
        public string issueDate { get; set; }
        public string strIssueDate { get; set; }
        public object status { get; set; }
        public string seri { get; set; }
        public string pcCode { get; set; }
        public object handoverCode { get; set; }
        public object cashierCode { get; set; }
        public string bookCmis { get; set; }
        public object fromDate { get; set; }
        public object toDate { get; set; }
        public string strFromDate { get; set; }
        public string strToDate { get; set; }
        public string home { get; set; }
        public double tax { get; set; }
        public string billNum { get; set; }
        public string currency { get; set; }
        public string priceDetails { get; set; }
        public string numeDetails { get; set; }
        public string amountDetails { get; set; }
        public string oldIndex { get; set; }
        public string newIndex { get; set; }
        public string nume { get; set; }
        public decimal amountNotTax { get; set; }
        public decimal amountTax { get; set; }
        public long multiple { get; set; }
        public string billType { get; set; }
        public string typeIndex { get; set; }
        public string groupTypeIndex { get; set; }
        public object createdDate { get; set; }
        public object idChanged { get; set; }
        public object dateChanged { get; set; }
        public object edong { get; set; }
        public object pcCodeExt { get; set; }
        public object code { get; set; }
        public string name { get; set; }
        public object nameNosign { get; set; }
        public string address { get; set; }
        public object addressNosign { get; set; }
        public object phoneByevn { get; set; }
        public object phoneByecp { get; set; }
        public object electricityMeter { get; set; }
        public string inning { get; set; }
        public object road { get; set; }
        public object station { get; set; }
        public object taxCode { get; set; }
        public object trade { get; set; }
        public object countPeriod { get; set; }
        public object team { get; set; }
        public object type { get; set; }
        public object lastQuery { get; set; }
        public object groupType { get; set; }
        public object billingChannel { get; set; }
        public object billingType { get; set; }
        public object billingBy { get; set; }
        public object cashierPay { get; set; }
        public object responseCode { get; set; }
        public object description { get; set; }
        public object strBillId { get; set; }
        public object strAmount { get; set; }
        public object strStatus { get; set; }
        public object strAmountNotTax { get; set; }
        public object strAmountTax { get; set; }
        public object strMultiple { get; set; }
        public object strType { get; set; }
        public object strTax { get; set; }
    }

    public class PendingReportItem
    {
        public long id { get; set; }
        public string txnDate { get; set; }
        public string edong { get; set; }
        public string cashierPay { get; set; }
        public long ecpBillId { get; set; }
        public string providerCode { get; set; }
        public string billingType { get; set; }
        public long offType { get; set; }
        public long intOffType { get; set; }
        public object auditNumberCore { get; set; }
        public long status { get; set; }
        public string requestDate { get; set; }
        public string billingDate { get; set; }
        public string responseDate { get; set; }
        public string responseCode { get; set; }
        public string description { get; set; }
        public string work { get; set; }
        public long pcId { get; set; }
        public long numberBillingTime { get; set; }
        public object pcCode { get; set; }
        public object customerCode { get; set; }
        public object strAmount { get; set; }
        public object strBillId { get; set; }
        public object strOffType { get; set; }
        public object strAuditNumber { get; set; }
        public string strRequestDate { get; set; }
        public object strResponseDate { get; set; }
        public string strWorkDate { get; set; }
        public object strTerm { get; set; }
        public object customer { get; set; }
        public BillItem bill { get; set; }
        public object billSerial { get; set; }
        public long auditNumber { get; set; }
    }

    public class DebtForBankObject
    {
        public object customerCode { get; set; }
        public object customerPayCode { get; set; }
        public object billId { get; set; }
        public object term { get; set; }
        public object strTerm { get; set; }
        public decimal amount { get; set; }
        public object period { get; set; }
        public object issueDate { get; set; }
        public object strIssueDate { get; set; }
        public long status { get; set; }
        public object seri { get; set; }
        public object pcCode { get; set; }
        public object handoverCode { get; set; }
        public object cashierCode { get; set; }
        public object bookCmis { get; set; }
        public object fromDate { get; set; }
        public object toDate { get; set; }
        public object strFromDate { get; set; }
        public object strToDate { get; set; }
        public object home { get; set; }
        public object tax { get; set; }
        public object billNum { get; set; }
        public object currency { get; set; }
        public object priceDetails { get; set; }
        public object numeDetails { get; set; }
        public object amountDetails { get; set; }
        public object oldIndex { get; set; }
        public object newIndex { get; set; }
        public object nume { get; set; }
        public object amountNotTax { get; set; }
        public object amountTax { get; set; }
        public object multiple { get; set; }
        public object billType { get; set; }
        public object typeIndex { get; set; }
        public object groupTypeIndex { get; set; }
        public object createdDate { get; set; }
        public object idChanged { get; set; }
        public string dateChanged { get; set; }
        public object edong { get; set; }
        public string pcCodeExt { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public object nameNosign { get; set; }
        public object address { get; set; }
        public object addressNosign { get; set; }
        public object phoneByevn { get; set; }
        public object phoneByecp { get; set; }
        public object electricityMeter { get; set; }
        public object inning { get; set; }
        public object road { get; set; }
        public object station { get; set; }
        public object taxCode { get; set; }
        public object trade { get; set; }
        public object countPeriod { get; set; }
        public object team { get; set; }
        public object type { get; set; }
        public object lastQuery { get; set; }
        public object groupType { get; set; }
        public object billingChannel { get; set; }
        public object billingType { get; set; }
        public object billingBy { get; set; }
        public object cashierPay { get; set; }
        public object responseCode { get; set; }
        public object description { get; set; }
        public object strBillId { get; set; }
        public object strAmount { get; set; }
        public object strStatus { get; set; }
        public object strAmountNotTax { get; set; }
        public object strAmountTax { get; set; }
        public object strMultiple { get; set; }
        public object strType { get; set; }
        public object strTax { get; set; }
    }

    public class FileExcel_HN
    {
        public string BillID { get; set; }
        public string BookCMIS { get; set; }
        public string CustomerCode { get; set; }
        public string pcCode { get; set; }     
        public string Name { get; set; }
        public string Address { get; set; }
        public string Delivery_Date { get; set; }
        public string Term { get; set; }
        public decimal Amount { get; set; }
        public string strAmount { get; set; }
        public string Time_Online { get; set; }       
        public string Desc { get; set; }
        public string status { get; set; }
    }

}