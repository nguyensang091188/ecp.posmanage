using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ePOS3.Utils
{
    public class ConvertResponseCode
    {
        public static string GetResponseDescription(int intRespCode)
        {
            string strDescription = string.Empty;
            switch (intRespCode)
            {
                case 0: strDescription = "Giao dịch thành công"; break;
                case 1: strDescription = "Giao dịch bị trùng lặp/duplicated"; break;
                case 2: strDescription = "Không đủ thời gian xử lý giao dịch"; break;
                case 3: strDescription = "Loại giao dịch không hợp lệ"; break;
                case 4: strDescription = "Mã nhà cung cấp không hợp lệ"; break;
                case 5: strDescription = "Nhà cung cấp không hỗ trợ dịch vụ"; break;
                //case 6: strDescription = "Mã đối tác không hợp lệ"; break;
                case 6: strDescription = "Điện lực đang quyết toán và chờ giao thu."; break;
                case 7: strDescription = "Điện lực giao thu nhưng chưa xác nhận"; break;
                case 8: strDescription = "Điện lực đã giao thu, đã xác nhận nhưng ECPay chưa lấy dữ liệu, chưa gọi hàm cập nhật trạng thái giao thu."; break;
                case 9: strDescription = "ECPay đang lấy dữ liệu, chưa cập nhật trạng thái giao thu"; break;
                // case 7: strDescription = "Đối tác không được sử dụng dịch vụ"; break;
                case 10: strDescription = "Chữ ký không hợp lệ"; break;
                case 20: strDescription = "Thông tin tài khoản hoặc mật khẩu không hợp lệ"; break;
                case 21: strDescription = "Tên khách hàng không hợp lệ"; break;
                case 22: strDescription = "Số chứng minh không hợp lệ"; break;
                case 23: strDescription = "Mật khẩu không hợp lệ"; break;
                case 24: strDescription = "Mật khẩu xác nhận không hợp lệ"; break;
                //case 24: strDescription = "PIN xác nhận không hợp lệ"; break;
                case 25: strDescription = "Định dạng tài khoản không hợp lệ"; break;
                case 26: strDescription = "Tài khoản ví đã tồn tại"; break;
                case 27: strDescription = "Tài khoản đã được đăng ký bởi khách hàng có số CMND khác"; break;
                case 29: strDescription = "Phiên làm việc chưa vượt giới hạn số lượng cho phép"; break;
                case 30: strDescription = "Thông tin tài khoản hoặc mật khẩu không hợp lệ"; break;
                //case 30: strDescription = "Tài khoản chưa đăng ký kênh dịch vụ"; break;
                case 31: strDescription = "Tài khoản ví đã bị hủy kênh dịch vụ"; break;
                //case 31: strDescription = "Tài khoản bị hủy kênh dịch vụ"; break;
                case 32: strDescription = "Tài khoản ví đã bị khóa kênh dịch vụ"; break;
                //case 32: strDescription = "Tài khoản bị khóa kênh dịch vụ"; break;
                case 33: strDescription = "Tài khoản ví chưa được duyệt kênh dịch vụ"; break;
                //case 33: strDescription = "Tài khoản kênh dịch vụ chưa được duyệt"; break;
                case 34: strDescription = "Không giải mã được mật khẩu"; break;
                //case 35: strDescription = "Mật khẩu không đúng"; break;
                case 35: strDescription = "Thông tin tài khoản hoặc mật khẩu không hợp lệ"; break;
                case 36: strDescription = "Vượt quá số lần đăng nhập đồng thời"; break;
                case 37: strDescription = "Phiên làm việc không tồn tại"; break;
                case 38: strDescription = "Phiên làm việc không hợp lệ"; break;
                case 39: strDescription = "Phiên làm việc hết thời gian hiệu lực"; break;
                //case 39: strDescription = "Session hết hiệu lực"; break;
                //case 40: strDescription = "Tài khoản ví chưa đăng ký"; break;
                case 40: strDescription = "Thông tin tài khoản hoặc mật khẩu không hợp lệ"; break;
                case 41: strDescription = "Tài khoản ví đã bị hủy"; break;
                case 42: strDescription = "Tài khoản ví đã bị khóa"; break;
                case 43: strDescription = "Tài khoản ví chưa được duyệt"; break;
                case 44: strDescription = "Vượt số lượng mã xác thực cho phép"; break;
                case 45: strDescription = "Mã xác thực không tồn tại"; break;
                case 46: strDescription = "Mã xác thực không hợp lệ"; break;
                case 47: strDescription = "Mã xác thực hết thời gian hiệu lực"; break;
                ///case 50: strDescription = "whitelist"; break;
                //case 51: strDescription = "blacklist"; break;
                case 52: strDescription = "Giao dịch trên hạn mức"; break;
                case 53: strDescription = "Giao dịch trên hạn mức"; break;
                case 54: strDescription = "Giao dịch dưới hạn mức"; break;
                case 55: strDescription = "Giao dịch dưới hạn mức"; break;
                case 56: strDescription = "Vượt hạn mức tổng giao dịch trong ngày"; break;
                case 57: strDescription = "Vượt hạn mức tổng giao dịch trong ngày"; break;
                case 58: strDescription = "Vượt hạn mức số lần giao dịch trong ngày"; break;
                case 59: strDescription = "Vượt hạn mức số lần giao dịch trong ngày"; break;
                case 60: strDescription = "Vượt hạn mức tổng thanh toán trong ngày"; break;
                case 66: strDescription = "Tài khoản ví nạp không tồn tại"; break;
                case 67: strDescription = "Tài khoản ví nạp đã bị hủy"; break;
                case 68: strDescription = "Tài khoản ví nạp đã bị khóa"; break;
                case 69: strDescription = "Tài khoản ví nạp chưa được duyệt"; break;
                case 70: strDescription = "Loại tài khoản ví nạp không hợp lệ"; break;
                //case 71: strDescription = "Tài khoản chuyển không tồn tại"; break;
                case 71: strDescription = "Tài khoản thanh toán không tồn tại"; break;
                //case 71: strDescription = "Tài khoản rút không tồn tại"; break;
                //case 72: strDescription = "Tài khoản chuyển đã bị hủy"; break;
                case 72: strDescription = "Tài khoản thanh toán đã bị hủy"; break;
                //case 72: strDescription = "Tài khoản rút đã bị hủy"; break;
                //case 73: strDescription = "Tài khoản chuyển đã bị khóa"; break;
                case 73: strDescription = "Tài khoản thanh toán đã bị khóa"; break;
                //case 73: strDescription = "Tài khoản rút đã bị khóa"; break;
                //case 74: strDescription = "Tài khoản chuyển chưa được duyệt"; break;
                case 74: strDescription = "Tài khoản thanh toán chưa được duyệt"; break;
                //case 74: strDescription = "Tài khoản rút chưa được duyệt"; break;
                //case 75: strDescription = "Loại tài khoản chuyển không hợp lệ"; break;
                case 75: strDescription = "Loại tài khoản thanh toán không hợp lệ"; break;
                //case 75: strDescription = "Loại tài khoản rút không hợp lệ"; break;
                case 76: strDescription = "Tài khoản nhận không tồn tại"; break;
                case 77: strDescription = "Tài khoản nhận đã bị hủy"; break;
                case 78: strDescription = "Tài khoản nhận đã bị khóa"; break;
                case 79: strDescription = "Tài khoản nhận chưa được duyệt"; break;
                case 80: strDescription = "Loại tài khoản nhận không hợp lệ"; break;
                case 81: strDescription = "Tài khoản chuyển chưa đổi mật khẩu"; break;
                case 82: strDescription = "Số tiền không hợp lệ"; break;
                //case 82: strDescription = "Mệnh giá nạp tiền không hợp lệ"; break;
                //case 82: strDescription = "Mệnh giá thẻ không hợp lệ"; break;
                case 83: strDescription = "Tài khoản không đủ số dư"; break;
                case 86: strDescription = "Tài khoản chuyển trùng với tài khoản nhận"; break;
                //case 86: strDescription = "Giao dịch lỗi và unlock lỗi"; break;
                case 88: strDescription = "Mã khách hàng không đúng định dạng"; break;
                case 90: strDescription = "Chưa đăng ký tài khoản ngân hàng"; break;
                //case 91: strDescription = "Giao dịch lỗi và unlock lỗi"; break;
                case 93: strDescription = "Không có giao dịch nào được tìm thấy"; break;
                case 95: strDescription = "Hiện tại không được thanh toán"; break;
                case 98: strDescription = "Giao dịch được treo lại để thanh toán sau"; break;
                case 99: strDescription = "Điện lực chưa hỗ trợ"; break;
                case 101: strDescription = "Chưa có dữ liệu giao thu"; break;
                case 604: strDescription = "Gửi mã xác thực lỗi"; break;
                //case 800: strDescription = "Đối tác ECPGW không tồn tại"; break;
                //case 801: strDescription = "Đối tác ECPGW đã bị khóa"; break;
                //case 802: strDescription = "IP không được phép truy cập"; break;
                //case 803: strDescription = "Sai MTI"; break;
                case 804: strDescription = "Sai chữ ký"; break;
                case 805: strDescription = "Khách hàng không tồn tại"; break;
                case 806: strDescription = "Số tiền không hợp lệ"; break;
                //case 807: strDescription = "Duplicated"; break;
                case 808: strDescription = "Đối tác không được sử dụng dịch vụ"; break;
                //case 809: strDescription = "Lỗi truy xuất CSDL"; break;
                case 810: strDescription = "Giao dịch nghi ngờ"; break;
                case 811: strDescription = "Hóa đơn hết hạn thanh toán"; break;
                case 812: strDescription = "Hóa đơn không tồn tại"; break;
                case 813: strDescription = "Sai mã khách hàng"; break;
                case 814: strDescription = "Hóa đơn đã thanh toán bởi ECPay"; break;
                case 815: strDescription = "Hết mệnh giá thẻ trong kho"; break;
                case 816: strDescription = "Nhà cung cấp không cho phép dùng dịch vụ"; break;
                //case 817: strDescription = "Tiền ECPay không đủ thực hiện giao dịch"; break;
                case 818: strDescription = "Khách hàng hết nợ"; break;
                //case 819: strDescription = "Timeout"; break;
                case 820: strDescription = "Giao dịch đã được hủy"; break;
                case 821: strDescription = "Lỗi hệ thống"; break;
                case 822: strDescription = "Lấy mã thẻ lỗi"; break;
                //case 823: strDescription = "Lỗi network"; break;
                case 824: strDescription = "Giao dịch lỗi"; break;
                case 825: strDescription = "Hóa đơn đã thanh toán bởi đối tác khác"; break;
                case 999: strDescription = "Lỗi hệ thống. Vui lòng liên hệ bộ phận kỹ thuật"; break;


                //LOI STORE
                case 0017: strDescription = "Không có thông tin mã khách hàng"; break;
                case 1099: strDescription = "Kết nối đến cơ sở dữ liệu đang bị ngắt"; break;
                case 1138: strDescription = "Tài khoản đang bị khóa"; break;
                case 1617: strDescription = "Tham số đầu vào để trống"; break;

                case 2006: strDescription = "Không tìm thấy bản ghi nào thỏa mãn để thực hiện nghiệp vụ"; break;
                case 2014: strDescription = "Edong cần mapping không tồn tại hoặc không hoạt động"; break;
                case 2015: strDescription = "Mã đơn vị EVN cần mapping không tồn tại hoặc không hoạt động"; break;
                case 2017: strDescription = "Edong cần mapping đã có ví cha"; break;
                case 2018: strDescription = "Trạng thái phải có giá trị 0 hoặc 1"; break;
                case 2019: strDescription = "Không thể gán ví cha là chính nó"; break;
            
                case 2022: strDescription = "Không thể check tồn đồng thời cả lô mã KH và lô mã Sổ GCS"; break;

                case 2032: strDescription = "Hóa đơn đã được giữ chấm nợ bởi số ví khác"; break;
                case 2039: strDescription = "Không thể login. Sai địa chỉ IP"; break;

                case 2044: strDescription = "Ví cấp dưới không được gán ví cấp trên làm con"; break;
                case 2046: strDescription = "Số ví thực hiện nghiệp vụ không được để trống"; break;

                case 2062: strDescription = "Ít nhất chọn 2 giá trị"; break;
                case 2063: strDescription = "Chưa Mapping ví edong với điện lực"; break;
                case 2064: strDescription = "Không thể khởi tạo WS SMS"; break;
                case 2065: strDescription = "Không có kết quả trả về khi gửi tin nhắn"; break;
                case 2066: strDescription = "Kết quả trả về khi gửi tin nhắn bị trống"; break;
                case 2067: strDescription = "Hệ thống chưa cấu hình đẩy đủ tham số cho nghiệp vụ"; break;
                case 2068: strDescription = "Số ví không tồn tại hoặc chưa kích hoạt"; break;
                case 2069: strDescription = "Điện lực không tồn tại hoặc chưa kích hoạt"; break;

                case 2070: strDescription = "Điện lực và số ví chưa được gán"; break;
                case 2071: strDescription = "Sổ ghi chỉ số chưa được thiết lập"; break;
                case 2087: strDescription = "Ví không có quyền thực hiện giao dịch này"; break;

                case 2090: strDescription = "Trùng khoảng thời gian đã cấu hình"; break;

                case 2121: strDescription = "Tài khoản không có quyền đăng nhập"; break;
                case 2122: strDescription = "Tài khoản đăng nhập lần đầu, yêu cầu cấu hình loại tài khoản"; break;

                case 3459: strDescription = "Sổ ghi chỉ số đã được gán"; break;

                case 4002: strDescription = "Hóa đơn không tồn tại"; break;

                case 5005: strDescription = "Trùng bản ghi"; break;
                case 5006: strDescription = "Không đủ số dư để chấm nợ"; break;
              

                case 9999: strDescription = "Lỗi hệ thống. Vui lòng liên hệ bộ phận kỹ thuật"; break;

                default:
                    strDescription = "Lỗi hệ thống. Vui lòng liên hệ bộ phận kỹ thuật"; break;
            }

            return strDescription;

        }


        public static string GetResponseDescriptionRestFull(int intRespCode)
        {
            string strDescription = string.Empty;
            switch (intRespCode)
            {
                case 200: strDescription = "Success"; break;
                case 201: strDescription = "Success"; break;
                case 400: strDescription = "Đối tượng tồn tại. Vui lòng liên hệ bộ phận kỹ thuật"; break;//Object is exist
                case 401: strDescription = "Không có quyền gọi API. Vui lòng liên hệ bộ phận kỹ thuật"; break;//Unauthorized
                case 500: strDescription = "Lỗi hệ thống. Vui lòng liên hệ bộ phận kỹ thuật"; break;//Internal Server Error
                default:
                    strDescription = "Lỗi hệ thống. Vui lòng liên hệ bộ phận kỹ thuật"; break;
            }
            return strDescription;
        }
    }
}