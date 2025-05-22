using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Data.UserManagement;
using CBS.BusinessService.UserManagement;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.ReportDataSetDto;
using System.Web;

namespace CBS.BusinessService.Accounts
{
    public class TellerProvissioningServices : BaseService
    {
        private readonly TellerServices _tellerServices;
        private readonly RoleServices _role;
        private readonly UserManagementServices _userManagementServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _brancheServices;
        public TellerProvissioningServices()
        {
            _tellerServices = new TellerServices();
            _role = new RoleServices();
            _userManagementServices = new UserManagementServices();
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _brancheServices = new BranchServices();
        }

        public async Task<ExecutionMessages> PrimaryTellerProvision(OpenningOfDayRequest model)
        {
            try
            {
                if (!ComputeDenomination(model.CurrencyNotes, Convert.ToInt32(model.InitialAmount)))
                {
                    GetExecutionMessages(model, false, $"{model.Amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "The amount you entered does not match the breakdown of the currency denominations provided. Please verify that the total cash amount you input aligns with the individual denominations listed. This ensures that the total cash in hand is accurate and properly accounted for. Review the denomination details and adjust the entered amount accordingly."
);
                    return ExecutionMessage;
                }


                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TellerProvioningHistory>>(APICallHelper.OpenningOfDayPrimaryTeller, model);
                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    await MapToTillOpenAndClossingDS(response.ApiResponseData.Data);
                    GetExecutionMessages(response, true, $"{model.Amount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.Amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }



            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task MapToTillOpenAndClossingDS(TellerProvioningHistory tellerProvisioningHistory)
        {
            var branch = await _brancheServices.GetBranch(GetBranchID());



            var tillDs = new TillOpenAndClossingDS
            {
                UserIdInChargeOfThisTeller = tellerProvisioningHistory.UserIdInChargeOfThisTeller,
                ProvisionedBy = tellerProvisioningHistory.ProvisionedBy,
                IsCashReplenished = tellerProvisioningHistory.IsCashReplenished,
                ReplenishedAmount = tellerProvisioningHistory.ReplenishedAmount,
                OpenedDate = tellerProvisioningHistory.OpenedDate.GetValueOrDefault(),
                ClossedDate = tellerProvisioningHistory.ClossedDate.GetValueOrDefault(),
                OpenOfDayAmount = tellerProvisioningHistory.OpenOfDayAmount,
                ReferenceId = tellerProvisioningHistory.ReferenceId,
                CloseOfReferenceId = tellerProvisioningHistory.CloseOfReferenceId,
                IsRequestedForCashReplenishment = tellerProvisioningHistory.IsRequestedForCashReplenishment,
                CashAtHand = tellerProvisioningHistory.CashAtHand,
                EndOfDayAmount = tellerProvisioningHistory.EndOfDayAmount,
                AccountBalance = tellerProvisioningHistory.AccountBalance,
                LastOPerationAmount = tellerProvisioningHistory.LastOPerationAmount,
                LastOperationType = tellerProvisioningHistory.LastOperationType,
                PreviouseBalance = tellerProvisioningHistory.PreviouseBalance,
                SubTellerComment = tellerProvisioningHistory.SubTellerComment,
                Note = tellerProvisioningHistory.Note,
                ClossedStatus = tellerProvisioningHistory.ClossedStatus,
                TillName = tellerProvisioningHistory.Teller.name, // Assuming you have a TillName property in the Branch object
                InitialPrinting = tellerProvisioningHistory.InitialPrinting,
                OpeningNote10000 = tellerProvisioningHistory.OpeningNote10000,
                OpeningNote5000 = tellerProvisioningHistory.OpeningNote5000,
                OpeningNote2000 = tellerProvisioningHistory.OpeningNote2000,
                OpeningNote1000 = tellerProvisioningHistory.OpeningNote1000,
                OpeningNote500 = tellerProvisioningHistory.OpeningNote500,
                OpeningCoin500 = tellerProvisioningHistory.OpeningCoin500,
                OpeningCoin100 = tellerProvisioningHistory.OpeningCoin100,
                OpeningCoin50 = tellerProvisioningHistory.OpeningCoin50,
                OpeningCoin25 = tellerProvisioningHistory.OpeningCoin25,
                OpeningCoin10 = tellerProvisioningHistory.OpeningCoin10,
                OpeningCoin5 = tellerProvisioningHistory.OpeningCoin5,
                OpeningCoin1 = tellerProvisioningHistory.OpeningCoin1,
                ClosingNote10000 = tellerProvisioningHistory.ClosingNote10000,
                ClosingNote5000 = tellerProvisioningHistory.ClosingNote5000,
                ClosingNote2000 = tellerProvisioningHistory.ClosingNote2000,
                ClosingNote1000 = tellerProvisioningHistory.ClosingNote1000,
                ClosingNote500 = tellerProvisioningHistory.ClosingNote500,
                ClosingCoin500 = tellerProvisioningHistory.ClosingCoin500,
                ClosingCoin100 = tellerProvisioningHistory.ClosingCoin100,
                ClosingCoin50 = tellerProvisioningHistory.ClosingCoin50,
                ClosingCoin25 = tellerProvisioningHistory.ClosingCoin25,
                ClosingCoin10 = tellerProvisioningHistory.ClosingCoin10,
                ClosingCoin5 = tellerProvisioningHistory.ClosingCoin5,
                ClosingCoin1 = tellerProvisioningHistory.ClosingCoin1,
                //TotalOpeningAmount = tellerProvisioningHistory.TotalOpeningAmount,
                //TotalClosingAmount = tellerProvisioningHistory.TotalClosingAmount,
                Id = tellerProvisioningHistory.Id,
                IsPrimaryTeller = tellerProvisioningHistory.IsPrimaryTeller,
                TellerType = tellerProvisioningHistory.Teller?.isPrimary == true ? "Primary-Till" : "Sub-Till",
                Logo = branch.Bank.LogoUrl,
                BranchName = branch.Name,
                BranchCode = branch.BranchCode,
                BranchAddress = branch.Address,
                BranchTelephone = branch.Telephone,
                HeadOfficeName = branch.Bank.Name,
                HeadOfficeAddress = branch.Bank.Address,
                HeadOfficeTelephone = branch.Bank.Telephone,
                HeadOfficeEmail = branch.Bank.Email,
                HeadOfficeWebSite = branch.Bank.WebSite,
                HeadOfficeInitial = branch.Bank.BankInitial,
                HeadOfficeCode = branch.Bank.BankCode
            };
            // Convert the single instance to a list
            var tillDsList = new List<TillOpenAndClossingDS> { tillDs };
            HttpContext.Current.Session["rptSource"] = tillDsList;
        }
        public List<TillOpenAndClossingDS> MapToTillOpenAndClossingDS(List<TellerProvioningHistory> tellerProvisioningHistoryList, List<Branch> branches)
        {
            var tillDsList = new List<TillOpenAndClossingDS>();

            foreach (var tellerProvisioningHistory in tellerProvisioningHistoryList)
            {
                var branch = branches.FirstOrDefault(b => b.Id == tellerProvisioningHistory.BranchId);

                if (branch == null)
                {
                    // Handle the case where the branch is not found
                    continue;
                }

                var tillDs = new TillOpenAndClossingDS
                {
                    UserIdInChargeOfThisTeller = tellerProvisioningHistory.UserIdInChargeOfThisTeller,
                    ProvisionedBy = tellerProvisioningHistory.ProvisionedBy,
                    IsCashReplenished = tellerProvisioningHistory.IsCashReplenished,
                    ReplenishedAmount = tellerProvisioningHistory.ReplenishedAmount,
                    OpenedDate = tellerProvisioningHistory.OpenedDate.GetValueOrDefault(),
                    ClossedDate = tellerProvisioningHistory.ClossedDate.GetValueOrDefault(),
                    OpenOfDayAmount = tellerProvisioningHistory.OpenOfDayAmount,
                    ReferenceId = tellerProvisioningHistory.ReferenceId,
                    CloseOfReferenceId = tellerProvisioningHistory.CloseOfReferenceId,
                    IsRequestedForCashReplenishment = tellerProvisioningHistory.IsRequestedForCashReplenishment,
                    CashAtHand = tellerProvisioningHistory.CashAtHand,
                    EndOfDayAmount = tellerProvisioningHistory.EndOfDayAmount,
                    AccountBalance = tellerProvisioningHistory.AccountBalance,
                    LastOPerationAmount = tellerProvisioningHistory.LastOPerationAmount,
                    LastOperationType = tellerProvisioningHistory.LastOperationType,
                    PreviouseBalance = tellerProvisioningHistory.PreviouseBalance,
                    SubTellerComment = tellerProvisioningHistory.SubTellerComment,
                    Note = tellerProvisioningHistory.Note,
                    ClossedStatus = tellerProvisioningHistory.ClossedStatus,
                    TillName = tellerProvisioningHistory.Teller?.name ?? "Unknown",
                    IsPrimaryTeller = tellerProvisioningHistory.Teller?.isPrimary ?? false,
                    TellerType = tellerProvisioningHistory.Teller?.isPrimary == true ? "Primary-Till" : "Sub-Till",
                    InitialPrinting = tellerProvisioningHistory.InitialPrinting,

                    // Opening Denominations
                    OpeningNote10000 = tellerProvisioningHistory.OpeningNote10000,
                    OpeningNote5000 = tellerProvisioningHistory.OpeningNote5000,
                    OpeningNote2000 = tellerProvisioningHistory.OpeningNote2000,
                    OpeningNote1000 = tellerProvisioningHistory.OpeningNote1000,
                    OpeningNote500 = tellerProvisioningHistory.OpeningNote500,
                    OpeningCoin500 = tellerProvisioningHistory.OpeningCoin500,
                    OpeningCoin350 = tellerProvisioningHistory.OpeningCoin350,
                    OpeningCoin250 = tellerProvisioningHistory.OpeningCoin250,
                    OpeningCoin200 = tellerProvisioningHistory.OpeningCoin200,
                    OpeningCoin150 = tellerProvisioningHistory.OpeningCoin150,
                    OpeningCoin100 = tellerProvisioningHistory.OpeningCoin100,
                    OpeningCoin50 = tellerProvisioningHistory.OpeningCoin50,
                    OpeningCoin25 = tellerProvisioningHistory.OpeningCoin25,
                    OpeningCoin10 = tellerProvisioningHistory.OpeningCoin10,
                    OpeningCoin5 = tellerProvisioningHistory.OpeningCoin5,
                    OpeningCoin1 = tellerProvisioningHistory.OpeningCoin1,

                    // Closing Denominations
                    ClosingNote10000 = tellerProvisioningHistory.ClosingNote10000,
                    ClosingNote5000 = tellerProvisioningHistory.ClosingNote5000,
                    ClosingNote2000 = tellerProvisioningHistory.ClosingNote2000,
                    ClosingNote1000 = tellerProvisioningHistory.ClosingNote1000,
                    ClosingNote500 = tellerProvisioningHistory.ClosingNote500,
                    ClosingCoin500 = tellerProvisioningHistory.ClosingCoin500,
                    ClosingCoin350 = tellerProvisioningHistory.ClosingCoin350,
                    ClosingCoin250 = tellerProvisioningHistory.ClosingCoin250,
                    ClosingCoin200 = tellerProvisioningHistory.ClosingCoin200,
                    ClosingCoin150 = tellerProvisioningHistory.ClosingCoin150,
                    ClosingCoin100 = tellerProvisioningHistory.ClosingCoin100,
                    ClosingCoin50 = tellerProvisioningHistory.ClosingCoin50,
                    ClosingCoin25 = tellerProvisioningHistory.ClosingCoin25,
                    ClosingCoin10 = tellerProvisioningHistory.ClosingCoin10,
                    ClosingCoin5 = tellerProvisioningHistory.ClosingCoin5,
                    ClosingCoin1 = tellerProvisioningHistory.ClosingCoin1,

                    //TotalOpeningAmount = tellerProvisioningHistory.TotalOpeningAmount,
                    //TotalClosingAmount = tellerProvisioningHistory.TotalClosingAmount,
                    Id = tellerProvisioningHistory.Id,

                    // Branch and Bank Information
                    Logo = branch.Bank?.LogoUrl,
                    BranchName = branch.Name,
                    BranchCode = branch.BranchCode,
                    BranchAddress = branch.Address,
                    BranchTelephone = branch.Telephone,
                    HeadOfficeName = branch.Bank?.Name,
                    HeadOfficeAddress = branch.Bank?.Address,
                    HeadOfficeTelephone = branch.Bank?.Telephone,
                    HeadOfficeEmail = branch.Bank?.Email,
                    HeadOfficeWebSite = branch.Bank?.WebSite,
                    HeadOfficeInitial = branch.Bank?.BankInitial,
                    HeadOfficeCode = branch.Bank?.BankCode
                };

                tillDsList.Add(tillDs);
            }

            // Store the list in the session
            HttpContext.Current.Session["rptSource"] = tillDsList;
            return tillDsList;
        }

        public async Task<ExecutionMessages> SubTellerProvision(OpenningOfDayRequest model)
        {
            try
            {
                if (!ComputeDenomination(model.CurrencyNotes, Convert.ToInt32(model.InitialAmount)))
                {
                    GetExecutionMessages(model, false, $"{model.InitialAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "The amount you entered does not match the breakdown of the currency denominations provided. Please verify that the total cash amount you input aligns with the individual denominations listed. This ensures that the total cash in hand is accurate and properly accounted for. Review the denomination details and adjust the entered amount accordingly.");
                    return ExecutionMessage;
                }
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TellerProvioningHistory>>(APICallHelper.OpenningOfDaySubTeller, model);
                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    await MapToTillOpenAndClossingDS(response.ApiResponseData.Data);
                    GetExecutionMessages(response, true, $"{model.InitialAmount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.InitialAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }



            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<List<Teller>> GetPrimaryTellers()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var branches = await _brancheServices.GetBranches();
                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    join b in branches on a.branchId equals b.Id
                                    where a.isPrimary
                                    select new Teller
                                    {
                                        activeStatus = a.activeStatus,
                                        bankId = a.bankId,
                                        branchId = a.branchId,
                                        code = a.code,
                                        id = a.id,
                                        name = $"{a.name}-{b.Name}"

                                    }).ToList();
                    return pTellers.ToList();
                }
                else
                {

                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    where a.isPrimary && a.branchId == GetBranchID()
                                    select a).ToList();
                    return pTellers.ToList();
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Teller>> GetSubTellers()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var branches = await _brancheServices.GetBranches();
                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    join b in branches on a.branchId equals b.Id
                                    where !a.isPrimary
                                    select new Teller
                                    {
                                        activeStatus = a.activeStatus,
                                        bankId = a.bankId,
                                        branchId = a.branchId,
                                        code = a.code,
                                        id = a.id,
                                        name = $"{a.name}-{b.Name}"

                                    }).ToList();
                    return pTellers.ToList();
                }
                else
                {

                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    where !a.isPrimary && a.branchId == GetBranchID()
                                    select a).ToList();
                    return pTellers.ToList();
                }


            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Teller>> GetSubTellers(string primaryTellerId)
        {
            try
            {
                // Retrieve the primary teller
                var teller = await _tellerServices.GetTeller(primaryTellerId);

                // Ensure the primary teller exists
                if (teller == null)
                {
                    // Handle the case where the primary teller is not found
                    return new List<Teller>(); // Or throw an exception or handle appropriately
                }

                // Retrieve all tellers
                var allTellers = await GetSubTellers();

                // Filter tellers by branch ID
                var branchTellers = allTellers.Where(x => x.branchId == teller.branchId).ToList();
                return branchTellers;
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception or handle as needed
            }
        }

        public async Task<List<Teller>> GetUserTellerRole()
        {
            try
            {
                List<Teller> pTellers;

                if (IsHeadOffice())
                {
                    // GetAllowAnonymous all user roles and filter for tellers
                    var userRoles = await _userManagementServices.GetUSerRoles();
                    var branches = await _brancheServices.GetBranches();
                    pTellers = (from a in userRoles
                                join b in branches on a.branchId equals b.Id
                                where a.IsTeller
                                select new Teller
                                {
                                    name = $"{a.RoleName}-{a.FirstName} {a.LastName} {b.Name}",
                                    id = a.UserId.ToString(),
                                }).ToList();

                    // If no tellers are found, add a default one
                    if (!pTellers.Any())
                    {
                        pTellers.Add(new Teller { name = $"Primary Teller-Default Admin", id = "4b352b37-332a-40c6-ab05-e38fcf109719" });
                    }
                }
                else
                {
                    // GetAllowAnonymous user roles for the current branch and filter for tellers
                    var userRoles = await _userManagementServices.GetUSerRoles();
                    pTellers = (from a in userRoles
                                where a.IsTeller && a.branchId == GetBranchID()
                                select new Teller
                                {
                                    name = $"{a.RoleName}-{a.FirstName} {a.LastName}",
                                    id = a.UserId.ToString(),
                                }).ToList();
                }

                return pTellers;
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                throw ex;
            }
        }
        public async Task<IEnumerable<StringValues>> GetUserTellerRoleDropDown()
        {
            try
            {
                List<StringValues> stringValues;

                if (IsHeadOffice())
                {
                    // GetAllowAnonymous all user roles and filter for tellers
                    var userRoles = await _userManagementServices.GetUSerRoles();
                    var branches = await _brancheServices.GetBranches();
                    stringValues = (from a in userRoles
                                    join b in branches on a.branchId equals b.Id
                                    where a.IsTeller
                                    select new StringValues
                                    {
                                        Text = $"[{a.RoleName}]-[{a.FirstName} {a.LastName}] [{b.Name}]",
                                        Value = $"{a.UserId}@{a.FirstName} {a.LastName}",
                                    }).ToList();


                }
                else
                {
                    // GetAllowAnonymous user roles for the current branch and filter for tellers
                    var userRoles = await _userManagementServices.GetUSerRoles();
                    stringValues = (from a in userRoles
                                    where a.IsTeller && a.branchId == GetBranchID()
                                    select new StringValues
                                    {
                                        Text = $"[{a.RoleName}]-[{a.FirstName} {a.LastName}]",
                                        Value = $"{a.UserId}@{a.FirstName} {a.LastName}",
                                    }).ToList();
                }

                return stringValues;
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                throw ex;
            }
        }

        public async Task<List<Teller>> GetUserTellerRole(string primaryTellerId)
        {
            try
            {
                // Retrieve the primary teller
                var teller = await _tellerServices.GetTeller(primaryTellerId);

                // Ensure the primary teller exists
                if (teller == null)
                {
                    // Handle the case where the primary teller is not found
                    return new List<Teller>(); // Or throw an exception or handle appropriately
                }

                // Retrieve all users with teller role
                var allTellerUsers = await GetUserTellerRole();

                // Filter users by branch ID
                var users = allTellerUsers.Where(x => x.branchId == teller.branchId).ToList();
                return users;
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception or handle as needed
            }
        }


        //public async Task<List<Teller>> GetUserRole()
        //{
        //    try
        //    {
        //        var pTellers = (from a in await _userManagementServices.GetUserList()
        //                        join r in await _role.GetRoles() on a.roleID equals r.Id
        //                        where r.IsTeller
        //                        select new Teller
        //                        {
        //                            Name = $"{r.Name}-{a.firstName} {a.lastName}",
        //                            id = a.id.ToString(),
        //                        }).ToList();

        //        if (!pTellers.Any())
        //        {
        //            pTellers.Add(new Teller { Name = $"Primary Teller-Default Admin", id = "4b352b37-332a-40c6-ab05-e38fcf109719" });
        //        }
        //        return pTellers.ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw ex;
        //    }
        //}
    }
}
