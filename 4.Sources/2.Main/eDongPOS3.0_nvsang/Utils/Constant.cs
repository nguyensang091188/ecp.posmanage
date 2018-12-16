using log4net;
using System;
using System.Collections.Generic;

namespace ePOS3.Utils
{
    public class Constant
    {
        public static Dictionary<string, string> StatusAccountant()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("-1", ALL);
            dict.Add("0", "Chưa thanh toán");
            dict.Add("1", "Đã thanh toán");
            return dict;
        }

        public static Dictionary<int, string> ECPayService()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(0, ALL);
            dict.Add(1, "Tiền điện");
            dict.Add(2, "Tiền nước");
            dict.Add(3, "Di động trả trước");
            dict.Add(4, "Di động trả sau");
            dict.Add(5, "Mã thẻ điện thoại");
            dict.Add(6, "Truyền hình");
            dict.Add(7, "Mã thẻ data 3G - 4G");
            dict.Add(8, "Mã thẻ game");
            dict.Add(9, "Tài chính");
            return dict;
        }

        public static Dictionary<string, string> CardData()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0506", "Data 3G Mobifone");
            return dict;
        }

        public static Dictionary<string, string> Provider_VTV()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0501", "VTVCab");
            return dict;
        }
        public static Dictionary<string, string> TopupPrePaid() // Tra truoc - Mua ma the
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0471", "VinaPhone");
            dict.Add("NCC0470", "MobiPhone");
            dict.Add("NCC0469", "Viettel");
            dict.Add("NCC0472", "VietNamMobile");
            dict.Add("NCC0481", "GMobile");
            return dict;
        }

        public static Dictionary<string, string> TopupPostPaid() // Tra sau
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0471", "VinaPhone");
            dict.Add("NCC0470", "MobiPhone");
            dict.Add("NCC0469", "Viettel");
            return dict;
        }

        public static Dictionary<string, string> CardGame()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0473", "VTC-ONLINE-VCOIN");
            dict.Add("NCC0474", "FPT-GATE");
            dict.Add("NCC0478", "ZING");
            dict.Add("NCC0489", "MYCARD");
            dict.Add("NCC0491", "ONCASH");
            dict.Add("NCC0492", "SOFTNYX");
            dict.Add("NCC0494", "GARENA");
            dict.Add("NCC0508", "LIKE");
            dict.Add("NCC0509", "MOBAY");
            dict.Add("NCC0510", "BIT");
            dict.Add("NCC0511", "VCARD");
            dict.Add("NCC0512", "APPOTA");
            dict.Add("NCC0514", "GRAB");
            return dict;
        }

        public static Dictionary<int, string> DetailService()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(0, ALL);
            dict.Add(1, "Tiền nước");
            dict.Add(2, "Di động trả trước");
            dict.Add(3, "Di động trả sau");
            dict.Add(4, "Mua mã thẻ");
            dict.Add(5, "Truyền hình");
            dict.Add(6, "Mã thẻ data 3G - 4G");
            dict.Add(7, "Mã thẻ game");
            dict.Add(9, "Tài chính");
            return dict;
        }
        public static Dictionary<string, string> Provider_Water()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0504", "Đà Nẵng");
            dict.Add("NCC0505", "Huế");
            return dict;
        }


        public static Dictionary<string, string> LogFile()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            //dict.Add("EVN-CPC", "");
            //dict.Add("EVN-HCM", "");
            //dict.Add("EVN-HN", "");
            //dict.Add("EVN-NPC", "");
            //dict.Add("EVN-SPC", "");
            dict.Add("CANHAN", "^0(16[2-9]|9[6-8])\\d{7}$|^0(9[14]|12[3-579])\\d{7}$|^0(9[03]|12[0-268])\\d{7}$|^0(1?99)\\d{7}$|^0(18[68]|92)\\d{7}$|^08[689]\\d{7}$");
            return dict;
        }
        public static Dictionary<string, string> PaymentStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("000", "Thành công");
            dict.Add("BILLING_TYPE_OFF", "Đang xử lý");
            dict.Add("092", "Nghi ngờ chờ xử lý");
            dict.Add("806", "HT_Đã TT nguồn khác");
            dict.Add("096", "HT_Đã TT số ví khác");
            dict.Add("099", "HT_Từ chối lỗi");
            dict.Add("095", "HT_Hóa đơn bị hủy");
            return dict;
        }

        public static Dictionary<string, string> ServiceStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("BILLING_TYPE_BILLING", "Thành công");
            dict.Add("BILLING_TYPE_TIMEOUT", "Nghi ngờ chờ xử lý");
            dict.Add("BILLING_TYPE_REJECT", "Hoàn trả");
            return dict;
        }

        public static Dictionary<string, string> UnitRepeats()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("SECOND", "Giây");
            dict.Add("MINUTE", "Phút");
            dict.Add("HOUS", "Giờ");
            return dict;
        }
        public static Dictionary<long, string> CheckStatus()
        {
            Dictionary<long, string> dict = new Dictionary<long, string>();
            dict.Add(1, "Yêu cầu mới");
            dict.Add(2, "Đang xử lý");
            dict.Add(3, "Xử lý thành công");
            dict.Add(4, "Xử lý lỗi");
            return dict;
        }
        public static Dictionary<string, string> StatusAcc()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("3", ALL);
            dict.Add("1", "TK thường");
            dict.Add("2", "TK tiền điện");
            dict.Add("4", "TK TĐTT");
            return dict;
        }

        public static Dictionary<string, string> BillStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("0", "Chưa thanh toán");
            dict.Add("1", "Đã thanh toán");
            dict.Add("2", "Đang chờ xử lý");
            return dict;
        }
        public static Dictionary<string, string> BillEVNStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "Còn nợ");
            dict.Add("2", "Hết nợ");
            return dict;
        }
        public static Dictionary<string, string> BillHandlingType()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("2", "Cờ");
            dict.Add("1", "Phiên GCS");
            dict.Add("4", "Đẩy chấm nợ Client");
            dict.Add("3", "Điện lực hết hạn mức");
            //dict.Add("5", "Phiên GCS");
            return dict;
        }
        public static Dictionary<string, string> BillHandlingStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("0", "Đang chờ xử lý");
            dict.Add("2", "Tự động");
            dict.Add("4", "Không tự động");
            dict.Add("1", "Hoàn thành");
            dict.Add("3", "Hủy");
            return dict;
        }

        public static Dictionary<string, string> BillHandlingCode()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("000", "Thành công");
            dict.Add("825", "Nguồn khác");
            dict.Add("814", "Ví khác");
            dict.Add("819", "Time Out");
            dict.Add("999", "Lỗi");
            return dict;
        }

        public static Dictionary<string, string> PaymentResult()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("BILLING_TYPE_EDONG_OTHER", "Hóa đơn thanh toán bởi ví khác");
            dict.Add("BILLING_TYPE_SOURCE_OTHER", "Hóa đơn được thanh toán bởi đối tác khác");
            dict.Add("BILLING_TYPE_TIMEOUT", "Giao dịch nghi ngờ chờ xử lý");
            dict.Add("BILLING_TYPE_ERROR", "Thanh toán lỗi");
            dict.Add("BILLING_TYPE_REVERT", "Hủy hóa đơn");
            dict.Add("BILLING_TYPE_OFF", "Đang chờ xử lý chấm nợ");
            dict.Add("BILLING_TYPE_BILLING", "Thanh toán thành công");
            dict.Add("BILLING_TYPE_PROCESS", "HĐ được check tồn về đã TT từ process");
            dict.Add("TKTD", "Tài khoản tiền điện");
            return dict;
        }
        public static Dictionary<string, string> BillingType()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("EDONG_OTHER", "Hóa đơn thanh toán bởi ví khác");
            dict.Add("SOURCE_OTHER", "Hóa đơn được thanh toán bởi đối tác khác");
            dict.Add("TIMEOUT", "Giao dịch nghi ngờ chờ xử lý");
            dict.Add("ERROR", "Thanh toán lỗi");
            dict.Add("REVERT", "Hủy hóa đơn");
            dict.Add("OFF", "Đang chờ xử lý chấm nợ");
            dict.Add("BILLING", "Thanh toán thành công");
            dict.Add("PROCESS", "HĐ được check tồn về đã TT từ process");
            return dict;
        }
        public static Dictionary<string, string> OnOffCredit()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("EVN-HN", "1");
            dict.Add("EVN-HCM", "2");
            dict.Add("EVN-NPC", "3");
            dict.Add("EVN-SPC", "4");
            dict.Add("EVN-CPC", "5");
            dict.Add("EVN-HP", "6");
            dict.Add("EVN-KH", "7");
            return dict;
        }
        public static Dictionary<string, string> EVN()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "Tổng CTY Điện lực HÀ NỘI");
            dict.Add("2", "Tổng CTY Điện lực HỒ CHÍ MINH");
            dict.Add("3", "Tổng CTY Điện lực miền Bắc");
            dict.Add("4", "Tổng CTY Điện lực miền Nam");
            dict.Add("5", "Tổng CTY Điện lực miền Trung");
            return dict;
        }

        public static Dictionary<string, string> BillHandling()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0468", "Tổng CTY Điện lực HÀ NỘI");
            dict.Add("NCC0466", "Tổng CTY Điện lực HỒ CHÍ MINH");
            dict.Add("NCC0483", "Tổng CTY Điện lực miền Bắc");
            dict.Add("NCC0498", "Tổng CTY Điện lực miền Nam");
            dict.Add("NCC0499", "Tổng CTY Điện lực miền Trung");
            dict.Add("NCC0500", "Công ty điện lực Hải Phòng");
            dict.Add("NCC0502", "Công ty điện lực Khánh Hòa");
            return dict;
        }
        public static Dictionary<string, string> Provider_Finance()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("NCC0515", "Home Credit");
            dict.Add("NCC0516", "Fe Credit");
            dict.Add("NCC0517", "Prudential Tài Chính");
            dict.Add("NCC0518", "MCredit");
            dict.Add("NCC0519", "MSB");
            dict.Add("NCC0520", "OCB");
            dict.Add("NCC0521", "Prudential");
            return dict;
        }

        public static Dictionary<string, string> CancelRequestStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "Chờ hủy do TNV nhập lên");
            //dict.Add("3", "Từ chối hủy");
            dict.Add("0", "Đã hủy");
            dict.Add("4", "Chuyển BP Kế toán");
            return dict;
        }
        public static Dictionary<string, string> StatusBookCMIS()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "Đã kích hoạt");
            dict.Add("0", "Chưa kích hoạt");
            return dict;
        }
        public static Dictionary<string, string> TypeAccount()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("1", "Tài khoản thường");
            dict.Add("2", "Tài khoản quầy thu đa năng");
            dict.Add("3", "Tài khoản quản lý");
            dict.Add("4", "Tài khoản tiền điện trả trước");
            dict.Add("5", "Tài khoản kế toán");
            dict.Add("-1", "Tài khoản hỗ trợ");
            dict.Add("0", "Ví tổng tiền điện");
            dict.Add("6", "Ví tổng tiền điện trả trước");
            return dict;
        }
        public static Dictionary<string, string> ParamStatus()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("1", "Hoạt động");
            dict.Add("0", "Không hoạt động");
            return dict;
        }

        public static Dictionary<string, string> APIMethod(string mtype = "0")
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (mtype == "1")
            {
                dict.Add("", "---Tất cả---");
            }
            dict.Add("GET", "GET");
            dict.Add("POST", "POST");
            dict.Add("PUT", "PUT");

            return dict;
        }
        public static Dictionary<string, string> LOGType(string mtype = "0")
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (mtype == "1")
            {
                dict.Add("", "---Tất cả---");
            }
            dict.Add("INFO", "INFO");
            dict.Add("DEBUG", "DEBUG");
            dict.Add("ERROR", "ERROR");

            return dict;
        }
        public static Dictionary<string, string> ApplicationList(string mtype = "0")
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (mtype == "1")
            {
                dict.Add("", "---Tất cả---");
            }
            dict.Add("ecpay-database", "ecpay-database");
            dict.Add("ecpay-finance", "ecpay-finance");
            dict.Add("ecpay-scheduler", "ecpay-scheduler");

            return dict;
        }
        public static string TKTD = "TKTD";
        public static string GROUP = "EVNPREPAID";
        public static string CODE = "MULTIPLE";
        public static string ONLINE = "Hoạt động";
        public static string OFFLINE = "Không hoạt động";

        public static int STATUS_1 = 1;
        public static int STATUS_0 = 0;

        public static string UPLOAD_FOLDER = "UploadedFiles";
        public static int CHILD_1 = 1;
        public static int CHILD_0 = 0;
        public static string STATUS_ONLINE = "1";
        public static string STATUS_OFFLINE = "0";
        public static string LEVEL_PC_1 = "1";
        public static string LEVEL_PC_2 = "2";
        public static string LEVEL_PC_3 = "3";
        public static int EVN_HANOI = 1;
        public static int EVN_HCM = 2;
        public static int EVN_NPC = 3;
        public static int EVN_SPC = 4;
        public static int EVN_CPC = 5;
        public static string WATER = "Nước";
        public static string TOPUP_PREPAID = "TOPUP-PREPAID";
        public static string TOPUP_POSTPAID = "TOPUP-POSTPAID";
        public static string BUY_CARD = "BUY-CARD";
        public static string FINANCE = "BILLING";

        public static string REPORT_SUM = "report_sum";
        public static string CONTROL_DEBTGCS = "ControlDebtGCS";
        public static string EDIT_BOOKCMIS = "edit_bookcmis";
        public static string REPORT_DETAIL = "report_detail";
        public static string REPORT_CASH = "report_cash";
        public static string REPORT_PREPAIDEVN = "report_prepaidEVN";
        public static string REPORT_PREPAIDEVN_DETAIL = "report_prepaidEVN_DETAIL";
        public static string REPORT_WALLET = "report_wallet";
        public static string REPORT_CASHDETAIL = "report_cashdetail";
        public static string REPORT_BILL = "report_bill";
        public static string REPORT_BILL_EVN = "report_bill_evn";
        public static string REPORT_CANCEL = "report_cancel";
        public static string REPORT_BILLHANDLING = "report_billhandling";
        public static string HASHTABLE_BILLHANDLING = "billhandling";
        public static string REPORT_TOTAL_BILLHANDLING = "report_total_billhandling";
        public static string REPORT_CARDIDENTIFIER = "report_CardIdentifier";
        public static string REPORT_SALES = "report_sales";
        public static string REPORT_BALANCE = "report_balance";
        public static string MANAGER_EDONG = "manager_edong";
        public static string PAGE_SIZE_LAST = "page_size_last";
        public static string PAGE_SIZE_LAST_ORTHER = "page_size_last_orther";
        public static string PAGE_SIZE_MANAGER_BOOKCMIS = "PAGE_SIZE_MANAGER_BOOKCMIS";
        public static string PAGE_SIZE_LAST_CARD = "page_size_last_card";
        public static string CUSTOMER_INFO_CHOICED = "CusByInfoChoice";
        public static string CUSTOMER_INFO__PAGE_CHOICED = "CusByInfoChoicePage";
        public static string REPORT_DETAIL_OTHER = "report_orther";
        public static string REPORT_DEBT = "REPORT_DEBT";
        public static string PAGE_SIZE_LAST_DEBT = "page_size_last_debt";





        #region Param
        public static string ONOFF_CREDIT = "OFF_FLAG";
        #endregion

        #region Title
        public static string INDEX_TITLE = "THÔNG TIN TÀI KHOẢN";
        public static string CHANGEPASS_TITLE = "ĐỔI MẬT KHẨU";
        public static string IMPORTDATA_TITLE = "HÓA ĐƠN GIAO THU";
        public static string IMPORTBILLPAYMENT_TITLE = "CHẤM NỢ";
        public static string IMPORTBOOKCMIS_TITLE = "GÁN GIAO THU THEO FILE";
        public static string RPTGENERALONOFF_TITLE = "BÁO CÁO THTT THU ON-OFF";
        public static string RPTEDONGCASH_TITLE = "BÁO CÁO VÍ - TIỀN MẶT";
        public static string RPTEVNPREPAID_TITLE = "BÁO CÁO TIỀN ĐIỆN TRẢ TRƯỚC";
        public static string RPTPOINTCOLLECTION_TITLE = "BÁO CÁO ĐIỂM THU";
        public static string RPTCHECKDEBT_TITLE = "BÁO CÁO CHECK TỒN";
        public static string CANCELREQUEST_TITLE = "DANH SÁCH YEU CẦU HỦY";
        public static string TRANFERSURVIVE_TITLE = "CHUYỂN TỒN";
        public static string EVNHP_TITLE = "QUẢN LÝ HẢI PHÒNG";
        public static string EVNKH_TITLE = "QUẢN LÝ KHÁNH HÒA";
        public static string HELP_SEARCHBILL_TITLE = "QUẢN LÝ HĐ";
        public static string HELP_SETPARAM_TITLE = "MÀN HÌNH QUẢN LÝ";
        public static string HELP_SETFINANCIAL_TITLE = "MÀN HÌNH QLDV TÀI CHÍNH";
        public static string HELP_ACCOUNT_TITLE = "QUẢN LÝ ĐĂNG NHẬP/ĐĂNG XUẤT";
        public static string HELP_LOGVIEW_TITLE = "TRA LOG";
        public static string MANAGERPC_TITLE = "QUẢN LÝ PC";
        public static string MANAGERACCOUNT_TITLE = "QUẢN LÝ VÍ";
        public static string MANAGER_LOGVIEW_TITLE = "TRA LOG";
        public static string MANAGER_BOOKCMIS_TITLE = "THIẾT LẬP TẢI SỔ GCS";
        public static string RPT_DELIVERYSPC_TITLE = "BÁO CÁO GIAO THU SPC";
        public static string RPT_DEBTRELIEFSPC_TITLE = "BÁO CÁO GIAO THU SPC";
        public static string RPT_WARNINASSIGN_NPC_TITLE = "BÁO CÁO HẠN MỨC";
        public static string RPT_HISTORYTRANFER_TITLE = "LỊCH SỬ GIAO DỊCH";
        public static string RPT_DEBTLIST_TITLE = "DANH SÁCH KHÁCH HÀNG";
        public static string RPT_CARDIDENTIFER_TITLE = "BÁO CÁO ĐỊNH DANH THẺ";
        public static string BILLHANDLING_TITLE = "QL HĐ ĐANG XỬ LÝ CHẤM  NỢ";
        public static string WITHDRAW_TITLE = "QL HẠN MỨC VÍ QUẦY";
        public static string CONTROLDEBT_TITLE = "KIỂM SOÁT CHẤM NỢ";
        public static string JOBCONTROL_TITILE = "QUẢN LÝ ĐỐI SOÁT";
        public static string RPTEDONGSALES_TITLE = "BÁO CÁO DOANH SỐ";
        public static string ALL = "-- Tất cả --";
        public static string NULL = "-------------";
        #endregion

        #region systemcontant
        public static string SUCCESS_CODE = "000";
        public static string CONNECTION_ERROR_CODE = "-001";
        public static string CONNECTION_ERROR_DESC = string.Format("{0} - Lỗi hệ thống, Vui lòng liên hệ bộ phận kỹ thuật", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        public static string PA_SUCCESS_CODE = "5017";
        #endregion

        #region Messages Error
        public static string ERROR_CONNECTION = "Lỗi kết nối";
        public static string ERROR_LOGIN_ACC = "Yêu cầu nhập thông tin trường tài khoản";
        public static string ERROR_ACC = "Tài khoản không đúng định dạng";
        public static string ERROR_LOGIN_PASS = "Yêu cầu nhập thông tin trường mật khẩu";
        public static string ERROR_LOGIN_CAPTCHA = "Vui lòng nhập mã xác thực";
        public static string ERROR_CAPTCHA = "Mã xác thực không đúng";
        public static string ERROR_LOGIN_CAPTCHA_TIMEOUT = "Vui lòng nhập lại mã xác thực";

        public static string ERROR_OLDPASS_NULL = "Mật khẩu cũ không đúng";
        public static string ERROR_NEWPASS_NULL = "Mật khẩu phải có 6-20 ký tự.";
        //public static string ERROR_NEWPASS_1 = "Mật khẩu gồm 6 đến 20 chữ hoặc số và không bao gồm ký tự đặc biệt";
        public static string ERROR_CONFIRMPASS_NULL = "Vui lòng nhập xác nhận mật khẩu";
        // public static string ERROR_CONFIRMPASS = "Xác nhận mật khẩu gồm 6 đến 8 chữ hoặc số và không bao gồm ký tự đặc biệt";
        public static string ERROR_NEWPASS_2 = "Xác nhận mật khẩu không khớp với mật khẩu mới";

        public static string ERROR_PCCODE = "Vui lòng nhập mã đơn vị";
        public static string ERROR_PCCODE_REGEX = "Mã đơn vị không đúng định dạng";
        public static string ERROR_PCEXT = "Vui lòng nhập tên mở rộng";
        public static string ERROR_PCFULLNAME = "Vui lòng nhập tên đầy đủ";
        public static string ERROR_PCSHORTNAME = "Vui lòng nhập tên viết tắt";
        public static string ERROR_PCLEVEL = "Vui lòng nhập cấp cho PC";
        public static string ERROR_PCPARENT = "Vui lòng chọn PC cha";
        public static string ERROR_EMAIL = "Vui lòng nhập địa chỉ mail theo định dạng name@domain.ext";
        public static string ERROR_PHONE_DUPLECATE = "Số điện thoại bị trùng";
        #endregion
    }

    public class Logging
    {

        public static ILog EPOSLogger = LogManager.GetLogger("EPOS");
        public static ILog PushLogger = LogManager.GetLogger("PUSH");
        public static ILog DaoLogger = LogManager.GetLogger("EPOSDAO");
        public static ILog LoginLogger = LogManager.GetLogger("LOGIN");
        public static ILog AccountLogger = LogManager.GetLogger("ACCOUNT");
        public static ILog CustomerLogger = LogManager.GetLogger("CUSTOMER");
        public static ILog SupportLogger = LogManager.GetLogger("SUPPORT");
        public static ILog ReportLogger = LogManager.GetLogger("REPORT");
        public static ILog ImportLogger = LogManager.GetLogger("IMPORT");
        public static ILog ManagementLogger = LogManager.GetLogger("MANAGEMENT");
        public static ILog AccountantLogger = LogManager.GetLogger("ACCOUNTANT");
        public static ILog BillLogger = LogManager.GetLogger("BILL");
        public static ILog ePosResFulLogger = LogManager.GetLogger("RESFUL");
    }
}