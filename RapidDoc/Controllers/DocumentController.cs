using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.DomainModels;
using RapidDoc.Filters;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Simple.ImageResizer;
using RapidDoc.Models.Grids;
using System.Runtime.Remoting;
using System.Reflection;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using RapidDoc.Attributes;
using System.Configuration;
using Rotativa;
using Rotativa.Options;
using System.Collections;
using System.Net;
using RapidDoc.Extensions;
using AutoMapper;


namespace RapidDoc.Controllers
{
    [ValidateInput(false)]
    public class DocumentController : BasicController
    {
        private readonly IDocumentService _DocumentService;
        private readonly IProcessService _ProcessService;
        private readonly IWorkflowService _WorkflowService;
        private readonly IEmplService _EmplService;
        private readonly ISystemService _SystemService;
        private readonly IWorkflowTrackerService _WorkflowTrackerService;
        private readonly IReviewDocLogService _ReviewDocLogService;
        private readonly IDocumentReaderService _DocumentReaderService;
        private readonly ICommentService _CommentService;
        private readonly IEmailService _EmailService;
        private readonly IHistoryUserService _HistoryUserService;
        private readonly ISearchService _SearchService;
        private readonly ICustomCheckDocument _CustomCheckDocument;
        private readonly IItemCauseService _ItemCauseService;
        private readonly IModificationUsersService _ModificationUsersService;
        private readonly INotificationUsersService _NotificationUsersService;
        private readonly IDocumentSubcriptionService _DocumentSubcriptionService;

        protected UserManager<ApplicationUser> UserManager { get; private set; }
        protected RoleManager<ApplicationRole> RoleManager { get; private set; }

        public DocumentController(IDocumentService documentService, IProcessService processService, 
            IWorkflowService workflowService, IEmplService emplService, IAccountService accountService, ISystemService systemService,
            IWorkflowTrackerService workflowTrackerService, IReviewDocLogService reviewDocLogService,
            IDocumentReaderService documentReaderService, ICommentService commentService, IEmailService emailService,
            IHistoryUserService historyUserService, ISearchService searchService, ICompanyService companyService, ICustomCheckDocument customCheckDocument, IItemCauseService itemCauseService, IModificationUsersService modificationUsers, INotificationUsersService notificationUsersService, IDocumentSubcriptionService documentSubcriptionService)
            : base(companyService, accountService)
        {
            _DocumentService = documentService;
            _ProcessService = processService;
            _WorkflowService = workflowService;
            _EmplService = emplService;
            _SystemService = systemService;
            _WorkflowTrackerService = workflowTrackerService;
            _ReviewDocLogService = reviewDocLogService;
            _DocumentReaderService = documentReaderService;
            _CommentService = commentService;
            _EmailService = emailService;
            _HistoryUserService = historyUserService;
            _SearchService = searchService;
            _CustomCheckDocument = customCheckDocument;
            _ItemCauseService = itemCauseService;
            _ModificationUsersService = modificationUsers;
            _NotificationUsersService = notificationUsersService;
            _DocumentSubcriptionService = documentSubcriptionService;

            ApplicationDbContext dbContext = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(dbContext));
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ArchiveDocuments()
        {
            return View();
        }

        public ActionResult AgreedDocuments()
        {
            return View();
        }

        public ActionResult MyDocuments()
        {
            return View();
        }

        public ActionResult MyFavorite()
        {
            return View();
        }

        public ActionResult MyTasks()
        {
            return View();
        }

        public ActionResult GetAllDocument()
        {
            var grid = new DocumentAjaxPagingGrid(_DocumentService.GetAllView(), 1, false, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);
            return PartialView("~/Views/Document/DocumentList.cshtml", grid);
        }

        public ActionResult GetArchiveDocument()
        {
            var grid = new DocumentAjaxPagingGrid(_DocumentService.GetArchiveView(), 1, false, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);
            return PartialView("~/Views/Document/DocumentList.cshtml", grid);
        }

        public ActionResult GetAllAgreedDocument()
        {
            var grid = new AgreedDocumentAjaxPagingGrid(_DocumentService.GetAgreedDocument(), 1, false, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);
            return PartialView("~/Views/Document/DocumentList.cshtml", grid);
        }

        public ActionResult GetAllMyDocument()
        {
            var grid = new MyDocumentAjaxPagingGrid(_DocumentService.GetMyDocumentView(), 1, false, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);
            return PartialView("~/Views/Document/DocumentList.cshtml", grid);
        }

        public ActionResult GetAllMyFavorite()
        {
            var grid = new MyFavoriteAjaxPagingGrid(_DocumentService.GetMyFavoriteView(), 1, false, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);
            return PartialView("~/Views/Document/DocumentList.cshtml", grid);
        }

        public ActionResult GetMyTaskDocument()
        {
            var grid = new MyTaskAjaxPagingGrid(_DocumentService.GetTaskDocumentView(), 1, false, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);
            return PartialView("~/Views/Document/DocumentTaskList.cshtml", grid);
        }

        public JsonResult GetDocumentList(int page)
        {
            var grid = new DocumentAjaxPagingGrid(_DocumentService.GetAllView(), page, true, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);

            return Json(new
            {
                Html = RenderPartialViewToString("DocumentList", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetArchiveDocumentList(int page)
        {
            var grid = new DocumentAjaxPagingGrid(_DocumentService.GetArchiveView(), page, true, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);

            return Json(new
            {
                Html = RenderPartialViewToString("DocumentList", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAgreedDocumentList(int page)
        {
            var grid = new AgreedDocumentAjaxPagingGrid(_DocumentService.GetAgreedDocument(), page, true, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);

            return Json(new
            {
                Html = RenderPartialViewToString("DocumentList", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMyDocumentList(int page)
        {
            var grid = new MyDocumentAjaxPagingGrid(_DocumentService.GetMyDocumentView(), page, true, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);

            return Json(new
            {
                Html = RenderPartialViewToString("DocumentList", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMyFavoriteList(int page)
        {
            var grid = new MyFavoriteAjaxPagingGrid(_DocumentService.GetMyFavoriteView(), page, true, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);

            return Json(new
            {
                Html = RenderPartialViewToString("DocumentList", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMyTaskList(int page)
        {
            var grid = new MyTaskAjaxPagingGrid(_DocumentService.GetTaskDocumentView(), page, true, _ReviewDocLogService, _DocumentService, _AccountService, _SearchService, _EmplService);

            return Json(new
            {
                Html = RenderPartialViewToString("DocumentTaskList", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowDocument(Guid id, bool isAfterView = false)
        {
            DocumentTable documentTable = _DocumentService.Find(id);
            ApplicationUser currentUser = _AccountService.Find(User.Identity.GetUserId());

            if (documentTable == null || currentUser == null)
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            if (documentTable.DocumentState == DocumentState.Created)
            {
                if (documentTable.ApplicationUserCreatedId == currentUser.Id || UserManager.IsInRole(currentUser.Id, "Administrator") || _ModificationUsersService.ContainDocumentUser(id, currentUser.Id))
                    return RedirectToAction("ShowDraft", "Document", new { id = id });
                else
                    return RedirectToAction("WithDrawnDocument", "Error");
            }

            var previousModelState = TempData["ModelState"] as ModelStateDictionary;
            if (previousModelState != null)
            {
                foreach (KeyValuePair<string, ModelState> kvp in previousModelState)
                    if (!ModelState.ContainsKey(kvp.Key))
                        ModelState.Add(kvp.Key, kvp.Value);
            }

            DocumentView docuView = _DocumentService.Document2View(documentTable);
            ProcessView process = _ProcessService.FindView(documentTable.ProcessTableId);
            EmplTable emplTable = _EmplService.GetEmployer(docuView.ApplicationUserCreatedId, docuView.CompanyTableId);

            if (docuView == null || process == null || emplTable == null || _DocumentService.isShowDocument(documentTable, currentUser, isAfterView) == false)
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            Task.Run(() =>
            {
                IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = id }, "", currentUser);
            });
            if (_NotificationUsersService.ContainDocumentUser(id, currentUser.Id))
            {
                foreach (var item in _NotificationUsersService.GetPartial(x => x.FromUserId == currentUser.Id && x.DocumentTableId == id))
                {
                    _EmailService.SendNotificationForUserEmail(id, item.ToUserId);                     
                }
                _NotificationUsersService.SetNotifyForUser(id, currentUser.Id);
            }
            object viewModel = InitialViewShowDocument(documentTable, process, docuView, currentUser, emplTable);
            return View(viewModel);
        }
        
        [HttpPost]
        public ActionResult ShowDocument(Guid id, OperationType operationType, IDictionary<string, object> documentData, ProcessView process, Guid processId)
        {
            DocumentTable docuTable = _DocumentService.Find(id);
            if (docuTable == null) return RedirectToAction("PageNotFound", "Error");

            ApplicationUser currentUser = _AccountService.Find(User.Identity.GetUserId());
            if (currentUser == null) return RedirectToAction("PageNotFound", "Error");

            if (operationType == OperationType.RejectDocument && process.DocType != DocumentType.OfficeMemo)
            {
                DateTime checkRejectDate = DateTime.UtcNow.AddMinutes(-5);
                HistoryUserTable history = _HistoryUserService.FirstOrDefault(x => x.DocumentTableId == id && x.HistoryType == Models.Repository.HistoryType.CancelledDocument);
                if (history != null && history.CreatedDate > checkRejectDate)
                {
                    checkRejectDate = history.CreatedDate;
                }

                if (!_CommentService.Contains(x => x.ApplicationUserCreatedId == currentUser.Id && x.DocumentTableId == id && x.CreatedDate >= checkRejectDate))
                {
                    ModelState.AddModelError(string.Empty, UIElementRes.UIElement.RejectReason);
                }
            }

            if (ModelState.IsValid)
            {
                if (_DocumentService.isSignDocument(id, currentUser))
                {
                    if (operationType == OperationType.ApproveDocument)
                    {
                        _WorkflowService.AgreementWorkflowApprove(id, process.TableName, docuTable.WWFInstanceId, processId, documentData);

                        DocumentTable documentTable = _DocumentService.Find(id);
                        if (documentTable != null && documentTable.ProcessTable != null && documentTable.DocumentState == DocumentState.Closed && !String.IsNullOrEmpty(documentTable.ProcessTable.AfterEndReaderRoleId))
                        {
                            try
                            {
                                var role = RoleManager.FindById(documentTable.ProcessTable.AfterEndReaderRoleId);
                                if (role != null && role.Users != null && role.Users.Count > 0)
                                {
                                    List<string> newReader = _DocumentReaderService.AddReader(documentTable.Id, role.Users.ToList());
                                    _EmailService.SendReaderEmail(documentTable.Id, newReader);
                                }
                            }
                            catch { }
                        }
                    }
                    else if (operationType == OperationType.RejectDocument)
                    {
                        _WorkflowService.AgreementWorkflowReject(id, process.TableName, docuTable.WWFInstanceId, processId, documentData);
                    }
                }
                return RedirectToAction("Index", "Document");
            }

            DocumentView docuView = _DocumentService.FindView(id);
            EmplTable emplResult = _EmplService.GetEmployer(docuView.ApplicationUserCreatedId, docuView.CompanyTableId);
            object viewModelResult = InitialViewShowDocument(docuTable, process, docuView, currentUser, emplResult);
            return View("~/Views/Document/ShowDocument.cshtml", viewModelResult);
        }

        private object InitialViewShowDocument(DocumentTable documentTable, ProcessView process, DocumentView docuView, ApplicationUser userTable, EmplTable emplTable)
        {
            var viewModel = new DocumentComposite();
            viewModel.ProcessView = process;

            docuView.isSign = _DocumentService.isSignDocument(documentTable.Id, userTable);
            docuView.isArchive = _ReviewDocLogService.isArchive(documentTable.Id, "", userTable);
            docuView.isFavorite = _ReviewDocLogService.isFavorite(documentTable.Id, "", userTable);
            viewModel.DocumentView = docuView;
            viewModel.docData = _DocumentService.GetDocumentView(documentTable.RefDocumentId, process.TableName);
            viewModel.fileId = docuView.FileId;           
            ViewBag.CreatedDate = _SystemService.ConvertDateTimeToLocal(userTable, docuView.CreatedDate);
            ViewBag.DocumentUrl = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + docuView.AliasCompanyName + "/Document/ShowDocument/" + docuView.Id + "?isAfterView=true";
            ViewBag.CountSubscribers = _DocumentSubcriptionService.GetPartial(x => x.DocumentTableId == documentTable.Id).Count();
            if (emplTable != null)
            {
                ViewBag.Initiator = emplTable.ApplicationUserId != null ? emplTable.FullName : docuView.ApplicationUserCreatedId;
                ViewBag.TitleName = emplTable.TitleTableId != null ? emplTable.TitleTable.TitleName : String.Empty;
                ViewBag.DepartmentName = emplTable.DepartmentTableId != null ? emplTable.DepartmentTable.DepartmentName : String.Empty;
                ViewBag.CompanyName = emplTable.CompanyTableId != null ? emplTable.CompanyTable.AliasCompanyName : String.Empty;
            }
            else
            {
                ViewBag.Initiator = String.Empty;
                ViewBag.TitleName = String.Empty;
                ViewBag.DepartmentName = String.Empty;
                ViewBag.CompanyName = String.Empty;
            }

            ModificationUsersTable modificationUser = _ModificationUsersService.FirstOrDefault(x => x.DocumentTableId == documentTable.Id && x.OriginalDocumentId != null);
            if (modificationUser != null)

                ViewBag.ModificationUser = _EmplService.FirstOrDefault(x => x.ApplicationUserId == modificationUser.UserId).FullName;
            else
                ViewBag.ModificationUser = String.Empty;

            ViewBag.RejectHistory = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.CancelledDocument);
            ViewBag.AddReaders = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.AddReader);
            ViewBag.RemoveReaders = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.RemoveReader);

            return viewModel;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ApproveNewDocument")]
        public ActionResult ApproveNewDocument(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName)
        {
            var view = PostDocument(processId, type, OperationType.ApproveDocument, null, fileId, collection, actionModelName);
            return view;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ApproveDocument")]
        public ActionResult ApproveDocument(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            var view = PostDocument(processId, type, OperationType.ApproveDocument, documentId, fileId, collection, actionModelName);
            return view;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RejectDocument")]
        public ActionResult RejectDocument(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            var view = PostDocument(processId, type, OperationType.RejectDocument, documentId, fileId, collection, actionModelName);
            return view;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveDraft")]
        public ActionResult SaveDraft(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid? documentId)
        {
            var view = PostDocument(processId, type, OperationType.SaveDraft, documentId, fileId, collection, actionModelName);
            return view;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SendOnRework")]
        public ActionResult SendOnRework(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            string[] users = _DocumentService.GetUserListFromStructure(collection["ReworkUsers"]);
            foreach (string user in users)
            {
                EmplTable empluser = _EmplService.FirstOrDefault(x => x.Id == new Guid(user) && x.Enable == true);
                if (!_ModificationUsersService.ContainDocumentUser(documentId, empluser.ApplicationUserId) && empluser != null && empluser.ApplicationUserId != User.Identity.GetUserId())
	            {
                    ModificationUsersTable modificationUsers = new ModificationUsersTable();
                    modificationUsers.DocumentTableId = documentId;
                    modificationUsers.UserId = empluser.ApplicationUserId;
                    _ModificationUsersService.SaveDomain(modificationUsers);

                    DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == documentId);
                    docTable.ActivityName = "На доработке";
                    _DocumentService.UpdateDocument(docTable, User.Identity.GetUserId());

                    _EmailService.SendNewModificationUserEmail(documentId, empluser.ApplicationUserId, collection["AdditionalTextRework"] != null | collection["AdditionalTextRework"] != string.Empty ? collection["AdditionalTextRework"] : "");
	            }
                
            }
            return RedirectToAction("Index", "Document");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveReworkDocument")]
        public ActionResult SaveReworkDocument(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {           
            ApplicationUser userTableCurrent = _AccountService.Find(User.Identity.GetUserId());
            DocumentTable documentTable = _DocumentService.FirstOrDefault(x => x.Id == documentId);

            if (_ModificationUsersService.FirstOrDefault(x => x.UserId == userTableCurrent.Id && x.DocumentTableId == documentTable.Id).OriginalDocumentId != null)
                return PostDocument(processId, type, OperationType.SaveDraft, documentId, fileId, collection, actionModelName);

            ApplicationUser userTablePrev = _AccountService.Find(documentTable.ApplicationUserCreatedId);
            if (userTableCurrent == null) return RedirectToAction("PageNotFound", "Error");

            EmplTable emplTable = _EmplService.FirstOrDefault(x => x.ApplicationUserId == userTablePrev.Id && x.Enable == true);
            if (emplTable == null) return RedirectToAction("PageNotFound", "Error");

            ProcessView process = _ProcessService.FindView(processId);
            var documentIdNew = _DocumentService.SaveDocument(_DocumentService.GetDocumentView(documentTable.RefDocumentId, process.TableName), process.TableName, GuidNull2Guid(process.Id), fileId, userTablePrev);

            DateTime date = DateTime.UtcNow;
            DateTime startTime = new DateTime(date.Year, date.Month, date.Day) + process.StartWorkTime;
            DateTime endTime = new DateTime(date.Year, date.Month, date.Day) + process.EndWorkTime;
            if ((startTime > date || date > endTime) && process.StartWorkTime != process.EndWorkTime) return RedirectToAction("PageNotFound", "Error");

            if (!String.IsNullOrEmpty(process.RoleId))
            {
                string roleName = RoleManager.FindById(process.RoleId).Name;
                if (!UserManager.IsInRole(userTableCurrent.Id, roleName))
                {
                    return RedirectToAction("PageNotFound", "Error");
                }
            }
            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentIdNew, HistoryType = Models.Repository.HistoryType.ModifiedDocument }, User.Identity.GetUserId());

            var view = PostDocument(processId, type, OperationType.SaveDraft, documentIdNew, fileId, collection, actionModelName);
            
            ModificationUsersTable modificationUsers = new ModificationUsersTable();
            modificationUsers.DocumentTableId = documentIdNew;
            modificationUsers.UserId = userTableCurrent.Id;
            modificationUsers.OriginalDocumentId = documentId;
            _ModificationUsersService.SaveDomain(modificationUsers);

            Guid newDocGuid = documentIdNew;
            DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == newDocGuid);
            docTable.ActivityName = "Доработан";
            _DocumentService.UpdateDocument(docTable, User.Identity.GetUserId());
            documentTable.ActivityName = "";
            _DocumentService.UpdateDocument(documentTable, User.Identity.GetUserId());

            _EmailService.SendNoteReadyModificationUserEmail(documentIdNew, _DocumentService.Find(documentId).ApplicationUserCreatedId);

            return RedirectToAction("ShowDraft", "Document", new { id = documentIdNew });
        }

        public ActionResult GetModificationsList(Guid documentId)
        {
            string currentUserId = User.Identity.GetUserId();
            Guid? parentDocId = _ModificationUsersService.GetParentDocument(documentId);
            DocumentTable  documentParentTable = _DocumentService.FirstOrDefault(x => x.Id == parentDocId);

            List<ModificationDocumentView> hierarchyModification = new List<ModificationDocumentView>();
            hierarchyModification.Add(new ModificationDocumentView { DocumentId = parentDocId, DocumentNum = documentParentTable.DocumentNum, ParentDocumentId = null, Name = _EmplService.FirstOrDefault(x => x.ApplicationUserId == documentParentTable.ApplicationUserCreatedId).FullName, CreateDateTime = documentParentTable.CreatedDate, Enable = currentUserId == documentParentTable.ApplicationUserCreatedId || _ModificationUsersService.ContainDocumentUser(parentDocId, currentUserId) ? true : false, NamesTo = _ModificationUsersService.GetModificationUserNamesFromDocument(parentDocId, documentParentTable.ApplicationUserCreatedId) });
            hierarchyModification.AddRange(_ModificationUsersService.GetHierarchyModification(parentDocId));
            return PartialView("~/Views/Document/_ModificationDocumentList.cshtml", hierarchyModification); 
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteDraft")]
        public ActionResult DeleteDraft(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            ProcessView processView = _ProcessService.FindView(processId);
            DocumentTable documentTable = _DocumentService.FirstOrDefault(x => x.Id == documentId);
            SearchTable searchTable = _SearchService.FirstOrDefault(x => x.DocumentTableId == documentId);

            if (documentTable.ApplicationUserCreatedId == User.Identity.GetUserId() && documentTable.DocumentState == DocumentState.Created)
            {
                _DocumentService.DeleteFiles(documentId);
                if (searchTable != null)
                    _SearchService.Delete(searchTable.Id);
                _CommentService.DeleteAll(documentId);
                _DocumentService.DeleteDocumentDraft(documentId, processView.TableName, documentTable.RefDocumentId);
                _ReviewDocLogService.DeleteAll(documentId);
                _HistoryUserService.DeleteAll(documentId);
                _DocumentReaderService.Delete(documentId);
                _ModificationUsersService.DeleteAll(documentId);
                _DocumentService.Delete(documentId);
                return RedirectToAction("Index", "Document");
            }
            else
                return RedirectToAction("PageNotFound", "Error");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WithdrawDocument")]
        public ActionResult WithdrawDocument(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            DocumentTable documentTable = _DocumentService.Find(documentId);
            if (documentTable.ApplicationUserCreatedId != User.Identity.GetUserId() && !(User.IsInRole("Administrator") || User.IsInRole("SetupAdministrator")))
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            ProcessTable process = _ProcessService.Find(processId);
            _WorkflowService.AgreementWorkflowWithdraw(documentId, process.TableName, documentTable.WWFInstanceId, processId);
            _NotificationUsersService.DeleteAll(documentId);
            var view = ShowDraft(documentId);
            return view;
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ApproveDocumentCZ")]
        public ActionResult ApproveDocumentCZ(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            string comment = String.Empty;
            if(collection["ApproveComment"] != null && _SystemService.CheckTextExists(collection["ApproveComment"]))
            {
                comment = collection["ApproveComment"];
            }

            var users = _DocumentService.SignDocumentCZ(documentId, TrackerType.Approved, comment);

            DocumentTable documentTable = _DocumentService.Find(documentId);
            _DocumentService.UpdateDocument(documentTable, User.Identity.GetUserId());

            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.ApproveDocument }, User.Identity.GetUserId());

            foreach (var userid in users)
            {
                string[] arrayStructrue = userid.Split('|');
                _EmailService.SendNewExecutorEmail(documentId, arrayStructrue[0], arrayStructrue[1] != null | arrayStructrue[1] != string.Empty ? arrayStructrue[1] : "");
            }
            _EmailService.SendInitiatorEmailSignCZ(documentId, User.Identity.GetUserId());
            return RedirectToAction("Index", "Document");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RejectDocumentCZ")]
        public ActionResult RejectDocumentCZ(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());

            string comment = String.Empty;
            if (collection["RejectComment"] != null && _SystemService.CheckTextExists(collection["RejectComment"]))
            {
                comment = collection["RejectComment"];
            }
            else
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            var users = _DocumentService.SignDocumentCZ(documentId, TrackerType.Cancelled, comment);         

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DocumentTable documentTable = _DocumentService.Find(documentId);
            _DocumentService.UpdateDocument(documentTable, user.Id);

            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.CancelledDocument }, User.Identity.GetUserId());

            _EmailService.SendInitiatorRejectEmail(documentId);
            return RedirectToAction("Index", "Document");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AddUsersDocumentCZ")]
        public ActionResult AddUsersDocumentCZ(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            string currentUserId = User.Identity.GetUserId();
            IDictionary<string, object> documentData = new Dictionary<string, object>();

            if ((collection["IsParallel"] != null && collection["Flow"] != null) && collection["Flow"] != String.Empty)
            {
                documentData.Add("IsParallel", collection["IsParallel"].ToLower().Contains("true"));
                documentData.Add("Flow", collection["Flow"]);

                List<string> users = _WorkflowService.GetUniqueUserList(documentId, documentData, "Flow");

                if(users.Count > 0)
                    _WorkflowService.CreateDynamicTracker(users, documentId, currentUserId, (bool)documentData["IsParallel"], (collection["AdditionaltextCZ"] != null | collection["AdditionaltextCZ"] != string.Empty) ? (string)collection["AdditionaltextCZ"] : "");

                DocumentTable documentTable = _DocumentService.Find(documentId);
                if (collection["IsNotifyCZ"].ToLower().Contains("true") == true)
                {
                    foreach (var user in users)
                    {
                        _NotificationUsersService.CreateNotifyForUser(documentId, currentUserId, user);
                    }
                }
                _DocumentService.UpdateDocument(documentTable, currentUserId);
            }

            return RedirectToAction("ShowDocument", new { id = documentId, isAfterView = true });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DelegateDocumentTask")]
        public ActionResult DelegateDocumentTask(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            string currentUserId = User.Identity.GetUserId();
            IDictionary<string, object> documentData = new Dictionary<string, object>();
            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();

            if (collection["Flow"] != null && collection["Flow"] != String.Empty)
            {
                documentData.Add("Flow", collection["Flow"]);

                List<string> users = _WorkflowService.GetUniqueUserList(documentId, documentData, "Flow");
                if (users.Count > 0)
                {
                    _WorkflowTrackerService.SaveTrackList(documentId, new List<Array> { new string[] { "Исполнитель", "", "" } });
                    users.ForEach(x => userList.Add(new WFTrackerUsersTable { UserId = x }));

                    WFTrackerTable activeTrackerTable = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.Waiting);
                    WFTrackerTable trackerTable = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == documentId && x.TrackerType == TrackerType.NonActive);
                    trackerTable.Users = userList;
                    trackerTable.TrackerType = TrackerType.Waiting;
                    trackerTable.StartDateSLA = activeTrackerTable.StartDateSLA;
                    trackerTable.SLAOffset = activeTrackerTable.SLAOffset;
                    _WorkflowTrackerService.SaveDomain(trackerTable, currentUserId);
                }   

                DocumentTable documentTable = _DocumentService.Find(documentId);
                _DocumentService.UpdateDocument(documentTable, currentUserId);

                if (collection["IsNotifyTask"].ToLower().Contains("true") == true)
                {
                    foreach (var user in users)
                    {
                        _NotificationUsersService.CreateNotifyForUser(documentId, currentUserId, user);
                    }
                }

                _EmailService.SendNewExecutorEmail(documentId, users);

                _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.DelegateTask }, User.Identity.GetUserId());
            }

            return RedirectToAction("ShowDocument", new { id = documentId, isAfterView = true }); 
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ApproveDocumentTask")]
        public ActionResult ApproveDocumentTask(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            string currentUserId = User.Identity.GetUserId();
            string reminderChairmanUser = String.Empty;
            ProcessView process = _ProcessService.FindView(processId);

            _DocumentService.SignTaskDocument(documentId, TrackerType.Approved);
            var documentIdNew = _DocumentService.GetDocumentView(_DocumentService.Find(documentId).RefDocumentId, process.TableName);
            if (collection["ApproveCommentTask"] != null && _SystemService.CheckTextExists(collection["ApproveCommentTask"]))
            {
                string approveCommentRequest = collection["ApproveCommentTask"].ToString();                
                documentIdNew.ReportText = approveCommentRequest;
                _DocumentService.UpdateDocumentFields(documentIdNew, process);
            }
            else
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            string userId = User.Identity.GetUserId();
            DocumentTable documentTable = _DocumentService.Find(documentId);
            documentTable.DocumentState = DocumentState.Closed;
            _DocumentService.UpdateDocument(documentTable, userId);

            if(collection["RefDocumentId"] != null)
            {
                Guid sourceDocumentId = Guid.Parse(collection["RefDocumentId"]);
                DocumentTable documentSourceTable = _DocumentService.Find(sourceDocumentId);

                if (documentSourceTable != null)
                {
                    if ((documentSourceTable.DocType == DocumentType.Order || documentSourceTable.DocType == DocumentType.IncomingDoc || documentSourceTable.DocType == DocumentType.AppealDoc) && documentSourceTable.Executed == false)
                    {
                        var sourceDocumentData = _DocumentService.GetDocumentView(documentSourceTable.RefDocumentId, documentSourceTable.ProcessTable.TableName);
                        sourceDocumentData.Executed = true;
                        var processSourceView = _ProcessService.FindView(documentSourceTable.ProcessTable.Id);
                        _DocumentService.UpdateDocumentFields(sourceDocumentData, processSourceView);

                        documentSourceTable.Executed = true;
                        _DocumentService.UpdateDocument(documentTable, userId);
                    }
                    else if (documentSourceTable.DocType == DocumentType.Protocol)
                    {
                        var sourceDocumentData = _DocumentService.GetDocumentView(documentSourceTable.RefDocumentId, documentSourceTable.ProcessTable.TableName);
                        string[] chairman = _SystemService.GuidsFromText(sourceDocumentData.Chairman);

                        if (chairman.Count() > 0 && !String.IsNullOrEmpty(chairman[0]))
                        {
                            EmplTable chairmanEmpl = _EmplService.Find(Guid.Parse(chairman[0]), currentUserId);
                            reminderChairmanUser = chairmanEmpl.ApplicationUserId;
                        }
                    }
                }
            }

            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.ApproveDocument }, User.Identity.GetUserId());
            if (documentIdNew.RefDocumentId != null)
                _EmailService.SendUsersClosedEmail(documentTable.Id, new List<string> { documentTable.ApplicationUserCreatedId, _DocumentService.Find(documentIdNew.RefDocumentId).ApplicationUserCreatedId, reminderChairmanUser });
            else
                _EmailService.SendInitiatorClosedEmail(documentTable.Id);

            return RedirectToAction("ShowDocument", new { id = documentId, isAfterView = true });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RejectDocumentTask")]
        public ActionResult RejectDocumentTask(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            string currentUserId = User.Identity.GetUserId();
            ProcessView process = _ProcessService.FindView(processId);
            var documentIdNew = _DocumentService.GetDocumentView(_DocumentService.Find(documentId).RefDocumentId, process.TableName);

            if (collection["RejectCommentTask"] != null && _SystemService.CheckTextExists(collection["RejectCommentTask"]))
            {
                string rejectCommentRequest = collection["RejectCommentTask"].ToString();
                documentIdNew.ReportText = rejectCommentRequest;
                _DocumentService.UpdateDocumentFields(documentIdNew, process);
            }
            else
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            DocumentTable documentTable = _DocumentService.Find(documentId);
            documentTable.WWFInstanceId = Guid.Empty;
            documentTable.DocumentState = DocumentState.Cancelled;
            documentTable.ActivityName = String.Empty;

            _DocumentService.UpdateDocument(documentTable, User.Identity.GetUserId());
           /* _DocumentService.SaveSignData(_DocumentService.GetCurrentSignStep(documentId, currentUserId).ToList(), TrackerType.Cancelled);*/
            _DocumentService.SignTaskDocument(documentId, TrackerType.Cancelled);
            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.CancelledDocument }, User.Identity.GetUserId());
            _EmailService.SendInitiatorRejectEmail(documentTable.Id);

            return RedirectToAction("Index", "Document");         
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ReturnTask")]
        public ActionResult ReturnTask(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            if (User.IsInRole("Administrator") || User.IsInRole("SetupAdministrator") || User.IsInRole("OpenDocuments"))
            {
                string currentUserId = User.Identity.GetUserId();
                ProcessView process = _ProcessService.FindView(processId);
                var documentIdNew = _DocumentService.GetDocumentView(_DocumentService.Find(documentId).RefDocumentId, process.TableName);

                List<WFTrackerTable> trackerTableList = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == documentId).ToList();

                foreach (var tracker in trackerTableList)
                {
                    tracker.TrackerType = TrackerType.Waiting;
                    tracker.SignDate = null;
                    tracker.SignUserId = null;
                    _WorkflowTrackerService.SaveDomain(tracker, currentUserId);
                }

                DocumentTable documentTable = _DocumentService.Find(documentId);
                documentTable.DocumentState = DocumentState.OnSign;
                documentTable.ActivityName = "Исполнители";
                _DocumentService.UpdateDocument(documentTable, currentUserId);

                documentIdNew.ReportText = "";
                _DocumentService.UpdateDocumentFields(documentIdNew, process);

                return RedirectToAction("Index", "Document");
            }

            return RedirectToAction("PageNotFound", "Error");
        }

        [HttpPost]
        [MultipleButton(Name= "action", Argument= "DeliveryOrder")]
        public ActionResult DeliveryOrder(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            DocumentTable documentTable = _DocumentService.Find(documentId);
            List<string> appUsers = new List<string>();
            List<FileTable> docFile = new List<FileTable>();

            if (!String.IsNullOrEmpty(collection["ReceiversOrder"]))
            {
                string[] users = _DocumentService.GetUserListFromStructure(collection["ReceiversOrder"].ToString());
                var documentModel = _DocumentService.GetDocumentView(documentTable.RefDocumentId, documentTable.ProcessTable.TableName);
                appUsers = _WorkflowService.EmplAndRolesToUserList(documentId, users);
                _DocumentSubcriptionService.SaveSubscriber(documentTable.Id, appUsers.ToArray(), User.Identity.GetUserId());
                if (collection["AddReaders"].ToLower().Contains("true") == true)
                {
                    List<string> readers = _WorkflowService.EmplAndRolesToReaders(documentId, users);
                    _DocumentReaderService.SaveOrderReader(documentTable.Id, readers.ToArray(), User.Identity.GetUserId());
                }

                if (collection["AddAttachment"].ToLower().Contains("true") == true)
                    docFile = _DocumentService.GetAllFilesDocument(documentTable.FileId).ToList();

                _EmailService.SendORDForUserEmail(documentTable.Id, appUsers, documentModel, docFile);
            }

            return RedirectToAction("ShowDocument", new { id = documentId, isAfterView = true });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PrintOrder")]
        public ActionResult PrintOrder(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            bool acquainted = false;
            if (collection["Acquainted"].ToLower().Contains("true") == true)
                acquainted = true;

            return RedirectToAction("PdfReport", "Report", new { id = documentId, processId = processId, acquainted = acquainted });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PrintPdfTrip")]
        public ActionResult PrintPdfTrip(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == documentId);
            ProcessTable process = _ProcessService.FirstOrDefault(x => x.Id == processId);
            if (!String.IsNullOrEmpty(collection["WorkerSearchId"]))
            {
                string worker = collection["WorkerSearchId"].ToString();
                ViewBag.Worker = worker;
                var documentView = _DocumentService.GetDocumentView(docTable.RefDocumentId, process.TableName);

                if (!String.IsNullOrEmpty(collection["SignNameManual"]))
                {
                    documentView.SignName = collection["SignNameManual"].ToString();
                }
                if (!String.IsNullOrEmpty(collection["SignTitleManual"]))
                {
                    documentView.SignTitle = collection["SignTitleManual"].ToString();
                }

                ViewBag.CompanyAliasName = docTable.CompanyTable.AliasCompanyName;

                return new ViewAsPdf("~/Views/Report/PdfReportTrip.cshtml", documentView)
                {
                    IsGrayScale = true,
                    PageSize = Size.A4,
                    FileName = String.Format("{0}.pdf", docTable.DocumentNum)
                };
            }
            else
                return new EmptyResult();
        }

        public ActionResult ProlongDocumentTask(string tableName, Guid documentId)
        {
            ApplicationUser userTable = _AccountService.Find(User.Identity.GetUserId());
            DocumentTable docTable = _DocumentService.Find(documentId);
            var  refDocument = _DocumentService.GetDocument(docTable.RefDocumentId, docTable.ProcessTable.TableName);
            if (userTable == null || (refDocument.ExecutionDate < DateTime.UtcNow.Date && (refDocument.ProlongationDate == null || refDocument.ProlongationDate < DateTime.UtcNow.Date))) return RedirectToAction("PageNotFound", "Error");

            EmplTable emplTable = _EmplService.FirstOrDefault(x => x.ApplicationUserId == userTable.Id && x.Enable == true);
            if (emplTable == null) return RedirectToAction("PageNotFound", "Error");

            ProcessView process = _ProcessService.FirstOrDefaultView(x => x.TableName == tableName + "Prolongation" && x.CompanyTableId == userTable.CompanyTableId);

            if (!String.IsNullOrEmpty(process.RoleId))
            {
                string roleName = RoleManager.FindById(process.RoleId).Name;
                if (!UserManager.IsInRole(userTable.Id, roleName))
                {
                    return RedirectToAction("PageNotFound", "Error");
                }
            }

            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, User.Identity.GetUserId());

            var viewModel = new DocumentComposite();
            viewModel.ProcessView = process;
            viewModel.docData = _DocumentService.RouteCustomModelView(process.TableName);
            viewModel.docData.RefDocumentId = documentId;
            viewModel.docData.TextTask = refDocument.MainField;
            viewModel.docData.ExecutionDate = refDocument.ExecutionDate;
            viewModel.docData.ProlongationOldDate = refDocument.ProlongationDate;
            viewModel.docData.RefDocNum = docTable.DocumentNum;
            viewModel.fileId = Guid.NewGuid();
            viewModel.ProcessTemplates = _DocumentService.GetAllTemplatesDocument((Guid)process.Id);
            return View("Create", viewModel);
        }

        public ActionResult CreateTaskFromDocument(Guid documentId)
        {
            ApplicationUser userTable = _AccountService.Find(User.Identity.GetUserId());
            DocumentTable docTable = _DocumentService.Find(documentId);
            var refDocument = _DocumentService.GetDocument(docTable.RefDocumentId, docTable.ProcessTable.TableName);
            if (userTable == null) return RedirectToAction("PageNotFound", "Error");

            EmplTable emplTable = _EmplService.FirstOrDefault(x => x.ApplicationUserId == userTable.Id && x.Enable == true);
            if (emplTable == null) return RedirectToAction("PageNotFound", "Error");

            ProcessView process = _ProcessService.FirstOrDefaultView(x => x.TableName == "USR_TAS_DailyTasks" && x.CompanyTableId == userTable.CompanyTableId);

            if (!String.IsNullOrEmpty(process.RoleId))
            {
                string roleName = RoleManager.FindById(process.RoleId).Name;
                if (!UserManager.IsInRole(userTable.Id, roleName))
                {
                    return RedirectToAction("PageNotFound", "Error");
                }
            }

            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, User.Identity.GetUserId());
            
            var viewModel = new DocumentComposite();
            viewModel.ProcessView = process;
            viewModel.docData = _DocumentService.RouteCustomModelView(process.TableName);
            viewModel.docData.RefDocumentId = documentId;
            viewModel.docData.RefDocNum = docTable.DocumentNum;
            viewModel.fileId = Guid.NewGuid();
            viewModel.ProcessTemplates = _DocumentService.GetAllTemplatesDocument((Guid)process.Id);
            return View("Create", viewModel);
        }
        public ActionResult CopyDocument(Guid processId, Guid fileId,Guid documentId)
        {
            ApplicationUser userTable = _AccountService.Find(User.Identity.GetUserId());
            DocumentTable docTable = _DocumentService.Find(documentId);

            if (userTable == null) return RedirectToAction("PageNotFound", "Error");

            EmplTable emplTable = _EmplService.FirstOrDefault(x => x.ApplicationUserId == userTable.Id && x.Enable == true);
            if (emplTable == null) return RedirectToAction("PageNotFound", "Error");

            ProcessView process = _ProcessService.FindView(processId);

            List<FileTable> docFile = _DocumentService.GetAllFilesDocument(docTable.FileId).ToList();
            Guid newDocFileId = Guid.NewGuid();
            docFile.ForEach(x => _DocumentService.DuplicateFile(x, newDocFileId));
            var docView = _DocumentService.GetDocumentView(_DocumentService.Find(documentId).RefDocumentId, process.TableName);
            if (docTable.DocType == DocumentType.Task)
            {
                docView.RefDocNum = null;
                docView.RefDocumentId = null;
            }

            var complexModelNew = RapidDoc.Extensions.Extensions.Clone(docView);
            var documentIdNew = _DocumentService.SaveDocument(complexModelNew, process.TableName, GuidNull2Guid(process.Id), newDocFileId, userTable);

            DateTime date = DateTime.UtcNow;
            DateTime startTime = new DateTime(date.Year, date.Month, date.Day) + process.StartWorkTime;
            DateTime endTime = new DateTime(date.Year, date.Month, date.Day) + process.EndWorkTime;
            if ((startTime > date || date > endTime) && process.StartWorkTime != process.EndWorkTime) return RedirectToAction("PageNotFound", "Error");

            if (!String.IsNullOrEmpty(process.RoleId))
            {
                string roleName = RoleManager.FindById(process.RoleId).Name;
                if (!UserManager.IsInRole(userTable.Id, roleName))
                {
                    return RedirectToAction("PageNotFound", "Error");
                }
            }
            _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentIdNew, HistoryType = Models.Repository.HistoryType.CopyDocumment }, User.Identity.GetUserId());

            return RedirectToAction("ShowDraft", "Document", new { id = documentIdNew });
        }

        public ActionResult Create(Guid id)
        {
            string userId = User.Identity.GetUserId();

            EmplTable emplTable = _EmplService.FirstOrDefault(x => x.ApplicationUserId == userId && x.Enable == true);
            if (emplTable == null) return RedirectToAction("PageNotFound", "Error");

            ProcessView process = _ProcessService.FindView(id);

            DateTime date = DateTime.UtcNow;
            DateTime startTime = new DateTime(date.Year, date.Month, date.Day) + process.StartWorkTime;
            DateTime endTime = new DateTime(date.Year, date.Month, date.Day) + process.EndWorkTime;
            if ((startTime > date || date > endTime) && process.StartWorkTime != process.EndWorkTime) return RedirectToAction("PageNotFound", "Error");

            if (!String.IsNullOrEmpty(process.RoleId))
            {
                string roleName = RoleManager.FindById(process.RoleId).Name;
                if (!UserManager.IsInRole(userId, roleName))
                {
                    return RedirectToAction("PageNotFound", "Error");
                }
            }

            var viewModel = new DocumentComposite();
            viewModel.ProcessView = process;
            viewModel.docData = _DocumentService.RouteCustomModelView(process.TableName);
            viewModel.fileId = Guid.NewGuid();
            viewModel.ProcessTemplates = _DocumentService.GetAllTemplatesDocument(id);          
            return View(viewModel);
        }

        public ActionResult ShowDraft(Guid id)
        {
            DocumentTable documentTable = _DocumentService.Find(id);
            DocumentView docuView = _DocumentService.Document2View(documentTable);
            ProcessView process = _ProcessService.FindView(documentTable.ProcessTableId);
            ApplicationUser currentUser = _AccountService.Find(User.Identity.GetUserId());
            EmplTable emplTable = _EmplService.GetEmployer(docuView.ApplicationUserCreatedId, docuView.CompanyTableId);

            if (documentTable == null || docuView == null || process == null || currentUser == null || emplTable == null || _DocumentService.isShowDocument(documentTable, currentUser) == false)
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            Task.Run(() =>
            {
                IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = id }, "", currentUser);
            });

            var viewModel = new DocumentComposite();
            viewModel.ProcessView = process;
            viewModel.DocumentView = docuView;
            viewModel.docData = _DocumentService.GetDocumentView(documentTable.RefDocumentId, process.TableName);
            viewModel.fileId = docuView.FileId;
            viewModel.ProcessTemplates = _DocumentService.GetAllTemplatesDocument(documentTable.ProcessTableId);

            ViewBag.CreatedDate = _SystemService.ConvertDateTimeToLocal(currentUser, docuView.CreatedDate);
            ViewBag.DocumentUrl = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + docuView.AliasCompanyName + "/Document/ShowDocument/" + docuView.Id + "?isAfterView=true";
            if (emplTable != null)
            {
                ViewBag.Initiator = emplTable.ApplicationUserId != null ? emplTable.FullName : docuView.ApplicationUserCreatedId;
                ViewBag.TitleName = emplTable.TitleTableId != null ? emplTable.TitleTable.TitleName : String.Empty;
                ViewBag.DepartmentName = emplTable.DepartmentTableId != null ? emplTable.DepartmentTable.DepartmentName : String.Empty;
                ViewBag.CompanyName = emplTable.CompanyTableId != null ? emplTable.CompanyTable.AliasCompanyName : String.Empty;
            }
            else
            {
                ViewBag.Initiator = String.Empty;
                ViewBag.TitleName = String.Empty;
                ViewBag.DepartmentName = String.Empty;
                ViewBag.CompanyName = String.Empty;
            }

            if (_ModificationUsersService.ContainDocumentUser(id, User.Identity.GetUserId()))
            {
                ViewBag.CountModificationUsers = _ModificationUsersService.GetPartial(x => x.DocumentTableId == id && x.UserId == currentUser.Id).Count();
            }
            else
                ViewBag.CountModificationUsers = 0;
            ModificationUsersTable modificationUser = _ModificationUsersService.FirstOrDefault(x => x.DocumentTableId == id && x.OriginalDocumentId != null);
            if (modificationUser != null)

                ViewBag.ModificationUser = _EmplService.FirstOrDefault(x => x.ApplicationUserId == modificationUser.UserId).FullName;
            else
                ViewBag.ModificationUser = String.Empty;

            ViewBag.RejectHistory = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.CancelledDocument);
            ViewBag.AddReaders = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.AddReader);
            ViewBag.RemoveReaders = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.RemoveReader);

            return View("ShowDraft", viewModel);
        }

        [HttpPost]
        public ActionResult ShowDraft(Guid documentId, ProcessView processView, Guid processId, OperationType operationType, dynamic docModel, Guid fileId, String actionModelName, IDictionary<string, object> documentData)
        {
            DocumentTable documentTable = _DocumentService.Find(documentId);
            if (documentTable.DocumentState != DocumentState.Created)
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            if (operationType == OperationType.ApproveDocument)
            {
                if (ModelState.IsValid)
                {
                    _SearchService.SaveSearchData(documentId, docModel, actionModelName);

                    if (documentTable.ProcessTable != null && !String.IsNullOrEmpty(documentTable.ProcessTable.StartReaderRoleId))
                    {
                        try
                        {
                            var role = RoleManager.FindById(documentTable.ProcessTable.StartReaderRoleId);
                            if (role != null && role.Users != null && role.Users.Count > 0)
                            {
                                List<string> newReader = _DocumentReaderService.AddReader(documentTable.Id, role.Users.ToList());
                                _EmailService.SendReaderEmail(documentTable.Id, newReader);
                            }
                        }
                        catch { }
                    }
                    _WorkflowService.RunWorkflow(documentTable, processView.TableName, documentData);

                    return RedirectToAction("Index", "Document");
                }
            }

            DocumentView docuView = _DocumentService.Document2View(documentTable);
            ApplicationUser currentUser = _AccountService.Find(User.Identity.GetUserId());
            EmplTable emplTable = _EmplService.GetEmployer(docuView.ApplicationUserCreatedId, docuView.CompanyTableId);

            var viewModel = new DocumentComposite();
            viewModel.ProcessView = processView;
            viewModel.DocumentView = docuView;
            viewModel.docData = docModel;
            viewModel.fileId = docuView.FileId;
            viewModel.ProcessTemplates = _DocumentService.GetAllTemplatesDocument(processId);

            ViewBag.CreatedDate = _SystemService.ConvertDateTimeToLocal(currentUser, docuView.CreatedDate);
            if (emplTable != null)
            {
                ViewBag.Initiator = emplTable.ApplicationUserId != null ? emplTable.FullName : docuView.ApplicationUserCreatedId;
                ViewBag.TitleName = emplTable.TitleTableId != null ? emplTable.TitleTable.TitleName : String.Empty;
                ViewBag.DepartmentName = emplTable.DepartmentTableId != null ? emplTable.DepartmentTable.DepartmentName : String.Empty;
                ViewBag.CompanyName = emplTable.CompanyTableId != null ? emplTable.CompanyTable.AliasCompanyName : String.Empty;
            }
            else
            {
                ViewBag.Initiator = String.Empty;
                ViewBag.TitleName = String.Empty;
                ViewBag.DepartmentName = String.Empty;
                ViewBag.CompanyName = String.Empty;
            }

            ViewBag.RejectHistory = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.CancelledDocument);
            ViewBag.AddReaders = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.AddReader);
            ViewBag.RemoveReaders = _HistoryUserService.GetPartialView(x => x.DocumentTableId == documentTable.Id && x.HistoryType == Models.Repository.HistoryType.RemoveReader);

            return View("ShowDraft", viewModel);
        }

        public ActionResult GetDocumentData(dynamic modelDoc, string tableName, string viewType, Guid processId)
        {
            ViewBag.ProcessId = processId;
            return PartialView("~/Views/Custom/" + tableName + "_" + viewType + ".cshtml", modelDoc);
        }

        public ActionResult GetAllComment(Guid documentId, string lastComment = "")
        {
            SaveComment(documentId, lastComment);
            var model = _CommentService.GetPartialView(x => x.DocumentTableId == documentId);
            return PartialView("~/Views/Shared/_Comments.cshtml", model);
        }

        [HttpPost]
        public void SaveComment(Guid id, string lastComment)
        {
            if (_SystemService.CheckTextExists(lastComment))
            {
                _CommentService.SaveDomain(new CommentTable { Comment = lastComment, DocumentTableId = id });
                _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = id, HistoryType = Models.Repository.HistoryType.NewComment }, User.Identity.GetUserId());
                _EmailService.SendInitiatorCommentEmail(id, lastComment);
            }
        }

        [HttpPost]
        public ActionResult Create(ProcessView processView, OperationType operationType, dynamic docModel, Guid fileId, String actionModelName, IDictionary<string, object> documentData)
        {
            if (operationType == OperationType.SaveDraft)
            {
                //Save Document
                ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
                var documentId = _DocumentService.SaveDocument(docModel, processView.TableName, GuidNull2Guid(processView.Id), fileId, user, documentData.ContainsKey("IsNotified") ? (bool)documentData["IsNotified"] : false, documentData.ContainsKey("Share") ? (bool)documentData["Share"] : false);
                DocumentTable documentTable = _DocumentService.Find(documentId);

                Task.Run(() =>
                {
                    IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                    IHistoryUserService _HistoryUserServiceTask = DependencyResolver.Current.GetService<IHistoryUserService>();
                    _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = documentId }, "", user);
                    _HistoryUserServiceTask.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, user.Id);
                });

                _SearchService.SaveSearchData(documentId, docModel, actionModelName);

                return RedirectToAction("MyDocuments", "Document");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (processView.DocType == DocumentType.Task && docModel.Separated == true)
                    {
                        string initailStructure = (string)documentData["Users"];
                        string[] arrayTempStructrue = initailStructure.Split(',');

                        Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                        string[] arrayStructure = arrayTempStructrue.Where(a => isGuid.IsMatch(a) == true).ToArray();
                        if (arrayStructure.Count() > 0)
                        {
                            foreach (var item in arrayStructure)
                            {
                                string seprateUser = item + "," + arrayTempStructrue[Array.IndexOf(arrayTempStructrue, item) + 1];
                                docModel.Users = seprateUser;
                                documentData["Users"] = seprateUser;
                                this.CreateSeparateTasks(processView, operationType, docModel, fileId, actionModelName, documentData);
                            }

                            return RedirectToAction("MyDocuments", "Document");
                        }
                    }
                    //Save Document
                    ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
                    var documentId = _DocumentService.SaveDocument(docModel, processView.TableName, GuidNull2Guid(processView.Id), fileId, user, documentData.ContainsKey("IsNotified") ? (bool)documentData["IsNotified"] : false, documentData.ContainsKey("Share") ? (bool)documentData["Share"] : false);
                    DocumentTable documentTable = _DocumentService.Find(documentId);

                    Task.Run(() =>
                    {
                        IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                        IHistoryUserService _HistoryUserServiceTask = DependencyResolver.Current.GetService<IHistoryUserService>();
                        _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = documentId }, "", user);
                        _HistoryUserServiceTask.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, user.Id);
                    });

                    _SearchService.SaveSearchData(documentId, docModel, actionModelName);

                    if (operationType == OperationType.ApproveDocument)
                    {
                        _WorkflowService.RunWorkflow(documentTable, processView.TableName, documentData);
                    }

                    if (processView.DocType == DocumentType.Task)
                        return RedirectToAction("MyDocuments", "Document");
                    else
                        return RedirectToAction("Index", "Document");
                }
            }

            var viewModel = new DocumentComposite();
            viewModel.ProcessView = processView;
            viewModel.docData = docModel;
            viewModel.fileId = fileId;
            viewModel.ProcessTemplates = _DocumentService.GetAllTemplatesDocument(GuidNull2Guid(processView.Id));

            return View("Create", viewModel);
        }

        public ActionResult DocumentToArchive(Guid id)
        {
            ApplicationUser userTable = _AccountService.Find(User.Identity.GetUserId());
            if (userTable == null) return HttpNotFound();

            DocumentTable docuTable = _DocumentService.Find(id);
            if (docuTable == null) return HttpNotFound();

            if ((_DocumentService.isSignDocument(id, userTable) == false && docuTable.DocumentState != DocumentState.OnSign)
                || docuTable.DocumentState == DocumentState.OnSign)
            {
                ReviewDocLogTable reviewTable = _ReviewDocLogService.FirstOrDefault(x => x.DocumentTableId == id && x.ApplicationUserCreatedId == userTable.Id);
                if (reviewTable != null)
                {
                    reviewTable.isArchive = true;
                    _ReviewDocLogService.SaveDomain(reviewTable, userTable.UserName);
                }
            }
            else
                return HttpNotFound();

            return RedirectToAction("Index");
        }

        public ActionResult DocumentFromArchive(Guid id)
        {
            ApplicationUser userTable = _AccountService.Find(User.Identity.GetUserId());
            if (userTable == null) return HttpNotFound();

            IEnumerable<ReviewDocLogTable> reviewTables = _ReviewDocLogService.GetPartial(x => x.DocumentTableId == id && x.ApplicationUserCreatedId == userTable.Id && x.isArchive == true).ToList();

            if (reviewTables != null)
            {
                foreach (var reviewTable in reviewTables)
                {
                    reviewTable.isArchive = false;
                    _ReviewDocLogService.SaveDomain(reviewTable);
                }
            }

            return RedirectToAction("ShowDocument", new { id = id, isAfterView = true });
        }

        public ActionResult AddReader(Guid id)
        {
            DualListView model = new DualListView();
            model.EmplList = InitializeReaderView(id).ToList();
            model.DocumentId = id;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddReader(Guid id, string[] listdata, bool? isAjax)
        {
            string errorText = String.Empty;
            string currentUserId = User.Identity.GetUserId();
            int checkCount = 0;

            foreach (var item in listdata)
            {
                if (!String.IsNullOrEmpty(item) && !_DocumentReaderService.Contains(x => x.DocumentTableId == id && x.UserId == item))
                    checkCount++;
            }

            var currentReaders = _DocumentReaderService.GetPartial(x => x.DocumentTableId == id && x.ApplicationUserCreatedId == currentUserId).GroupBy(x => x.UserId).ToList();
            if ((currentReaders.Count() + checkCount) > 20)
            {
                if (RoleManager.RoleExists("MailingAdmin"))
                {
                    IdentityUserRole user = RoleManager.FindByName("MailingAdmin").Users.FirstOrDefault(x => x.UserId == currentUserId);
                    if (user == null)
                        errorText = ValidationRes.ValidationResource.ErrorLimitReaders;
                }
            }

            if (isAjax == true && String.IsNullOrEmpty(errorText))
            {
                try
                {
                    List<string> newReader = _DocumentReaderService.SaveReader(id, listdata, currentUserId);
                    _EmailService.SendReaderEmail(id, newReader.Distinct().ToList());
                }
                catch (Exception ex)
                {
                    errorText = ex.Message;
                }
            }

            if (!String.IsNullOrEmpty(errorText))
            {
                return Json(new { result = "Error",  errorText = errorText});
            }

            return Json(new { result = "Redirect", url = Url.Action("ShowDocument", new { id = id, isAfterView = true }) });
        }

        private IEnumerable<EmplDualListView> InitializeReaderView(Guid id)
        {
            List<EmplDualListView> result = new List<EmplDualListView>();
            string[] signUsers = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == id && x.SignUserId != null).Select(x => x.SignUserId).ToArray();
            var emplList = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId != null && !signUsers.Contains(x.ApplicationUserId) && x.Enable == true);

            result = emplList.Select(m => new EmplDualListView
            {
                AliasCompanyName = m.AliasCompanyName,
                FullName = m.FullName,
                ApplicationUserId = m.ApplicationUserId,
                isActiveDualList = _DocumentReaderService.Contains(x => x.DocumentTableId == id && x.UserId == m.ApplicationUserId && x.RoleId == null)
            }).Union(from x in RoleManager.Roles.AsEnumerable()
                        where (x.RoleType == RoleType.Group || x.RoleType == RoleType.GroupOrder)
                        select new EmplDualListView
                        {
                            AliasCompanyName = UIElementRes.UIElement.Group,
                            FullName = x.Description,
                            ApplicationUserId = x.Id,
                            isActiveDualList = _DocumentReaderService.Contains(z => z.DocumentTableId == id && z.RoleId == x.Id)
                        }).ToList();

            return result;
        }

        public ActionResult AddExecutor(Guid id, string activityId)
        {
            List<EmplDualListView> result = new List<EmplDualListView>();
            var emplList = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId != null && x.Enable == true);
            WFTrackerTable tracker = _WorkflowTrackerService.FirstOrDefault(x => x.ActivityID == activityId && x.DocumentTableId == id && x.TrackerType == TrackerType.Waiting);

            if (tracker != null && tracker.Users != null)
            {
                result = emplList.Select(m => new EmplDualListView
                {
                    AliasCompanyName = m.AliasCompanyName,
                    FullName = m.FullName,
                    ApplicationUserId = m.ApplicationUserId,
                    isActiveDualList = tracker.Users.Any(x => x.UserId == m.ApplicationUserId)
                }).ToList();
            }

            DualListView model = new DualListView();
            model.EmplList = result;
            model.DocumentId = id;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddExecutor(Guid id, string activityId, string[] listdata, bool? isAjax)
        {
            if (isAjax == true)
            {
                WFTrackerTable tracker = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == id && x.ActivityID == activityId);
                if(tracker != null)
                {
                    ApplicationUser userTable = _AccountService.Find(User.Identity.GetUserId());
                    if (userTable == null) return HttpNotFound();

                    foreach (string data in listdata)
                    {
                        if (tracker.Users.Exists(x => x.UserId == data) == false)
                        {
                            _EmailService.SendNewExecutorEmail(id, data);
                        }
                    }

                    tracker.Users.RemoveAll(x => x.InitiatorUserId != null);

                    if (listdata != null)
                    {
                        foreach (string data in listdata)
                        {
                            if (tracker.Users.Exists(x => x.UserId == data) == false)
                            {
                                tracker.Users.Add(new WFTrackerUsersTable { UserId = data, InitiatorUserId = userTable.Id });
                            }
                        }
                    }

                    _WorkflowTrackerService.SaveDomain(tracker);
                }
            }

            return Json(new { result = "Redirect", url = Url.Action("ShowDocument", new { id = id, isAfterView = true }) });
        }

        public ActionResult NotificationUsers(Guid id, string activityId)
        {
            string currentUserId = User.Identity.GetUserId();
            var currentItem = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == id && x.ActivityID == activityId && x.TrackerType == TrackerType.Waiting);
            if (currentItem != null && currentItem.DocumentTable != null &&
                currentUserId == currentItem.DocumentTable.ApplicationUserCreatedId && DateTime.UtcNow.AddHours(-9) > currentItem.ModifiedDate)
            {
                currentItem.LastNotificationDate = DateTime.UtcNow;
                _WorkflowTrackerService.SaveDomain(currentItem, currentUserId);
                foreach(var user in currentItem.Users)
                    _EmailService.SendNewExecutorEmail(id, user.UserId);
            }

            return RedirectToAction("ShowDocument", new { id = id, isAfterView = true });
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult AjaxUpload(Guid processId, Guid fileId, HttpPostedFileBase files)
        {
            ProcessTable process = _ProcessService.Find(processId);
            var statuses = new List<ViewDataUploadFilesResult>();
            bool error = false;
            string errorText = String.Empty;

            if (process.DocSize > 0)
            {
                if (((files.ContentLength / 1024f) / 1024f) > process.DocSize)
                {
                    error = true;
                    errorText = String.Format(ValidationRes.ValidationResource.ErrorDocSize, process.DocSize, Math.Round(((files.ContentLength / 1024f) / 1024f), 2), files.ContentLength);
                }
            }

            if(String.IsNullOrEmpty(files.FileName) || files.FileName.Split('.').Count() == 1)
            {
                error = true;
                errorText = ValidationRes.ValidationResource.ErrorFileWithoutType;
            }

            DocumentTable document = _DocumentService.FirstOrDefault(x => x.FileId == fileId && x.DocType == process.DocType);
            if(document != null)
            {
                if ((document.DocumentState == DocumentState.Closed || document.DocumentState == DocumentState.Cancelled) && !User.IsInRole("Administrator") && document.DocType != DocumentType.OutcomingDoc)
                {
                    error = true;
                    errorText = ValidationRes.ValidationResource.ErrorDocumentState;
                }
            }
            if (_DocumentService.GetAllFilesDocument(fileId).Where(x => x.Version == "1" && x.ApplicationUserCreatedId == User.Identity.GetUserId()).Count() > 20)
            {
                error = true;
                errorText = ValidationRes.ValidationResource.ErrorDocCount;
            }

            System.IO.FileStream inFile;
            byte[] binaryData;
            string contentType;

            if (files != null && !string.IsNullOrEmpty(files.FileName) && fileId != Guid.Empty && error != true)
            {
                BinaryReader binaryReader = new BinaryReader(files.InputStream);
                byte[] data = binaryReader.ReadBytes(files.ContentLength);
                ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());

                var thumbnail = new byte[] { };
                contentType = files.ContentType.ToString().ToUpper();
                thumbnail = GetThumbnail(data, contentType);

                if (document != null && document.DocumentState != DocumentState.Created && _DocumentService.GetAllFilesDocument(fileId).ToList().Exists(x => x.ApplicationUserCreatedId == User.Identity.GetUserId() && x.ApplicationUserCreatedId != document.ApplicationUserCreatedId && x.CreatedDate > DateTime.UtcNow.AddMinutes(-5)) == false && document.DocType != DocumentType.Task)
                {
                    _EmailService.SendInitiatorEmailDocAdding(document.Id);
                }
                // here you can save your file to the database...
                FileTable doc = new FileTable();
                doc.DocumentFileId = fileId;
                doc.FileName = files.FileName;
                doc.ContentType = contentType;
                doc.ContentLength = files.ContentLength;
                doc.Data = data;
                doc.Thumbnail = thumbnail;
                doc.Version = "1";
                doc.VersionName = "Version 1";

                Guid Id = _DocumentService.SaveFile(doc);

                string createdUser = String.Empty;
                EmplTable emplTable = _EmplService.GetEmployer(doc.ApplicationUserCreatedId, process.CompanyTableId);
                if (emplTable == null)
                {
                    createdUser = doc.ApplicationUserCreated.UserName;
                }
                else
                {
                    createdUser = emplTable.FullName;
                }

                if (thumbnail.Length == 0)
                {
                    inFile = new System.IO.FileStream(Server.MapPath("~/Content/FileUpload/content-types/64/Text.png"),
                                System.IO.FileMode.Open,
                                System.IO.FileAccess.Read);
                    binaryData = new Byte[inFile.Length];
                    long bytesRead = inFile.Read(binaryData, 0,
                                         (int)inFile.Length);
                    inFile.Close();
                    thumbnail = binaryData;
                }

                statuses.Add(new ViewDataUploadFilesResult()
                {
                    id = Id.ToString(),
                    name = doc.FileName,
                    size = doc.ContentLength,
                    url = @"/Document/DownloadFile/" + Id.ToString(),
                    deleteUrl = @"/Document/DeleteFile/" + Id.ToString(),
                    thumbnailUrl = @"data:image/png;base64," + Convert.ToBase64String(thumbnail),
                    deleteType = "DELETE",
                    createdUser = createdUser,
                    createdDate = GetLocalTime(doc.CreatedDate, user.TimeZoneId).ToString(),
                    versionName = doc.VersionName,
                    isReplaceFile = false,
                    isClosed = false
                });
            }
            else
            {              
                statuses.Add(new ViewDataUploadFilesResult()
                {
                    id = String.Empty,
                    name = files.FileName,
                    error = errorText,
                    size = files.ContentLength,
                    url = "",
                    deleteUrl = "",
                    thumbnailUrl = "",
                    deleteType = "DELETE",
                    createdUser = "",
                    createdDate = "",
                    versionName = "",
                    isReplaceFile = false,
                    isClosed = false
                });
            }
            
            
            var uploadedFiles = new
            {
                files = statuses.ToArray()
            };
          
            JsonResult result = Json(uploadedFiles);
            result.ContentType = "text/plain";
            return result;
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult AjaxUploadReplaceFile(Guid processId, Guid fileId, Guid fileDocId)
        {
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                if (hpf.ContentLength == 0)
                    continue;

                ProcessTable process = _ProcessService.Find(processId);
                var statuses = new List<ViewDataUploadFilesResult>();
                bool error = false;

                if (String.IsNullOrEmpty(hpf.FileName) || hpf.FileName.Split('.').Count() == 1)
                {
                    error = true;
                }

                if (process.DocSize > 0)
                {
                    if (((hpf.ContentLength / 1024f) / 1024f) > process.DocSize)
                        error = true;
                }

                if(fileDocId == Guid.Empty)
                    error = true;

                DocumentTable document = _DocumentService.FirstOrDefault(x => x.FileId == fileId);
                if (document != null)
                {
                    if ((document.DocumentState == DocumentState.Closed || document.DocumentState == DocumentState.Cancelled) && !User.IsInRole("Administrator") && document.DocType != DocumentType.OutcomingDoc)
                    {
                        error = true;
                    }
                }

                System.IO.FileStream inFile;
                byte[] binaryData;
                string contentType;

                if (hpf != null && !string.IsNullOrEmpty(hpf.FileName) && fileId != Guid.Empty && error != true)
                {
                    BinaryReader binaryReader = new BinaryReader(hpf.InputStream);
                    byte[] data = binaryReader.ReadBytes(hpf.ContentLength);
                    ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());

                    var thumbnail = new byte[] { };
                    contentType = hpf.ContentType.ToString().ToUpper();
                    thumbnail = GetThumbnail(data, contentType);

                    // here you can save your file to the database...
                    FileTable doc = new FileTable();
                    doc.DocumentFileId = fileId;
                    doc.FileName = hpf.FileName;
                    doc.ContentType = contentType;
                    doc.ContentLength = hpf.ContentLength;
                    doc.Data = data;
                    doc.Thumbnail = thumbnail;
                    doc.Version = "1";
                    doc.VersionName = "Version 1";

                    FileTable replaceFileTable = _DocumentService.GetFile(fileDocId);
                    if(replaceFileTable != null)
                    {
                        doc.Version = Convert.ToString(Convert.ToInt32(replaceFileTable.Version) + 1);
                        doc.VersionName = "Version " + doc.Version;
                        doc.ReplaceRef = fileDocId;
                    }

                    Guid Id = _DocumentService.SaveFile(doc);

                    string createdUser = String.Empty;
                    EmplTable emplTable = _EmplService.GetEmployer(doc.ApplicationUserCreatedId, process.CompanyTableId);
                    if (emplTable == null)
                    {
                        createdUser = doc.ApplicationUserCreated.UserName;
                    }
                    else
                    {
                        createdUser = emplTable.FullName;
                    }

                    if (thumbnail.Length == 0)
                    {
                        inFile = new System.IO.FileStream(Server.MapPath("~/Content/FileUpload/content-types/64/Text.png"),
                                    System.IO.FileMode.Open,
                                    System.IO.FileAccess.Read);
                        binaryData = new Byte[inFile.Length];
                        long bytesRead = inFile.Read(binaryData, 0,
                                             (int)inFile.Length);
                        inFile.Close();
                        thumbnail = binaryData;
                    }

                    statuses.Add(new ViewDataUploadFilesResult()
                    {
                        id = Id.ToString(),
                        name = doc.FileName,
                        size = doc.ContentLength,
                        url = @"/Document/DownloadFile/" + Id.ToString(),
                        deleteUrl = @"/Document/DeleteFile/" + Id.ToString(),
                        thumbnailUrl = @"data:image/png;base64," + Convert.ToBase64String(thumbnail),
                        deleteType = "DELETE",
                        createdUser = createdUser,
                        createdDate = GetLocalTime(doc.CreatedDate, user.TimeZoneId).ToString(),
                        versionName = doc.VersionName,
                        isReplaceFile = false,
                        isClosed = false
                    });
                }
                else
                {
                    statuses.Add(new ViewDataUploadFilesResult()
                    {
                        id = String.Empty,
                        name = hpf.FileName,
                        error = String.Format(ValidationRes.ValidationResource.ErrorDocSize, process.DocSize, Math.Round(((hpf.ContentLength / 1024f) / 1024f), 2), hpf.ContentLength),
                        size = hpf.ContentLength,
                        url = "",
                        deleteUrl = "",
                        thumbnailUrl = "",
                        deleteType = "DELETE",
                        createdUser = "",
                        createdDate = "",
                        versionName = "",
                        isReplaceFile = false,
                        isClosed = false
                    });
                }


                var uploadedFiles = new
                {
                    files = statuses.ToArray()
                };

                JsonResult result = Json(uploadedFiles);
                result.ContentType = "text/plain";
                return result;
            }

            return null;
        }

        [HttpGet]
        public JsonResult GetAllFileDocument(Guid id)
        {
            var statuses = new List<ViewDataUploadFilesResult>();
            var files = _DocumentService.GetAllFilesDocument(id);
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            DocumentTable document = _DocumentService.FirstOrDefault(x => x.FileId == id);

            foreach (var file in files)
            {
                var thumbnail = new byte[] { };
                System.IO.FileStream inFile;
                byte[] binaryData;

                if (file.Thumbnail.Length == 0)
                {
                    inFile = new System.IO.FileStream(Server.MapPath("~/Content/FileUpload/content-types/64/Text.png"),
                                System.IO.FileMode.Open,
                                System.IO.FileAccess.Read);
                    binaryData = new Byte[inFile.Length];
                    long bytesRead = inFile.Read(binaryData, 0,
                                         (int)inFile.Length);
                    inFile.Close();
                    thumbnail = binaryData;
                }
                else
                {
                    thumbnail = file.Thumbnail;
                }

                string createdUser = String.Empty;
                EmplTable emplTable = _EmplService.GetEmployer(file.ApplicationUserCreatedId, file.ApplicationUserCreated.CompanyTableId);

                if(emplTable == null)
                {
                    createdUser = file.ApplicationUserCreated.UserName;
                }
                else
                {
                    createdUser = emplTable.FullName;
                }

                string deleteType = "DELETE";
                if (document != null)
                {
                    if (document.DocumentState != DocumentState.Created && !CheсkFileRightDelete(file, user, document))
                    {
                        deleteType = String.Empty;
                    }
                }

                bool isClosed = true;
                if (document != null && document.DocumentState != DocumentState.Cancelled && document.DocumentState != DocumentState.Closed)
                {
                    isClosed = false;
                }

                bool isReplaceFile = _DocumentService.FileReplaceContains(file.Id);

                statuses.Add(new ViewDataUploadFilesResult()
                {
                    id = file.Id.ToString(),
                    name = file.FileName,
                    size = file.ContentLength,
                    url = @"/Document/DownloadFile/" + file.Id.ToString(),
                    deleteUrl = @"/Document/DeleteFile/" + file.Id.ToString(),
                    thumbnailUrl = @"data:image/png;base64," + Convert.ToBase64String(thumbnail),
                    deleteType = deleteType,
                    createdUser = createdUser,
                    createdDate = GetLocalTime(file.CreatedDate, user.TimeZoneId).ToString(),
                    versionName = file.VersionName,
                    isReplaceFile = isReplaceFile,
                    isClosed = isClosed
                });
            }
            
            var uploadedFiles = new
            {
                files = statuses.ToArray()
            };

            JsonResult result = Json(uploadedFiles, JsonRequestBehavior.AllowGet);
            result.ContentType = "text/plain";
            return result;
        }

        [HttpPost]
        public JsonResult UploadFile()
        {
            String result = String.Empty;
            HttpPostedFileBase file = Request.Files["fileInput"];

            if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
            {
                if (((file.ContentLength / 1024f) / 1024f) > 10)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(String.Format(ValidationRes.ValidationResource.ErrorDocSize, 10, Math.Round(((file.ContentLength / 1024f) / 1024f), 2)));
                }
                else
                {
                    string contentType;
                    BinaryReader binaryReader = new BinaryReader(file.InputStream);
                    byte[] data = binaryReader.ReadBytes(file.ContentLength);

                    var thumbnail = new byte[] { };
                    contentType = file.ContentType.ToString().ToUpper();
                    thumbnail = GetThumbnail(data, contentType);

                    FileTable doc = new FileTable();
                    doc.DocumentFileId = Guid.NewGuid();
                    doc.FileName = file.FileName;
                    doc.ContentType = contentType;
                    doc.ContentLength = file.ContentLength;
                    doc.Data = data;
                    doc.Thumbnail = thumbnail;
                    doc.Version = "1";
                    doc.VersionName = "Version 1";

                    Guid Id = _DocumentService.SaveFile(doc);
                    result = @"/Document/DownloadFile/" + Id.ToString();
                }
            }

            return Json(result);
        }

        [HttpPost]
        public void FavoriteDocument(string documentId, string userId)
        {
            if(!String.IsNullOrEmpty(documentId) && !String.IsNullOrEmpty(userId))
            {
                Guid documentIdGuid = Guid.Parse(documentId);
                var item = _ReviewDocLogService.FirstOrDefault(x => x.ApplicationUserCreatedId == userId && x.DocumentTableId == documentIdGuid);

                if(item != null)
                {
                    item.isFavorite = !item.isFavorite;
                    _ReviewDocLogService.SaveDomain(item);
                }
            }
        }

        private bool CheсkFileRightDelete(FileTable fileTable, ApplicationUser user, DocumentTable document)
        {
            if(UserManager.IsInRole(user.Id, "Administrator"))
            {
                return true;
            }

            var steps = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == document.Id && x.TrackerType == TrackerType.Waiting);

            if (steps != null)
            {
                foreach (var step in steps)
                {
                    if (step.StartDateSLA < fileTable.CreatedDate && fileTable.ApplicationUserCreatedId == user.Id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private byte[] GetThumbnail(byte[] fileData, string contentType)
        {
            var thumbnail = new byte[] {  };

            if (contentType == "IMAGE/PNG"
                || contentType == "IMAGE/GIF"
                || contentType == "IMAGE/JPEG"
                || contentType == "IMAGE/BMP")
            {
                thumbnail = ImageResizer(fileData);
            }

            return thumbnail;
        }

        private byte[] ImageResizer(byte[] entireImage)
        {
            try
            {
                ImageResizer resizer = new ImageResizer(entireImage);
                return resizer.Resize(64, 64, false, ImageEncoding.Png);
            }
            catch (Exception)
            {
                return new byte[] { };
            }
        }

        [HttpGet]
        public FileContentResult DownloadFile(Guid Id)
        {
            FileTable fileTable = _DocumentService.GetFile(Id);
            return File(fileTable.Data, fileTable.ContentType, fileTable.FileName);
        }

        [AcceptVerbs(HttpVerbs.Delete)]
        public JsonResult DeleteFile(Guid Id)
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            FileTable file = _DocumentService.GetFile(Id);
            DocumentTable document = _DocumentService.FirstOrDefault(x => x.FileId == file.DocumentFileId);

            if (document == null || document.DocumentState == DocumentState.Created || CheсkFileRightDelete(file, user, document))
            {
                string fileName = _DocumentService.DeleteFile(Id);
                values.Add(fileName, true);

                if (document != null)
                    _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = document.Id, Description = fileName, HistoryType = Models.Repository.HistoryType.DeletedFile }, User.Identity.GetUserId());
            }
            else
            {
                return Json(null);
            }

            var deletedFiles = new
            {
                files = values
            };

            JsonResult result = Json(deletedFiles);
            result.ContentType = "text/plain";
            return result;
        }

        public ActionResult RoutePostMethod(ProcessView processView, dynamic docModel, int type, OperationType operationType, Guid? documentId, Guid fileId, String actionModelName, IDictionary<string, object> documentData)
        {
            ActionResult view;

            switch(type)
            {
                case 1:
                    view = Create(processView, operationType, docModel, fileId, actionModelName, documentData);
                    break;
                case 2:
                    if(_DocumentService.UpdateDocumentFields(docModel, processView))
                        _SearchService.SaveSearchData(GuidNull2Guid(documentId), docModel, actionModelName);
                    view = ShowDocument(GuidNull2Guid(documentId), operationType, documentData, processView, GuidNull2Guid(processView.Id));
                    break;
                case 3:
                    SaveDocumentDraft(GuidNull2Guid(documentId), docModel, processView);
                    view = ShowDraft(GuidNull2Guid(documentId), processView, GuidNull2Guid(processView.Id), operationType, docModel, fileId, actionModelName, documentData);
                    break;
                default:
                    view = View();
                    break;
            }

            return view;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PostDocument(Guid processId, int type, OperationType operationType, Guid? documentId, Guid fileId, FormCollection collection, string actionModelName)
        {
            IDictionary<string, object> documentData = new Dictionary<string, object>();
            IDictionary<string, IList> listData = new Dictionary<string, IList>();
            Type typeActionModel = Type.GetType("RapidDoc.Models.ViewModels." + actionModelName + "_View");
            var actionModel = Activator.CreateInstance(typeActionModel);

            Regex isList = new Regex(@"[A-Za-z0-9\-]+_[A-Za-z0-9\-]+_Table__[A-Za-z0-9\-]+[*]+[A-Za-z0-9\-]+[[A-Za-z0-9\-]+].[A-Za-z0-9\-]+", RegexOptions.Compiled); 
                          
            if (collection.AllKeys.Contains("DocumentView.IsNotified"))
                documentData.Add("IsNotified", collection["DocumentView.IsNotified"].ToLower().Contains("true"));

            if (collection.AllKeys.Contains("DocumentView.Share"))
                documentData.Add("Share", collection["DocumentView.Share"].ToLower().Contains("true"));

            ProcessView processView = _ProcessService.FindView(processId);
            if (processView == null)
                return RedirectToAction("PageNotFound", "Error");

            //---------------------------------------------------------------------------
            if (collection["ApproveCommentRequest"] != null && _SystemService.CheckTextExists(collection["ApproveCommentRequest"]))
            {
                Guid documentGuidId = GuidNull2Guid(documentId);
                string approveCommentRequest = collection["ApproveCommentRequest"].ToString();
                if (actionModel.GetType().IsSubclassOf(typeof(BasicDocumantOfficeMemoView)))
                {
                    var trackers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentGuidId && x.TrackerType == TrackerType.Waiting);
                    foreach (var tracker in trackers)
                    {
                        approveCommentRequest = _SystemService.DeleteAllTags(approveCommentRequest);
                        tracker.Comments = approveCommentRequest;
                        _WorkflowTrackerService.SaveDomain(tracker);
                    }
                }
                else
                {
                    SaveComment(GuidNull2Guid(documentId), approveCommentRequest);
                }
            }
            if (collection["RejectCommentRequest"] != null && _SystemService.CheckTextExists(collection["RejectCommentRequest"]))
            {
                Guid documentGuidId = GuidNull2Guid(documentId);
                string rejectCommentRequest = collection["RejectCommentRequest"].ToString();
                if (actionModel.GetType().IsSubclassOf(typeof(BasicDocumantOfficeMemoView)))
                {
                    var trackers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentGuidId && x.TrackerType == TrackerType.Waiting);
                    foreach (var tracker in trackers)
                    {
                        rejectCommentRequest = _SystemService.DeleteAllTags(rejectCommentRequest);
                        tracker.Comments = rejectCommentRequest;
                        _WorkflowTrackerService.SaveDomain(tracker);
                    }
                }
                else
                {
                    SaveComment(documentGuidId, rejectCommentRequest);
                }
            }
            else if (collection["RejectComment"] != null && _SystemService.CheckTextExists(collection["RejectComment"]))
            {
                Guid documentGuidId = GuidNull2Guid(documentId);
                string rejectCommentRequest = collection["RejectComment"].ToString();
                if (actionModel.GetType().IsSubclassOf(typeof(BasicDocumantOfficeMemoView)))
                {
                    var trackers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == documentGuidId && x.TrackerType == TrackerType.Waiting);
                    foreach (var tracker in trackers)
                    {
                        rejectCommentRequest = _SystemService.DeleteAllTags(rejectCommentRequest);
                        tracker.Comments = rejectCommentRequest;
                        _WorkflowTrackerService.SaveDomain(tracker);
                    }
                }
                else 
                {
                    SaveComment(documentGuidId, rejectCommentRequest);
                }
            }
            else
            {
                if (operationType == OperationType.RejectDocument)
                    ModelState.AddModelError(string.Empty, UIElementRes.UIElement.RejectReason);
            }
            //---------------------------------------------------------------------------
            foreach (var key in collection.AllKeys)
            {
                if (isList.IsMatch(key) && listData.Count() == 0)
                {
                    listData = GetChildrenBranch(collection);
                    foreach (var item in listData)
                    {
                        System.Reflection.PropertyInfo propertyInfo = typeActionModel.GetProperty(item.Key);
                        if (propertyInfo != null)
                        {
                            propertyInfo.SetValue(actionModel, item.Value, null);
                        }
                    }                  
                }
                else
                {
                    MappingModelFields(key, key, ref actionModel, typeActionModel, collection, ref documentData);                
                }
            }

            if (operationType != OperationType.SaveDraft)
            {
                if (documentId != null)
                {
                    DocumentTable docuTable = _DocumentService.Find(GuidNull2Guid(documentId));
                    CheckCustomDocument(typeActionModel, actionModel, operationType, docuTable, _DocumentService.isSignDocument(docuTable.Id));
                }

                CheckCustomDocument(typeActionModel, actionModel, operationType);
                CheckAttachedFiles(processView, fileId, documentId);
                _CustomCheckDocument.PreUpdateViewModel(typeActionModel, actionModel, ModelState.IsValid);
            }

            ActionResult view = RoutePostMethod(processView, actionModel, type, operationType, documentId, fileId, actionModelName, documentData);
            return view;
        }

        public ActionResult GetTrackerList(Guid id, bool signDocument)
        {
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DocumentTable documentTable = _DocumentService.Find(id);
            ViewBag.DocumentId = id;
            ViewBag.SignDocument = signDocument;

            switch(documentTable.DocType)
            {
                case DocumentType.Request:
                    var modelRequest = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id, timeZoneInfo, documentTable.DocType);
                    return PartialView("~/Views/Document/_TrackerList.cshtml", modelRequest);
                case DocumentType.OfficeMemo:
                    ViewBag.CountWaiting = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == id && x.TrackerType == TrackerType.Waiting).ToList().Count;
                    var modelOfficeMemo = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled), timeZoneInfo, documentTable.DocType);
                    return PartialView("~/Views/Document/_TrackerListCZ.cshtml", modelOfficeMemo);
                default:
                    var model = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id, timeZoneInfo, documentTable.DocType);
                    return PartialView("~/Views/Document/_TrackerList.cshtml", model);
            }
        }

        public ActionResult GetTaskList(Guid id)
        {
            var model = _DocumentService.GetDocumentRefTask(id);
            if (model != null)
                model.ForEach(y => y.DocumentText = _SystemService.DeleteAllTags(y.DocumentText));
            
            return PartialView("~/Views/Document/_TaskList.cshtml", model);       
        }

        public ActionResult GetFileList(Guid id)
        {
            var model = _DocumentService.GetAllFilesDocument(id).ToList();
            return PartialView("~/Views/Document/_FileList.cshtml", model);
        }

        private void CheckAttachedFiles(ProcessView process, Guid fileId, Guid? documentId)
        {
            var files = _DocumentService.GetAllFilesDocument(fileId).ToList();

            if ((process.MandatoryNumberFiles > 0) && ((documentId != null && process.MandatoryDocDate <= _DocumentService.Find(documentId).CreatedDate) || (process.MandatoryDocDate <= DateTime.Now && documentId == null) || (process.MandatoryDocDate == null)))
            {              
                if(files.Count < process.MandatoryNumberFiles)
                {
                    ModelState.AddModelError(string.Empty, String.Format(ValidationRes.ValidationResource.ErrorMandatoryNumberFiles, process.MandatoryNumberFiles, files.Count));
                }

                if (process.MandatoryFileTypes != null && process.MandatoryFileTypes != String.Empty)
                {
                    string[] fileTypes = process.MandatoryFileTypes.Split('|');

                    foreach(var file in files)
                    {
                        if(!fileTypes.Contains(Path.GetExtension(file.FileName.ToUpper())))
                        {
                            ModelState.AddModelError(string.Empty, String.Format(ValidationRes.ValidationResource.ErrorMandatoryFileTypes, process.MandatoryFileTypes, file.FileName));
                        }
                    }
                }             
            }
        }

        private void CheckCustomDocument(Type type, dynamic actionModel, OperationType operationType, DocumentTable documentTable = null, bool isSign = false)
        {
            List<string> errorList = new List<string>();

            if (documentTable == null)
            {
                errorList.AddRange(_CustomCheckDocument.CheckCustomDocument(type, actionModel, operationType));
                errorList.AddRange(_CustomCheckDocument.CheckCustomDocumentHY(type, actionModel, operationType));
                errorList.AddRange(_CustomCheckDocument.CheckCustomDocumentCZ(type, actionModel, operationType));
                errorList.AddRange(_CustomCheckDocument.CheckCustomDocumentPRT(type, actionModel, operationType));
            }
            else
            {
                var current = _DocumentService.GetCurrentSignStep(documentTable.Id).ToList();
                errorList.AddRange(_CustomCheckDocument.CheckCustomPostDocument(type, actionModel, documentTable, isSign, current));
            }

            foreach(var error in errorList)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private void SaveDocumentDraft(Guid documentId, dynamic docModel, ProcessView processView)
        {
            DocumentTable documentTable = _DocumentService.Find(documentId);
            if (documentTable != null)
            {
                docModel.Id = documentTable.RefDocumentId;
                docModel.DocumentTableId = documentTable.Id;
                _DocumentService.UpdateDocumentFields(docModel, processView);
            }
        }

        public ActionResult GetWaitingUserCZ(Guid id)
        {
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DocumentTable documentTable = _DocumentService.Find(id);
            var model = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id && (x.TrackerType == TrackerType.Waiting || x.TrackerType == TrackerType.NonActive), timeZoneInfo, documentTable.DocType).OrderBy(x => x.CreatedDate);
            ViewBag.DocumentId = id;
            return View("~/Views/Document/ShowWaitingUsersCZ.cshtml", model);       
        }

        public ActionResult GetSubcriptionUser(Guid id)
        {
            var subscribtionList = new List<DocumentSubscriptionListView>();
            DocumentTable documentTable = _DocumentService.Find(id);
            List<DocumentSubcriptionListTable> subscribtionListTable= _DocumentSubcriptionService.GetPartial(x => x.DocumentTableId
                 == id).ToList();
            foreach (var item in subscribtionListTable)
            {
                EmplTable emplUser = _EmplService.FirstOrDefault(x => x.ApplicationUserId == item.UserId);
                EmplTable emplUserCreated = _EmplService.FirstOrDefault(x => x.ApplicationUserId == item.ApplicationUserCreatedId);
                if (emplUser != null && emplUserCreated != null)
                {
                    subscribtionList.Add(new DocumentSubscriptionListView() { DocumentNum = documentTable.DocumentNum, DocumentTableId = documentTable.Id, UserName = emplUser.ShortFullNameType2, CreateUserName = emplUserCreated.ShortFullNameType2, UserId = item.UserId, LogDate = item.CreatedDate.ToLocalTime() });
                }
            }
            ViewBag.DocumentId = id;
            return View("~/Views/Document/ShowSubscriberList.cshtml", subscribtionList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            if (disposing && RoleManager != null)
            {
                RoleManager.Dispose();
                RoleManager = null;
            }
            base.Dispose(disposing);
        }

        private void CreateSeparateTasks(ProcessView processView, OperationType operationType, dynamic docModel, Guid fileId, String actionModelName, IDictionary<string, object> documentData)
        {
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            var documentId = _DocumentService.SaveDocument(docModel, processView.TableName, GuidNull2Guid(processView.Id), fileId, user, documentData.ContainsKey("IsNotified") ? (bool)documentData["IsNotified"] : false, documentData.ContainsKey("Share") ? (bool)documentData["Share"] : false);
            DocumentTable documentTable = _DocumentService.Find(documentId);

            Task.Run(() =>
            {
                IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                IHistoryUserService _HistoryUserServiceTask = DependencyResolver.Current.GetService<IHistoryUserService>();
                _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = documentId }, "", user);
                _HistoryUserServiceTask.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, user.Id);
            });

            _SearchService.SaveSearchData(documentId, docModel, actionModelName);

            if (operationType == OperationType.ApproveDocument)
            {
                //Previous code for startReaderRole
                /*if (documentTable.ProcessTable != null && !String.IsNullOrEmpty(documentTable.ProcessTable.StartReaderRoleId))
                {
                    try
                    {
                        var role = RoleManager.FindById(documentTable.ProcessTable.StartReaderRoleId);
                        if (role != null && role.Users != null && role.Users.Count > 0)
                        {
                            List<string> newReader = _DocumentReaderService.AddReader(documentTable.Id, role.Users.ToList());
                            _EmailService.SendReaderEmail(documentTable.Id, newReader);
                        }
                    }
                    catch { }
                }*/
                _WorkflowService.RunWorkflow(documentTable, processView.TableName, documentData);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveChanges")]
        [Authorize(Roles = "Administrator, SetupAdministrator")]
        public ActionResult SaveChanges(Guid processId, int type, Guid fileId, FormCollection collection, string actionModelName, Guid documentId)
        {
            IDictionary<string, IList> listData = new Dictionary<string, IList>();
            DocumentTable docTable = _DocumentService.Find(documentId);
            Regex isList = new Regex(@"[A-Za-z0-9\-]+_[A-Za-z0-9\-]+_Table__[A-Za-z0-9\-]+[*]+[A-Za-z0-9\-]+[[A-Za-z0-9\-]+].[A-Za-z0-9\-]+", RegexOptions.Compiled); 
            IDictionary<string, object> documentData = new Dictionary<string, object>();
            Type typeActionModel = Type.GetType("RapidDoc.Models.ViewModels." + actionModelName + "_View");
            var actionModel = Activator.CreateInstance(typeActionModel);
            var processSourceView = _ProcessService.FindView(processId);

            foreach (var key in collection.AllKeys.OrderBy(x => x))
            {
                if (isList.IsMatch(key) && listData.Count() == 0)
                {
                    listData = GetChildrenBranch(collection);
                    foreach (var item in listData)
                    {
                        System.Reflection.PropertyInfo propertyInfo = typeActionModel.GetProperty(item.Key);
                        if (propertyInfo != null)
                        {
                            propertyInfo.SetValue(actionModel, item.Value, null);
                        }
                    }
                }
                else
                {
                    MappingModelFields(key, key, ref actionModel, typeActionModel, collection, ref documentData);
                }  
            }

            _DocumentService.UpdateDocumentFields(actionModel, processSourceView);
            return RedirectToAction("ShowDocument", new { id = documentId, isAfterView = true });
        }

        public string GetProcessName(Guid? id)
        {
            DocumentTable document = _DocumentService.FirstOrDefault(x => x.Id == id);
            if (document != null)
                return document.ProcessName;

            return String.Empty;
        }

        public void MappingModelFields(string keyCollection, string field, ref object actionModel, Type type,  FormCollection collection, ref IDictionary<string, object> documentData)
        {
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(field);

            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType.IsEnum)
                {
                    var valueEnum = Enum.Parse(propertyInfo.PropertyType, collection[keyCollection].ToString(), true);
                    propertyInfo.SetValue(actionModel, valueEnum, null);
                    documentData.Add(field, valueEnum);
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    bool valueBool = collection[keyCollection].ToLower().Contains("true");
                    propertyInfo.SetValue(actionModel, valueBool, null);
                    documentData.Add(field, valueBool);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime?))
                {
                    HttpCookie cultureCookie = HttpContext.Request.Cookies["lang"];
                    DateTime? valueDate = null;

                    bool isRequired = propertyInfo
                            .GetCustomAttributes(typeof(RequiredAttribute), false)
                            .Length == 1;

                    if (cultureCookie != null)
                    {
                        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo(cultureCookie.Value);
                        valueDate = collection[keyCollection] == "" ? null : (DateTime?)DateTime.Parse(collection[keyCollection], cultureinfo);
                    }
                    else
                    {
                        valueDate = collection[keyCollection] == "" ? null : (DateTime?)DateTime.Parse(collection[keyCollection]);
                    }

                    if ((isRequired == true && valueDate != null) || (isRequired == false))
                    {
                        propertyInfo.SetValue(actionModel, valueDate, null);
                        documentData.Add(field, valueDate);
                    }
                    else
                        ModelState.AddModelError(string.Empty, String.Format(ValidationRes.ValidationResource.ErrorFieldisNull, GetAttributeDisplayName(propertyInfo)));
                }
                else if (propertyInfo.PropertyType == typeof(Guid?))
                {
                    bool isRequired = propertyInfo
                            .GetCustomAttributes(typeof(RequiredAttribute), false)
                            .Length == 1;

                    Guid? valueNotGuid = collection[keyCollection] == "" ? null : (Guid?)Guid.Parse(collection[keyCollection]);

                    if ((isRequired == true && valueNotGuid != null) || (isRequired == false))
                    {
                        propertyInfo.SetValue(actionModel, valueNotGuid, null);
                        documentData.Add(field, valueNotGuid);
                    }
                    else
                        ModelState.AddModelError(string.Empty, String.Format(ValidationRes.ValidationResource.ErrorFieldisNull, GetAttributeDisplayName(propertyInfo)));
                }
                else if (propertyInfo.PropertyType == typeof(Guid))
                {
                    Guid valueGuid = Guid.Parse(collection[keyCollection]);
                    propertyInfo.SetValue(actionModel, valueGuid, null);
                    documentData.Add(field, valueGuid);
                }
                else if (propertyInfo.PropertyType == typeof(TimeSpan))
                {
                    TimeSpan valueTimeSpan = TimeSpan.Parse(collection[keyCollection]);
                    propertyInfo.SetValue(actionModel, valueTimeSpan, null);
                    documentData.Add(field, valueTimeSpan);
                }
                else
                {
                    bool isRequired = propertyInfo
                            .GetCustomAttributes(typeof(RequiredAttribute), false)
                            .Length == 1;

                    if ((isRequired == true && !String.IsNullOrEmpty(collection[keyCollection]) && !String.IsNullOrWhiteSpace(collection[keyCollection]) && _SystemService.CheckTextExists(collection[keyCollection])) || (isRequired == false))
                    {
                        var value = Convert.ChangeType(collection[keyCollection], propertyInfo.PropertyType);
                        propertyInfo.SetValue(actionModel, value, null);
                        documentData.Add(field, value);
                    }
                    else
                        ModelState.AddModelError(string.Empty, String.Format(ValidationRes.ValidationResource.ErrorFieldisNull, GetAttributeDisplayName(propertyInfo)));
                }
            }
        }

        public IDictionary<string, IList> GetChildrenBranch(FormCollection formCollection)
        {
            IDictionary<string, object> documentData = new Dictionary<string, object>();
            IDictionary<string, IList> rootList = new Dictionary<string, IList>();
            IDictionary<Type, IList> allList = new Dictionary<Type, IList>();

            object[] cachePreList = new object[2];
            string modelListName = "", parIdName = "", relField = "", parentGuid = "";
            bool oneList = false;
            List<string> countUniqueNum = new List<string>();

            Regex isList = new Regex(@"[A-Za-z0-9\-]+_[A-Za-z0-9\-]+_Table__[A-Za-z0-9\-]+[*]+[A-Za-z0-9\-]+[[A-Za-z0-9\-]+].[A-Za-z0-9\-]+", RegexOptions.Compiled);  
            Regex takeModelName = new Regex(@"[A-Za-z0-9\-]+_[A-Za-z0-9\-]+_Table", RegexOptions.Compiled);
            Regex takeFieldName = new Regex(@"].[A-Za-z0-9\-]+", RegexOptions.Compiled);
            Regex takeParticularFieldName = new Regex(@"[[]+[A-Za-z0-9\-]+[]].", RegexOptions.Compiled);
            Regex takeRelNameField = new Regex(@"_[A-Za-z0-9\-]+_", RegexOptions.Compiled);
            Regex takeParentGuid = new Regex(@"[*]+[A-Za-z0-9]+", RegexOptions.Compiled);
            

            foreach (var collectionList in formCollection.AllKeys.Where(x => isList.IsMatch(x) == true))
            {
                countUniqueNum.Clear();
                if (parentGuid != takeParentGuid.Match(collectionList).Value.Remove(0, 1))
                {
                    parentGuid = takeParentGuid.Match(collectionList).Value.Remove(0, 1);

                    Regex takeChilBranch = new Regex(@"[*]" + parentGuid + @"[[A-Za-z0-9\-]+].[A-Za-z0-9\-]+", RegexOptions.Compiled);

                    cachePreList[0] = null;
                    cachePreList[1] = null;
                    allList.Clear();
                    formCollection.AllKeys.Where(x => takeChilBranch.IsMatch(x) == true).Reverse()./*Where(x => takeModelName.IsMatch(x) == true).*/Where(x => takeParticularFieldName.IsMatch(x) == true).ToList().ForEach(x => countUniqueNum.Add(takeParticularFieldName.Match(x).Value));
                    if (countUniqueNum.GroupBy(x => x).Count() <= 1)                     
                        oneList = true;
                        
                    foreach (var keyComplex in formCollection.AllKeys.Where(x => takeChilBranch.IsMatch(x) == true).Reverse())
                    {
                        if (modelListName != takeModelName.Match(keyComplex).Value || oneList == true)
                        {
                            modelListName = takeModelName.Match(keyComplex).Value;
                            relField = takeRelNameField.Match(modelListName).Value.Trim('_');

                            Type typeComplexModel = Type.GetType("RapidDoc.Models.DomainModels." + modelListName);
                            var typeList = typeof(List<>);
                            var constructedListType = typeList.MakeGenericType(typeComplexModel);
                            var resultComplexList = (IList)Activator.CreateInstance(constructedListType);

                            Regex takeParListFields = new Regex(modelListName + @"__[A-Za-z0-9\-]+[*]" + parentGuid + @"[[A-Za-z0-9\-]+].[A-Za-z0-9\-]+", RegexOptions.Compiled);
                            foreach (var keyParField in formCollection.AllKeys.Where(x => takeParListFields.IsMatch(x) == true))
                            {
                                if (parIdName != takeParticularFieldName.Match(keyParField).Value.TrimEnd('.'))
                                {
                                    parIdName = takeParticularFieldName.Match(keyParField).Value.TrimEnd('.');

                                    var complexModel = Activator.CreateInstance(typeComplexModel);

                                    typeComplexModel.GetProperty("CreatedDate").SetValue(complexModel, DateTime.UtcNow, null);

                                    typeComplexModel.GetProperty("ModifiedDate").SetValue(complexModel, DateTime.UtcNow, null);
                                    foreach (var keyField in formCollection.AllKeys.Where(x => takeParListFields.IsMatch(x) == true && x.Contains(parIdName)))
                                    {
                                        string modelListFieldName = takeFieldName.Match(keyField).Value;
                                        if (!String.IsNullOrEmpty(modelListFieldName))
                                        {
                                            modelListFieldName = modelListFieldName.Remove(0, 2);
                                            MappingModelFields(keyField.ToString(), modelListFieldName, ref complexModel, typeComplexModel, formCollection, ref documentData);
                               
                                        }
                                        if (cachePreList[1] != null)
                                        {
                                             typeComplexModel.GetProperty(cachePreList[1].ToString()).SetValue(complexModel, cachePreList[0], null);
                                        }
                                    }                        
                                    resultComplexList.Add(complexModel);                      
                                }                       
                                
                                documentData.Clear();
                            }

                            if (cachePreList[1] != null || oneList == true)
                                allList.Add(typeComplexModel, resultComplexList);
                            cachePreList[0] = resultComplexList;
                            cachePreList[1] = relField;
                        }
                        if (oneList == true)
                            oneList = false;
                    }
                    if (rootList.ContainsKey(relField))
                    {
                        if (allList.Count() > 0)
                        {
                            foreach (var item in allList.FirstOrDefault(x => x.Key != null).Value)
                            {
                                rootList[relField].Add(item);
                            }
                        }               
                    }
                    else
                        rootList.Add(relField, allList.FirstOrDefault(x => x.Key != null).Value);
                }
                oneList = false;
            }

            return rootList;
        }
	}
}