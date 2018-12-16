using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ePOS3.Models
{

    public class AddControlDebtModel
    {
        public string Add_Corporation { get; set; }
        public string Add_PC { get; set; }
        public string Add_Day_1 { get; set; }
        public string Add_Day_2 { get; set; }
        public string Add_Day_3 { get; set; }
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> CorporationList;
        public IEnumerable<SelectListItem> Day_1_List;
        public IEnumerable<SelectListItem> Day_2_List;
        public IEnumerable<SelectListItem> Day_3_List;
        
    }


    public class EditBillHandlingModel
    {
        public string StatusEdit { get; set; }      
        public string Code { get; set; }
        public IEnumerable<SelectListItem> StatusList;
        public IEnumerable<SelectListItem> CodeList;
    }

    public class ReportPointCollectionModel
    {
        public string PC { get; set; }
        public string Service { get; set; }
        public string Status_Acc { get; set; }
        public string AccParent { get; set; }
        public string AccChild { get; set; }

        public IEnumerable<SelectListItem> Services;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> StatusAccList;
        public IEnumerable<SelectListItem> ParentList;
        public IEnumerable<SelectListItem> ChildList;
    }

    public class ReportDetailEVNModel
    {
        public string PC_EVN { get; set; }
        public string AccParent_EVN { get; set; }
        public string AccChild_EVN { get; set; }
        public string Status_EVN { get; set; }
        public string StatusAcc_EVN { get; set; }

        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> ParentList;
        public IEnumerable<SelectListItem> ChildList;
        public IEnumerable<SelectListItem> StatusDetailList;
        public IEnumerable<SelectListItem> StatusAccList;
    }
    public class ReportDebtListModel
    {
        public string EVN { get; set; }
        public string PC { get; set; }  
        public string Status { get; set; }
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> EVNList;
        public IEnumerable<SelectListItem> StatusList;
    }
    public class ReportDetailWater
    {
        public string Service_Water { get; set; }
        public string PC_Water { get; set; }
        public string AccParent_Water { get; set; }
        public string AccChild_Water { get; set; }
        public string Status_Water { get; set; }

        public IEnumerable<SelectListItem> ServiceList;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> ParentList;
        public IEnumerable<SelectListItem> ChildList;
        public IEnumerable<SelectListItem> StatusList;
    }

    public class ReportDetailTopup
    {
        public string Service_Topup { get; set; }
        public string PC_Topup { get; set; }
        public string AccParent_Topup { get; set; }
        public string AccChild_Topup { get; set; }
        public string Status_Topup { get; set; }
        public IEnumerable<SelectListItem> ServiceList;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> ParentList;
        public IEnumerable<SelectListItem> ChildList;
        public IEnumerable<SelectListItem> StatusList;
    }

    public class ControlDebtModel
    {
        public string Corporation { get; set; }
        public string pc { get; set; }
        public string pc_code { get; set; }
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> CorporationList;
    }

    public class ReportEDongCashModel
    {
       
        public string Wallet { get; set; }
        public string WalletDetail { get; set; }
        #region Tong hop      
        public string AccountParent { get; set; }     
        public string AccountChild { get; set; }
        #endregion
        #region Chi tiet      
        public string AccountParentDetail { get; set; }  
        public string AccountChildDetail { get; set; }        
        #endregion
        #region Tài khoản ví
        [Display(Name = "VÍ QUẢN LÝ")]
        public string AccountParentWallet { get; set; }

        [Display(Name = "VÍ TNV")]
        public string AccountChildWallet { get; set; }
        #endregion
        public IEnumerable<SelectListItem> StatusList;
        public IEnumerable<SelectListItem> WalletList;
        public IEnumerable<SelectListItem> AccountList;
        public IEnumerable<SelectListItem> AccAssignList;
    }
    public class ReportEDongSalesModel
    {

        public string Wallet { get; set; }
        public string WalletBalance { get; set; }
        #region Doanh so      
        public string AccountParent { get; set; }
        public string AccountChild { get; set; }
        #endregion
        #region So du      
        public string AccountParentBalance { get; set; }
        public string AccountChildBalance { get; set; }
        #endregion
        #region Tài khoản ví
        [Display(Name = "VÍ QUẢN LÝ")]
        public string AccountParentWallet { get; set; }

        [Display(Name = "VÍ TNV")]
        public string AccountChildWallet { get; set; }
        #endregion
        public IEnumerable<SelectListItem> WalletList;
        public IEnumerable<SelectListItem> AccountList;
        public IEnumerable<SelectListItem> AccAssignList;
        
    }

    public class CheckDebtModel
    {
        
        public string Deb_Id { get; set; }
       
        public string Deb_FromDate { get; set; }
       
        public string Deb_ToDate { get; set; }
     
        public string Deb_Edong { get; set; }

     
        public string Stock_Type { get; set; }
     
        public string Stock_Edong { get; set; }
      
        public string Stock_Email { get; set; }
       
        public string Stock_PC { get; set; }
      
        public string Stock_Code { get; set; }

        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> EdongList;
    }

    public class DeliverySPCModel
    {
       
        public string PCCode { get; set; }
        public string PCCode_Detail { get; set; }
       
        public string BBBG { get; set; }
        public string BBBG_Detail { get; set; }
      
        public string BookCMIS { get; set; }
        public string BookCMIS_Detail { get; set; }
       
        public string Account { get; set; }
        public string Account_Detail { get; set; }
      
        public string Month { get; set; }
        public string Month_Detail { get; set; }
      
        public string Year { get; set; }
        public string Year_Detail { get; set; }
        public IEnumerable<SelectListItem> MonthList;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> YearList;
    }

    public class CardIdentifierModel
    {
        public string PC { get; set; }
        public string Customer { get; set; }
        public string CardCode { get; set; }
        public IEnumerable<SelectListItem> PCList;
    }

    public class WithdrawModel
    {       
        public string Account { get; set; }
        public string AccountCash { get;set; }

        public IEnumerable<SelectListItem> AccList;
    }

    public class BillHandlingModel
    {
        [Display(Name = "CÔNG TY ĐL")]
        public string PCCode { get; set; }
        [Display(Name = "VÍ TNV")]
        public string Account { get; set; }
        [Display(Name = "MÃ KHÁCH HÀNG")]
        public string Customer { get; set; }
        [Display(Name = "LOẠI GIỮ CHẤM NỢ")]
        public string Type { get; set; }
        [Display(Name = "THU TỪ NGÀY")]
        public string Fromdate { get; set; }
        [Display(Name = "ĐẾN NGÀY")]
        public string Todate { get; set; }
        [Display(Name = "TRẠNG THÁI CHẤM NỢ")]
        public string Status { get; set; }
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> AccList;
        public IEnumerable<SelectListItem> TypeList;
        public IEnumerable<SelectListItem> StatusList;

    }

    public class TransferSurviveModel
    {
        [Display(Name = "CÔNG TY ĐIỆN LỰC")]
        public string PCCode { get; set; }
        public string Add_PCCode { get; set; }

        [Display(Name = "SỔ GCS")]
        public string BookCMIS { get; set; }
        public string Add_BookCMIS { get; set; }
        [Display(Name = "HẠN THU TỒN TỪ NGÀY")]
        public string FromDate { get; set; }
        [Display(Name = "ĐẾN NGÀY")]
        public string ToDate { get; set; }
        [Display(Name = "HẠN THU TỒN")]
        public string Add_ToDate { get; set; }
        [Display(Name = "TÀI KHOẢN VÍ MỚI")]
        public string Account { get; set; }
        public string Add_Account { get; set; }
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> BookCMISList;
        public IEnumerable<SelectListItem> AccList;
    }

    public class CancelRequestModel
    {
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string pcCode { get; set; }
        [Display(Name = "VÍ TNV")]
        public string Edong { get; set; }
        [Display(Name = "MÃ KHÁCH HÀNG")]
        public string Customer { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string status { get; set; }
        [Display(Name = "TỪ NGÀY")]
        public string FromDate { get; set; }
        [Display(Name = "ĐẾN NGÀY")]
        public string ToDate { get; set; }
        public IEnumerable<SelectListItem> StatusList;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> EdongList;
        public IEnumerable<SelectListItem> DecidedList;
        [Display(Name = "Quyết định")]
        public string Decided { get; set; }
        [Display(Name = "Lí do")]
        public string Desc { get; set; }
    }

    public class BoockCMISModel
    {
        [Display(Name = "TỔNG CTY")]
        public string Corporation { get; set; }
        public string Corporation_History { get; set; }
       
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string PCCode { get; set; }
        public string PCCode_History { get; set; }
       

        [Display(Name = "Số BBBG")]
        public string handOver { get; set; }
        [Display(Name = "SỐ GCS")]
        public string BookCMIS { get; set; }
        public string BookCMIS_History { get; set; }
        [Display(Name = "NGÀY GCS")]
        public string Day { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string Status { get; set; }

        public IEnumerable<SelectListItem> CorporationList;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> StatusCMISList;
        public IEnumerable<SelectListItem> DayList;
    }

    public class AddBookCISModel
    {
        [Display(Name = "TỔNG CTY")]
        public string Add_Corporation { get; set; }
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string Add_PCCode { get; set; }
        [Display(Name = "SỐ GCS")]
        public string Add_BookCMIS { get; set; }
        [Display(Name = "NGÀY GCS")]
        public string Add_Day { get; set; }
        [Display(Name = "NGÀY PHÁT HÀNH")]
        public string Add_Released { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string Add_Status { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
        public IEnumerable<SelectListItem> CorporationList;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> DayList;
    }

    public class ManagerAccModel
    {
        [Display(Name = "SỐ VÍ")]
        public string Edong { get; set; }
        [Display(Name = "HỌ TÊN")]
        public string FullName { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string Status { get; set; }
        [Display(Name = "CẤP")]
        public string Level { get; set; }
        public string Account { get; set; }
        public IEnumerable<SelectListItem> StatusList;
    }

    public class MapBookCMISModel
    {

        public string EDongCMIS { get; set; }
        [Display(Name = "SỐ QUYỂN")]
        public string BookCMIS { get; set; }
        public string book { get; set; }
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string pcCode { get; set; }
        public string pc { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string statusCMIS { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> StatusCMISList;
    }
    public class EditAccountModel
    {
        public string parent { get; set; }
        public string account { get; set; }
        [Display(Name = "HỌ VÀ TÊN")]
        public string name { get; set; }
        [Display(Name = "SỐ GIẤY TỜ")]
        public string IdNumber { get; set; }
        [Display(Name = "NGÀY CẤP")]
        public string IdNumberDate { get; set; }
        [Display(Name = "NƠI CẤP")]
        public string IdNumberPlace { get; set; }
        [Display(Name = "LOẠI TK")]
        public string type { get; set; }
        [Display(Name = "ĐỊA CHỈ")]
        public string address { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }

        [Display(Name = "ĐIỆN THOẠI")]
        public string phone { get; set; }

        [Display(Name = "CUỐI NGÀY")]
        public string debtdate { get; set; }
        [Display(Name = "SỐ TIỀN NỢ VÍ TỔNG")]
        public string debtamount { get; set; }
        public IEnumerable<SelectListItem> TypeList;
    }

    public class ManagerPCModel
    {
        [Display(Name = "MÃ SỐ THUẾ")]
        public string TaxCode { get; set; }

        [Display(Name = "MÃ ĐƠN VỊ")]
        public string Code { get; set; }
        [Display(Name = "TÊN ĐƠN VỊ")]
        public string Name { get; set; }
        [Display(Name = "ĐỊA CHỈ")]
        public string Address { get; set; }
        [Display(Name = "SỐ ĐIỆN THOẠI")]
        public string Phone { get; set; }
        [Display(Name = "TỔNG CÔNG TY")]
        public string Provider { get; set; }
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string PC { get; set; }
        public IEnumerable<SelectListItem> Providers;
        public IEnumerable<SelectListItem> PCList;
    }

    public class EVNPCModel
    {
        [Display(Name = "MÃ ĐƠN VỊ")]
        public string CodePC { get; set; }
        [Display(Name = "TÊN ĐẦY ĐỦ")]
        public string FullName { get; set; }
        [Display(Name = "TÊN VIẾT TẮT")]
        public string ShortName { get; set; }
        [Display(Name = "MÃ MỞ RỘNG")]
        public string Ext { get; set; }
        [Display(Name = "ĐỊA CHỈ")]
        public string AddressPC { get; set; }
        [Display(Name = "SĐT CSKH")]
        public string Phone1 { get; set; }
        [Display(Name = "SĐT Sửa chữa")]
        public string Phone2 { get; set; }
        [Display(Name = "EMAIL-TO")]
        public string MailTo { get; set; }
        [Display(Name = "EMAIL-CC")]
        public string MailCc { get; set; }
        [Display(Name = "CẤP")]
        public string level { get; set; }
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string PCCode { get; set; }
        [Display(Name = "TỔNG CTY")]
        public string Corporation { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string Status { get; set; }
        [Display(Name = "MÃ SỐ THUẾ")]
        public string Tax_Code { get; set; }
        [Display(Name = "MÃ THẺ ĐL")]
        public string CardEVN { get; set; }

        [Display(Name = "MÃ ĐỐI TÁC")]
        public string ProviderCode { get; set; }
        public IEnumerable<SelectListItem> Corporations;
        public IEnumerable<SelectListItem> PCList;
        public IEnumerable<SelectListItem> StatusList;
    }

    public class SearchBillModel
    {
        [Display(Name = "CTY ĐIỆN LỰC")]
        public string pcCode { get; set; }
        [Display(Name = "MÃ HÓA ĐƠN")]
        public string billId { get; set; }
        [Display(Name = "MÃ KHÁCH HÀNG")]
        public string customer { get; set; }
        [Display(Name = "TÊN KHÁCH HÀNG")]
        public string name { get; set; }
        [Display(Name = ("ĐIỆN THOẠI KH"))]
        public string phone { get; set; }
        [Display(Name = "ĐỊA CHỈ")]
        public string address { get; set; }
        [Display(Name = "SỔ GCS")]
        public string BookCMIS { get; set; }
        [Display(Name = "TRẠNG THÁI TT")]
        public string status { get; set; }
        [Display(Name = "THÁNG HĐ")]
        public string month { get; set; }
        [Display(Name = "PHÁT HÀNH TỪ NGÀY")]
        public string fromdate { get; set; }
        [Display(Name = "ĐẾN NGÀY")]
        public string todate { get; set; }
        [Display(Name = "SỐ TIỀN TỪ")]
        public string amount_from { get; set; }
        [Display(Name = "ĐẾN")]
        public string amount_to { get; set; }
        public IEnumerable<SelectListItem> statusList;
        public IEnumerable<SelectListItem> pcList;
        [Display(Name = "TRẠNG THÁI TT")]
        public string statusEVN { get; set; }
        public IEnumerable<SelectListItem> statusEVNList;
    }

    public class SetParamModel
    {
        [Display(Name = "TRẠNG THÁI")]
        public string status_job { get; set; }
        public string status_param { get; set; }
        public string Corporation { get; set; }

        public string status { get; set; }        
        public string agent { get; set; }

        public string name { get; set; }
        public string desc { get; set; }
        public string PC { get; set; }
        public IEnumerable<SelectListItem> StatusList;
        public IEnumerable<SelectListItem> CorporationList;
        public IEnumerable<SelectListItem> PCList;
        
    }

    public class LogViewModel
    {
        [Display(Name = "TỪ NGÀY")]
        public string FromDate { get; set; }
        public string FromDate_Store { get; set; }
        [Display(Name = "ĐẾN NGÀY")]
        public string ToDate { get; set; }
        public string ToDate_Store { get; set; }
        [Display(Name = "VÍ QUẢN LÝ")]
        public string Parent { get; set; }
        [Display(Name = "VÍ TNV")]
        public string Account { get; set; }

        [Display(Name = "MÁY CHỦ")]
        public string Server { get; set; }
        public IEnumerable<SelectListItem> Types;
        public IEnumerable<SelectListItem> ParentList;
        public IEnumerable<SelectListItem> AccountList;
        public IEnumerable<SelectListItem> ServerList;
    }

    public class EditJobControlModel
    {
        public string Corporation { get; set; }
        public string PCJobControl { get; set; }
        public string offFlag { get; set; }
        public string offWork { get; set; }
        public string cdrTime { get; set; }
        public string open { get; set; }
        public string type { get; set; }
        public string checkCdr { get; set; }
        public string checkHoliday { get; set; }
        public string checkKeep { get; set; }
        public string status { get; set; }
        public string cdrSat { get; set; }
        public string cdrSun { get; set; }

        public IEnumerable<SelectListItem> CorporationList;
        public IEnumerable<SelectListItem> PCList;
    }

    public class AddJobModel
    {
        [Display(Name = "MÃ")]
        public string Add_JobCode { get; set; }
        [Display(Name = "TÊN")]
        public string Add_JobName { get; set; }
        [Display(Name = "MÔ TẢ")]
        public string Add_JobDesc { get; set; }
        [Display(Name = "ĐỐI TƯỢNG THỰC THI")]
        public string Add_JobSubject { get; set; }
        [Display(Name = "NGÀY CHẠY")]
        public string Add_JobDay { get; set; }
        [Display(Name = "LẶP LẠI")]
        public string Add_JobRepeat { get; set; }
        [Display(Name = "THỜI GIAN LẶP LẠI")]
        public string Add_JobTimeRepeat { get; set; }
        [Display(Name = "ĐƠN VỊ LẶP LẠI")]
        public string Add_JobUnitRepeat { get; set; }
        [Display(Name = "SỐ LUỒNG XỬ LÝ")]
        public string Add_JobNumProcess { get; set; }
        [Display(Name = "THỜI GIAN BẮT ĐẦU")]
        public string Add_JobTimeStart { get; set; }
        [Display(Name = "THỜI GIAN KẾT THÚC")]
        public string Add_JobTimeEnd { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string Add_JobStatus { get; set; }

        public IEnumerable<SelectListItem> UnitRepeatList;

    }

    public class SearchAPIModel
    {
        [Display(Name = "TÊN")]
        public string API_Name { get; set; }
        [Display(Name = "THUỘC TÍNH")]
        public string API_Method { get; set; }
        public IEnumerable<SelectListItem> APIMethodList;
        [Display(Name = "ĐƯỜNG DẪN")]
        public string API_Url { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string API_status { get; set; }

    }
    public class AddAPIModel
    {
        [Display(Name = "TÊN")]
        public string Add_APIName { get; set; }
        [Display(Name = "THUỘC TÍNH")]
        public string Add_APIMethod { get; set; }
        public IEnumerable<SelectListItem> APIMethodList;
        [Display(Name = "ĐƯỜNG DẪN")]
        public string Add_APIURL { get; set; }
        [Display(Name = "TRẠNG THÁI")]
        public string Add_APIStatus { get; set; }

    }
}
