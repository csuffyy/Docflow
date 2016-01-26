using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Extensions;
using RapidDoc.Models.Repository;
using System.Globalization;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;


namespace RapidDoc.Controllers
{
    public class DocumentBaseController : BasicController
    {
        private readonly IDocumentBaseService _Service;
        private readonly IDepartmentService _DepartmentService;
        private readonly IEmplService _EmplService;
        private readonly IProcessService _ProcessService;
        private readonly IDocumentService _DocumentService;

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public DocumentBaseController(IDocumentBaseService Service, IDepartmentService departmentService, IEmplService emplService, IProcessService processService, ICompanyService companyService, IAccountService accountService, IDocumentService documentService)
            : base(companyService, accountService)
        {
            _Service = Service;
            _DepartmentService = departmentService;
            _EmplService = emplService;
            _ProcessService = processService;
            _DocumentService = documentService;

            ApplicationDbContext dbContext = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
        }

        public ActionResult IndexRequest()
        {
            RequestBaseView requestBaseView = new RequestBaseView();
            
            requestBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            requestBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(requestBaseView);
        }

        public ActionResult IndexOfficeMemo()
        {
            OfficeMemoBaseView officeMemoBaseView = new OfficeMemoBaseView();

            officeMemoBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            officeMemoBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(officeMemoBaseView);
        }

        public ActionResult IndexTask()
        {
            TaskBaseView tastBaseView = new TaskBaseView();

            tastBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            tastBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(tastBaseView);
        }

        public ActionResult IndexOrder()
        {
            OrderBaseView orderBaseView = new OrderBaseView();

            orderBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            orderBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(orderBaseView);
        }

        public ActionResult IndexIncoming()
        {
            IncomingBaseView incomingBaseView = new IncomingBaseView();

            incomingBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            incomingBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(incomingBaseView);
        }

        public ActionResult IndexOutcoming()
        {
            OutcomingBaseView outcomingBaseView = new OutcomingBaseView();

            outcomingBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            outcomingBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(outcomingBaseView);
        }

        public ActionResult IndexAppeal()
        {
            AppealBaseView appealBaseView = new AppealBaseView();

            appealBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            appealBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(appealBaseView);
        }
        public ActionResult IndexProtocol()
        {
            ProtocolBaseView protocolBaseView = new ProtocolBaseView();
            ViewBag.ProcessList = _ProcessService.GetProtocolListProcess();
            protocolBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            protocolBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(protocolBaseView);
        }

        [HttpPost]
        public ActionResult Search(DocumentType documentType, int filterType, DateTime? startDate, DateTime? endDate, Guid? processTableId)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.FilterType = filterType;
            switch (documentType)
            {
                case DocumentType.Request:
                    return View("_DocumentBaseRequest", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.OfficeMemo:
                    return View("_DocumentBaseOfficeMemo", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.Task:
                    if (filterType == (int)TaskFilterType.Executors)
                        return View("_DocumentBaseTask", _Service.GetAllViewUserDocumentWithExecutors(documentType, startDate, endDate));
                    else
                        return View("_DocumentBaseTask", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.Order:
                    return View("_DocumentBaseOrder", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.IncomingDoc:
                    return View("_DocumentBaseIncoming", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.OutcomingDoc:
                    return View("_DocumentBaseOutcoming", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.AppealDoc:
                    return View("_DocumentBaseAppeal", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.Protocol:
                    List<DocumentBaseView> docBaseView, docBaseViewProlong;                   
                    ApplicationDbContext dbContext = new ApplicationDbContext();
                    DocumentTable docTable;
                    ProtocolTaskDocumentBaseStatus Status;
                    ProtocolProlongTaskDocumentBaseStatus prolongStatus;

                    List<DocumentBaseProtocolTasksView> protocolTasks;
                    List<DocumentBaseProtocolProlongTasksView> protocolProlongTasks;

                    switch ((ProtocolFilterType)filterType)
	                {                 
                        case ProtocolFilterType.Folder:
                            List<Guid> uniqueListProtocolFolders = new List<Guid>();
                            List<ProtocolFoldersTable> protocolFolders = dbContext.ProtocolFoldersTable.Where(x => x.ProcessTableId == processTableId).ToList();
                            docBaseView = _Service.GetAllViewUserDocument(documentType, startDate, endDate).Where(x => x.ProcessTableId == processTableId).ToList();
                            List<DocumentBaseProtocolFolderView> documentBaseProtocolFolder = new List<DocumentBaseProtocolFolderView>();
                            foreach (var item in docBaseView)
                            {
                                if (item.ProtocolFolderId != null)
                                    uniqueListProtocolFolders = uniqueListProtocolFolders.Concat(_Service.GetParentListFolders(item.ProtocolFolderId)).Distinct().ToList();
                            }

                            foreach (var protocolId in uniqueListProtocolFolders)
                            {
                                ProtocolFoldersTable protocolFoldersTable = protocolFolders.FirstOrDefault(x => x.Id == protocolId);
                                List<DocumentBaseView> protocolFolderDocumentBases = new List<DocumentBaseView>();
                                foreach (var doc in docBaseView.Where(x => x.ProtocolFolderId == protocolId))
                                {
                                    protocolFolderDocumentBases.Add(doc);
                                }
                                documentBaseProtocolFolder.Add(new DocumentBaseProtocolFolderView { ProtocolFoldersId = protocolId, ProtocolFoldersParentId = protocolFoldersTable.ProtocolFoldersParentId, ProtocolFolderName = protocolFoldersTable.ProtocolFolderName, documentBaseList = protocolFolderDocumentBases });
                            }
                            return View("_DocumentBaseProtocolFolders", documentBaseProtocolFolder);
                        case ProtocolFilterType.TaskStatus:                                                
                        case ProtocolFilterType.TaskExecutor:
                             protocolTasks = new List<DocumentBaseProtocolTasksView>();
                             docBaseView = _Service.GetAllViewUserDocumentWithExecutors(DocumentType.Task, startDate, endDate);
                             foreach (var doc in docBaseView)
                             {
                                USR_TAS_DailyTasks_Table task = dbContext.USR_TAS_DailyTasks_Table.FirstOrDefault(x => x.DocumentTableId == doc.Id);

                                docTable = _DocumentService.Find(task.RefDocumentId);

                                if (docTable != null && docTable.DocType == DocumentType.Protocol)
                                {
                                    if (doc.DocumentState == DocumentState.Closed || doc.DocumentState == DocumentState.Cancelled)
                                        Status = ProtocolTaskDocumentBaseStatus.Executed;
                                    else if ((task.ProlongationDate != null && task.ProlongationDate < DateTime.UtcNow) ||
                                        (task.ProlongationDate == null && task.ExecutionDate < DateTime.UtcNow))
                                        Status = ProtocolTaskDocumentBaseStatus.Overdue;
                                    else
                                        Status = ProtocolTaskDocumentBaseStatus.AtWork;

                                    var protocolTable = _DocumentService.GetDocument(docTable.RefDocumentId, docTable.ProcessTable.TableName);
                                    if (protocolTable.Subject != null)
                                    {
                                        protocolTasks.Add(new DocumentBaseProtocolTasksView { DocumentNum = doc.DocumentNum, CreatedDate = doc.CreatedDate, CreateTaskDate = doc.CreatedDate.ToShortDateString(), TaskStatus = Status, DepartmentName = doc.DepartmentName, Id = doc.Id, ProtocolNum = protocolTable.Subject, UserName = doc.UserName });
                                    }
                                }
                             }
                            return View("_DocumentBaseProtocolTasks", protocolTasks);		                
                        case ProtocolFilterType.ProlongTaskExecutor:
                        case ProtocolFilterType.ProlongTaskStatus:
                        case ProtocolFilterType.ProlongTaskChairman:
                            ProcessTable process = _ProcessService.FirstOrDefault(x => x.TableName == "USR_TAS_DailyTasksProlongation" && x.CompanyTableId == user.CompanyTableId);
                            protocolProlongTasks = new List<DocumentBaseProtocolProlongTasksView>();

                            docBaseView = _Service.GetAllViewUserDocument(DocumentType.Task, startDate, endDate);
                            if ((ProtocolFilterType)filterType == ProtocolFilterType.ProlongTaskChairman)
                            {
                                List<EmplTable> emplTables = _EmplService.GetAll().ToList();
                                docBaseViewProlong = _Service.GetAllViewUserDocumentWithExecutors(DocumentType.Request, startDate, endDate, process.Id).Where(x => x.TrackerActivityName == "Председатель").ToList(); ;
                            }
                            else
                            {                           
                                docBaseViewProlong = _Service.GetAllViewUserDocumentWithExecutors(DocumentType.Request, startDate, endDate, process.Id);
                            }

                            foreach (var doc in docBaseView)
                            {
                                USR_TAS_DailyTasks_Table task = dbContext.USR_TAS_DailyTasks_Table.FirstOrDefault(x => x.DocumentTableId == doc.Id);
                           
                                docTable = _DocumentService.Find(task.RefDocumentId);

                                List<USR_TAS_DailyTasksProlongation_Table> taskProlong = dbContext.USR_TAS_DailyTasksProlongation_Table.Where(x => x.RefDocumentId == doc.Id).ToList();

                                if (docTable != null && docTable.DocType == DocumentType.Protocol && taskProlong.Count() > 0)
                                {
                                    foreach (var prolong in taskProlong.Where(x => x.DocumentTableId != null && docBaseViewProlong.Any(y => y.Id == x.DocumentTableId)))
                                    {
                                        docBaseViewProlong.Where(y => y.Id == prolong.DocumentTableId).ToList().ForEach(x =>
                                        {
                                            
                                            if (x.DocumentState == DocumentState.Closed || doc.DocumentState == DocumentState.Cancelled)
                                                prolongStatus = ProtocolProlongTaskDocumentBaseStatus.Executed;
                                            else
                                                prolongStatus = ProtocolProlongTaskDocumentBaseStatus.AtWork;

                                            protocolProlongTasks.Add(new DocumentBaseProtocolProlongTasksView { DocumentNum = x.DocumentNum, CreatedDate = x.CreatedDate, CreateProlongTaskDate = x.CreatedDate.ToShortDateString(),  TaskStatus = prolongStatus, DepartmentName = x.DepartmentName, Id = x.Id, UserName = x.UserName});
                                        });
                                    }
                                }
                            }
                            return View("_DocumentBaseProtocolProlongTasks", protocolProlongTasks);
                        default:
                            return View("_DocumentBaseProtocol", _Service.GetAllViewUserDocument(documentType, startDate, endDate).Where(x => x.ProcessTableId == processTableId).ToList());
	                }
                  
            }
            return new EmptyResult();
        }       
    }
}