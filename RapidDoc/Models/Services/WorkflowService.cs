﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Security;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Activities.Statements;
using System.Runtime.DurableInstancing;
using System.Threading;
using System.Activities.Hosting;
using RapidDoc.Activities;
using System.ServiceModel.Activities.Description;
using RapidDoc.Models.Services;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Activities.XamlIntegration;
using System.Activities.Expressions;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.Repository;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using RapidDoc.Activities.CodeActivities;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RapidDoc.Models.Services
{
    public interface IWorkflowService
    {
        WFUserFunctionResult WFMatchingUpManager(Guid documentId, string currentUserId, int level = 1, string profileName = "");
        WFUserFunctionResult WFSpecificUser(Guid documentId, String userName);
        WFUserFunctionResult WFRoleUser(Guid documentId, String roleName);
        WFUserFunctionResult WFStaffStructure(Guid documentId, Expression<Func<EmplTable, bool>> predicate);
        WFUserFunctionResult WFCreatedUser(Guid documentId);
        WFUserFunctionResult WFUsersDocument(Guid documentId, string currentUserId);
        WFUserFunctionResult WFChooseManual(Guid documentId, Dictionary<string, Object> documentData, string manualKey, string currentUserId);
        string WFChooseSpecificUserFromService(string serviceName, ServiceIncidientPriority priority, ServiceIncidientLevel level, ServiceIncidientLocation location);
        void RunWorkflow(DocumentTable documentTable, string TableName, IDictionary<string, object> documentData, string currentUserId = "");
        void AgreementWorkflowApprove(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId, IDictionary<string, object> documentData);
        void ActiveWorkflowApprove(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId, IDictionary<string, object> documentData, string currentUser);
        void AgreementWorkflowReject(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId, IDictionary<string, object> documentData);
        void AgreementWorkflowWithdraw(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId);
        void CreateTrackerRecord(string systemName, DocumentState step, Guid documentId, string bookmarkName, List<WFTrackerUsersTable> listUser, string currentUserId, string workflowId, bool useManual, int slaOffset, bool executionStep);
        List<Array> GetRequestTree(Activity activity, IDictionary<string, object> documentData, string _parallel = "");
        List<Array> GetTrackerList(Guid documentId, Activity activity, IDictionary<string, object> documentData, DocumentType documentType);
        List<string> GetUniqueUserList(Guid documentId, IDictionary<string, object> documentData, string nameField, bool getAll = false);
        List<string> EmplAndRolesToUserList(Guid documentId, string[] list);
        List<string> EmplAndRolesToReaders(Guid documentId, string[] list);
        void CreateDynamicTracker(List<string> users, Guid documentId, string currentUserId, bool parallel, string additionalText = "");
        bool CheckSkipStepOrder(Guid documentId, List<WFTrackerUsersTable> userlist, string createdBy);
        void CreateSeparateTasks(ProcessView processView, dynamic docModel, Guid fileId, String actionModelName, IDictionary<string, object> documentData, ApplicationUser userTable);
        Guid GuidNull2Guid(Guid? value);      
    }

    public class WorkflowService : IWorkflowService
    {
        private IRepository<ApplicationUser> repoUser;
        private IRepository<ServiceIncidentTable> repoIncident;
        private IUnitOfWork _uow;
        private readonly IDocumentService _DocumentService;
        private readonly IEmplService _EmplService;
        private readonly IWorkflowTrackerService _WorkflowTrackerService;
        private readonly IEmailService _EmailService;
        private readonly IHistoryUserService _HistoryUserService;
        private readonly IReviewDocLogService _ReviewDocLogService;
        private readonly ICustomCheckDocument _CustomCheckDocument;
        private readonly IProcessService _ProcessService;
        private readonly ISystemService _SystemService;
        private readonly INotificationUsersService _NotificationUsersService;
        private readonly ISearchService _SearchService;
        private readonly ICommentService _CommentService;

        protected RoleManager<ApplicationRole> RoleManager { get; private set; }
        
        IDictionary<string, object> outputParameters;              

        public WorkflowService(IUnitOfWork uow, IDocumentService documentService, IEmplService emplService, 
            IWorkflowTrackerService workflowTrackerService, IEmailService emailService, IHistoryUserService historyUserService,
            IReviewDocLogService reviewDocLogService, ICustomCheckDocument customCheckDocument, IProcessService processService, ISystemService systemService, INotificationUsersService notificationUsersService, ISearchService searchService, ICommentService commentService)
        {
            repoUser = uow.GetRepository<ApplicationUser>();
            repoIncident = uow.GetRepository<ServiceIncidentTable>();
            _uow = uow;
            _DocumentService = documentService;
            _EmplService = emplService;
            _WorkflowTrackerService = workflowTrackerService;
            _EmailService = emailService;
            _HistoryUserService = historyUserService;
            _ReviewDocLogService = reviewDocLogService;
            _CustomCheckDocument = customCheckDocument;
            _ProcessService = processService;
            _SystemService = systemService;
            _NotificationUsersService = notificationUsersService;
            _SearchService = searchService;
            _CommentService = commentService;

            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
        }

        public WFUserFunctionResult WFMatchingUpManager(Guid documentId, string currentUserId, int level = 1, string profileName = "")
        {
            bool skip = false;
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();
            EmplTable currentEmplUser = _EmplService.FirstOrDefault(x => x.ApplicationUserId == documentTable.ApplicationUserCreatedId && x.Enable == true);
            EmplTable manager = WFMatchingUpManagerFinder(currentEmplUser, level, currentUserId, profileName);

            if (manager != null)
                userList.Add(new WFTrackerUsersTable { UserId = manager.ApplicationUserId });

            if (!String.IsNullOrEmpty(profileName) && !String.IsNullOrWhiteSpace(profileName))
            {
                if(userList.Count == 0)
                {
                    skip = true;
                }
                else
                {
                    skip = checkSkipStep(documentId, userList, documentTable.ApplicationUserCreatedId);
                }
            }

            return new WFUserFunctionResult { Users = userList, Skip = skip };
        }
        private bool checkSkipStep(Guid documentId, List<WFTrackerUsersTable> userlist, string createdBy)
        {
            var trackerTables = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Approved).ToList();

            foreach (var user in userlist)
            {
                if (user.UserId == createdBy || (trackerTables != null && trackerTables.Any(x => x.SignUserId == user.UserId)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckSkipStepOrder(Guid documentId, List<WFTrackerUsersTable> userlist, string createdBy)
        {
            var trackerTables = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Approved).ToList();

            foreach (var user in userlist)
            {
                if (user.UserId == createdBy || (trackerTables != null && trackerTables.Any(x => x.SignUserId == user.UserId)))
                {
                    return true;
                }
            }

            return false;
        }

        private EmplTable WFMatchingUpManagerFinder(EmplTable emplTable, int level, string currentUserId, string profileName = "")
        {
            if (((level == 0 && profileName == null) || (emplTable.ProfileName == profileName || emplTable.TitleTable.ProfileName == profileName)) && emplTable.Enable == true) return emplTable;
            EmplTable manager = _EmplService.FindIntercompany(emplTable.ManageId ?? Guid.Empty);

            if(manager == null || manager.Id == manager.ManageId)
                return null;

            level--;
            return WFMatchingUpManagerFinder(manager, level, currentUserId, profileName);
        }
        public WFUserFunctionResult WFSpecificUser(Guid documentId, String userName)
        {
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();
            ApplicationUser userTable = repoUser.Find(x => (x.UserName == userName || x.Id == userName) && x.Enable == true);

            if (userTable != null)
            {
                userList.Add(new WFTrackerUsersTable { UserId = userTable.Id });
            }

            return new WFUserFunctionResult { Users = userList, Skip = checkSkipStep(documentId, userList, documentTable.ApplicationUserCreatedId) };
        }
        public WFUserFunctionResult WFChooseManual(Guid documentId, Dictionary<string, Object> documentData, string manualKey, string currentUserId)
        {
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();
            
            if ((string)documentData[manualKey] != "" )
            {              
                string[] result = _DocumentService.GetUserListFromStructure((string)documentData[manualKey]);
                foreach (var item in result)
                {
                    Guid emplId = Guid.Parse(item);
                    EmplTable empl = _EmplService.Find(emplId, currentUserId);

                    if (empl.Enable == true)
                        userList.Add(new WFTrackerUsersTable { UserId = empl.ApplicationUserId });
                }
            }
            return new WFUserFunctionResult { Users = userList, Skip = checkSkipStep(documentId, userList, documentTable.ApplicationUserCreatedId) }; 
        }
        public WFUserFunctionResult WFRoleUser(Guid documentId, String roleName)
        {
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();

            if (!String.IsNullOrEmpty(roleName))
            {
                RoleManager<ApplicationRole> RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));

                if (RoleManager.RoleExists(roleName))
                {
                    var users = RoleManager.FindByName(roleName).Users;
                    foreach (IdentityUserRole user in users)
                    {
                        userList.Add(new WFTrackerUsersTable { UserId = user.UserId });
                    }
                    RoleManager.Dispose();
                }
            }
            return new WFUserFunctionResult { Users = userList, Skip = checkSkipStep(documentId, userList, documentTable.ApplicationUserCreatedId) };
        }

        public WFUserFunctionResult WFStaffStructure(Guid documentId, Expression<Func<EmplTable, bool>> predicate)
        {
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();
            var empls = _EmplService.GetPartialIntercompany(predicate).Where(x => x.Enable == true).Select(x => x.ApplicationUserId).ToList();

            foreach(string empl in empls)
            {
                userList.Add(new WFTrackerUsersTable { UserId = empl });
            }

            return new WFUserFunctionResult { Users = userList, Skip = checkSkipStep(documentId, userList, documentTable.ApplicationUserCreatedId) };
        }
        public WFUserFunctionResult WFCreatedUser(Guid documentId)
        {
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();

            ApplicationUser userTable = repoUser.Find(x => x.Id == documentTable.ApplicationUserCreatedId && x.Enable == true);

            if (userTable != null)
                userList.Add(new WFTrackerUsersTable { UserId = userTable.Id });

            return new WFUserFunctionResult { Users = userList, Skip = false };
        }
        public WFUserFunctionResult WFUsersDocument(Guid documentId, string currentUserId)
        {
            var documentTable = _DocumentService.Find(documentId);
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();
            var domainTable = _DocumentService.RouteCustomRepository(documentTable.ProcessTable.TableName).GetById(documentTable.RefDocumentId);

            if (domainTable != null)
            {
                if(domainTable.Users != null)
                {
                    string[] result = _DocumentService.GetUserListFromStructure(domainTable.Users);
                    foreach(var item in result)
                    {
                        Guid emplId = Guid.Parse(item);
                        EmplTable empl = _EmplService.FindIntercompany(emplId);

                        if (empl != null && empl.Enable == true && empl.ApplicationUserId != null && (empl.ApplicationUserId != documentTable.ApplicationUserCreatedId || documentTable.DocType == DocumentType.Task))
                        {
                            userList.Add(new WFTrackerUsersTable { UserId = empl.ApplicationUserId });
                        }
                    }
                }
            }
            if (documentTable.DocType != DocumentType.Task)
                userList.Add(new WFTrackerUsersTable { UserId = documentTable.ApplicationUserCreatedId });
            return new WFUserFunctionResult { Users = userList, Skip = false };
        }
        public string WFChooseSpecificUserFromService(string serviceName, ServiceIncidientPriority priority, ServiceIncidientLevel level, ServiceIncidientLocation location)
        {
            var rm = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
            ServiceIncidentTable incidentTable = repoIncident.Find(x => x.ServiceName == serviceName && x.ServiceIncidientLevel == level && x.ServiceIncidientPriority == priority && x.ServiceIncidientLocation == location);

            if (incidentTable != null)
            {
                ApplicationRole identityRole = rm.FindById(incidentTable.RoleTableId);
                return identityRole.Name;
            }

            return String.Empty;
        }
        public void RunWorkflow(DocumentTable documentTable, string TableName, IDictionary<string, object> documentData, string currentUserId = "")
        {        
            SqlWorkflowInstanceStore instanceStore = SetupInstanceStore();
            FileTable fileTableWF = GetActualFileWF(TableName, documentTable);
            Activity activity = ChooseActualWorkflow(TableName, fileTableWF);
            _WorkflowTrackerService.SaveTrackList(documentTable.Id, this.GetTrackerList(documentTable.Id, activity, documentData, documentTable.DocType), currentUserId);
            StartAndPersistInstance(documentTable.Id, DocumentState.Agreement, documentData, instanceStore, activity, fileTableWF, currentUserId); 
            DeleteInstanceStoreOwner(instanceStore);
            _EmailService.SendExecutorEmail(documentTable.Id, documentData.ContainsKey("AdditionalText") ? (string)documentData["AdditionalText"] : "");
            if (documentTable.IsNotified == true)
            {
                foreach (var user in _DocumentService.GetSignUsersDirect(documentTable))
                {
                    _NotificationUsersService.CreateNotifyForUser(documentTable.Id, documentTable.ApplicationUserCreatedId, user.Id);
                }
            }                     
        }
        public void AgreementWorkflowApprove(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId, IDictionary<string, object> documentData)
        {
            string currentUserId = HttpContext.Current.User.Identity.GetUserId();
            IEnumerable<WFTrackerTable> bookmarks = _DocumentService.GetCurrentSignStep(documentId, currentUserId).ToList();
            _DocumentService.SaveSignData(bookmarks, TrackerType.Approved, String.IsNullOrEmpty("") ? true : false);
            if (bookmarks != null)
            {
                foreach (var bookmark in bookmarks)
                {
                    SqlWorkflowInstanceStore instanceStore = SetupInstanceStore();

                    WorkflowApplicationInstance instanceInfo =
                            WorkflowApplication.GetInstance(WWFInstanceId, instanceStore);

                    FileTable fileTableWF = GetRightFileWF(TableName, processId, instanceInfo);
                    Activity activity = ChooseActualWorkflow(TableName, fileTableWF, instanceInfo.DefinitionIdentity != null);
                    LoadAOrCompleteInstance(documentId, DocumentState.Agreement, TrackerType.Approved, documentData, instanceStore, activity, instanceInfo, bookmark);
                    DeleteInstanceStoreOwner(instanceStore);
                    _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.ApproveDocument }, HttpContext.Current.User.Identity.GetUserId());
                    DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == documentId);
                    if (docTable.DocType == DocumentType.Order)
                    {
                        WFTrackerTable stepCount = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == docTable.Id).OrderByDescending(x => x.LineNum).FirstOrDefault(x => x.SystemName != null);
                        _EmailService.SendExecutorEmail(documentId, documentData.ContainsKey("AdditionalText") ? (string)documentData["AdditionalText"] : "", stepCount != null && _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == docTable.Id && x.LineNum > stepCount.LineNum).Count() == 1 ? true : false);
                    }
                    else
                        _EmailService.SendExecutorEmail(documentId, documentData.ContainsKey("AdditionalText") ? (string)documentData["AdditionalText"] : "");

                    
                    if (docTable.IsNotified == true)
                    {
                        foreach (var user in _DocumentService.GetSignUsersDirect(docTable))
                        {
                            _NotificationUsersService.CreateNotifyForUser(documentId, _DocumentService.FirstOrDefault(x => x.Id == documentId).ApplicationUserCreatedId, user.Id);
                        }
                    }
                }
            }
        }
        public void AgreementWorkflowReject(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId, IDictionary<string, object> documentData)
        {
            string currentUserId = HttpContext.Current.User.Identity.GetUserId();
            IEnumerable<WFTrackerTable> bookmarks = _DocumentService.GetCurrentSignStep(documentId, currentUserId).ToList();
            _DocumentService.SaveSignData(bookmarks, TrackerType.Cancelled, String.IsNullOrEmpty("") ? true : false);
            if (bookmarks != null)
            {
                foreach (var bookmark in bookmarks)
                {
                    SqlWorkflowInstanceStore instanceStore = SetupInstanceStore();

                    WorkflowApplicationInstance instanceInfo =
                            WorkflowApplication.GetInstance(WWFInstanceId, instanceStore);

                    FileTable fileTableWF = GetRightFileWF(TableName, processId, instanceInfo);
                    Activity activity = ChooseActualWorkflow(TableName, fileTableWF, instanceInfo.DefinitionIdentity != null);
                    LoadAOrCompleteInstance(documentId, DocumentState.Cancelled, TrackerType.Cancelled, documentData, instanceStore, activity, instanceInfo, bookmark);
                    DeleteInstanceStoreOwner(instanceStore);
                    _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.CancelledDocument }, HttpContext.Current.User.Identity.GetUserId());
                    _EmailService.SendInitiatorRejectEmail(documentId);
                }
            }
        }

        public void ActiveWorkflowApprove(Guid documentId, string TableName, Guid WWFInstanceId, Guid processId, IDictionary<string, object> documentData, string currentUser)
        {
            IEnumerable<WFTrackerTable> bookmarks = _DocumentService.GetCurrentSignStep(documentId, currentUser).ToList();
            _DocumentService.SaveSignData(bookmarks, TrackerType.Active, String.IsNullOrEmpty(currentUser) ? true : false, currentUser);
            if (bookmarks != null)
            {
                foreach (var bookmark in bookmarks)
                {
            
                    SqlWorkflowInstanceStore instanceStore = SetupInstanceStore();

                    WorkflowApplicationInstance instanceInfo =
                            WorkflowApplication.GetInstance(WWFInstanceId, instanceStore);
                    FileTable fileTableWF = GetRightFileWF(TableName, processId, instanceInfo);
                    Activity activity = ChooseActualWorkflow(TableName, fileTableWF, instanceInfo.DefinitionIdentity != null);
                    LoadAOrCompleteInstance(documentId, DocumentState.Agreement, TrackerType.Active, documentData, instanceStore, activity, instanceInfo, bookmark, currentUser);
                    DeleteInstanceStoreOwner(instanceStore);
                    _EmailService.SendExecutorEmail(documentId, documentData.ContainsKey("AdditionalText") ? (string)documentData["AdditionalText"] : "");
                }
            }
        }

        public void AgreementWorkflowWithdraw(Guid documentId, string tableName, Guid WWFInstanceId, Guid processId)
        {
            string currentUser = HttpContext.Current.User.Identity.GetUserId();
            DocumentTable documentTable = _DocumentService.Find(documentId);
            try
            {
                SqlWorkflowInstanceStore instanceStore = SetupInstanceStore();
                WorkflowApplicationInstance instanceInfo = WorkflowApplication.GetInstance(WWFInstanceId, instanceStore);
                 FileTable fileTableWF = GetRightFileWF(tableName, processId, instanceInfo);
                 Activity activity = ChooseActualWorkflow(tableName, fileTableWF, instanceInfo.DefinitionIdentity != null);
                 WithdrawInstance(documentId, DocumentState.Cancelled, TrackerType.Cancelled, instanceStore, activity, instanceInfo);
                 DeleteInstanceStoreOwner(instanceStore);
                 _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.Withdraw }, HttpContext.Current.User.Identity.GetUserId());
            }
            catch (InstancePersistenceCommandException)
            {
                if (documentTable.DocType == DocumentType.OfficeMemo || documentTable.DocType == DocumentType.Task || documentTable.DocType == DocumentType.OutcomingDoc)
                    MakingWithdrawDoc(documentTable, currentUser);
                
            }
        }
        private void MakingWithdrawDoc(DocumentTable documentTable, string currentUser)
        {
            documentTable.WWFInstanceId = Guid.Empty;
            documentTable.DocumentState = DocumentState.Created;
            documentTable.ActivityName = String.Empty;

            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    _DocumentService.UpdateDocument(documentTable, currentUser);
                    break;
                }
                catch
                {
                    retries = retries - 1;
                    if (retries <= 0) throw;
                    Thread.Sleep(1000);
                }
            }

            IEnumerable<WFTrackerTable> wftrackers = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentTable.Id).ToList();
            foreach (var item in wftrackers)
            {
                item.Users.Clear();
                _WorkflowTrackerService.SaveDomain(item, currentUser);
            }

            _WorkflowTrackerService.DeleteAll(documentTable.Id);
        }
        private SqlWorkflowInstanceStore SetupInstanceStore()
        {
            SqlWorkflowInstanceStore instanceStore =
                new SqlWorkflowInstanceStore(ConfigurationManager.ConnectionStrings["WFConnection"].ToString());
            InstanceView view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(40));
            instanceStore.DefaultInstanceOwner = view.InstanceOwner;

            return instanceStore;
        }
        private void DeleteInstanceStoreOwner(SqlWorkflowInstanceStore instanceStore)
        {
            InstanceView view = instanceStore.Execute(instanceStore.CreateInstanceHandle(instanceStore.DefaultInstanceOwner), new DeleteWorkflowOwnerCommand(), TimeSpan.FromSeconds(40));
        }

        public void StartAndPersistInstance(Guid _documentId, DocumentState _state, IDictionary<string, object> documentData, SqlWorkflowInstanceStore instanceStore, Activity activity, FileTable fileTableWF, string currentUserId = "")
        {
            AutoResetEvent instanceUnloaded = new AutoResetEvent(false);
            var documentTable = _DocumentService.Find(_documentId);
            IDictionary<string, object> inputArguments = new Dictionary<string, object>();
            currentUserId = getCurrentUserId(currentUserId);

            inputArguments.Add("inputStep" ,  _state);
            inputArguments.Add("inputDocumentId" ,  _documentId);
            inputArguments.Add("inputCurrentUser", currentUserId);
            inputArguments.Add("documentData", documentData);

            WorkflowIdentity workflowIdentity = new WorkflowIdentity
            {
                Name = fileTableWF.VersionName,
                Version = new Version(0, 0, 0, Convert.ToInt32(fileTableWF.Version)) 
            };
            WorkflowApplication application = new WorkflowApplication(activity, inputArguments, workflowIdentity);
            application.InstanceStore = instanceStore;
            application.Extensions.Add(new WFTrackingParticipant());

            #region Workflow Delegates

            application.PersistableIdle = (e) =>
            {
                var ex = e.GetInstanceExtensions<WFTrackingParticipant>();
                outputParameters = ex.First().Outputs;
                return PersistableIdleAction.Unload;

            };

            application.Completed = (e) =>
            {
                outputParameters = e.Outputs;
            };

            application.Unloaded = (e) =>
            {
                instanceUnloaded.Set();
            };

            application.OnUnhandledException = (e) =>
            {
                return UnhandledExceptionAction.Terminate;
            };

            #endregion Workflow Delegates

            //application.Persist();
            application.Run();
            instanceUnloaded.WaitOne();

            documentTable.WWFInstanceId = application.Id;
            documentTable.DocumentState = (DocumentState)outputParameters["outputStep"];
            _DocumentService.UpdateDocument(documentTable, currentUserId);
        }
        public void LoadAOrCompleteInstance(Guid _documentId, DocumentState _state, TrackerType _trackerType, IDictionary<string, object> documentData, SqlWorkflowInstanceStore instanceStore, Activity activity, WorkflowApplicationInstance instanceInfo, WFTrackerTable bookmark, string currentUser = "")
        {
            try
            {
                AutoResetEvent instanceUnloaded = new AutoResetEvent(false);
                string currentUserId = HttpContext.Current.User.Identity.GetUserId();

                IDictionary<string, object> inputArguments = new Dictionary<string, object>();
                inputArguments.Add("inputStep", _state);
                inputArguments.Add("inputCurrentUser", String.IsNullOrEmpty(currentUser) ? currentUserId : currentUser);
                inputArguments.Add("documentData", documentData);

                WorkflowApplication application = new WorkflowApplication(activity, instanceInfo.DefinitionIdentity);
                application.InstanceStore = instanceStore;
                application.Extensions.Add(new WFTrackingParticipant());

                #region Workflow Delegates

                application.PersistableIdle = (e) =>
                {
                    var ex = e.GetInstanceExtensions<WFTrackingParticipant>();
                    outputParameters = ex.Last().Outputs;
                    return PersistableIdleAction.Unload;
                };

                application.Completed = (e) =>
                {
                    outputParameters = e.Outputs;
                };

                application.Unloaded = (workflowApplicationEventArgs) =>
                {
                    instanceUnloaded.Set();

                };

                application.OnUnhandledException = (e) =>
                {
                    return UnhandledExceptionAction.Terminate;
                };

                #endregion Workflow Delegates

                application.Load(instanceInfo);

                //bookmarks = _DocumentService.GetCurrentSignStep(_documentId, String.IsNullOrEmpty(currentUser) ? currentUserId : currentUser).ToList();
                //_DocumentService.SaveSignData(bookmarks, _trackerType, String.IsNullOrEmpty(currentUser) ? true : false);
               
                application.ResumeBookmark(bookmark.ActivityName, inputArguments);

                //application.Persist();
                instanceUnloaded.WaitOne();             
                 
                DocumentTable documentTable = _DocumentService.Find(_documentId);
                documentTable.WWFInstanceId = application.Id;
                documentTable.DocumentState = (DocumentState)outputParameters["outputStep"];

                int retries = 3;
                while (retries > 0)
                {
                    try
                    {
                        _DocumentService.UpdateDocument(documentTable, String.IsNullOrEmpty(currentUser) ? currentUserId : currentUser);
                        break;
                    }
                    catch
                    {
                        retries = retries - 1;
                        if (retries <= 0) throw;
                        Thread.Sleep(1000);
                    }
                }

                _CustomCheckDocument.UpdateDocumentData(documentTable, documentData);

                if (documentTable.DocumentState == DocumentState.Closed)
                {
                    _EmailService.SendInitiatorClosedEmail(documentTable.Id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void WithdrawInstance(Guid _documentId, DocumentState _state, TrackerType _trackerType,  SqlWorkflowInstanceStore instanceStore, Activity activity, WorkflowApplicationInstance instanceInfo)
        {            
            try
            {
                DocumentTable documentTable = _DocumentService.Find(_documentId);
                AutoResetEvent instanceUnloaded = new AutoResetEvent(false);
                IEnumerable<WFTrackerTable> bookmarks;
                string currentUserId = HttpContext.Current.User.Identity.GetUserId();

                IDictionary<string, object> inputArguments = new Dictionary<string, object>();
                inputArguments.Add("inputStep", _state);
                inputArguments.Add("inputCurrentUser", currentUserId);
                inputArguments.Add("documentData", null);
                inputArguments.Add("isWithdraw", true);

                WorkflowApplication application = new WorkflowApplication(activity, instanceInfo.DefinitionIdentity);
                application.InstanceStore = instanceStore;
                application.Extensions.Add(new WFTrackingParticipant());

                #region Workflow Delegates

                application.PersistableIdle = (e) =>
                {
                    var ex = e.GetInstanceExtensions<WFTrackingParticipant>();
                    outputParameters = ex.Last().Outputs;
                    return PersistableIdleAction.Unload;
                };

                application.Completed = (e) =>
                {
                    outputParameters = e.Outputs;
                };

                application.Unloaded = (workflowApplicationEventArgs) =>
                {
                    instanceUnloaded.Set();

                };

                application.OnUnhandledException = (e) =>
                {
                    return UnhandledExceptionAction.Terminate;
                };

                #endregion Workflow Delegates

                application.Load(instanceInfo);

                bookmarks = _DocumentService.GetCurrentSignStep(_documentId, currentUserId).ToList();
                _DocumentService.SaveSignData(bookmarks, _trackerType);

                if (bookmarks != null)
                {
                    foreach (var bookmark in bookmarks)
                    {
                        application.ResumeBookmark(bookmark.ActivityName, inputArguments);

                        //application.Persist();
                        instanceUnloaded.WaitOne();
                    }
                }

                MakingWithdrawDoc(documentTable, currentUserId);            
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FileTable GetActualFileWF(string _tableName, DocumentTable documentTable)
        {
            FileTable fileWF = _DocumentService.GetAllXAMLDocument(documentTable.ProcessTableId).OrderByDescending(x => Convert.ToInt32(x.Version)).FirstOrDefault();
            return fileWF;
        }
        public Activity ChooseActualWorkflow(string _tableName, FileTable fileWF, bool flag = true)
        {
            if (flag == true)
            {
                using (System.IO.Stream stream = new System.IO.MemoryStream(fileWF.Data))
                {
                    using (var xamlReader = new System.Xaml.XamlXmlReader(stream, new System.Xaml.XamlXmlReaderSettings { LocalAssembly = System.Reflection.Assembly.GetExecutingAssembly() }))
                    {
                        Activity activity = System.Activities.XamlIntegration.ActivityXamlServices.Load(xamlReader, 
                            new System.Activities.XamlIntegration.ActivityXamlServicesSettings { CompileExpressions = true }) as DynamicActivity;
                        return activity;
                    }
                }
            }
            else
            { 
                Type type = Type.GetType("RapidDoc.Activities." + _tableName);
                if (type != null)
                    return Activator.CreateInstance(type) as Activity;

                return null;
            }
        }

        public FileTable GetRightFileWF(string _tableName, Guid processId, WorkflowApplicationInstance instanceInfo)
        {
            FileTable fileWF;

            if (instanceInfo != null && instanceInfo.DefinitionIdentity != null)
            {
                Int32 revision = instanceInfo.DefinitionIdentity.Version.Revision;
                return fileWF = _DocumentService.GetAllXAMLDocument(processId).Where(x => x.Version == revision.ToString()).FirstOrDefault();
            }

            return null;
        }

        public void CreateTrackerRecord(string systemName, DocumentState step, Guid documentId, string bookmarkName, List<WFTrackerUsersTable> listUser, string currentUserId, string activityId, bool useManual, int slaOffset, bool executionStep)
        {
            if ((step != DocumentState.Cancelled) &&
                (step != DocumentState.Closed))
            {
                WFTrackerTable trackerTable = _WorkflowTrackerService.FirstOrDefault(x => x.ActivityID == activityId && x.DocumentTableId == documentId);
                trackerTable.SystemName = systemName;
                trackerTable.ActivityName = bookmarkName;
                if (trackerTable.Users == null || trackerTable.Users.Count() == 0)
                    trackerTable.Users = listUser;
                trackerTable.TrackerType = TrackerType.Waiting;
                trackerTable.ManualExecutor = useManual;
                trackerTable.SLAOffset = slaOffset;
                trackerTable.StartDateSLA = DateTime.UtcNow;
                trackerTable.ExecutionStep = executionStep;
                trackerTable.SignDate = null;
                trackerTable.SignUserId = null;
                _WorkflowTrackerService.SaveDomain(trackerTable, currentUserId);

                foreach(var user in listUser)
                {
                    try
                    {
                        _ReviewDocLogService.Delete(documentId, user.UserId);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (trackerTable.ParallelID == String.Empty)
                {
                    IEnumerable<WFTrackerTable> trackerTableCancel = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == trackerTable.DocumentTableId && x.LineNum > trackerTable.LineNum && x.ActivityID != trackerTable.ActivityID && x.TrackerType != TrackerType.NonActive).ToList();
                    foreach (var item in trackerTableCancel)
                    {
                        item.TrackerType = TrackerType.NonActive;
                        item.SignDate = null;
                        item.SignUserId = null;
                        if (item.Users != null)
                            item.Users.Clear();
                        item.SLAOffset = 0;
                        item.StartDateSLA = null;
                        item.ManualExecutor = false;
                        _WorkflowTrackerService.SaveDomain(item, currentUserId);
                    }
                }
            }
        }

        public List<Array> GetRequestTree(Activity activity, IDictionary<string, object> _documentData, string _parallel = "")
        {
            string[] myIntArray = new string[4];
            List<Array> allSteps = new List<Array>();

            if (activity.GetType() == typeof(WFChooseUpManager) ||
                activity.GetType() == typeof(WFChooseStaffStructure) ||
                activity.GetType() == typeof(WFChooseSpecificUserFromService) ||
                activity.GetType() == typeof(WFChooseSpecificUser) ||
                activity.GetType() == typeof(WFChooseRoleUser) ||
                activity.GetType() == typeof(WFChooseManualExecution) ||
                activity.GetType() == typeof(WFChooseDocUsers) ||
                activity.GetType() == typeof(WFChooseCreatedUser))
            {
                myIntArray.SetValue(activity.DisplayName, 0);
                myIntArray.SetValue(activity.Id, 1);
                myIntArray.SetValue(_parallel, 2);
                allSteps.Add(myIntArray);
            }

            if (activity.GetType() == typeof(WFSetUsersForTaskProlongation))
            {
                Guid refDocId = (Guid)_documentData["RefDocumentId"];
                DocumentTable documentTable = _DocumentService.Find(refDocId);
                int i = 0;
                List<List<WFTrackerUsersTable>> endListUsers = new List<List<WFTrackerUsersTable>>();
                string currentUser = HttpContext.Current.User.Identity.GetUserId();
                WFTrackerTable trackerTableUser = _DocumentService.FirstOrDefaultTrackerItem(documentTable.ProcessTable, refDocId, currentUser);

                List<WFTrackerTable> trackerTalbeUserList = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == refDocId && x.LineNum < trackerTableUser.LineNum).OrderByDescending( x => x.LineNum).ToList();
                trackerTalbeUserList.ForEach(x => {
                    List<WFTrackerUsersTable> users = new List<WFTrackerUsersTable>();
                    x.Users.ForEach(z => users.Add(new WFTrackerUsersTable { UserId = z.UserId }));
                    endListUsers.Add(users);
                    allSteps.Add(new string[3] { "Исполнитель", (++i).ToString(), "" });
                });

                bool protocol = false;
                bool task = false;
                if (documentTable.RefDocumentId != null)
                {                
                    var taskTable = (USR_TAS_DailyTasks_Table)_DocumentService.GetDocument(documentTable.RefDocumentId, documentTable.ProcessTable.TableName);
                    var refTaskDocument = _DocumentService.Find(taskTable.RefDocumentId);

                    if (refTaskDocument != null && refTaskDocument.DocType == DocumentType.Task)
                    {
                        task = true;
                        List<List<WFTrackerUsersTable>> listUsersId = new List<List<WFTrackerUsersTable>>();
                        string process = "USR_TAS_DailyTasks";
                        listUsersId = _DocumentService.GetUsersUpProlongatedTask(refTaskDocument.Id, process, documentTable.CreatedDate);
                        if (listUsersId.Count() > 0)
                        {
                            endListUsers.AddRange(listUsersId);

                            for (int y = 1; y <= listUsersId.Count(); y++)
			                {
                                allSteps.Add(new string[3] { "Исполнитель", (++i).ToString(), ""});
			                }                         
                        }
                    }

                    if (refTaskDocument != null && refTaskDocument.DocType == DocumentType.Protocol)
                    {
                        protocol = true;
                        var docuTable = (IBasicProtocol)_DocumentService.GetDocument(refTaskDocument.RefDocumentId, refTaskDocument.ProcessTable.TableName);
                        if (!String.IsNullOrEmpty(docuTable.Chairman))
                        {
                            Guid chairmanEmplId = Guid.Parse(_SystemService.GuidsFromText(docuTable.Chairman)[0]);
                            EmplTable chairmanEmpl = _EmplService.Find(chairmanEmplId);
                            if (chairmanEmpl != null)
                            {
                                string chairmanId = chairmanEmpl.ApplicationUserId;
                                endListUsers.Add(new List<WFTrackerUsersTable> { new WFTrackerUsersTable { UserId = chairmanId } });
                                allSteps.Add(new string[3] { "Председатель", (++i).ToString(), "" });
                            }
                        }
                    }
                }

                if (protocol == false && task == false)
                {
                    endListUsers.Add(new List<WFTrackerUsersTable> { new WFTrackerUsersTable { UserId = documentTable.ApplicationUserCreatedId } });
                    allSteps.Add(new string[3] { "Инициатор", (++i).ToString(), "" });
                }

                _documentData.Add("endListUsers", endListUsers);
            }

            if (activity.GetType() == typeof(WFSetUsersForOrder) || activity.GetType() == typeof(WFSetUsersForProtocol) || activity.GetType() == typeof(WFSetUsersForOND))
            {
                bool parallel = false;
                List<string> userList = (List<string>)_documentData["ListAgreement"];
                dynamic particularActivity;

                if (activity.GetType() == typeof(WFSetUsersForProtocol))               
                    particularActivity = activity as WFSetUsersForProtocol;
                else if (activity.GetType() == typeof(WFSetUsersForOrder))
                    particularActivity = activity as WFSetUsersForOrder;
                else
                {
                    particularActivity = activity as WFSetUsersForOND;
                    parallel = (bool)_documentData["Parallel"];
                }
                
                var particularActivityExpression = particularActivity.inputSystemName.Expression as System.Activities.Expressions.Literal<string>;
                foreach(string userId in userList)
                {
                    if (_documentData.ContainsKey("Parallel") && ((parallel == true && String.IsNullOrEmpty(_parallel)) || (parallel == false && !String.IsNullOrEmpty(_parallel))))
                        break;

                    EmplTable empl = _EmplService.FirstOrDefault(x => x.ApplicationUserId == userId);
                    string activityName = String.Empty;
                    if(empl != null && empl.TitleTable != null)
                    {
                        activityName = empl.TitleTable.TitleName;
                    }
                    else
                    {
                        ApplicationUser user = repoUser.GetById(userId);
                        activityName = user.UserName;
                    }
                    myIntArray.SetValue(activityName, 0);
                    myIntArray.SetValue(activity.Id + userId, 1);
                    myIntArray.SetValue(_parallel, 2);
                    if (particularActivityExpression != null)
                        myIntArray.SetValue(particularActivityExpression.Value, 3);
                    allSteps.Add(myIntArray);
                    myIntArray = new string[4];
                }
            }

            if ((activity is System.Activities.Statements.Parallel) || (activity is ParallelForEach<string>))
                _parallel = activity.Id;

            IEnumerator<Activity> list = WorkflowInspectionServices.GetActivities(activity).Where(x => x.GetType() != typeof(EnvironmentLocationReference<List<String>>)).GetEnumerator();

            while (list.MoveNext())
            {
                var allStepsBuf = allSteps.Concat(GetRequestTree(list.Current, _documentData, _parallel));
                allSteps = allStepsBuf.ToList();
            }
            if ((activity is System.Activities.Statements.Parallel) && (activity.Id == _parallel))
                _parallel = "";
            return allSteps;
        }
      

        public List<Array> GetTrackerList(Guid documentId, Activity activity, IDictionary<string, object> documentData, DocumentType documentType)
        {
            List<Array> allSteps = new List<Array>();
           
            
            switch (documentType)
            {
                case DocumentType.Request:
                    allSteps = this.GetRequestTree(activity, documentData);
                    break;
                case DocumentType.OfficeMemo:
                    List<string> users = this.GetUniqueUserList(documentId, documentData, "DocumentWhom");
                    allSteps = this.GetRequestTree(activity, documentData);
                    documentData["DocumentWhom"] = users;
                    break;
                case DocumentType.Task:
                    allSteps = this.GetRequestTree(activity, documentData);
                    break;
                case DocumentType.OutcomingDoc:
                case DocumentType.Order:
                    List<string> usersOrder = this.GetUniqueUserList(documentId, documentData, "ListAgreement");
                    documentData["ListAgreement"] = usersOrder;
                    allSteps = this.GetRequestTree(activity, documentData);
                    break;
                case DocumentType.Protocol:
                    List<string> usersProtocol = this.GetUniqueUserList(documentId, documentData, "ListAgreement");
                    List<string> chairmanProtocol = this.GetUniqueUserList(documentId, documentData, "Chairman");
                    documentData["ListAgreement"] = usersProtocol.Concat(chairmanProtocol).Distinct().ToList();
                    allSteps = this.GetRequestTree(activity, documentData);
                    break;
                default:
                    allSteps = this.GetRequestTree(activity, documentData);
                    break;
            }
            
            return allSteps;
        }


        public List<string> GetUniqueUserList(Guid documentId, IDictionary<string, object> documentData, string nameField, bool getAll = false)
        {
            List<string> ofmList = new List<string>();
            string initailStructure = (string)documentData[nameField];
            string[] arrayTempStructrue = initailStructure.Split(',');

            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] arrayStructure = arrayTempStructrue.Where(a => isGuid.IsMatch(a) == true).ToArray();
            DocumentTable documentTable = _DocumentService.Find(documentId);
            var trackerListCheck = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled || x.TrackerType == TrackerType.Waiting)).ToList();

            foreach (string emplIdStr in arrayStructure)
            {
                Guid emplId = Guid.Parse(emplIdStr);
                var emplTable = _EmplService.FirstOrDefault(x => x.Id == emplId && x.Enable == true);

                if (emplTable != null && (documentTable == null || documentTable.ApplicationUserCreatedId != emplTable.ApplicationUserId) && !ofmList.Exists(x => x == emplTable.ApplicationUserId))
                {
                    if ((trackerListCheck.Any(x => (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled) && x.SignUserId == emplTable.ApplicationUserId)
                        || trackerListCheck.Any(x => x.Users.Any(p => p.UserId == emplTable.ApplicationUserId) && x.TrackerType == TrackerType.Waiting)) && getAll == false)
                        continue;
                    else
                        ofmList.Add(emplTable.ApplicationUserId);
                }
                else
                {
                    RoleManager<ApplicationRole> RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
                    ApplicationRole role = RoleManager.FindById(emplIdStr);
                    if (role != null)
                    {
                        if (role.Users != null && role.Users.Count() > 0)
                        {
                            var empllist = _EmplService.GetPartialIntercompany(x => x.Enable == true).ToList();

                            foreach (IdentityUserRole userRole in role.Users)
                            {
                                if ((documentTable == null || documentTable.ApplicationUserCreatedId != userRole.UserId)
                                    && !ofmList.Exists(x => x == userRole.UserId) && empllist.Any(x => x.ApplicationUserId == userRole.UserId))
                                {
                                    if ((trackerListCheck.Any(x => (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled) && x.SignUserId == userRole.UserId)
                                        || trackerListCheck.Any(x => x.Users.Any(p => p.UserId == userRole.UserId) && x.TrackerType == TrackerType.Waiting)) && getAll == false)
                                        continue;
                                    else
                                        ofmList.Add(userRole.UserId);
                                }
                            }
                        }
                    }
                }
            }

            return ofmList;
        }

        public List<string> EmplAndRolesToUserList(Guid documentId, string[] list)
        {
            List<string> ofmList = new List<string>();

            foreach (string emplIdStr in list)
            {
                Guid emplId = Guid.Parse(emplIdStr);
                var emplTable = _EmplService.FirstOrDefault(x => x.Id == emplId && x.Enable == true);

                if (emplTable != null && !ofmList.Exists(x => x == emplTable.ApplicationUserId) && repoUser.Any(x => x.Id == emplTable.ApplicationUserId && x.Enable == true))
                {
                    if (!_WorkflowTrackerService.Contains(x => x.DocumentTableId == documentId && x.SignUserId == emplTable.ApplicationUserId))
                        ofmList.Add(emplTable.ApplicationUserId);
                }
                else
                {
                    RoleManager<ApplicationRole> RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
                    ApplicationRole role = RoleManager.FindById(emplIdStr);
                    if (role != null && role.Users != null && role.Users.Count() > 0)
                    {
                        var empllist = _EmplService.GetPartialIntercompany(x => x.Enable == true).ToList();

                        foreach (IdentityUserRole userRole in role.Users)
                        {
                            if (!ofmList.Exists(x => x == userRole.UserId) && empllist.Any(x => x.ApplicationUserId == userRole.UserId) && repoUser.Any(x => x.Id == userRole.UserId && x.Enable == true))
                            {
                                if (!_WorkflowTrackerService.Contains(x => x.DocumentTableId == documentId && x.SignUserId == userRole.UserId))
                                    ofmList.Add(userRole.UserId);
                            }
                        }
                    }
                }
            }

            return ofmList;
        }

        public List<string> EmplAndRolesToReaders(Guid documentId, string[] list)
        {
            List<string> ofmList = new List<string>();

            foreach (string emplIdStr in list)
            {
                Guid emplId = Guid.Parse(emplIdStr);
                var emplTable = _EmplService.FirstOrDefault(x => x.Id == emplId && x.Enable == true);

                if (emplTable != null && !ofmList.Exists(x => x == emplTable.ApplicationUserId))
                {
                    if (!_WorkflowTrackerService.Contains(x => x.DocumentTableId == documentId && x.SignUserId == emplTable.ApplicationUserId))
                        ofmList.Add(emplTable.ApplicationUserId);
                }
                else
                {
                    ofmList.Add(emplIdStr);
                }
            }

            return ofmList;
        }


        public void CreateDynamicTracker(List<string> users, Guid documentId, string currentUserId, bool parallel, string additionalText = "")
        {
            List<string> result = new List<string>();
            List<string> reminderList = new List<string>();
            if (users == null && users.Count == 0)
                return;

            var trackerListCheck = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled || x.TrackerType == TrackerType.Waiting)).ToList();

            foreach(var userId in users)
            {
                if (trackerListCheck.Any(x => (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled) && x.SignUserId == userId)
                        || trackerListCheck.Any(x => x.Users.Any(p => p.UserId == userId) && x.TrackerType == TrackerType.Waiting))
                    continue;
                else
                    result.Add(userId);
            }

            if (result.Count == 0)
                return;

            var documentTable = _DocumentService.Find(documentId);
            var employers = _EmplService.GetPartialIntercompany(x => x.Enable == true).ToList();
            DateTime createdDate = DateTime.UtcNow;
            string activityId = Guid.NewGuid().ToString();

            using (var bcp = new System.Data.SqlClient.SqlBulkCopy(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                bcp.BatchSize = result.Count;
                bcp.DestinationTableName = "[dbo].[WFTrackerTable]";
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(Guid));
                table.Columns.Add("LineNum", typeof(int));
                table.Columns.Add("DocumentTableId", typeof(Guid));
                table.Columns.Add("ActivityName", typeof(string));
                table.Columns.Add("ActivityID", typeof(string));
                table.Columns.Add("ParallelID", typeof(string));
                table.Columns.Add("SignUserId", typeof(string));
                table.Columns.Add("SignDate", typeof(DateTime));
                table.Columns.Add("TrackerType", typeof(RapidDoc.Models.Repository.TrackerType));
                table.Columns.Add("ManualExecutor", typeof(Boolean));
                table.Columns.Add("SLAOffset", typeof(int));
                table.Columns.Add("ExecutionStep", typeof(Boolean));
                table.Columns.Add("TimeStamp", typeof(Byte[]));
                table.Columns.Add("CreatedDate", typeof(DateTime));
                table.Columns.Add("ModifiedDate", typeof(DateTime));
                table.Columns.Add("ApplicationUserCreatedId", typeof(string));
                table.Columns.Add("ApplicationUserModifiedId", typeof(string));
                table.Columns.Add("StartDateSLA", typeof(DateTime));
                table.Columns.Add("Comments", typeof(string));
                table.Columns.Add("AdditionalText", typeof(string));
                table.Columns.Add("SystemName", typeof(string));

                int num = 0;
                foreach (string item in result)
                {
                    var emplTable = employers.Find(x => x.ApplicationUserId == item);

                    num++;
                    DataRow row = table.NewRow();
                    row["Id"] = Guid.NewGuid();
                    row["LineNum"] = DBNull.Value;
                    row["DocumentTableId"] = documentId;
                    if (emplTable != null)
                        row["ActivityName"] = emplTable.TitleName;
                    else
                        row["ActivityName"] = String.Empty;
                    row["ActivityID"] = activityId;
                    if(parallel == false)
                        row["ParallelID"] = String.Empty;
                    else
                        row["ParallelID"] = "Parallel";
                    row["SignUserId"] = DBNull.Value;
                    row["SignDate"] = DBNull.Value;

                    if (num == 1 || parallel == true)
                    {
                        row["TrackerType"] = TrackerType.Waiting;
                        reminderList.Add(item);
                    }
                    else
                        row["TrackerType"] = TrackerType.NonActive;

                    row["ManualExecutor"] = 0;
                    row["SLAOffset"] = 0;
                    row["ExecutionStep"] = 0;
                    row["TimeStamp"] = DBNull.Value;
                    row["CreatedDate"] = createdDate;
                    row["ModifiedDate"] = createdDate;
                    row["ApplicationUserCreatedId"] = currentUserId;
                    row["ApplicationUserModifiedId"] = currentUserId;

                    if (num == 1 || parallel == true)
                        row["StartDateSLA"] = createdDate;
                    else
                        row["StartDateSLA"] = DBNull.Value;

                    row["Comments"] = DBNull.Value;
                    row["AdditionalText"] = additionalText;
                    row["SystemName"] = DBNull.Value;

                    table.Rows.Add(row);
                }

                bcp.WriteToServer(table);
                _uow.Commit();
            }

            using (var bcp = new System.Data.SqlClient.SqlBulkCopy(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                var trackerlist = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && x.ActivityID == activityId).OrderBy(x => x.LineNum).ToList();

                bcp.BatchSize = trackerlist.Count;
                bcp.DestinationTableName = "[dbo].[WFTrackerUsersTable]";
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(Guid));
                table.Columns.Add("TimeStamp", typeof(Byte[]));
                table.Columns.Add("InitiatorUserId", typeof(string));
                table.Columns.Add("UserId", typeof(string));
                table.Columns.Add("WFTrackerTable_Id", typeof(Guid));

                int num = 0;
                foreach (var item in trackerlist)
                {
                    DataRow row = table.NewRow();
                    row["Id"] = Guid.NewGuid();
                    row["TimeStamp"] = DBNull.Value;
                    row["InitiatorUserId"] = DBNull.Value;
                    row["UserId"] = result[num];
                    row["WFTrackerTable_Id"] = item.Id;
                    table.Rows.Add(row);
                    num++;
                }

                bcp.WriteToServer(table);
                _uow.Commit();
            }

            _EmailService.SendNewExecutorEmail(documentId, reminderList, additionalText);
        }

        private string getCurrentUserId(string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                return currentUserId;
            }
            else
            {
                return HttpContext.Current.User.Identity.GetUserId();
            }
        }

        public void CreateSeparateTasks(ProcessView processView, dynamic docModel, Guid fileId, String actionModelName, IDictionary<string, object> documentData, ApplicationUser userTable)
        {
            var documentId = _DocumentService.SaveDocument(docModel, processView.TableName, GuidNull2Guid(processView.Id), fileId, userTable, documentData.ContainsKey("IsNotified") ? (bool)documentData["IsNotified"] : false, documentData.ContainsKey("Share") ? (bool)documentData["Share"] : false);
            DocumentTable documentTable = _DocumentService.Find(documentId);
            if ((documentData.ContainsKey("AdditionalText") && documentData["AdditionalText"] != null && documentData["AdditionalText"] != String.Empty))
                _CommentService.Save(new CommentTable { Comment = (string)documentData["AdditionalText"], DocumentTableId = documentId });
            Task.Run(() =>
            {
                IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                IHistoryUserService _HistoryUserServiceTask = DependencyResolver.Current.GetService<IHistoryUserService>();
                _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = documentId }, "", userTable);
                _HistoryUserServiceTask.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, userTable.Id);
            });

            _SearchService.SaveSearchData(documentId, docModel, actionModelName, userTable.Id);

            RunWorkflow(documentTable, processView.TableName, documentData, userTable.Id);

        }

        public Guid GuidNull2Guid(Guid? value)
        {
            return value ?? Guid.Empty;
        }   
    }
}