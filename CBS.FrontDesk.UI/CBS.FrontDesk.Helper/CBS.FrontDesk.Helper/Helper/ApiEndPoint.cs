using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Helper
{
    public class ApiEndPoint
    {
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
        public static string  Delete_ChartOfAccount = "/api/v1/ChartOfAccount/DeleteChartOfAccountByAccountNumber/{0}";


        //Branch 
        //Get,Update,Delete Branch By Id
        public static string Get_Update_Delete_Branch = "/api/v1/Branch/{0}";
        //Get All Branch
        public static string GetAllBranch = "/api/v1/Branchs";
        // POST  Create a Branch
        public static string CreateBranch = "/api/v1/Branch";
    }
}
