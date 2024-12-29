using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Message
{
    public enum SystemMessageStatus
    {
        Success = 1,
        Failed = 2,
        Exist = 3,
        NotFound = 4,
        Error = 5,
    }
    public enum ExecutionProcessOption
    {
        AlreadyExist = 1,
        PasswordDoNotMatch = 2,
        OldPasswordIncorrect = 3,
        Invalidquestion = 4,
        InvalidAnswer = 5,
        
        TryCatch = 6,
        InsertObject = 7,
        UpdateUpject = 8,
        DeleteObject = 9,
        DoesNotExist = 10,
        LoginSuccessful = 11,
        InvalidImage = 12,
        InvalidImageFormat = 13,
        InvalidDocumentFormat = 14,
        InsufficientFund = 15,
        OVADoesNotExistForSelectedBank = 16,
        AlreadyExecutedSomeTransactions = 17,
        ScriptExecution = 18,
        YouHaveNotSubscriptForThisService = 19,
        InvalidCSVFile = 20,
        FileIsEmpty = 21,

        AlreadyApproved = 22,
        NoFileWasSelected = 23,
        AwaitsExecution = 24,
        ConfirmationDone = 25,
        SearchNotFound = 26,
        SearchCompleted = 27,
        AdjustmentExecution = 28,
        AdjustmentExecutionAlreadyDone = 29,
        EmailNotVerified = 30,
        InvalidUserNameOrPassword = 31,
        AccountDisabled = 32,
        AccountIsBlocked = 33,
        AccountBlockedAccountUsedByAnotherPerson = 34,
        InvalidUserNameOrPasswordContactAdmin = 35,
        LoginSuccessfull = 36,
        ChangePasswordFirstLogin = 37,
        YoueAccountIsBlockedWithThreeAttempt = 38,
        SMSSendSuccess = 39,
        ActivationSuccessFull = 40,
        SearchFound = 41,
        UserNameIsRequired = 42,
        UserPasswordIsRequired = 43,
        ClientHasAPending = 44,
        PasswordLegnt = 45,
        MustHaveDigit = 46,
        MustHaveSpecialCharacter = 47,
        MustHaveUpperCase = 48,
        MustHaveLowerCase = 49,
        HasSpecialChar = 50,
        MustNotBeLoginName = 51,
        ActivationPointRequired = 52,
        MakeDepositFailed = 53,
        SetForExecution = 54,
        AdjustmentVerificationCompleted = 55,
        AdjustmentConfirmationCompleted = 56,
        AdjustmentInitialization = 57,
        SMSCodeAlreadyVerified = 58,
        SMSCodeAlreadyExpired = 59,
        AdjustmentAreadyInProgress = 60,
        MenuNotAssigned = 61,
        MFAVerified = 62,
        MFAFailed = 63,
        Extraction = 64,
        CanNotRemoveFrom = 65,
        CanNotRemoveDefault = 66,
        WrongInterestDistribution = 67,
        InvestmentActivation = 68,
        BankDailyInvestment = 69,
        BankDailyLimitActivationSuccessFull = 70,
        BankCurrentBalanceNotSufficient = 71,
        BankCurrentBalanceFinish = 72,
        AlreadyExtracted = 73,
        ValueMustBeGreaterThanZero = 74,
        NoScoringWasFound = 75,
        ExceptionOccured = 76,
        MultipleAdjustmentExecution = 77,
        VerificationCompleted = 78,
        InitializationCompleted = 79,
        EmptyGrid = 80,
        CanNotDelete = 81,
        CanNotDeleteRequestInProgress = 82,
        FailedLoanResolutionCompleted = 83,
        CanNotDeleteCustmerHaveaDebt = 84,
        CanNot_Pull_transaction_For_More_Than_3_Months = 85,
        DefaultFailedMessages = 87,
        DefaultSuccessdMessages = 89,
        InvalidOption = 90,
        UnknownError = 91,
    }
    public enum MessagesResults
    {
        Success = 1,
        Failed = 2,
        Error = 3,
        Exist = 4,
        NoteFound = 5,
        SearchFound = 6,
        Required = 7
    }

    public interface IExecutionMessages
    {
        string ObjectName { get; set; }
        string SMSTest { get; set; }
        int CorpocashActivationState { get; set; }
        string ExecutionfailedWithName { get; set; }
        string ExecutionfailedWithError { get; set; }
        string Executionfailed { get; set; }
        string MessageString { get; set; }
        string SessionID { get; set; }
        string MessageStatus { get; set; }
        int attempt { get; set; }
        string Telephone { get; set; }
        int left { get; set; }
        int ObjectID { get; set; }
        bool Result { get; set; }
        Exception ex { get; set; }
        ExecutionProcessOption ProcessOption { get; set; }
        MessagesResults MessagesResults { get; set; }
        string ObjectName1 { get; set; }
    }

    public class ExecutionMessages : IExecutionMessages
    {
        public string ObjectName { get; set; }
        public string SMSTest { get; set; }
        public int CorpocashActivationState { get; set; }
        public string ExecutionfailedWithName { get; set; }
        public string ExecutionfailedWithError { get; set; }

        public string Executionfailed { get; set; }
        public string MessageString { get; set; }
        public string SessionID { get; set; }
        public string MessageStatus { get; set; }
        public int attempt { get; set; }
        public string Telephone { get; set; }
        public int left { get; set; }
        public int ObjectID { get; set; }
        public object Data { get; set; }
        public bool Result { get; set; }
        public Exception ex { get; set; }
        public ExecutionProcessOption ProcessOption { get; set; }
        public MessagesResults MessagesResults { get; set; }
        public string ObjectName1 { get; set; }

        public ExecutionMessages()
        {
            ExecutionfailedWithName = $"Execution failed {ObjectName}";
            Executionfailed = $"Execution failed;";
            ExecutionfailedWithError = $"Execution failed, Error:";
        }
        
    }

    public class Messaging
    {

        public static string MessageResult(IExecutionMessages Result)
        {

            switch (Result.MessagesResults)
            {
                case MessagesResults.Exist:
                    {
                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.AlreadyExist:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} already exist.";
                                break;

                        }
                    }

                    break;
                case MessagesResults.Required:
                    {
                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.ActivationPointRequired:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName}, activation point is required, Please select an activation point.";
                                break;
                            case ExecutionProcessOption.InvestmentActivation:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName}, your approval is required, Please click on check box.";
                                break;
                        }
                    }

                    break;

                case MessagesResults.Failed:
                    {
                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.InvalidAnswer:
                                Result.MessageString = $"{Result.Executionfailed} Invalide answer.";
                                break;
                            case ExecutionProcessOption.DefaultFailedMessages:
                                Result.MessageString = $"{Result.Executionfailed} { Result.MessageString}";
                                break;
                            case ExecutionProcessOption.CanNotDelete:
                                Result.MessageString = $"Can not delete {Result.Executionfailed}, it is used by another processes";
                                break;
                            case ExecutionProcessOption.CanNotDeleteCustmerHaveaDebt:
                                Result.MessageString = $"Can not re-initialize loan status, {Result.ObjectName}, process terminated.";
                                break;
                            case ExecutionProcessOption.CanNotDeleteRequestInProgress:
                                Result.MessageString = $"Can not delete {Result.Executionfailed}, request is in progress of execution.";
                                break;
                            case ExecutionProcessOption.EmptyGrid:
                                Result.MessageString = $"No transaction was selectd, data grid is empty. Transaction teminated.";
                                break;
                            case ExecutionProcessOption.NoScoringWasFound:
                                Result.MessageString = $"{Result.Executionfailed} Scoring must be uploaded first.";
                                break;
                            case ExecutionProcessOption.CanNotRemoveDefault:
                                Result.MessageString = $"{Result.Executionfailed} Canot unsubscribe client's default service; {Result.ObjectName}";
                                break;
                            case ExecutionProcessOption.CanNotRemoveFrom:
                                Result.MessageString = $"{Result.Executionfailed} Canot unsubscribe {Result.ObjectName} from client services";
                                break;
                            case ExecutionProcessOption.AlreadyExtracted:
                                Result.MessageString = $"{Result.Executionfailed}, {Result.ObjectName} is already extracted.";
                                break;
                            case ExecutionProcessOption.MakeDepositFailed:
                                Result.MessageString = $"{Result.Executionfailed} Make deposit can't be done on {Result.ObjectName}";
                                break;
                            case ExecutionProcessOption.Invalidquestion:
                                Result.MessageString = $"{Result.Executionfailed} Invalide question.";
                                break;
                            case ExecutionProcessOption.MFAFailed:
                                Result.MessageString = $"{Result.Executionfailed} Invalide code.";
                                break;
                            case ExecutionProcessOption.OldPasswordIncorrect:
                                Result.MessageString = $"{Result.Executionfailed} Old password is incorrect.";
                                break;
                            case ExecutionProcessOption.PasswordDoNotMatch:
                                Result.MessageString = $"{Result.Executionfailed} Password do not matched.";
                                break;
                            case ExecutionProcessOption.InvalidImageFormat:
                                Result.MessageString = $"{Result.Executionfailed} Invalid image format.";
                                break;
                            case ExecutionProcessOption.InvalidDocumentFormat:
                                Result.MessageString = $"{Result.Executionfailed} Invalid document format.";
                                break;
                            case ExecutionProcessOption.MenuNotAssigned:
                                Result.MessageString = $"{Result.Executionfailed} Menu not configured for usage.";
                                break;

                            case ExecutionProcessOption.InsufficientFund:
                                Result.MessageString = $"{Result.Executionfailed} Inssufficient fund; {Result.ObjectName}";
                                break;
                            case ExecutionProcessOption.OVADoesNotExistForSelectedBank:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} does not have selected OVA account.";
                                break;
                            case ExecutionProcessOption.AlreadyExecutedSomeTransactions:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} already did some loan transaction(s).";
                                break;
                            case ExecutionProcessOption.YouHaveNotSubscriptForThisService:
                                Result.MessageString = $"{Result.Executionfailed} You have not subscribed for the {Result.ObjectName} service. Contact your service provider to confirm your subscription to this service.";
                                break;
                            case ExecutionProcessOption.InvalidCSVFile:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} contains invalid content.";
                                break;
                            case ExecutionProcessOption.FileIsEmpty:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} content is empty.";
                                break;
                            case ExecutionProcessOption.AlreadyApproved:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} is already approved. No action can be performed agin on this file.";
                                break;
                            case ExecutionProcessOption.NoFileWasSelected:
                                Result.MessageString = $"{Result.Executionfailed} No file was selected.";
                                break;
                            case ExecutionProcessOption.AwaitsExecution:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} stil awaits execution. Contact immediately data file executer.";
                                break;
                            case ExecutionProcessOption.AdjustmentExecutionAlreadyDone:
                                Result.MessageString = $"{Result.Executionfailed} Adjustment is already done on {Result.ObjectName} mobile money account.";
                                break;
                            case ExecutionProcessOption.EmailNotVerified:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName}: Your momokash account is not verified. Check your email to verifiy your account.";
                                break;
                            case ExecutionProcessOption.InvalidUserNameOrPassword:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName}: You have entered invalid username or password, Attempt(s) {Result.attempt}, left {Result.left}";
                                break;
                            case ExecutionProcessOption.AccountDisabled:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName}: Your momokash account has been disabled, please contact your administrator.";
                                break;
                            case ExecutionProcessOption.AccountIsBlocked:
                                Result.MessageString = $"{Result.Executionfailed}  {Result.ObjectName} You momokash account has been blocked. Too many attempts, Contact your administrator of access";
                                break;
                            case ExecutionProcessOption.AccountBlockedAccountUsedByAnotherPerson:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} Your momkash account is open or used by process. Contact your administrator to kill the session.";
                                break;
                            case ExecutionProcessOption.InvalidUserNameOrPasswordContactAdmin:
                                Result.MessageString = $"{Result.Executionfailed} Invalid username or answer, contact your administrator.";

                                break;
                            case ExecutionProcessOption.YoueAccountIsBlockedWithThreeAttempt:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName}: Your account have been blocked on, Attempt(s) {Result.attempt} loggin, left {Result.left}";

                                break;
                            case ExecutionProcessOption.UserNameIsRequired:
                                Result.MessageString = $"Username is required.";

                                break;
                            case ExecutionProcessOption.SMSCodeAlreadyVerified:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} already verified.";

                                break;
                            case ExecutionProcessOption.SMSCodeAlreadyExpired:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} already expired.";

                                break;
                            case ExecutionProcessOption.AdjustmentAreadyInProgress:
                                Result.MessageString = $"{Result.Executionfailed} {Result.ObjectName} already inprogress...";

                                break;
                            case ExecutionProcessOption.UserPasswordIsRequired:
                                Result.MessageString = $"User password is required.";


                                break;
                            case ExecutionProcessOption.PasswordLegnt:
                                Result.MessageString = $"Minimum Password Length Required is: 10 characters.";
                                break;
                            case ExecutionProcessOption.MustHaveSpecialCharacter:
                                Result.MessageString = $"Your Password must have at least one special character.";
                                break;
                            case ExecutionProcessOption.MustHaveDigit:
                                Result.MessageString = $"Your Password must contain digit.";
                                break;
                            case ExecutionProcessOption.MustHaveUpperCase:
                                Result.MessageString = $"Your Password must contain upper case.";
                                break;
                            case ExecutionProcessOption.MustHaveLowerCase:
                                Result.MessageString = $"Your Password must contain lower case.";
                                break;
                            case ExecutionProcessOption.MustNotBeLoginName:
                                Result.MessageString = $"Your Password must not be your login Name";
                                break;

                            case ExecutionProcessOption.ClientHasAPending:
                                Result.MessageString = $"Disactivation process failed, {Result.ObjectName} has an unpaid loan of {Result.SMSTest}. Transaction terminated.";

                                break;
                            case ExecutionProcessOption.WrongInterestDistribution:
                                Result.MessageString = $"Interest distribution setting failed, {Result.ObjectName} Transaction terminated.";

                                break;
                            case ExecutionProcessOption.DoesNotExist:
                                Result.MessageString = $"{Result.ObjectName} does not exit in client list of services. Client should officially subscrib to {Result.ObjectName}.  Process terminated.";

                                break;
                            case ExecutionProcessOption.BankDailyInvestment:
                                Result.MessageString = $"{Result.ObjectName} the amount should be less than the dailylimit. Contact your admin for support.";

                                break;
                            case ExecutionProcessOption.BankCurrentBalanceFinish:
                                Result.MessageString = $"{Result.ObjectName} Your system investment is finished. Process terminated.";

                                break;
                            case ExecutionProcessOption.BankCurrentBalanceNotSufficient:
                                Result.MessageString = $"{Result.ObjectName} Your system state does not permit you to do this investment.{Result.ObjectName}. amo Process terminated.";

                                break;
                            default:
                                Result.MessageString= Result.MessageString;
                                break;
                        }

                        
                    }
                    //            EmailNotVerified = 30,
                    //InvalidUserNameOrPassword = 31,
                    //AccountDisabled = 32,
                    //AccountIsBlocked = 33,
                    //AccountBlockedAccountUsedByAnotherPerson = 34,
                    //InvalidUserNameOrPasswordContactAdmin = 35,

                    break;
                case MessagesResults.Success:
                    {

                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.DeleteObject:
                                Result.MessageString = $"{Result.ObjectName} deleted successfuly.";
                                break;
                            case ExecutionProcessOption.DefaultSuccessdMessages:
                                Result.MessageString = $"{Result.ObjectName} {Result.MessageString}";
                                break;
                            case ExecutionProcessOption.InitializationCompleted:
                                Result.MessageString = $"{Result.ObjectName}, Initialization process completed.";
                                break;
                            case ExecutionProcessOption.VerificationCompleted:
                                Result.MessageString = $"Verification completed.";
                                break;
                            case ExecutionProcessOption.Extraction:
                                Result.MessageString = $"{Result.ObjectName} extraction successfull.";
                                break;

                            case ExecutionProcessOption.UpdateUpject:
                                Result.MessageString = $"{Result.ObjectName} updated successfuly.";
                                break;
                            case ExecutionProcessOption.MFAVerified:
                                Result.MessageString = $"{Result.ObjectName} verification successfuly.";
                                break;
                            case ExecutionProcessOption.InsertObject:
                                Result.MessageString = $"{Result.ObjectName} created successfuly.";
                                break;
                            case ExecutionProcessOption.ScriptExecution:
                                Result.MessageString = $"{Result.ObjectName} was executed successfuly.";
                                break;
                            case ExecutionProcessOption.ConfirmationDone:
                                Result.MessageString = $"{Result.ObjectName} confirmation was done successffuly";
                                break;
                            case ExecutionProcessOption.SearchCompleted:
                                Result.MessageString = $"{Result.ObjectName} searching...";
                                break;
                            case ExecutionProcessOption.AdjustmentExecution:
                                Result.MessageString = $"Adjustment completed.";
                                break;
                            case ExecutionProcessOption.AdjustmentConfirmationCompleted:
                                Result.MessageString = $"Validation and confirmation completed.";
                                break;
                            case ExecutionProcessOption.FailedLoanResolutionCompleted:
                                Result.MessageString = $"{Result.ObjectName} failed loans were completly resolved";
                                break;
                            case ExecutionProcessOption.AdjustmentVerificationCompleted:
                                Result.MessageString = $"Verification completed.";
                                break;
                            case ExecutionProcessOption.AdjustmentInitialization:
                                Result.MessageString = $"Adjustment initialization submited.";
                                break;

                            case ExecutionProcessOption.ChangePasswordFirstLogin:
                                Result.MessageString = $"Change of password.";
                                break;
                            case ExecutionProcessOption.SMSSendSuccess:
                                Result.MessageString = $"{Result.ObjectName} SMS(s) successfuly send.";
                                break;
                            case ExecutionProcessOption.LoginSuccessful:
                                Result.MessageString = $"{Result.ObjectName}, Access granted.";
                                break;
                            case ExecutionProcessOption.ActivationSuccessFull:
                                Result.MessageString = $"{Result.ObjectName}, Activation suscessfull.";
                                break;
                            case ExecutionProcessOption.BankDailyLimitActivationSuccessFull:
                                Result.MessageString = $"{Result.ObjectName}, Activation suscessfull.";
                                break;
                        }

                    }
                    break;
                case MessagesResults.SearchFound:
                    {

                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.SearchFound:
                                Result.MessageString = $"{Result.ObjectName}, data found for activation. Loading.....";
                                break;
                        }

                    }
                    break;
                case MessagesResults.NoteFound:
                    {

                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.SearchNotFound:
                                Result.MessageString = $"Not found 404";
                                break;
                        }

                    }
                    break;
                case MessagesResults.Error:
                    {
                        switch (Result.ProcessOption)
                        {
                            case ExecutionProcessOption.TryCatch:

                                if (Result.MessageStatus == "")
                                {
                                    Result.MessageString = $"{Result.MessageString}";
                                }
                                else
                                {
                                    Result.MessageString = $"{Result.ExecutionfailedWithError} {Result.MessageString}";

                                }
                                break;
                            case ExecutionProcessOption.ExceptionOccured:
                                Result.MessageString = $"Exception occured, Error: {Result.ExecutionfailedWithError} {Result.MessageString}";
                                break;
                        }
                    }


                    break;

                default:
                    Result.MessageString = $"Error: General error occured.";
                    break;

            }
            return Result.MessageString;
        }


    }

}
