
using System.Threading.Tasks;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.User;

namespace CBS.FrontDesk.Helper
{
    public abstract class APICallHelper
    {

        //Authentication
        public static string Authentication = "/api/Authentication/Session/Login";
        public static string RefreshToken = " /api/Authentication/Session/RefreshToken";
           
        //User
        public static string GetUserByID = "/api/User/{0}";
        public static string DeleteUser = "/api/User/{0}";
        public static string GetUsers = "/api/User/GetAllUsers";
        public static string createUserUrl = "/api/User";
        public static string UpdateUserProfile = "/api/User/profile";
        public static string ChangePassword = "/api/User/changepassword";
        public static string ResetPassword = "/api/User/resetpassword";
        public static string UploadProfilePhoto = "/api/User/UpdateUserProfilePhoto";
        public static string GetRecentRegisteredUsers = "/api/User/GetRecentlyRegisteredUsers";
        public static string UpdateUser = "/api/User/{0}";

        //Role
        public static string Get_Update_Delete_Role = "/api/Role/{0}";
        public static string GetAllRoles = "/api/Role";
        public static string CreateRole = "/api/Role";

        //UserPermission
        public static string Get_Update_Delete_UserPermission = "/api/UserPermission/{0}";
        public static string GetAllUserPermission = "/api/UserPermissions";
        public static string CreateUserPermission = "/api/UserPermission";
        public static string GetUserPermissions = "/api/UserPermission/User/{0}";

        //RolePermission
        public static string Get_Update_Delete_RolePermission = "/api/RolePermission/{0}";
        public static string GetAllRolePermission = "/api/RolePermissions";
        public static string CreateRolePermission = "/api/RolePermission";
        public static string GetRolePermissions = "/api/RolePermission/Role/{0}";


        //MenuMaster
        public static string Get_Update_Delete_MenuMaster = "/api/MenuMaster/{0}";
        public static string GetAllMenuMaster = "/api/MenuMasters";
        public static string GetAllMenuMasterToAssignPermission = "/api/MenuMasters/AssignPemission";
        public static string CreateMenuMaster = "/api/MenuMaster";
        public static string GetMenuMasterByParentID = "/api/MenuMaster/Parent/{0}";


        //Customer
        public static string CreateIndividualProfile = "/api/v1/Customer";
        public static string GetAllIndividualProfile = "/api/v1/Customers";
        public static string SubcriptionAggregates = "/api/v1/SubcriptionAggregates";
        public static string DeleteCustomer = "/api/v1/Customer/{0}";
        public static string GetCustomerByID = "/api/v1/Customer/{0}";
        public static string UploadCustomerDocument = "/api/v1/uploadCustomerDocuments";
        public static string GetCustomerDefaultEnums = "/api/v1/SubscriptionAggregates/GetAll";

        public static string ResetPin = "/api/v1/Customer/Pin/Reset";

        public static string Get_Update_Delete_Question = "/api/v1/Questions/{0}";
        public static string CreateQuestion = "/api/v1/CreateQuestion";
        public static string GetAllQuestions = "/api/v1/Questions";

        //Saving
        public static string GetSavingProducts = "/api/v1/SavingProduct";
        public static string GetCustomerBalance = "/api/v1/Account/Balance/Customer/{0}";
        public static string UpdateIndividualProfile = "/api/v1/Customer/{0}";
        public static string GetCustomerAccounts = "/api/v1/Account/Customer/{0}";
        public static string InitialDeposit = "/api/v1/Transaction/Initiate/{0}";
        public static string GetAllAccounts = "/api/v1/Account";
        public static string MakeDepoit = "/api/v1/Transaction/Deposit";
        public static string MakeWithdrawal = "/api/v1/Transaction/Withdrawal";
        public static string MakeTransfer = "/api/v1/Transaction/Transfer";
        public static string MakeTrGetAccountByAccountNumber = "/api/v1/Account/AccountNumber/{0}";
        public static string GetTransactionHistoryByAccountNumber = "/api/v1/Transaction/AccountNumber/{0}";
        public static string GetTransactionHistoryByCustomerNumber = "/api/v1/Transaction/Customer/{0}";
        public static string GetAllTransactions = "/api/v1/Transaction";

        public static string PrimaryTellerProvisioning = "/api/v1/Teller/Provision/PrimaryTeller";

        //

        //Account
        public static string AddCustomerSavingAccount = "/api/v1/Account";
        

        //Teller
        public static string Get_Update_Delete_Teller = "/api/v1/Teller/{0}";
        public static string GetAllTeller = "/api/v1/Teller";
        public static string CreateTeller = "/api/v1/Teller";

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
        public static string GetAllTransferLimits = "/api/v1/v1/TransferLimits";
        public static string CreateTransferLimits = "/api/TransferLimits";
        //WithdrawalLimitss
        public static string Get_Update_Delete_WithdrawalLimits = "/api/v1/WithdrawalLimits/{0}";
        public static string GetAllWithdrawalLimits = "/api/v1/WithdrawalLimits";
        public static string CreateWithdrawalLimits = "/api/v1/WithdrawalLimits";

        //Configuration
        public static string GetAllConfigurationEnums = "/api/v1/Config/EnumData";

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
        public static string Get_Update_Delete_Period = "/api/v1/Period/{0}";
        public static string GetAllPeriod = "/api/v1/Periods";
        public static string CreatePeriod = "/api/v1/Period";
        public static string AttachedDocuments = "/api/v1/AttachedLoanDocument";
        public static string Get_Update_Delete_InstallmentType = "/api/v1/InstallmentType/{0}";
        public static string GetAllInstallmentType = "/api/v1/InstallmentTypes";
        public static string CreateInstallmentType = "/api/v1/InstallmentType";
        public static string Get_Delete_DocumentAttachedToLoan = "/api/v1/AttachedLoanDocument/{0}";
        public static string GetAllAttachedments = "/api/v1/AttachedLoanDocuments";
        public static string Get_Update_Delete_LoanProduct = "/api/v1/LoanProduct/{0}";
        public static string GetAllLoanProduct = "/api/v1/LoanProducts";
        public static string CreateLoanProduct = "/api/v1/LoanProduct";
        public static string GetLoanProductConfigurationAgregates = "/api/v1/LoanProductConfigurationAgregates";

        //Upload FIles
        public static string UploadFile = "/api/FIleManagement/Upload";

        //Loan Application
        public static string Get_Update_Delete_LoanApplication = "/api/v1/LoanApplication/{0}";
        public static string GetAllLoanApplication = "/api/v1/LoanApplications";
        public static string CreateLoanApplication = "/api/v1/LoanApplication";
        public static string GetLoanApplicationInstallment = "/api/v1/LoanApplication/Installment/{0}";
        public static string LoanSimulation = "/api/v1/LoanApplication/Simulation";
        public static string ApproveLoanApplication = "/api/v1/LoanApplication/ChangeStatus/{0}";


        //Tax  
        public static string Get_Update_Delete_Tax = "/api/v1/Tax/{0}";
        public static string GetAllTax = "/api/v1/Taxs";
        public static string CreateTax = "/api/v1/Tax";
        //LoanOtherFee
        public static string Get_Update_Delete_LoanOtherFee = "/api/v1/LoanOtherFee/{0}";
        public static string GetAllLoanOtherFee = "/api/v1/LoanOtherFees";
        public static string CreateLoanOtherFee = "/api/v1/LoanOtherFee";
        //Collateral
        public static string Get_Update_Delete_Collateral = "/api/v1/Collateral/{0}";
        public static string GetAllCollateral = "/api/v1/Collaterals";
        public static string CreateCollateral = "/api/v1/Collateral";
        //LoanCollateral
        public static string Get_Update_Delete_LoanCollateral = "/api/v1/LoanCollateral/{0}";
        public static string GetAllLoanCollateral = "/api/v1/LoanCollaterals";
        public static string CreateLoanCollateral = "/api/v1/LoanCollateral";
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
        //CustomerLoanAccount
        public static string Get_Update_Delete_CustomerLoanAccount = "/api/v1/CustomerLoanAccount/{0}";
        public static string GetAllCustomerLoanAccount = "/api/v1/CustomerLoanAccounts";
        public static string CreateCustomerLoanAccount = "/api/v1/CustomerLoanAccount";
        //AlertProfile
        public static string Get_Update_Delete_AlertProfile = "/api/v1/AlertProfile/{0}";
        public static string GetAllAlertProfile = "/api/v1/AlertProfiles";
        public static string CreateAlertProfile = "/api/v1/AlertProfile";



        //Accounting Roles
        public static string GetAllAccountingRules = "/api/v1/AccountingRules";
        //OperationEvent
        //Get,Update,Delete OperationEvent By Id
        public static string Get_Update_Delete_OperationEvent = "/api/v1/OperationEvent/{0}";
        //Get All OperationEvent
        public static string GetAllOperationEvent = "/api/v1/OperationEvent";
        // POST  Create a OperationEvent
        public static string CreateOperationEvent = "/api/v1/OperationEvent";


        //OperationEventAttribute
        //Get,Update,Delete OperationEventAttribute By Id
        public static string Get_Update_Delete_OperationEventAttribute = "/api/v1/OperationEventAttribute/{0}";
        //Get All OperationEventAttribute
        public static string GetAllOperationEventAttribute = "/api/v1/OperationEventAttributes";
        // POST  Create a OperationEventAttribute
        public static string CreateOperationEventAttribute = "/api/v1/OperationEventAttribute";

        //AccountingRule
        //Get,Update,Delete AccountingRule By Id
        public static string Get_Update_Delete_AccountingRule = "/api/v1/AccountingRule/{0}";
        //Get All AccountingRule
        public static string GetAllAccountingRule = "/api/v1/AccountingRules";
        // POST  Create a AccountingRule
        public static string CreateAccountingRule = "/api/v1/AccountingRule";

        //AccountingRuleEntry
        //Get,Update,Delete AccountingRuleEntry By Id
        public static string Get_Update_Delete_AccountingRuleEntry = "/api/v1/AccountingRuleEntry/{0}";
        //Get All OperationEventAttribute
        public static string GetAllAccountingRuleEntry = "/api/v1/AccountingRuleEntry";
        // POST  Create a OperationEventAttribute
        public static string CreateAccountingRuleEntry = "/api/v1/AccountingRuleEntry";

        //AccountClass
        //Get,Update,Delete AccountClass By Id
        public static string Get_Update_Delete_AccountClass = "/api/v1/AccountClass/{0}";
        //Get All AccountClass
        public static string GetAllAccountClass = "/api/v1/AccountClasss";
        // POST  Create a AccountClass
        public static string CreateAccountClass = "/api/v1/AccountClass";

        //AccountCategory
        //Get,Update,Delete AccountCategory By Id
        public static string Get_Update_Delete_AccountCategory = "/api/v1/AccountCartegory/{0}";
        //Get All AccountCategory
        public static string GetAllAccountCategory = "/api/v1/AccountCartegories";
        // POST  Create a AccountClass
        public static string CreateAccountCategory = "/api/v1/AccountCartegory";

        //ChartOfAccount
        //Get,Update,Delete ChartOfAccount By Id
        public static string Get_Update_Delete_ChartOfAccount = "/api/v1/ChartOfAccount/{0}";
        public static string Get_ChartOfAccount_By_AccountNumber = "/api/v1/ChartOfAccount/GetChartOfAccountByAccountNumber/{0}";
        //Get All ChartOfAccount
        public static string GetAllChartOfAccount = "/api/v1/ChartOfAccounts";
        // POST  Create a ChartOfAccount
        public static string CreateChartOfAccount = "/api/v1/ChartOfAccount";
        public static string GetAllJsTreeNode = "/api/v1/ChartOfAccounts/JsTreeNode";
        //Get,Update,Delete ChartOfAccount By Id
        public static string Delete_ChartOfAccount = "/api/v1/ChartOfAccount/DeleteChartOfAccountByAccountNumber/{0}";


    }
}
