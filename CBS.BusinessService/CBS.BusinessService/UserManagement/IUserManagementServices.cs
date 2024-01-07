using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;

namespace CBS.BusinessService.UserManagement
{
    public interface IUserManagementServices
    {
        Task<ExecutionMessages> CreateUser(User user);
        Task<IEnumerable<Role>> GetRoles();
        Guid ConvertStringToGuid(string input);
        Task<IEnumerable<UserList>> GetUserList();
        Task<UserList> GetUser(Guid userid);
        Task<ExecutionMessages> DeleteUser(Guid userid);
        Task<ExecutionMessages> UpdateUserProfile(UserList user);
        Task<ExecutionMessages> ChangePassword(UserList user);
        Task<ExecutionMessages> ResetPassword(UserList user);
        Task<ExecutionMessages> UploadPicture(HttpPostedFileBase uploadBase);
        Task<CustomDataTable> GetUsersDataTable(DataTableOptions dataTableOptions);
        Task<IEnumerable<Branch>> GetBranches();



    }
}