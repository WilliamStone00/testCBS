using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Translations;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.Translations
{
    public class MockTranslationPlaceholderService : BaseService
    {
        private readonly List<TranslationPlaceholder> _mockPlaceholders;
        private readonly Random _random = new Random();
        private int _placeholderCounter = 1000;

        public MockTranslationPlaceholderService()
        {
            // Initialize with sample data
            _mockPlaceholders = GenerateMockPlaceholders();
        }

        public List<TranslationPlaceholder> GenerateMockPlaceholders()
        {
            return new List<TranslationPlaceholder>
            {
                new TranslationPlaceholder
                {
                    Id = "TP001",
                    Placeholder = "{CustomerName}",
                    English = "Customer Name",
                    French = "Nom du client",
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now.AddDays(-30),
                    ModifiedBy = "Admin",
                    ModifiedDate = DateTime.Now.AddDays(-5)
                },
                new TranslationPlaceholder
                {
                    Id = "TP002",
                    Placeholder = "{AccountNumber}",
                    English = "Account Number",
                    French = "Numéro de compte",
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now.AddDays(-30),
                    ModifiedBy = "System",
                    ModifiedDate = DateTime.Now.AddDays(-10)
                },
                new TranslationPlaceholder
                {
                    Id = "TP003",
                    Placeholder = "{TransactionDate}",
                    English = "Transaction Date",
                    French = "Date de la transaction",
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now.AddDays(-25),
                    ModifiedBy = null,
                    ModifiedDate = null
                },
                new TranslationPlaceholder
                {
                    Id = "TP004",
                    Placeholder = "{Amount}",
                    English = "Amount",
                    French = "Montant",
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now.AddDays(-20),
                    ModifiedBy = "Admin",
                    ModifiedDate = DateTime.Now.AddDays(-2)
                },
                new TranslationPlaceholder
                {
                    Id = "TP005",
                    Placeholder = "{Currency}",
                   English = "Currency",
                   French = "Devise",
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now.AddDays(-15),
                    ModifiedBy = null,
                    ModifiedDate = null
                },
                new TranslationPlaceholder
                {
                    Id = "TP006",
                   English = "Branch Name",
                   French = "Nom de la succursale",
                    CreatedBy = "User1",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    ModifiedBy = "User1",
                    ModifiedDate = DateTime.Now.AddDays(-1)
                },
                new TranslationPlaceholder
                {
                    Id = "TP007",
                    Placeholder = "{LoanAmount}",
                  English = "Loan Amount",
                  French = "Montant du prêt",
                    CreatedBy = "LoanOfficer",
                    CreatedDate = DateTime.Now.AddDays(-8),
                    ModifiedBy = null,
                    ModifiedDate = null
                },
                new TranslationPlaceholder
                {
                    Id = "TP008",
                    Placeholder = "{InterestRate}",
                    English = "Interest Rate",
                    French = "Taux d'intérêt",
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    ModifiedBy = "Admin",
                    ModifiedDate = DateTime.Now
                }
            };
        }

        //public async Task<CustomDataTable> GetTranslationPlaceholdersDataTableAsync(TranslationPlaceholderQuery query)
        //{
        //    try
        //    {
        //        // Simulate async behavior
        //        await Task.Delay(50);

        //        // Get mock data
        //        var placeholders = GenerateMockPlaceholders();



        //        //// Wrap mock data into CustomDataTable format
        //        //var dataTable = new CustomDataTable
        //        //{
        //        //    Data = placeholders,
        //        //    TotalRecords = placeholders.Count
        //        //};

        //        return placeholders;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Translation placeholder service unavailable: {ex.Message}", ex);
        //    }
        //}

        public async Task<CustomDataTable> GetTranslationPlaceholdersDataTableAsync(TranslationPlaceholderQuery query)
        {
            try
            {
                // Simulate async behavior
                await Task.Delay(50);

                // Get mock data
                var placeholders = GenerateMockPlaceholders();

                // Wrap mock data into CustomDataTable format
                var dataTable = new CustomDataTable
                {
                    data = placeholders,                
                };

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"Translation placeholder service unavailable: {ex.Message}", ex);
            }
        }


        public async Task<TranslationPlaceholder> GetTranslationPlaceholderByIdAsync(string id)
        {
            await Task.Delay(100); // Simulate async delay

            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _mockPlaceholders.FirstOrDefault(p =>
                p.Id?.Equals(id, StringComparison.OrdinalIgnoreCase) == true);
        }

        public async Task<ExecutionMessages> CreateTranslationPlaceholderAsync(TranslationPlaceholder model)
        {
            await Task.Delay(200); // Simulate async delay

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(model.Placeholder))
                {
                    GetExecutionMessages(null, false, "N/A", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                        "Placeholder name is required.");
                    return ExecutionMessage;
                }

                // Check for duplicates
                if (_mockPlaceholders.Any(p =>
                    p.Placeholder?.Equals(model.Placeholder, StringComparison.OrdinalIgnoreCase) == true))
                {
                    GetExecutionMessages(null, false, model.Placeholder, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                        $"Placeholder '{model.Placeholder}' already exists.");
                    return ExecutionMessage;
                }

                // Generate new ID
                model.Id = $"TP{_placeholderCounter++:D3}";
                model.CreatedBy = GetUserFullName() ?? "MockUser";
                model.CreatedDate = DateTime.Now;

                // Add to mock collection
                _mockPlaceholders.Add(model);

                GetExecutionMessages(model, true, model.Placeholder, MessagesResults.Success,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                    $"Translation placeholder '{model.Placeholder}' created successfully.");

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Placeholder, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex,
                    $"Failed to create translation placeholder: {ex.Message}");
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> UpdateTranslationPlaceholderAsync(TranslationPlaceholder model)
        {
            await Task.Delay(200); // Simulate async delay

            try
            {
                if (string.IsNullOrWhiteSpace(model?.Id))
                {
                    GetExecutionMessages(null, false, "N/A", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        "Placeholder ID is required.");
                    return ExecutionMessage;
                }

                var existing = _mockPlaceholders.FirstOrDefault(p => p.Id == model.Id);
                if (existing == null)
                {
                    GetExecutionMessages(null, false, model.Id, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        $"Translation placeholder with ID '{model.Id}' not found.");
                    return ExecutionMessage;
                }

                // Check for duplicate placeholder name (excluding self)
                if (_mockPlaceholders.Any(p =>
                    p.Id != model.Id &&
                    p.Placeholder?.Equals(model.Placeholder, StringComparison.OrdinalIgnoreCase) == true))
                {
                    GetExecutionMessages(null, false, model.Placeholder, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        $"Placeholder '{model.Placeholder}' already exists.");
                    return ExecutionMessage;
                }

                // Update properties
                existing.Placeholder = model.Placeholder;
                existing.ModifiedBy = GetUserFullName() ?? "MockUser";
                existing.ModifiedDate = DateTime.Now;

                GetExecutionMessages(existing, true, model.Placeholder, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                    $"Translation placeholder '{model.Placeholder}' updated successfully.");

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Placeholder, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex,
                    $"Failed to update translation placeholder: {ex.Message}");
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> DeactivateTranslationPlaceholderAsync(string placeholderId)
        {
            await Task.Delay(200); // Simulate async delay

            try
            {
                if (string.IsNullOrWhiteSpace(placeholderId))
                {
                    GetExecutionMessages(null, false, "N/A", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        "Placeholder ID is required.");
                    return ExecutionMessage;
                }

                var existing = _mockPlaceholders.FirstOrDefault(p => p.Id == placeholderId);
                if (existing == null)
                {
                    GetExecutionMessages(null, false, placeholderId, MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        $"Translation placeholder with ID '{placeholderId}' not found.");
                    return ExecutionMessage;
                }

                // Check if it's a system placeholder               
                existing.ModifiedBy = GetUserFullName() ?? "MockUser";
                existing.ModifiedDate = DateTime.Now;

                GetExecutionMessages(null, true, existing.Placeholder, MessagesResults.Success,
                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                    $"Translation placeholder '{existing.Placeholder}' deactivated successfully.");

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, placeholderId, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex,
                    $"Failed to deactivate translation placeholder: {ex.Message}");
                return ExecutionMessage;
            }
        }

        // Optional: Helper method to get mock user (replace with actual user retrieval logic)
        private string GetMockUser()
        {
            // In a real implementation, this would come from authentication context
            var users = new[] { "Admin", "User1", "User2", "LoanOfficer", "Teller" };
            return users[_random.Next(users.Length)];
        }

        // Optional: Method to reset mock data (useful for testing)
        public void ResetMockData()
        {
            _placeholderCounter = 1000;
            _mockPlaceholders.Clear();
            _mockPlaceholders.AddRange(GenerateMockPlaceholders());
        }



    }



    //public class TranslationPlaceholderQuery
    //{
    //    public int Draw { get; set; }
    //    public int Start { get; set; }
    //    public int Length { get; set; }
    //    public Search Search { get; set; }
    //    public List<Order> Order { get; set; }
    //    public List<Column> Columns { get; set; }
    //    public bool? IsActive { get; set; }
    //    public bool? IsSystem { get; set; }
    //}

    public class Search
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public Search Search { get; set; }
    }
}
