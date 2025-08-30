using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.Data.Entity.SalaryManagement
{
    public class SalaryUploadModel
    {
        public string Id { get; set; }
        public string SalaryCode { get; set; }
        public string Matricule { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string LaisonBankAccountNumber { get; set; }
        public decimal NetSalary { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string FileUploadId { get; set; }
        public string Phonumber { get; set; }
        public DateTime Date { get; set; }
        public string UploadedBy { get; set; }
        public List<BranchPayrollSummary> BranchPayrollSummaries { get; set; }

    }
   

    public class SalaryUploadModelWithBranchStatisticsDto
    {
        public List<SalaryUploadModel> SalaryUploadModelDtos { get; set; }
        public List<SalaryUploadModel> ReainingSalaryModelWithoutMembers { get; set; }
        public List<BranchPayrollSummary> BranchPayrollSummaries { get; set; }
        public List<IndividualProfile> AllSalaryEarners { get; set; }
        public List<IndividualProfile> AllSalaryEarnersNotFoundInTheCurrentpayroll { get; set; }
        public SalaryUploadModelWithBranchStatisticsDto()
        {
            SalaryUploadModelDtos=new List<SalaryUploadModel>();
            BranchPayrollSummaries=new List<BranchPayrollSummary>();
            ReainingSalaryModelWithoutMembers=new List<SalaryUploadModel>();
            AllSalaryEarners=new List<IndividualProfile>();
            AllSalaryEarnersNotFoundInTheCurrentpayroll=new List<IndividualProfile>();
        }
    }
    public class SalaryUploadModelCarrier
    {

        public FileUploadDto FileUpload { get; set; }
        public List<FileUploadDto> FileUploads { get; set; }
        public SalaryUploadModel SalaryUploadModel { get; set; }
        public List<SalaryUploadModel> SalaryUploadModels { get; set; }
        public AddSalaryUploadModelCommand AddSalaryUploadModelCommand { get; set; }
        public SalaryUploadModelSummaryDto SalaryUploadModelSummaryDto { get; set; }
        public SalaryAnalysisResultSummary SalaryAnalysisResultSummary { get; set; }
        public List<SalaryAnalysisResultDetailNew> SalaryAnalysisResultDetails { get; set; }
        public SalaryAnalysisCommand SalaryAnalysisCommand { get; set; }
        public ActivateSalaryFileCommand ActivateSalaryFileCommand { get; set; }
        public List<BranchPayrollSummary> BranchPayrollSummaries { get; set; }
        public SalaryUploadModelCarrier()
        {
            SalaryAnalysisCommand=new SalaryAnalysisCommand();
            AddSalaryUploadModelCommand =new AddSalaryUploadModelCommand();
            SalaryUploadModelSummaryDto=new SalaryUploadModelSummaryDto();
            FileUpload=new FileUploadDto();
            FileUploads=new List<FileUploadDto>();
            SalaryUploadModels=new List<SalaryUploadModel>();
            SalaryUploadModel=new SalaryUploadModel();
            SalaryAnalysisResultSummary=new SalaryAnalysisResultSummary();
            SalaryAnalysisResultDetails=new List<SalaryAnalysisResultDetailNew>();
            ActivateSalaryFileCommand=new ActivateSalaryFileCommand();
            BranchPayrollSummaries=new List<BranchPayrollSummary>();
        }
    }
    public class GetAllFileUploadSalaryFileActivatedQuery
    {
        public bool Status { get; set; }
        public bool Both { get; set; }
    }
    public class GetSalaryUploadModelWithBranchStatisticsQuery
    {
        public string FileUploadId { get; set; } // Required File Upload ID to filter salary uploads.
        public bool IncludeBranchStatistics { get; set; } // Determines if branch statistics should be included.

       
    }
    public class GetSalaryUploadModelQuery
    {
        public DateTime Date { get; set; } // Optional filter by date.
        public string FileUploadId { get; set; } // Optional filter by file upload ID.
        public string SalaryCode { get; set; } // Optional filter by salary code.
    }
    public class SalaryUploadModelSummaryDto
    {

        public decimal TotalNetSalary { get; set; }
        public int TotalMembers { get; set; }
        public string FileUploadId { get; set; }
        

    }

    public class BranchPayrollSummary
    {

        public decimal TotalNetSalary { get; set; }
        public int TotalMembers { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

    }

    public class SetFileUploadPrivateViewCommand
    {
        /// <summary>
        /// The FileUpload ID whose private view status will be updated.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The new value of the PrivateView property (true = private, false = public).
        /// </summary>
        public bool PrivateView { get; set; }
    }

    public class ActivateSalaryFileCommand
    {

        public bool Status { get; set; }
        public string Id { get; set; }
    }

    public class AddSalaryUploadModelCommand
    {
        [Required(ErrorMessage = "File is required.")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Salary Type is required.")]
        [StringLength(50, ErrorMessage = "Salary Type must be 50 characters or fewer.")]
        public string SalaryType { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Branch Id is required.")]
        [StringLength(64, ErrorMessage = "Branch Id must be 64 characters or fewer.")]
        public string BranchId { get; set; }

        [StringLength(64, ErrorMessage = "Standing Order Source Account Id must be 64 characters or fewer.")]
        public string StandingOrderSourceChartOfAccountId { get; set; }

        public bool PrivateView { get; set; }
    }
    public class GetFileUploadsDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }

        // ✅ Filters
        public string BranchId { get; set; }
        public string UploadedBy { get; set; }
        public string FileType { get; set; }
        public string FileCategory { get; set; }
        public bool PrivateView { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ActionParam { get; set; }
    }
    public class FileUploadDto
    {
        public string Id { get; set; } // Unique Identifier for the file
        public string FileName { get; set; } // Original file name
        public string FilePath { get; set; } // Path where the file is stored
        public string FileHash { get; set; } // Unique hash to prevent duplicate uploads
        public string BranchId { get; set; } // Branch Id associated with the upload
        public string BranchName { get; set; } // Branch name associated with the upload
        public string UploadedBy { get; set; } // User who uploaded the file
        public DateTime UploadedOn { get; set; } // Timestamp of when the file was uploaded
        public string FileUploadId { get; set; } // Reference id for file tracking
        public string FileCategory { get; set; }
        public string SalaryProcessingStatus { get; set; }
        public bool IsAvalaibleForExecution { get; set; }
        public string FileCode { get; set; }
        public string FileType { get; set; }
        public string StandingOrderSourceChartOfAccountId { get; set; }
        public bool PrivateView { get; set; }
        public int TotalBranchesThatHaveExecutedPayrol { get; set; }
        public int TotalBranchesInvolvedInPayrolProcessing { get; set; }
        public List<SalaryUploadModel> SalaryUploadModels { get; set; }
       
    }
}
