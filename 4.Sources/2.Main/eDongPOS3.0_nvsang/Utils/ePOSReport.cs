using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Collections;
using System.IO;

namespace ePOS3.Utils
{
    public class ePOSReport
    {
        public static ExcelPackage Sumary(string id, string template, string fromdate, string todate, string service, string pcCode, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["BCTonghop"];
            ws.Cells[2, 1].Value = "Từ ngày: " + fromdate + " đến ngày: " + todate;
            ws.Cells[3, 1].Value = string.IsNullOrEmpty(service) == true ? "Dịch vụ: Tất cả" : "Dịch vụ: " + service;
            ws.Cells[4, 1].Value = string.IsNullOrEmpty(pcCode) == true ? "Nhà cung cấp: Tất cả" : "Nhà cung cấp: " + pcCode;
            ws.Cells[6, 2].Value = posAccount.edong + " - " + posAccount.name;
            ws.Cells[7, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            if (ePOSSession.GetObject(id) != null)
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                int rowIndex = 12;
                bool check = false;
                int count = 0;
                for (int i = 0; i < items.Count(); i++)
                {
                    //if (string.IsNullOrEmpty(items.ElementAt(i).col_1))
                    //{
                    //    check = true;
                    //    count = count + 1;
                    //}
                    //else check = false;



                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 1].Value = items.ElementAt(i).col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = items.ElementAt(i).col_20;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = decimal.Parse(items.ElementAt(i).col_21);
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = decimal.Parse(items.ElementAt(i).col_22);
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = decimal.Parse(items.ElementAt(i).col_2);
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = decimal.Parse(items.ElementAt(i).col_3);
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Value = decimal.Parse(items.ElementAt(i).col_4);
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 8].Value = decimal.Parse(items.ElementAt(i).col_5);
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 9].Value = decimal.Parse(items.ElementAt(i).col_10);
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Value = decimal.Parse(items.ElementAt(i).col_11);
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = decimal.Parse(items.ElementAt(i).col_18);
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 12].Value = decimal.Parse(items.ElementAt(i).col_19);
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 13].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 13].Value = decimal.Parse(items.ElementAt(i).col_6);
                    ws.Cells[rowIndex, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 14].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 14].Value = decimal.Parse(items.ElementAt(i).col_7);
                    ws.Cells[rowIndex, 15].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 15].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 15].Value = decimal.Parse(items.ElementAt(i).col_8);
                    ws.Cells[rowIndex, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 16].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 16].Value = decimal.Parse(items.ElementAt(i).col_9);
                    ws.Cells[rowIndex, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 17].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 17].Value = decimal.Parse(items.ElementAt(i).col_12);
                    ws.Cells[rowIndex, 18].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 18].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 18].Value = decimal.Parse(items.ElementAt(i).col_13);
                    ws.Cells[rowIndex, 19].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 19].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 19].Value = decimal.Parse(items.ElementAt(i).col_14);
                    ws.Cells[rowIndex, 20].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 20].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 20].Value = decimal.Parse(items.ElementAt(i).col_15);
                    //try
                    //{
                    //    if ((!check && count > 0) || (i == items.Count() - 1 && count > 0))
                    //    {
                    //        ws.Cells[rowIndex - count, 1, rowIndex, 1].Merge = true;
                    //        ws.Cells[rowIndex - count, 2, rowIndex, 2].Merge = true;
                    //        count = 0;
                    //    }
                    //}
                    //catch (Exception)
                    //{


                    //}

                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[9, 1, rowIndex - 1, 20])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage SummaryDetail(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Detail_EVN"];
            ws.Cells[3, 1].Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 6;
                int i = 1;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 1].Value = i;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_1;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_15) == true ? 0 : decimal.Parse(item.col_15);
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_2;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 5].Value = item.col_3;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 6].Value = item.col_4;
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Value = string.IsNullOrEmpty(item.col_14) == true ? 0 : decimal.Parse(item.col_14);
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 8].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(item.col_5);
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 9].Value = item.col_6;
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Value = item.col_12;
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Value = item.col_13;
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 12].Value = item.col_7;
                    rowIndex++;
                    i++;
                }
                using (ExcelRange cell = ws.Cells[5, 1, rowIndex - 1, 12])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        //public static XLWorkbook SummaryDetail(string id, ePosAccount posAccount)
        //{
        //    XLWorkbook workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("Báo cáo điểm thu chi tiết");
        //    int row = 1, col = 1;

        //    worksheet.Style.Font.SetFontName("Times New Roman");
        //    worksheet.Range(string.Format("A{0}:L{1}", row, row)).Merge();
        //    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    worksheet.Cell(row, col).Value = "BÁO CÁO ĐIỂM THU CHI TIẾT";
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col).Style.Font.FontSize = 14;
        //    row++;
        //    row++;

        //    worksheet.Range(string.Format("A{0}:L{1}", row, row)).Merge();
        //    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //    worksheet.Cell(row, col).Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col).Style.Font.FontSize = 10;
        //    row++;
        //    row++;

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "STT";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Mã khách hàng";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Mã thẻ";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Tên khách hàng";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Địa chỉ";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Sổ GCS";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Nạp TKTĐ";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Số tiền";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Kỳ HĐ";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Ngày thu";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Ngày chấm";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Trạng thái";

        //    if (ePOSSession.GetObject(id) != null)
        //    {
        //        List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
        //        row++;
        //        int i = 1;
        //        foreach (var item in items)
        //        {
        //            worksheet.Cell(row, 1).Value = i;
        //            worksheet.Cell(row, 2).Value = item.col_1;
        //            worksheet.Cell(row, 3).Style.NumberFormat.Format = "@";
        //            worksheet.Cell(row, 3).Value = item.col_15;
        //            worksheet.Cell(row, 4).Value = item.col_2;
        //            worksheet.Cell(row, 5).Value = item.col_3;
        //            worksheet.Cell(row, 6).Style.NumberFormat.Format = "@";
        //            worksheet.Cell(row, 6).Value = item.col_4;
        //            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
        //            worksheet.Cell(row, 7).Value = item.col_14;
        //            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
        //            worksheet.Cell(row, 8).Value = item.col_5;
        //            worksheet.Cell(row, 9).Value = item.col_6;
        //            worksheet.Cell(row, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        //            worksheet.Cell(row, 10).Value = item.col_12;
        //            worksheet.Cell(row, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        //            worksheet.Cell(row, 11).Value = item.col_13;
        //            worksheet.Cell(row, 12).Value = item.col_7;
        //            row++;
        //            i++;
        //        }
        //    }
        //    IXLCells c2 = worksheet.Range(string.Format("A{0}:L{1}", 5, row - 1)).Cells();
        //    c2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //    c2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //    worksheet.Column(1).Width = 10;
        //    worksheet.Column(2).Width = 15;
        //    worksheet.Column(3).Width = 25;
        //    worksheet.Column(4).Width = 25;
        //    worksheet.Column(5).Width = 10;
        //    worksheet.Column(6).Width = 10;
        //    worksheet.Column(7).Width = 10;
        //    worksheet.Column(8).Width = 10;
        //    worksheet.Column(9).Width = 10;
        //    worksheet.Column(10).Width = 15;
        //    worksheet.Column(11).Width = 15;
        //    worksheet.Column(12).Style.Alignment.WrapText = true;
        //    worksheet.Column(12).Width = 25;
        //    return workbook;
        //}

        public static ExcelPackage CashWallet(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Sum_CashWallet"];
            ws.Cells[2, 1].Value = "Số ví: " + posAccount.edong;
            ws.Cells[3, 1].Value = "Họ và tên: " + posAccount.name;
            ws.Cells[4, 1].Value = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");

            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 6;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_6) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_6, false));
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Value = string.IsNullOrEmpty(item.col_7) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_7, false));
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 8].Value = string.IsNullOrEmpty(item.col_8) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_8, false));
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 9].Value = string.IsNullOrEmpty(item.col_9) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_9, false));
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(item.col_10) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_10, false));
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item.col_11) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_11, false));
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 12].Value = string.IsNullOrEmpty(item.col_12) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_12, false));
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 13].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 13].Value = string.IsNullOrEmpty(item.col_13) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_13, false));
                    ws.Cells[rowIndex, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 14].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 14].Value = string.IsNullOrEmpty(item.col_14) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_14, false));
                    ws.Cells[rowIndex, 15].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 15].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 15].Value = string.IsNullOrEmpty(item.col_15) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_15, false));
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[5, 1, rowIndex - 1, 15])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage PrepaidElectricity(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Sum_CashWallet"];
            ws.Cells[2, 1].Value = "Số ví: " + posAccount.edong;
            ws.Cells[3, 1].Value = "Họ và tên: " + posAccount.name;
            ws.Cells[4, 1].Value = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");

            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 6;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_6) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_6, false));
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[5, 1, rowIndex - 1, 6])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage PrepaidElectricityDetail(string id, string account, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Detail_CashWallet"];
            string[] array = account.Split('-');
            ws.Cells[3, 1].Value = "Số ví: " + array[0].Trim();
            ws.Cells[4, 1].Value = "Họ và tên: " + array[1].Trim();
            ws.Cells[5, 1].Value = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");
            if (ePOSSession.GetObject(id) != null)
            {
                ObjCashDetail data = (ObjCashDetail)ePOSSession.GetObject(id);
                ws.Cells[7, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[7, 9].Value = "Tồn đầu kỳ: " + data.Amount_old;
                int rowIndex = 10;
                foreach (var item in data.items)
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 2].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 2].Value = string.IsNullOrEmpty(item.col_2) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_2, false));
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    rowIndex++;
                }
                ws.Cells[rowIndex, 1, rowIndex, 4].Merge = true;
                ws.Cells[rowIndex, 1, rowIndex, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xD3D3D3));
                ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[rowIndex, 1].Style.Font.Bold = true;
                ws.Cells[rowIndex, 1].Style.Font.Italic = true;
                ws.Cells[rowIndex, 1].Value = "Tồn cuối kỳ" + data.Amount_old;

                using (ExcelRange cell = ws.Cells[7, 1, rowIndex, 4])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage CashWalletDetail(string id, string template, string account, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Detail_CashWallet"];
            string[] array = account.Split('-');

            ws.Cells[3, 1].Value = "Số ví: " + array[0].Trim();
            ws.Cells[4, 1].Value = "Họ và tên: " + array[1].Trim();
            ws.Cells[5, 1].Value = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");
            if (ePOSSession.GetObject(id) != null)
            {
                ObjCashDetail data = (ObjCashDetail)ePOSSession.GetObject(id);
                ws.Cells[10, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[10, 9].Style.Numberformat.Format = "#,##0";
                ws.Cells[10, 9].Value = string.IsNullOrEmpty(data.debt_old) == true ? 0 : decimal.Parse(Validate.ProcessReplace(data.debt_old, false));

                ws.Cells[10, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[10, 10].Style.Numberformat.Format = "#,##0";
                ws.Cells[10, 10].Value = string.IsNullOrEmpty(data.Amount_old) == true ? 0 : decimal.Parse(Validate.ProcessReplace(data.Amount_old, false));

                ws.Cells[10, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[10, 11].Style.Numberformat.Format = "#,##0";
                ws.Cells[10, 11].Value = string.IsNullOrEmpty(data.cash_old) == true ? 0 : decimal.Parse(Validate.ProcessReplace(data.cash_old, false));
                int rowIndex = 12;
                foreach (var item in data.items)
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 2].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 2].Value = string.IsNullOrEmpty(item.col_2) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_2, false));
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_6) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_6, false));
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Value = string.IsNullOrEmpty(item.col_7) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_7, false));
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 8].Value = string.IsNullOrEmpty(item.col_8) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_8, false));
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 9].Value = string.IsNullOrEmpty(item.col_9) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_9, false));
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(item.col_10) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_10, false));
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item.col_11) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_11, false));
                    rowIndex++;
                }
                ws.Cells[rowIndex, 1, rowIndex, 8].Merge = true;
                ws.Cells[rowIndex, 1, rowIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xD3D3D3));
                ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[rowIndex, 1].Style.Font.Bold = true;
                ws.Cells[rowIndex, 1].Style.Font.Italic = true;
                ws.Cells[rowIndex, 1].Value = "Tồn cuối kỳ";

                ws.Cells[rowIndex, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xD3D3D3));
                ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[rowIndex, 9].Style.Font.Bold = true;
                ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                ws.Cells[rowIndex, 9].Value = string.IsNullOrEmpty(data.debt_new) == true ? 0 : decimal.Parse(Validate.ProcessReplace(data.debt_new, false));

                ws.Cells[rowIndex, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 10].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xD3D3D3));
                ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[rowIndex, 10].Style.Font.Bold = true;
                ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(data.Amount_new) == true ? 0 : decimal.Parse(Validate.ProcessReplace(data.Amount_new, false));

                ws.Cells[rowIndex, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 11].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xD3D3D3));
                ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[rowIndex, 11].Style.Font.Bold = true;
                ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(data.cash_new) == true ? 0 : decimal.Parse(Validate.ProcessReplace(data.cash_new, false));

                using (ExcelRange cell = ws.Cells[7, 1, rowIndex, 11])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage EDongWallet(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Wallet_Account"];
            ws.Cells[2, 1].Value = "Số ví: " + posAccount.edong;
            ws.Cells[3, 1].Value = "Họ và tên: " + posAccount.name;
            ws.Cells[4, 1].Value = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");
            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 7;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_6) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_6, false));
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[7, 1, rowIndex - 1, 6])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static XLWorkbook DetailDelivery(Dictionary<string, List<ObjDeliveryDetailReport>> dicHoaDonTNV, ObjReportDeliveryDetailCommon common, ePosAccount posAccount)
        {
            int row = 1;
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BCChiTietDuLieuGiaoThu");
            worksheet.Style.Font.SetFontName("Times New Roman");
            worksheet.Style.Font.SetFontSize(12);
            List<ObjDeliveryDetailReport> listDelivery = new List<ObjDeliveryDetailReport>();
            foreach (var k in dicHoaDonTNV)
            {
                if (row > 1)
                {
                    row = row + 3;
                }
                listDelivery = k.Value;

                int col = 1;

                //Title
                worksheet.Range(string.Format("A{0}:M{1}", row, row)).Merge();
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Value = "BÁO CÁO CHI TIẾT DỮ LIỆU GIAO THU";
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Font.FontSize = 16;


                string strDvi = common == null ? "Mã Điện lực: " : "Mã Điện lực: " + common.PCCode;
                row++;
                worksheet.Range(string.Format("A{0}:D{1}", row, row)).Merge();
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Value = strDvi;


                string strDate = common == null ? "Từ ngày :                  " + "Đến ngày: "
                    : "Từ ngày : " + common.ToDate + "   Đến ngày: " + common.ToDate;
                worksheet.Range(string.Format("G{0}:H{1}", row, row)).Merge();
                worksheet.Cell(row, col + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col + 6).Style.Font.Italic = true;
                worksheet.Cell(row, col + 6).Value = strDate;

                // string strTNV = common == null ? "Mã TNV: " : "Mã TNV:" + common.AccountCode; //+ "-" + common.AccountName;
                string strTNV = common == null ? "Mã TNV: " : "Mã TNV:" + k.Key;

                row++;
                worksheet.Range(string.Format("A{0}:E{1}", row, row)).Merge();
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Value = strTNV;

                string strSoHD = common == null ? "Mã nhân viên: " + "Họ và tên nhân viên:"
                    : "Mã nhân viên: " + common.EmpCode + "  Họ và tên nhân viên:" + common.EmpName;
                row++;
                worksheet.Range(string.Format("A{0}:E{1}", row, row)).Merge();
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Value = strSoHD;


                row++;
                row++;
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "STT";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Số biên bản";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Ngày giao ";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Mã TNV";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Số hóa đơn";//Họ và tên thu ngân

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Mã KH";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Tên ĐC Khách hàng";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Số seri";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Mã GCS";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Mã KVUC-STT";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Kỳ-tháng/năm";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Dịch vụ";

                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col++).Value = "Số tiền";


                IXLCells c2 = worksheet.Range(string.Format("A{0}:M{1}", row, row)).Cells();
                c2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                c2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                int rowData = row;
                int rowCount = row;
                if (listDelivery != null && listDelivery.Count > 0)
                {
                    //Nhóm theo số ví
                    Dictionary<string, int> dEdongCount = new Dictionary<string, int>();

                    if (listDelivery != null)
                    {
                        int i = 1;
                        int stt = 1;
                        foreach (ObjDeliveryDetailReport obj in listDelivery)
                        {
                            col = 0;
                            row++;
                            rowCount++;

                            worksheet.Cell(row, 1).Value = stt;
                            stt++;

                            worksheet.Cell(row, 1).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            worksheet.Cell(row, 2).Value = "'" + obj.S_ID_REPORT;
                            worksheet.Cell(row, 3).Value = obj.S_DELIVERY_DATE;

                            worksheet.Cell(row, 4).Value = "'" + obj.S_EDONG_ACCOUNT;
                            worksheet.Cell(row, 5).Value = obj.S_SO_HDON;
                            worksheet.Cell(row, 6).Value = obj.S_CUSTOMER_CODE;
                            worksheet.Cell(row, 7).Value = obj.S_CUSTOMER_NAME;
                            worksheet.Cell(row, 8).Value = "'" + obj.S_SERI_ID;
                            worksheet.Cell(row, 9).Value = obj.S_GCS_CODE;
                            worksheet.Cell(row, 10).Value = "'" + obj.S_AREA;
                            worksheet.Cell(row, 11).Value = "'" + obj.S_PERIOD_YEAR;
                            worksheet.Cell(row, 12).Value = obj.S_TYPE;
                            worksheet.Cell(row, 13).Value = obj.N_AMOUNT_SUM;
                            worksheet.Cell(row, 13).Style.NumberFormat.Format = "#,##0";
                        }
                    }
                }


                string strTongSoHD = common == null ? "Tổng số hóa đơn: "
                   : "Tổng số hóa đơn: " + listDelivery.Count;
                col = 13;
                row++;

                worksheet.Cell(row, col - 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col - 3).Style.Font.Bold = true;
                worksheet.Cell(row, col - 3).Value = strTongSoHD;

                decimal sumAcount = 0;
                if (listDelivery != null)
                {
                    sumAcount = listDelivery.Sum(p => p.N_AMOUNT_SUM);
                }

                string strTongTienHD = common == null ? "Tổng tiền: "
                   : "Tổng tiền: " + sumAcount.ToString("#,##0");

                worksheet.Range(string.Format("L{0}:M{1}", row, row)).Merge();
                worksheet.Cell(row, col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col - 1).Style.Font.Bold = true;
                worksheet.Cell(row, col - 1).Value = strTongTienHD;
                worksheet.Column(col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                IXLCells c1 = worksheet.Range(string.Format("A{0}:M{1}", rowData, rowCount)).Cells();
                c1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                c1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                //Định dạng lại kích thước các cột
                worksheet.Column(1).Width = 8;
                worksheet.Column(1).Style.Alignment.WrapText = true;
                worksheet.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Column(2).Width = 12;
                worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Column(3).Width = 15;
                worksheet.Column(3).Style.Alignment.WrapText = true;
                worksheet.Column(4).Width = 10;
                worksheet.Column(4).Style.Alignment.WrapText = true;

                worksheet.Column(5).Width = 10;
                worksheet.Column(5).Style.Alignment.WrapText = true;

                worksheet.Column(6).Width = 17;
                worksheet.Column(6).Style.Alignment.WrapText = true;

                worksheet.Column(7).Width = 45;
                worksheet.Column(7).Style.Alignment.WrapText = true;

                worksheet.Column(8).Width = 9;
                worksheet.Column(8).Style.Alignment.WrapText = true;
                worksheet.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Column(9).Width = 11;
                worksheet.Column(9).Style.Alignment.WrapText = true;

                worksheet.Column(10).Width = 24;
                worksheet.Column(10).Style.Alignment.WrapText = true;

                worksheet.Column(11).Width = 16;
                worksheet.Column(11).Style.Alignment.WrapText = true;

                worksheet.Column(8).Width = 10;
                worksheet.Column(12).Style.Alignment.WrapText = true;
                worksheet.Column(12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Column(13).Width = 12;
                worksheet.Column(13).Style.Alignment.WrapText = true;
            }
            return workbook;
        }

        public static XLWorkbook SumDelivery(List<ObjDeliverySummuryReport> listDelivery, ObjReportDeliverySummuryCommon common, ePosAccount posAccount)
        {
            int row = 1, col = 1;
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BCTongHopDuLieuGiaoThu");
            worksheet.Style.Font.SetFontName("Times New Roman");
            worksheet.Style.Font.SetFontSize(12);

            worksheet.Range(string.Format("A{0}:M{1}", row, row)).Merge();
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Value = "BÁO CÁO TỔNG HỢP DỮ LIỆU GIAO THU";
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Font.FontSize = 16;

            row++;
            row++;
            string strDvi = common == null ? "Mã Điện lực: " : "Mã Điện lực: " + common.PCCode;
            worksheet.Range(string.Format("A3:C3")).Merge();
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Value = strDvi;


            string strDate = common == null ? "Từ ngày : " + "Đến ngày: "
                : "Từ ngày : " + common.FromDate + "      Đến ngày: " + common.ToDate;
            worksheet.Range(string.Format("G3:M3")).Merge();
            worksheet.Cell(row, col + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col + 6).Value = strDate;
            worksheet.Cell(row, col + 6).Style.Font.Italic = true;

            string strTNV = "Mã TNV: ";
            if (common != null)
            {
                strTNV = common.Account == "ALL" ? "" : ("Mã TNV:" + common.Account);
            }

            row++;
            worksheet.Range(string.Format("A4:C4")).Merge();
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col).Value = strTNV;


            row++;

            string strTongSoHD = common == null ? "Tổng số hóa đơn: "
               : "Tổng số hóa đơn : " + common.BillCount.ToString("#,##0");

            worksheet.Range(string.Format("A5:B5")).Merge();
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Value = strTongSoHD;

            string strTongTienHD = common == null ? "Tổng tiền: "
                    : "Tổng tiền : " + common.BillAmount.ToString("#,##0");

            worksheet.Range(string.Format("C5:E5")).Merge();
            worksheet.Cell(row, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col + 2).Style.Font.Bold = true;
            worksheet.Cell(row, col + 2).Value = strTongTienHD;


            string strTienHC = common == null ? "Tiền HC: "
               : "Tiền HC: " + common.AmountHC.ToString("#,##0");
            row++;
            worksheet.Range(string.Format("A6:B6")).Merge();
            worksheet.Cell(row, col).Style.Font.Italic = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col).Value = strTienHC;

            string strThueHC = common == null ? "Thuế HC: "
                : "Thuế HC : " + common.VATHC.ToString("#,##0");

            worksheet.Range(string.Format("C6:E6")).Merge();
            worksheet.Cell(row, col + 2).Style.Font.Italic = true;
            worksheet.Cell(row, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col + 2).Value = strThueHC;

            string strTienVC = common == null ? "Tiền VC: "
           : "Tiền VC: " + common.AmountVC.ToString("#,##0");
            row++;
            worksheet.Range(string.Format("A7:B7", row, row)).Merge();
            worksheet.Cell(row, col).Style.Font.Italic = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col).Value = strTienVC;

            string strThueVC = common == null ? "Thuế VC: "
                : "Thuế VC : " + common.VATVC.ToString("#,##0");

            worksheet.Range(string.Format("C7:E7")).Merge();
            worksheet.Cell(row, col + 2).Style.Font.Italic = true;
            worksheet.Cell(row, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col + 2).Value = strThueVC;


            row++;
            row++;

            worksheet.Range(string.Format("A9:A10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.WrapText = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số biên bản";
            worksheet.Range(string.Format("B9:B10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.WrapText = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Ngày giao ";

            worksheet.Range(string.Format("C9:C10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Mã TNV";

            worksheet.Range(string.Format("D9:D10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Họ và tên thu ngân";

            worksheet.Range(string.Format("E9:E10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Sổ GCS";

            worksheet.Range(string.Format("F9:F10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Tháng-năm";

            worksheet.Range(string.Format("G9:G10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Kỳ";

            worksheet.Range(string.Format("H9:H10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số khách hàng"; //"Số trang bảng kê"


            worksheet.Range(string.Format("I9:J9")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row++, col).Value = "Số hóa đơn"; //"Số trang bảng kê"


            worksheet.Range(string.Format("I10:I10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "HC";

            worksheet.Range(string.Format("J10:J10")).Merge();
            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "VC";

            worksheet.Range(string.Format("K9:K10")).Merge();
            worksheet.Cell(row - 1, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row - 1, col).Style.Font.Bold = true;
            worksheet.Cell(row - 1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row - 1, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row - 1, col++).Value = "Tiền HC";

            worksheet.Range(string.Format("L9:L10")).Merge();
            worksheet.Cell(row - 1, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row - 1, col).Style.Font.Bold = true;
            worksheet.Cell(row - 1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row - 1, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row - 1, col++).Value = "Thuế HC";

            worksheet.Range(string.Format("M9:M10")).Merge();
            worksheet.Cell(row - 1, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row - 1, col).Style.Font.Bold = true;
            worksheet.Cell(row - 1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row - 1, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row - 1, col++).Value = "Tiền VC";

            worksheet.Range(string.Format("N9:N10")).Merge();
            worksheet.Cell(row - 1, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row - 1, col).Style.Font.Bold = true;
            worksheet.Cell(row - 1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row - 1, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row - 1, col++).Value = "Thuế VC";

            worksheet.Range(string.Format("O9:O10")).Merge();
            worksheet.Cell(row - 1, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row - 1, col).Style.Font.Bold = true;
            worksheet.Cell(row - 1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row - 1, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row - 1, col++).Value = "Tổng tiền";


            IXLCells c2 = worksheet.Range(string.Format("A9:O10")).Cells();
            c2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            c2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int rowCount = row;
            if (listDelivery != null && listDelivery.Count > 0)
            {
                Dictionary<int, int> dEdongCount = new Dictionary<int, int>();
                var lstEdong = listDelivery.Select(p => p.S_ID_REPORT).Distinct().ToList();
                foreach (var edong in lstEdong)
                {
                    int edongCount = listDelivery.Where(p => p.S_ID_REPORT == edong).Count();
                    dEdongCount.Add(edong, edongCount);
                }

                Dictionary<string, int> dicTNVCount = new Dictionary<string, int>();
                var lstTenTNV = listDelivery.Select(p => p.S_MA_TNGAN).Distinct().ToList();
                foreach (var tnv in lstTenTNV)
                {
                    int tnvCount = listDelivery.Where(p => p.S_MA_TNGAN == tnv).Count();
                    dicTNVCount.Add(tnv, tnvCount);
                }

                if (listDelivery != null)
                {
                    int i = 1;
                    int sobb = 0;
                    List<ObjDeliverySummuryReport> ltemp = new List<ObjDeliverySummuryReport>();
                    foreach (ObjDeliverySummuryReport obj in listDelivery.OrderBy(x => x.S_ID_REPORT))
                    {
                        row++;
                        rowCount = row;

                        if (dEdongCount[obj.S_ID_REPORT] == i)
                        {
                            worksheet.Range(string.Format("A{0}:A{1}", row - i + 1, row)).Merge();
                            worksheet.Range(string.Format("B{0}:B{1}", row - i + 1, row)).Merge();
                            i = 1;
                        }
                        else
                        {
                            i++;
                        }

                        if (sobb != obj.S_ID_REPORT)
                        {
                            sobb = obj.S_ID_REPORT;
                            worksheet.Cell(row, 1).Value = "'" + obj.S_ID_REPORT;
                            worksheet.Cell(rowCount, 1).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(rowCount, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            worksheet.Cell(rowCount, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(rowCount, 2).Value = "'" + obj.S_DELIVERY_DATE;
                            worksheet.Cell(rowCount, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            worksheet.Cell(rowCount, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            ltemp = listDelivery.Where(x => x.S_ID_REPORT == sobb).ToList();
                            string strEdong = "";
                            int j = 1;
                            foreach (ObjDeliverySummuryReport item in ltemp.OrderBy(x => x.S_TEN_TNGAN))
                            {
                                List<ObjDeliverySummuryReport> ltempChild = new List<ObjDeliverySummuryReport>();

                                worksheet.Cell(rowCount, 3).Value = item.S_MA_TNGAN;
                                worksheet.Cell(rowCount, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                worksheet.Cell(rowCount, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(rowCount, 4).Value = "'" + item.S_TEN_TNGAN;
                                worksheet.Cell(rowCount, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                worksheet.Cell(rowCount, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                if (dicTNVCount[item.S_MA_TNGAN] == j)
                                {
                                    worksheet.Range(string.Format("C{0}:C{1}", rowCount - j + 1, rowCount)).Merge();
                                    worksheet.Range(string.Format("D{0}:D{1}", rowCount - j + 1, rowCount)).Merge();
                                    j = 1;
                                }
                                else
                                {
                                    j++;
                                }

                                if (strEdong != item.S_TEN_TNGAN)
                                {
                                    strEdong = item.S_TEN_TNGAN;

                                    ltempChild = ltemp.Where(x => x.S_TEN_TNGAN == strEdong).ToList();

                                    int rowchild = rowCount;

                                    foreach (var itemChild in ltempChild)
                                    {
                                        worksheet.Cell(rowchild, 5).Value = itemChild.S_GCS_CODE;
                                        worksheet.Cell(rowchild, 6).Value = "'" + itemChild.S_MONTH_YEAR;
                                        worksheet.Cell(rowchild, 7).Value = itemChild.N_PERIOD;
                                        worksheet.Cell(rowchild, 8).Value = itemChild.N_PAGE_REPORT;
                                        worksheet.Cell(rowchild, 8).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 9).Value = itemChild.N_HC_BILL_SUM;
                                        worksheet.Cell(rowchild, 9).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 10).Value = itemChild.N_VC_BILL_SUM;
                                        worksheet.Cell(rowchild, 10).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 11).Value = itemChild.N_HC_BILL_AMOUNT;
                                        worksheet.Cell(rowchild, 11).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 12).Value = itemChild.N_HC_BILL_VAT;
                                        worksheet.Cell(rowchild, 12).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 13).Value = itemChild.N_VC_BILL_AMOUNT;
                                        worksheet.Cell(rowchild, 13).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 14).Value = itemChild.N_VC_BILL_VAT;
                                        worksheet.Cell(rowchild, 14).Style.NumberFormat.Format = "#,##0";
                                        worksheet.Cell(rowchild, 15).Value = itemChild.N_AMOUNT_SUM;
                                        worksheet.Cell(rowchild, 15).Style.NumberFormat.Format = "#,##0";
                                        rowchild++;
                                    }
                                }
                                rowCount++;
                            }
                        }
                    }
                }
            }

            IXLCells c1 = worksheet.Range(string.Format("A9:O{0}", rowCount)).Cells();
            c1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            c1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //Định dạng lại kích thước các cột
            worksheet.Column(1).Width = 12;
            worksheet.Column(1).Style.Alignment.WrapText = true;
            worksheet.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Column(2).Width = 18;
            worksheet.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Column(3).Width = 15;
            worksheet.Column(3).Style.Alignment.WrapText = true;
            worksheet.Column(4).Width = 22;
            worksheet.Column(4).Style.Alignment.WrapText = true;

            worksheet.Column(5).Width = 18;
            worksheet.Column(5).Style.Alignment.WrapText = true;

            worksheet.Column(6).Width = 15;
            worksheet.Column(6).Style.Alignment.WrapText = true;

            worksheet.Column(7).Width = 8;
            worksheet.Column(7).Style.Alignment.WrapText = true;

            worksheet.Column(8).Width = 22;
            worksheet.Column(8).Style.Alignment.WrapText = true;

            worksheet.Column(9).Width = 25;
            worksheet.Column(9).Style.Alignment.WrapText = true;

            worksheet.Column(10).Width = 25;
            worksheet.Column(10).Style.Alignment.WrapText = true;

            worksheet.Column(11).Width = 25;
            worksheet.Column(11).Style.Alignment.WrapText = true;

            worksheet.Column(12).Width = 18;
            worksheet.Column(12).Style.Alignment.WrapText = true;

            worksheet.Column(13).Width = 18;
            worksheet.Column(13).Style.Alignment.WrapText = true;

            worksheet.Column(14).Width = 18;
            worksheet.Column(14).Style.Alignment.WrapText = true;

            worksheet.Column(15).Width = 18;
            worksheet.Column(15).Style.Alignment.WrapText = true;

            worksheet.Column(16).Width = 18;
            worksheet.Column(16).Style.Alignment.WrapText = true;


            worksheet.Column(17).Width = 18;
            worksheet.Column(17).Style.Alignment.WrapText = true;


            worksheet.Column(18).Width = 22;
            worksheet.Column(18).Style.Alignment.WrapText = true;
            return workbook;
        }

        public static ExcelPackage SumDelivery(List<ObjDeliverySummuryReport> listDelivery, ObjReportDeliverySummuryCommon common, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Sum_SPCCollection"];

            ws.Cells[2, 1].Value = "Số ví: " + posAccount.edong;
            ws.Cells[3, 1].Value = "Họ và tên: " + posAccount.name;
            ws.Cells[4, 1].Value = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");

            return epk;
        }


        public static XLWorkbook SumDebtReliefSPC(List<ObjDebtReliefSummuryReport> reports, ePosAccount posAccount)
        {
            string strDvi = string.Empty;
            string strMaTNV = string.Empty;
            string strFromDate = string.Empty;
            string strToDate = string.Empty;
            if (reports != null && reports.Count > 0)
            {
                ObjDebtReliefSummuryReport objFirst = reports.FirstOrDefault();
                if (objFirst != null)
                {
                    strDvi = objFirst.PC_CODE_COMMON;
                    strMaTNV = !string.IsNullOrEmpty(objFirst.TEN_TNGAN) ? objFirst.TEN_TNGAN.Split('-')[0] : string.Empty;
                    strFromDate = reports.Min(p => p.D_NGAY_GIAO).ToString("dd/MM/yyyy");
                    strToDate = reports.Max(p => p.D_NGAY_GIAO).ToString("dd/MM/yyyy");
                }
            }
            int row = 1, col = 1;
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BCTongHopGachNo");
            worksheet.Style.Font.SetFontName("Times New Roman");
            worksheet.Style.Font.SetFontSize(12);

            worksheet.Range(string.Format("A{0}:O{1}", row, row)).Merge();
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Value = "BÁO CÁO TỔNG HỢP GẠCH NỢ";
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Font.FontSize = 16;

            row++;
            //worksheet.Range(string.Format("A2:B2")).Merge();
            //worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Value = "Mã Điện lực:";
            worksheet.Cell(row, col + 1).Style.Font.Bold = true;
            worksheet.Cell(row, col + 1).Value = strDvi;

            worksheet.Cell(row, col + 5).Style.Font.Italic = true;
            worksheet.Cell(row, col + 5).Value = "Từ ngày:";
            worksheet.Cell(row, col + 6).Style.Font.Italic = true;
            worksheet.Cell(row, col + 6).Value = strFromDate;

            worksheet.Cell(row, col + 7).Style.Font.Italic = true;
            worksheet.Cell(row, col + 7).Value = "Đến ngày:";
            worksheet.Cell(row, col + 8).Style.Font.Italic = true;
            worksheet.Cell(row, col + 8).Value = strToDate;

            row++;
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Value = "Mã TNV:";
            worksheet.Cell(row, col + 1).Style.Font.Bold = true;
            worksheet.Cell(row, col + 1).Value = strMaTNV;

            row++;
            worksheet.Range(string.Format("A{0}:A{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Đơn vị";

            worksheet.Range(string.Format("B{0}:B{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.WrapText = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số biên bản giao";

            worksheet.Range(string.Format("C{0}:C{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.WrapText = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Ngày giao";

            worksheet.Range(string.Format("D{0}:D{1}", row, row + 1)).Merge();
            // worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Mã TNV";

            worksheet.Range(string.Format("E{0}:E{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Họ và tên TNV";

            worksheet.Range(string.Format("F{0}:F{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số hóa đơn giao";

            worksheet.Range(string.Format("G{0}:G{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số tiền giao";

            worksheet.Range(string.Format("H{0}:H{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Thuế GTGT giao";

            worksheet.Range(string.Format("I{0}:I{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số HĐ thu";

            worksheet.Range(string.Format("J{0}:J{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số tiền thu";

            worksheet.Range(string.Format("K{0}:K{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Thuế GTGT thu";

            worksheet.Range(string.Format("L{0}:L{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số HĐ tồn";

            worksheet.Range(string.Format("M{0}:M{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Số tiền tồn";

            worksheet.Range(string.Format("N{0}:N{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Thuế GTGT tồn";

            worksheet.Range(string.Format("O{0}:O{1}", row, row + 1)).Merge();
            //worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xD8D8D8);
            worksheet.Cell(row, col).Style.Font.Bold = true;
            worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(row, col++).Value = "Tổng tiền tồn";

            int rowCount = row;
            row++;
            if (reports != null && reports.Count > 0)
            {
                //Nhóm theo số ví
                Dictionary<string, int> dEdongCount = new Dictionary<string, int>();
                //var lstEdong = listDelivery.DistinctBy(p => p.S_EDONG_ACCOUNT).ToList();
                //foreach (ObjBillStatusDetailReport edong in lstEdong)
                //{
                //    int edongCount = listJob.Where(p => p.S_EDONG_ACCOUNT == edong.S_EDONG_ACCOUNT).Count();
                //    dEdongCount.Add(edong.S_EDONG_ACCOUNT, edongCount);
                //}

                if (reports != null)
                {
                    int i = 1;
                    foreach (ObjDebtReliefSummuryReport obj in reports)
                    {
                        col = 1;
                        row++;
                        rowCount++;

                        //if (dEdongCount[obj.S_EDONG_ACCOUNT] == i)
                        //{
                        //    worksheet.Range(string.Format("A{0}:A{1}", row - i + 1, row)).Merge();
                        //    worksheet.Cell(row, 1).Value = "'" + obj.S_EDONG_ACCOUNT;
                        //    i = 1;
                        //}
                        //else
                        //{
                        //    i++;
                        //    worksheet.Cell(row, 1).Value = "'" + obj.S_EDONG_ACCOUNT;
                        //}

                        worksheet.Cell(row, col++).Value = obj.MA_DVIQLY;

                        worksheet.Cell(row, col++).Value = "'" + obj.SO_BBGIAO;
                        worksheet.Cell(row, col++).Value = "'" + obj.NGAY_GIAO;
                        worksheet.Cell(row, col++).Value = obj.TEN_TNGAN.Split('-')[0];
                        worksheet.Cell(row, col).Value = obj.TEN_TNGAN.Split('-')[1];

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_HDON_GIAO;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_TIEN_GIAO;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_THUE_GIAO;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_HDON_THU_DUOC;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_TIEN_THU_DUOC;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_GTGT_THU_DUOC;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_HDON_TON;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_TIEN_TON;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_GTGT_TON;

                        worksheet.Cell(row, col++).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Value = obj.SO_TIEN_TON_TONG;

                        // worksheet.Cell(row, col++).Value = obj.TRANG_THAI;

                    }
                }
            }

            //IXLCells c1 = worksheet.Range(string.Format("A4:P{0}", row)).Cells();
            IXLCells c1 = worksheet.Range(string.Format("A4:O{0}", row)).Cells();
            //c1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            //c1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            c1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            c1.Style.Border.OutsideBorderColor = XLColor.Black;

            //Định dạng lại kích thước các cột
            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 16;
            worksheet.Column(3).Width = 10;
            worksheet.Column(4).Width = 10;
            worksheet.Column(5).Width = 25;
            worksheet.Column(5).Style.Alignment.WrapText = true;

            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 12;
            worksheet.Column(8).Width = 17;
            worksheet.Column(9).Width = 15;
            worksheet.Column(10).Width = 15;
            worksheet.Column(11).Width = 15;
            worksheet.Column(12).Width = 15;
            worksheet.Column(13).Width = 15;
            worksheet.Column(14).Width = 15;
            worksheet.Column(15).Width = 15;
            return workbook;
        }

        public static ExcelPackage DetailDebtReliefSPC(List<ObjDebtReliefDetailReport> listDebtRelief, string template, ePosAccount posAccount)
        {
            string strDvi = string.Empty;
            string strMaTNV = string.Empty;
            string strFromDate = string.Empty;
            string strToDate = string.Empty;
            string strMaNhanVien = string.Empty;
            int TongHDGiao = 0;
            decimal TongTienGiao = 0;
            decimal TongTienCham = 0;

            if (listDebtRelief != null && listDebtRelief.Count > 0)
            {
                ObjDebtReliefDetailReport objFirst = listDebtRelief.FirstOrDefault();
                if (objFirst != null)
                {
                    strDvi = objFirst.MA_DON_VI;
                    strMaTNV = objFirst.MA_TNGAN;
                    strFromDate = listDebtRelief.Min(p => p.D_NGAY_GIAO).ToString("dd/MM/yyyy");
                    strToDate = listDebtRelief.Max(p => p.D_NGAY_GIAO).ToString("dd/MM/yyyy");
                    strMaNhanVien = objFirst.MA_NVIEN_GIAO;
                }

                TongHDGiao = listDebtRelief.Select(p => p.MA_TNGAN == strMaTNV).Count();
                TongTienGiao = listDebtRelief.Where(p => p.MA_TNGAN == strMaTNV).ToList().Sum(p => p.SO_TIEN_GIAO_VAT);
                TongTienCham = listDebtRelief.Where(p => p.MA_TNGAN == strMaTNV).ToList().Sum(p => p.SO_TIEN_CHAM_VAT);
            }

            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Detail_DebtReliefSPC"];
            ws.Cells[3, 1].Style.Font.Bold = true;
            ws.Cells[3, 1].Value = "Mã Điện lực: " + strDvi;
            ws.Cells[3, 1].Style.Font.Italic = true;
            ws.Cells[4, 1].Value = "Từ ngày: " + strFromDate + " đến ngày: " + strToDate;
            ws.Cells[5, 1].Style.Font.Bold = true;
            ws.Cells[5, 1].Value = "Mã TNV: " + strMaTNV;
            ws.Cells[6, 1].Style.Font.Bold = true;
            ws.Cells[6, 1].Value = "Mã TNV: " + strMaNhanVien;

            int rowIndex = 9;
            if (listDebtRelief != null && listDebtRelief.Count > 0)
            {
                int i = 1;
                foreach (ObjDebtReliefDetailReport obj in listDebtRelief)
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 1].Value = i++;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 2].Value = obj.SO_BBGIAO;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 3].Value = obj.MA_NVIEN_GIAO;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 4].Value = obj.TEN_NVIEN_GIAO;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 5].Value = obj.MA_KHANG;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 6].Value = obj.TEN_KHANG;
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 7].Value = obj.DIA_CHI;
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 8].Value = obj.DANH_SO;
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 9].Value = obj.MA_SOGCS;
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Value = obj.ID_HDON;
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = obj.SO_TIEN_GIAO_VAT;
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 12].Value = obj.SO_TIEN_CHAM_VAT;
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 13].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 13].Value = obj.TRANG_THAI;
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[9, 1, rowIndex - 1, 13])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }

            return epk;
        }

        public static ExcelPackage CardIdentifier(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["CardIdentifier"];
            ws.Cells[3, 1].Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
            ws.Cells[4, 1].Value = "Ngày tháng: " + DateTime.Now.ToString("dd/MM/yyyy");

            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 8;
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                for (int i = 0; i < items.Count; i++)
                {

                    ws.Cells[rowIndex, 1].Value = i + 1;
                    ws.Cells[rowIndex, 2].Value = items.ElementAt(i).col_1;
                    ws.Cells[rowIndex, 3].Value = items.ElementAt(i).col_2;
                    ws.Cells[rowIndex, 4].Value = items.ElementAt(i).col_3;
                    ws.Cells[rowIndex, 5].Value = items.ElementAt(i).col_4;
                    ws.Cells[rowIndex, 6].Value = items.ElementAt(i).col_5;
                    ws.Cells[rowIndex, 7].Value = items.ElementAt(i).col_6;
                    ws.Cells[rowIndex, 8].Value = items.ElementAt(i).col_7;
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[8, 1, rowIndex - 1, 8])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }

            return epk;
        }

        public static ExcelPackage InStock(List<Hashtable> tables, string account, string createDate, string template)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["InStock"];
            ws.Cells[3, 1].Value = "Số ví yêu cầu check tồn: " + account;
            ws.Cells[4, 1].Value = "Thời điểm check ngày: " + createDate;

            if (tables.Count() > 0)
            {
                int rowIndex = 7;
                int index = 1;
                foreach (var item in tables)
                {
                    ws.Cells[rowIndex, 1].Value = index;
                    ws.Cells[rowIndex, 2].Value = item["customer"];
                    ws.Cells[rowIndex, 3].Value = item["GSC"];
                    ws.Cells[rowIndex, 4].Value = item["DVQL"];
                    index++;
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[7, 1, rowIndex - 1, 4])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }

            return epk;
        }

        public static ExcelPackage OutStock(List<Hashtable> table, string account, string createDate, string template)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["OutStock"];
            ws.Cells[3, 1].Value = "Số ví yêu cầu check tồn: " + account;
            ws.Cells[4, 1].Value = "Thời điểm check ngày: " + createDate;

            if (table.Count() > 0)
            {
                int rowIndex = 8;
                int index = 1;
                foreach (var item in table)
                {
                    ws.Cells[rowIndex, 1].Value = index;
                    ws.Cells[rowIndex, 2].Value = item["customer"];
                    ws.Cells[rowIndex, 3].Value = item["name"];
                    ws.Cells[rowIndex, 4].Value = item["address"];
                    ws.Cells[rowIndex, 5].Value = item["bookCMIS"];
                    ws.Cells[rowIndex, 6].Value = item["ElectricityMeter"];
                    ws.Cells[rowIndex, 7].Value = item["pc"];
                    ws.Cells[rowIndex, 8].Value = item["phoneByevn"];
                    ws.Cells[rowIndex, 9].Value = item["phoneByecp"];
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Value = item["Term"];
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item["Amount"].ToString()) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item["Amount"].ToString(), false));
                    ws.Cells[rowIndex, 12].Value = item["Description"];
                    index++;
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[8, 1, rowIndex - 1, 12])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        //public static XLWorkbook BillHandling(string id, ePosAccount posAccount)
        //{
        //    var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("HĐ đang xử lý chấm nợ");
        //    int row = 1, col = 1;
        //    worksheet.Style.Font.SetFontName("Times New Roman");
        //    worksheet.Range(string.Format("A{0}:L{1}", row, row)).Merge();
        //    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    worksheet.Cell(row, col).Value = "HÓA ĐƠN ĐANG XỬ LÝ CHẤM NỢ";
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col).Style.Font.FontSize = 14;
        //    row++;
        //    row++;
        //    worksheet.Range(string.Format("A{0}:L{1}", row, row)).Merge();
        //    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //    worksheet.Cell(row, col).Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col).Style.Font.FontSize = 10;
        //    row++;

        //    worksheet.Range(string.Format("A{0}:L{1}", row, row)).Merge();
        //    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //    worksheet.Cell(row, col).Value = "Ngày tháng: " + DateTime.Now.ToString("dd/MM/yyyy");
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col).Style.Font.FontSize = 10;
        //    row++;
        //    row++;

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Mã GD Offline";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Ví TNV";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Ngày thu";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Mã khách hàng";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Mã HĐ";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Kỳ";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Số tiền";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Loại";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Trạng thái";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Lịch chấm";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Mã lỗi";

        //    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0x6495ED);
        //    worksheet.Cell(row, col).Style.Font.Bold = true;
        //    worksheet.Cell(row, col++).Value = "Lý do";

        //    if (ePOSSession.GetObject(id) != null)
        //    {
        //        List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
        //        row++;
        //        foreach (var item in items)
        //        {
        //            worksheet.Cell(row, 1).Style.NumberFormat.Format = "@"; //Mã GD Offline
        //            worksheet.Cell(row, 1).Value = item.col_8;
        //            worksheet.Cell(row, 2).Style.NumberFormat.Format = "@"; //Vi TNV
        //            worksheet.Cell(row, 2).Value = item.col_1;
        //            worksheet.Cell(row, 3).Style.NumberFormat.Format = "@"; //Ngay thu
        //            worksheet.Cell(row, 3).Value = item.col_2;
        //            worksheet.Cell(row, 4).Value = item.col_3; // Ma khach hang                    
        //            worksheet.Cell(row, 5).Style.NumberFormat.Format = "@"; //Mã HĐ
        //            worksheet.Cell(row, 5).Value = item.col_15;
        //            worksheet.Cell(row, 6).Style.NumberFormat.Format = "@"; // Ky
        //            worksheet.Cell(row, 6).Value = item.col_4;
        //            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0"; // So tien
        //            worksheet.Cell(row, 7).Value = item.col_5;
        //            worksheet.Cell(row, 8).Value = item.col_6;//loai
        //            worksheet.Cell(row, 9).Value = item.col_11; // Trang thai
        //            worksheet.Cell(row, 10).Value = item.col_7; // lich châm  
        //            worksheet.Cell(row, 11).Style.NumberFormat.Format = "@";
        //            worksheet.Cell(row, 11).Value = item.col_16; // Mã lỗi
        //            worksheet.Cell(row, 12).Value = item.col_13;   // Lý do                 
        //            row++;
        //        }
        //    }
        //    IXLCells c2 = worksheet.Range(string.Format("A{0}:L{1}", 5, row - 1)).Cells();
        //    c2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //    c2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //    worksheet.Column(1).Width = 10;
        //    worksheet.Column(2).Width = 15;
        //    worksheet.Column(3).Width = 25;
        //    worksheet.Column(4).Width = 25;
        //    worksheet.Column(5).Width = 25;
        //    worksheet.Column(6).Width = 25;
        //    worksheet.Column(7).Width = 25;
        //    worksheet.Column(8).Width = 25;
        //    worksheet.Column(9).Width = 25;
        //    worksheet.Column(10).Width = 25;
        //    worksheet.Column(11).Width = 25;
        //    worksheet.Column(12).Width = 25;
        //    return workbook;
        //}

        public static ExcelPackage BillHandling(string id, string pc, string datefrom, string dateto, string template, ePosAccount posAccount)
        {
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["HD_DXLCN"];
            ws.Cells[2, 1].Value = "Từ ngày: " + datefrom + " đến ngày: " + dateto;
            if (string.IsNullOrEmpty(pc))
                ws.Cells[3, 1].Value = "Điện lực:";
            else
            {
                try
                {
                    ws.Cells[2, 1].Value = "Điện lực: " + (from x in tempPosAcc.EvnPC where x.ext == pc select x).FirstOrDefault().fullName.ToUpper();
                }
                catch (Exception ex)
                {
                    Logging.ReportLogger.ErrorFormat("BillHandling => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                    ws.Cells[2, 1].Value = "Điện lực:";
                }
            }
            ws.Cells[5, 1].Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
            ws.Cells[6, 1].Value = "Ngày tháng: " + DateTime.Now.ToString("dd/MM/yyyy");
            if (ePOSSession.GetObject(id) != null)
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                int rowIndex = 9;
                foreach (var item in items)
                {
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Value = item.col_8;  //Mã GD Offline
                    ws.Cells[rowIndex, 2].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_1;  //Vi TNV
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Value = item.col_2; //Ngay thu
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_3; // Ma khach hang     
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Value = item.col_15; //Mã HĐ
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 6].Value = item.col_4; // Ky
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false)); // So tien
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 8].Value = item.col_21; //Ngay GCS
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 9].Value = item.col_6; //loai
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 10].Value = item.col_11; // Trang thai
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 11].Value = item.col_16; // lich châm 
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 12].Value = item.col_7; //Ma loi                     
                    ws.Cells[rowIndex, 13].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 13].Value = item.col_13;   // Lý do  
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[9, 1, rowIndex - 1, 13])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage Reconciliation(string id, string pc, string template, ePosAccount posAccount)
        {
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["HĐ đang xử lý chấm nợ"];
            if (string.IsNullOrEmpty(pc))
            {
                ws.Cells[2, 1].Value = "";
            }
            else
            {
                try
                {
                    ws.Cells[2, 1].Value = (from x in tempPosAcc.EvnPC where x.ext == pc select x).FirstOrDefault().fullName.ToUpper();
                }
                catch (Exception ex)
                {
                    Logging.ReportLogger.ErrorFormat("ExportReconciliation => User: {0}, Error: {1}, Session: {2}", posAccount.edong, ex.Message, posAccount.session);
                    ws.Cells[2, 1].Value = "";
                }
            }
            ws.Cells[3, 1].Value = "Ngày tháng: " + DateTime.Now.ToString("dd/MM/yyyy");

            if (ePOSSession.GetObject(id) != null)
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                int i = 1;
                int rowIndex = 8;
                long total = 0;
                foreach (var item in items)
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Value = i;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_3;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Value = item.col_17;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_18;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Value = item.col_5;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 6].Value = item.col_2;
                    total = total + long.Parse(item.col_14);
                    rowIndex++;
                    i++;
                }
                ws.Cells[5, 3].Value = total.ToString("N0");
                using (ExcelRange cell = ws.Cells[8, 1, rowIndex - 1, 6])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage TotalBillHandling(string id, string total, string amount, string fromdate, string todate, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["BillHandling"];
            ws.Cells[3, 1].Value = "(Từ ngày: " + fromdate + " đến ngày: " + todate + ")";
            ws.Cells[4, 1].Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
            ws.Cells[5, 1].Value = "Ngày tháng: " + DateTime.Now.ToString("dd/MM/yyyy");
            ws.Cells[6, 1].Value = "Tổng số HĐ đang chờ xử lý chấm nợ: " + total;
            ws.Cells[6, 3].Value = "Tổng tiền: " + amount;
            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 9;
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                foreach (var item in Constant.BillHandling())
                {
                    int i = 1;
                    var temp_parent = (from x in items where x.col_1 == item.Key select x).FirstOrDefault();
                    ws.Cells[rowIndex, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x94AAD6));
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Value = "";
                    ws.Cells[rowIndex, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x94AAD6));
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = temp_parent.col_2;
                    ws.Cells[rowIndex, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x94AAD6));
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(temp_parent.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(temp_parent.col_3, false));
                    ws.Cells[rowIndex, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x94AAD6));
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(temp_parent.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(temp_parent.col_4, false));
                    rowIndex++;
                    foreach (var temp_child in (from x in items where x.col_5 == item.Key select x))
                    {
                        ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[rowIndex, 1].Value = i;
                        ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[rowIndex, 2].Value = temp_child.col_2;
                        ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                        ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(temp_child.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(temp_child.col_3, false));
                        ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                        ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(temp_child.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(temp_child.col_4, false));
                        i++;
                        rowIndex++;
                    }
                }
                using (ExcelRange cell = ws.Cells[9, 1, rowIndex - 1, 4])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage Bill(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Bill"];
            ws.Cells[3, 1].Value = "Số ví yêu cầu báo cáo: " + posAccount.edong;
            ws.Cells[4, 1].Value = "Ngày tháng: " + DateTime.Now.ToString("dd/MM/yyyy");
            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 7;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Value = item.col_3;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_4;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 5].Value = item.col_5;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 6].Value = item.col_6;
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 7].Value = item.col_7;
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 8].Value = item.col_8;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 9].Value = string.IsNullOrEmpty(item.col_9) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_9, false));
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(item.col_10) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_10, false));
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item.col_11) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_11, false));
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 12].Value = string.IsNullOrEmpty(item.col_12) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_12, false));
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[7, 1, rowIndex - 1, 12])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }
        public static ExcelPackage BillEVN(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["BillEVN"];
            ws.Cells[5, 1].Value = "Người yêu cầu : " + posAccount.edong;
            ws.Cells[6, 1].Value = "Thời gian: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 9;
                int i = 1;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    if (!string.IsNullOrEmpty(item.col_2))
                    {
                        ws.Cells[rowIndex, 1].Value = i;
                    }
                    else
                    {
                        ws.Cells[rowIndex, 1].Value = "";
                    }
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Value = item.col_11;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_3;
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 5].Value = item.col_4;
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 6].Value = item.col_5;
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 7].Value = item.col_6;
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 8].Value = item.col_8;
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 9].Value = item.col_9;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(item.col_10) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_10, false));
                    rowIndex++;
                    if (!string.IsNullOrEmpty(item.col_2))
                    {
                        i++;
                    }
                }
                using (ExcelRange cell = ws.Cells[9, 1, rowIndex - 1, 10])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage WaterDetail(string id, string template, string fromdate, string todate, string total_trans,
            string total_bill, string total_amount, string pc, string service, ePosAccount posAccount)
        {
            ePosAccount tempPosAcc = (ePosAccount)ePOSSession.GetObject(posAccount.session);
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["Sheet1"];
            ws.Cells[4, 1].Value = "Từ ngày: " + fromdate + " đến ngày: " + todate;
            ws.Cells[5, 1].Value = "Dịch vụ: " + service;
            ws.Cells[6, 1].Value = "Nhà cung cấp: " + pc;
            ws.Cells[8, 1].Value = "Ví yêu cầu xuất: " + posAccount.edong;
            ws.Cells[9, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");//"Thời gian: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            ws.Cells[11, 3].Value = total_trans;
            ws.Cells[11, 6].Value = total_amount;//total_bill
            ws.Cells[11, 9].Value = total_bill;//total_amount

            if (ePOSSession.GetObject(id) != null)
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                int i = 1;
                int rowIndex = 13;
                foreach (var item in items)
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Value = i; //STT
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2; // Ngày thanh toán
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Value = item.col_3; // Tài khoản ví
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_12; // Họ tên TNV
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false)); // Số tiền
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_5) == true ? 1 : decimal.Parse(Validate.ProcessReplace(item.col_5, false)); // Số lượng
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Value = item.col_6; // Dịch vụ
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 8].Value = item.col_7; // NCC
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 9].Value = item.col_8; // Mã khách hàng
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 10].Value = item.col_9; // Trạng thái
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item.col_13) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_13, false)); // Chiết khấu
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 12].Value = item.col_10; // Tên khách hàng
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 13].Style.WrapText = true;
                    ws.Cells[rowIndex, 13].Value = item.col_11; // Địa chỉ
                    rowIndex++;
                    i++;
                }
                using (ExcelRange cell = ws.Cells[12, 1, rowIndex - 1, 13])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage DebtBank(string id, string template, string fromdate, string todate, string bill, string amount, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);

            ExcelWorksheet ws = epk.Workbook.Worksheets["DSKH No"];
            ws.Cells[2, 1].Value = "Từ ngày: " + fromdate + " đến ngày: " + todate;
            ws.Cells[3, 2].Value = "Tổng số HĐ: " + bill;
            ws.Cells[3, 4].Value = "Tổng tiền: " + amount;
            if (ePOSSession.GetObject(id) != null)
            {
                List<ObjReport> items = (List<ObjReport>)ePOSSession.GetObject(id);
                int rowIndex = 5;
                foreach (var item in items)
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 3].Value = item.col_3;
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 4].Value = item.col_4;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 6].Value = item.col_6;
                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[4, 1, rowIndex - 1, 6])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage EdongSales(string id, string template, ePosAccount posAccount)
        {
            decimal sum_in = 0;
            decimal sum_return = 0;
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["BC Doanhthu"];
            ws.Cells[3, 4].Value = "Ngày: " + DateTime.Now.ToString("HH:mm") + "-" + DateTime.Now.ToString("dd/MM/yyyy");
            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 11;
                int i = 1;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {


                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_6) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_6, false));
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Value = string.IsNullOrEmpty(item.col_7) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_7, false));
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 8].Value = string.IsNullOrEmpty(item.col_8) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_8, false));
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 9].Value = string.IsNullOrEmpty(item.col_9) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_9, false));
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(item.col_10) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_10, false));
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item.col_11) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_11, false));
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 12].Value = string.IsNullOrEmpty(item.col_12) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_12, false));
                    ws.Cells[rowIndex, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 13].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 13].Value = string.IsNullOrEmpty(item.col_13) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_13, false));
                    ws.Cells[rowIndex, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 14].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 14].Value = string.IsNullOrEmpty(item.col_14) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_14, false));
                    ws.Cells[rowIndex, 15].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 15].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 15].Value = string.IsNullOrEmpty(item.col_15) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_15, false));
                    ws.Cells[rowIndex, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 16].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 16].Value = string.IsNullOrEmpty(item.col_16) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_16, false));
                    ws.Cells[rowIndex, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 17].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 17].Value = string.IsNullOrEmpty(item.col_17) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_17, false));
                    ws.Cells[rowIndex, 18].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 18].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 18].Value = string.IsNullOrEmpty(item.col_18) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_18, false));
                    ws.Cells[rowIndex, 19].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 19].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 19].Value = string.IsNullOrEmpty(item.col_19) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_19, false));
                    if (i < ((List<ObjReport>)ePOSSession.GetObject(id)).Count)
                    {
                        sum_in += decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                        sum_in += decimal.Parse(Validate.ProcessReplace(item.col_10, false)) + decimal.Parse(Validate.ProcessReplace(item.col_12, false));
                        sum_in += decimal.Parse(Validate.ProcessReplace(item.col_14, false)) + decimal.Parse(Validate.ProcessReplace(item.col_16, false));
                        sum_return += decimal.Parse(Validate.ProcessReplace(item.col_6, false)) + decimal.Parse(Validate.ProcessReplace(item.col_18, false));
                    }

                    rowIndex++;
                    i++;
                }
                //Tổng thu:
                ws.Cells[5, 3].Style.Numberformat.Format = "#,##0";
                ws.Cells[5, 3].Value = sum_in;
                //Tổng hoàn:
                ws.Cells[6, 3].Style.Numberformat.Format = "#,##0";
                ws.Cells[6, 3].Value = sum_return;
                using (ExcelRange cell = ws.Cells[11, 1, rowIndex - 1, 19])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }

        public static ExcelPackage EdongBalance(string id, string template, ePosAccount posAccount)
        {
            FileInfo temp = new FileInfo(template);
            ExcelPackage epk = new ExcelPackage(temp);
            ExcelWorksheet ws = epk.Workbook.Worksheets["BC Vi tien mat"];
            ws.Cells[3, 4].Value = "Ngày: " + DateTime.Now.ToString("HH:mm") + " " + DateTime.Now.ToString("dd/MM/yyyy");


            if (ePOSSession.GetObject(id) != null)
            {
                int rowIndex = 8;
                foreach (var item in (List<ObjReport>)ePOSSession.GetObject(id))
                {
                    ws.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 1].Style.Numberformat.Format = "@";
                    ws.Cells[rowIndex, 1].Value = item.col_1;
                    ws.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[rowIndex, 2].Value = item.col_2;
                    ws.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 3].Value = string.IsNullOrEmpty(item.col_3) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_3, false));
                    ws.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 4].Value = string.IsNullOrEmpty(item.col_4) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_4, false));
                    ws.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 5].Value = string.IsNullOrEmpty(item.col_5) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_5, false));
                    ws.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 6].Value = string.IsNullOrEmpty(item.col_6) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_6, false));
                    ws.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 7].Value = string.IsNullOrEmpty(item.col_7) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_7, false));
                    ws.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 8].Value = string.IsNullOrEmpty(item.col_8) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_8, false));
                    ws.Cells[rowIndex, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 9].Value = string.IsNullOrEmpty(item.col_9) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_9, false));
                    ws.Cells[rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 10].Value = string.IsNullOrEmpty(item.col_10) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_10, false));
                    ws.Cells[rowIndex, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 11].Value = string.IsNullOrEmpty(item.col_11) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_11, false));
                    ws.Cells[rowIndex, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "#,##0";
                    ws.Cells[rowIndex, 12].Value = string.IsNullOrEmpty(item.col_12) == true ? 0 : decimal.Parse(Validate.ProcessReplace(item.col_12, false));

                    rowIndex++;
                }
                using (ExcelRange cell = ws.Cells[8, 1, rowIndex - 1, 12])
                {
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var border = cell.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }
            }
            return epk;
        }
    }
}