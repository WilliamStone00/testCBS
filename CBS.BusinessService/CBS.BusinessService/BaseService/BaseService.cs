

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Web;
using CBS.FrontDesk.Data.Message;
using System.Reflection.Emit;
using System.Web.Helpers;
using CBS.FrontDesk.Data.ImageProperties;
using System.IO.Compression;
using CBS.FrontDesk.Helper.Helper;
using System.Web.Security;
using CBS.FrontDesk.Data.Entity;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using System.Threading.Tasks;
using CBS.FrontDesk.Helper;
using System.Text.RegularExpressions;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;

namespace BusinessServices
{
    using System;
    using System.Globalization;

    public static class LocalizedDateConverter
    {
        private const string DefaultCultureCode = "en-US";
        private const string DefaultDateFormat = "d"; // Short date pattern

        /// <summary>
        /// Converts start and end date strings into DateTime values using localization and formatting logic.
        /// </summary>
        public static void ConvertDateRange(
            string startDateInput,
            string endDateInput,
            out DateTime startDate,
            out DateTime endDate,
            string cultureCode = DefaultCultureCode,
            string inputFormat = null)
        {
            var culture = GetCultureOrDefault(cultureCode);

            endDate = ParseDate(endDateInput, culture, inputFormat) ?? DateTime.Today;
            startDate = ParseDate(startDateInput, culture, inputFormat) ?? new DateTime(endDate.Year, endDate.Month, 1);
        }

        /// <summary>
        /// Parses a string into a DateTime using a specific or default format and culture.
        /// </summary>
        public static DateTime? ParseDate(string input, string cultureCode = DefaultCultureCode, string inputFormat = null)
        {
            var culture = GetCultureOrDefault(cultureCode);
            return ParseDate(input, culture, inputFormat);
        }

        /// <summary>
        /// Converts a DateTime to a localized string using a specific format and culture.
        /// </summary>
        public static string ToLocalizedString(DateTime date, string format = DefaultDateFormat, string cultureCode = DefaultCultureCode)
        {
            var culture = GetCultureOrDefault(cultureCode);
            return date.ToString(format, culture);
        }

        /// <summary>
        /// Gets a valid CultureInfo object. Defaults to en-US if the input is invalid.
        /// </summary>
        private static CultureInfo GetCultureOrDefault(string cultureCode)
        {
            try
            {
                return new CultureInfo(string.IsNullOrWhiteSpace(cultureCode) ? DefaultCultureCode : cultureCode);
            }
            catch
            {
                return new CultureInfo(DefaultCultureCode);
            }
        }

        /// <summary>
        /// Internal parse method with optional format.
        /// </summary>
        private static DateTime? ParseDate(string input, CultureInfo culture, string format)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (!string.IsNullOrWhiteSpace(format))
            {
                if (DateTime.TryParseExact(input, format, culture, DateTimeStyles.None, out var exact))
                    return exact;
            }

            if (DateTime.TryParse(input, culture, DateTimeStyles.None, out var fallback))
                return fallback;

            return null;
        }
    }


    public class BaseService
    {
        private Random random = new Random();
        readonly RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();
        public DateTime CurrentDate { get; set; }
        public string UserGroup { get; set; }
        public string UserID { get; set; }
        public string BankID { get; set; }
        public string BankName { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public DateTime NewCurrentDate { get; set; }
        public TimeSpan NewCurrentTime { get; set; }
        public string CredixEmail { get; set; }
        public string ConsultantEmail { get; set; }
        public string MainBank { get; set; }
        public string OperatorEmail { get; set; }
        public string[] Days_EN { get; set; }
        public string[] MonthsArrayList { get; set; }
        public string[] MonthsArrayFr { get; set; }
        public string[] Time_ { get; set; }
        public string culture { get; set; }
        public string yyyymmddmmssms { get; set; }
        public CultureInfo CurrentCultureInfo { get; set; }
        public int Hours_ { get; set; }
        public string event_bank_investment { get; set; }
        public string event_daily_bank_increase { get; set; }
        public ExecutionMessages ExecutionMessage { get; set; }
        public ImagesProperties ImagesProperty { get; set; }
        public string StringCurrentMonth { get; set; }
        public int StringCurrentMonthConvertedToInt { get; set; }
        public string StringCurrentDay { get; set; }
        public int InCurrentMonth { get; set; }
        public int InCurrentDay { get; set; }
        public string[] Characters { get; set; }
        public string[] Years { get; set; }
        public string[] ServicesShortNames { get; set; }
        public string[] ServicesNames { get; set; }



        public BaseService()
        {
            event_bank_investment = "event:bank:investment:create";
            event_daily_bank_increase = "event:bank:dailylimit:increase";
            ExecutionMessage = new ExecutionMessages();
            ImagesProperty = new ImagesProperties();
            CredixEmail = "credixinfo@credixcm.com";
            ConsultantEmail = "consultant@credixcm.com";
            culture = "en";
            ServicesShortNames = new string[] { "M", "C", "S", "CR", "U", "MD" };
            Years = new string[] { "2019", "2020", "2021", "2021" };
            ServicesNames = new string[] { "MOMOKASH", "CORPOCASH", "SAFETYNET", "CORPOCASH RECOVERY", "UNICEF", "MOMOKASH Dealer" };
            CurrentCultureInfo = Thread.CurrentThread.CurrentCulture;
            OperatorEmail = "operator@credixcm.com";
            NewCurrentDate = new DateTime(2000, 01, 01);
            NewCurrentTime = new TimeSpan(0, 0, 0);
            Characters = new string[] { "'", "/", " ", "-", "=", "*", "\\" };
            //CurrentDate = DateTime.UtcNow.AddMinutes(61);
            CurrentDate = DateTime.Now;
            string Btime = CurrentDate.TimeOfDay.Hours.ToString() + ":" + CurrentDate.TimeOfDay.Minutes.ToString() + ":" + CurrentDate.TimeOfDay.Seconds.ToString();
            CurrentTime = TimeSpan.Parse(Btime);
            Hours_ = 0;
            yyyymmddmmssms = GetDateYYYYMMDDMMSSMSS();
            MonthsArrayList = CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames;
            //HttpCookie langCookie = HttpContext.Current.Request.Cookies["culture"];//fr or en
            //if (langCookie != null)
            //{
            //    culture = langCookie.Value;
            //}
            Days_EN = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            Time_ = new string[] { "00:00:00", "01:00:00", "02:00:00", "03:00:00", "04:00:00", "05:00:00", "06:00:00", "07:00:00"
            , "08:00:00", "09:00:00", "10:00:00", "11:00:00", "12:00:00", "13:00:00", "14:00:00", "15:00:00", "16:00:00", "17:00:00", "18:00:00", "19:00:00", "20:00:00", "21:00:00", "22:00:00", "23:00:00"};
            MainBank = "MFI";
            StringCurrentMonth = CurrentDate.ToString("MMMM");
            InCurrentMonth = CurrentDate.Month;
            //StringCurrentDay = CurrentDate.DayOfWeek.ToString();
            //InCurrentMonth = CurrentDate.Day;
        }
        public string ToQueryString<T>(T obj)
        {
            var properties = from p in typeof(T).GetProperties()
                             where p.GetValue(obj, null) != null
                             select $"{p.Name}={Uri.EscapeDataString(p.GetValue(obj, null).ToString())}";

            return string.Join("&", properties.ToArray());
        }
        public string RemoveDuplicateSlashes(string url)
        {
            // Replace occurrences of double forward slashes (//) with single forward slash (/)
            return url.Replace("//", "/");
        }
        public bool ComputeDenomination(CurrencyNotes currencyNotes, int amount)
        {
            int totalNotesValue = currencyNotes.note10000 * 10000 +
                                  currencyNotes.note5000 * 5000 +
                                  currencyNotes.note2000 * 2000 +
                                  currencyNotes.note1000 * 1000 +
                                  currencyNotes.note500 * 500 +
                                  currencyNotes.coin500 * 500 +
                                  currencyNotes.coin100 * 100 +
                                  currencyNotes.coin50 * 50 +
                                  currencyNotes.coin25 * 25 +
                                  currencyNotes.coin10 * 10 +
                                  currencyNotes.coin5 * 5 +
                                  currencyNotes.coin1;
            if (amount!= totalNotesValue)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Validates that the sum of denominations in a `BulkDeposit` object matches the entered amount.
        /// </summary>
        /// <param name="currencyNotes">The `CurrencyNotes` object containing denomination counts.</param>
        /// <param name="amount">The entered amount in the `BulkDeposit` object.</param>
        /// <returns>A tuple containing a boolean indicating validation success and a discrepancy message if any.</returns>
        public (bool, string) ValidateDenominations(CurrencyNotes currencyNotes, decimal amount)
        {
            decimal calculatedAmount =
                (currencyNotes.note10000 * 10000) +
                (currencyNotes.note5000 * 5000) +
                (currencyNotes.note2000 * 2000) +
                (currencyNotes.note1000 * 1000) +
                (currencyNotes.note500 * 500) +
                (currencyNotes.coin500 * 500) +
                (currencyNotes.coin350 * 350) +
                (currencyNotes.coin250 * 250) +
                (currencyNotes.coin200 * 200) +
                (currencyNotes.coin150 * 150) +
                (currencyNotes.coin100 * 100) +
                (currencyNotes.coin50 * 50) +
                (currencyNotes.coin25 * 25) +
                (currencyNotes.coin10 * 10) +
                (currencyNotes.coin5 * 5) +
                (currencyNotes.coin1 * 1);

            if (calculatedAmount == amount)
            {
                return (true, string.Empty);
            }

            decimal discrepancy = calculatedAmount - amount;
            string discrepancyType = discrepancy > 0 ? "Excess" : "Shortage";
            string discrepancyMessage = $"Denomination sum is {discrepancyType} by {Math.Abs(discrepancy):N2}. Entered Amount: {amount:N2}, Calculated Amount: {calculatedAmount:N2}.";

            return (false, discrepancyMessage);
        }

        public static DataTable ConvertToDataTable<T>(T obj, string tableName)
        {
            var table = new DataTable(tableName);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            var row = table.NewRow();
            foreach (var prop in props)
                row[prop.Name] = prop.GetValue(obj) ?? DBNull.Value;

            table.Rows.Add(row);
            return table;
        }

        public DataTable ConvertToDataTable<T>(List<T> list, string tableName)
        {
            var table = new DataTable(tableName);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in list)
            {
                var row = table.NewRow();
                foreach (var prop in props)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        public Guid ConvertStringToGuid(string input)
        {
            // Check if the input string is null or empty
            if (string.IsNullOrEmpty(input))
            {
                return Guid.NewGuid(); // Return a new Guid if the input string is null or empty
            }

            // Check if the input string represents a GUID
            if (Guid.TryParse(input, out Guid result))
            {
                return result; // Return the parsed GUID
            }
            else
            {
                return Guid.NewGuid(); // Return a new Guid if the input string is not a valid GUID
            }
        }

        /// <summary>
        /// This function create archives of all files created for previous days and delete files already archived.
        /// param: DestinationzipPath, The destination to save the archive files. Path example, @"C:\ServiceLogs\Momokash\KYC_SCORING\Archive".
        /// param: sourcefilepath, The source path where files are being read from. Path example, @"C:\ServiceLogs\Momokash\KYC_SCORING\Log".
        /// param: filename, The Name file the saved archive file. example KYC_SCORING.
        /// </summary>
        /// <param Name="DestinationzipPath">The destination to save the archive files. Path example, @"C:\ServiceLogs\Momokash\KYC_SCORING\Archive"</param>
        /// <param Name="sourcefilepath">The source path where files are being read from. Path example, @"C:\ServiceLogs\Momokash\KYC_SCORING\Log"</param>
        /// <param Name="filename">The Name file the saved archive file. example KYC_SCORING</param>
        public void CreateFileArchives(string DestinationzipPath, string sourcefilepath, string filename)
        {
            if (!Directory.Exists(DestinationzipPath))
            {
                Directory.CreateDirectory(DestinationzipPath);
            }
            var zipFile = $"{DestinationzipPath}\\{filename}_{DateTime.Now.ToString("dd.MMM.yyyy.hh.mm.ss")}.zip";
            var files = Directory.GetFiles(sourcefilepath);
            using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                foreach (var fPath in files)
                {
                    FileInfo fi = new FileInfo(fPath);
                    if (fi.CreationTime.ToString("dd.MMM.yyyy") != DateTime.Now.ToString("dd.MMM.yyyy"))
                    {
                        archive.CreateEntryFromFile(fPath, Path.GetFileName(fPath));
                        File.Delete(fPath);
                    }
                }
            }
        }
        public static string FormatCurrency(decimal amount, string currencySymbol = "XAF", int decimalPlaces = 2)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1:N" + decimalPlaces + "}", currencySymbol, amount);
        }
        public bool CheckTekn()
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies["Token"];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (!authTicket.Expired)
                {
                    HttpContext.Current.Session["Token"] = authTicket.UserData;
                    return true;
                }
            }
            //Connect to server and get new token
            return false;
        }

        public List<string> PrepareDates(string currentdate)
        {
            int current = Convert.ToInt32(currentdate);
            List<string> dates = new List<string> { $"{current}", $"{current + 1}", $"{current - 1}" };
            return dates;

        }
       
       
        public bool IsExist(string FilePath)
        {
            return File.Exists(FilePath);
        }
        public bool IsAnyNullOrEmpty(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public string AnyNullOrEmpty(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject);
                    if (string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }
            return "N";
        }

        public string GenerateFileHash(string filePath)
        {
            using (var md5Instance = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hashResult = md5Instance.ComputeHash(stream);
                    return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public string SelectSubscriberServiceShortName(string str)
        {
            string service = Array.Find(ServicesNames, element => element == str);
            string[] lengh = str.Split(new Char[] { ' ' });
            if (str.Length == 1)
            {
                return str.ToUpper();
            }
            int index = Array.FindIndex(ServicesNames, element => element == service);
            string res = "M";

            if (lengh.Length == 2)
            {
                res = $"{lengh[0].Substring(0, 1)}{lengh[1].Substring(0, 1)}";
            }
            else /*(index != 3)*/
            {
                res = service.Substring(0, 1);
            }

            return res.ToUpper();
        }

        //public UserSession GetUserSession()
        //{
        //    LocalSession local = new LocalSession();
        //    if (HttpContext.Current.User.Identity.IsAuthenticated)
        //    {

        //        var userSession = new UserSession();
        //        string str = HttpContext.Current.User.Identity.Name;
        //        userSession = local.GetUserSession(str);
        //        return userSession;
        //    }


        //    return null;
        //}
        public double RoundUp(double valueToRound)
        {
            return (Math.Floor(valueToRound + 0.5));
        }

        public double RoundUpRoundDown(double valueToRound)
        {
            double floorValue = Math.Floor(valueToRound);
            if ((valueToRound - floorValue) > .5)
            {
                return (floorValue + 1);
            }
            else
            {
                return (RoundUp(valueToRound));
            }
        }
        /// <summary>
        /// Key e.g =Loan_List
        /// Criterial e.g=20201210 transaction date
        /// </summary>
        /// <param Name="Key"></param>
        /// <param Name="Criteria"></param>
        /// <returns>Concatinated key as (Loan_List:20201210) for example.</returns>
        public string GenerateKey(string Key, string Criteria)
        {
            Key = $"{Key}:{Criteria}";
            return Key;
        }
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GenerateTimeStamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
       
        
        
       
        public string RoundUpMOMOKASH(string Amount)
        {
            string NewValue = null;
            string[] Value = Amount.Split('.');
            if (Value.Length == 1)
            {
                NewValue = Amount;
            }
            else
            {
                if (ConverToInteger(Value[0]) >= 5)
                {
                    NewValue = Math.Round(ConverToDouble(Value[0]), 0).ToString();
                }
                else
                {
                    string s = Math.Round(ConverToDouble(Value[0]), 0).ToString();
                    int i = ConverToInteger(s) + 1;
                    NewValue = i.ToString();
                }

            }


            return NewValue;
        }
     

        public ExecutionMessages GetExecutionMessages(object data,bool Result, string ObjectName, MessagesResults MessagesResults, ExecutionProcessOption ExecutionProcessOption, string messageStatus, Exception ex = null, string errorMessage = null, string SessionID = null)
        {

            ExecutionMessage.Result = Result;
            ExecutionMessage.ObjectName = ObjectName;
            ExecutionMessage.MessagesResults = MessagesResults;
            ExecutionMessage.ProcessOption = ExecutionProcessOption;
            ExecutionMessage.MessageStatus = messageStatus;
            ExecutionMessage.Data = data;

            if (ex != null)
            {
                ExecutionMessage.MessageString = ex.Message.ToString();
            }
            else
            {
                ExecutionMessage.MessageString = errorMessage;
            }
            ExecutionMessage.SessionID = SessionID;
            return ExecutionMessage;
        }

        public List<string> GetWeekDaysByCulture(CultureInfo culture)
        {
            return new List<string>
    {

        culture.DateTimeFormat.GetDayName(DayOfWeek.Monday),
        culture.DateTimeFormat.GetDayName(DayOfWeek.Tuesday),
        culture.DateTimeFormat.GetDayName(DayOfWeek.Wednesday),
        culture.DateTimeFormat.GetDayName(DayOfWeek.Thursday),
        culture.DateTimeFormat.GetDayName(DayOfWeek.Friday),
        culture.DateTimeFormat.GetDayName(DayOfWeek.Saturday),
        culture.DateTimeFormat.GetDayName(DayOfWeek.Sunday)
    };
        }

        public List<string> GetMonthNamesByCulture(CultureInfo culture)
        {
            return new List<string>
    {
        culture.DateTimeFormat.GetMonthName(1),
        culture.DateTimeFormat.GetMonthName(2),
        culture.DateTimeFormat.GetMonthName(3),
        culture.DateTimeFormat.GetMonthName(4),
        culture.DateTimeFormat.GetMonthName(5),
        culture.DateTimeFormat.GetMonthName(6),
        culture.DateTimeFormat.GetMonthName(7),
        culture.DateTimeFormat.GetMonthName(8),
        culture.DateTimeFormat.GetMonthName(9),
        culture.DateTimeFormat.GetMonthName(10),
        culture.DateTimeFormat.GetMonthName(11),
        culture.DateTimeFormat.GetMonthName(12),
    };
        }
        public int CalculateAgeString(DateTime Dob)
        {
            try
            {
                int age = 0;
                if (Dob.Date > CurrentDate.Date)
                {
                    age = 0;
                }
                else
                {
                    DateTime Now = CurrentDate;
                    int Years = new DateTime(Now.Subtract(Dob).Ticks).Year - 1;
                    age = Years;
                }
                return age;
            }
            catch (Exception ex)
            {

                throw ex;
            }



        }
        public int MismatchCharacterCount(string strInput, char matchChar)
        {
            int zeroCount = strInput.Count(x => x == matchChar);
            return zeroCount;
        }
        public bool TestCase(int zeroCount, int testcase)
        {
            if (zeroCount >= testcase)
            {
                return false;
            }
            return true;
        }



        public ExecutionMessages CheckFileContent(string path, string fileName)
        {


            try
            {
                int count = 0;
                string csvData = File.ReadAllText(path);
                foreach (string row in csvData.Split('\n').Skip(1))
                {
                    if (count.Equals(0))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {

                        }
                        else
                        {
                            ExecutionMessage.Result = true;
                            ExecutionMessage.ObjectName = fileName;
                            return ExecutionMessage;
                        }
                        count += 1;
                    }

                }

            }

            catch (Exception ex)
            {


                throw ex;
            }


            return ExecutionMessage;
        }
        public string GetCureentWeek()
        {


            DateTime startOfWeek = DateTime.Today.AddDays((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)DateTime.Today.DayOfWeek);
            string result = string.Join(",", Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i).ToString("dd-MM-yyyy")));
            return result;
        }

        public IEnumerable<DateTime> GetCureentWeekDate()
        {



            var now = CurrentDate;
            var currentDay = now.DayOfWeek;
            int days = (int)currentDay;
            DateTime sunday = now.AddDays(-days);
            var daysThisWeek = Enumerable.Range(0, 7).Select(d => sunday.AddDays(d)).ToList();
            return daysThisWeek;
        }
        public IEnumerable<DateTime> GetWeeksOftheMonth()
        {
            var month = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day);
            var weeks = Enumerable.Range(0, 4).Select(n => month.AddDays(n * 7 - (int)month.DayOfWeek + 1)).TakeWhile(monday => monday.Month == month.Month);
            return weeks;
        }



        public RoundUp RoundOperation(double p)
        {
            RoundUp data = new RoundUp();
            if ((p % 1).Equals(0))
            {
                data.AmountPaid = Convert.ToInt32(p);
                data.ExcessAmount = 0;
                data.RoundUpAmount = Convert.ToInt32(p);
                data.HasExcessAmount = false;
                return data;
            }
            else
            {
                data.AmountPaid = Convert.ToInt32(p);
                data.ExcessAmount = 1 - (p - Math.Truncate(p));// p -Convert.ToInt32(data.AmountPaid)+1 ;

                if (((p - Math.Truncate(p)) <= 0.4))
                {
                    p = p + 1;
                    data.RoundUpAmount = Convert.ToInt32(p);
                }
                else
                {
                    p = p + 1;
                    data.RoundUpAmount = Math.Truncate(p);
                }
                data.HasExcessAmount = true;
                return data;
            }
        }
        public List<string> GetOperationTypes()
        {
            return new List<string>
            {   "All",
                "CASH_IN",
                "WITHDRAWAL",
                "Transfer",
                "CASH_IN_Remittance",
                "CASH_W_Remittance",
                "CASH_IN_MOMO_CASH",
                "LOAN_RECOVERY",
                "LOAN_REPAYMENT_MOMO_CASH",
                "LOAN_REPAYMENT",
                "CASHIN_MOBILEMONEY",
                "OTP_CASHOUT_THIRD_PARTY",
                "GAV_TRANSFER_RECEIVED",
                "GAV_TRANSFER",
                "Loan_Disbursement",
                "Mobile_Money_Topup",
                "Orange_Money_Topup",
                "OTP_GENERATION_TRANSACTION"
            };
        }

        public List<string> GetStatuses()
        {
            return new List<string> { "All", "Failed", "Successful" };
        }
        public int MonthStringToIntegerMMMM(string month)
        {
            int I = DateTime.ParseExact(month, "MMMM", CultureInfo.CurrentCulture).Month;

            return I;
        }



        public string RemoveSpecialCahracters(string str, string charactertoRemove)
        {
            string strr = str.Replace($"{charactertoRemove}", "-");
            return strr;
        }
       
     
        public string SplitDateStringYYYYMMDD(string str)
        {
            string Year, Month, Day;
            Year = str.Substring(0, 4);
            Month = str.Substring(4, 2);
            Day = str.Substring(6, 2);
            return $"{Day}/{Month}/{Year}";
        }
        public string HHMMSS(string str)
        {
            string HH, MM, SS;
            HH = str.Substring(0, 2);
            MM = str.Substring(2, 2);
            SS = str.Substring(4, 2);
            string time = $"{HH}:{MM}:{SS}";
            return time;
        }
        //public IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        //{
        //    return Enumerable.Range(0, (endDate - startDate).Days + 1).Select(d => startDate.AddDays(d));
        //}
        public IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate must be greater than or equal to startDate");

            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }
        public string GetDateDDMMYYYYMMSSMSS()
        {
            string MM = DateTime.Now.Month.ToString();
            string DD = DateTime.Now.Day.ToString();
            string MMN = DateTime.Now.Minute.ToString();
            if (MM.Length == 1)
            {
                MM = $"0{MM}";
            }
            if (DD.Length == 1)
            {
                DD = $"0{DD}";
            }
            if (MMN.Length == 1)
            {
                MMN = $"0{MMN}";
            }
            string datee = $"{DD}{MM}{DateTime.Now.Year}{MMN}{DateTime.Now.Second}{DateTime.Now.Millisecond}";
            return datee;
        }
        public string GetDateStringToSingle(string str)
        {
            string[] Date = str.Split(new Char[] { '-', '/' });
            string datee = $"{Date[0]}{Date[1]}{Date[2]}";
            return datee;
        }
        public string GetDateStringToYYYMMDD(string str)
        {
            string[] Date = str.Split(new Char[] { '-', '/', ' ' });
            string datee = null;
            string Day = Date[0];
            string Month = Date[1];
            string Year = Date[2];
            if (Date[0].Length == 1)
            {
                Day = $"0{Date[0]}";
            }
            if (Date[1].Length == 1)
            {
                Month = $"0{Date[1]}";
            }
            datee = $"{Year}{Month}{Day}";
            return datee;
        }
        public DateTime ConvertToDate(int year, int month, int day)
        {
            try
            {
                // Check if the year, month, and day values are valid
                if (year < 1 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
                {
                    throw new ArgumentException("Invalid date components.");
                }

                // Create a DateTime object using the provided year, month, and day
                DateTime date = new DateTime(year, month, day);
                return date;
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., invalid date components)
                Console.WriteLine($"Error converting date: {ex.Message}");
                throw;
            }
        }
        public (int year, int month, int day) ConvertToYearMonthDay(DateTime date)
        {
            try
            {
                // Extract year, month, and day components from the DateTime object
                int year = date.Year;
                int month = date.Month;
                int day = date.Day;

                return (year, month, day);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Error converting date: {ex.Message}");
                throw;
            }
        }
        public string GetDateStringToYYYMMDDPROD(string str)
        {
            string[] Date = str.Split(new Char[] { '-', '/', ' ' });
            string datee = null;
            string Day = Date[0];
            string Month = Date[1];
            string Year = Date[2];
            if (Date[0].Length == 1)
            {
                Day = $"0{Date[0]}";
            }
            if (Date[1].Length == 1)
            {
                Month = $"0{Date[1]}";
            }
            datee = $"{Year}{Day}{Month}";
            return datee;
        }
        public string DateDayMonthYear(string str)
        {
            string[] Date = str.Split(new Char[] { '-', '/' });
            string datee = null;
            string Day = Date[0];
            string Month = Date[1];
            string Year = Date[2];
            if (Date[0].Length == 1)
            {
                Day = $"0{Date[0]}";
            }
            if (Date[1].Length == 1)
            {
                Month = $"0{Date[1]}";
            }
            datee = $"{Day}/{Month}/{Year}";
            return datee;
        }
        public string GetDateYYYYMMDDMMSSMSS()
        {
            string MM = DateTime.Now.Month.ToString();
            string DD = DateTime.Now.Day.ToString();
            string MMN = DateTime.Now.Minute.ToString();
            if (MM.Length == 1)
            {
                MM = $"0{MM}";
            }
            if (DD.Length == 1)
            {
                DD = $"0{DD}";
            }
            if (MMN.Length == 1)
            {
                MMN = $"0{MMN}";
            }
            string datee = $"{DateTime.Now.Year}{MM}{DD}{MMN}{DateTime.Now.Second}{DateTime.Now.Millisecond}";
            return datee;
        }

        public double ConvertScientificNumerationToDouble(string numeration)
        {
            if (IsNumeric(numeration))
            {
                double numeratin = Double.Parse(numeration, System.Globalization.NumberStyles.Float);
                return numeratin;
            }
            return 0;
        }
        public bool ExecuteSQlScriptFullPath(string FullPath)
        {
            var constr = ConfigurationManager.ConnectionStrings["ScriptRunner"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    FileInfo fileInfo = new FileInfo(FullPath);
                    string script = fileInfo.OpenText().ReadToEnd();
                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        public bool ExecuteSQlScript(string path)
        {
            var constr = ConfigurationManager.ConnectionStrings["ScriptRunner"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(constr))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    string script = fileInfo.OpenText().ReadToEnd();
                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        public bool FindSpecialChar(string input)
        {
            string specialChar = @"\|!#$%&/()=?»«@£§€{};'<>_,[]";
            foreach (var item in specialChar)
            {
                if (input.Contains(item)) return true;
            }

            return false;
        }

        public void CheckIfFileExist(string filepath)
        {
            if (File.Exists(filepath))
            {

                File.Delete(filepath);
                File.Create(filepath).Dispose();
            }
            else
            {
                File.Create(filepath).Dispose();
            }
        }

        public void CreatePath(string Directoypath)
        {
            if (!Directory.Exists(Directoypath))
            {
                Directory.CreateDirectory(Directoypath);
            }
        }

        public void CreateFullPath(string Directoypath, string filePath)
        {
            if (!Directory.Exists(Directoypath))
            {
                Directory.CreateDirectory(Directoypath);
            }
            if (File.Exists(filePath))
            {

                File.Delete(filePath);
                File.Create(filePath).Dispose();
            }
            else
            {
                File.Create(filePath).Dispose();
            }
        }
        public ExportOption GetExportOptions<T>(List<T> data, string filename = "Exported_File", bool showheader = true)
        {
            ExportOption options = new ExportOption { data = data, bytes = ExcelExportHelper.ExportExcel(data, filename, showheader), filename = $"{filename}_{CurrentDate.ToString("yyyyMMdd_hh_mm_ss")}.xlsx", path = ExcelExportHelper.ExcelContentType };
            return options;
        }
        public string DurationInSystem(DateTime Dob)
        {
            try
            {
                int Years = 0;
                DateTime Now = CurrentDate;

                //string D = $"{Dob.Day}-{Dob.Month}-{Dob.Year}";
                //DateTime DT = Convert.ToDateTime(D);
                Years = new DateTime(Now.Subtract(Dob).Ticks).Year - 1;


                DateTime PastYearDate = Dob.AddYears(Years);
                int Months = 0;
                for (int i = 1; i <= 12; i++)
                {
                    if (PastYearDate.AddMonths(i) == Now)
                    {
                        Months = i;

                        break;
                    }
                    else if (PastYearDate.AddMonths(i) >= Now)
                    {
                        Months = i - 1;
                        break;
                    }
                }
                int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
                int Hours = Now.Subtract(PastYearDate).Hours;
                int Minutes = Now.Subtract(PastYearDate).Minutes;
                int Seconds = Now.Subtract(PastYearDate).Seconds;
                int Weeks = ((Days % 365) % 30) / 7;
                string str = String.Format("{0} Year(s) {1} Month(s) {2} Week(s) {3} Day(s) {4} Hour(s) {5} Minute(s) {6} Second(s)", Years, Months, Weeks, Days, Hours, Minutes, Seconds);

                return str;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string LastLogin(DateTime lastloginDate)
        {
            try
            {
                //int Years = 0;
                //DateTime Now = CurrentDate;
                //Years = new DateTime(Now.Subtract(lastloginDate).Ticks).Year - 1;
                //DateTime PastYearDate = lastloginDate.AddYears(Years);
                //int Months = 0;
                //for (int i = 1; i <= 12; i++)
                //{
                //    if (PastYearDate.AddMonths(i) == Now)
                //    {
                //        Months = i;

                //        break;
                //    }
                //    else if (PastYearDate.AddMonths(i) >= Now)
                //    {
                //        Months = i - 1;
                //        break;
                //    }
                //}
                //int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
                //int Hours = Now.Subtract(PastYearDate).Hours;
                //int Minutes = Now.Subtract(PastYearDate).Minutes;
                //int Seconds = Now.Subtract(PastYearDate).Seconds;
                //int Weeks = ((Days % 365) % 30) / 7;
                //string str = String.Format("{0}Y {1}M {2}W {3}D {4}H {5}Ms {6}Ss", Years, Months, Weeks, Days, Hours, Minutes, Seconds);

                return "0:0:0";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        //public string GetUserFullName()
        //{
        //    string fullName = context.Session.GetString("FullName");
        //    return fullName;
        //}
        //public string GetUserName()
        //{
        //    string fullName = context.Session.GetString("UserName");
        //    return fullName;
        //}
        //public string GetUserID()
        //{
        //    string userid = context.Session.GetString("UserID");
        //    return userid;
        //}



        //public string GetUserGroup()
        //{
        //    string role = context.Session.GetString("GroupNmae"); ;
        //    return role;
        //}

        public string GenerateTransactionCode(int minValue, int maxExclusiveValue, string Priffix)
        {
            string Transactioncode = null;
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);

            Transactioncode = Priffix + (minValue + (ui % diff));
            Transactioncode = $"{Transactioncode}X{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}";
            return Transactioncode;
        }
        public long GenerateTransactionCode(int minValue, int maxExclusiveValue)
        {
            long Transactioncode = 0;
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);

            Transactioncode = (minValue + (ui % diff));
            return Transactioncode;
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }
        public string CurrencyFormatter(double amount, int decimalplace = 0, string currency = "")
        {
            string str = null;
            if (currency.Equals(string.Empty))
            {
                str = amount.ToString("#,##0.0");
            }
            else
            {
                if (decimalplace.Equals(1))
                {
                    str = amount.ToString("#,##0.0 " + currency);
                }
                else if (decimalplace.Equals(2))
                {
                    str = amount.ToString("#,##0.00 " + currency);
                }
                else
                {
                    str = amount.ToString("#,##0 " + currency);
                }
            }

            return str;
        }
        public string CurrencyFormatter(decimal amount, int decimalplace = 0, string currency = "")
        {
            string str = null;
            if (currency.Equals(string.Empty))
            {
                str = amount.ToString("#,##0.0");
            }
            else
            {
                if (decimalplace.Equals(1))
                {
                    str = amount.ToString("#,##0.0 " + currency);
                }
                else if (decimalplace.Equals(2))
                {
                    str = amount.ToString("#,##0.00 " + currency);
                }
                else
                {
                    str = amount.ToString("#,##0 " + currency);
                }
            }

            return str;
        }
        public string PercentageFormater(double wholeValue, double quetion)
        {
            string str = null;
            str = ((quetion / wholeValue) * 100).ToString();
            str = ConverToDouble(str).ToString("#,##0.0") + "%";
            return str;
        }
        public double PercentageFormaterDouble(double wholeValue, double quetion)
        {
            double str = 0;
            str = ((quetion / wholeValue) * 100);
            return str;
        }

        public double CheckBonus(bool state, double bonusAmount)
        {
            if (state.Equals(true))
            {
                return bonusAmount;
            }
            return 0;
        }
        public string GetFullName(string gender, string FN, string MN, string LN)
        {
            string title = "MRS. ";
            if (gender.ToUpper().Equals("MALE"))
            {
                title = "MR. ";
            }
            string str = null;
            if (LN == null)
            {
                str = $"{FN.ToUpper()} {MN.ToUpper()}";
            }
            else if (MN == null)
            {
                str = $"{FN.ToUpper()} {MN.ToUpper()}";
            }
            else if (MN != null && FN != null && LN != null)
            {
                str = $"{FN.ToUpper()} {MN.ToUpper()} {LN.ToUpper()}";
            }
            return str;
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = new byte[bytesNumber];
            csp.GetBytes(buffer);
            return buffer;
        }

        public double ConverToDouble(string value)
        {
            try
            {
                if (value == null)
                {
                    value = "0";
                }
                else if (IsNumeric(value))
                {

                }
                else
                {
                    value = "0";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Convert.ToDouble(value);
        }
        public int ConverToInteger(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return 0;
                }

                // Try to parse the input string to an integer
                if (decimal.TryParse(value, out decimal decimalValue))
                {
                    // Cast the decimal value to an integer
                    return (int)decimalValue;
                }
                else
                {
                    return 0; // Conversion failed, return default value
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public string Add237(string subscriberNumber)
        {
            if (!subscriberNumber.StartsWith("237"))
                subscriberNumber = $"237{subscriberNumber}";
            return subscriberNumber;
        }
        public string Remove237(string subscriberNumber)
        {
            string str = subscriberNumber.Substring(3);
            return str;
        }

        public int SortString(string s1, string s2, string sortDirection)
        {

            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }
        public int SortDouble(string s1, string s2, string sortDirection)
        {
            double i1 = double.Parse(s1);
            double i2 = double.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }
        public int SortDate(string s1, string s2, string sortDirection)
        {
            DateTime i1 = GetDateTime(s1);
            DateTime i2 = GetDateTime(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }
        public int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = 0;
            int i2 = 0;
            if (!IsNumeric(s1))
            {
                i1 = 0;
            }
            else if (!IsNumeric(s2))
            {
                i2 = 0;
            }
            i1 = int.Parse(s1);
            i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }
        public DateTime StringDateTime(string date)
        {
            DateTime DT = Convert.ToDateTime(date);
            return DT;

        }
        public string DateDDMMYY(string date)
        {
            string[] newDate = date.Split(new Char[] { ':', '/', '-', ' ' });
            return $"{newDate[2]}/{newDate[1]}/{newDate[0]}";
        }
        public string ConvertToTimeString(string time)
        {
            try
            {
                string[] tim = time.Split(':');
                if (tim[0].Length == 1)
                {
                    tim[0] = $"0{tim[0]}";
                }
                if (tim[1].Length == 1)
                {
                    tim[1] = $"0{tim[1]}";
                }
                if (tim[2].Length == 1)
                {
                    tim[2] = $"0{tim[2]}";
                }
                string timex = $"{tim[0]}:{tim[1]}:{tim[2]}";
                return timex;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public DateTime ConvertToTime(string time)
        {
            try
            {
                string[] tim = time.Split(':');
                if (tim[0].Length == 1)
                {
                    tim[0] = $"0{tim[0]}";
                }
                if (tim[1].Length == 1)
                {
                    tim[1] = $"0{tim[1]}";
                }
                if (tim[2].Length == 1)
                {
                    tim[2] = $"0{tim[2]}";
                }

                string timex = $"{tim[0]}:{tim[1]}:{tim[2]}";

                DateTime dateTime = DateTime.ParseExact(timex, "HH:mm:ss", CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public DateTime GetDateTime(string date)
        {
            try
            {

                //string str = RearrangeRegistrationDateToDateFormat(date, Convert.ToInt32(ConfigurationManager.AppSettings["local"]));
                //var d = Convert.ToDateTime(str);
                if (date == null)
                {
                    date = GetDateFormat(CurrentDate);
                }
                return GetDateTimeCulture(date);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DateTime GetDateTimeCulture(string date, string time = null, bool isdatetime_joint = false)
        {
            try
            {
                var dateTime = DateTime.Now;
                if (date == "2022-06-01")
                {

                }
                if (date == "2022-06-03")
                {

                }

                if (date.Contains(" "))
                {
                    string[] sssx = date.Split(' ');
                    date = sssx[0];
                }
                date = date.Replace("-", "/");
                if (date == null)
                {
                    return new DateTime();
                }
                string[] sss = date.Split('/', '-');
                string i = "0";

                string newDate = date;
                if (sss[0].Length == 1 && sss[1].Length == 1)
                {


                    sss[0] = $"0{sss[0]}";
                    sss[1] = $"0{sss[1]}";
                    //newDate = $"0{sss[0]}/0{sss[1]}/{sss[2]}";
                }

                else if (sss[1].Length == 1)
                {
                    //newDate = $"{sss[0]}/0{sss[1]}/{sss[2]}";
                    sss[1] = $"0{sss[1]}";
                }
                else if (sss[0].Length == 1)
                {
                    //newDate = $"0{sss[0]}/{sss[1]}/{sss[2]}";
                    sss[0] = $"0{sss[0]}";
                    //sss[1] = $"0{sss[1]}";
                }
                if (sss[0].Length.Equals(4))
                {
                    if (Convert.ToInt32(sss[2]) > 12)
                    {
                        newDate = $"{sss[2]}/{sss[1]}/{sss[0]}";
                    }
                    newDate = $"{sss[2]}/{sss[1]}/{sss[0]}";
                }
                else if ((ConverToInteger(sss[1]) > ConverToInteger(sss[0]) && ConverToInteger(sss[1]) > 12))
                {
                    newDate = $"{sss[1]}/{sss[0]}/{sss[2]}";
                }
                else
                {
                    newDate = $"{sss[0]}/{sss[1]}/{sss[2]}";
                }
                if (!IsNumeric(sss[1]))
                {
                    i = DateTime.ParseExact(sss[1], "MMM", CultureInfo.CurrentCulture).Month.ToString();
                    if (i.Length == 1)
                    {
                        i = $"0{i}";
                    }
                    newDate = $"{sss[0]}/{i}/{sss[2]}";
                }
                if (isdatetime_joint)
                {
                    time = ConvertToTimeString(time);
                    newDate = $"{newDate} {time}";
                    dateTime = DateTime.ParseExact(newDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);

                }
                else
                {
                    dateTime = DateTime.ParseExact(newDate, "dd/MM/yyyy", CultureInfo.CurrentCulture);

                }


                return dateTime;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Returns and interger when subscracting two date
        /// </summary>
        /// <param Name="date"></param>
        /// <returns></returns>
        public int DateDefferenceReturningCount(DateTime date)
        {
            int days = 0;
            days = Math.Abs(DateTime.UtcNow.Subtract(date).Days);
            return days;
        }

        public string GetDateFormat(DateTime date, string format)
        {
            string str = null;
            str = date.ToString(format);
            return str;
        }
        public List<StringValues> GetDisbursmentFeeTypes()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Percentage", Value = "Percentage" },
                new StringValues { Text = "Fixed", Value = "Fixed" },
            };
            return list;
        }
        public List<StringValues> GetLanguages()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "English", Value = "English" },
                new StringValues { Text = "French", Value = "French" },
            };
            return list;
        }
        public List<StringValues> GetGender()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Male", Value = "Male" },
                new StringValues { Text = "Female", Value = "Female" },
                new StringValues { Text = "Others", Value = "Others" },
            };
            return list;
        }
        public List<StringValues> GetInstallmentTypesEnums()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Yearly", Value = "Yearly" },
                new StringValues { Text = "Monthly", Value = "Monthly" },
                new StringValues { Text = "Weekly", Value = "Weekly" },
                new StringValues { Text = "Daily", Value = "Daily" },
            };
            return list;
        }
        public List<StringValues> GetServices()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Corrupted_Balance_Monitoring", Value = "Corrupted_Balance_Monitoring" },
                new StringValues { Text = "Loan_Monitoring", Value = "Loan_Monitoring" },
                new StringValues { Text = "Dormant_Accounts_Monitoring", Value = "Dormant_Accounts_Monitoring" },
                new StringValues { Text = "Others_System_Monitoring", Value = "Others_System_Monitoring" },
            };
            return list;
        }
        public List<StringValues> GetScheduleTypes()
        {
            return new List<StringValues>
        {
            new StringValues { Text = "Maturity quarterly actual 360", Value = "MaturityQuarterlyActual360" },
            new StringValues { Text = "Maturity quarterly actual 360 domestic", Value = "MaturityQuarterlyActual360Domestic" },
            new StringValues { Text = "Fixed principal quarterly", Value = "FixedPrincipalQuarterly" },
            new StringValues { Text = "Fixed principal bi-annually", Value = "FixedPrincipalBiAnnually" },
            new StringValues { Text = "Fixed principal annually", Value = "FixedPrincipalAnnually" },
            new StringValues { Text = "Fixed principal biweekly", Value = "FixedPrincipalBiweekly" },
            new StringValues { Text = "Fixed principal two monthly", Value = "FixedPrincipalTwoMonthly" },
            new StringValues { Text = "Fixed principal monthly", Value = "FixedPrincipalMonthly" },
            new StringValues { Text = "Flat monthly", Value = "FlatMonthly" },
            new StringValues { Text = "Flat quarterly", Value = "FlatQuarterly" },
            new StringValues { Text = "Flat biweekly", Value = "FlatBiweekly" },
            new StringValues { Text = "Flat annually", Value = "FlatAnnually" },
            new StringValues { Text = "Flat two monthly", Value = "FlatTwoMonthly" },
            new StringValues { Text = "Flat bi-annually", Value = "FlatBiAnnually" },
            new StringValues { Text = "Differential bi-annually", Value = "DifferentialBiAnnually" },
            new StringValues { Text = "Annuity monthly", Value = "AnnuityMonthly" },
            new StringValues { Text = "Annuity quarterly", Value = "AnnuityQuarterly" },
            new StringValues { Text = "Annuity bi-annually", Value = "AnnuityBiAnnually" },
            new StringValues { Text = "Annuity two monthly", Value = "AnnuityTwoMonthly" },
            new StringValues { Text = "Annuity biweekly", Value = "AnnuityBiweekly" },
            new StringValues { Text = "Annuity annually", Value = "AnnuityAnnually" },
            new StringValues { Text = "Annuity monthly fact", Value = "AnnuityMonthlyFact" },
            new StringValues { Text = "Annuity quarterly fact", Value = "AnnuityQuarterlyFact" },
            new StringValues { Text = "Annuity bi-annually fact", Value = "AnnuityBiAnnuallyFact" },
            new StringValues { Text = "Differential quarterly", Value = "DifferentialQuarterly" }
        };
        }
        public List<StringValues> GetInterestRateTypes()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Percentage", Value = "Percentage" },
                new StringValues { Text = "Fixed", Value = "Fixed" },
            };
            return list;
        }
      
        public List<StringValues> GetLoanStatus()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Pending", Value = "Pending" },
                new StringValues { Text = "Rejected", Value = "Rejected" },
                new StringValues { Text = "Understudies", Value = "Understudies" },
                new StringValues { Text = "Approved", Value = "Approved" },
                new StringValues { Text = "Disbursed", Value = "Disbursed" },
                //new StringValues { Text = "CASH", Value = "CASH" },
                //new StringValues { Text = "CASH", Value = "CASH" }
            };
            return list;
        }
        public List<StringValues>GetPaymentSources()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "CASH", Value = "CASH" },
                new StringValues { Text = "CHECK", Value = "CHECK" },
                //new StringValues { Text = "CASH", Value = "CASH" },
                //new StringValues { Text = "CASH", Value = "CASH" }
            };
            return list;
        }
        public List<StringValues> GetCustomerProfile()
        {
            var list = new List<StringValues>
            {
                new StringValues { Text = "Individual", Value = "Individual" },
                new StringValues { Text = "Group", Value = "Group" },
                new StringValues { Text = "Organization", Value = "Organization" },
                //new StringValues { Text = "CASH", Value = "CASH" }
            };
            return list;
        }

        /// <summary>
        /// 1. M/d/yyyy
        /// 2. MM/dd/yyyy
        /// 3. dd/MMM/yyyy
        /// 4. MMM/dd/yyyy
        /// 5. MMMM/dd/yyyy
        /// 6. d/MM/yyyy
        /// 7. dd/MM/yyyy
        /// 8. ddd, dd/MMM/yyyy
        /// 9. dd/MMM/yyyy
        /// 10. dddd/MMMM/yyyy
        /// 0. MM/dd/yyyy
        /// </summary>
        /// <param Name="date"></param>
        /// <param Name="select"></param>
        /// <returns></returns>
        public string GetDateFormat(DateTime date, int select = 0)
        {

            switch (select)
            {
                case 1:
                    return date.ToString("M/d/yyyy");
                case 2:
                    return date.ToString("MM/dd/yyyy");
                case 3:
                    return date.ToString("dd/MMM/yyyy");
                case 4:
                    return date.ToString("MMM/dd/yyyy");
                case 5:
                    return date.ToString("MMMM/dd/yyyy");
                case 6:
                    return date.ToString("d/MM/yyyy");
                case 7:
                    return date.ToString("dd/MM/yyyy");
                case 8:
                    return date.ToString("ddd, dd/MMM/yyyy");
                case 9:
                    return date.ToString("dd/MMM/yyyy");
                case 10:
                    return date.ToString("dddd/MMMM/yyyy");
                default:
                    return date.ToString("dd/MM/yyyy HH:mm:ss");
            }



        }
        public bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        public string BlacklitedState(bool state)
        {
            if (state)
            {
                return "Black listed";
            }
            return "Active";
        }
        public string GetUserFullName()
        {
            var data = HttpContext.Current?.Session?["FullName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "None" : data;
        }

        public string GetUserName()
        {
            var data = HttpContext.Current?.Session?["UserName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "UnknownUser" : data;
        }

        public string GetSessionCode()
        {
            var data = HttpContext.Current?.Session?["SessionCode"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoSession" : data;
        }

        public string GetGroup()
        {
            var data = HttpContext.Current?.Session?["GroupName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoGroup" : data;
        }

        public string GetBankID()
        {
            var data = HttpContext.Current?.Session?["BankID"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? string.Empty : data;
        }

        public bool IsHeadOffice()
        {
            var value = HttpContext.Current?.Session?["IsHeadOffice"];
            return value is bool booleanValue && booleanValue;
        }

        public string GetBankCode()
        {
            var data = HttpContext.Current?.Session?["BankCode"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoBankCode" : data;
        }
        public string GetLanguage()
        {
            var data = HttpContext.Current?.Session?["SelectedLanguage"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "en" : data;
        }
        public string GetRoleId()
        {
            var data = HttpContext.Current?.Session?["RoleId"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoRoleId" : data;
        }

        public string GetBranchCode()
        {
            var data = HttpContext.Current?.Session?["BranchCode"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoBranchCode" : data;
        }

        public string GetBranchName()
        {
            var data = HttpContext.Current?.Session?["BranchName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoBranchName" : data;
        }

        public string GetBranchID()
        {
            var data = HttpContext.Current?.Session?["BranchID"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? string.Empty : data;
        }

        public bool IsMainBranch()
        {
            var value = HttpContext.Current?.Session?["IsHavingBank"];
            return value != null && Convert.ToBoolean(value);
        }

        public static string CleanTelephoneNumber(string phoneNumber)
        {
            // Remove any non-numeric characters
            string numericPhoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Check if the cleaned number has a length of 9
            if (numericPhoneNumber.Length == 9) 
            {
                return numericPhoneNumber;
            }
            else
            {
                throw new ArgumentException($"Invalid telephone number format or length: {phoneNumber}");
            }
        }

        // Helper method to check if a string consists of numeric characters only
        private static bool IsNumeric(string value)
        {
            foreach (char c in value)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
        public string GetOrganizationID()
        {
            string str = "";// HttpContext.Current.Session["OrganizationID"].ToString();
            return str;
        }
        public string GetUserToDoAction()
        {
            string str = HttpContext.Current.Session["userid_action"].ToString();
            return str;
        }


        public string GetBankName()
        {
            var data = HttpContext.Current?.Session?["BankName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "UnknownBank" : data;
        }

        public string GetField(string field)
        {
            if (string.IsNullOrWhiteSpace(field)) return "InvalidField";
            var data = HttpContext.Current?.Session?[field]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? $"NoValueFor:{field}" : data;
        }

        public string GetUserID()
        {
            var data = HttpContext.Current?.Session?["UserID"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoUser" : data;
        }

        public string GetGroupID()
        {
            var data = HttpContext.Current?.Session?["GroupID"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoGroupID" : data;
        }

        public string RearrangeRegistrationDateToDateFormatOld(string date, int state)
        {

            if (IsNumeric(date))
            {
                date = SplitDateStringYYYYMMDD(date);

            }
            string D = null;
            try
            {
                //string[] givenDatee = date.Split(new Char[] { '/', ' ', '-' });
                //if (date.Length.Equals(10))
                //{
                //    date = $"{givenDatee[1]}/{givenDatee[0]}/{givenDatee[2]}";
                //}
                string[] givenDate = date.Split(new Char[] { '/', ' ', '-' });

                if (state.Equals(1))
                {
                    if (givenDate[0].Length.Equals(4))
                    {
                        if (Convert.ToInt32(givenDate[2]) > 12)
                        {
                            D = $"{givenDate[2]}/{givenDate[1]}/{givenDate[0]}";
                        }
                        D = $"{givenDate[2]}/{givenDate[1]}/{givenDate[0]}";
                    }

                    else if (givenDate[1].Length.Equals(2))
                    {
                        if (Convert.ToInt32(givenDate[0]) > 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else
                        {
                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";

                        }
                    }
                    else if (givenDate[2].Length.Equals(1))
                    {
                        if (Convert.ToInt32(givenDate[0]) > 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else
                        {
                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";

                        }
                    }

                }
                else
                {
                    if (givenDate[0].Length.Equals(4))
                    {
                        if (Convert.ToInt32(givenDate[2]) > 12)
                        {
                            D = $"{givenDate[1]}/{givenDate[2]}/{givenDate[0]}";
                        }
                        D = $"{givenDate[2]}/{givenDate[1]}/{givenDate[0]}";
                    }

                    else if (givenDate[1].Length.Equals(2))
                    {
                        if (Convert.ToInt32(givenDate[0]) > 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else
                        {
                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";

                        }
                    }
                    else if (givenDate[2].Length.Equals(1))
                    {
                        if (Convert.ToInt32(givenDate[0]) > 12)
                        {
                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";

                        }
                        else
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";

                        }
                    }
                    // D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return D;
        }

        public int ConvertMonthstringToMonthIn(string monthStr)
        {
            int month = DateTime.ParseExact(monthStr, "MMMM", CultureInfo.CurrentCulture).Month;
            return month;
        }

        public string RearrangeRegistrationDateToDateFormat(string date, int state)
        {

            if (date == "9/6/2021")
            {

            }
            int day = CurrentDate.Day; int month = CurrentDate.Month;
            string D = null;
            if (date == null || date == "")
            {
                date = "01/01/0001";
            }


            if (IsNumeric(date))
            {

                date = SplitDateStringYYYYMMDD(date);

            }

            try
            {

                string[] givenDatee = date.Split(new Char[] { '/', ' ', '-', 'T' });
                if (givenDatee[0].Length.Equals(4))
                {
                    if (ConverToInteger(givenDatee[1]) >= 12)
                    {
                        date = $"{givenDatee[2]}/{givenDatee[1]}/{givenDatee[0]}";
                    }
                    else if (ConverToInteger(givenDatee[1]) < ConverToInteger(givenDatee[2]))
                    {

                        date = $"{givenDatee[2]}/{givenDatee[1]}/{givenDatee[0]}";
                    }
                    else
                    {
                        date = $"{givenDatee[1]}/{givenDatee[2]}/{givenDatee[0]}";
                    }


                }
                string[] givenDate = date.Split(new Char[] { '/', ' ', '-' });
                if (!IsNumeric(givenDate[1].ToString()))
                {
                    givenDate[1] = DateTime.ParseExact(givenDate[1], "MMM", CultureInfo.CurrentCulture).Month.ToString();

                }
                string newDate = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                if (state.Equals(1))
                {
                    if (culture == "en")
                    {
                        if (ConverToInteger(givenDate[0]) >= 12 && ConverToInteger(givenDate[1]) <= 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else if (ConverToInteger(givenDate[0]) <= 12 && ConverToInteger(givenDate[1]) <= 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else
                        {
                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                        }
                    }
                    else
                    {
                        if (ConverToInteger(givenDate[0]) >= 12 && ConverToInteger(givenDate[1]) <= 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else if (ConverToInteger(givenDate[0]) < 12)
                        {

                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";

                        }
                        else { D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}"; }
                    }
                    //if (ConverToInteger(givenDate[0]) >= 12 && ConverToInteger(givenDate[1]) <= 12)
                    //{
                    //    D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                    //}
                    //else
                    //{

                    //    D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                    //    if (ConverToInteger(givenDate[0]) >= month || ConverToInteger(givenDate[1]) >= month)
                    //    {
                    //        D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                    //    }
                    //    else
                    //    {
                    //        D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                    //    }
                    //}
                }
                else
                {

                    if (culture == "en")
                    {
                        if (ConverToInteger(givenDate[0]) >= 12 && ConverToInteger(givenDate[1]) <= 12)
                        {
                            D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                        }
                        //else if (ConverToInteger(givenDate[0]) <= 12 && ConverToInteger(givenDate[1]) <= 12)
                        //{
                        //    D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                        //}
                        else
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }


                    }
                    else
                    {
                        if (ConverToInteger(givenDate[0]) >= 12 && ConverToInteger(givenDate[1]) <= 12)
                        {
                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                        }
                        else if (ConverToInteger(givenDate[0]) < 12)
                        {

                            D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";

                        }
                        else { D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}"; }
                    }

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return D;
        }
        public DateTime ConvertStringToDate(string date)
        {


            if (date == null || date == "")
            {
                date = "01/01/0001";
            }
            string[] givenDatee = date.Split(new Char[] { '/', ' ', '-' });

            if (givenDatee[0].Length == 4)
            {
                date = $"{givenDatee[2]}/{givenDatee[1]}/{givenDatee[0]}";
            }
            else
            {
                date = $"{givenDatee[0]}/{givenDatee[1]}/{givenDatee[2]}";
            }
            if (givenDatee[0].Length == 4)
            {
                date = GetDateStringToSingle(date);
            }

            if (IsNumeric(date))
            {

                date = SplitDateStringYYYYMMDD(date);

            }
            DateTime DT = new DateTime();
            try
            {

                DT = Convert.ToDateTime(date);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return DT;
        }

        public DateTime RegistrationDateFormatConvertion(string date)
        {

            string D = null;
            DateTime DT = new DateTime();
            try
            {
                string[] givenDate = date.Split(new Char[] { '/', ' ', '-' });
                if (ConverToInteger(givenDate[0]) < 12)
                {
                    D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                }
                else if (ConverToInteger(givenDate[1]) > 12 || ConverToInteger(givenDate[0]) <= 12)
                {
                    D = $"{givenDate[0]}/{givenDate[1]}/{givenDate[2]}";
                }
                else
                {
                    D = $"{givenDate[1]}/{givenDate[0]}/{givenDate[2]}";
                }

                DT = Convert.ToDateTime(D);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return DT;
        }


        public string FixAddress(string address)
        {


            try
            {
                string[] address_ = address.Split(new Char[] { '=' });
                if (address_[0].Contains("ADDRESS_TYPE"))
                {
                    return address_[1];
                }
                return address;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        public void DeleteFile(string filepath)
        {

            if (IsFilePathExist(filepath))
            {
                File.Delete(filepath);
            }
        }


        //public ExecutionMessages ValidateFileExtension(string extention)
        //{

        //    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".gif", ".GIF", ".jpeg", ".PNG", ".JPG", ".JPEG", ".JPG", ".bmp", ".CSV", ".csv", ".xlsx", ".XLSX" };
        //    if (allowedExtensions.Contains(extention))
        //    {
        //        return GetExecutionMessages(true, "", MessagesResults.Success, ExecutionProcessOption.SearchCompleted, SystemMessageStatus.Success.ToString());

        //    }
        //    else
        //    {
        //        return GetExecutionMessages(false, "File", MessagesResults.Failed, ExecutionProcessOption.InvalidCSVFile, SystemMessageStatus.Failed.ToString());

        //    }
        //}
        public ImagesProperties GetFileProperties(string path, HttpPostedFileBase postedFileBase)
        {
            try
            {
                var guid = Guid.NewGuid();

                var properties = new ImagesProperties();
                if (path.Contains("~"))
                {
                    properties = new ImagesProperties { MappedPath = Path.Combine(HttpContext.Current.Server.MapPath($"{path}"), $""), Extension = Path.GetExtension(postedFileBase.FileName), FileName = Path.GetFileName(postedFileBase.FileName), FileID = guid, FullPath = $"{path}{guid}{Path.GetExtension(postedFileBase.FileName)}", Path = $"{path}{guid}{Path.GetExtension(postedFileBase.FileName)}" };
                    properties.Path = $"{properties.MappedPath}{guid}{Path.GetExtension(postedFileBase.FileName)}";
                    CreatePath(properties.MappedPath);
                }
                else
                {
                    CreatePath(path);
                    properties = new ImagesProperties { MappedPath = path, Extension = Path.GetExtension(postedFileBase.FileName), FileName = Path.GetFileName(postedFileBase.FileName), FileID = guid, FullPath = $"{path}\\{guid}{Path.GetExtension(postedFileBase.FileName)}", Path = $"{path}\\{guid}{Path.GetExtension(postedFileBase.FileName)}" };

                }
                return properties;
                //CreatePath(path);
                //var guid = Guid.NewGuid();
                //var properties = new ImagesProperties();
                //if (path.Contains("~"))
                //{
                //    properties = new ImagesProperties { MappedPath = Path.Combine(HttpContext.Current.Server.MapPath($"{path}/"), $"{guid}{Path.GetExtension(postedFileBase.FileName)}"), Extension = Path.GetExtension(postedFileBase.FileName), FileName = Path.GetFileName(postedFileBase.FileName), FileID = guid, FullPath = $"{path}/{guid}{Path.GetExtension(postedFileBase.FileName)}", Path = $"{path}/{guid}{Path.GetExtension(postedFileBase.FileName)}" };

                //}
                //else
                //{
                //    properties = new ImagesProperties { MappedPath = path, Extension = Path.GetExtension(postedFileBase.FileName), FileName = Path.GetFileName(postedFileBase.FileName), FileID = guid, FullPath = $"{path}/{guid}{Path.GetExtension(postedFileBase.FileName)}", Path = $"{path}/{guid}{Path.GetExtension(postedFileBase.FileName)}" };

                //}
                //return properties;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ImagesProperties UploadFile(string path, HttpPostedFileBase postedFileBase)
        {
            try
            {
                if (postedFileBase != null)
                {
                    ImagesProperty = GetFileProperties(path, postedFileBase);
                    if (ImagesProperty.FullPath.Contains("~"))
                    {
                        postedFileBase.SaveAs(ImagesProperty.Path);
                    }
                    else
                    {
                        postedFileBase.SaveAs(ImagesProperty.FullPath);
                    }

                }
                return ImagesProperty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //try
            //{
            //    if (postedFileBase != null)
            //    {
            //        ImagesProperty = GetFileProperties(path, postedFileBase);
            //        postedFileBase.SaveAs(ImagesProperty.FullPath);
            //    }
            //    return ImagesProperty;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

        }

        public bool IsFilePathExist(string fullPath)
        {

            return (File.Exists(fullPath)) ? true : false;

        }

        public string SaveFileInLocalDisk<T>(List<T> items, string pathToSAveFile, string pathDirectory)
        {
            CreatePath(pathDirectory);
            var DT = ToDataTable(items);
            ExportWriter.ToCSV(DT, pathToSAveFile);
            return pathToSAveFile;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                values[0] = values[0].ToString().Replace("\r", "");
                dataTable.Rows.Add(values);

            }

            //put a breakpoint here and check datatable

            return dataTable;

        }

    }


    public static class ExportWriter
    {

        public static void ToCSV(this DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);

            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

    }
    public class RoundUp
    {
        public double RoundUpAmount { get; set; }
        public double ExcessAmount { get; set; }
        public bool HasExcessAmount { get; set; }
        public int AmountPaid { get; set; }
    }
    public class CsvExportHelper
    {
        public static StringBuilder ExportData<T>(IEnumerable<T> list)
        {
            var stringBuilder = new StringBuilder();
            //Create header part
            var headerProperties = typeof(T).GetProperties();
            for (int i = 0; i < headerProperties.Length - 1; i++)
            {
                stringBuilder.Append(headerProperties[i].Name + ",");
            }
            var lastProp = headerProperties[headerProperties.Length - 1].Name;
            stringBuilder.Append(lastProp + Environment.NewLine);
            //Create Rows
            foreach (var item in list)
            {
                var rowValues = typeof(T).GetProperties();
                for (int i = 0; i < rowValues.Length - 1; i++)
                {
                    var prop = rowValues[i];
                    stringBuilder.Append(prop.GetValue(item) + ",");
                }
                stringBuilder.Append(rowValues[rowValues.Length - 1].GetValue(item) + Environment.NewLine);
            }
            return stringBuilder;
        }



        public void WriteTsv<T>(IEnumerable<T> data, TextWriter output)
        {

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                output.Write(prop.DisplayName); // header
                output.Write("\t");
            }
            output.WriteLine();
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    output.Write(prop.Converter.ConvertToString(
                         prop.GetValue(item)));
                    output.Write("\t");
                }
                output.WriteLine();
            }
        }

    }

}
