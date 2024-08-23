using CBS.API.Helper;
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;

using System.Configuration;
using System.Web.Mvc;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;

namespace CBS.FrontDesk.UI
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();
            // Register your hub class
            container.RegisterType<NotificationHub>(new ContainerControlledLifetimeManager());

            // Register your dependencies here using container.RegisterType<>()
            container.RegisterType<IAuthenticationServices, AuthenticationServices>();
            container.RegisterType<IUserManagementServices, UserManagementServices>();
            //container.RegisterType<IMemberAccountJob, MemberAccountJob>();
            container.RegisterType<IBranchServices, BranchServices>();

            container.RegisterType<CountryServices, CountryServices>();
            container.RegisterType<RegionServices, RegionServices>();
            container.RegisterType<AccountingServices, AccountingServices>();
            container.RegisterType<IAccountingEntryServices, AccountingEntryServices>();
            container.RegisterType<IUserManagementServices, UserManagementServices>();

            container.RegisterType<SubDivisionServices, SubDivisionServices>();
            container.RegisterType<IBranchServices, BranchServices>();
            container.RegisterType<TownServices, TownServices>();
            container.RegisterType<OrganizationServices, OrganizationServices>();
            container.RegisterType<BankServices, BankServices>();
            container.RegisterType<ApiCallerHelper, ApiCallerHelper>();
            container.RegisterType<APICallHelper, APICallHelper>();

            container.RegisterType<INotificationServices, NotificationServices>();
            container.RegisterType<ApiCallerHelper>("TransactionApiCallerHelper",
                new InjectionConstructor(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString()));

            // Register ApiCallerHelper for BankConfigurationBaseUrl
            container.RegisterType<ApiCallerHelper>("BankConfigApiCallerHelper",
                new InjectionConstructor(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString()));
            container.RegisterType<ApiCallerHelper>(new InjectionConstructor(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString()));
            container.RegisterType<ApiCallerHelper>(
    new InjectionConstructor(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString()));

            container.RegisterType<ApiCallerHelper>(new InjectionConstructor(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString()));

            // If you're using Unity.MVC, you can register it with the PerRequestLifetimeManager:
            // container.RegisterType<ApiCallerHelper>(
            //     new PerRequestLifetimeManager(),
            //     new InjectionConstructor(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString()));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}