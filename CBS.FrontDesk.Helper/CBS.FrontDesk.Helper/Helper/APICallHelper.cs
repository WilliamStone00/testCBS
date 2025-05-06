using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.VariantTypes;

namespace CBS.FrontDesk.Helper
{
    public abstract class APICallHelper
    {

        //BlacklistAccounts
        public static string Get_Update_Delete_BlacklistAccount = "/api/v1/BlacklistAccount/{0}";
        public static string GetAllBlacklistAccount = "/api/v1/BlacklistAccounts";
        public static string CreateBlacklistAccount = "/api/v1/BlacklistAccount";

        //AccountBookingDirection
        public static string Get_Update_Delete_AccountBookingDirection = "/api/v1/AccountBookingDirection/{0}";
        public static string GetAllAccountBookingDirection = "/api/v1/AccountBookingDirections";
        public static string CreateAccountBookingDirection = "/api/v1/AccountBookingDirection";

        public static string GetAllFinancialDocument = "/api/v1/Documents";
        public static string GetlocationInforUrl = "/api/v1/Country/Location/CMR";
        //TrialBalanceFile  
        //GetAllowAnonymous,Update,Delete TrialBalanceFile By Id
        public static string Get_Update_Delete_TrialBalanceFile = "/api/v1/TrialBalanceFile/{0}";

        public static string GetAllUserDownLoads = "/api/v1/DownloadFileByUserIdQuery/{0}";
        public static string DownloadLoanFile = "/api/v1/FileDownloadInfos/download/{0}";
        public static string Get_Delete_ReportDownLoad = "/api/v1/ReportDownLoad/{0}";
        public static string GetAllReportDownLoad = "/api/v1/ReportDownLoads";
        public static string Get_TrialBalanceFile = "/api/v1/TrialBalanceFiles";
        public static string Get_DownloadedFile = "/api/v1/DownloadFileByIdQuery/{0}";
        /// <summary>
        /// CashMovementTracker
        /// </summary>
        //GetAllowAnonymous,Update,Delete AccountClass By Id
        public static string Get_CashMovementTracker = "/api/v1/CashMovementTracker/GetCashMovementTrackerQuery/{0}";
        //GetAllowAnonymous All AccountClass
        public static string Update_CashMovementTracker = "/api/v1/CashMovementTracker/UpdateCashMovementTrackerCommand/{0}";
        public static string Delete_CashMovementTracker = "/api/v1/CashMovementTracker/{0}";

        public static string GetAllCashMovementTracker = "/api/v1/CashMovementTracker/GetAllCashMovementTrackerQuery";
        // POST  Create a AccountClass
        public static string CreateCashMovementTracker = "/api/v1/CashMovementTracker/AddCashMovementTrackerCommand";
        /// <summary>
        /// CashMovementTrackerConfiguration
        /// </summary>
        //GetAllowAnonymous,Update,Delete AccountClass By Id
        public static string Get_CashMovementTrackerConfiguration = "/api/v1//CashMovementTrackingConfiguration/GetCashMovementTrackingConfigurationQuery/{0}";
        //GetAllowAnonymous All AccountClass
        public static string Update_CashMovementTrackerConfiguration = "/api/v1/CashMovementTrackingConfiguration/UpdateCashMovementTrackingConfigurationCommand/{0}";
        public static string Delete_CashMovementTrackerConfiguration = "/api/v1/CashMovementTrackingConfiguration/{0}";

        public static string GetAllCashMovementTrackerConfiguration = "/api/v1/CashMovementTrackingConfiguration/GetAllCashMovementTrackingConfigurationQuery";
        // POST  Create a AccountClass
        public static string CreateCashMovementTrackerConfiguration = "/api/v1/CashMovementTrackingConfiguration/AddCashMovementTrackingConfigurationCommand";

        public static string CreateAccountPolicy = "/api/v1/AccountPolicy";
        public static string GetAccountPolicies = "/api/v1/AccountPolicies";
        public static string Get_Update_Delete_AccountPolicy = "/api/v1/AccountPolicy/{0}";
        //Authentication
        public static string MFAAuthentication = "/api/v1/MFA/MFALogin";
        public static string Authentication = "/api/Authentication/Session/Login";
        public static string RefreshToken = " /api/Authentication/Session/RefreshToken";
        public static string SessionLogout = "/api/Session/logout";

        //CreateMemberAccountActivation
        public static string CreateMemberAccountActivation = "/api/v1/MemberAccountActivation";
        public static string Get_Update_Delete_MemberAccountActivation = "/api/v1/MemberAccountActivation/{0}";
        public static string GetAllMemberAccountActivation = "/api/v1/MemberAccountActivation";
        public static string GetMemberAccountActivationByCustomerID = "/api/v1/MemberAccountActivation/GetCustomerMemberAccountActivation/{0}";

        //CreateMemberAccountActivationPolicy
        public static string CreateMemberAccountActivationPolicy = "/api/v1/MemberAccountActivationPolicy";
        public static string Get_Update_Delete_MemberAccountActivationPolicy = "/api/v1/MemberAccountActivationPolicy/{0}";
        public static string GetAllMemberAccountActivationPolicy = "/api/v1/MemberAccountActivationPolicy";
        public static string GetMemberAccountActivationPolicyByCustomerID = "/api/v1/MemberAccountActivationPolicy/GetCustomerMemberAccountActivationPolicy/{0}";

        public static string Get_Update_Delete_CustomerCategory = "/api/v1/CustomerCategory/{0}";
        public static string GetAllCustomerCategory = "/api/v1/CustomerCategory";
        public static string CreateCustomerCategory = "/api/v1/CustomerCategory";

        public static string Get_Update_Delete_HolyDay = "/api/v1/HolyDay/{0}";
        public static string GetAllHolyDay = "/api/v1/HolyDays";
        public static string CreateHolyDay = "/api/v1/HolyDay";


        public static string Get_Or_Delete_AccountOpeningRule = "/api/v1/AccountOpeningRule/{0}";
        public static string GetAllAccountOpeningRule = "/api/v1/AccountOpeningRule/All";
        public static string UpdateAccountOpeningRule = "/api/v1/AccountOpeningRule/Update";
        public static string AddAccountOpeningRule = "/api/v1/AccountOpeningRule/Add";

        public static string Get_Vault = "/api/v1/Vault/{0}";
        public static string Update_Vault = "/api/v1/Vault/update";
        public static string GetAllVaultByBranch = "/api/v1/Vault/branch/{0}";
        public static string GetAllVault = "/api/v1/Vault/all";
        public static string CreateVault = "/api/v1/Vault/create";
        public static string VaultInitialize = "/api/v1/Vault/initialize";

        //CashCeilingRequest
        public static string Get_CashCeilingRequest = "/api/v1/CashCeilingRequest/{0}";
        public static string Delete_CashCeilingRequest = "/api/v1/CashCeilingRequest/{0}";
        public static string Update_CashCeilingRequest = "/api/v1/CashCeilingRequest/update";
        public static string GetAllCashCeilingRequest = "/api/v1/CashCeilingRequest/all";
        public static string CreateCashCeilingRequest = "/api/v1/CashCeilingRequest/create";
        public static string ValidateCashCeilingRequest = "/api/v1/CashCeilingRequest/validate";

        //StandingOrder
        public static string Get_StandingOrder = "/api/v1/StandingOrder/{0}";
        public static string Delete_StandingOrder = "/api/v1/StandingOrder/{0}";
        public static string Update_StandingOrder = "/api/v1/StandingOrder/update";
        public static string GetAllStandingOrder = "/api/v1/StandingOrder/all";
        public static string CreateStandingOrder = "/api/v1/StandingOrder/create";
        public static string Get_StandingOrderByMemberId = "/api/v1/StandingOrder/member/{0}";

        //SalaryUpload
        public static string Delete_SalaryUpload = "/api/v1/SalaryUpload/{0}";
        public static string GetAllSalaryUploadByFileCategory = "/api/v1/SalaryUpload/all/{0}";
        public static string GetAllSalaryUploadByFileUploadId = "/api/v1/SalaryUpload/all";
        public static string GetAllSalaryUploadWithBranchstatisticsByFileUploadId = "/api/v1/SalaryUpload/all/salary-model-withbranchstatistics";
        public static string CreateSalaryUpload = "/api/v1/SalaryUpload/upload";
        public static string DownloadSalaryUpload = "/api/v1/SalaryUpload/download-salay-file/{0}";
        public static string ActivateSalaryUpload = "/api/v1/SalaryUpload/Activate-salary-file";
        public static string GetAllSalaryUploadByFileBaseOnStatus = "/api/v1/SalaryUpload/salary-file-uploaded";
        public static string GetSalaryFileUploadByFileId = "/api/v1/SalaryUpload/get-file-upload-byid/{0}";
        public static string GetSalaryModelForBranchByFileId = "/api/v1/SalaryAnalysis/get-salary-model-for-branch-by-fileid/{0}";

        public static string GetMemberByCustomerType = "/api/v1/Customer/MembersByType/{0}";


        //SalaryExecution
        public static string ExecuteSalary = "/api/v1/SalaryExecution/execute-salary";
        public static string UploadAnalysedSalaryFile = "/api/v1/SalaryExecution/upload-analysed-file";
        public static string GetExecutedAnalyzedSalary = "/api/v1/SalaryExecution/salary-extracts";

        //SalaryAnalysis
        public static string Delete_SalaryAnalysis = "/api/v1/SalaryAnalysis/{0}";
        public static string GetSalaryAnalysisByFileUploadId = "/api/v1/SalaryAnalysis/by-file-upload-id/{0}";
        public static string GetSalaryAnalysisById = "/api/v1/SalaryAnalysis/{0}";
        public static string ExecuteSalaryAnalysis = "/api/v1/SalaryAnalysis/analyze";

        public static string Get_Update_Delete_HolyDayRecurring = "/api/v1/HolyDayRecurring/{0}";
        public static string GetAllHolyDayRecurring = "/api/v1/HolyDayRecurrings";
        public static string CreateHolyDayRecurring = "/api/v1/HolyDayRecurring";

        public static string DenominationCashChangePrimaryTeller = "/api/v1/CashChange/primary-teller";
        public static string DenominationCashChangeSubTeller = "/api/v1/CashChange/sub-teller";
        public static string DenominationCashChangeVault = "/api/v1/CashChange/vault";
        public static string GetDenominationHistories = "/api/v1/CashChange/history";
        public static string GetDenominationHistoryById = "/api/v1/CashChange/{0}";
        //User
        public static string GetUserByID = "/api/User/{0}";
        public static string DeleteUser = "/api/User/{0}";
        public static string GetUsers = "/api/User/GetAllUsers";
        public static string createUserUrl = "/api/User";
        public static string UpdateUserProfile = "/api/User/profile";
        public static string ChangePassword = "/api/User/changepassword";
        public static string ResetPassword = "/api/User/resetpassword";
        public static string MFAActivation = "/api/v1/MFA/Enable/Disactivate/MFA";
        public static string MFAVerification = "/api/v1/MFA/VerifyMFA";
        public static string FLoginChangePasswordCommand = "/api/User/FLoginChangePasswordCommand";
        public static string UploadProfilePhoto = "/api/User/UpdateUserProfilePhoto";
        public static string GetRecentRegisteredUsers = "/api/User/GetRecentlyRegisteredUsers";
        public static string UpdateUser = "/api/User/{0}";
        public static string GetUserSessions = "/api/UserSession/GetUser/Sessions/{0}";
        public static string GetUserSessionByUserNameAndCode = "/api/Session/by-session-code";
        public static string InvalidateAllActivetUsers = "/api/Session/invalidate-by-recovery-code";
        public static string GetCurrentIdletimeByBranch = "/api/v1/idletime/by-branch/{0}";
        public static string LogoutuserSessions = "/api/Session/logout";
        public static string GenerateRecoveryCode = "/api/Session/generate-recovery-code";


        //FLoginChangePasswordCommand
        //Role
        public static string Get_Update_Delete_Role = "/api/Role/{0}";
        public static string GetAllRoles = "/api/Role";
        public static string CreateRole = "/api/Role";
        //Role
        public static string GetMenuTransaltion = "/api/v1/menutranslation/{0}";
        public static string GetAllMenuTransaltion = "/api/v1/menutranslation/all";
        public static string AddMenuTransaltion = "/api/v1/menutranslation/add";
        public static string UpdateMenuTransaltion = "/api/v1/menutranslation/update";
        public static string GetUntreatedMenuTransaltion = "/api/v1/menutranslation/untranslated";
        //

        //UserPermission
        public static string Get_Update_Delete_UserPermission = "/api/UserPermission/{0}";
        public static string GetAllUserPermission = "/api/UserPermissions";
        public static string CreateUserPermission = "/api/UserPermission";
        public static string DeleteUserPermission = "/api/UserPermission/Delete";
        public static string GetUserPermissions = "/api/UserPermission/User/{0}";
        public static string GetAllUserRoles = "/api/RoleUsers/GetAllUserRoles";

        //RolePermission
        public static string Get_Update_Delete_RolePermission = "/api/RolePermission/{0}";
        public static string GetAllRolePermission = "/api/RolePermissions";
        public static string CreateRolePermission = "/api/RolePermission";
        public static string GetRolePermissions = "/api/RolePermission/Role/{0}";
        public static string DeleteRolePermisions = "/api/RolePermission/Delete";


        //MenuMaster
        public static string Get_Update_Delete_MenuMaster = "/api/MenuMaster/{0}";
        public static string GetAllMenuMaster = "/api/MenuMasters";
        public static string GetAllMenuMasterToAssignPermission = "/api/MenuMasters/AssignPemission";
        public static string CreateMenuMaster = "/api/MenuMaster";
        public static string GetMenuMasterByParentID = "/api/MenuMaster/Parent/{0}";
        public static string GetAssignPemissions = "/api/MenuMasters/AssignPemission";
        //CloseFeeParameter
        public static string Get_Update_Delete_CloseFeeParameter = "/api/v1/CloseFeeParameter/{0}";
        public static string GetAllCloseFeeParameter = "/api/v1/CloseFeeParameter";
        public static string CreateCloseFeeParameter = "/api/v1/CloseFeeParameter";
        //OldLoanAccountingMaping
        public static string Get_Update_Delete_OldLoanAccountingMaping = "/api/v1/OldLoanAccountingMaping/{0}";
        public static string GetAllOldLoanAccountingMaping = "/api/v1/OldLoanAccountingMaping";
        public static string CreateOldLoanAccountingMaping = "/api/v1/OldLoanAccountingMaping";
        //EntryFeeParameter
        public static string Get_Update_Delete_EntryFeeParameter = "/api/v1/EntryFeeParameter/{0}";
        public static string GetAllEntryFeeParameter = "/api/v1/EntryFeeParameter";
        public static string CreateEntryFeeParameter = "/api/v1/EntryFeeParameter";

        public static string InitiateBulkDownloadIndividualAccountBalances = "/api/v1/FileDownloadInfo/InitiateBulkDownloadIndividualAccountBalances";


        //TransactionReversal
        public static string Delete_GetReversalRequest = "/api/v1/ReversalRequest/{0}";
        public static string GetAllReversalRequest = "/api/v1/ReversalRequest/All";
        public static string CreateReversalRequest = "/api/v1/ReversalRequest/Request";
        public static string ValidateReversalRequest = "/api/v1/ReversalRequest/Validate";
        public static string ApprovedReversalRequest = "/api/v1/ReversalRequest/Approved";
        public static string TreatRequestReversalRequest = "/api/v1/ReversalRequest/TreatRequest";


        //ManagementFeeParameter
        public static string Get_Update_Delete_ManagementFeeParameter = "/api/v1/ManagementFeeParameter/{0}";
        public static string GetAllManagementFeeParameter = "/api/v1/ManagementFeeParameter";
        public static string CreateManagementFeeParameter = "/api/v1/ManagementFeeParameter";
        //Get_Update_Delete_WithdrawalNotification
        public static string Get_Update_Delete_WithdrawalNotification = "/api/v1/WithdrawalNotification/{0}";
        public static string GetAllWithdrawalNotification = "/api/v1/WithdrawalNotification";
        public static string CreateWithdrawalNotification = "/api/v1/WithdrawalNotification";
        public static string GetAllWithdrawalNotificationByCustomerId = "/api/v1/WithdrawalNotification/GetAllWithdrawalNotificationByCustomerId/{0}";
        public static string WithdrawalNotificationValidateNotification = "/api/v1/WithdrawalNotification/ValidateNotification/{0}";
        public static string PayinSavingWithdrawalNotification = "/api/v1/WithdrawalNotification/CashDeskWithdrawalNotificationCommand/{0}";
        //ReopenFeeParameter
        public static string Get_Update_Delete_ReopenFeeParameter = "/api/v1/ReopenFeeParameter/{0}";
        public static string GetAllReopenFeeParameter = "/api/v1/ReopenFeeParameter";
        public static string CreateReopenFeeParameter = "/api/v1/ReopenFeeParameter";
        //ChargesWaived
        public static string Get_Update_Delete_ChargesWaived = "/api/v1/ChargesWaived/{0}";
        public static string GetAllChargesWaived = "/api/v1/ChargesWaived";
        public static string CreateChargesWaived = "/api/v1/ChargesWaived";
        //SysConfiguration
        public static string Get_Update_Delete_SysConfiguration = "/api/v1/Config/{0}";
        public static string CreateSysConfiguration = "/api/v1/Config";
        public static string GetAllSysConfiguration = "/api/v1/Configs";
        //Customer
        ///api/v1/Customers/{BranchId}
        public static string CreateIndividualProfile = "/api/v1/Customer";
        public static string SearchByAnyCriterialQuery = "/api/v1/Customers/SearchByAnyCriterialQuery";
        public static string GetAllByParameters = "/api/v1/Customer/GetAllByParameters";
        public static string MembersDatatableQuery = "/api/v1/customers/datatable";


        public static string AllCustomers = "/api/v1/Customers";
        public static string GetAllIndividualProfileByBranch = "/api/v1/Customers/{0}";
        public static string SubcriptionAggregates = "/api/v1/SubcriptionAggregates";
        public static string DeleteCustomer = "/api/v1/Customer/{0}";
        public static string GetCustomerByID = "/api/v1/Customer/{0}";
        public static string UploadCustomerDocument = "/api/v1/uploadCustomerDocuments";
        public static string GetCustomerDefaultEnums = "/api/v1/SubscriptionAggregates/GetAll";
        public static string CreateMembershipNextOfKing = "/api/v1/MembershipNextOfKings";
        public static string CreateCardSignatureSpecimen = "/api/v1/CardSignatureSpecimen";

        public static string ResetPin = "/api/v1/Customer/Pin/Reset/{0}";

        public static string Get_Update_Delete_Question = "/api/v1/Questions/{0}";
        public static string CreateQuestion = "/api/v1/CreateQuestion";
        public static string GetAllQuestions = "/api/v1/Questions";


        //AndroidAppVersion
        public static string AddOrUpdateAndriodVersion = "/api/v1/androidapp/versions/add-or-update";
        public static string DeleteAndriodVersion = "/api/v1/androidapp/versions/{0}";
        public static string GetAndriodVersion = "/api/v1/androidapp/versions/{0}";
        public static string GetAndriodVersionByAppCode = "/api/v1/androidapp/versions/appcode/{0}";
        public static string GetAllAndriodVersions = "/api/v1/androidapp/versions";

        //ChangeCustomerPhone
        public static string ChangePhonuNumberRequest = "/api/v1/ChangeCustomerPhone/change-phone-number";
        public static string ApprovePhoneNumberRequest = "/api/v1/ChangeCustomerPhone/approve-phone-number";
        public static string GetPendingPhoneNumberChangeRequest = "/api/v1/ChangeCustomerPhone/History/{0}";
        public static string GetChangeCustomerPhone = "/api/v1/ChangeCustomerPhone/Get-History-byid/{0}";
        public static string DeletePhoneNumberChnageRequest = "/api/v1/ChangeCustomerPhone/{0}";

        //GroupType
        public static string Get_Update_Delete_GroupType = "/api/v1/GroupType/{0}";
        public static string CreateGroupType = "/api/v1/AddGroupType";
        public static string GetAllGroupTypes = "/api/v1/GroupTypes";
        public static string LoanApplicationFeesPending = "/api/v1/LoanApplicationFees/Pending/{0}";
        //

        //Group
        public static string Get_Update_Delete_Group = "/api/v1/Group/{0}";
        public static string CreateGroup = "/api/v1/Group/Add";
        public static string GetAllGroups = "/api/v1/Groups";
        public static string SearchByAnyCriterialGroupQuery = "/api/v1/Group/SearchByAnyCriterialGroupQuery";
        //GroupMembers
        public static string RemoveMemberFromGroup = "/api/v1/GroupCustomer/{0}";
        public static string AddMemberToGroup = "/api/v1/GroupCustomer/Add";
        public static string GetAllGroupMembers = "/api/v1/GetGroupCustomersByGroupId/{0}";

        //Saving
        public static string GetSavingProducts = "/api/v1/SavingProduct";
        public static string GetCustomerBalance = "/api/v1/Account/Balance/Customer/{0}";
        public static string UpdateIndividualProfile = "/api/v1/Customer/{0}";
        public static string ActivateOrDiactivateCustomer = "/api/v1/Customer/ActivateOrDis-activate";
        public static string GetAccountBalanceByAccountNumber = "/api/v1/Account/AccountNumber/{0}";

        public static string GetCustomerAccounts = "/api/v1/Account/Customer/{0}";
        public static string InitialDeposit = "/api/v1/Transaction/Initiate/{0}";
        public static string GetAllAccountsByBranchIdQuery = "/api/v1/Account/GetAllAccountsByBranchIdQuery/{0}";
        public static string GetAllAccounts = "/api/v1/Account";
        public static string GetRemittanceAccount = "/api/v1/Remittance/Account";
        public static string GetAllRemittanceAccounts = "/api/v1/Remittance/AccountsAsPerBranch/{0}";
        public static string GetMemberAccountByAccountID = "/api/v1/Account/{0}";

        public static string MakeDepoit = "/api/v1/Transaction/Deposit";
        public static string BulkDeposit = "/api/v1/Transaction/BulkDeposit";
        public static string GetAllMembersPagginatedSummaryAccounts = "/api/v1/Account/Pagginated/MembersAccountSummary";
        public static string GetAllMembersSummaryAccounts = "/api/v1/Account/MembersAccountSummary";
        //
        public static string MakeWithdrawal = "/api/v1/Transaction/Withdrawal";
        public static string MakeLoanRepayment = "/api/v1/Transaction/LoanRepaymentCommand";
        public static string TransferRequest = "/api/v1/Transaction/TransferRequestCommand";
        public static string TransferConfirmation = "/api/v1/Transaction/TransferConfirmationCommand";
        public static string GetTransfer = "/api/v1/Transfer/{0}";
        public static string GetPendingTransfers = "/api/v1/Transfer/Pending";
        public static string GetTransfers = "/api/v1/Transfers";
        public static string GetAllCustomerAccountsByCustomerId = "/api/v1/Account/Customer/{0}";
        ///api/v1/Account/Customer/{id}
        public static string MakeTrGetAccountByAccountNumber = "/api/v1/Account/AccountNumber/{0}";
        public static string GetTransactionHistoryByAccountNumber = "/api/v1/Transaction/AccountNumber/{0}";
        public static string GetTransactionHistoryByCustomerNumber = "/api/v1/Transaction/Customer/{0}";
        public static string GetAllTransactionsByDatesAndCustomerIDQuery = "/api/v1/Transaction/GetAllTransactionsByDatesAndCustomerIDQuery";
        public static string AccountMigration = "/api/v1/Account/AccountMigration";

        public static string GetTransactionsByQueryParameters = "/api/v1/Transaction/GetTransactionsByQueryParameters";
        public static string GetTransaction = "/api/v1/Transaction/{0}";
        //Teller
        public static string PrimaryTellerProvisioning = "/api/v1/Teller/Dinomination/Provisioning/PrimaryTeller";
        public static string SubTellerProvisioning = "/api/v1/Teller/Provision/SubTeller";
        public static string SubTellerEndOfDay = "/api/v1/Teller/SubTeller/EndOfDay";
        public static string PrimaryTellerEndOfDay = "/api/v1/Teller/PrimaryTeller/EndOfDay";
        public static string EndOfDayAccountant = "/api/v1/Teller/EndOfDayAccountant";
        public static string GetTellerAccountInfo = "/api/v1/Account/GetTellerAccount";
        public static string OpenningOfDayPrimaryTeller = "/api/v1/Teller/Primary/OpenningOfTheDay";
        public static string OpenningOfDaySubTeller = "/api/v1/Teller/SubTeller/OpenningOfTheDay";
        public static string GetTellerDailyOperations = "/api/v1/TellerOperations/DailyOperations";
        public static string TellerOpenningAndClossingQuery = "/api/v1/Teller/TellerOpenningAndClossingQuery";
        public static string GetTillCashStatus = "/api/v1/Teller/TillStatus";


        //
        //
        public static string GetSubTellerProvisioningHistoryByUserIncharge = "/api/v1/Teller/GetSubTellerProvisioningHistoryByUserIncharge/{0}";
        public static string GetPrimaryTellerProvisioningHistoryByUserIncharge = "/api/v1/Teller/GetPrimaryTellerProvisioningHistoryByUserIncharge/{0}";
        public static string GetAllSubTellerProvioningHistoryQuery = "/api/v1/Teller/GetAllSubTellerProvioningHistoryQuery";
        public static string GetAllPrimaryTellerProvisioningHistoryBetweenDatesQuery = "/api/v1/Teller/GetAllPrimaryTellerProvisioningHistoryBetweenDatesQuery";
        public static string GetAllSubTellerProvioningHistoryBetweenDatesQuery = "/api/v1/Teller/GetAllSubTellerProvioningHistoryBetweenDatesQuery";
        public static string GetAllPrimaryTellerProvisioningHistoryQuery = "/api/v1/Teller/GetAllPrimaryTellerProvisioningHistoryQuery";
        public static string CurrentOpenOfDayHistory = "/api/v1/Teller/CurrentOpenOfDayHistory";
        public static string GetSubTellerProvioningHistoryQuery = "/api/v1/Teller/GetSubTellerProvioningHistoryQuery/{0}";
        public static string GetPrimaryTellerProvisioningHistoryQuery = "/api/v1/Teller/GetPrimaryTellerProvisioningHistoryQuery/{0}";
        public static string GetSubTellerProvioningHistoryByPrimaryTellerUserIDQuery = "/api/v1/Teller/GetSubTellerProvioningHistoryByPrimaryTellerUserIDQuery/{0}";
        public static string EndOfDaySubTellerBYPrimaryTeller = "/api/v1/Teller/EndOfDaySubTellerBYPrimaryTeller";
        public static string GetPrimaryTellerProvisioningHistoryByBranchIDQuery = "/api/v1/Teller/GetPrimaryTellerProvisioningHistoryByBranchIDQuery/{0}";
        public static string GetCustomersByBranchID = "/api/v1/Customers/{0}";

        ///api/v1/Teller/SubTellerEndOfDay
        //GetPrimaryTellerProvisioningHistoryByBranchIDQuery
        public static string TellerCashReplenishmentRequest = "/api/v1/CashReplenishment";
        public static string TellerCashReplenishmentRequestApproval = "/api/v1/CashReplenishment/{0}";
        //Account
        public static string AddCustomerSavingAccount = "/api/v1/Account";


        //Teller
        public static string Get_Update_Delete_Teller = "/api/v1/Teller/{0}";
        public static string MobileMoneyTellerConfiguration = "/api/v1/Teller/MobileMoney/Configuration/{0}";
        public static string GetAllTeller = "/api/v1/Teller";
        public static string CreateTeller = "/api/v1/Teller";
        //DailyTeller
        public static string Get_Update_Delete_DailyTeller = "/api/v1/DailyTeller/{0}";
        public static string GetDailyTellerUser = "/api/v1/DailyTeller/User";
        public static string GetAllDailyTeller = "/api/v1/DailyTeller/All";
        public static string GetAllDailyTellerByBranch = "/api/v1/DailyTeller/Branch";
        public static string CreateDailyTeller = "/api/v1/DailyTeller";


        //Accounting DayHF


        public static string OpenOfAccountingDay = "/api/v1/AccountingDay/Open";
        public static string CloseOfAccountingDay = "/api/v1/AccountingDay/Close";
        public static string GetAllOpenedAccountingDays = "/api/v1/AccountingDay/GetAllOpenAccountingDays";
        public static string GetCurrentAccountingDay = "/api/v1/AccountingDay/GetCurrentDay/{0}";
        public static string GetAccountingDayById = "/api/v1/AccountingDay/{0}";
        public static string DeleteAccountingDay = "/api/v1/AccountingDay/Delete";
        public static string AccountingDayActionsCommand = "/api/v1/AccountingDay/AccountingDayActionsCommand";

        //PrimaryTellerCashReplenishment
        public static string Get_Update_Delete_PrimaryTellerCashReplenishment = "/api/v1/PrimaryTellerCashReplenishment/{0}";
        public static string GetAllPrimaryTellerCashReplenishment = "/api/v1/PrimaryTellerCashReplenishment/GetAllRequest";
        public static string GetAllPrimaryTellerCashReplenishmentByBranch = "/api/v1/PrimaryTellerCashReplenishment/ReplenishmentByBranch";
        public static string ValidatePrimaryTellerCashReplenishment = "/api/v1/PrimaryTellerCashReplenishment/Topup";
        public static string PrimaryTellerCashReplenishmentRequest = "/api/v1/PrimaryTellerCashReplenishment/Request";
        public static string GetAllApendingPrimaryTellerCashReplenishment = "/api/v1/PrimaryTellerCashReplenishment/AllPendingProvisions";


        //PrimaryTellerCashReplenishment/AllPendingProvisions
        //SubTellerCashReplenishment
        public static string Get_Update_Delete_SubTellerCashReplenishment = "/api/v1/SubTellerCashReplenishment/Request/{0}";
        public static string GetAllSubTellerCashReplenishment = "/api/v1/SubTellerCashReplenishment/GetAllRequest";
        public static string GetAllSubTellerCashReplenishmentByBranch = "/api/v1/SubTellerCashReplenishment/GetRequestByBranch";
        public static string CreateSubTellerCashReplenishment = "/api/v1/SubTellerCashReplenishment/Request";
        public static string ValidateSubTellerCashReplenishment = "/api/v1/SubTellerCashReplenishment/RequestValidation/{0}";
        public static string GetAllApendingSubTellerCashReplenishment = "/api/v1/SubTellerCashReplenishment/Request/Pending";

        //MobileMoneyCashTopup
        public static string GetMobileMoneyCashTopup = "/api/v1/MobileMoneyCashTopup/Request/{0}";
        public static string GetAllMobileMoneyCashTopup = "/api/v1/MobileMoneyCashTopup/Requests";
        public static string CreateMobileMoneyCashTopup = "/api/v1/MobileMoneyCashTopup/Request";
        public static string ValidateMobileMoneyCashTopup = "/api/v1/MobileMoneyCashTopup/RequestValidation/{0}";
        public static string DeleteMobileMoneyCashTopup = "/api/v1/MobileMoneyCashTopup/Request/Delete/{0}";

        //SubTellerCashReplenishment/Request/Pending
        //OtherTransaction
        public static string Get_Update_Delete_OtherTransaction = "/api/v1/OtherTransaction/{0}";
        public static string GetAllOtherTransaction = "/api/v1/OtherTransaction";
        public static string CreateOtherTransaction = "/api/v1/OtherTransaction";
        public static string GetMemberOnboardingDetails = "/api/v1/MemberOnboarding/GetDetail";
        public static string MobileMoneyNoneCashCashIn = "/api/v1/OtherTransaction/MobileMoney-None-Cash-Cash-In";

        public static string CreateOtherTransactionMobileMoney = "/api/v1/OtherTransaction/MobileMoney";
        //SavingProduct
        public static string Get_Update_Delete_SavingProduct = "/api/v1/SavingProduct/{0}";
        public static string GetAllSavingProducts = "/api/v1/SavingProduct";
        public static string CreateSavingProduct = "/api/v1/SavingProduct";

        //DepositLimit
        public static string Get_Update_Delete_DepositLimit = "/api/v1/DepositLimits/{0}";
        public static string GetAllDepositLimits = "/api/v1/DepositLimits";
        public static string CreateDepositLimit = "/api/v1/DepositLimits";
        //TransferLimits
        public static string Get_Update_Delete_TransferLimits = "/api/v1/TransferLimits/{0}";
        public static string GetAllTransferLimits = "/api/v1/TransferLimits";
        public static string CreateTransferLimits = "/api/v1/TransferLimits";
        //WithdrawalLimitss
        public static string Get_Update_Delete_WithdrawalLimits = "/api/v1/WithdrawalLimits/{0}";
        public static string GetAllWithdrawalLimits = "/api/v1/WithdrawalLimits";
        public static string CreateWithdrawalLimits = "/api/v1/WithdrawalLimits";

        //Configuration
        public static string GetAllConfigurationEnums = "/api/v1/Config/EnumData";
        public static string LoanProductEnumAggregates = "/api/v1/EnumAggregates";
        ///api/v1/EnumAggregates
        //Country
        public static string Get_Update_Delete_Country = "/api/v1/Country/{0}";
        public static string GetAllCountry = "/api/v1/Countrys";
        public static string CreateCountry = "/api/v1/Country";

        //Country
        public static string Get_Update_Delete_Region = "/api/v1/Region/{0}";
        public static string GetAllRegion = "/api/v1/Regions";
        public static string CreateRegion = "/api/v1/Region";
        //Organization
        public static string Get_Update_Delete_Organization = "/api/v1/Organization/{0}";
        public static string GetAllOrganization = "/api/v1/Organizations";
        public static string CreateOrganization = "/api/v1/Organization";
        //Subdivision
        public static string Get_Update_Delete_Subdivision = "/api/v1/Subdivision/{0}";
        public static string GetAllSubdivision = "/api/v1/Subdivisions";
        public static string CreateSubdivision = "/api/v1/Subdivision";
        //Town
        public static string Get_Update_Delete_Town = "/api/v1/Town/{0}";
        public static string GetAllTown = "/api/v1/Towns";
        public static string CreateTown = "/api/v1/Town";
        //EconomicActivity
        public static string Get_Update_Delete_EconomicActivity = "/api/v1/EconomicActivity/{0}";
        public static string GetAllEconomicActivity = "/api/v1/EconomicActivitys";
        public static string CreateEconomicActivity = "/api/v1/EconomicActivity";
        //Division
        public static string Get_Update_Delete_Division = "/api/v1/Division/{0}";
        public static string GetAllDivision = "/api/v1/Divisions";
        public static string CreateDivision = "/api/v1/Division";
        //Currency
        public static string Get_Update_Delete_Currency = "/api/v1/Currency/{0}";
        public static string GetAllCurrency = "/api/v1/Currencys";
        public static string CreateCurrency = "/api/v1/Currency";
        //Branch
        public static string Get_Update_Delete_Branch = "/api/v1/Branch/{0}";
        public static string GetBranchesByBankID = "api/v1/Branch/GetBranchsByBank/{0}";
        ///api/v1/Branch/GetBranchsByBank/{bankid}
        public static string GetAllBranch = "/api/v1/Branchs";
        public static string CreateBranch = "/api/v1/Branch";
        //Bank
        public static string Get_Update_Delete_Bank = "/api/v1/Bank/{0}";
        public static string GetAllBank = "/api/v1/Banks";
        public static string CreateBank = "/api/v1/Bank";


        //Loan
        public static string Get_Update_Delete_Document = "/api/v1/Document/{0}";
        public static string GetAllDocument = "/api/v1/Documents";
        public static string CreateDocument = "/api/v1/Document";
        public static string Get_Update_Delete_DocumentPack = "/api/v1/DocumentPack/{0}";
        public static string GetAllDocumentPack = "/api/v1/DocumentPacks";
        public static string CreateDocumentPack = "/api/v1/DocumentPack";
        public static string Get_Update_Delete_Guaranty = "/api/v1/Guaranty/{0}";
        public static string GetAllGuaranty = "/api/v1/Guaranties";
        public static string CreateGuaranty = "/api/v1/Guaranty";
        public static string Get_Update_Delete_GuarantyPack = "/api/v1/GuarantyPack/{0}";
        public static string GetAllGuarantyPack = "/api/v1/GuarantyPacks";
        public static string CreateGuarantyPack = "/api/v1/GuarantyPack";
        public static string Get_Update_Delete_LoanProductCategory = "/api/v1/LoanProductCategory/{0}";
        public static string GetAllLoanProductCategory = "/api/v1/LoanProductCategorys";
        public static string CreateLoanProductCategory = "/api/v1/LoanProductCategory";
        public static string AttachedDocuments = "/api/FIleManagement/Upload";
        public static string AttachedDocumentsRemotelyLoan = "/api/v1/DocumentAttachedToLoan/AddLoanAttachedDocumentCallBackCommand";
        public static string AttachedDocumentsRemotelyBank = "/api/v1/Bank/UpdateBankLogoCallBack";

        public static string Get_Update_Delete_LoanTerm = "/api/v1/LoanTerm/{0}";
        public static string GetAllLoanTerm = "/api/v1/LoanTerms";
        public static string CreateLoanTerm = "/api/v1/LoanTerm";


        public static string Get_Update_Delete_InstallmentType = "/api/v1/InstallmentType/{0}";
        public static string GetAllInstallmentType = "/api/v1/InstallmentTypes";
        public static string CreateInstallmentType = "/api/v1/InstallmentType";
        public static string Get_Delete_DocumentAttachedToLoan = "/api/v1/DocumentAttachedToLoan/{0}";
        public static string GetAllAttachedments = "/api/v1/DocumentAttachedToLoans";
        public static string Get_Update_Delete_LoanProduct = "/api/v1/LoanProduct/{0}";
        public static string GetAllMembersCurrentLoans = "/api/v1/LoanProduct/{0}";
        public static string GetAllLoanProduct = "/api/v1/LoanProducts";
        public static string GetAllLoanProductLighterVersion = "/api/v1/LoanProducts/Lighterversion";
        public static string CreateLoanProduct = "/api/v1/LoanProduct";
        public static string GetLoanProductConfigurationAgregates = "/api/v1/LoanProductConfigurationAgregates";
        public static string GetAllLoanApplicationByCustomerId = "/api/v1/LoanApplication/GetAllLoanApplicationByCustomerId/{0}";
        public static string AttachedDocumentsRemotelyMembers = "/api/v1/uploadCustomerDocuments";

        //

        //Upload FIles
        public static string UploadFile = "/api/FIleManagement/Upload";
        public static string UpdateBankLogo = "/api/v1/UploadBankLogo";
        public static string UpdateBranchLogo = "/api/v1/UploadBranchLogo";
        ///api/v1/Loan/Disbursed
        //Loan Application
        public static string Get_Update_Delete_LoanApplication = "/api/v1/LoanApplication/{0}";
        public static string GetAllLoanApplicationByParameter = "/api/v1/LoanApplications/Query/{0}";
        public static string CreateLoanApplication = "/api/v1/LoanApplication";
        public static string GetLoanApplicationInstallment = "/api/v1/LoanApplication/Installment/{0}";
        public static string LoanSimulation = "/api/v1/LoanApplication/Simulation";
        public static string ApproveLoanApplication = "/api/v1/LoanApplication/ChangeStatus/{0}";
        public static string ValidateLoanApplicationStatus = "/api/v1/LoanApplication/ChangeStatus/{0}";
        public static string GenerateAmortizationSchedule = "/api/LoanAmortization/Loan/Simulation";
        public static string Disbursed = "/api/v1/Loan/Disbursed";
        public static string GenerateOTP = "/api/v1/OTPNotification";
        public static string GetLoanApplicationDatatable = "/api/v1/LoanApplication/Paggination-DataTable";
        //

        //LOAN
        public static string GetAllLoanByCustomerId = "/api/v1/Loan/GetAllLoanByCustomerId";
        public static string GetLoan = "/api/v1/Loan/{0}";
        public static string GetLoanPortFolio = "/api/v1/Loan/generate-delinquent-loan-report";
        public static string GetLoans = "/api/v1/Loans/GetAllLoans";
        public static string InitiateBulkDownloadLoans = "/api/v1/FileDownloadInfo/InitiateBulkDownloadLoans";
        public static string BulkDownloadDeleteAndGetLoan = "/api/v1/FileDownloadInfo/{0}";
        public static string GetAllBulkDownloadInfosLoan = "/api/v1/FileDownloadInfos";
        public static string GelLoansSearchByAnyCriterialQuery = "/api/v1/Loan/SearchByAnyCriterialQuery";
        public static string GetAllBulkDownloadInfosLoanPerUser = "/api/v1/FileDownloadInfos/GetAllFileDownloadInfoByUserIdQuery/{0}";
        public static string DownloadFileByIdQuery = "/api/v1/FileDownloadInfos/download/{0}";

        public static string LoaDataTablePaggination = "/api/v1/Loan/Paggination-DataTable";


        public static string LoaDataTablePagginationFroCmoney = "/api/v1/cmoney/members/activations/datatable";
        public static string LoadDataTablePagginationForUsers = "/api/User/datatable";
        public static string LoadDataTablePagginationForUserSessions = "/api/Session/datatable";
        public static string LoadDataTablePagginationForUserSession = "/api/Session/datatable";

        ///api/v1/FileDownloadInfos/GetAllFileDownloadInfoByUserIdQuery/{userid}
        ///api/v1/Loan/SearchByAnyCriterialQuery
        public static string GetAllLoanByBranchIdQuery = "/api/v1/Loan/GetAllLoanByBranchIdQuery/{id}";

        //LOAN Amortization
        public static string GetLoanAmortizationByID = "/api/LoanAmortization/GetLoanAmortization/{0}";
        public static string GetAllLoanAmortizationByLoanIdQuery = "/api/LoanAmortization/Installments/GetAllLoanAmortizationByLoanIdQuery/{0}";
        public static string LoanInstallationSimulation = "/api/LoanAmortization/Loan/Simulation";

        //Tax  
        public static string Get_Update_Delete_Tax = "/api/v1/Tax/{0}";
        public static string GetAllTax = "/api/v1/Taxs";
        public static string CreateTax = "/api/v1/Tax";
        //LoanOtherFee
        public static string Get_Update_Delete_FeeRange = "/api/v1/FeeRange/{0}";
        public static string GetAllFeeRange = "/api/v1/FeeRanges";
        public static string CreateFeeRange = "/api/v1/FeeRange";
        //Collateral
        public static string Get_Update_Delete_Collateral = "/api/v1/Collateral/{0}";
        public static string GetAllCollateral = "/api/v1/Collaterals";
        public static string CreateCollateral = "/api/v1/Collateral";
        //LoanCollateral
        public static string Get_Update_Delete_LoanCollateral = "/api/v1/LoanCollateral/{0}";
        public static string GetAllLoanCollateral = "/api/v1/LoanCollaterals";
        public static string CreateLoanCollateral = "/api/v1/LoanCollateral";

        //LoanProductCollateral
        public static string Get_Update_Delete_LoanProductCollateral = "/api/v1/LoanProductCollateral/{0}";
        public static string GetAllLoanProductCollateral = "/api/v1/LoanProductCollaterals";
        public static string CreateLoanProductCollateral = "/api/v1/LoanProductCollateral";
        public static string GetAllLaonApplicationCollateralByApplicationIdQuery = "/api/v1/GetAllLaonApplicationCollateralByApplicationIdQuery/{0}";

        //LoanCommentry
        public static string Get_Update_Delete_LoanCommentry = "/api/v1/LoanCommentry/{0}";
        public static string GetAllLoanCommentry = "/api/v1/LoanCommentrys";
        public static string CreateLoanCommentry = "/api/v1/LoanCommentry";
        //LoanCommeteeMember
        public static string Get_Update_Delete_LoanCommeteeMember = "/api/v1/LoanCommeteeMember/{0}";
        public static string GetAllLoanCommeteeMember = "/api/v1/LoanCommeteeMembers";
        public static string CreateLoanCommeteeMember = "/api/v1/LoanCommeteeMember";
        //LoanCommiteeValidation
        public static string Get_Update_Delete_LoanCommiteeValidation = "/api/v1/LoanCommiteeValidation/{0}";
        public static string GetAllLoanCommiteeValidation = "/api/v1/LoanCommiteeValidations";
        public static string CreateLoanCommiteeValidation = "/api/v1/LoanCommiteeValidation";
        //LoanCommiteeValidationCriteria
        public static string Get_Update_Delete_LoanCommiteeValidationCriteria = "/api/v1/LoanCommiteeValidationCriteria/{0}";
        public static string GetAllLoanCommiteeValidationCriteria = "/api/v1/LoanCommiteeValidationCriterias";
        public static string CreateLoanCommiteeValidationCriteria = "/api/v1/LoanCommiteeValidationCriteria";
        //LoanGuarantor
        public static string Get_Update_Delete_LoanGuarantor = "/api/v1/LoanGuarantor/{0}";
        public static string GetAllLoanGuarantor = "/api/v1/LoanGuarantors";
        public static string CreateLoanGuarantor = "/api/v1/LoanGuarantor";
        //OtherFee
        public static string Get_Update_Delete_OtherFee = "/api/v1/OtherFee/{0}";
        public static string GetAllOtherFee = "/api/v1/OtherFees";
        public static string CreateOtherFee = "/api/v1/OtherFee";
        //LoanDeliquencyConfiguration
        public static string Get_Update_Delete_LoanDeliquencyConfiguration = "/api/v1/LoanDeliquencyConfiguration/{0}";
        public static string GetAllLoanDeliquencyConfiguration = "/api/v1/LoanDeliquencyConfigurations";
        public static string CreateLoanDeliquencyConfiguration = "/api/v1/LoanDeliquencyConfiguration";
        //FundingLine
        public static string Get_Update_Delete_FundingLine = "/api/v1/FundingLine/{0}";
        public static string GetAllFundingLine = "/api/v1/FundingLines";
        public static string CreateFundingLine = "/api/v1/FundingLine";
        //WriteOffLoanConfiguration
        public static string Get_Update_Delete_WriteOffLoan = "/api/v1/WriteOffLoanConfiguration/{0}";
        public static string GetAllWriteOffLoans = "/api/v1/WriteOffLoanConfigurations";
        public static string CreateWriteOffLoan = "/api/v1/WriteOffLoanConfiguration";

        //Penalty
        public static string Get_Update_Delete_Penalty = "/api/v1/Penalty/{0}";
        public static string GetAllPenalty = "/api/v1/Penalties";
        public static string CreatePenalty = "/api/v1/Penalty";
        //LoanPurpose
        public static string Get_Update_Delete_LoanPurpose = "/api/v1/LoanPurpose/{0}";
        public static string GetAllLoanPurpose = "/api/v1/LoanPurposes";
        public static string CreateLoanPurpose = "/api/v1/LoanPurpose";
        //Fee
        public static string Get_Update_Delete_Fee = "/api/v1/Fee/{0}";
        public static string GetAllFee = "/api/v1/Fees";
        public static string CreateFee = "/api/v1/Fee";

        //Remittance
        public static string Get_Update_Delete_Remittance = "/api/v1/Remittance/Request/{0}";
        public static string RemittanceRequestValidation = "/api/v1/Remittance/RequestValidation/{0}";
        public static string CreateRemittanceRequest = "/api/v1/Remittance/Request";
        public static string GetRemittancesByQUeryParameters = "/api/v1/Remittance/GetAll";
        public static string GetRemittanceCharge = "/api/v1/Remittance/Charge";
        public static string GetAllRemittanceRequests = "/api/v1/Remittance/Requests";
        public static string GenerateOTPRemittance = "/api/v1/Remittance/GenerateRemittanceOTP";
        public static string GetRemittanceDataTable = "/api/v1/datatable";



        //GeneralDailyDashboard
        public static string GetAllGeneralDailyDashboard = "/api/v1/Dashboard/GetAllGeneralDailyDashboard";
        public static string GetGeneralDailyDashboardByBranch = "/api/v1/Dashboard/GetGeneralDailyDashboardByBranch";
        public static string GetAllGeneralDailySummaryDashboard = "/api/v1/Dashboard/GetAllGeneralDailySummaryDashboard";
        public static string GetAllMembersDashboard = "/api/v1/Dashboard/GetAllMembersDashboard";
        public static string GetAllAccountsDashboard = "/api/v1/Dashboard/GetAllAccountsDashboard";
        public static string GetAllAccountingDashboard = "/api/v1/DashboardStatisticsDto";
        public static string GetLoanDashboardQuery = "/api/v1/Dashboard/GetLoanDashboardQuery";
        //FeePolicy
        public static string Get_Update_Delete_FeePolicy = "/api/v1/FeePolicy/{0}";
        public static string GetAllFeePolicy = "/api/v1/FeePolicys";
        public static string CreateFeePolicy = "/api/v1/FeePolicy";


        //CMoney
        public static string CMoneyMemberActivation = "/api/v1/cmoney/members/activate";
        public static string CMoneyMemberResetPin = "/api/v1/cmoney/members/reset-pin";
        public static string CMoneyMemberResepinWithSecurity = "/api/v1/cmoney/members/reset-pin-with-security";
        public static string CMoneyMemberActivationUpdate = "/api/v1/cmoney/members/update-activation";
        public static string CMoneyMemberDeactivation = "/api/v1/cmoney/members/deactivate";
        public static string CMoneyMemberReactivation = "/api/v1/cmoney/members/reactivate";
        public static string CMoneyGetMember = "/api/v1/cmoney/members/{0}";
        public static string CMoneyGetMembers = "/api/v1/cmoney/members";
        public static string CMoneySetMembersSecretQuestion = "/api/v1/cmoney/members/secret-question";
        public static string CMonetGenerateOTP = "/api/v1/cmoney/members/generate-otp";
        public static string CMoneyGetMembersPaggination = "/api/v1/cmoney/Customers/CMoneyMembersPagginationQuery";
        public static string CMoneyChangePhoneNumber = "/api/v1/cmoney/members/change-phone-number";



        //SavingProductFee
        public static string Get_Update_Delete_SavingProductFee = "/api/v1/SavingProductFee/{0}";
        public static string GetAllSavingProductFee = "/api/v1/SavingProductFees";
        public static string CreateSavingProductFee = "/api/v1/SavingProductFee";
        //CustomerLoanAccount
        public static string Get_Update_Delete_CustomerLoanAccount = "/api/v1/CustomerLoanAccount/{0}";
        public static string GetAllCustomerLoanAccount = "/api/v1/CustomerLoanAccounts";
        public static string CreateCustomerLoanAccount = "/api/v1/CustomerLoanAccount";
        //AlertProfile
        public static string Get_Update_Delete_AlertProfile = "/api/v1/AlertProfile/{0}";
        public static string GetAllAlertProfile = "/api/v1/AlertProfiles";
        public static string CreateAlertProfile = "/api/v1/AlertProfile";


        //Accounting Roles and Event
        public static string GetAllAccountingRules = "/api/v1/AccountingRules";
        public static string GetAllOperationServicesAccountingRuleEntryQuery = "/api/v1/AccountingEntry/OperationServicesAccountingRuleEntryQuery/{0}";
        //
        //OperationEvent
        //GetAllowAnonymous,Update,Delete OperationEvent By Id
        public static string Get_Update_Delete_OperationEvent = "/api/v1/OperationEvent/{0}";
        //GetAllowAnonymous All OperationEvent
        public static string GetAllOperationEvent = "/api/v1/OperationEvent";
        // POST  Create a OperationEvent
        public static string CreateOperationEvent = "/api/v1/OperationEvent";


        //OperationEventAttribute
        //GetAllowAnonymous,Update,Delete OperationEventAttribute By Id
        public static string Get_Update_Delete_OperationEventAttribute = "/api/v1/OperationEventAttribute/{0}";
        //GetAllowAnonymous All OperationEventAttribute
        public static string GetAllOperationEventAttribute = "/api/v1/OperationEventAttributes";
        // POST  Create a OperationEventAttribute
        public static string CreateOperationEventAttribute = "/api/v1/OperationEventAttribute";

        //AccountingRule
        //GetAllowAnonymous,Update,Delete AccountingRule By Id
        public static string Get_Update_Delete_AccountingRule = "/api/v1/AccountingRule/{0}";
        //GetAllowAnonymous All AccountingRule
        public static string GetAllAccountingRule = "/api/v1/AccountingRules";
        // POST  Create a AccountingRule
        public static string CreateAccountingRule = "/api/v1/AccountingRule";

        public static string CreateAccountingRules = "/api/v1/AccountingRules";
        // POST  Create a AccountingRule
        public static string CreateAccounOnUploadie = "/api/v1/Account/UploadAccountCommand";
        //AccountingRuleEntry
        //GetAllowAnonymous,Update,Delete AccountingRuleEntry By Id/api/v1/AccountingRuleEntry/GetAccountingRuleEntryByEventCodeQuery/Vault_To_Liaison
        public static string Get_Update_Delete_AccountingRuleEntry = "/api/v1/AccountingRuleEntry/{0}";

        //GetAllowAnonymous,Update,Delete AccountingRuleEntry By Id/api/v1/AccountingRuleEntry/GetAccountingRuleEntryByEventCodeQuery/Vault_To_Liaison
        public static string Get_AccountingRuleEntryByEventCode = "/api/v1/AccountingRuleEntry/GetAccountingRuleEntryByEventCodeQuery/{0}";
        //GetAllowAnonymous All OperationEventAttribute
        public static string GetAllAccountingRuleEntry = "/api/v1/AccountingRuleEntry";
        // POST  Create a OperationEventAttribute
        public static string CreateAccountingRuleEntry = "/api/v1/AccountingRuleEntry";
        //GetAllowAnonymous All OperationEventAttribute
        public static string GetAccountingRuleEntries = "/api/v1/AccountingEntry/LoadAccountingRuleEntryQuery";
        //AccountClass
        //GetAllowAnonymous,Update,Delete AccountClass By Id
        public static string Get_Update_Delete_AccountClass = "/api/v1/AccountClass/{0}";
        //GetAllowAnonymous All AccountClass
        public static string GetAllAccountClass = "/api/v1/AccountClasss";
        // POST  Create a AccountClass
        public static string CreateAccountClass = "/api/v1/AccountClass";

        //AccountCategory
        //GetAllowAnonymous,Update,Delete AccountCategory By Id
        public static string Get_Update_Delete_AccountCategory = "/api/v1/AccountCartegory/{0}";

        public static string Get_AccountClassCategory = "/api/v1/AccountClassCartegory/{0}";
        //GetAllowAnonymous All AccountCategory
        public static string GetAllAccountCategory = "/api/v1/AccountCartegories";
        // POST  Create a AccountClass
        public static string CreateAccountCategory = "/api/v1/AccountCartegory";

        public static string GetProductAccountingBookUrl = "/api/v1/ProductAccountingBook/{0}";

        public static string GetProductAccountingBookbyproductnameUrl = "/api/v1/ProductAccountingBook/GetProductAccountingConfigurations/{0}";
        public static string GetProductAccountingBookbyproductTypeUrl = "/api/v1/ProductAccountingBook/GetAllProductAccountingConfigurations/{0}";
        //AccountCategory AccountProduct {name}
        //GetAllowAnonymous,Update,Delete AccountCategory By Id
        public static string Get_Update_Delete_ChartOfAccountManagementPosition = "/api/v1/ChartOfAccountManagementPosition/{0}";
        public static string Get_ChartOfAccountManagementPosition = "/api/v1/ChartOfAccountManagementPosition/{0}";
        public static string GetAllChartOfAccountManagementPosition = "/api/v1/ChartOfAccountManagementPositions";
        public static string DownloadChartOfAccountManagementPositionUrl = "/api/v1/ChartOfAccountManagementPositions/DownloadQuery";
        public static string CreateChartOfAccountManagementPosition = "/api/v1/ChartOfAccountManagementPosition";

        //DocumentType
        public static string Create_DocumentType = "/api/v1/DocumentType";
        public static string Get_Update_Delete_DocumentType = "/api/v1/DocumentType/{0}";
        public static string GetAllDocumentTypes = "/api/v1/DocumentType";

        //DocumentReference   

        public static string Get_DocumentType_By_DocumentReference = "/api/v1/DocumentTypeByDocumentReference/{0}";
        public static string UpdateDocumentReferenceCode = "/api/v1/FinancialReport/DocumentReferenceCode/Update/{0}";
        public static string Delete_DocumentReference = "/api/v1/FinancialReport/DocumentReferenceCode/Delete/{0}";
        public static string GetDocumentReferenceCode = "/api/v1/FinancialReport/GetDocumentReferenceCodeById/{0}";
        public static string GetAllDocumentReferenceCode = "/api/v1/FinancialReport/DocumentReferenceCode";
        public static string CreateDocumentReferenceCode = "/api/v1/FinancialReport/DocumentReferenceCode/Create";
        public static string GetCorrespondingmappingByDocumentRefenceId = "/api/v1/FinancialReport/GetCorrespondingMappingByDocumentReferenceCodeIdQuery/{0}";
        public static string GetCorrespondingmappingExceptionByDocumentRefenceId = "/api/v1/FinancialReport/GetCorrespondingMappingExceptionsByDocumentReferenceCodeIdQuery/{0}";




        //CorrespondingMapping 

        public static string Get_Update_Delete_CorrespondingMapping = "/api/v1/CorrespondingMapping/{0}";
        public static string Get_CorrespondingMapping = "/api/v1/CorrespondingMapping/{0}";
        //GetAllowAnonymous All AccountCategory
        public static string GetAllCorrespondingMapping = "/api/v1/CorrespondingMapping";
        // POST  Create a AccountClass
        public static string CreateCorrespondingMapping = "/api/v1/CorrespondingMapping";
        //CorrespondingMapping

        public static string Get_Update_Delete_CorrespondingMappingException = "/api/v1/CorrespondingMappingException/{0}";
        public static string Get_CorrespondingMappingException = "/api/v1/CorrespondingMappingException/{0}";
        //GetAllowAnonymous All AccountCategory
        public static string GetAllCorrespondingMappingException = "/api/v1/CorrespondingMappingException";


        //ChartOfAccount
        //GetAllowAnonymous,Update,Delete ChartOfAccount By Id
        public static string Get_Update_Delete_ChartOfAccount = "/api/v1/ChartOfAccount/{0}";
        //GetAllowAnonymous All ChartOfAccount
        public static string GetAllChartOfAccount = "/api/v1/ChartOfAccounts"; ///
        public static string GetAllChartOfAccountManagementPositionByChart = "api/v1/ChartOfAccountManagementPosition/GetAllChartOfAccounts";
        public static string Get_ChartOfAccount_By_AccountNumber = "/api/v1/ChartOfAccount/GetChartOfAccountByAccountNumber/{0}";
        //GetAllowAnonymous All ChartOfAccount

        public static string GetOperationEventAttributes = "/api/v1/AccountType/GetOperationEventAttributes/{0}";
        public static string GetAllOperationEventAttributes = "/api/v1/OperationEventAttributes";
        ///api/v1/OperationEventAttributes
        // POST  Create a ChartOfAccount
        public static string CreateChartOfAccount = "/api/v1/ChartOfAccount";
        public static string GetAllJsTreeNode = "/api/v1/ChartOfAccounts/JsTreeNode";
        //GetAllowAnonymous,Update,Delete ChartOfAccount By Id
        public static string Delete_ChartOfAccount = "/api/v1/ChartOfAccount/DeleteChartOfAccountByAccountNumber/{0}";
        // POST  Create a ChartOfAccount
        public static string ManualEntriePosting = "/api/v1/AccountingEntry/ManualPostingEventCommand";

        //Account/api/v1/AccountingEntry/AddCashInfusionCommand
        //Credit a specifique Account record AddCashInfusionCommand   
        public static string CashInfusion = "/api/v1/AccountingEntry/AddCashInfusionCommand";
        public static string GetAccountByReference = "/api/v1/Account/GetAccountByReference/{0}";

        public static string GetAccountByAccountType = "/api/v1/Account/GetAccountByAccountType/{0}";
        public static string GetAccount = "/api/v1/Account/{0}";
        public static string Delete_Account = "/api/v1/Account/{0}";
        public static string GetAccountByAccountNumberUrl = "/api/v1/Account/GetAccountByAccountNumberRef/{0}";

        public static string GetAlAccounts = "/api/v1/Accounts";
        public static string GetAllLiaisonAccount = "/api/v1/Accounts/LiaisonAccount";
        public static string GetAllAccountByBranch = "/api/v1/Accounts/{0}";
  
        public static string GetAllBankAccountChartUsedToCreditCashFlow = "/api/v1/ChartOfAccountManagementPosition/{0}/{1}";

        public static string GetAllBranchAccountUsedToCreditCashFlow = "/api/v1/Account/GetAccountByMFIBankAccountNumberQuery/{0}";
        public static string CreateAccount = "/api/v1/Account";///api/v1/Account/ManualEntry
        public static string CreateAccountMET = "/api/v1/Account/ManualEntry";///api/v1/Account/ManualEntry
        public static string GetAccountUploadFiles = "/api/v1/Accounts/UploadFiles/{0}";
        public static string PutAccount = "/api/v1/Account/{0}";
        public static string GetSystemLiaisonAccountQueryUrl = "/api/v1/Account/GetSystemLiaisonAccountQuery/{0}";

        public static string GetAccountType = "/api/v1/AccountType/{0}";
        public static string GetAccountTypeAccountRubrique = "/api/v1/AccountType/AccountRubrique/{0}";
        public static string GetAllAccountType = "/api/v1/AccountTypes/AllSystemAccountTypes";
        public static string GetAllAccountTypeAccountRubrique = "/api/v1/AccountTypes/AccountRubrique";
        public static string Create_AccountTypeForSystem = "/api/v1/AccountType/AccountTypeForSystem";
        public static string Create_update_AccountTypeAccountRubrique = "/api/v1/AccountType/AccountRubrique";
        public static string Create_AccountType = "/api/v1/AccountType";
        public static string Update_AccountType = "/api/v1/AccountType/{0}";
        public static string Delete_AccountType = "/api/v1/AccountType/{0}";
        public static string Delete_AccountTypeAccountRubrique = "/api/v1/AccountType/AccountRubrique/{0}";
        public static string AccountingEntry_Generate4ColumnTrialBalance = "api/v1/AccountingEntries/Generate4ColumnTrialBalance";
        public static string AccountingEntry_Generate6ColumnTrialBalance = "api/v1/AccountingEntries/Generate6ColumnTrialBalance";
        public static string AccountingEntry_BalanceSheetColumn = "api/v1/AccountingEntries/GenerateCOBACBalanceSheet";
        public static string AccountingEntry_IncomeStatement = "api/v1/AccountingEntries/GenerateIncomeAndExpenseStatement";
        public static string AccountingEntry_AccountGl = "api/v1/AccountingEntry/GetAccountGLEntry";
        public static string AccountingEntry_BranchLiaisonEntries = "api/v1/AccountingEntries/BranchLiaisonEntries";
        public static string AccountingEntry_LiaisonEntries = "/api/v1/AccountingEntries/LiaisonEntries";
        public static string AccountingEntry_Get_Update_delete = "/api/v1/AccountingEntry/{0}";
        public static string AccountingEntry_Get_reference_Id = "/api/v1/AccountingEntry/GetAccountingEntryByReferenceIdQuery/{0}";
        public static string AccountingEntry_ReverseEntry = "/api/v1/AccountingEntry/ReverseAccountingEntry";
        public static string PostAutomatedEventEntryCommand_url = "/api/v1/AccountingEntries/PostAutomatedEventEntryCommand";
        //Post_AccountingEntry_Entries
        public static string AccountingEntry_Entries_branchId_accountId = "/api/v1/AccountingEntries/{0}/{1}";
        public static string AccountingEntry_Entries = "/api/v1/AccountingEntries";
        public static string AccountingEntry_Posting_Entries = "/api/v1/AccountingEntries/RetrieveEntries";
        public static string Trialbalance4Column_Entries = "/api/v1/AccountingEntries/Trialbalance4Column";
        public static string CashReplenishmentRequest = "/api/v1/BankingOperation/RequestForCashReplenishment";
        public static string DepositNotificationUrl = "/api/v1/BankingOperation/AddDepositNotificationCommand";
        public static string ApproveDepositNotificationUrl = "/api/v1/BankingOperation/AddDepositApprovalCommand";

 
        public static string BalanceSheet_EntriesUrl = "/api/v1/AccountingEntries/GenerateBalanceSheet";
        public static string BalanceSheet_EntriesUrlPDF = "/api/v1/AccountingEntries/GenerateBalanceSheetPDF";
        public static string GetCashBranchToBranchTransferUrl = "/api/v1/AccountingEntry/GetBranchToBranchTransferDto/{0}";
        public static string GeneralLedgerStatementUrl = "/api/v1/AccountingEntry/GetAccountStatementEntry";
        public static string GeneralLedgerStatementDetailUrl = "/api/v1/AccountingEntry/GetAccountGLEntry";
        public static string JournalEntryUrl = "/api/v1/AccountingEntry/JournalEntry";

        public static string UpdateCashReplenishmentRequest = "/api/v1/BankingOperation/UpdateRequestForCashReplenishment/{0}";
        public static string CashReplenishmentResponse = "/api/v1/BankingOperation/ResponseToCashReplenishment";
        public static string GetAllCashReplenishmentRequests = "/api/v1/BankingOperation/GetAllCashReplenishmentRequests";
        public static string GetAllCashReplenishmentQueryAsBranch = "/api/v1/BankingOperation/GetAllCashReplenishmentQueryAsBranch/{0}";
        public static string GetAllCashRequestApprovalQuery = "/api/v1/BankingOperation/GetAllCashRequestApprovalQuery";
        public static string GetAccountByEvenCodeUrl = "/api/v1/Account/GetAccountByEventCodeQuery";
        public static string BranchToBranchTransferUrl = "/api/v1/AccountingEntry/BranchToBranchTransferCommand";
        public static string CashClearingTransferCashReplenishmentUrl = "/api/v1/AccountingEntry/CashClearingTransferFromCashReplenishmentCommand";
        public static string CashClearingTransferBankDepositUrl = "/api/v1/AccountingEntry/CashClearingTransferFromBankDepositCommand";
        public static string GetCashReplenishmentRequestByBranchId = "/api/v1/BankingOperation/GetCashReplenishmentRequestByBranchId/{0}";
        public static string GetBankTransactionQueryByIdURL = "/api/v1/BankingOperation/GetBankTransactionQueryById/{0}";
        public static string GetBankTransactionQueryByReferenceIdURL = "/api/v1/BankingOperation/GetBankTransactionQueryByReferenceId/{0}";
        public static string GetCashReplenishmentRequestById = "/api/v1/BankingOperation/GetCashReplenishmentRequestById/{0}";
        public static string GetAllDepositNotificationRequestRequests = "/api/v1/BankingOperation/GetAllDepositNotificationQuery";
        ///BankingOperation/GetAllDepositNotificationRedirectionQuery   
        public static string GetDepositNotificationRequestRequests = "/api/v1/BankingOperation/GetDepositNotificationQuery/{0}";
        public static string GetAllDepositNotificationRequestQueryAsBranch = "/api/v1/BankingOperation/GetAllCashReplenishmentQueryAsBranch/{0}";
        public static string GetAllDepositNotificationRedirectionQuery = "/api/v1/BankingOperation/GetAllDepositNotificationRedirectionQuery";
        public static string UserNotificationRequestByIdUrl = "/api/v1/UsersNotification/{0}";
        public static string UserNotificationRequestUrl = "/api/v1/UsersNotifications";
        public static string GetDepositNotificationRequestById = "/api/v1/BankingOperation/GetDepositNotificationQuery/{0}";
        public static string GetCashReplenishmentRequestIdReference = "/api/v1/BankingOperation/GetCashReplenishmentRequestByRefereceId/{0}";
        public static string TransactionReversalRequest = "/api/v1/BankingOperation/TransactionReversalRequest";
        public static string TransactionReversalRequestApproval = "/api/v1/BankingOperation/TransactionReversalRequestApproval";
        public static string GetTransactionReversalRequest = "/api/v1/BankingOperation/GetTransactionReversalRequestById/{0}";
        public static string GetAllTransactionReversalRequest = "/api/v1/BankingOperation/GetAllTransactionReversalRequests";
        public static string GetTransactionReversalRequestByReferenceId = "/api/v1/BankingOperation/GetTransactionReversalRequestByReferenceId/{0}";
        public static string BankCashOutCommandUrl = "/api/v1/BankingOperation/BankCashOutCommand";
        public static string AttachedCashOutReceiptRemotely = "BankReceipt/CashOutOperation";
        public static string AttachedCashInReceiptRemotely = "BankReceipt/DepositOperation";
        public static string UpdateDepositNotificationCommandUrl = "/api/v1/BankingOperation/UpdateDepositNotificationCommand/{0}";
        ///CheckIfTransactionReversalRequestByReferenceIdExist/  
        public static string CheckIfTransactionReversalRequestByReferenceIdExist = "/api/v1/BankingOperation/CheckIfTransactionReversalRequestByReferenceIdExist/{0}";

        public static string Create_EntryTempData = "/api/v1/EntryTempData";
        public static string CleanAccountingEntryUrl = "/api/v1/AccountingEntry/CleanAccountAndAccountingEntriesCommand";
        public static string Url_Get_Update_delete_EntryTempData = "/api/v1/EntryTempData/{0}";
        public static string Url_Get_RefereceId = "api/v1/EntryTempDatas/{0}";
        public static string Url_Get_PostedRefereceId = "/api/v1/EntryTempDatas/PostedEntry/{0}";
        public static string Post_AccountingEntry_Entries = "/api/v1/EntryTempData/PostManualEntries";
        public static string GetReferenceSequenceUrl = "/api/v1/EntryTempDatas/GetSequence";
        public static string Url_Get_AllPostedEntries = "/api/v1/EntryTempDatas/PostedEntry";
        public static string Url_Get_AllPostedEntriesStatus = "/api/v1/EntryTempData/RetrieveEntriesByFilterOption";//?status={0}&fromDate={1}&toDate={2}&branchId={3}&issuedBy={4}&approvedBy={5}";

        public static string Post_ManaulEntryApproval_Entries = "/api/v1/EntryTempData/ManaulEntryApproval";
        //BUdgetmanagement
        public static string Get_Update_Delete_Budget = "/api/v1/Budget/{0}";
        public static string Create_Budget = "/api/v1/Budget";
        public static string Get_All_Budget = "/api/v1/Budget";
        public static string Get_All_BudgetPeriod = "/api/v1/BudgetPeriods";
        public static string Get_All_OrganizationalUnit = "api/v1/OrganizationalUnit";
        public static string LockBudget_Budget = "/api/v1/Budget/LockBudget/{0}";
        public static string ApprovedBudget_Budget = "/api/v1/Budget/ApprovedBudget/{0}";

        //BUdgetmanagement
        public static string Get_Update_Delete_BudgetCategory = "/api/v1/BudgetCategory/{0}";
        public static string Get_All_BudgetCategory = "/api/v1/BudgetCategory";
        public static string Create_BudgetCategory = "/api/v1/BudgetCategory";
        public static string LockBudget_BudgetCategory = "/api/v1/BudgetCategory/LockBudget/{0}";
        public static string ActivateBudget_BudgetCategory = "/api/v1/BudgetCategory/ApprovedBudget/{0}";

        public static string Get_Update_Delete_TrailBalanceUploud = "/api/v1/TrailBalanceUploud/{0}";

        public static string GetAllTrailBalanceUploud = "/api/v1/TrailBalanceUplouds";

        //ThirdPartyBranche
        public static string Get_Update_Delete_CorrespondingBankBranche = "/api/v1/CorrespondingBankBranch/{0}";
        public static string GetAllCorrespondingBankBranche = "/api/v1/ThirdPartyBranches";
        public static string CreateCorrespondingBankBranche = "/api/v1/ThirdPartyBranche";
        //ThirdPartyBranche
        //public static string Get_Update_Delete_CorrespondingBank = "/api/v1/ThirdPartyBranche/{0}";
        //public static string GetAllThirdPartyInstitution = "/api/v1/ThirdPartyBranches";
        //public static string CreateThirdPartyInstitution = "/api/v1/ThirdPartyInstitution";
        ////CorrespondingBankBranche
        //public static string Get_Update_Delete_CorrespondingBankBranche = "/api/v1/CorrespondingBankBranche/{0}";
        //public static string GetAllCorrespondingBankBranche = "/api/v1/CorrespondingBankBranches";
        //public static string CreateCorrespondingBankBranche = "/api/v1/CorrespondingBankBranche";/api/v1//{id}
        ////CorrespondingBankBranche
        public static string Get_Update_Delete_CorrespondingBank = "/api/v1/ThirdPartyInstitution/{0}";
        public static string GetAllCorrespondingBank = "/api/v1/ThirdPartyInstitutions";
        public static string CreateCorrespondingBank = "/api/v1/ThirdPartyInstitution";
        //BankingZone
        public static string Get_Update_Delete_BankingZone = "/api/v1/BankingZone/{0}";
        public static string GetAllBankingZone = "/api/v1/BankingZones";
        public static string CreateBankingZone = "/api/v1/BankingZone";

        //BankZoneBranch ///
        public static string Get_Update_Delete_BankZoneBranch = "/api/v1/BankZoneBranch/{0}";
        public static string Get_BankZoneBranchbyBranchId = "/api/v1/BankZoneBranch/BankZoneBranchbyBranchId/{0}/{1}";
        public static string Get_BankZoneBranch_by_ZoneID = "/api/v1/BankZoneBranch/{0}";
        public static string GetAllBankZoneBranch = "/api/v1/BankZoneBranchs";
        public static string CreateBankZoneBranch = "/api/v1/BankZoneBranch";



        //AuditTrail//
        public static string Get_AuditTrail = "/api/v1/AuditTrail/{0}";
        public static string Get_AuditTrailByUserName = "/api/v1/AuditTrail/{0}";
        public static string Delete_AuditTrail = "/api/v1/AuditTrail/{0}";
        public static string Get_GetAuditTrailByPagging = "/api/v1/AuditTrail/pagging";
        public static string Get_GetAuditTrailBy_DataTable_Pagging = "/api/v1/AuditTrail/datatable-pagging";

        //MemberNoneCashOperation//
        public static string GetAll_MemberNoneCashOperations = "/api/v1/MemberNoneCashOperations";
        public static string Validate_MemberNoneCashOperation = "/api/v1/MemberNoneCashOperation/Validate/{0}";
        public static string Delete_MemberNoneCashOperation = "/api/v1/MemberNoneCashOperation/{0}";
        public static string Get_MemberNoneCashOperation = "/api/v1/MemberNoneCashOperation/{0}";
        public static string Create_MemberNoneCashOperation = "/api/v1/MemberNoneCashOperation";

        //Bulk Operation
        public static string SimulateAccountTopup= "/api/v1/BulkOperations/Topup";
        public static string SimulateContribution = "/api/v1/BulkOperations/Contribution";
        public static string GetAllBulkOperations = "/api/v1/BulkOperations";
    }
}
