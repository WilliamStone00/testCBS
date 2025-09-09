// Location: ~/BusinessService/CheckManagementSystem/Configurations/NotificationConfiguration/NotificationConfigService.cs

using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.NotificationConfiguration
{
    public class NotificationConfigmockService : BaseService
    {
        // In a real application, you would have an ApiCallerHelper here.
        // For now, we will work directly with our mock data stores.

        #region Mock Data Stores

        /// <summary>
        /// This is our mock "database table" for saved NotificationConfig objects.
        /// </summary>
        private static readonly List<NotificationConfig> _mockNotificationConfigs = new List<NotificationConfig>
        {
            // Pre-populate with some sample data for testing.
            new NotificationConfig
            {
                Id = "NotifConfig_Central_ChequeCashed",
                IsCentralized = true,
                BranchId = null,
                NotificationType = "ChequeCashed",
                Description = "Central template for when a cheque is paid.",
                TemplateBody = "Dear {ClientName}, your cheque #{ChequeNumber} for the amount of {Amount} has been successfully paid on {TransactionDate}. Thank you for banking with us.",
                IsActive = true
            },
            new NotificationConfig
            {
                Id = "NotifConfig_BR123_ChequeCashed",
                IsCentralized = false,
                BranchId = "BR123",
                NotificationType = "ChequeCashed",
                Description = "Main Branch's custom template for cashed cheques.",
                TemplateBody = "GREETINGS from Main Branch! Your cheque #{ChequeNumber} for {Amount} was paid. Your new balance is {NewBalance}.",
                IsActive = true
            }
        };

        /// <summary>
        /// This list defines the available Notification Types your system supports.
        /// In a real system, this might come from a configuration file or a database table.
        /// </summary>
        private static readonly List<NotificationType> _mockNotificationTypes = new List<NotificationType>
        {
            new NotificationType
            {
                Value = "ChequeCashed",
                Text = "Cheque Cashed Notification",
                Description = "Sent to the issuer when their cheque has been successfully paid.",
                Placeholders = new List<string> { "{ClientName}", "{AccountNumber}", "{ChequeNumber}", "{Amount}", "{TransactionDate}", "{NewBalance}" }
            },
            new NotificationType
            {
                Value = "CheckbookReady",
                Text = "Checkbook Ready for Pickup",
                Description = "Sent to the client when their requested checkbook is printed and ready for collection.",
                Placeholders = new List<string> { "{ClientName}", "{BranchName}", "{BranchAddress}", "{CollectionEndDate}" }
            },
            new NotificationType
            {
                Value = "InsufficientFunds",
                Text = "Insufficient Funds Warning",
                Description = "Sent to the issuer when a cheque is presented but their account has insufficient funds.",
                Placeholders = new List<string> { "{ClientName}", "{AccountNumber}", "{ChequeNumber}", "{Amount}", "{DeficitAmount}" }
            }
        };

        #endregion


        #region Mock Service Methods

        /// <summary>
        /// MOCK: Gets the list of all available notification types.
        /// </summary>
        public Task<IEnumerable<NotificationType>> GetNotificationTypesMockAsync()
        {
            // Simulate an async operation by returning a completed task.
            return Task.FromResult(_mockNotificationTypes.AsEnumerable());
        }

        /// <summary>
        /// MOCK: Gets saved configurations, supporting all filters.
        /// </summary>
        public Task<IEnumerable<NotificationConfig>> GetConfigsMockAsync(bool isCentralized, string branchId = null, string notificationType = null)
        {
            IEnumerable<NotificationConfig> query = _mockNotificationConfigs;

            // Apply filters one by one.
            query = query.Where(c => c.IsCentralized == isCentralized);

            if (!isCentralized)
            {
                // If it's not centralized, it must match the branchId.
                query = query.Where(c => c.BranchId == branchId);
            }

            if (!string.IsNullOrEmpty(notificationType))
            {
                query = query.Where(c => c.NotificationType.Equals(notificationType, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(query.ToList().AsEnumerable());
        }

        /// <summary>
        /// MOCK: Simulates creating a new NotificationConfig.
        /// </summary>
        public Task<ExecutionMessages> CreateMockAsync(NotificationConfig model)
        {
            // Simulate creating a new ID.
            model.Id = "NotifConfig_" + Guid.NewGuid().ToString().Substring(0, 8);
            _mockNotificationConfigs.Add(model);

            GetExecutionMessages(model, true, "Notification Config", MessagesResults.Success, ExecutionProcessOption.InsertObject, "Success");
            return Task.FromResult(ExecutionMessage);
        }

        /// <summary>
        /// MOCK: Simulates updating an existing NotificationConfig.
        /// </summary>
        public Task<ExecutionMessages> UpdateMockAsync(NotificationConfig model)
        {
            var existing = _mockNotificationConfigs.FirstOrDefault(c => c.Id == model.Id);
            if (existing != null)
            {
                // Simulate updating the properties in our "database".
                existing.Description = model.Description;
                existing.TemplateBody = model.TemplateBody;
                existing.IsActive = model.IsActive;

                GetExecutionMessages(existing, true, "Notification Config", MessagesResults.Success, ExecutionProcessOption.UpdateUpject, "Success");
            }
            else
            {
                GetExecutionMessages(model, false, "Notification Config", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, "Failed", null, "Item not found.");
            }
            return Task.FromResult(ExecutionMessage);
        }

        /// <summary>
        /// MOCK: Simulates deleting a NotificationConfig.
        /// </summary>
        public Task<ExecutionMessages> DeleteMockAsync(string id)
        {
            var itemToRemove = _mockNotificationConfigs.FirstOrDefault(c => c.Id == id);
            if (itemToRemove != null)
            {
                _mockNotificationConfigs.Remove(itemToRemove);
                GetExecutionMessages(null, true, $"Notification Config:{id}", MessagesResults.Success, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, "Deleted successfully.");
            }
            else
            {
                GetExecutionMessages(null, false, $"Notification Config:{id}", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
            }
            return Task.FromResult(ExecutionMessage);
        }

        #endregion
    }
}