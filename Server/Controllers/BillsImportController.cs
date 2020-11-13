using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AliaaCommon;
using FarsiLibrary.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using TciCommon.Server;
using TciEnergy.Blazor.Shared.Models;

namespace TciEnergy.Blazor.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BillsImportController : BaseController
    {
        public BillsImportController(ProvinceDBs dbs) : base(dbs) { }

        [HttpPost]
        public async Task<UploadExcelResult> UploadExcel(IFormFile file)
        {
            var fileName = Path.GetTempFileName();
            FileInfo finfo = new FileInfo(fileName);
            finfo.Attributes = FileAttributes.Temporary;
            using var uploadedStream = file.OpenReadStream();
            using var memoryStream = new MemoryStream((int)file.Length);
            await uploadedStream.CopyToAsync(memoryStream);
            uploadedStream.Close();

            memoryStream.Position = 0;
            using var fileStream = finfo.OpenWrite();
            await memoryStream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            fileStream.Close();

            memoryStream.Position = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var excel = new ExcelPackage(memoryStream);
            var sheet = excel.Workbook.Worksheets[excel.Compatibility.IsWorksheets1Based ? 1 : 0];
            var result = new UploadExcelResult { FileName = finfo.Name };
            var headers = Enumerable.Range(1, sheet.Dimension.Columns)
                .Select(col => new { col, text = sheet.GetValue<string>(1, col) })
                .Where(h => !string.IsNullOrEmpty(h.text));
            result.Headers = new List<UploadExcelResult.Header>();
            foreach (var h in headers)
            {
                var similarField = GetBestSimilarField(BillFields.Keys, h.text, out float rate);
                result.Headers.Add(new UploadExcelResult.Header
                {
                    Text = h.text,
                    ColumnIndex = h.col,
                    BestSimilarField = BillFields[similarField],
                    SimilarityRate = rate
                });
            }
            return result;
        }

        private static readonly Dictionary<string, string> BillFields = ElecBill.ValidImportProperties.ToDictionary(k => DisplayUtils.DisplayName(k), v => v.Name);

        private string GetBestSimilarField(IEnumerable<string> fields, string compareTo, out float bestSimilarity)
        {
            bestSimilarity = 0;
            string best = fields.FirstOrDefault();
            foreach (var f in fields)
            {
                float sim = StringUtils.GetSimilarityRateOfStrings(f, compareTo);
                if (sim > bestSimilarity)
                {
                    bestSimilarity = sim;
                    best = f;
                }
            }
            return best;
        }

        [HttpPost]
        public ActionResult<List<string>> SubmitExcelColumns(SelectedExcelColumns req)
        {
            var errors = new List<string>();
            var saveList = new List<ElecBill>();

            var filePath = Path.Combine(Path.GetTempPath(), req.FileName);
            if (!System.IO.File.Exists(filePath))
            {
                errors.Add("فایل موجود نیست! لطفا دوباره آپلود نمائید.");
                return errors;
            }

            var columnMap = req.SelectedColumns.Select(kv => new { Column = kv.Key, Property = ElecBill.ValidImportProperties.First(p => p.Name == kv.Value) })
                .ToDictionary(a => a.Column, a => a.Property);

            var billsDuplicateCheck = db.Aggregate<ElecBill>()
                .Group(k => k.SubsNum, g => new { SubsNum = g.Key, Dates = g.Select(b => b.CurrentDate).ToList() })
                .ToEnumerable().ToDictionary(a => a.SubsNum, a => a.Dates);

            var file = new FileInfo(filePath);
            using var pkg = new ExcelPackage(file);
            var sheet = pkg.Workbook.Worksheets[pkg.Compatibility.IsWorksheets1Based ? 1 : 0];

            int row = 2;
            while (row <= sheet.Dimension.Rows)
            {
                ElecBill obj = new ElecBill();
                int subsNumCol = 1;
                foreach (int col in columnMap.Keys)
                {
                    var prop = columnMap[col];
                    if (prop.Name == nameof(ElecBill.SubsNum))
                        subsNumCol = col;

                    object value = null;
                    try
                    {
                        value = sheet.GetValue(row, col);
                        if (value == null)
                            continue;
                        Type ptype = prop.PropertyType;
                        int intValue;
                        float floatValue;
                        long longValue;
                        if (ptype == typeof(int) && (value is double || value is float))
                        {
                            intValue = (int)(double)value;
                            prop.SetValue(obj, intValue);
                        }
                        else if (ptype == typeof(float) && value is double)
                        {
                            floatValue = (float)(double)value;
                            prop.SetValue(obj, floatValue);
                        }
                        else if (ptype == typeof(long) && value is double)
                        {
                            longValue = (long)(double)value;
                            prop.SetValue(obj, longValue);
                        }
                        else if (ptype == typeof(DateTime) && (value is double || value is float))
                        {
                            string strDate = value.ToString();
                            DateTime date = PersianDateUtils.ParseToPersianDate(strDate).ToDateTime();
                            date = new DateTime(date.Ticks, DateTimeKind.Utc);
                            date = date.AddHours(12);
                            prop.SetValue(obj, date);
                        }
                        else if (value is string && ptype == typeof(int) && int.TryParse((string)value, out intValue))
                            prop.SetValue(obj, intValue);
                        else if (value is string && ptype == typeof(float))
                        {
                            string strValue = (string)value;
                            if (strValue.StartsWith("."))
                                strValue = "0" + strValue;
                            prop.SetValue(obj, float.Parse(strValue));
                        }
                        else if (ptype == typeof(long) && long.TryParse((string)value, out longValue))
                            prop.SetValue(obj, longValue);
                        else if (ptype == typeof(DateTime) && value is string)
                        {
                            var date = PersianDateUtils.ParseToPersianDate((string)value).ToDateTime().AddHours(12);
                            prop.SetValue(obj, date);
                        }
                        else
                            prop.SetValue(obj, value);
                    }
                    catch
                    {
                        errors.Add($"سلول {ColumnAlphabet(col)}{row} دارای مقدار نامعتبر است ({value}). ");
                    }
                }
                var subscriber = db.FindFirst<Subscriber>(s => s.ElecSub.ElecSubsNum == obj.SubsNum);
                if (subscriber == null)
                    errors.Add($"سلول {ColumnAlphabet(subsNumCol)}{row}: مشترک با شماره اشتراک {obj.SubsNum} تعریف نشده است.");
                else
                {
                    obj.CityId = subscriber.City;
                    if (!req.OverwriteExistingBills)
                    {
                        if (billsDuplicateCheck.ContainsKey(obj.SubsNum) && billsDuplicateCheck[obj.SubsNum].Contains(obj.CurrentDate))
                            errors.Add($"سطر {row}: قبض با شماره اشتراک {obj.SubsNum} و تاریخ فعلی {obj.CurrentDate.ToPersianDate()} قبلا موجود است!");
                        else
                        {
                            if (billsDuplicateCheck.ContainsKey(obj.SubsNum))
                                billsDuplicateCheck[obj.SubsNum].Add(obj.CurrentDate);
                            else
                                billsDuplicateCheck.Add(obj.SubsNum, new List<DateTime> { obj.CurrentDate });
                        }
                    }
                }
                if (errors.Count == 0)
                    saveList.Add(obj);
                row++;
            }

            if (errors.Count == 0)
            {
                if (req.OverwriteExistingBills)
                    foreach (var obj in saveList)
                        db.DeleteOne<ElecBill>(eb => eb.Year == obj.Year && eb.Period == obj.Period && eb.SubsNum == obj.SubsNum);
                db.InsertMany(saveList);
            }
            pkg.Dispose();
            file.Delete();
            return errors;
        }

        private static string ColumnAlphabet(int col)
        {
            if (col <= 26)
                return ((char)('A' + col - 1)).ToString();
            else
            {
                char first = (char)('A' + (col / 26) - 1);
                char second = (char)('A' + (col % 26) - 1);
                return first.ToString() + second.ToString();
            }
        }
    }
}
