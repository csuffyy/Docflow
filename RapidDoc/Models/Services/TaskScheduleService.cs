using System;
using System.Web;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNet.Identity;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Infrastructure;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;


namespace RapidDoc.Models.Services
{
    public interface ITaskScheduleService
    {
        IEnumerable<TaskScheduleTable> GetAll();
        IEnumerable<TaskScheduleTable> GetAllIntercompany();
        IEnumerable<TaskScheduleView> GetAllView();
        IEnumerable<TaskScheduleTable> GetPartial(Expression<Func<TaskScheduleTable, bool>> predicate);
        IEnumerable<TaskScheduleView> GetPartialView(Expression<Func<TaskScheduleTable, bool>> predicate);
        IEnumerable<TaskScheduleTable> GetPartialIntercompany(Expression<Func<TaskScheduleTable, bool>> predicate);
        IEnumerable<TaskScheduleView> GetPartialIntercompanyView(Expression<Func<TaskScheduleTable, bool>> predicate);
        List<TaskScheduleView> GetTaskScheduleView();
        TaskScheduleTable FirstOrDefault(Expression<Func<TaskScheduleTable, bool>> predicate);
        TaskScheduleView FirstOrDefaultView(Expression<Func<TaskScheduleTable, bool>> prediacate);
        bool Contains(Expression<Func<TaskScheduleTable, bool>> predicate);
        void Save(TaskScheduleView viewTable);
        void SaveDomain(TaskScheduleTable domainTable, string userId = "");
        void Delete(Guid id);
        TaskScheduleTable Find(Guid id, string userId = "");
        TaskScheduleView FindView(Guid id);
        IEnumerable<FileTable> GetAllFilesTaskSchedule(Guid taskFileId);
        void CreateTaskFromSchedule(TaskScheduleTable taskSchedule, DateTime finalDate, ApplicationUser user, TaskScheduleHistroyTable taskScheduleHistroy = null);
        void UpdateRefDate(DateTime finalDate, Guid idSchedule, string userId = "");
    }

    public class TaskScheduleService : ITaskScheduleService
    {
        private IRepository<TaskScheduleTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<FileTable> repoFile;
        private IUnitOfWork uow;
        private readonly IProcessService _ProcessService;
        private readonly IDocumentService _DocumentService;
        private readonly ISearchService _SearchService;
        private readonly IWorkflowService _WorkflowService;
        private readonly ITaskScheduleHistroyService _TaskScheduleHistroyService;

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public TaskScheduleService(IUnitOfWork _uow, IProcessService processService, IDocumentService documentService, ISearchService searchService, IWorkflowService workflowService, ITaskScheduleHistroyService taskScheduleHistroyService)
        {
            uow = _uow;
            repo = uow.GetRepository<TaskScheduleTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoFile = uow.GetRepository<FileTable>();
            _ProcessService = processService;
            _DocumentService = documentService;
            _SearchService = searchService;
            _WorkflowService = workflowService;
            _TaskScheduleHistroyService = taskScheduleHistroyService;

            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_uow.GetDbContext<ApplicationDbContext>()));
        }

        public IEnumerable<TaskScheduleTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<TaskScheduleView> GetAllView()
        {
            return Mapper.Map<IEnumerable<TaskScheduleTable>, IEnumerable<TaskScheduleView>>(GetAll());
        }

        public IEnumerable<TaskScheduleTable> GetPartial(Expression<Func<TaskScheduleTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<TaskScheduleView> GetPartialView(Expression<Func<TaskScheduleTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<TaskScheduleTable>, IEnumerable<TaskScheduleView>>(GetPartial(predicate));
        }

        public IEnumerable<TaskScheduleTable> GetPartialIntercompany(Expression<Func<TaskScheduleTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<TaskScheduleView> GetPartialIntercompanyView(Expression<Func<TaskScheduleTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<TaskScheduleTable>, IEnumerable<TaskScheduleView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public List<TaskScheduleView> GetTaskScheduleView()
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();

            if (UserManager.IsInRole(userId, "Administrator"))
            {
                return GetAllView().ToList();
            }
            else
            {
                return GetPartialView(x => x.ApplicationUserCreatedId == userId).ToList();
            }
        }

        public TaskScheduleTable FirstOrDefault(Expression<Func<TaskScheduleTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public TaskScheduleView FirstOrDefaultView(Expression<Func<TaskScheduleTable, bool>> prediacate)
        {
            return Mapper.Map<TaskScheduleTable, TaskScheduleView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<TaskScheduleTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(TaskScheduleView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new TaskScheduleTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
            else
            {
                var domainTable = Find(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
        }

        public void SaveDomain(TaskScheduleTable domainTable, string userId = "")
        {
            if (String.IsNullOrEmpty(userId))
                userId = HttpContext.Current.User.Identity.GetUserId();

            ApplicationUser user = repoUser.GetById(userId);
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.Id = Guid.NewGuid();
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
                domainTable.CompanyTableId = user.CompanyTableId;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Update(domainTable);
            }
            uow.Commit();
        }

        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            uow.Commit();
        }

        public TaskScheduleTable Find(Guid id, string userId = "")
        {
            ApplicationUser user;
            if (!String.IsNullOrEmpty(userId))
                user = repoUser.GetById(userId);
            else
                user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());

            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public TaskScheduleView FindView(Guid id)
        {
            return Mapper.Map<TaskScheduleTable, TaskScheduleView>(Find(id));
        }

        public IEnumerable<FileTable> GetAllFilesTaskSchedule(Guid taskFileId)
        {
            return repoFile.FindAll(x => x.DocumentFileId == taskFileId);
        }


        public IEnumerable<TaskScheduleTable> GetAllIntercompany()
        {
            return repo.All();
        }

        public void CreateTaskFromSchedule(TaskScheduleTable taskSchedule, DateTime finalDate, ApplicationUser user, TaskScheduleHistroyTable taskScheduleHistroy = null)
        {
            USR_TAS_DailyTasks_View docModel = new USR_TAS_DailyTasks_View();
            docModel.MainField = taskSchedule.MainField;

            docModel.ExecutionDate = finalDate;
            docModel.Users = taskSchedule.Users;
            docModel.RefDocumentId = null;
            docModel.RefDocNum = null;
            docModel.ProjectTableId = taskSchedule.ProjectTableId;
            ProcessTable processTable = _ProcessService.FirstOrDefault(x => x.TableName == "USR_TAS_DailyTasks" && x.CompanyTableId == taskSchedule.CompanyTableId);

            List<FileTable> docFile = _DocumentService.GetAllFilesDocument(taskSchedule.fileId).ToList();
            Guid newDocFileId = Guid.NewGuid();
            docFile.ForEach(x => _DocumentService.DuplicateFile(x, user.Id, newDocFileId));

            var taskDocumentId = _DocumentService.SaveDocument(docModel, "USR_TAS_DailyTasks", processTable.Id, newDocFileId, user, false, false);
            DocumentTable documentTable = _DocumentService.Find(taskDocumentId);

            TaskScheduleHistroyTable taskScheduleHistroyTable = new TaskScheduleHistroyTable();
            taskScheduleHistroyTable.TaskScheduleId = taskSchedule.Id;
            taskScheduleHistroyTable.DocumentId = documentTable.Id;
            taskScheduleHistroyTable.DocumentNum = documentTable.DocumentNum;

            _TaskScheduleHistroyService.SaveDomain(taskScheduleHistroyTable, taskSchedule.CompanyTableId);

            _SearchService.SaveSearchData(taskDocumentId, docModel, "USR_TAS_DailyTasks", user.Id);
            Dictionary<string, object> taskDocumentData = new Dictionary<string, object>();
            taskDocumentData.Add("ExecutionDate", docModel.ExecutionDate);
            taskDocumentData.Add("MainField", docModel.MainField);
            taskDocumentData.Add("Users", docModel.Users);
            _WorkflowService.RunWorkflow(documentTable, "USR_TAS_DailyTasks", taskDocumentData, user.Id);
        }


        public void UpdateRefDate(DateTime finalDate, Guid idSchedule, string userId ="")
        {
            TaskScheduleTable updateSchedule = Find(idSchedule, userId);

            updateSchedule.RefDate = finalDate;
            SaveDomain(updateSchedule, updateSchedule.ApplicationUserCreatedId);
        }
    }
}