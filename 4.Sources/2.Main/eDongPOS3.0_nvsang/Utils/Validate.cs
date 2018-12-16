using ePOS3.eStoreWS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ePOS3.Utils
{
    public class Validate
    {
        private static string Code = Convert.ToString(ConfigurationManager.AppSettings["max_Code"]);
        private static string BookCMIS = Convert.ToString(ConfigurationManager.AppSettings["max_BookCMIS"]);

        public static string[] ProcessNumberGroup(string v)
        {
            // 1
            // Keep track of words found in this Dictionary.
            var d = new Dictionary<string, bool>();


            // 3
            // Split the input and handle spaces and punctuation.
            string[] a = v.Split(new char[] { ' ', ',', ';', ':', '.', '\n', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            string[] b = new string[a.Length];


            // 4
            // Loop over each word
            int i = 0;
            foreach (string current in a)
            {
                // 5
                // Lowercase each word
                string upper = current.ToUpper();

                // 6
                // If we haven't already encountered the word,
                // append it to the result.
                if (!string.IsNullOrEmpty(upper))
                {
                    b[i] = current;
                    i++;
                }
            }
            // 7
            // Return the duplicate words removed
            return b;
        }

        public static string[] ProcessCustomerGroup(string v)
        {
            // 1
            // Keep track of words found in this Dictionary.
            var d = new Dictionary<string, bool>();


            // 3
            // Split the input and handle spaces and punctuation.
            string[] a = v.Split(new char[] { ' ', ',', ';', ':', '.', '\n', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            string[] b = new string[a.Length];


            // 4
            // Loop over each word
            int i = 0;
            foreach (string current in a)
            {
                // 5
                // Lowercase each word
                string upper = current.ToUpper();

                // 6
                // If we haven't already encountered the word,
                // append it to the result.
                if (!string.IsNullOrEmpty(upper) && !d.ContainsKey(upper))
                {
                    b[i] = current;
                    d.Add(upper, true);
                    i++;
                }
            }
            // 7
            // Return the duplicate words removed
            return b;
        }

        public static string ProcessReplace(string v, bool flag)
        {
            string b = null;
            if (flag)
            {
                string[] a = ProcessCustomerGroup(v);
                foreach (string item in a)
                {
                    if (string.IsNullOrEmpty(b))
                        b = "'" + item + "'";
                    else
                        b = b + "," + "'" + item + "'";
                }
            }
            else
            {
                string[] a = ProcessNumberGroup(v);
                foreach (string item in a)
                {
                    b = b + item;

                }
            }
            return b;
        }

        public static bool Validate_Email(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                             @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                             @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            if (Regex.IsMatch(value, pattern))
                return true;
            else
                return false;
        }

        public static bool Validate_Account(string acc)
        {
            if (string.IsNullOrEmpty(acc)) return false;
            string pattern = @"^PATTVPC\d{4}$";
            if (!Regex.IsMatch(acc, pattern))
                return true;
            else
                return false;
        }

        public static string Login(string acc, string password, string captchar, string imgCaptcha, ref int index) {
            if (!Validate_Account(acc.Trim()))
                return "Ví không được phép truy cập";
            if (password.Trim().Length < 6 || password.Trim().Length > 20)
            {
                index = 1;
                return "Thông tin tài khoản hoặc mật khẩu không hợp lệ";
            }
            if (captchar.ToLower() != imgCaptcha.ToLower())
            {
                index = 2;
                return "Mã xác thực không đúng";
            }
            return null;
        }

        public static string ChagePass(string oldPass, string newPass, string confirmPass, ref int index)
        {
            index = 0;            
            if (newPass.CompareTo(oldPass) == 0)
            {
                index = 1;
                return "Mật khẩu mới không được trùng với mật khẩu cũ";
            }            
            if (newPass.CompareTo(confirmPass) != 0)
            {
                index = 2;
                return Constant.ERROR_NEWPASS_2;
            }
            return null;
        }

        public static string check_FileExit(string url, string file)
        {
            string[] filename = file.Split('.');

            bool exists = false;
            bool ischeck = false;
            int i = 0;
            string str_result = string.Empty;
            while (ischeck == false)
            {
                if (i == 0)
                {
                    exists = System.IO.File.Exists(url + @"\" + file);
                    if (!exists)
                    {
                        ischeck = true;
                        str_result = file;
                    }
                }
                else
                {
                    exists = System.IO.File.Exists(url + @"\" + filename[0] + "_" + i + "." + filename[1]);
                    if (!exists)
                    {
                        ischeck = true;
                        str_result = filename[0] + "_" + i + "." + filename[1];
                    }
                }
                i++;
            }
            return str_result;
        }

        public static string check_ReportGeneral(string acc, string Pay_FromDate,
            string Pay_ToDate, string Upload_FromDate, string Upload_ToDate, int index)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(acc))
                return "Vui lòng nhập thông tin VÍ TNV";
            if (index == 1)
                if (JsonConvert.DeserializeObject<string[]>(acc).Count() > 5)
                    return "Không chọn quá 5 ví TNV";
            if (string.IsNullOrEmpty(Pay_FromDate) && string.IsNullOrEmpty(Pay_ToDate) && string.IsNullOrEmpty(Upload_FromDate) && string.IsNullOrEmpty(Upload_ToDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (!string.IsNullOrEmpty(Pay_FromDate) || !string.IsNullOrEmpty(Pay_ToDate))
            {
                if (string.IsNullOrEmpty(Pay_FromDate))
                    return "Vui lòng nhập thời gian tìm kiếm";
                if (string.IsNullOrEmpty(Pay_ToDate))
                    return "Vui lòng nhập thời gian tìm kiếm";
                if (!DateTime.TryParseExact(Pay_FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                    return "Giá trị 'Chấm nợ Từ ngày' này không hợp lệ";
                if (!DateTime.TryParseExact(Pay_ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                    return "Giá trị 'Chấm nợ Đến ngày' này không hợp lệ";
                if (dateFrom.Date > date.Date)
                    return "Giá trị 'Chấm nợ Từ ngày' phải nhỏ hơn ngày hiện tại";
                if (dateTo.Date > date.Date)
                    return "Giá trị 'Chấm nợ Đến ngày' phải nhỏ hơn ngày hiện tại";
                if (dateFrom.Date > dateTo.Date)
                    return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
                if (Pay_FromDate.Substring(3).CompareTo(Pay_ToDate.Substring(3)) != 0)
                    return "Bạn phải chọn thời gian 'Chấm nợ' trong một tháng";
            }
            //if (!string.IsNullOrEmpty(Upload_FromDate) || !string.IsNullOrEmpty(Upload_ToDate))
            //{
            if (string.IsNullOrEmpty(Upload_FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(Upload_ToDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (!DateTime.TryParseExact(Upload_FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Thu Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(Upload_ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Giá trị 'Thu Đến ngày' này không hợp lệ";
            if (dateFrom.Date > date.Date)
                return "Giá trị 'Thu Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (dateTo.Date > date.Date)
                return "Giá trị 'Thu Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom.Date > dateTo.Date)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            if (Upload_FromDate.Substring(3).CompareTo(Upload_ToDate.Substring(3)) != 0)
                return "Bạn phải chọn thời gian 'Thu' trong một tháng";
            //}
            return null;
        }

        public static string check_EDongCash(string account, string FromDate, string ToDate)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(account))
                return "Vui lòng chọn thông tin VÍ TNV";
            if (string.IsNullOrEmpty(FromDate))
                return "Chưa chọn TỪ NGÀY tìm kiếm. Vui lòng kiểm tra lại";
            if (string.IsNullOrEmpty(ToDate))
                return "Chưa chọn ĐẾN NGÀY tìm kiếm. Vui lòng kiểm tra lại";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Giá trị 'Đến ngày' này không hợp lệ";
            if (dateFrom > date)
                return "'Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (dateTo > date)
                return "'Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom > dateTo)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            return null;
        }
        public static string check_EDongSales(string account, string FromDate, string ToDate)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            //if (string.IsNullOrEmpty(account))
            //    return "Vui lòng chọn thông tin VÍ TNV";
            if (string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập “từ ngày”  ";
            if (string.IsNullOrEmpty(ToDate))
                return "Vui lòng nhập “đến ngày”  ";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Giá trị 'Đến ngày' này không hợp lệ";
            if (dateFrom >= date)
                return "Từ ngày không được lớn hơn ngày hiện tại";
            //if (dateTo > date)
            //    return "'Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom > dateTo)
                return "Từ ngày không được lớn hơn đến ngày  ";
            if ((dateFrom.Year != dateTo.Year) || (dateFrom.Month != dateTo.Month))
                return "Thời gian tìm kiếm phải trong cùng 1 tháng";
            return null;
        }
        public static string check_ReportDebt(string id, string FromDate, string ToDate)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            if (string.IsNullOrEmpty(id))
            {
                if (string.IsNullOrEmpty(FromDate))
                    return "Chưa chọn từ ngày tìm kiếm. Vui lòng kiểm tra lại";
                if (string.IsNullOrEmpty(ToDate))
                    return "Chưa chọn đến ngày tìm kiếm. Vui lòng kiểm tra lại";
                if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                    return "Giá trị 'Từ ngày' này không hợp lệ";
                if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                    return "Giá trị 'Đến ngày' này không hợp lệ";
                DateTime date = DateTime.Now;
                if (dateFrom.Date > date.Date)
                    return "'Từ ngày' phải nhỏ hơn ngày hiện tại";
                if (dateTo.Date > date.Date)
                    return "'Đến ngày' phải nhỏ hơn ngày hiện tại";
                if (dateFrom.Date > dateTo.Date)
                    return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            }
            return null;
        }

        public static string check_AddDebt(string type, string email, string code)
        {
            if (string.IsNullOrEmpty(email))
                return "Vui lòng nhập thông tin Email";
            string[] array_email = email.Split(';');
            if (array_email.Length > 8)
                return "Số lượng email phải <= 8";
            foreach (var item in array_email)
            {
                if (!Validate_Email(item))
                    return Constant.ERROR_EMAIL;
            }
            if (string.IsNullOrEmpty(code))
                return "Vui lòng nhập thông tin Mã KH/GCS";
            if (int.Parse(type) == 1)
            {
                if (PhoneNumber.ProcessCustomerGroup(code).Length > int.Parse(Code))
                    return "Số lượng Mã khách hàng quá lớn";
            }
            else
            {
                if (PhoneNumber.ProcessCustomerGroup(code).Length > int.Parse(BookCMIS))
                    return "Số lượng mã ghi chỉ số quá lớn";
            }
            return null;
        }

        public static string check_MergeEvnpcBlocking(string FromDate, string ToDate, string WorkDate)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime dateWork = DateTime.MinValue;
            DateTime dateNow = DateTime.Now;
            if (string.IsNullOrEmpty(FromDate))
                return "Chưa chọn thời gian bắt đầu. Vui lòng kiểm tra lại";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Thời gian bắt đầu không hợp lệ";
            //if (dateFrom < dateNow)
            //    return "Thời gian bắt đầu phải lớn hơn ngày giờ hiện tại";
            if (string.IsNullOrEmpty(ToDate))
                return "Chưa chọn thời gian kết thúc. Vui lòng kiểm tra lại";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Thời gian kết thúc không hợp lệ";
            //if (dateTo < dateFrom)
            //    return "Thời gian kết thúc phải lớn hơn thời gian bắt đầu";
            if (string.IsNullOrEmpty(WorkDate))
                return "Chưa chọn thời gian chấm. Vui lòng kiểm tra lại";
            if (!DateTime.TryParseExact(WorkDate.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateWork))
                return "Thời gian chấm không hợp lệ";
            //if(dateWork < dateTo)
            //    return "Thời gian chấm phải lớn hơn thời gian kết thúc";
            return null;
        }
        public static string check_TransferMoney(string edong, string balance, string amount, string desc, string captchar, string tem_captchar, ePosAccount posAccount)
        {
            if (string.IsNullOrEmpty(edong))
                return "Phải nhập trường Ví quầy";
            long temp_balance = string.IsNullOrEmpty(balance) == true ? 0 : long.Parse(balance.Replace(",", "").Replace(".", ""));
            long temp_amount = string.IsNullOrEmpty(amount) == true ? 0 : long.Parse(amount.Replace(",", "").Replace(".", ""));
            if (temp_balance <= 0)
                return "Số dư khả dụng phải > 0";
            long Im_balance = 0;
            string[] array = { edong };
            try
            {
                responseEntity entity = ePosDAO.GetReportEdongWallet(array, posAccount);
                if (entity.code.CompareTo(Constant.SUCCESS_CODE) == 0)
                {
                    long _balance = 0;
                    long _waitpay = 0;
                    long _lockpay = 0;

                    for (int i = 0; i < entity.listResponseReportEdong.Count(); i++)
                    {
                        _lockpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).lockAmount) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).lockAmount);
                        _balance = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).balance) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).balance);
                        _waitpay = string.IsNullOrEmpty(entity.listResponseReportEdong.ElementAt(i).waitPay) == true ? 0 : long.Parse(entity.listResponseReportEdong.ElementAt(i).waitPay);
                    }
                    Im_balance = _balance - _lockpay - _waitpay;
                    if (Im_balance != temp_balance)
                        return "Yêu cầu cập nhật lại số dư khả dụng";
                }
                else
                {
                    return "Lấy số dư khả dụng tức thời bị lỗi.";
                }
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.ErrorFormat("check_TransferMoney => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                return "Lấy số dư khả dụng tức thời bị lỗi.";
            }

            if (string.IsNullOrEmpty(amount))
                return "Phải nhập trường Số tiền thu hồi";
            if (decimal.Parse(amount) <= 0)
                return "Số tiền thu hồi phải > 0";
            if (temp_amount > temp_balance)
                return "Số tiền thu hồi không được > Số dư khả dụng";
            if (string.IsNullOrEmpty(desc))
                return "Phải nhập trường Nội dung";
            if (string.IsNullOrEmpty(captchar))
                return "Phải nhập trường Mã xác thực";
            if (captchar.ToLower().CompareTo(tem_captchar.ToLower()) != 0)
                return "Mã xác thực không chính xác";
            return null;
        }

        public static string check_CashMoney(string edong, string amount, ePosAccount posAccount)
        {
            //Nếu là tổng thẻ tiền điện trả trước
            if(posAccount.type == 6)
            {
                ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
                var item = (from x in tempPosAcc.Childs where x.phoneNumber == edong select x).FirstOrDefault();
                //Nếu không phải ví TNV thể tiền điện trả trước
                if (4 != long.Parse(item.type))
                    return "<b> Ví tổng: " + (from x in Constant.TypeAccount() where long.Parse(x.Key) == posAccount.type select x).FirstOrDefault().Value +
                    " </br>Ví quầy: " + (from x in Constant.TypeAccount() where x.Key == item.type select x).FirstOrDefault().Value + ". </br> Vui lòng kiểm tra lại! </b>";
            }            
            if (string.IsNullOrEmpty(amount))
                return "Số tiền ứng để trống";
            if (int.Parse(amount.Trim().Replace(",", "").Replace(".", "")) == 0)
                return "Số tiền ứng phải lớn hơn 0";
            return null;
        }

        public static string check_SearchBillHandling(string pccode, string customer, string fromdate, string fromtime, string todate, string totime)
        {
            if (string.IsNullOrEmpty(pccode) && string.IsNullOrEmpty(customer.Trim()))
                return "Công ty điện lực hoặc mã khách hàng để trống. Vui lòng kiểm tra lại";
            if (string.IsNullOrEmpty(fromdate))
                return "Chưa chọn thu từ ngày. Vui lòng kiểm tra lại";
            if (string.IsNullOrEmpty(fromtime))
                return "Chưa chọn thời gian thu từ ngày. Vui lòng kiểm tra lại";
            if (string.IsNullOrEmpty(todate))
                return "Chưa chọn đên ngày. Vui lòng kiểm tra lại.";
            if (string.IsNullOrEmpty(totime))
                return "Chưa chọn đến giờ. Vui lòng kiểm tra lại";
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            if (!DateTime.TryParseExact(fromdate.Trim() + " " + fromtime.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Thời gian thu từ ngày không hợp lệ";
            if (!DateTime.TryParseExact(todate.Trim() + " " + totime.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Thời gian kết thúc không hợp lệ";
            if (dateTo < dateFrom)
                return "Vui lòng kiểm tra lại thời gian tra cứu";
            int monthDiff = MonthDiff(dateFrom, dateTo);
            if (monthDiff < 0)
                return "Vui lòng kiểm tra lại thời gian tra cứu";
            if (monthDiff > 1)
                return "Khoảng thời gian tra cứu không quá một tháng.";
            return null;
        }

        private static int MonthDiff(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return -1;
            }

            int months = ((endDate.Year * 12) + endDate.Month) - ((startDate.Year * 12) + startDate.Month);

            if (endDate.Day >= startDate.Day)
            {
                months++;
            }

            return months;
        }

        public static string checkTimeTransOff(string date, string bill, int index)
        {
            DateTime dateNow = DateTime.Now;
            DateTime dateFrom = DateTime.MinValue;
            if (!DateTime.TryParseExact(date.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Thời gian chấm không hợp lệ";
            if (index == 1)
                if (string.IsNullOrEmpty(bill))
                    return "Bạn phải chọn ít nhất một bản ghi để cập nhật";
            //if(dateNow > dateFrom)
            //    return "Vui lòng kiểm tra lại thời gian chấm";
            return null;
        }

        public static string check_TransferSurvive(string FromDate, string ToDate)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(FromDate))
                return "Chưa chọn TỪ NGÀY tìm kiếm. Vui lòng kiểm tra lại";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Từ ngày' này không hợp lệ";
            if (dateFrom > date)
                return "'Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (!string.IsNullOrEmpty(ToDate))
            {
                if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                    return "Giá trị 'Đến ngày' này không hợp lệ";
                if (dateTo > date.AddDays(30))
                    return "'Đến ngày' không vượt quá ngày hiện tại 1 tháng!";
                if (dateFrom > dateTo)
                    return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            }
            return null;
        }

        public static string check_AddTransferSurvive(string account, string date, string BookCMIS)
        {
            if (string.IsNullOrEmpty(account))
                return "Phải nhập thông tin tài khoản ví mới!";
            if (string.IsNullOrEmpty(date))
                return "Phải nhập thông tin hạn thu tồn!";
            DateTime dateTo = DateTime.MinValue;
            DateTime dateNow = DateTime.Now;
            if (!DateTime.TryParseExact(date.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Hạn thu tồn không hợp lệ";
            if (dateTo < dateNow)
                return "Hạn thu tồn phải lớn hơn ngày hiện tại";
            if (dateTo > dateNow.AddDays(30))
                return "Hạn thu tồn không vượt quá ngày hiện tại 1 tháng!";
            if (string.IsNullOrEmpty(BookCMIS.Substring(1, BookCMIS.Length - 2)))
                return "Bạn phải chọn ít nhất một sổ SGC";
            return null;
        }

        public static string check_CancelRequest(string FromDate, string ToDate)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;

            if (string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(ToDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTo))
                return "Giá trị 'Đến ngày' này không hợp lệ";
            DateTime date = DateTime.Now;
            if (dateFrom.Date > date.Date)
                return "'Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (dateTo.Date > date.Date)
                return "'Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom.Date > dateTo.Date)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            return null;
        }

        public static string check_AddMapBookCMIS(string bookcmis)
        {
            if (string.IsNullOrEmpty(bookcmis))
                return "Vui lòng nhập thông tin số quyển";
            string[] array = PhoneNumber.ProcessCustomerGroup(bookcmis);
            if (array.Length >= 30)
            {
                return "Bạn chỉ được gán 30 sổ một lần";
            }
            return null;
        }

        public static string check_UpdateAcc(string email, string debtdate)
        {
            if (!string.IsNullOrEmpty(email))
                if (!Validate_Email(email))
                    return Constant.ERROR_EMAIL;
            if (!string.IsNullOrEmpty(debtdate))
            {
                DateTime dateTo = DateTime.MinValue;
                DateTime date = DateTime.Now;
                if (!DateTime.TryParseExact(debtdate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                    return "Giá trị 'Cuối ngày' này không hợp lệ";

                if (dateTo.Date != date.Date)
                    return "Giá trị 'Cuối ngày' phải bằng ngày hiện tại";
            }
            return null;
        }

        public static string check_MergeEVNPC(string parten, string code, string ext, string fullname, string shortname,
            string emailCC, string emailTO, string phone1, string phone2, string address, string tax, string cardEVN, string providerCode, ref int index)
        {
            index = 0;
            if (string.IsNullOrEmpty(code))
            {
                index = 1;
                return Constant.ERROR_PCCODE;
            }

            //if (!string.IsNullOrEmpty(parten)) //^PQ.*
            //    if (!Regex.IsMatch(code.ToUpper(), "^"+ parten+".*"))
            //    {
            //        index = 1;
            //        return Constant.ERROR_PCCODE_REGEX;
            //    }                   
            if (string.IsNullOrEmpty(fullname))
            {
                index = 2;
                return Constant.ERROR_PCFULLNAME;
            }
            if (string.IsNullOrEmpty(ext))
            {
                index = 3;
                return Constant.ERROR_PCEXT;
            }
            //if (string.IsNullOrEmpty(ext))
            //{
            //    index = 6;
            //    return Constant.ERROR_PCEXT;
            //}
            //if (!string.IsNullOrEmpty(phone1) && !string.IsNullOrEmpty(phone2))
            //    if (phone1.CompareTo(phone2) == 0)
            //        return Constant.ERROR_PHONE_DUPLECATE;
            if (!string.IsNullOrEmpty(emailTO))
                foreach (var item in emailTO.Split(';'))
                {
                    if (!Validate_Email(item))
                    {
                        index = 4;
                        return Constant.ERROR_EMAIL;
                    }
                }


            if (!string.IsNullOrEmpty(emailCC))
                foreach (var item in emailCC.Split(';'))
                {
                    if (!Validate_Email(item))
                    {
                        index = 5;
                        return Constant.ERROR_EMAIL;
                    }
                }
            if (string.IsNullOrEmpty(providerCode))
            {
                index = 6;
                return "Vui lòng nhập mã đối tác";
            }
            //if (string.IsNullOrEmpty(cardEVN))
            //{
            //    index = 7;
            //    return "Vui lòng nhập mã thẻ điện lực";
            //}
            return null;
        }

        public static string check_SearchBill(string customer, string name, string address, string billId, string booKCMIS,
            string FromDate, string ToDate, string month, string amount_from, string amount_to)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(customer) && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(address) && string.IsNullOrEmpty(billId) &&
                string.IsNullOrEmpty(booKCMIS) && string.IsNullOrEmpty(FromDate) && string.IsNullOrEmpty(ToDate) && string.IsNullOrEmpty(month) &&
                string.IsNullOrEmpty(amount_from) && string.IsNullOrEmpty(amount_to))
                return "Vui lòng nhập thêm tham số để tìm kiếm";
            if (!string.IsNullOrEmpty(FromDate))
            {
                if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                    return "Giá trị 'Từ ngày' này không hợp lệ";
                if (dateFrom.Date > date.Date)
                    return "'Từ ngày' phải nhỏ hơn ngày hiện tại";
                if (!string.IsNullOrEmpty(month))
                    if (FromDate.Substring(3).CompareTo(month) != 0)
                        return "Bạn phải tìm kiếm trong " + month;

            }
            if (!string.IsNullOrEmpty(ToDate))
            {
                if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTo))
                    return "Giá trị 'Đến ngày' này không hợp lệ";
                if (dateTo.Date > date.Date)
                    return "'Đến ngày' phải nhỏ hơn ngày hiện tại";
                if (!string.IsNullOrEmpty(month))
                    if (ToDate.Substring(3).CompareTo(month) != 0)
                        return "Bạn phải tìm kiếm trong " + month;
            }
            if (dateFrom.Date > dateTo.Date)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            return null;
        }

        public static string check_UpdateOnOffCredit(string date)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateNow = DateTime.Now;
            if (string.IsNullOrEmpty(date))
                return "Chưa chọn thời gian. Vui lòng kiểm tra lại";
            if (!DateTime.TryParseExact(date.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị thời gian không hợp lệ";
            if (dateFrom < dateNow)
                return "Giá trị thời gian phải lớn hơn ngày giờ hiện tại";
            return null;
        }

        public static string check_Job(string code, string name, string desc, string subject, string day, ref int index)
        {
            index = 0;
            if (string.IsNullOrEmpty(code))
            {
                index = 1;
                return "Phải nhập thông tin mã.";
            }
            if (string.IsNullOrEmpty(name))
            {
                index = 2;
                return "Phải nhập thông tin tên.";
            }
            if (string.IsNullOrEmpty(desc))
            {
                index = 3;
                return "Phải nhập thông tin mô tả.";
            }
            if (string.IsNullOrEmpty(subject))
            {
                index = 4;
                return "Phải nhập thông tin đối tượng thực thi.";
            }
            if (!string.IsNullOrEmpty(day))
                foreach (var item in PhoneNumber.ProcessCustomerGroup(day))
                {
                    if (int.Parse(item.Trim()) <= 0 || int.Parse(item.Trim()) > 32)
                    {
                        index = 5;
                        return "Thông tin Ngày chạy không đúng định dạng!";
                    }
                }
            return null;
        }

        public static string check_ControlDebtGCS(string pc)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(pc))
                return "Vui lòng chọn điện lực";            
            return null;
        }

        public static string check_ReportWater(string acc, string FromDate, string ToDate, int index)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(acc))
                return "Vui lòng nhập thông tin VÍ TNV";
            if (index == 1)
                if (JsonConvert.DeserializeObject<string[]>(acc).Count() > 5)
                    return "Không chọn quá 5 ví TNV";
            if (string.IsNullOrEmpty(ToDate) && string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(ToDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Chấm nợ Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Giá trị 'Chấm nợ Đến ngày' này không hợp lệ";
            if (dateFrom.Date > date.Date)
                return "Giá trị 'Chấm nợ Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (dateTo.Date > date.Date)
                return "Giá trị 'Chấm nợ Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom.Date > dateTo.Date)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            if (FromDate.Substring(3).CompareTo(ToDate.Substring(3)) != 0)
                return "Bạn phải chọn thời gian 'Chấm nợ' trong một tháng";
            return null;
        }

        public static string check_Param(string group, string code, string value, ref int index)
        {
            index = 0;
            if (string.IsNullOrEmpty(group))
            {
                index = 1;
                return "Phải nhập thông tin nhóm.";
            }
            if (string.IsNullOrEmpty(code))
            {
                index = 2;
                return "Phải nhập thông tin mã.";
            }
            if (string.IsNullOrEmpty(value))
            {
                index = 3;
                return "Phải nhập thông tin giá trị";
            }
            return null;
        }

        public static string checkTreeHandling(string FromDate, string TimeFrom, string ToDate, string TimeTo)
        {
            if(string.IsNullOrEmpty(FromDate) && string.IsNullOrEmpty(ToDate) && string.IsNullOrEmpty(TimeFrom) && string.IsNullOrEmpty(TimeTo))
                return null;
            else
            {
                DateTime dateFrom = DateTime.MinValue;
                DateTime dateTo = DateTime.MinValue;
                DateTime date = DateTime.Now;
                if(string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(TimeFrom))
                    return "Giá trị 'Thu HĐ từ ngày' này không hợp lệ";
                if (string.IsNullOrEmpty(ToDate) && !string.IsNullOrEmpty(TimeTo))
                    return "Giá trị 'đến ngày' này không hợp lệ";
                if (!string.IsNullOrEmpty(FromDate))
                { 
                    if (!DateTime.TryParseExact(string.IsNullOrEmpty(TimeFrom) == true?  FromDate.Trim() +" 00:00:00" : FromDate.Trim() +" " + TimeFrom.Trim(), 
                        "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                        return "Giá trị 'Thu HĐ từ ngày' này không hợp lệ";
                    if (dateFrom.Date > date.Date)
                        return "Giá trị 'Thu HĐ từ ngày' phải nhỏ hơn ngày hiện tại";
                }

                if (!string.IsNullOrEmpty(ToDate))
                {
                    if (!DateTime.TryParseExact(string.IsNullOrEmpty(TimeTo) == true? ToDate.Trim() +" 00:00:00" : ToDate.Trim() +" "+ TimeTo.Trim(),
                        "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                        return "Giá trị 'đến ngày' này không hợp lệ";
                    if (dateTo.Date > date.Date)
                        return "Giá trị 'đến ngày' phải nhỏ hơn ngày hiện tại";
                }
                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    if (dateFrom.Date > dateTo.Date)
                        return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
                }
                return null;
            }
        }

        public static string checkDebtForBank(string pcCode, string FromDate, string ToDate)
        {
           
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            if (string.IsNullOrEmpty(pcCode))
                return "Vui lòng chọn điện lực";
            if (string.IsNullOrEmpty(ToDate) && string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(ToDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Chấm nợ Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Giá trị 'Chấm nợ Đến ngày' này không hợp lệ";
            if (dateFrom.Date > date.Date)
                return "Giá trị 'Chấm nợ Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (dateTo.Date > date.Date)
                return "Giá trị 'Chấm nợ Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom.Date > dateTo.Date)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            if (FromDate.Trim().Substring(3).CompareTo(ToDate.Trim().Substring(3)) != 0)
                return "Bạn phải chọn thời gian trong một tháng";
            return null;
        }

        public static string check_ControlDebtGCS(string pc, string day_1, string day_2, string day_3)
        {
            if (string.IsNullOrEmpty(pc))
                return "Vui lòng chọn điện lực";
            if (string.IsNullOrEmpty(day_1.Trim()))
                return "Vui lòng chọn số ngày giữ tính từ phiên GCS";
            if (string.IsNullOrEmpty(day_2.Trim()))
                return "Vui lòng chọn số ngày giữu tính từ ngày thu";
            if (int.Parse(day_2.Trim()) >= int.Parse(day_1.Trim()))
                return "Số ngày giữ sau ngày thu phải nhỏ hơn số ngày giữ sau phiên GCS";
            if (string.IsNullOrEmpty(day_3.Trim()))
                return "Vui lòng chọn ngày bắt đầu không giữ";
            if (int.Parse(day_3.Trim()) <= int.Parse(day_1.Trim()))
                return "Ngày bắt đầu không giữ phải lớn hơn số ngày giữ sau phiên GCS";
            return null;
        }

        public static string checkAPI(string name, string method, string url)
        {
            if (string.IsNullOrEmpty(name.Trim()))
                return "Vui lòng nhập tên";
            if (string.IsNullOrEmpty(method.Trim()))
                return "Vui lòng nhập thuộc tính";
            if (string.IsNullOrEmpty(url.Trim()))
                return "Vui lòng nhập đường dẫn";
            return null;
        }
        public static string check_ReportFinance(string acc, string FromDate, string ToDate, int index)
        {
            DateTime dateFrom = DateTime.MinValue;
            DateTime dateTo = DateTime.MinValue;
            DateTime date = DateTime.Now;
            //if (string.IsNullOrEmpty(acc))
            //    return "Vui lòng nhập thông tin VÍ TNV";
            if (index == 1)
                if (JsonConvert.DeserializeObject<string[]>(acc).Count() > 5)
                    return "Không chọn quá 5 ví TNV";

            if (string.IsNullOrEmpty(ToDate) && string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(FromDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (string.IsNullOrEmpty(ToDate))
                return "Vui lòng nhập thời gian tìm kiếm";
            if (!DateTime.TryParseExact(FromDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                return "Giá trị 'Chấm nợ Từ ngày' này không hợp lệ";
            if (!DateTime.TryParseExact(ToDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
                return "Giá trị 'Chấm nợ Đến ngày' này không hợp lệ";
            if (dateFrom.Date > date.Date)
                return "Giá trị 'Chấm nợ Từ ngày' phải nhỏ hơn ngày hiện tại";
            if (dateTo.Date > date.Date)
                return "Giá trị 'Chấm nợ Đến ngày' phải nhỏ hơn ngày hiện tại";
            if (dateFrom.Date > dateTo.Date)
                return "'Từ ngày' phải nhỏ hơn 'Đến ngày' tìm kiếm";
            if (FromDate.Substring(3).CompareTo(ToDate.Substring(3)) != 0)
                return "Bạn phải chọn thời gian 'Chấm nợ' trong một tháng";
            return null;
        }
    }
}