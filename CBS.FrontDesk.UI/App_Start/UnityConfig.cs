
using System.Configuration;
using System.Web.Mvc;
using CBS.API.Helper;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;

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
            container.RegisterType<IMemberAccountJob, MemberAccountJob>();
            container.RegisterType<IBranchServices, BranchServices>();
            
            container.RegisterType<CountryServices, CountryServices>();
            container.RegisterType<RegionServices, RegionServices>();
            container.RegisterType<DivisionServices, DivisionServices>();
            container.RegisterType<SubDivisionServices, SubDivisionServices>();
            container.RegisterType<TownServices, TownServices>();
            container.RegisterType<OrganizationServices, OrganizationServices>();
            container.RegisterType<BankServices, BankServices>();
            //container.RegisterType<ApiCallerHelper, ApiCallerHelper>();
            //container.RegisterType<APICallHelper, APICallHelper>();
            // Register ApiCallerHelper for TransactionBaseUrl
            container.RegisterType<ApiCallerHelper>("TransactionApiCallerHelper",
                new InjectionConstructor(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString()));

            // Register ApiCallerHelper for BankConfigurationBaseUrl
            container.RegisterType<ApiCallerHelper>("BankConfigApiCallerHelper",
                new InjectionConstructor(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString()));
            container.RegisterType<ApiCallerHelper>(new InjectionConstructor(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString()));
            container.RegisterType<ApiCallerHelper>(
    new InjectionConstructor(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString()));

            // If you're using Unity.MVC, you can register it with the PerRequestLifetimeManager:
            // container.RegisterType<ApiCallerHelper>(
            //     new PerRequestLifetimeManager(),
            //     new InjectionConstructor(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString()));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }

}