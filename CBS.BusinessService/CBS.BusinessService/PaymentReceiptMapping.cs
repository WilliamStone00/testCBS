using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System;
using System.Collections.Generic;
using System.Linq;
using CBS.FrontDesk.Data.Entity.Config;
using System.Drawing;
using System.IO;
using ZXing;
using ZXing.Common;

namespace CBS.BusinessService
{


    public static class PaymentReceiptMapping
    {
        public static List<PaymentReciptDS> MapPaymentReceipt(PaymentReceipt paymentReceipt, Branch branch)
        {
            // Validate input
            if (paymentReceipt == null)
                throw new ArgumentNullException(nameof(paymentReceipt));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            string barcodeData = $"{branch.Bank.BankCode}|{paymentReceipt.InternalReferenceNumber}|{paymentReceipt.MemberReference}|{paymentReceipt.Amount:N1}";
           string barcodeImagePath = GenerateAndSaveBarcodeImage(barcodeData, paymentReceipt.MemberName, paymentReceipt.InternalReferenceNumber, branch.Name);

            // Generate DenominationDS list and filter out denominations with zero value
        var denominations = new List<DenominationDS>
        {
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "10000 Note", Quantity = paymentReceipt.Note10000, Value = 10000m * paymentReceipt.Note10000, DenominationType = "Notes" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "5000 Note", Quantity = paymentReceipt.Note5000, Value = 5000m * paymentReceipt.Note5000, DenominationType = "Notes" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "2000 Note", Quantity = paymentReceipt.Note2000, Value = 2000m * paymentReceipt.Note2000, DenominationType = "Notes" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "1000 Note", Quantity = paymentReceipt.Note1000, Value = 1000m * paymentReceipt.Note1000, DenominationType = "Notes" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "500 Note", Quantity = paymentReceipt.Note500, Value = 500m * paymentReceipt.Note500, DenominationType = "Notes" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "500 Coin", Quantity = paymentReceipt.Coin500, Value = 500m * paymentReceipt.Coin500, DenominationType = "Coins" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "100 Coin", Quantity = paymentReceipt.Coin100, Value = 100m * paymentReceipt.Coin100, DenominationType = "Coins" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "50 Coin", Quantity = paymentReceipt.Coin50, Value = 50m * paymentReceipt.Coin50, DenominationType = "Coins" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "25 Coin", Quantity = paymentReceipt.Coin25, Value = 25m * paymentReceipt.Coin25, DenominationType = "Coins" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "10 Coin", Quantity = paymentReceipt.Coin10, Value = 10m * paymentReceipt.Coin10, DenominationType = "Coins" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "5 Coin", Quantity = paymentReceipt.Coin5, Value = 5m * paymentReceipt.Coin5, DenominationType = "Coins" },
            new DenominationDS { PaymentReceiptId = paymentReceipt.Id, NoteOrCoins = "1 Coin", Quantity = paymentReceipt.Coin1, Value = 1m * paymentReceipt.Coin1, DenominationType = "Coins" }
        }
            .Where(d => d.Value > 0)
            .ToList();

            // Generate PaymentDetailDS
            var paymentDetails = paymentReceipt.PaymentDetails?.Select(pd => new PaymentDetailDS
            {
                Id = pd.Id,
                MemberName = pd.MemberName,
                MemberReference = pd.MemberReference,
                PaymentReceiptId = pd.PaymentReceiptId,
                SericeName = pd.SericeName,
                Amount = pd.Amount,
                Fee = pd.Fee,
                LoanCapital = pd.LoanCapital,
                Interest = pd.Interest,
                VAT = pd.VAT,
                AccountNumber = pd.AccountNumber,
                AccountingDay = pd.AccountingDay,
                Date = pd.Date
            }).ToList() ?? new List<PaymentDetailDS>();

            return new List<PaymentReciptDS>
        {
            new PaymentReciptDS
            {
                Id = paymentReceipt.Id,
                MemberName = paymentReceipt.MemberName,
                MemberReference = paymentReceipt.MemberReference,
                Amount = paymentReceipt.Amount,
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
                DepositorCNI = paymentReceipt.DepositorCNI,
                DepositorName = paymentReceipt.DepositorName,
                DepositorPhone = paymentReceipt.DepositorPhone,
                PaymentDetailDs = paymentDetails,
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
                HeadOfficeWebSite = branch.Bank.WebSite
            }
        };
        }

        public static string GenerateAndSaveBarcodeImage(string data, string memberName, string transactionRef, string branchName)
        {
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

            using (Bitmap fullBitmap = new Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                // Copy raw pixels
                var bitmapData = fullBitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                fullBitmap.UnlockBits(bitmapData);

                // Trim whitespace
                Bitmap trimmed = TrimBitmap(fullBitmap);

                // Create full path
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string year = DateTime.Now.Year.ToString();
                string sanitizedBranch = branchName.Replace(" ", "_");
                string sanitizedMember = memberName.Replace(" ", "_");
                string folderPath = Path.Combine(baseDir, "AppFiles", "BarCodeImages", year, sanitizedBranch);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"{sanitizedMember}_{transactionRef}_{DateTime.Now:yyyyMMddHHmmss}.png";
                string fullPath = Path.Combine(folderPath, fileName);

                // 🔁 Check if file exists — delete before recreating
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                // 💾 Save trimmed, compressed PNG
                trimmed.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
                trimmed.Dispose();

                return fullPath;
            }
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

    }



}
