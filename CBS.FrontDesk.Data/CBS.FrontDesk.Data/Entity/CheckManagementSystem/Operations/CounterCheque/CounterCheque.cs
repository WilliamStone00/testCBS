using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque
{
    public class CounterCheques
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string BranchId { get; set; }
        public string CustomerId { get; set; }
        // For displaying in the list
        public string CheckLeafId { get; set; }
        public string Id { get; set; }
        public string CheckNumber { get; set; }
        public DateTime? IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Catergoryid { get; set; }
        public string ClientName { get; set; }
        public int NumberOfPages { get; set; }
        public string ChequeBookId { get; set; } 

    }

    // DTO for the modal action form
    public class CounterChequeActionDto
    {
        [Required]
        public string CounterChequeId { get; set; }
        [Required]
        public string Motive { get; set; }
        public string Action { get; set; } // "Review", "Validate", or "Reject"
    }

    
    public class CounterChequeQuery
    {
        public CounterChequeQuery()
        {
            DataTableOptions = new DataTableOptions();
        }

        public DataTableOptions DataTableOptions { get; set; }

        public string Id { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string BranchId { get; set; }
        
    }


    public class ChequeRequestDto
    {
        public string Id { get; set; }              
        public string CustomerId { get; set; }      
        public decimal Amount { get; set; }         
        public string AccountNumber { get; set; }   
        public string BranchId { get; set; }        
        public string CheckLeafId { get; set; }     
        public string CheckNumber { get; set; }    
        public DateTime? IssuedOn { get; set; }      
        public string IssuedBy { get; set; }        
    }


    public class CustomerCheckBookDisplayDto
    {
        public string Id { get; set; }                 // checkbookId
        public string CustomerId { get; set; }
        public string AccountId { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchId { get; set; }
        public decimal Balance { get; set; }
        public string BranchName { get; set; }


        public int NumberOfLeaves { get; set; }
        public int RemainingLeaves { get; set; }
        public int UsedLeaves { get; set; }

        public int StartSerialNumber { get; set; }
        public int EndSerialNumber { get; set; }
        public int CurrentSerialNumber { get; set; }




        public DateTime CreatedDate { get; set; }

        public string CheckBookCategoryId { get; set; }
        public CheckBookCategoryDto CheckBookCategory { get; set; }

        public List<CheckLeafDto> Leaves { get; set; }

        // Nested DTOs included here
        public class CheckBookCategoryDto
        {
            public string Id { get; set; }
            
          
            public string Name { get; set; }

            public int NumberOfCheckBooks { get; set; }
            public int NumberOfPages { get; set; }


            
            public string CreatedBy { get; set; }

        }

        public class CheckLeafDto
        {
            public string Id { get; set; }
            public string CheckBookId { get; set; }
            public string CounterCheckId { get; set; }
            public int SerialNumber { get; set; }
            public string Status { get; set; }




        }
    }






    public class FullCustomerChequeBookResponse
    {
        public CustomerDto customerDto { get; set; }
        public List<AccountDto> accountDtos { get; set; }
        public List<CheckBookEntry> checkBooks { get; set; }
    }

    public class CustomerDto
    {
        public string customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        // Add more fields as needed
    }

    public class AccountDto
    {
        public string id { get; set; }
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        // Add more fields as needed
    }

    public class CheckBookEntry
    {
        public CheckBook checkBook { get; set; }
        public AccountDto account { get; set; }
    }

    public class CheckBook
    {
        public string id { get; set; }
        public string accountId { get; set; }
        public string accountNumber { get; set; }
        public string checkBookCategoryId { get; set; }
        public CheckBookCategory checkBookCategory { get; set; }
        public int numberOfLeaves { get; set; }
        public int usedLeaves { get; set; }
        public int remainingLeaves { get; set; }
    }

    public class CheckBookCategory
    {
        public string id { get; set; }
        public string name { get; set; }
    }



}
