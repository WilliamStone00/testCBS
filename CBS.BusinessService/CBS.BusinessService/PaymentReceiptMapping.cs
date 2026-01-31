using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;

namespace CBS.BusinessService
{


    public static class PaymentReceiptMapping
    {

        public static List<PaymentReciptDS> MapPaymentReceipt(PaymentReceipt paymentReceipt, Branch branch)
        {
            if (paymentReceipt == null) throw new ArgumentNullException(nameof(paymentReceipt));
            if (branch == null) throw new ArgumentNullException(nameof(branch));
            if (branch.Bank == null) throw new ArgumentNullException(nameof(branch.Bank));

            // --- Barcode ---
            string barcodeData = $"{branch.Bank.BankCode}|{paymentReceipt.InternalReferenceNumber}|{paymentReceipt.MemberReference}|{paymentReceipt.Amount:N1}";
            string barcodeImagePath = GenerateAndSaveBarcodeImage(barcodeData, paymentReceipt.MemberName, paymentReceipt.InternalReferenceNumber, branch.Name);

            // --- Helpers & keyword sets ---
            var subscriptionKeywords = new[]
            {
                "membership", "entrance fee", "byelaws", "bye laws", "building contribution", "sbc",
                "loan policy", "registration", "adhesion", "adhésion", "inscription", "cotisation"
            };

            var memberShareKeywords = new[]
            {
                "membershare", "member share", "share capital", "share(s)", "shares", "share"
            };

            // Keep balances for these on subscription receipts
            var savingsKeywords = new[]
            {
                "saving", "savings", "regular saving", "savings account", "compte epargne"
            };
            var depositKeywords = new[]
            {
                "deposit", "term deposit", "time deposit", "fixed deposit", "compte depot"
            };

            bool ContainsAny(string text, string[] keys) =>
                !string.IsNullOrWhiteSpace(text) &&
                keys.Any(k => text.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

            bool IsMemberShare(string name) =>
                !string.IsNullOrWhiteSpace(name)
                && ContainsAny(name, memberShareKeywords)
                && name.IndexOf("loan", StringComparison.OrdinalIgnoreCase) < 0;

            bool IsSavings(string name) => ContainsAny(name, savingsKeywords);
            bool IsDeposit(string name) => ContainsAny(name, depositKeywords);
            bool IsBalanceBearingAccount(string name) => IsMemberShare(name) || IsSavings(name) || IsDeposit(name);

            decimal ParseDec(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return 0m;
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out v)) return v;
                return 0m;
            }

            var serviceNames = (paymentReceipt.PaymentDetails ?? Enumerable.Empty<PaymentDetail>())
                               .Select(d => d.SericeName ?? string.Empty)
                               .ToList();

            bool isSubscriptionReceipt = serviceNames.Any(n => ContainsAny(n, subscriptionKeywords));

            // --- Denominations (filter zeros) ---
            var denominations = new List<DenominationDS>
            {
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "10000 Note", Quantity = paymentReceipt.Note10000, Value = 10000m * paymentReceipt.Note10000, DenominationType = "Notes" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "5000 Note",  Quantity = paymentReceipt.Note5000,  Value =  5000m * paymentReceipt.Note5000,  DenominationType = "Notes" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "2000 Note",  Quantity = paymentReceipt.Note2000,  Value =  2000m * paymentReceipt.Note2000,  DenominationType = "Notes" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "1000 Note",  Quantity = paymentReceipt.Note1000,  Value =  1000m * paymentReceipt.Note1000,  DenominationType = "Notes" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "500 Note",   Quantity = paymentReceipt.Note500,   Value =   500m * paymentReceipt.Note500,   DenominationType = "Notes" },

                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "500 Coin", Quantity = paymentReceipt.Coin500, Value = 500m * paymentReceipt.Coin500, DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "350 Coin", Quantity = paymentReceipt.Coin350, Value = 350m * paymentReceipt.Coin350, DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "250 Coin", Quantity = paymentReceipt.Coin250, Value = 250m * paymentReceipt.Coin250, DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "200 Coin", Quantity = paymentReceipt.Coin200, Value = 200m * paymentReceipt.Coin200, DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "150 Coin", Quantity = paymentReceipt.Coin150, Value = 150m * paymentReceipt.Coin150, DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "100 Coin", Quantity = paymentReceipt.Coin100, Value = 100m * paymentReceipt.Coin100, DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "50 Coin",  Quantity = paymentReceipt.Coin50,  Value =  50m * paymentReceipt.Coin50,  DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "25 Coin",  Quantity = paymentReceipt.Coin25,  Value =  25m * paymentReceipt.Coin25,  DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "10 Coin",  Quantity = paymentReceipt.Coin10,  Value =  10m * paymentReceipt.Coin10,  DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "5 Coin",   Quantity = paymentReceipt.Coin5,   Value =   5m * paymentReceipt.Coin5,   DenominationType = "Coins" },
                new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "1 Coin",   Quantity = paymentReceipt.Coin1,   Value =   1m * paymentReceipt.Coin1,   DenominationType = "Coins" }
            }
            .Where(d => d.Value > 0)
            .ToList();

            // --- Details: for subscription, zero only NON-balance-bearing lines ---
            var paymentDetails = (paymentReceipt.PaymentDetails ?? Enumerable.Empty<PaymentDetail>())
                .Select(pd =>
                {
                    var name = pd.SericeName;
                    var balance = pd.AccountBalance; // string

                    if (isSubscriptionReceipt)
                    {
                        // Keep balances for Member Share, Savings, Deposit; zero others
                        balance = IsBalanceBearingAccount(name) ? balance : "0";
                    }

                    return new PaymentDetailDS
                    {
                        Id = pd.Id,
                        MemberName = pd.MemberName,
                        MemberReference = pd.MemberReference,
                        PaymentReceiptId = pd.PaymentReceiptId,
                        SericeName = name,
                        AccountBalance = balance, // string
                        Amount = pd.Amount,
                        Fee = pd.Fee,
                        LoanCapital = pd.LoanCapital,
                        Interest = pd.Interest,
                        VAT = pd.VAT,
                        AccountNumber = pd.AccountNumber,
                        AccountingDay = pd.AccountingDay,
                        Date = pd.Date
                    };
                })
                .ToList();

            // --- Total Balance (string) ---
            // On subscription receipts, sum balances for Member Share + Savings + Deposit
            string totalBalanceDisplay = isSubscriptionReceipt
                ? paymentDetails
                    .Where(d => IsBalanceBearingAccount(d.SericeName))
                    .Sum(d => ParseDec(d.AccountBalance))
                    .ToString("N1", CultureInfo.InvariantCulture)
                : (string.IsNullOrWhiteSpace(paymentReceipt.TotalAccountBalances)
                    ? "0"
                    : paymentReceipt.TotalAccountBalances);
            // --- Logo & WaterMark (LOCAL CACHED PATHS) ---
            string logoImagePath = GenerateAndSaveBankLogoImage(branch.Bank.LogoUrl, branch.Name);
            string waterMarkImagePath = GenerateAndSaveBankWaterMarkImage(branch.Bank.WaterMarkUrl, branch.Name);
            return new List<PaymentReciptDS>
    {
        new PaymentReciptDS
        {
            Id = paymentReceipt.Id,
            MemberName = paymentReceipt.MemberName,
            MemberReference = paymentReceipt.MemberReference,
            Amount = paymentReceipt.Amount,
            ReceiptType = paymentReceipt.ReceiptType,

            // Computed total for subscription; otherwise original DB string
            TotalAccountBalances = totalBalanceDisplay,

            Charges = paymentReceipt.Charges,
            TotalAmount = paymentReceipt.TotalAmount,
            AmountInWord = paymentReceipt.AmountInWord,
            ReceiptTitle = paymentReceipt.ReceiptTitle,
            CashierName = paymentReceipt.CashierName,
            TillName = paymentReceipt.TillName,
            ServiceType = paymentReceipt.ServiceType,
            OperationType = paymentReceipt.OperationType,
            OperationTypeGrouping = paymentReceipt.OperationTypeGrouping,
            AccountingDay = paymentReceipt.AccountingDay,
            Date = paymentReceipt.Date,
            BarcodeData = barcodeData,
            BarcodeImagePath = barcodeImagePath,
            InternalReferenceNumber = paymentReceipt.InternalReferenceNumber,
            ExternalReferenceNumber = paymentReceipt.ExternalReferenceNumber,
            SourceOfRequest = paymentReceipt.SourceOfRequest,
            PortalUsed = paymentReceipt.PortalUsed,

            DenominationDs = denominations,
            PaymentDetailDs = paymentDetails,

            DepositorCNI = paymentReceipt.DepositorCNI,
            DepositorName = paymentReceipt.DepositorName,
            DepositorPhone = paymentReceipt.DepositorPhone,

            Logo = logoImagePath,
            BranchAddress = branch.Address,
            BranchCode = branch.BranchCode,
            BranchName = branch.Name,
            BranchTelephone = branch.Telephone,

            HeadOfficeAddress = branch.Bank.Address,
            HeadOfficeCode = branch.Bank.BankCode,
            HeadOfficeEmail = branch.Bank.Email,
            HeadOfficeInitial = branch.Bank.BankInitial,
            HeadOfficeName = branch.Bank.Name,
            HeadOfficeTelephone = branch.Bank.Telephone,
            HeadOfficeWebSite = branch.Bank.WebSite,

            HeadOfficeBankInitial = branch.Bank.BankInitial,
            HeadOfficeCategoryInformation = branch.Bank.CategoryInformation,
            HeadOfficeCustomerServiceContact = branch.Bank.CustomerServiceContact,
            HeadOfficeFax = branch.Bank.Fax,
            HeadOfficeImmatriculationNumber = branch.Bank.ImmatriculationNumber,
            HeadOfficeMotto = branch.Bank.Motto,
            HeadOfficePBox = branch.Bank.PBox,
            HeadOfficeRegistrationInformation = branch.Bank.RegistrationInformation,
            HeadOfficeRegistrationNumber = branch.Bank.RegistrationInformation,
            HeadOfficeShortHeaderInfo = branch.Bank.RegistrationInformation,
            HeadOfficeWaterMark = waterMarkImagePath,
        }
    };
        }

        public static List<PaymentReciptDS> MapPaymentReceiptxx(PaymentReceipt paymentReceipt, Branch branch)
        {
            if (paymentReceipt == null) throw new ArgumentNullException(nameof(paymentReceipt));
            if (branch == null) throw new ArgumentNullException(nameof(branch));
            if (branch.Bank == null) throw new ArgumentNullException(nameof(branch.Bank));

            // --- Barcode ---
            string barcodeData = $"{branch.Bank.BankCode}|{paymentReceipt.InternalReferenceNumber}|{paymentReceipt.MemberReference}|{paymentReceipt.Amount:N1}";
            string barcodeImagePath = GenerateAndSaveBarcodeImage(barcodeData, paymentReceipt.MemberName, paymentReceipt.InternalReferenceNumber, branch.Name);

            // --- Helpers & keyword sets ---
            var subscriptionKeywords = new[]
            {
        "membership", "entrance fee", "byelaws", "bye laws", "building contribution", "sbc",
        "loan policy", "registration", "adhesion", "adhésion", "inscription", "cotisation"
    };

            var memberShareKeywords = new[]
            {
        "membershare", "member share", "share capital", "share(s)", "shares", "share"
    };

            bool ContainsAny(string text, string[] keys) =>
                !string.IsNullOrWhiteSpace(text) &&
                keys.Any(k => text.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

            bool IsMemberShare(string name) =>
                !string.IsNullOrWhiteSpace(name)
                && ContainsAny(name, memberShareKeywords)
                && name.IndexOf("loan", StringComparison.OrdinalIgnoreCase) < 0;

            decimal ParseDec(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return 0m;
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out v)) return v;
                return 0m;
            }

            var serviceNames = (paymentReceipt.PaymentDetails ?? Enumerable.Empty<PaymentDetail>())
                               .Select(d => d.SericeName ?? string.Empty)
                               .ToList();

            bool isSubscriptionReceipt = serviceNames.Any(n => ContainsAny(n, subscriptionKeywords));

            // --- Denominations (filter zeros) ---
            var denominations = new List<DenominationDS>
    {
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "10000 Note", Quantity = paymentReceipt.Note10000, Value = 10000m * paymentReceipt.Note10000, DenominationType = "Notes" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "5000 Note",  Quantity = paymentReceipt.Note5000,  Value =  5000m * paymentReceipt.Note5000,  DenominationType = "Notes" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "2000 Note",  Quantity = paymentReceipt.Note2000,  Value =  2000m * paymentReceipt.Note2000,  DenominationType = "Notes" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "1000 Note",  Quantity = paymentReceipt.Note1000,  Value =  1000m * paymentReceipt.Note1000,  DenominationType = "Notes" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "500 Note",   Quantity = paymentReceipt.Note500,   Value =   500m * paymentReceipt.Note500,   DenominationType = "Notes" },

        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "500 Coin", Quantity = paymentReceipt.Coin500, Value = 500m * paymentReceipt.Coin500, DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "350 Coin", Quantity = paymentReceipt.Coin350, Value = 350m * paymentReceipt.Coin350, DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "250 Coin", Quantity = paymentReceipt.Coin250, Value = 250m * paymentReceipt.Coin250, DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "200 Coin", Quantity = paymentReceipt.Coin200, Value = 200m * paymentReceipt.Coin200, DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "150 Coin", Quantity = paymentReceipt.Coin150, Value = 150m * paymentReceipt.Coin150, DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "100 Coin", Quantity = paymentReceipt.Coin100, Value = 100m * paymentReceipt.Coin100, DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "50 Coin",  Quantity = paymentReceipt.Coin50,  Value =  50m * paymentReceipt.Coin50,  DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "25 Coin",  Quantity = paymentReceipt.Coin25,  Value =  25m * paymentReceipt.Coin25,  DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "10 Coin",  Quantity = paymentReceipt.Coin10,  Value =  10m * paymentReceipt.Coin10,  DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "5 Coin",   Quantity = paymentReceipt.Coin5,   Value =   5m * paymentReceipt.Coin5,   DenominationType = "Coins" },
        new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "1 Coin",   Quantity = paymentReceipt.Coin1,   Value =   1m * paymentReceipt.Coin1,   DenominationType = "Coins" }
    }
            .Where(d => d.Value > 0)
            .ToList();

            // --- Details: for subscription, zero non-MemberShare balances (strings) ---
            var paymentDetails = (paymentReceipt.PaymentDetails ?? Enumerable.Empty<PaymentDetail>())
                .Select(pd =>
                {
                    var name = pd.SericeName;
                    var balance = pd.AccountBalance; // string

                    if (isSubscriptionReceipt)
                    {
                        balance = IsMemberShare(name) ? balance : "0";
                    }

                    return new PaymentDetailDS
                    {
                        Id = pd.Id,
                        MemberName = pd.MemberName,
                        MemberReference = pd.MemberReference,
                        PaymentReceiptId = pd.PaymentReceiptId,
                        SericeName = name,
                        AccountBalance = balance, // string
                        Amount = pd.Amount,
                        Fee = pd.Fee,
                        LoanCapital = pd.LoanCapital,
                        Interest = pd.Interest,
                        VAT = pd.VAT,
                        AccountNumber = pd.AccountNumber,
                        AccountingDay = pd.AccountingDay,
                        Date = pd.Date
                    };
                })
                .ToList();

            // --- Total Balance (string) ---
            string totalBalanceDisplay = isSubscriptionReceipt
                // Sum only MemberShare lines (parse strings safely), then format
                ? paymentDetails
                    .Where(d => IsMemberShare(d.SericeName))
                    .Sum(d => ParseDec(d.AccountBalance))
                    .ToString("N1", CultureInfo.InvariantCulture)
                // Non-subscription: keep what came from DB (could be "******")
                : (string.IsNullOrWhiteSpace(paymentReceipt.TotalAccountBalances)
                    ? "0"
                    : paymentReceipt.TotalAccountBalances);

            return new List<PaymentReciptDS>
    {
        new PaymentReciptDS
        {
            Id = paymentReceipt.Id,
            MemberName = paymentReceipt.MemberName,
            MemberReference = paymentReceipt.MemberReference,
            Amount = paymentReceipt.Amount,
            ReceiptType = paymentReceipt.ReceiptType,

            // ⬇️ Computed MemberShare-only total (string) when subscription;
            // otherwise, original DB string (keeps masking)
            TotalAccountBalances = totalBalanceDisplay,

            Charges = paymentReceipt.Charges,
            TotalAmount = paymentReceipt.TotalAmount,
            AmountInWord = paymentReceipt.AmountInWord,
            ReceiptTitle = paymentReceipt.ReceiptTitle,
            CashierName = paymentReceipt.CashierName,
            TillName = paymentReceipt.TillName,
            ServiceType = paymentReceipt.ServiceType,
            OperationType = paymentReceipt.OperationType,
            OperationTypeGrouping = paymentReceipt.OperationTypeGrouping,
            AccountingDay = paymentReceipt.AccountingDay,
            Date = paymentReceipt.Date,
            BarcodeData = barcodeData,
            BarcodeImagePath = barcodeImagePath,
            InternalReferenceNumber = paymentReceipt.InternalReferenceNumber,
            ExternalReferenceNumber = paymentReceipt.ExternalReferenceNumber,
            SourceOfRequest = paymentReceipt.SourceOfRequest,
            PortalUsed = paymentReceipt.PortalUsed,

            DenominationDs = denominations,
            PaymentDetailDs = paymentDetails,

            DepositorCNI = paymentReceipt.DepositorCNI,
            DepositorName = paymentReceipt.DepositorName,
            DepositorPhone = paymentReceipt.DepositorPhone,

            Logo = branch.Bank.LogoUrl,
            BranchAddress = branch.Address,
            BranchCode = branch.BranchCode,
            BranchName = branch.Name,
            BranchTelephone = branch.Telephone,

            HeadOfficeAddress = branch.Bank.Address,
            HeadOfficeCode = branch.Bank.BankCode,
            HeadOfficeEmail = branch.Bank.Email,
            HeadOfficeInitial = branch.Bank.BankInitial,
            HeadOfficeName = branch.Bank.Name,
            HeadOfficeTelephone = branch.Bank.Telephone,
            HeadOfficeWebSite = branch.Bank.WebSite,

            HeadOfficeBankInitial = branch.Bank.BankInitial,
            HeadOfficeCategoryInformation = branch.Bank.CategoryInformation,
            HeadOfficeCustomerServiceContact = branch.Bank.CustomerServiceContact,
            HeadOfficeFax = branch.Bank.Fax,
            HeadOfficeImmatriculationNumber = branch.Bank.ImmatriculationNumber,
            HeadOfficeMotto = branch.Bank.Motto,
            HeadOfficePBox = branch.Bank.PBox,
            HeadOfficeRegistrationInformation = branch.Bank.RegistrationInformation,
            HeadOfficeRegistrationNumber = branch.Bank.RegistrationInformation,
            HeadOfficeShortHeaderInfo = branch.Bank.RegistrationInformation,
            HeadOfficeWaterMark = branch.Bank.WaterMarkUrl,
        }
    };
        }






        public static string GenerateAndSaveBarcodeImage(string data, string memberName, string transactionRef, string branchName)
        {
            string fullPath = string.Empty;

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 0,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(data);

            using (Bitmap fullBitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppArgb))
            {
                var bitmapData = fullBitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                fullBitmap.UnlockBits(bitmapData);

                using (Bitmap trimmed = TrimBitmap(fullBitmap))
                {
                    try
                    {
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string year = DateTime.Now.Year.ToString();

                        string sanitizedBranch = SanitizeFileName(branchName);
                        string sanitizedMember = SanitizeFileName(memberName);

                        string folderPath = Path.Combine(baseDir, "AppFiles", "BarCodeImages", year, sanitizedBranch);
                        Directory.CreateDirectory(folderPath); // ensures all folders exist

                        string fileName = $"{sanitizedMember}_{transactionRef}_{DateTime.Now:yyyyMMddHHmmss}.png";
                        fullPath = Path.Combine(folderPath, fileName);

                        if (File.Exists(fullPath))
                            File.Delete(fullPath);

                        trimmed.Save(fullPath, ImageFormat.Png);
                        return fullPath;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GDI+] Error saving barcode image: {ex.Message}");
                        return fullPath;
                    }
                }
            }
        }

        public static string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input.Replace(" ", "_");
        }

        public static Bitmap TrimBitmap(Bitmap source)
        {
            int minX = source.Width;
            int minY = source.Height;
            int maxX = 0;
            int maxY = 0;

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color pixel = source.GetPixel(x, y);
                    if (pixel.A > 0 && pixel.ToArgb() != Color.White.ToArgb())
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (minX >= maxX || minY >= maxY)
                return new Bitmap(1, 1); // Blank fallback

            Rectangle crop = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            return source.Clone(crop, source.PixelFormat);
        }





        private static string NormalizeImagePath(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            // Remove extra quotes and whitespace from DB/url
            var v = raw.Trim().Trim('"', '\'');
            return v;
        }
        public static string GenerateAndSaveBankLogoImage(string rawLogoUrl, string branchName)
    => GenerateAndSaveBankImage(rawLogoUrl, branchName, "BankLogos", "BankLogo");

        public static string GenerateAndSaveBankWaterMarkImage(string rawWaterMarkUrl, string branchName)
            => GenerateAndSaveBankImage(rawWaterMarkUrl, branchName, "BankWaterMarks", "BankWaterMark");
        private static string GenerateAndSaveBankImage(
    string rawUrl,
    string branchName,
    string imageSubFolder,      // e.g. "BankLogos", "BankWaterMarks"
    string defaultNamePrefix)   // e.g. "BankLogo", "BankWaterMark"
        {
            try
            {
                string imgUrl = NormalizeImagePath(rawUrl);
                if (string.IsNullOrWhiteSpace(imgUrl))
                    return string.Empty;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string year = DateTime.Now.Year.ToString();
                string sanitizedBranch = SanitizeFileName(branchName ?? "UnknownBranch");

                // ------------------------------------------------------------------
                // 1) Resolve special case: ~/AppFiles/Images/noimage.jpg  (placeholder)
                // ------------------------------------------------------------------
                string imagesRoot = Path.Combine(baseDir, "AppFiles", "Images");

                // Normalize slashes to make checking easier
                string urlForCheck = imgUrl.Replace("\\", "/");

                bool isNoImagePlaceholder =
                    urlForCheck.EndsWith("/AppFiles/Images/noimage.jpg", StringComparison.OrdinalIgnoreCase) ||
                    urlForCheck.EndsWith("/AppFiles/Images/noimage.JPG", StringComparison.OrdinalIgnoreCase) ||
                    urlForCheck.IndexOf("noimage.jpg", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    urlForCheck.IndexOf("noimage.JPG", StringComparison.OrdinalIgnoreCase) >= 0;

                if (isNoImagePlaceholder)
                {
                    // If it's a watermark, use your real watermark design as default
                    if (string.Equals(defaultNamePrefix, "BankWaterMark", StringComparison.OrdinalIgnoreCase))
                    {
                        // Default watermark file
                        imgUrl = Path.Combine(imagesRoot,
                            "Screenshot_2025-09-01_010309-removebg-preview-removebg-preview (2).PNG");
                    }
                    else
                    {
                        // Default logo/no-image
                        imgUrl = Path.Combine(imagesRoot, "noimage.JPG");
                    }
                }
                else
                {
                    // ------------------------------------------------------------------
                    // 2) Handle application-relative and simple relative paths
                    //    e.g. ~/AppFiles/Images/logo.png  or  AppFiles/Images/logo.png
                    // ------------------------------------------------------------------
                    if (urlForCheck.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
                    {
                        // "~/AppFiles/Images/xxx"  ->  <baseDir>\AppFiles\Images\xxx
                        var rel = urlForCheck.Substring(2).TrimStart('/', '\\');
                        imgUrl = Path.Combine(baseDir, rel.Replace('/', '\\'));
                    }
                    else if (urlForCheck.StartsWith("/AppFiles/", StringComparison.OrdinalIgnoreCase))
                    {
                        // "/AppFiles/Images/xxx"  ->  <baseDir>\AppFiles\Images\xxx
                        var rel = urlForCheck.TrimStart('/', '\\');
                        imgUrl = Path.Combine(baseDir, rel.Replace('/', '\\'));
                    }
                    else if (!imgUrl.Contains(":") && !imgUrl.StartsWith("\\\\"))
                    {
                        // Plain relative path like "AppFiles/Images/xxx"
                        imgUrl = Path.Combine(baseDir, imgUrl.Replace('/', '\\'));
                    }
                }

                // ------------------------------------------------------------------
                // 3) Prepare destination folder where Crystal will read from
                // ------------------------------------------------------------------
                // Folder: AppFiles\BankLogos\2025\BAPCCUL_Bafia  or  AppFiles\BankWaterMarks\2025\BAPCCUL_Bafia
                string folderPath = Path.Combine(baseDir, "AppFiles", imageSubFolder, year, sanitizedBranch);
                Directory.CreateDirectory(folderPath);

                string fileName;

                try
                {
                    // If URL, extract last segment
                    var uri = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
                    if (uri.IsAbsoluteUri)
                    {
                        fileName = Path.GetFileName(uri.LocalPath);
                    }
                    else
                    {
                        fileName = Path.GetFileName(imgUrl);
                    }
                }
                catch
                {
                    // Fallback if parsing fails
                    fileName = $"{sanitizedBranch}_{defaultNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}.png";
                }

                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"{sanitizedBranch}_{defaultNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}.png";

                string fullPath = Path.Combine(folderPath, fileName);

                // If already cached, reuse
                if (File.Exists(fullPath))
                    return fullPath;

                // ------------------------------------------------------------------
                // 4) HTTP/HTTPS URL → download to local cache
                // ------------------------------------------------------------------
                if (imgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    using (var http = new HttpClient())
                    {
                        var bytes = http.GetByteArrayAsync(imgUrl).GetAwaiter().GetResult();
                        File.WriteAllBytes(fullPath, bytes);
                    }
                    return fullPath;
                }

                // ------------------------------------------------------------------
                // 5) Local or UNC path → copy into our cache folder
                // ------------------------------------------------------------------
                if (File.Exists(imgUrl))
                {
                    File.Copy(imgUrl, fullPath, overwrite: true);
                    return fullPath;
                }

                // Fallback: nothing worked
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BankImage] Error saving image ({imageSubFolder}): {ex.Message}");
                return string.Empty;
            }
        }

    }



}
