using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using CBS.API.Helper;
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
        List<UserSessionDataTable> MapUserSessionDtoToDataTable(List<UserSessionDto> sessionDtos);
        Guid ConvertStringToGuid(string input);
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(Guid userid);
        Task<IEnumerable<StringValues>> GetUserDropDownList();
        Task<IEnumerable<UserSessionDto>> GetUserSessions();
        Task<IEnumerable<UserSessionDto>> GetUserSessions(Guid userid);
        Task<UserDto> GetUserDto(Guid userid);
        Task<ExecutionMessages> DeleteUser(Guid userid);
        Task<ExecutionMessages> UpdateUserProfile(User user);
        Task<ExecutionMessages> ChangePassword(User user);
        Task<ExecutionMessages> ResetPassword(User user);
        Task<ExecutionMessages> UploadPicture(HttpPostedFileBase uploadBase);
        Task<CustomDataTable> GetUsersDataTable(DataTableOptions dataTableOptions);
        Task<ExecutionMessages> FLoginChangePassword(FLoginChangePassword fLogin);
        Task<IEnumerable<Branch>> GetBranches();
        Task<ExecutionMessages> EnableMFA(MFAActivation mFAActivation);
        Task<ExecutionMessages> MFACodeVerification(MFAActivation mFAActivation);

        Task<CustomDataTable> GetDataTableAsync(GetAllUserSessionsDataTableQuery getAllUsersDataTableQuery);
        Task<CustomDataTable> GetDataTableAsync(GetAllUsersDataTableQuery getAllUsersDataTableQuery);

    }
}