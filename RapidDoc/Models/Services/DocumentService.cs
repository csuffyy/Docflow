using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.CSharp.RuntimeBinder;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Interfaces;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System.Text.RegularExpressions;
using System.Data.Entity.Core.Objects;


namespace RapidDoc.Models.Services
{
    public interface IDocumentService
    {
        Guid SaveDocument(dynamic viewTable, string tableName, Guid processId, Guid fileId, ApplicationUser user, bool isNotify = false, bool share = false);
        IEnumerable<DocumentTable> GetAll();
        IQueryable<DocumentView> GetAllView();
        IQueryable<DocumentView> GetArchiveView();
        IQueryable<DocumentView> GetMyDocumentView();
        IQueryable<DocumentView> GetMyFavoriteView();
        IQueryable<DocumentTaskView> GetTaskDocumentView();
        IEnumerable<DocumentTable> GetPartial(Expression<Func<DocumentTable, bool>> predicate);
        DocumentTable FirstOrDefault(Expression<Func<DocumentTable, bool>> predicate);
        DocumentView FirstOrDefaultView(Expression<Func<DocumentTable, bool>> predicate);
        bool Contains(Expression<Func<DocumentTable, bool>> predicate);
        IQueryable<DocumentView> GetAgreedDocument();
        DocumentTable Find(Guid? id);
        DocumentView FindView(Guid? id);
        DocumentView Document2View(DocumentTable documentTable);
        dynamic GetDocument(Guid refDocumentId, string tableName);
        dynamic GetDocumentView(Guid refDocumentId, string tableName);
        IEnumerable<dynamic> GetDocumentAll(string tableName);
        dynamic RouteCustomModelView(string customModel);
        dynamic RouteCustomModelDomain(string customModel);
        dynamic RouteCustomRepository(string customModel);
        void UpdateDocument(DocumentTable domainTable, string currentUserId = "");
        bool UpdateDocumentFields(dynamic viewTable, ProcessView processView);
        void SaveDocumentText(DocumentTable domainTable);
        bool isShowDocument(DocumentTable documentTable, ApplicationUser user, bool isAfterView = false);
        bool isSignDocument(Guid documentId, ApplicationUser user = null);
        IEnumerable<WFTrackerTable> GetCurrentSignStep(Guid documentId, string currentUserId = "", ApplicationUser user = null);
        SLAStatusList SLAStatus(Guid documentId, string currentUserId = "", ApplicationUser user = null);
        void SaveSignData(IEnumerable<WFTrackerTable> trackerTables, TrackerType trackerType, bool changeSignUser = true, string currentUserId = "");
        Guid SaveFile(FileTable file, string userId);
        void UpdateFile(FileTable file);
        bool FileContains(Guid documentFileId);
        FileTable GetFile(Guid Id);
        FileTable FirstOrDefaultFile(Expression<Func<FileTable, bool>> predicate);
        bool FileReplaceContains(Guid id);
        IEnumerable<FileTable> GetAllFilesDocument(Guid documentFileId);
        string DeleteFile(Guid Id);
        List<ApplicationUser> GetSignUsers(DocumentTable docuTable);
        List<ApplicationUser> GetSignUsersDirect(DocumentTable docuTable);
        List<WFTrackerUsersTable> GetUsersSLAStatus(DocumentTable docuTable, SLAStatusList status);
        DateTime? GetSLAPerformDate(Guid DocumentId, DateTime? CreatedDate, double SLAOffset);
        IEnumerable<FileTable> GetAllTemplatesDocument(Guid processId);
        IEnumerable<FileTable> GetAllXAMLDocument(Guid processId);
        void DeleteFiles(Guid documentId);
        void DeleteDocumentDraft(Guid documentId, string tableName, Guid refDocumentId);
        void Delete(Guid Id);
        List<string> SignDocumentCZ(Guid documentId, TrackerType trackerType, string comment = "");
        void SignTaskDocument(Guid documentId, TrackerType trackerType, string userId = "");
        List<TaskDelegationView> GetDocumentRefTask(Guid documentId);
        string[] GetUserListFromStructure(string users);
        Guid? UpdateProlongationDate(Guid refDocumentid, DateTime prolongationDate, string currentUserId);
        void ORDRegistration(Guid refDocumentid, string currentUserId, Guid? bookingNumberId);
        void INDRegistration(Guid refDocumentid, string currentUserId, Guid? bookingNumberId);
        void OUTRegistration(Guid refDocumentid, string currentUserId, Guid? bookingNumberId);
        void APPRegistration(Guid refDocumentid, string currentUserId);
        SelectList RevocationORDList(Guid? id, bool edit);
        SelectList AdditionORDList(Guid? id, bool edit);
        SelectList AdditionORDKZHList(Guid? id, bool edit);
        SelectList RevocationORDKZHList(Guid? id, bool edit);
        SelectList IncomingDocList<T>(Guid? id) where T : BasicIncomingDocumentsTable;
        SelectList OutcomingDocList<T>(Guid? id) where T : BasicOutcomingDocumentsTable;
        List<IncomingDublicateView> CheckIncomeDublicateDocument(Guid OrganizationId, string OutgoingNumber, DateTime OutgoingDate);
        Type GetTableType(string TableName);
        string ScrubHtml(string value);
        double GetSLAHours(Guid documentId, DateTime? startDate, DateTime? endDate);
        Guid DuplicateFile(FileTable fileTable, string userId, Guid? docFileId = null);
        WFTrackerTable FirstOrDefaultTrackerItem(ProcessTable process, Guid documentId, string userId);
        DocumentComposite CreateViewBodyTaskFromDocument(DocumentTable docTable, ProcessView process, ApplicationUser userTable);
        void CloseUpRelatedTasks(Guid documentId, ApplicationUser userTable);
        void CloseDownRelatedTasks(Guid documentId, ApplicationUser userTable, TrackerType trackerType);
        dynamic CloseTask(DocumentTable docTable, ApplicationUser userTable, string mainCloseText, string closeUserId, TrackerType trackerType);
        List<List<WFTrackerUsersTable>> GetUsersUpProlongatedTask(Guid documentId, string process);
        Guid FindRootTaskDocument(Guid documentId, string processTableName);
        void ApplyProlongateDate(Guid documentId, DateTime prolongationDate, string currentUserId, ProcessView process);
        void ProlongateDate(Guid documentId, DateTime prolongationDate, string currentUserId, ProcessView process);
        
    }

    public class DocumentService : IDocumentService
    {
        private IRepository<ProcessTable> repoProcess;
        private IRepository<DocumentTable> repoDocument;
        private IRepository<FileTable> repoFile;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork _uow;
        private readonly INumberSeqService _NumberSeqService;
        private readonly IProcessService _ProcessService;
        private readonly IGroupProcessService _GroupProcessService;
        private readonly IWorkflowTrackerService _WorkflowTrackerService;
        private readonly IDelegationService _DelegationService;
        private readonly IDocumentReaderService _DocumentReaderService;
        private readonly IWorkScheduleService _WorkScheduleService;
        private readonly IReviewDocLogService _ReviewDocLogService;
        private readonly IEmplService _EmplService;
        private readonly IModificationUsersService _ModificationUsersService;
        private readonly ISystemService _SystemService;
       

        protected UserManager<ApplicationUser> UserManager { get; private set; }
        protected RoleManager<ApplicationRole> RoleManager { get; private set; }

        public DocumentService(IUnitOfWork uow, INumberSeqService numberSeqService, IProcessService processService, 
            IWorkflowTrackerService workflowTrackerService,
            IDelegationService delegationService, IDocumentReaderService documentReaderService, IWorkScheduleService workScheduleService,
            IReviewDocLogService reviewDocLogService, IEmplService emplService, IModificationUsersService modificationUsersService, IGroupProcessService groupProcessService, ISystemService systemService)
        {
            _uow = uow;
            repoProcess = uow.GetRepository<ProcessTable>();
            repoDocument = uow.GetRepository<DocumentTable>();
            repoFile = uow.GetRepository<FileTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            _NumberSeqService = numberSeqService;
            _ProcessService = processService;
            _WorkflowTrackerService = workflowTrackerService;
            _DelegationService = delegationService;
            _DocumentReaderService = documentReaderService;
            _WorkScheduleService = workScheduleService;
            _ReviewDocLogService = reviewDocLogService;
            _EmplService = emplService;
            _ModificationUsersService = modificationUsersService;
            _GroupProcessService = groupProcessService;
            _SystemService = systemService;
            
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_uow.GetDbContext<ApplicationDbContext>()));
            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
        }

        public Guid SaveDocument(dynamic viewTable, string tableName, Guid processId, Guid fileId, ApplicationUser user, bool isNotify = false, bool share = false)
        {
            var docuTable = new DocumentTable();
            docuTable.ProcessTableId = processId;
            docuTable.CreatedDate = DateTime.UtcNow;
            docuTable.ModifiedDate = docuTable.CreatedDate;
            docuTable.DocumentState = DocumentState.Created;
            docuTable.FileId = fileId;
            docuTable.CompanyTableId = user.CompanyTableId;
            docuTable.ApplicationUserCreatedId = user.Id;
            docuTable.ApplicationUserModifiedId = user.Id;
            docuTable.DocType = _ProcessService.Find(processId, user.Id).DocType;
            docuTable.IsNotified = isNotify;
            docuTable.Share = share;

            Guid numberSeqId = _ProcessService.Find(processId, user.Id).GroupProcessTable.NumberSeriesTableId ?? Guid.Empty;
            docuTable.DocumentNum = _NumberSeqService.GetDocumentNum(numberSeqId, user.Id);

            while(_uow.GetRepository<DocumentTable>().Contains(x => x.DocumentNum == docuTable.DocumentNum))
            {
                docuTable.DocumentNum = _NumberSeqService.GetDocumentNum(numberSeqId, user.Id);
            }

            _uow.GetRepository<DocumentTable>().Add(docuTable);
            _uow.Commit();

            var domainTable = RouteCustomModelDomain(tableName);

            Type typeDomain = Type.GetType("RapidDoc.Models.DomainModels." + tableName + "_Table");
            Type typeDomainView = Type.GetType("RapidDoc.Models.ViewModels." + tableName + "_View");
            Mapper.CreateMap(typeDomainView, typeDomain)
                            .ForMember("ApplicationUserCreatedId", opt => opt.Ignore())
                            .ForMember("ApplicationUserModifiedId", opt => opt.Ignore())
                            .ForMember("CreatedDate", opt => opt.Ignore())
                            .ForMember("ModifiedDate", opt => opt.Ignore());
            Mapper.Map(viewTable, domainTable, typeDomainView, typeDomain);
            domainTable.DocumentTableId = docuTable.Id;
            domainTable.CreatedDate = DateTime.UtcNow;
            domainTable.ModifiedDate = domainTable.CreatedDate;
            RouteCustomRepository(tableName).Add(domainTable);
            _uow.Commit();

            docuTable.RefDocumentId = domainTable.Id;
            _uow.GetRepository<DocumentTable>().Update(docuTable);
            _uow.Commit();

            return docuTable.Id;
        }

        public bool UpdateDocumentFields(dynamic viewTable, ProcessView process)
        {
            try
            {
                if (viewTable.Id != null)
                {
                    var domainTable = RouteCustomRepository(process.TableName).GetById(viewTable.Id);

                    if (domainTable != null)
                    {
                        Type typeDomain = Type.GetType("RapidDoc.Models.DomainModels." + process.TableName + "_Table");
                        Type typeDomainView = Type.GetType("RapidDoc.Models.ViewModels." + process.TableName + "_View");
                        Mapper.CreateMap(typeDomainView, typeDomain)
                            .ForMember("ApplicationUserCreatedId", opt => opt.Ignore())
                            .ForMember("ApplicationUserModifiedId", opt => opt.Ignore())
                            .ForMember("CreatedDate", opt => opt.Ignore())
                            .ForMember("ModifiedDate", opt => opt.Ignore());
                        Mapper.Map(viewTable, domainTable, typeDomainView, typeDomain);

                        domainTable.ModifiedDate = DateTime.UtcNow;
                        RouteCustomRepository(process.TableName).Update(domainTable);
                        _uow.Commit();

                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public IEnumerable<DocumentTable> GetAll()
        {
            return repoDocument.All();
        }

        public IQueryable<DocumentView> GetAllView()
        {
            ApplicationUser user = getCurrentUserId();
            DateTime currentDate = DateTime.UtcNow;
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            if (UserManager.IsInRole(user.Id, "Administrator"))
            {
                var items = from document in contextQuery.DocumentTable
                        where !(contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isArchive == true))
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                        orderby document.ModifiedDate descending
                        select new DocumentView
                        {
                            ActivityName = document.ActivityName,
                            ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                            ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                            CompanyTableId = document.CompanyTableId,
                            CreatedDate = document.CreatedDate,
                            DocumentNum = document.DocumentNum,
                            DocumentState = document.DocumentState,
                            DocumentText = document.DocumentText,
                            FileId = document.FileId,
                            Id = document.Id,
                            ModifiedDate = document.ModifiedDate,
                            ProcessTableId = document.ProcessTableId,
                            AliasCompanyName = company.AliasCompanyName,
                            ProcessName = process.ProcessName,
                            CreatedBy = empl.SecondName + " " + empl.FirstName
                        };

                return items.AsQueryable();
            }
            else
            {
                var delegations = (from delegation in contextQuery.DelegationTable
                                   join emplTo in contextQuery.EmplTable on delegation.EmplTableToId equals emplTo.Id
                                      where delegation.DateFrom <= currentDate && delegation.DateTo >= currentDate && delegation.isArchive == false
                                      && delegation.CompanyTableId == user.CompanyTableId && emplTo.ApplicationUserId == user.Id
                                      select delegation).ToList();

                List<Guid> childGroup = new List<Guid>();

                foreach (var item in delegations.Where(x => x.GroupProcessTableId != null))
                {
                    childGroup.AddRange(_GroupProcessService.GetGroupChildren(item.GroupProcessTableId));
                    childGroup.Add((Guid)item.GroupProcessTableId);
                }

                var childGroupArray = childGroup.Distinct().ToArray();

                List<Guid> documentAccessList = new List<Guid>();

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            where document.ApplicationUserCreatedId == user.Id && document.DocType != DocumentType.Task
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            join tracker in contextQuery.WFTrackerTable on document.Id equals tracker.DocumentTableId
                                            where document.DocType != DocumentType.Task && tracker.TrackerType == TrackerType.Waiting && tracker.Users.Any(x => x.UserId == user.Id)
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            from tracker in contextQuery.WFTrackerTable.Where(x => x.DocumentTableId == document.Id).OrderByDescending(x => x.LineNum).Take(1)
                                            where document.DocType == DocumentType.Task && tracker.TrackerType == TrackerType.Waiting && tracker.Users.Any(x => x.UserId == user.Id)
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            join modification in contextQuery.ModificationUsersTable on document.Id equals modification.DocumentTableId
                                            where modification.UserId == user.Id && document.DocumentState == DocumentState.Created
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                                            join role in contextQuery.Roles on process.StartReaderRoleId equals role.Id
                                            where process.StartReaderRoleId != null && role.Users.Any(x => x.UserId == user.Id)
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            join reader in contextQuery.DocumentReaderTable on document.Id equals reader.DocumentTableId
                                            where document.DocumentState != DocumentState.Created && reader.UserId == user.Id
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                            join reader in contextQuery.DocumentReaderTable on document.Id equals reader.DocumentTableId
                                            join role in contextQuery.Roles on reader.RoleId equals role.Id
                                            where document.DocumentState != DocumentState.Created && reader.RoleId != null && role.Users.Any(x => x.UserId == user.Id)
                                            select document.Id);

                if (delegations.Count() > 0)
                {
                    documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                                    where (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                                    && d.CompanyTableId == user.CompanyTableId
                                                    && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                                                    && (d.ProcessTableId == null || d.ProcessTableId == document.ProcessTableId)
                                                    && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId)) ))
                                                select document.Id);
                }

                documentAccessList.Distinct();
                var documentAccessListArray = documentAccessList.ToArray();

                var items = from document in contextQuery.DocumentTable
                            where !(contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isArchive == true))
                            && documentAccessListArray.Contains(document.Id)
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            orderby String.IsNullOrEmpty(document.ActivityName), document.ModifiedDate descending
                            select new DocumentView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                DocumentText = document.DocumentText,
                                FileId = document.FileId,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName
                            };
                /*
                var items = from document in contextQuery.DocumentTable
                            where
                                (document.ApplicationUserCreatedId == user.Id ||
                                    contextQuery.ModificationUsersTable.Any(m => m.UserId == user.Id && m.DocumentTableId == document.Id && document.DocumentState == DocumentState.Created) 
                                    ||
                                     contextQuery.ProcessTable.Any(p => p.Id == document.ProcessTableId && contextQuery.Roles.Where( pr => pr.Id == p.StartReaderRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id )))
                                    ||
                                    contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == null && x.TrackerType == TrackerType.Waiting && x.Users.Any(b => b.UserId == user.Id)) ||

                                    ((contextQuery.DocumentReaderTable.Any(r => r.DocumentTableId == document.Id && r.UserId == user.Id) || (

                                    contextQuery.DocumentReaderTable.Any(d => d.RoleId != null && d.DocumentTableId == document.Id && contextQuery.Roles.Where( r => r.Id == d.RoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id )))
                                    
                                        
                                        )) && document.DocumentState != DocumentState.Created) ||
                                    (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                    && d.CompanyTableId == user.CompanyTableId
                                    && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                                    && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                                    && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                                    ))
                                )
                                &&
                                !(contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isArchive == true))
                                && !(document.ApplicationUserCreatedId == user.Id && document.DocType == DocumentType.Task)
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            orderby String.IsNullOrEmpty(document.ActivityName), document.ModifiedDate descending
                            select new DocumentView {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                DocumentText = document.DocumentText,
                                FileId = document.FileId,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName
                            };*/

                return items.AsQueryable();
            }
        }

        public IQueryable<DocumentView> GetArchiveView()
        {
            ApplicationUser user = getCurrentUserId();
            DateTime currentDate = DateTime.UtcNow;
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            if (UserManager.IsInRole(user.Id, "Administrator"))
            {
                var items = from document in contextQuery.DocumentTable
                        where (contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isArchive == true))
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                        orderby document.ModifiedDate descending
                        select new DocumentView
                        {
                            ActivityName = document.ActivityName,
                            ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                            ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                            CompanyTableId = document.CompanyTableId,
                            CreatedDate = document.CreatedDate,
                            DocumentNum = document.DocumentNum,
                            DocumentState = document.DocumentState,
                            DocumentText = document.DocumentText,
                            FileId = document.FileId,
                            Id = document.Id,
                            ModifiedDate = document.ModifiedDate,
                            ProcessTableId = document.ProcessTableId,
                            AliasCompanyName = company.AliasCompanyName,
                            ProcessName = process.ProcessName,
                            CreatedBy = empl.SecondName + " " + empl.FirstName
                       };

                return items.AsQueryable();
            }
            else
            {
                var delegations = contextQuery.DelegationTable.Where(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                    && d.CompanyTableId == user.CompanyTableId && d.GroupProcessTableId != null).ToList();
                List<Guid> childGroup = new List<Guid>();

                foreach (var item in delegations)
                {
                    childGroup.AddRange(_GroupProcessService.GetGroupChildren(item.GroupProcessTableId));
                    childGroup.Add((Guid)item.GroupProcessTableId);
                }

                var childGroupArray = childGroup.Distinct().ToArray();

                var items = from document in contextQuery.DocumentTable
                       where
                           (document.ApplicationUserCreatedId == user.Id ||
                               contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == null && x.TrackerType == TrackerType.Waiting && x.Users.Any(b => b.UserId == user.Id)) ||
                               contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == user.Id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled)) ||
                               contextQuery.DocumentReaderTable.Any(r => r.DocumentTableId == document.Id && r.UserId == user.Id) ||
                                     contextQuery.ProcessTable.Any(p => p.Id == document.ProcessTableId && contextQuery.Roles.Where( pr => pr.Id == p.StartReaderRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id )))
                                         ||
                               (contextQuery.DocumentReaderTable.Any(d => d.RoleId != null && d.DocumentTableId == document.Id && contextQuery.Roles.Where(r => r.Id == d.RoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))) ||
                               (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                               && d.CompanyTableId == user.CompanyTableId
                               && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                               && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                               && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                               ))
                           )
                           && (contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isArchive == true))
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                        orderby document.ModifiedDate descending
                        select new DocumentView
                        {
                            ActivityName = document.ActivityName,
                            ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                            ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                            CompanyTableId = document.CompanyTableId,
                            CreatedDate = document.CreatedDate,
                            DocumentNum = document.DocumentNum,
                            DocumentState = document.DocumentState,
                            DocumentText = document.DocumentText,
                            FileId = document.FileId,
                            Id = document.Id,
                            ModifiedDate = document.ModifiedDate,
                            ProcessTableId = document.ProcessTableId,
                            AliasCompanyName = company.AliasCompanyName,
                            ProcessName = process.ProcessName,
                            CreatedBy = empl.SecondName + " " + empl.FirstName
                        };

                return items.AsQueryable();
            }
        }

        public IQueryable<DocumentView> GetAgreedDocument() 
        {
            ApplicationUser user = getCurrentUserId();
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            var items = from document in contextQuery.DocumentTable
                    where
                       (contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == user.Id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled)))
                       &&
                       !(contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isArchive == true))
                        join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                        join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                        let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                        orderby document.ModifiedDate descending
                    select new DocumentView
                    {
                        ActivityName = document.ActivityName,
                        ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                        ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                        CompanyTableId = document.CompanyTableId,
                        CreatedDate = document.CreatedDate,
                        DocumentNum = document.DocumentNum,
                        DocumentState = document.DocumentState,
                        DocumentText = document.DocumentText,
                        FileId = document.FileId,
                        Id = document.Id,
                        ModifiedDate = document.ModifiedDate,
                        ProcessTableId = document.ProcessTableId,
                        AliasCompanyName = company.AliasCompanyName,
                        ProcessName = process.ProcessName,
                        CreatedBy = empl.SecondName + " " + empl.FirstName
                    };

            return items.AsQueryable();
        }

        public IQueryable<DocumentView> GetMyDocumentView()
        {
            ApplicationUser user = getCurrentUserId();
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            var items = from document in contextQuery.DocumentTable
                        where document.ApplicationUserCreatedId == user.Id
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                        let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                        orderby document.CreatedDate descending
                        select new DocumentView
                        {
                            ActivityName = document.ActivityName,
                            ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                            ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                            CompanyTableId = document.CompanyTableId,
                            CreatedDate = document.CreatedDate,
                            DocumentNum = document.DocumentNum,
                            DocumentState = document.DocumentState,
                            DocumentText = document.DocumentText,
                            FileId = document.FileId,
                            Id = document.Id,
                            ModifiedDate = document.ModifiedDate,
                            ProcessTableId = document.ProcessTableId,
                            AliasCompanyName = company.AliasCompanyName,
                            ProcessName = process.ProcessName,
                            CreatedBy = empl.SecondName + " " + empl.FirstName
                        };

            return items.AsQueryable();
        }

        public IQueryable<DocumentView> GetMyFavoriteView()
        {
            ApplicationUser user = getCurrentUserId();
            DateTime currentDate = DateTime.UtcNow;
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            if (UserManager.IsInRole(user.Id, "Administrator"))
            {
                var items = from document in contextQuery.DocumentTable
                            where (contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isFavorite == true))
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            orderby document.ModifiedDate descending
                            select new DocumentView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                DocumentText = document.DocumentText,
                                FileId = document.FileId,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName
                            };

                return items.AsQueryable();
            }
            else
            {
                var delegations = contextQuery.DelegationTable.Where(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                    && d.CompanyTableId == user.CompanyTableId && d.GroupProcessTableId != null).ToList();
                List<Guid> childGroup = new List<Guid>();

                foreach (var item in delegations)
                {
                    childGroup.AddRange(_GroupProcessService.GetGroupChildren(item.GroupProcessTableId));
                    childGroup.Add((Guid)item.GroupProcessTableId);
                }

                var childGroupArray = childGroup.Distinct().ToArray();

                var items = from document in contextQuery.DocumentTable
                            where
                                (document.ApplicationUserCreatedId == user.Id ||
                                    contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == null && x.TrackerType == TrackerType.Waiting && x.Users.Any(b => b.UserId == user.Id)) ||
                                    contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == user.Id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled)) ||
                                    contextQuery.DocumentReaderTable.Any(r => r.DocumentTableId == document.Id && r.UserId == user.Id) ||
                                          contextQuery.ProcessTable.Any(p => p.Id == document.ProcessTableId && contextQuery.Roles.Where(pr => pr.Id == p.StartReaderRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))
                                              ||
                                    (contextQuery.DocumentReaderTable.Any(d => d.RoleId != null && d.DocumentTableId == document.Id && contextQuery.Roles.Where(r => r.Id == d.RoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))) ||
                                    (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                    && d.CompanyTableId == user.CompanyTableId
                                    && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                                    && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                                    && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                                    ))
                                )
                                && (contextQuery.ReviewDocLogTable.Any(x => x.ApplicationUserCreatedId == user.Id && x.DocumentTableId == document.Id && x.isFavorite == true))
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            orderby document.ModifiedDate descending
                            select new DocumentView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                DocumentText = document.DocumentText,
                                FileId = document.FileId,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName
                            };

                return items.AsQueryable();
            }
        }

        public IQueryable<DocumentTaskView> GetTaskDocumentView()
        {
            ApplicationUser user = getCurrentUserId();
            DateTime currentDate = DateTime.UtcNow;
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            var delegations = contextQuery.DelegationTable.Where(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                    && d.CompanyTableId == user.CompanyTableId && d.GroupProcessTableId != null).ToList();
            List<Guid> childGroup = new List<Guid>();

            foreach (var item in delegations)
            {
                childGroup.AddRange(_GroupProcessService.GetGroupChildren(item.GroupProcessTableId));
                childGroup.Add((Guid)item.GroupProcessTableId);
            }

            var childGroupArray = childGroup.Distinct().ToArray();

            var items = from document in contextQuery.DocumentTable
                        where (contextQuery.ProcessTable.Any(p => p.Id == document.ProcessTableId && contextQuery.Roles.Where(pr => pr.Id == p.StartReaderRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))
                                    ||
                                    contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && x.SignUserId == null && x.TrackerType == TrackerType.Waiting && x.Users.Any(b => b.UserId == user.Id)) ||
                                    (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                    && d.CompanyTableId == user.CompanyTableId
                                    && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                                    && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                                    && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                                    ))
                                )
                        join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                        join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                        join documentData in contextQuery.USR_TAS_DailyTasks_Table on document.Id equals documentData.DocumentTableId
                        let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                        where process.DocType == DocumentType.Task
                        orderby document.CreatedDate descending
                        select new DocumentTaskView
                        {
                            ActivityName = document.ActivityName,
                            ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                            ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                            CompanyTableId = document.CompanyTableId,
                            CreatedDate = document.CreatedDate,
                            DocumentNum = document.DocumentNum,
                            DocumentState = document.DocumentState,
                            DocumentText = document.DocumentText,
                            Id = document.Id,
                            ModifiedDate = document.ModifiedDate,
                            ProcessTableId = document.ProcessTableId,
                            AliasCompanyName = company.AliasCompanyName,
                            ProcessName = process.ProcessName,
                            CreatedBy = empl.SecondName + " " + empl.FirstName,
                            ExecutionDate = documentData.ProlongationDate == null ? documentData.ExecutionDate : documentData.ProlongationDate
                        };

            return items.AsQueryable();
        }

        public IEnumerable<DocumentTable> GetPartial(Expression<Func<DocumentTable, bool>> predicate)
        {
            return repoDocument.FindAll(predicate);
        }

        public DocumentTable FirstOrDefault(Expression<Func<DocumentTable, bool>> predicate)
        {
            return repoDocument.Find(predicate);
        }
        public DocumentView FirstOrDefaultView(Expression<Func<DocumentTable, bool>> predicate)
        {
            return Mapper.Map<DocumentTable, DocumentView>(FirstOrDefault(predicate));
        }

        public DocumentTable Find(Guid? id)
        {
            return repoDocument.Find(a => a.Id == id);
        }
        public DocumentView FindView(Guid? id)
        {
            return Mapper.Map<DocumentTable, DocumentView>(Find(id));
        }

        public bool Contains(Expression<Func<DocumentTable, bool>> predicate)
        {
            return repoDocument.Contains(predicate);
        }

        public DocumentView Document2View(DocumentTable documentTable)
        {
            return Mapper.Map<DocumentTable, DocumentView>(documentTable);
        }

        public dynamic GetDocument(Guid refDocumentId, string tableName)
        {
            var domainModel = RouteCustomRepository(tableName).GetById(refDocumentId);
            return domainModel;
        }

        public dynamic GetDocumentView(Guid refDocumentId, string tableName)
        {
            var viewTable = RouteCustomModelView(tableName);
            var domainTable = GetDocument(refDocumentId, tableName);
            Type typeDomain = Type.GetType("RapidDoc.Models.DomainModels." + tableName + "_Table");
            Type typeDomainView = Type.GetType("RapidDoc.Models.ViewModels." + tableName + "_View");
            Mapper.CreateMap(typeDomain, typeDomainView);
            Mapper.Map(domainTable, viewTable, typeDomain, typeDomainView);
            return viewTable;
        }

        public IEnumerable<dynamic> GetDocumentAll(string tableName)
        {
            var domainModel = RouteCustomRepository(tableName).All();
            return domainModel;
        }

        public void UpdateDocument(DocumentTable domainTable, string currentUserId = "")
        {
            ApplicationUser user = getCurrentUserId(currentUserId);
            domainTable.ApplicationUserModifiedId = user.Id;
            domainTable.ModifiedDate = DateTime.UtcNow;

            if (domainTable.DocumentState == DocumentState.Agreement || domainTable.DocumentState == DocumentState.Execution || domainTable.DocumentState == DocumentState.OnSign)
            {
                ApplicationDbContext dbContext = new ApplicationDbContext();
                List<WFTrackerTable> items = dbContext.WFTrackerTable.Where(x => x.DocumentTableId == domainTable.Id && x.TrackerType == TrackerType.Waiting).OrderBy(x => x.LineNum).ToList();
                dbContext.Dispose();
                string currentName = String.Empty;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (domainTable.DocType == DocumentType.Task)
                        {
                            currentName = item.ActivityName + "/";
                            break;
                        }

                        currentName = currentName.Contains(Regex.Replace(item.ActivityName, @"[\d-]", string.Empty)) != true ? currentName + Regex.Replace(item.ActivityName, @"[\d-]", string.Empty) + "/" : currentName;
                        if(currentName.Length > 60)
                            break;
                    }
                }

                if (currentName != String.Empty)
                {
                    currentName = currentName.Remove(currentName.Length - 1);

                    if(currentName.Length > 60)
                        currentName = currentName.Substring(0, 60) + "...";
                }

                domainTable.ActivityName = currentName;
            }
            else if (domainTable.DocumentState == DocumentState.Closed || domainTable.DocumentState == DocumentState.Cancelled)
            {
                domainTable.ActivityName = "";
            }

            _uow.GetRepository<DocumentTable>().Update(domainTable);
            _uow.Commit();
        }

        public void SaveDocumentText(DocumentTable domainTable)
        {
            _uow.GetRepository<DocumentTable>().Update(domainTable);
            _uow.Commit();
        }

        
        public bool isShowDocument(DocumentTable documentTable, ApplicationUser user, bool isAfterView = false)
        {
            if (user.Id == documentTable.ApplicationUserCreatedId)
            {
                return true;
            }

            if(documentTable.Share == true)
            {
                return true;
            }

            IEnumerable<WFTrackerTable> trackerTables = null;

            if (isAfterView == false)
            {
                trackerTables = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentTable.Id && x.SignUserId == null);
            }
            else
            {
                trackerTables = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentTable.Id);
            }

            if (checkTrackUsers(trackerTables, user.Id))
            {
                return true;
            }

            if (_DelegationService.CheckDelegation(user, documentTable.ProcessTable, trackerTables) == true)
            {
                return true;
            }

            if (_DocumentReaderService.Contains(x => x.DocumentTableId == documentTable.Id && x.UserId == user.Id) || _DocumentReaderService.ContainsRoleUser(documentTable.Id, user.Id))
            {
                
                return true;
            }

            if (UserManager.IsInRole(user.Id, "Administrator") ||
                (UserManager.IsInRole(user.Id, "FullView_Request") && documentTable.DocType == DocumentType.Request) ||
                (UserManager.IsInRole(user.Id, "FullView_Appeal") && documentTable.DocType == DocumentType.AppealDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_Incoming") && documentTable.DocType == DocumentType.IncomingDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_OfficeMemo") && documentTable.DocType == DocumentType.OfficeMemo) ||
                (UserManager.IsInRole(user.Id, "FullView_Order") && documentTable.DocType == DocumentType.Order) ||
                (UserManager.IsInRole(user.Id, "FullView_Outcoming") && documentTable.DocType == DocumentType.OutcomingDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_Protocol") && documentTable.DocType == DocumentType.Protocol) ||
                (UserManager.IsInRole(user.Id, "FullView_Task") && documentTable.DocType == DocumentType.Task))
            {
                return true;
            }

            if (_ModificationUsersService.ContainDocumentUser(documentTable.Id, user.Id))
            {
                return true;
            }

            if (_ProcessService.GetPartial(x => x.Id == documentTable.ProcessTableId).Any(p =>
                RoleManager.Roles.Where( pr => pr.Id == p.StartReaderRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id ))))
            {
                return true;
            }

            if (_ProcessService.GetPartial(x => x.Id == documentTable.ProcessTableId).Any(p =>
                RoleManager.Roles.Where(pr => pr.Id == p.DocumentBaseRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id))))
            {
                return true;
            }

            if (documentTable.DocType == DocumentType.Task)
            {
                var task = GetDocument(documentTable.RefDocumentId, documentTable.ProcessTable.TableName);
                if(task != null)
                {
                    var refDocument = Find(task.RefDocumentId);
                    if (refDocument != null && refDocument.DocType == DocumentType.Protocol)
                    {
                        var protocol = GetDocument(refDocument.RefDocumentId, refDocument.ProcessTable.TableName);
                        if(protocol != null && !String.IsNullOrEmpty(protocol.Chairman))
                        {
                            string[] chairman = GetUserListFromStructure(protocol.Chairman);

                            if(chairman.Count() > 0)
                            {
                                Guid emplId = Guid.Parse(chairman[0]);
                                var empl = _EmplService.FirstOrDefault(x => x.Id == emplId);
                                if (empl != null && empl.ApplicationUserId == user.Id)
                                    return true;
                            }
                        }
                    }
                }
            }

            if (documentTable.DocType == DocumentType.OfficeMemo)
            {
                var documentsRef = GetDocumentRefTask(documentTable.Id);

                foreach (var item in documentsRef)
                {
                    trackerTables = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == item.DocumentId);
                    if (checkTrackUsers(trackerTables, user.Id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool isSignDocument(Guid documentId, ApplicationUser user = null)
        {
            IEnumerable<WFTrackerTable> trackerTables = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentId && x.SignUserId == null);
            DocumentTable documentTable = Find(documentId);

            if (user == null)
                user = getCurrentUserId();

            if(checkTrackUsers(trackerTables, user.Id))
            {
                return true;
            }

            if (_DelegationService.CheckDelegation(user, documentTable.ProcessTable, trackerTables) == true)
            {
                return true;
            }

            return false;
        }

        public List<ApplicationUser> GetSignUsers(DocumentTable docuTable)
        {
            List<ApplicationUser> signUsers = new List<ApplicationUser>();

            if(docuTable != null)
            {
                IEnumerable<WFTrackerTable> trackerTables = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == docuTable.Id && x.TrackerType == TrackerType.Waiting);

                if (trackerTables != null)
                {
                    foreach (var trackerTable in trackerTables)
                    {
                        if (trackerTable.Users != null)
                        {
                            foreach (var trackUser in trackerTable.Users)
                            {
                                ApplicationUser user = repoUser.GetById(trackUser.UserId);
                                if (user != null)
                                    signUsers.Add(user);
                            }
                        }
                    }
                }
                List<ApplicationUser> delegationUserCheck = signUsers.ToList();
                signUsers.AddRange(_DelegationService.GetDelegationUsers(docuTable, delegationUserCheck));
            }

            return signUsers;
        }

        public List<ApplicationUser> GetSignUsersDirect(DocumentTable docuTable)
        {
            List<ApplicationUser> signUsers = new List<ApplicationUser>();

            if (docuTable != null)
            {
                List<WFTrackerTable> trackerTables = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == docuTable.Id && x.TrackerType == TrackerType.Waiting).ToList();
                List<ApplicationUser> users = repoUser.FindAll(x => x.Enable == true && x.Email != String.Empty).ToList();

                if (trackerTables != null)
                {
                    foreach (var trackerTable in trackerTables)
                    {
                        if (trackerTable.Users != null)
                        {
                            foreach (var trackUser in trackerTable.Users)
                            {
                                ApplicationUser user = users.FirstOrDefault(x => x.Id == trackUser.UserId);
                                if (user != null)
                                    signUsers.Add(user);
                            }
                        }
                    }
                }
                List<ApplicationUser> delegationUserCheck = signUsers.ToList();
                signUsers.AddRange(_DelegationService.GetDelegationUsers(docuTable, delegationUserCheck));
            }

            return signUsers;
        }

        public SLAStatusList SLAStatus(Guid documentId, string currentUserId = "", ApplicationUser user = null)
        {
            IEnumerable<WFTrackerTable> items = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Waiting && x.SLAOffset > 0);

            if (items != null)
            {
                if (user == null)
                {
                    user = getCurrentUserId(currentUserId);
                }
                if (items.Any(x => x.Users.Any(a => a.UserId == user.Id)))
                {
                    WFTrackerTable item = items.FirstOrDefault();
                    DateTime? date = GetSLAPerformDate(documentId, item.StartDateSLA, item.SLAOffset);

                    if (date != null && item.StartDateSLA != null)
                    {
                        if (date < DateTime.UtcNow)
                        {
                            return SLAStatusList.Disturbance;
                        }
                        else if (date >= DateTime.UtcNow && (100 * Convert.ToInt32((date.Value - DateTime.UtcNow).TotalMinutes)) / Convert.ToInt32(((DateTime)date.Value - (DateTime)item.StartDateSLA).TotalMinutes) <= 20)
                        {
                            return SLAStatusList.Warning;
                        }
                    }
                }
            }

            return SLAStatusList.NoWarning;
        }

        private bool checkTrackUsers(IEnumerable<WFTrackerTable> trackerTables, string userId)
        {
            if (trackerTables != null)
            {
                foreach (var trackerTable in trackerTables)
                {
                    if (trackerTable.Users != null)
                    {
                        if (trackerTable.Users.Exists(x => x.UserId == userId) || trackerTable.SignUserId == userId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<WFTrackerTable> GetCurrentSignStep(Guid documentId, string currentUserId = "", ApplicationUser user = null)
        {
            if (user == null)
            {
                user = getCurrentUserId(currentUserId);
            }
            IEnumerable<WFTrackerTable> trackerTables = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Waiting);
            DocumentTable document = Find(documentId);
            ProcessTable process = _ProcessService.Find(document.ProcessTableId, currentUserId);
            List<WFTrackerTable> signStep = new List<WFTrackerTable>();

            if (trackerTables != null)
            {
                foreach (var trackerTable in trackerTables)
                {
                    if (_DelegationService.CheckTrackerUsers(trackerTable, user.Id))
                    {
                        signStep.Add(trackerTable);
                    }
                }

                var delegations = _DelegationService.GetDelegationUsers(document, user, trackerTables);

                foreach (var delegation in delegations)
                {
                    if (!signStep.Any(x => x.Id == delegation.Id))
                    {
                        signStep.Add(delegation);
                    }

                }
            }

            return signStep;
        }

        public void SaveSignData(IEnumerable<WFTrackerTable> trackerTables, TrackerType trackerType, bool changeSignUser = true, string currentUserId = "")
        {
            string userId = String.IsNullOrWhiteSpace(currentUserId) ? HttpContext.Current.User.Identity.GetUserId() : currentUserId; 

            foreach (var trackerTable in trackerTables)
            {
                int retries = 3;
                while (retries > 0)
                {
                    try
                    {

                        if (changeSignUser == true)
                        {
                            trackerTable.SignDate = DateTime.UtcNow;
                            trackerTable.SignUserId = userId;
                        }                    
                        trackerTable.TrackerType = trackerType;
                        _WorkflowTrackerService.SaveDomain(trackerTable, userId);
                        break;
                    }
                    catch
                    {
                        retries = retries - 1;
                        if (retries <= 0) throw;
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        public Guid SaveFile(FileTable file, string userId)
        {
            file.ApplicationUserCreatedId = userId;
            file.ApplicationUserModifiedId = userId;
            file.CreatedDate = DateTime.UtcNow;
            file.ModifiedDate = file.CreatedDate;

            _uow.GetRepository<FileTable>().Add(file);
            _uow.Commit();

            return file.Id;
        }

        public void UpdateFile(FileTable file)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            file.ApplicationUserModifiedId = userId;
            file.ModifiedDate = DateTime.UtcNow;

            _uow.GetRepository<FileTable>().Update(file);
            _uow.Commit();
        }

        public FileTable GetFile(Guid Id)
        {
            return repoFile.GetById(Id);
        }

        public FileTable FirstOrDefaultFile(Expression<Func<FileTable, bool>> predicate)
        {
            return repoFile.Find(predicate);
        }

        public bool FileReplaceContains(Guid id)
        {
            return repoFile.Any(x => x.ReplaceRef == id);
        }

        public IEnumerable<FileTable> GetAllFilesDocument(Guid documentFileId)
        {
            return repoFile.FindAll(x => x.DocumentFileId == documentFileId);
        }

        public IEnumerable<FileTable> GetAllTemplatesDocument(Guid processId)
        {
            return repoFile.FindAll(x => x.DocumentFileId == processId && x.ContentType != "APPLICATION/XAML+XML");
        }

        public IEnumerable<FileTable> GetAllXAMLDocument(Guid processId)
        {
            return repoFile.FindAll(x => x.DocumentFileId == processId && x.ContentType == "APPLICATION/XAML+XML");
        }

        public bool FileContains(Guid documentFileId)
        {
            return repoFile.Any(x => x.DocumentFileId == documentFileId);
        }

        public string DeleteFile(Guid Id)
        {
            string fileName = repoFile.GetById(Id).FileName;
            repoFile.Delete(a => a.Id == Id);
            _uow.Commit();

            return fileName;
        }
        public void DeleteFiles(Guid documentId)
        {

            repoFile.Delete(a => a.DocumentFileId == documentId);
            _uow.Commit();
        }
        public void DeleteDocumentDraft(Guid documentId, string tableName, Guid refDocumentId)
        {
             var domainTable = GetDocument(refDocumentId, tableName);
             RouteCustomRepository(tableName).Delete(domainTable);

             _uow.Commit();
        }
        public void Delete(Guid Id)
        {
            repoDocument.Delete(x => x.Id == Id);
            _uow.Commit();
        }

        public List<WFTrackerUsersTable> GetUsersSLAStatus(DocumentTable docuTable, SLAStatusList status)
        {
            List<WFTrackerUsersTable> users = new List<WFTrackerUsersTable>();
            IEnumerable<WFTrackerTable> items = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == docuTable.Id && x.TrackerType == TrackerType.Waiting && x.SLAOffset > 0);

            if (items != null)
            {
                foreach (var item in items)
                {
                    DateTime? date = GetSLAPerformDate(docuTable.Id, item.StartDateSLA, item.SLAOffset);

                    if (date != null)
                    {
                        if (SLAStatusList.Disturbance == status && date < DateTime.UtcNow)
                        {
                            users.AddRange(item.Users);
                        }

                        if (SLAStatusList.Warning == status && DateTime.UtcNow < date && (
                            Convert.ToInt32((date.Value - DateTime.UtcNow).TotalDays) == 7 || Convert.ToInt32((date.Value - DateTime.UtcNow).TotalDays) < 4))
                        {
                            users.AddRange(item.Users);
                        }
                    }
                }
            }

            return users;
        }

        public DateTime? GetSLAPerformDate(Guid DocumentId, DateTime? CreatedDate, double SLAOffset)
        {
            if (CreatedDate != null)
            {
                DocumentTable documentTable = Find(DocumentId);

                if (documentTable != null && documentTable.ProcessTable != null)
                {
                    WorkScheduleTable scheduleTable = _WorkScheduleService.Find(documentTable.ProcessTable.WorkScheduleTableId ?? Guid.Empty);
                    if (scheduleTable != null)
                    {
                        CreatedDate = GetWorkStartDate(CreatedDate, scheduleTable);
                        //double SLAMinutes = (SLAOffset * 60);

                        return GetSLAAddOffset(scheduleTable, CreatedDate, SLAOffset);
                    }
                }
            }

            return null;
        }

        private DateTime GetSLAAddOffset(WorkScheduleTable scheduleTable, DateTime? CreatedDate, double SLAMinutes)
        {
            DateTime tempCreatedDate = CreatedDate ?? DateTime.MinValue;

            if ((scheduleTable.WorkEndTime - tempCreatedDate.TimeOfDay).TotalMinutes >= SLAMinutes)
            {
                return tempCreatedDate.AddMinutes(SLAMinutes);
            }
            else
            {
                SLAMinutes = SLAMinutes - (scheduleTable.WorkEndTime - tempCreatedDate.TimeOfDay).TotalMinutes;
                tempCreatedDate = GetWorkStartDate(tempCreatedDate.Date.AddDays(1), scheduleTable);

                return GetSLAAddOffset(scheduleTable, tempCreatedDate, SLAMinutes);
            }
        }

        private DateTime GetWorkStartDate(DateTime? CreatedDate, WorkScheduleTable scheduleTable)
        {
            DateTime tempCreatedDate = CreatedDate ?? DateTime.MinValue;

            tempCreatedDate = FindWorkDay(scheduleTable.Id, tempCreatedDate);

            if (tempCreatedDate.Hour > scheduleTable.WorkEndTime.Hours)
            {
                tempCreatedDate = FindWorkDay(scheduleTable.Id, tempCreatedDate);
                tempCreatedDate = tempCreatedDate.Date + scheduleTable.WorkStartTime;
            }
            else if (tempCreatedDate.Hour < scheduleTable.WorkStartTime.Hours)
            {
                tempCreatedDate = tempCreatedDate.Date + scheduleTable.WorkStartTime;
            }

            return tempCreatedDate;
        }

        private DateTime FindWorkDay(Guid scheduleId, DateTime createdDate)
        {
            DateTime tempCreatedDate = createdDate;

            while (_WorkScheduleService.CheckDayType(scheduleId, tempCreatedDate) == true)
            {
                tempCreatedDate = tempCreatedDate.AddDays(1);
            }

            return tempCreatedDate;
        }

        private ApplicationUser getCurrentUserId(string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                return repoUser.GetById(currentUserId);
            }
            else
            {
                return repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            }
        }

        public dynamic RouteCustomModelView(string customModel)
        {
            Type type = Type.GetType("RapidDoc.Models.ViewModels." + customModel + "_View");
            if (type != null)
                return Activator.CreateInstance(type) as IDocument;

            return null;
        }

        public dynamic RouteCustomModelDomain(string customModel)
        {
            Type type = Type.GetType("RapidDoc.Models.DomainModels." + customModel + "_Table");
            if (type != null)
                return Activator.CreateInstance(type) as IDocument;
            return null;
        }

        public dynamic RouteCustomRepository(string customModel)
        {
            Type type = Type.GetType("RapidDoc.Models.DomainModels." + customModel + "_Table");
            if (type != null)
            {
                var method = typeof(RapidDoc.Models.Infrastructure.IUnitOfWork).GetMethod("GetRepository");
                var methodGeneric = method.MakeGenericMethod(type);
                return methodGeneric.Invoke(_uow, null);
            }
            return null;
        }

        public List<string> SignDocumentCZ(Guid documentId, TrackerType trackerType, string comment = "")
        {
            List<string> ret = new List<string>();
            List<Guid> childGroup = new List<Guid>();
            string currentUserId = HttpContext.Current.User.Identity.GetUserId();
            ret.AddRange(SignDocumentUserCZ(documentId, trackerType, currentUserId, comment));

            var document = Find(documentId);
            var emplTables = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId == currentUserId && x.Enable == true).ToList();

            foreach(var empl in emplTables)
            {
                var delegationItems = _DelegationService.GetPartial(x => x.EmplTableToId == empl.Id
                    && x.DateFrom <= DateTime.UtcNow && x.DateTo >= DateTime.UtcNow
                    && x.isArchive == false && x.CompanyTableId == empl.CompanyTable.Id).ToList();

                foreach(var delegation in delegationItems)
                {
                    if (delegation.GroupProcessTableId != null && delegation.ProcessTableId == null)
                    {
                        childGroup.AddRange(_GroupProcessService.GetGroupChildren(delegation.GroupProcessTableId));
                        childGroup.Add((Guid)delegation.GroupProcessTableId);
                    }

                    var item = _EmplService.FirstOrDefault(x => x.Id == delegation.EmplTableFromId && x.CompanyTableId == delegation.CompanyTableId && x.Enable == true);
                    if (item != null && (delegation.ProcessTableId == document.ProcessTableId || (childGroup.Any(d => d == document.ProcessTable.GroupProcessTableId) && delegation.GroupProcessTableId != null) || (delegation.ProcessTableId == null && delegation.GroupProcessTableId == null)))
                    {
                        ret.AddRange(SignDocumentUserCZ(documentId, trackerType, item.ApplicationUserId, comment));
                    }
                    childGroup.Clear();
                }
            }
            return ret;
        }

        private List<string> SignDocumentUserCZ(Guid documentId, TrackerType trackerType, string userid, string comment = "")
        {
            List<string> ret = new List<string>();
            var trackerParallel = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && x.SignUserId == null && x.ParallelID != String.Empty && x.TrackerType == TrackerType.Waiting && x.Users.Any(p => p.UserId == userid)).ToList();
            trackerParallel.ForEach(x => x.Comments = comment);
            if (trackerParallel != null && trackerParallel.Count > 0)
            {
                SaveSignData(trackerParallel, trackerType);
            }

            var trackerSeq = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && x.SignUserId == null && x.ParallelID == String.Empty && x.Users.Any(p => p.UserId == userid)).ToList();
            if (trackerSeq != null && trackerSeq.Count > 0)
            {
                trackerSeq.ForEach(x => x.Comments = comment);
                SaveSignData(trackerSeq, trackerType);

                if (trackerType == TrackerType.Approved)
                {
                    foreach (var item in trackerSeq)
                    {
                        var nextstep = _WorkflowTrackerService.GetPartial(y => y.TrackerType == TrackerType.NonActive && y.LineNum > item.LineNum && y.ActivityID == item.ActivityID).OrderBy(o => o.LineNum).FirstOrDefault();
                        if (nextstep != null)
                        {
                            nextstep.TrackerType = TrackerType.Waiting;
                            nextstep.StartDateSLA = DateTime.UtcNow;
                            _WorkflowTrackerService.SaveDomain(nextstep, userid);
                            if (nextstep.Users != null)
                            {
                                ret.AddRange(nextstep.Users.Select(x => x.UserId + "|" + nextstep.AdditionalText));
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public void SignTaskDocument(Guid documentId, TrackerType trackerType, string userId = "")
        {
            string currentUserId = String.IsNullOrEmpty(userId) ? HttpContext.Current.User.Identity.GetUserId() : userId;
            bool isSign = false;

            var document = Find(documentId);
            List<Guid> childGroup = new List<Guid>();
            //WFTrackerTable trackerTable = FirstOrDefaultTrackerItem(document.ProcessTable, documentId, currentUserId);
            WFTrackerTable trackerTable = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == documentId && x.SignUserId == null && x.TrackerType == TrackerType.Waiting && x.Users.Any(p => p.UserId == currentUserId));
            if (trackerTable == null)
            {
                var emplTables = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId == currentUserId && x.Enable == true).ToList();

                foreach (var empl in emplTables)
                {
                    var delegationItems = _DelegationService.GetPartial(x => x.EmplTableToId == empl.Id
                        && x.DateFrom <= DateTime.UtcNow && x.DateTo >= DateTime.UtcNow
                        && x.isArchive == false && x.CompanyTableId == empl.CompanyTable.Id).ToList();

                    if (isSign == true)
                        break;

                    foreach (var delegation in delegationItems)
                    {
                        if (delegation.GroupProcessTableId != null && delegation.ProcessTableId == null)
                        {
                            childGroup.AddRange(_GroupProcessService.GetGroupChildren(delegation.GroupProcessTableId));
                            childGroup.Add((Guid)delegation.GroupProcessTableId);
                        }

                        var item = _EmplService.FirstOrDefault(x => x.Id == delegation.EmplTableFromId && x.CompanyTableId == delegation.CompanyTableId && x.Enable == true);
                        if (item != null && (delegation.ProcessTableId == document.ProcessTableId || (childGroup.Any(d => d == document.ProcessTable.GroupProcessTableId) && delegation.GroupProcessTableId != null) || (delegation.ProcessTableId == null && delegation.GroupProcessTableId == null)))
                        {
                            trackerTable = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == documentId && x.SignUserId == null && x.TrackerType == TrackerType.Waiting && x.Users.Any(p => p.UserId == item.ApplicationUserId));
                            if (trackerTable != null)
                            {
                                SaveSignData(new List<WFTrackerTable> { trackerTable }, trackerType, true, userId);
                                isSign = true;
                                break;
                            }
                        }
                        childGroup.Clear();
                    }
                }
            }
            else
            {
                SaveSignData(new List<WFTrackerTable> { trackerTable }, trackerType, true, userId);
            }

            var trackerTables = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId && x.SignUserId == null && x.TrackerType == TrackerType.Waiting).ToList();

            if (trackerTables != null && trackerTables.Count > 0)
                SaveSignData(trackerTables, TrackerType.Active, false);
        }

        public List<TaskDelegationView> GetDocumentRefTask(Guid documentId)
        {
            List<TaskDelegationView> taskDelegationList = new List<TaskDelegationView>();
            ApplicationUser currentUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());

            var  taskList= _uow.GetDbContext<ApplicationDbContext>().USR_TAS_DailyTasks_Table.Where(x => x.RefDocumentId == documentId).ToList();
            taskList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { 
                DocumentNum = y.DocumentTable.DocumentNum, 
                DocumentId = y.DocumentTable.Id, 
                DocType = y.DocumentTable.DocType,
                DateCreate = y.CreatedDate, 
                UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, 
                ReportText = y.ReportText, 
                DocumentState = y.DocumentTable.DocumentState,
                Users = y.Users,
                ExecutionDate = y.ExecutionDate,
                ProlongationDate = y.ProlongationDate,
                FileId = y.DocumentTable.FileId,
                DocumentText = y.MainField
            }));

            var  prolongationsList= _uow.GetDbContext<ApplicationDbContext>().USR_TAS_DailyTasksProlongation_Table.Where(x => x.RefDocumentId == documentId).ToList();
            prolongationsList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { 
                DocumentNum = y.DocumentTable.DocumentNum, 
                DocumentId = y.DocumentTable.Id, 
                DateCreate = y.CreatedDate, 
                UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, 
                DocumentState = y.DocumentTable.DocumentState, 
                DocType = y.DocumentTable.DocType,
                ProlongationDate = y.ProlongationDate
            }));

            //Orders
            var mainActivityList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_MainActivity_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            mainActivityList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var businessTripList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_BusinessTrip_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            businessTripList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var staffList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_Staff_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            staffList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var receptionList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_Reception_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            receptionList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var dismissalList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_Dismissal_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            dismissalList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var transferList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_Transfer_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            transferList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var holidayList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_Holiday_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            holidayList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var changeStaffList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_ChangeStaff_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            changeStaffList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));
            var sanctionList = _uow.GetDbContext<ApplicationDbContext>().USR_ORD_Sanction_Table.Where(x => x.AdditionDocumentId == documentId).ToList();
            sanctionList.ForEach(y => taskDelegationList.Add(new TaskDelegationView { DocumentNum = y.DocumentTable.DocumentNum, DocumentId = y.DocumentTable.Id, DateCreate = y.CreatedDate, UserCreate = _EmplService.GetEmployer(y.DocumentTable.ApplicationUserCreatedId, currentUser.CompanyTableId).FullName, DocType = y.DocumentTable.DocType }));

            return taskDelegationList;
        }


        public string[] GetUserListFromStructure(string users)
        {
            string initailStructure = users;
            string[] arrayTempStructrue = initailStructure.Split(',');

            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] arrayStructure = arrayTempStructrue.Where(a => isGuid.IsMatch(a) == true).ToArray();

            return arrayStructure;
        }

        public Guid? UpdateProlongationDate(Guid refDocumentid, DateTime prolongationDate, string currentUserId)
        {
            DocumentTable documentTable = Find(refDocumentid);
            ProcessView processView = _ProcessService.FindView(documentTable.ProcessTableId, currentUserId);
            var trackers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentTable.Id);
            var document = GetDocumentView(documentTable.RefDocumentId, processView.TableName);

            if (trackers != null)
            {
                document.ProlongationDate = prolongationDate;
                UpdateDocumentFields(document, processView);

                foreach (var item in trackers)
                {
                    item.SLAOffset = Convert.ToInt32(GetSLAHours(refDocumentid, item.StartDateSLA, prolongationDate));
                    _WorkflowTrackerService.SaveDomain(item, currentUserId);
                }
            }

            DocumentTable parentDocumentSourceTable = Find(document.RefDocumentId);
            if (parentDocumentSourceTable != null && parentDocumentSourceTable.DocType == DocumentType.Task && parentDocumentSourceTable.Executed == false)
            {
                Guid rootDocId = FindRootTaskDocument(parentDocumentSourceTable.Id, processView.TableName);
                DocumentTable rootDocumentSourceTable = Find(rootDocId);

                if (rootDocumentSourceTable.DocType == DocumentType.Task)
                    ProlongateDate(rootDocId, prolongationDate, currentUserId, processView);

                ApplyProlongateDate(rootDocId, prolongationDate, currentUserId, processView);
            }

            

            return document.RefDocumentId;
        }

        public void ORDRegistration(Guid refDocumentid, string currentUserId, Guid? bookingNumberId)
        {
            DocumentTable documentTable = Find(refDocumentid);
            ProcessView processView = _ProcessService.FindView(documentTable.ProcessTableId, currentUserId);

            var document = GetDocumentView(documentTable.RefDocumentId, processView.TableName);
            NumberSeriesTable numberSeq = _NumberSeqService.FirstOrDefault(x => x.TableName == processView.TableName);

            if (numberSeq == null)
                return;

            string number = String.Empty;
            number = _NumberSeqService.GetDocumentNumORD(numberSeq.Id, bookingNumberId, currentUserId);

            if (!String.IsNullOrEmpty(number))
            {
                document.OrderNum = number;
                UpdateDocumentFields(document, processView);
            }
        }

        public void INDRegistration(Guid refDocumentid, string currentUserId, Guid? bookingNumberId)
        {
            DocumentTable documentTable = Find(refDocumentid);
            ProcessView processView = _ProcessService.FindView(documentTable.ProcessTableId, currentUserId);
            var document = GetDocumentView(documentTable.RefDocumentId, processView.TableName);

            if (document.OutcomingNumberDocId != null)
            {
                DocumentTable documentTableOut = Find(document.OutcomingNumberDocId);

                if (documentTableOut != null)
                {
                    var item = GetDocumentView(documentTableOut.RefDocumentId, documentTableOut.ProcessTable.TableName);
                    document.OutcomingNumber = item.OutcomingDocNum;
                }
            }

            NumberSeriesTable numberSeq = _NumberSeqService.FirstOrDefault(x => x.TableName == processView.TableName);
            
            if (numberSeq == null)
                return;

            string number = String.Empty;
            number = _NumberSeqService.GetDocumentNumORD(numberSeq.Id, bookingNumberId, currentUserId);

            if (!String.IsNullOrEmpty(number))
            {
                document.IncomingDocNum = number;
                UpdateDocumentFields(document, processView);
            }
        }

        public void APPRegistration(Guid refDocumentid, string currentUserId)
        {
            DocumentTable documentTable = Find(refDocumentid);
            ProcessView processView = _ProcessService.FindView(documentTable.ProcessTableId, currentUserId);
            var document = GetDocumentView(documentTable.RefDocumentId, processView.TableName);

            NumberSeriesTable numberSeq = _NumberSeqService.FirstOrDefault(x => x.TableName == processView.TableName);

            if (numberSeq == null)
                return;

            string number = String.Empty;
            number = _NumberSeqService.GetDocumentNumORD(numberSeq.Id, null, currentUserId);

            if (!String.IsNullOrEmpty(number))
            {
                document.RegistrationNum = number;
                UpdateDocumentFields(document, processView);
            }
        }

        public void OUTRegistration(Guid refDocumentid, string currentUserId, Guid? bookingNumberId)
        {
            DocumentTable documentTable = Find(refDocumentid);
            ProcessView processView = _ProcessService.FindView(documentTable.ProcessTableId, currentUserId);
            var document = GetDocumentView(documentTable.RefDocumentId, processView.TableName);

            if (document.IncomingNumberDocId != null)
            {
                DocumentTable documentTableInc = Find(document.IncomingNumberDocId);

                if (documentTableInc != null)
                {
                    var item = GetDocumentView(documentTableInc.RefDocumentId, documentTableInc.ProcessTable.TableName);
                    document.IncomingNumber = item.IncomingDocNum;
                    document.IncomingDate = item.RegistrationDate;
                }
            }

            NumberSeriesTable numberSeq = _NumberSeqService.FirstOrDefault(x => x.TableName == processView.TableName);

            if (numberSeq == null)
                return;

            string number = String.Empty;
            number = _NumberSeqService.GetDocumentNumORD(numberSeq.Id, bookingNumberId, currentUserId);

            if (!String.IsNullOrEmpty(number))
            {
                document.OutcomingDocNum = number;
                UpdateDocumentFields(document, processView);
            }
        }

        public SelectList RevocationORDList(Guid? id, bool edit)
        {
            List<USR_ORD_SelectListView> result = new List<USR_ORD_SelectListView>();
            result.Insert(0, new USR_ORD_SelectListView { Name = UIElementRes.UIElement.NoValue, Id = null });

            result.AddRange(GetOrderList<USR_ORD_MainActivity_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_Staff_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_Reception_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_Dismissal_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_Transfer_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_Holiday_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_ChangeStaff_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_Sanction_Table>(id, edit));
            result.AddRange(GetOrderList<USR_ORD_BusinessTrip_Table>(id, edit));

            return new SelectList(result, "Id", "Name", id);
        }

        public SelectList RevocationORDKZHList(Guid? id, bool edit)
        {
            List<USR_ORD_SelectListView> result = new List<USR_ORD_SelectListView>();
            result.Insert(0, new USR_ORD_SelectListView { Name = UIElementRes.UIElement.NoValue, Id = null });

            result.AddRange(GetOrderList<USK_ORD_MainActivity_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_Staff_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_Reception_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_Dismissal_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_Transfer_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_Holiday_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_ChangeStaff_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_Sanction_Table>(id, edit));
            result.AddRange(GetOrderList<USK_ORD_BusinessTrip_Table>(id, edit));

            return new SelectList(result, "Id", "Name", id);
        }

        public SelectList AdditionORDList(Guid? id, bool edit)
        {
            List<USR_ORD_SelectListView> result = new List<USR_ORD_SelectListView>();
            result.Insert(0, new USR_ORD_SelectListView { Name = UIElementRes.UIElement.NoValue, Id = null });

            result.AddRange(GetOrderList<USR_ORD_MainActivity_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_Staff_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_Reception_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_Dismissal_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_Transfer_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_Holiday_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_ChangeStaff_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_Sanction_Table>(id, edit, true));
            result.AddRange(GetOrderList<USR_ORD_BusinessTrip_Table>(id, edit, true));

            return new SelectList(result, "Id", "Name", id);
        }

        public SelectList AdditionORDKZHList(Guid? id, bool edit)
        {
            List<USR_ORD_SelectListView> result = new List<USR_ORD_SelectListView>();
            result.Insert(0, new USR_ORD_SelectListView { Name = UIElementRes.UIElement.NoValue, Id = null });

            result.AddRange(GetOrderList<USK_ORD_MainActivity_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_Staff_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_Reception_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_Dismissal_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_Transfer_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_Holiday_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_ChangeStaff_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_Sanction_Table>(id, edit, true));
            result.AddRange(GetOrderList<USK_ORD_BusinessTrip_Table>(id, edit, true));

            return new SelectList(result, "Id", "Name", id);
        }

        public SelectList IncomingDocList<T>(Guid? id) where T : BasicIncomingDocumentsTable
        {
            List<USR_IND_IncomingDocList> result = new List<USR_IND_IncomingDocList>();
            result.Insert(0, new USR_IND_IncomingDocList { Name = UIElementRes.UIElement.NoValue, Id = null });

            IRepository<T> repo = _uow.GetRepository<T>();
            List<T> items = repo.FindAll(x => !String.IsNullOrEmpty(x.IncomingDocNum) && x.Executed == true).OrderBy(x => x.IncomingDocNum).ToList();

            items.ForEach(x => result.Add(new USR_IND_IncomingDocList() { Name = x.IncomingDocNum + "/" + x.RegistrationDate.Value.ToShortDateString() + " " + x.DocumentSubject, Id = x.DocumentTableId }));

            return new SelectList(result, "Id", "Name", id);
        }

        public SelectList OutcomingDocList<T>(Guid? id) where T : BasicOutcomingDocumentsTable
        {
            List<USR_OND_OutcomingDocList> result = new List<USR_OND_OutcomingDocList>();
            result.Insert(0, new USR_OND_OutcomingDocList { Name = UIElementRes.UIElement.NoValue, Id = null });

            IRepository<T> repo = _uow.GetRepository<T>();
            List<T> items = repo.FindAll(x => !String.IsNullOrEmpty(x.OutcomingDocNum)).OrderBy(x => x.OutcomingDocNum).ToList();

            items.ForEach(x => result.Add(new USR_OND_OutcomingDocList() { Name = x.OutcomingDocNum + "/" + x.OutgoingDate.Value.ToShortDateString() + " " + x.DocumentSubject, Id = x.DocumentTableId }));

            return new SelectList(result, "Id", "Name", id);
        }

        private List<USR_ORD_SelectListView> GetOrderList<T>(Guid? id, bool edit, bool addition = false) where T : BasicOrderTable
        {
            List<USR_ORD_SelectListView> result = new List<USR_ORD_SelectListView>();
            IRepository<T> repo = _uow.GetRepository<T>();
            List<T> items = new List<T>();

            if (edit == true)
                if (addition)
                    items = repo.FindAll(x => !String.IsNullOrEmpty(x.OrderNum) && x.DocumentTable.Cancel == false && x.Addition == false && x.AdditionCount < 4).ToList();
                else
                    items = repo.FindAll(x => !String.IsNullOrEmpty(x.OrderNum) && x.DocumentTable.Cancel == false).ToList();
            else
                items = repo.FindAll(x => x.DocumentTableId == id).ToList();

            foreach (var item in items)
            {
                result.Add(new USR_ORD_SelectListView() { Name = item.OrderNum + ", " + item.OrderDate.Value.ToShortDateString(), Id = item.DocumentTableId });
            }
            return result;
        }

        public List<IncomingDublicateView> CheckIncomeDublicateDocument(Guid OrganizationId, string OutgoingNumber, DateTime OutgoingDate)
        {
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();
            List<IncomingDublicateView> items = new List<IncomingDublicateView>();
            items.AddRange(from item in contextQuery.USR_IND_IncomingDocuments_Table
                           join document in contextQuery.DocumentTable on item.DocumentTableId equals document.Id
                           join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                           let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                           where item.OrganizationTableId == OrganizationId && item.OutgoingNumber == OutgoingNumber
                           && item.OutgoingDate == OutgoingDate && document.DocumentState != DocumentState.Cancelled && document.DocumentState != DocumentState.Created
                           select new IncomingDublicateView
                               {
                                   RegistrationDate = item.RegistrationDate,
                                   CreatedBy = empl.FirstName + " " + empl.SecondName + " " + empl.MiddleName,
                                   CompanyName = company.AliasCompanyName
                               });

            items.AddRange(from item in contextQuery.USK_IND_IncomingDocuments_Table
                           join document in contextQuery.DocumentTable on item.DocumentTableId equals document.Id
                           join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                           let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                           where item.OrganizationTableId == OrganizationId && item.OutgoingNumber == OutgoingNumber
                           && item.OutgoingDate == OutgoingDate && document.DocumentState != DocumentState.Cancelled && document.DocumentState != DocumentState.Created
                           select new IncomingDublicateView
                           {
                               RegistrationDate = item.RegistrationDate,
                               CreatedBy = empl.FirstName + " " + empl.SecondName + " " + empl.MiddleName,
                               CompanyName = company.AliasCompanyName
                           });

            items.AddRange(from item in contextQuery.USC_IND_IncomingDocuments_Table
                           join document in contextQuery.DocumentTable on item.DocumentTableId equals document.Id
                           join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                           let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                           where item.OrganizationTableId == OrganizationId && item.OutgoingNumber == OutgoingNumber
                           && item.OutgoingDate == OutgoingDate && document.DocumentState != DocumentState.Cancelled && document.DocumentState != DocumentState.Created
                           select new IncomingDublicateView
                           {
                               RegistrationDate = item.RegistrationDate,
                               CreatedBy = empl.FirstName + " " + empl.SecondName + " " + empl.MiddleName,
                               CompanyName = company.AliasCompanyName
                           });
            return items;
        }

        public Type GetTableType(string TableName)
        {
            return Type.GetType("RapidDoc.Models.DomainModels." + TableName + "_Table");
        }

        public string ScrubHtml(string value)
        {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }


        public double GetSLAHours(Guid documentId, DateTime? startDate, DateTime? endDate)
        {
            if (startDate != null && endDate != null)
            {
                DocumentTable documentTable = Find(documentId);

                if (documentTable != null && documentTable.ProcessTable != null)
                {
                    WorkScheduleTable scheduleTable = _WorkScheduleService.Find(documentTable.ProcessTable.WorkScheduleTableId ?? Guid.Empty);
                    if (scheduleTable != null)
                    {
                        startDate = GetWorkStartDate(startDate, scheduleTable);

                        return GetSLAOffset(scheduleTable, startDate.Value, endDate.Value) - 1;
                    }
                }
            }

            return 0;
        }

        private double GetSLAOffset(WorkScheduleTable scheduleTable, DateTime createdDate, DateTime endDate)
        {
            double minutes = 0;
            while (createdDate < endDate + scheduleTable.WorkEndTime)
            {
                minutes += Math.Round((scheduleTable.WorkEndTime - createdDate.TimeOfDay).TotalMinutes);
                    
                createdDate = GetWorkStartDate(createdDate.Date.AddDays(1), scheduleTable);
            }
            return Math.Round(minutes);
        }

        public Guid DuplicateFile(FileTable fileTable, string userId, Guid? docFileId = null)
        {
            FileTable doc = new FileTable();

            if (docFileId != null)
                doc.DocumentFileId = (Guid)docFileId;
            else
                doc.DocumentFileId = Guid.NewGuid();

            doc.FileName = fileTable.FileName;
            doc.ContentType = fileTable.ContentType;
            doc.ContentLength = fileTable.ContentLength;
            doc.Data = fileTable.Data;
            doc.Thumbnail = fileTable.Thumbnail;
            doc.Version = "1";
            doc.VersionName = "Version 1";

            Guid Id = SaveFile(doc, userId);

            return doc.DocumentFileId;
        }

        public WFTrackerTable FirstOrDefaultTrackerItem(ProcessTable process, Guid documentId, string userId)
        {
            WFTrackerTable trackerTableUser = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Waiting && x.Users.Any(y => y.UserId == userId));
            if (trackerTableUser == null)
            {
                ApplicationUser user = repoUser.GetById(userId);
                if (user != null)
                {
                    var items = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId).OrderBy(x => x.LineNum).ToList();
                    foreach (var item in items)
                    {
                        if (_DelegationService.CheckDelegation(user, process, item))
                            return item;
                    }
                }
            }

            return trackerTableUser;
        }

        public DocumentComposite CreateViewBodyTaskFromDocument(DocumentTable docTable, ProcessView process, ApplicationUser userTable)
        {
            var refDocument = GetDocument(docTable.RefDocumentId, docTable.ProcessTable.TableName);

            var viewModel = new DocumentComposite();
            viewModel.ProcessView = process;
            viewModel.docData = RouteCustomModelView(process.TableName);

            viewModel.docData.RefDocumentId = docTable.Id;
            viewModel.docData.RefDocNum = docTable.DocumentNum;
            if (docTable.DocType == DocumentType.Task)
            {
                viewModel.docData.MainField = refDocument.MainField;
                viewModel.docData.ExecutionDate = refDocument.ProlongationDate != null ? refDocument.ProlongationDate : refDocument.ExecutionDate;
            }
            viewModel.fileId = Guid.NewGuid();
            viewModel.ProcessTemplates = GetAllTemplatesDocument((Guid)process.Id);

            return viewModel;
        }



        public void CloseUpRelatedTasks(Guid documentId, ApplicationUser userTable)
        {
            List<DocumentTable> documentChildTasks = new List<DocumentTable>();
            string jointMainRelatedText = "";
            
            DocumentTable docTable = Find(documentId);

            List<USR_TAS_DailyTasks_Table> childTasks = _uow.GetDbContext<ApplicationDbContext>().USR_TAS_DailyTasks_Table.Where(x => x.RefDocumentId == docTable.Id).ToList();

            childTasks.ForEach(x => documentChildTasks.Add(Find(x.DocumentTableId)));

            if (documentChildTasks.Where(x => x.DocumentState != DocumentState.Closed && x.DocumentState != DocumentState.Cancelled).Count() == 0 && documentChildTasks.Where(x => x.DocumentState == DocumentState.Closed).Count() > 0)
            {
                foreach (var childTask in childTasks)
                {
                    string executor = documentChildTasks.FirstOrDefault(x => x.Id == childTask.DocumentTableId).ApplicationUserModifiedId;
                    jointMainRelatedText += childTask.ReportText.Contains(UIElementRes.UIElement.Executed) == true ?
                        (UIElementRes.UIElement.Executed + ": " + _EmplService.FirstOrDefault(e => e.ApplicationUserId == executor).ShortFullNameType2 + ": " + childTask.ReportText) :
                        (UIElementRes.UIElement.Executed + ": " + _EmplService.FirstOrDefault(e => e.ApplicationUserId == executor).ShortFullNameType2 + ": " + _SystemService.DeleteAllTags(childTask.ReportText) + "</br>")
                        ;
                    ;
                }

                var documentIdNew = CloseTask(docTable, userTable, jointMainRelatedText, documentChildTasks.FirstOrDefault().ApplicationUserCreatedId, TrackerType.Approved);

                DocumentTable parentDocumentSourceTable = Find(documentIdNew.RefDocumentId);
                if (parentDocumentSourceTable != null && parentDocumentSourceTable.DocType == DocumentType.Task && parentDocumentSourceTable.Executed == false)
                {
                    CloseUpRelatedTasks(parentDocumentSourceTable.Id, userTable);
                }
            }
        }

        public dynamic CloseTask(DocumentTable docTable, ApplicationUser userTable, string mainCloseText, string closeUserId, TrackerType trackerType)
        {
            ProcessView process = _ProcessService.FirstOrDefaultView(x => x.TableName == "USR_TAS_DailyTasks" && x.CompanyTableId == userTable.CompanyTableId);

            var documentIdNew = GetDocumentView(Find(docTable.Id).RefDocumentId, process.TableName);

            SignTaskDocument(docTable.Id, trackerType, closeUserId);
            documentIdNew.ReportText = mainCloseText;
            UpdateDocumentFields(documentIdNew, process);

            docTable.DocumentState = trackerType == TrackerType.Approved ? DocumentState.Closed : DocumentState.Cancelled;
            UpdateDocument(docTable, closeUserId);

            return documentIdNew;
        }


        public void CloseDownRelatedTasks(Guid documentId, ApplicationUser userTable, TrackerType trackerType)
        {
            DocumentTable docTable = Find(documentId);
            string closeText = "Закрыто автоматически";

            List<USR_TAS_DailyTasks_Table> childTasks = _uow.GetDbContext<ApplicationDbContext>().USR_TAS_DailyTasks_Table.Where(x => x.RefDocumentId == docTable.Id && x.ReportText == null).ToList();

            if (childTasks.Count() > 0)
            {
                foreach (var childTask in childTasks)
                {
                    DocumentTable childDocTask = Find(childTask.DocumentTableId);
                    WFTrackerTable defaultExecutor = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == childDocTask.Id && x.SignUserId == null).FirstOrDefault();

                    CloseTask(childDocTask, userTable, closeText, defaultExecutor.Users.FirstOrDefault().UserId, trackerType);

                    CloseDownRelatedTasks(childDocTask.Id, userTable, trackerType);
                }
            }
        }

        public List<List<WFTrackerUsersTable>> GetUsersUpProlongatedTask(Guid documentId, string process)
        {
            List<List<WFTrackerUsersTable>> listUsersId = new List<List<WFTrackerUsersTable>>();
            List<List<WFTrackerUsersTable>> listUsersBufId = new List<List<WFTrackerUsersTable>>();
            DocumentTable docTable = Find(documentId);
            List<WFTrackerTable> trackerUsers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Waiting).OrderByDescending(y => y.LineNum).ToList();

            foreach (var trackerUser in trackerUsers)
            {
                List<WFTrackerUsersTable> userListTracker = new List<WFTrackerUsersTable>();
                foreach (var user in trackerUser.Users)
                {
                    userListTracker.Add( new WFTrackerUsersTable { UserId = user.UserId });
                }
                if (userListTracker.Count() > 0)
                    listUsersId.Add(userListTracker);  
            }

            var documentIdNew = GetDocumentView(Find(documentId).RefDocumentId, process);
            DocumentTable parentDocumentSourceTable = Find(documentIdNew.RefDocumentId);

            if (parentDocumentSourceTable != null && parentDocumentSourceTable.DocType == DocumentType.Task && parentDocumentSourceTable.Executed == false)
            {
                listUsersBufId = GetUsersUpProlongatedTask(parentDocumentSourceTable.Id, process);
            }
            else if (parentDocumentSourceTable != null && parentDocumentSourceTable.DocType == DocumentType.Protocol)
            {
                var protocolDoc = GetDocumentView(parentDocumentSourceTable.RefDocumentId, process);
                if (!String.IsNullOrEmpty(protocolDoc.Chairman))
                {
                    Guid chairmanEmplId = Guid.Parse(_SystemService.GuidsFromText(protocolDoc.Chairman)[0]);
                    EmplTable chairmanEmpl = _EmplService.Find(chairmanEmplId);
                    if (chairmanEmpl != null)
                    {
                        string chairmanId = chairmanEmpl.ApplicationUserId;
                        listUsersBufId.Add(new List<WFTrackerUsersTable> { new WFTrackerUsersTable { UserId = chairmanId } });
                    }
                }
            }
            else if (parentDocumentSourceTable != null)
            {
                listUsersBufId.Add(new List<WFTrackerUsersTable> { new WFTrackerUsersTable { UserId = parentDocumentSourceTable.ApplicationUserCreatedId } });
            }

            if (listUsersBufId.Count() > 0)
            {
                foreach (var listUserId in listUsersId)
                {
                    foreach (var userId in listUserId)
                    {
                        if (listUsersBufId.Any(x => x.Any(y => y.UserId == userId.UserId)))
                        {
                            listUsersBufId.ForEach(x => x.RemoveAll(y => y.UserId == userId.UserId));
                        }
                    }
                }

                listUsersBufId.Where(x => x.Count() > 0).ToList().ForEach(y => listUsersId.Add(y));
            }
            return listUsersId;
        }


        public Guid FindRootTaskDocument(Guid documentId, string processTableName)
        {
            Guid id = new Guid();
            var documentIdRef = GetDocumentView(Find(documentId).RefDocumentId, processTableName);
            DocumentTable parentDocumentSourceTable = Find(documentIdRef.RefDocumentId);

            if (parentDocumentSourceTable != null && parentDocumentSourceTable.DocType == DocumentType.Task)
            {
                id = FindRootTaskDocument(parentDocumentSourceTable.Id, processTableName);
                return id;
            }
            else if (parentDocumentSourceTable != null)
                return parentDocumentSourceTable.Id;

            return documentId;
        }


        public void ApplyProlongateDate(Guid documentId, DateTime prolongationDate,  string currentUserId, ProcessView process)
        {         
            List<USR_TAS_DailyTasks_Table> childTasks = _uow.GetDbContext<ApplicationDbContext>().USR_TAS_DailyTasks_Table.Where(x => x.RefDocumentId == documentId && x.ReportText == null).ToList();

            if (childTasks.Count() > 0)
            {
                foreach (var childTask in childTasks)
                {
                    ProlongateDate(childTask.DocumentTableId, prolongationDate, currentUserId, process);

                    ApplyProlongateDate(childTask.DocumentTableId, prolongationDate, currentUserId, process);
                }
            }
        }


        public void ProlongateDate(Guid documentId, DateTime prolongationDate, string currentUserId, ProcessView process)
        {
            DocumentTable taskDoc = Find(documentId);
            var trackers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == taskDoc.Id);
            
            if (trackers != null)
            {
                var document = GetDocumentView(taskDoc.RefDocumentId, process.TableName);
                document.ProlongationDate = prolongationDate;
                UpdateDocumentFields(document, process);

                foreach (var item in trackers)
                {
                    item.SLAOffset = Convert.ToInt32(GetSLAHours(taskDoc.Id, item.StartDateSLA, prolongationDate));
                    _WorkflowTrackerService.SaveDomain(item, currentUserId);
                }
            }
        }
    }
}