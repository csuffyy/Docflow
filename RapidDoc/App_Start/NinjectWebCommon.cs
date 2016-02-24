[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(RapidDoc.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(RapidDoc.App_Start.NinjectWebCommon), "Stop")]

namespace RapidDoc.App_Start
{
    using System;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using RapidDoc.Mappers;
    using RapidDoc.Models.Infrastructure;
    using RapidDoc.Models.Services;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                //GlobalConfiguration.Configuration.DependencyResolver = new RapidDoc.Models.Infrastructure.NinjectDependencyResolver(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IMapper>().To<CommonMapper>().InSingletonScope();
            kernel.Bind<IUnitOfWork>().To<UnitOfWork<ApplicationDbContext>>().InRequestScope();
            kernel.Bind<IDomainService>().To<DomainService>();
            kernel.Bind<ICompanyService>().To<CompanyService>();
            kernel.Bind<IProfileService>().To<ProfileService>();
            kernel.Bind<ITitleService>().To<TitleService>();
            kernel.Bind<IGroupProcessService>().To<GroupProcessService>();
            kernel.Bind<IProcessService>().To<ProcessService>();
            kernel.Bind<IEmplService>().To<EmplService>();
            kernel.Bind<IDelegationService>().To<DelegationService>();
            kernel.Bind<IDepartmentService>().To<DepartmentService>();
            kernel.Bind<IDocumentService>().To<DocumentService>();
            kernel.Bind<INumberSeqService>().To<NumberSeqService>();
            kernel.Bind<IAccountService>().To<AccountService>();
            kernel.Bind<IWorkflowService>().To<WorkflowService>();
            kernel.Bind<ISystemService>().To<SystemService>();
            kernel.Bind<IWorkflowTrackerService>().To<WorkflowTrackerService>();
            kernel.Bind<IReviewDocLogService>().To<ReviewDocLogService>();
            kernel.Bind<IDocumentReaderService>().To<DocumentReaderService>();
            kernel.Bind<ICommentService>().To<CommentService>();
            kernel.Bind<IWorkScheduleService>().To<WorkScheduleService>();
            kernel.Bind<IEmailService>().To<EmailService>();
            kernel.Bind<IHistoryUserService>().To<HistoryUserService>();
            kernel.Bind<ISearchService>().To<SearchService>();
            kernel.Bind<ICustomCheckDocument>().To<CustomCheckDocument>();
            kernel.Bind<IServiceIncidentService>().To<ServiceIncidentService>();
            kernel.Bind<ITripSettingsService>().To<TripSettingsService>();
            kernel.Bind<IItemCauseService>().To<ItemCauseService>();
            kernel.Bind<IReportService>().To<ReportService>();
            kernel.Bind<IModificationUsersService>().To<ModificationUsersService>();
            kernel.Bind<INotificationUsersService>().To<NotificationUsersService>();
            kernel.Bind<IDocumentBaseService>().To<DocumentBaseService>();
            kernel.Bind<IOrganizationService>().To<OrganizationService>();
            kernel.Bind<ICountryService>().To<CountryService>();
            kernel.Bind<IReasonRequestService>().To<ReasonRequestService>();
            kernel.Bind<IQuestionRequestService>().To<QuestionRequestService>();
            kernel.Bind<IProtocolFoldersService>().To<ProtocolFoldersService>();
            kernel.Bind<ITripMRPService>().To<TripMRPService>();
            kernel.Bind<IDocumentSubcriptionService>().To<DocumentSubcriptionService>();
            kernel.Bind<IPortalParametersService>().To<PortalParametersService>();
        }        
    }
}
