
using System.Web.Mvc;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using Unity;
using Unity.AspNet.Mvc;


namespace CBS.FrontDesk.UI
{


    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register your dependencies here using container.RegisterType<>()
            container.RegisterType<IAuthenticationServices, AuthenticationServices>();
            container.RegisterType<IUserManagementServices, UserManagementServices>();
            container.RegisterType<IIndividualProfileServices, IndividualProfileServices>();
            container.RegisterType<CountryServices, CountryServices>();
            container.RegisterType<RegionServices, RegionServices>();
            container.RegisterType<DivisionServices, DivisionServices>();
            container.RegisterType<SubDivisionServices, SubDivisionServices>();
            container.RegisterType<TownServices, TownServices>();
            container.RegisterType<OrganizationServices, OrganizationServices>();
            container.RegisterType<BankServices, BankServices>();
            container.RegisterType<BranchServices, BranchServices>();
            container.RegisterType<ApiCallerHelper, ApiCallerHelper>();
            container.RegisterType<APICallHelper, APICallHelper>();
            
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }

}