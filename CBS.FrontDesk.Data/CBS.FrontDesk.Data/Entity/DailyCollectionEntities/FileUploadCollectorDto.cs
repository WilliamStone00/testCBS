using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
    public class FileUploadCollectorDto
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
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    
    public static List<FileUploadCollectorDto> GetSampleFileUploadList()
        {
            return new List<FileUploadCollectorDto>
    {
        new FileUploadCollectorDto
        {
            Id = "1",
            FileName = "employee_payroll_jan2024.xlsx",
            FilePath = "/uploads/payroll/employee_payroll_jan2024.xlsx",
            FileHash = "a1b2c3d4e5f6789012345678901234567890abcd",
            BranchId = "BR001",
            BranchName = "Main Branch",
            UploadedBy = "john.doe@company.com",
            UploadedOn = DateTime.Now.AddDays(-10),
            FileUploadId = "FU001",
            FileCategory = "Payroll",
            SalaryProcessingStatus = "Processed",
            IsAvalaibleForExecution = true,
            FileCode = "PAY001",
            FileType = "Excel",
            CreatedDate = DateTime.Now.AddDays(-10),
            CreatedBy = "john.doe@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "2",
            FileName = "timesheet_feb2024.csv",
            FilePath = "/uploads/timesheets/timesheet_feb2024.csv",
            FileHash = "b2c3d4e5f6789012345678901234567890abcde1",
            BranchId = "BR002",
            BranchName = "North Branch",
            UploadedBy = "jane.smith@company.com",
            UploadedOn = DateTime.Now.AddDays(-8),
            FileUploadId = "FU002",
            FileCategory = "Timesheet",
            SalaryProcessingStatus = "Pending",
            IsAvalaibleForExecution = false,
            FileCode = "TS001",
            FileType = "CSV",
            CreatedDate = DateTime.Now.AddDays(-8),
            CreatedBy = "jane.smith@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "3",
            FileName = "bonus_calculation_march2024.pdf",
            FilePath = "/uploads/bonus/bonus_calculation_march2024.pdf",
            FileHash = "c3d4e5f6789012345678901234567890abcdef12",
            BranchId = "BR001",
            BranchName = "Main Branch",
            UploadedBy = "michael.johnson@company.com",
            UploadedOn = DateTime.Now.AddDays(-6),
            FileUploadId = "FU003",
            FileCategory = "Bonus",
            SalaryProcessingStatus = "In Progress",
            IsAvalaibleForExecution = true,
            FileCode = "BON001",
            FileType = "PDF",
            CreatedDate = DateTime.Now.AddDays(-6),
            CreatedBy = "michael.johnson@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "4",
            FileName = "deduction_report_april2024.xlsx",
            FilePath = "/uploads/deductions/deduction_report_april2024.xlsx",
            FileHash = "d4e5f6789012345678901234567890abcdef123",
            BranchId = "BR003",
            BranchName = "South Branch",
            UploadedBy = "sarah.wilson@company.com",
            UploadedOn = DateTime.Now.AddDays(-4),
            FileUploadId = "FU004",
            FileCategory = "Deductions",
            SalaryProcessingStatus = "Failed",
            IsAvalaibleForExecution = false,
            FileCode = "DED001",
            FileType = "Excel",
            CreatedDate = DateTime.Now.AddDays(-4),
            CreatedBy = "sarah.wilson@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "5",
            FileName = "overtime_log_may2024.csv",
            FilePath = "/uploads/overtime/overtime_log_may2024.csv",
            FileHash = "e5f6789012345678901234567890abcdef1234",
            BranchId = "BR002",
            BranchName = "North Branch",
            UploadedBy = "david.brown@company.com",
            UploadedOn = DateTime.Now.AddDays(-2),
            FileUploadId = "FU005",
            FileCategory = "Overtime",
            SalaryProcessingStatus = "Processed",
            IsAvalaibleForExecution = true,
            FileCode = "OT001",
            FileType = "CSV",
            CreatedDate = DateTime.Now.AddDays(-2),
            CreatedBy = "david.brown@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "6",
            FileName = "attendance_june2024.xlsx",
            FilePath = "/uploads/attendance/attendance_june2024.xlsx",
            FileHash = "f6789012345678901234567890abcdef12345",
            BranchId = "BR004",
            BranchName = "East Branch",
            UploadedBy = "lisa.davis@company.com",
            UploadedOn = DateTime.Now.AddDays(-1),
            FileUploadId = "FU006",
            FileCategory = "Attendance",
            SalaryProcessingStatus = "Pending",
            IsAvalaibleForExecution = true,
            FileCode = "ATT001",
            FileType = "Excel",
            CreatedDate = DateTime.Now.AddDays(-1),
            CreatedBy = "lisa.davis@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "7",
            FileName = "leave_balance_july2024.pdf",
            FilePath = "/uploads/leaves/leave_balance_july2024.pdf",
            FileHash = "6789012345678901234567890abcdef123456",
            BranchId = "BR001",
            BranchName = "Main Branch",
            UploadedBy = "robert.miller@company.com",
            UploadedOn = DateTime.Now,
            FileUploadId = "FU007",
            FileCategory = "Leaves",
            SalaryProcessingStatus = "In Progress",
            IsAvalaibleForExecution = false,
            FileCode = "LV001",
            FileType = "PDF",
            CreatedDate = DateTime.Now,
            CreatedBy = "robert.miller@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "8",
            FileName = "tax_deduction_aug2024.csv",
            FilePath = "/uploads/tax/tax_deduction_aug2024.csv",
            FileHash = "789012345678901234567890abcdef1234567",
            BranchId = "BR003",
            BranchName = "South Branch",
            UploadedBy = "jennifer.garcia@company.com",
            UploadedOn = DateTime.Now.AddHours(-12),
            FileUploadId = "FU008",
            FileCategory = "Tax",
            SalaryProcessingStatus = "Processed",
            IsAvalaibleForExecution = true,
            FileCode = "TAX001",
            FileType = "CSV",
            CreatedDate = DateTime.Now.AddHours(-12),
            CreatedBy = "jennifer.garcia@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "9",
            FileName = "insurance_premium_sep2024.xlsx",
            FilePath = "/uploads/insurance/insurance_premium_sep2024.xlsx",
            FileHash = "89012345678901234567890abcdef12345678",
            BranchId = "BR002",
            BranchName = "North Branch",
            UploadedBy = "kevin.martinez@company.com",
            UploadedOn = DateTime.Now.AddHours(-6),
            FileUploadId = "FU009",
            FileCategory = "Insurance",
            SalaryProcessingStatus = "Failed",
            IsAvalaibleForExecution = false,
            FileCode = "INS001",
            FileType = "Excel",
            CreatedDate = DateTime.Now.AddHours(-6),
            CreatedBy = "kevin.martinez@company.com"
        },
        new FileUploadCollectorDto
        {
            Id = "10",
            FileName = "commission_report_oct2024.pdf",
            FilePath = "/uploads/commission/commission_report_oct2024.pdf",
            FileHash = "9012345678901234567890abcdef123456789",
            BranchId = "BR004",
            BranchName = "East Branch",
            UploadedBy = "amanda.taylor@company.com",
            UploadedOn = DateTime.Now.AddHours(-3),
            FileUploadId = "FU010",
            FileCategory = "Commission",
            SalaryProcessingStatus = "Pending",
            IsAvalaibleForExecution = true,
            FileCode = "COM001",
            FileType = "PDF",
            CreatedDate = DateTime.Now.AddHours(-3),
            CreatedBy = "amanda.taylor@company.com"
        }
    };
        }
    }
}