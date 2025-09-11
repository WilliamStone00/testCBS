using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class ReportHeader
    {
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchAddress { get; set; }
        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string BranchCode { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string MainAccountNumber { get; set; }
    }
    public class AccountingEntriesReport : ReportHeader
    {
        public string Message { get; set; }
        public List<AccountingEntryDto> AccountingEntries { get; set; }

        public List<JournalEntryDto>  BuildJournalEntry(AccountingEntriesReport Entries,string username)
        {
            List<JournalEntryDto> listDto = new List<JournalEntryDto>();
            decimal SumDebit = 0;
            decimal SumCredit = 0;
            int TotalOperations = 0;
            int NumberDebit = 0;
            int NumberCredit = 0;
            foreach (var account in Entries.AccountingEntries)
            {
 
                JournalEntryDto dto = new JournalEntryDto();
                dto.AccountNumber = account.AccountNumber;
                dto.Description = account.Naration;
                dto.ValueDate = account.ValueDate.ToString("dd-MM-yyyy HH:mm:ss");
                dto.Reference = account.ReferenceID.ToString();
                dto.EntryDate = account.EntryDate.ToString("dd-MM-yyyy HH:mm:ss");
                dto.Address = this.Address;
                dto.BranchLocation = this.Location;
                dto.Location = this.Location;
                dto.Capital = this.Capital;
                dto.WebSite = this.WebSite;
                dto.BranchTelephone = this.BranchTelephone;
                dto.ImmatriculationNumber = this.ImmatriculationNumber;
                dto.Name = this.Name;
                dto.Auxilary = account.Representative;
                dto.PrintersName = username;
                dto.BranchCode = this.BranchCode;
                dto.BranchName = this.BranchName;
                dto.FromDate = this.FromDate.ToString("dd-MM-yyyy");
                dto.ToDate = this.ToDate.ToString("dd-MM-yyyy");
                dto.Debit = account.DrAmount;
                dto.Credit = account.CrAmount;
                dto.SumCredit = ConvertToLong((Entries.AccountingEntries.Where(x=>x.CrAmount>0).Sum(x=>x.CrAmount)));
                dto.SumDebit = ConvertToLong(Entries.AccountingEntries.Where(x => x.DrAmount > 0).Sum(x => x.DrAmount));
                dto.NumberCredit = ConvertToLong(Entries.AccountingEntries.Where(x => x.CrAmount > 0).Count());
                dto.NumberDebit = ConvertToLong(Entries.AccountingEntries.Where(x => x.CrAmount > 0).Count());
                dto.NumberEntries = ConvertToLong(Entries.AccountingEntries.Count());
                listDto.Add(dto);
            }
            return listDto;
        }
        public static long ConvertToLong(object value)
        {
            try
            {
                if (value == null)
                {
                    // Handle null value by returning 0 or a default value.
                    return 0;
                }

                // If the value is already a numeric type, convert directly.
                if (value is int || value is long || value is short || value is byte)
                {
                    return Convert.ToInt64(value);
                }

                if (value is decimal || value is double || value is float)
                {
                    // Round the value before converting to avoid truncation errors.
                    return Convert.ToInt64(Math.Round(Convert.ToDecimal(value)));
                }

                if (value is string)
                {
                    // Remove potential formatting characters like commas or currency symbols.
                    string cleanedValue = value.ToString().Replace(",", "").Replace("$", "").Trim();

                    if (decimal.TryParse(cleanedValue, out decimal parsedDecimal))
                    {
                        return Convert.ToInt64(Math.Round(parsedDecimal));
                    }
                    else
                    {
                        throw new FormatException("The string value cannot be parsed as a numeric value.");
                    }
                }

                // Attempt to convert any other object type if possible.
                if (value is IConvertible)
                {
                    return Convert.ToInt64(value);
                }

                // If none of the above conditions are met, throw an exception.
                throw new InvalidCastException("The provided value is not convertible to a long.");
            }
            catch (OverflowException)
            {
                // Handle values that are out of range for Int64.
                throw new OverflowException("The value is too large or too small to be converted to a long.");
            }
            catch (FormatException ex)
            {
                // Handle invalid formats.
                throw new FormatException($"Invalid format: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions.
                throw new Exception($"An error occurred during conversion: {ex.Message}");
            }
        }
    }
    public class AccountingEntryServiceResponse
    {
        public List<AccountingEntry > Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
    }
    public class AccountingEntryQuery
    {
        public AccountingEntryDto AccountingEntry { get; set; }
        public List<Branch> Branchs { get; set; }
        public SystemQuery SystemQuery { get; set; } = new SystemQuery();
 
        public List<ReportInfo> ReportDownloadInfo { get; set; } = new List<ReportInfo>();
    }



    public class AccountEntry
    {
        public string Reference { get; set; }
        public string AccountNumber { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
    public class AccountingEntry 
    {
   

        // Unique ID number for the entry=
        public string Id { get; set; }

        // Date the accounting entry was created
        public DateTime EntryDate { get; set; }
        // Effective date for posting the accounting impact
        public DateTime ValueDate { get; set; }
        // Type of entry (Debit or Credit)
        public string EntryType { get; set; }
        // Currency denomination 
        public string Currency { get; set; }
        // Text description explaining the purpose of the transaction
        public string Description { get; set; }
        // ID linking to source documents related to transaction
        public string ReferenceID { get; set; }
        // Status of workflow ( Posted,  Reversed)
        public string Status { get; set; }
        // Source system or module that generated the entry
        public string Source { get; set; }
        public string BankId { get; set; } // Related branch 
        public string BranchId { get; set; } // Related branch 
        public string DrAccountId { get; set; }
        public string DrAccountNumber { get; set; }
        public string CrAccountId { get; set; }
        public string CrAccountNumber { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public string AccountId { get; set; }
        public string OperationType { get; set; }
        public string CrCurrentBalance { get; set; }
        public string DrCurrentBalance { get; set; }
        public string AccountNumber { get; set; }
        public string AccountNumberReference { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        
        public void TagedAsReversed(List<AccountingEntry> entries, string userId)
        {
            foreach (var entry in entries)
                Status = "Reversed";
        }
    }

    public class BranchToBranchTransferDto  
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public CurrencyNotesRequest CurrencyNotesRequest { get; set; }

    }
    public class AccountingEntryDto
    {

        // Unique ID number for the entry=
        public string Id { get; set; }

        // Date the accounting entry was created
        public DateTime EntryDate { get; set; }

        public string EntryDatetime { get; set; }

        // Effective date for posting the accounting impact
        public DateTime ValueDate { get; set; }

        // Type of entry (Debit or Credit)
        public string EntryType { get; set; }


        // Currency denomination 
        public string Currency { get; set; }

        // Text description explaining the purpose of the transaction
        public string Description { get; set; }

        // ID linking to source documents related to transaction
        public string ReferenceID { get; set; }

        public string Status { get; set; }

        public string ReviewedBy { get; set; }
        public string DrAccountId { get; set; }
        public string CrAccountId { get; set; }
        public string DrAccountNumber { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string CrAccountNumber { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public decimal CurrentBalance { get; set; } = 0;
        public bool IsAuxilaryEntry { get; set; } = false;
        public string AccountNumberReference { get; set; }
        public string InitiatorId { get; set; } = "";
        public string Source { get; set; }
        public string EventCode { get; set; }  // Generated from rule
        public string BankId { get; set; } // Related Bank 
        public string BranchId { get; set; } // Related branch 
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ExternalBranchId { get; set; }

        public string OperationType { get; set; }
        public decimal DrBalanceBroughtForward { get; set; } = 0;
        public decimal CrBalanceBroughtForward { get; set; } = 0;
        public decimal Amount { get; set; }
        public decimal CrCurrentBalance { get; set; }
        public decimal DrCurrentBalance { get; set; }
        public string AccountId { get; set; }
        public string AccountCartegory { get; set; }
        public string EntryDateTime { get; set; }
        public string Representative { get; set; }
        public string Naration { get; set; }
    }


    public class SystemQuery
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; } //= new DateTimeOffset();
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }
 
        public string FileType { get; set; }
        public string ReportType { get; set; }
        public List<string> AccountIds { get; set; }
        public List<string> BranchIds { get; set; }
        public string BranchId { get; set; }
    }
    public class ReportDto
    {
        public string Id { get; set; }
        public string ReportType { get; set; }
        public string Extension { get; set; }
        public string DownloadPath { get; set; }
        public string Size { get; set; }
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string BranchName { get; set; }
        public string Username { get; set; }
    }
    public static class BSCartegory
    {

        public static string Assets = "Assets";
        public static string LIABILITIES = "LIABILITIES";
       public static string Income = "Income";
        public static string Expense = "Expense";
    }
    public class GLQuery
    {

        public string FileType { get; set; }
        public string BranchId { get; set; }
        public List<string> BranchIds { get; set; }
    }
    public class JEQuery
    {
        public DateTimeOffset ToDate { get; set; }
        public DateTimeOffset FromDate { get; set; }
        public string FileType { get; set; }
        public string BranchId { get; set; }
        public List<string> BranchIds { get; set; }
    }
    public class BSQuery
    {
        public string BranchId { get; set; }
        public DateTimeOffset ToDate { get; set; }
        public DateTimeOffset FromDate { get; set; }
        public string FileType { get; set; }
        public string DocumentId { get; set; }
    }
        
    public class TrialBalance4Column
    {
        public DateTimeOffset ToDate { get; set; }
        public DateTimeOffset FromDate { get; set; }
        public string FileType { get; set; }
        public string ReportType { get; set; }

        public string BranchId { get; set; }
    }
}