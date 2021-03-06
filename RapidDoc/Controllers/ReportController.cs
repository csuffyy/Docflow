﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Extensions;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using Excel = Microsoft.Office.Interop.Excel;
using RapidDoc.Models.DomainModels;
using System.Activities;
using RapidDoc.Activities;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Office.Interop.Excel;
using Rotativa;
using Rotativa.Options;
using System.Text.RegularExpressions;
using System.Drawing;

namespace RapidDoc.Controllers
{
    public class ReportController : BasicController
    {
        private readonly IWorkflowTrackerService _WorkflowTrackerService;
        private readonly IDocumentService _DocumentService;
        private readonly IDepartmentService _DepartmentService;
        private readonly IProcessService _ProcessService;
        private readonly IEmplService _EmplService;
        private readonly IReportService _ReportService;
        private readonly IWorkScheduleService _WorkScheduleService;
        private readonly IEmailService _EmailService;
        private readonly IWorkflowService _WorkflowService;
        private readonly ICommentService _CommentService;
        private readonly IProtocolFoldersService _ProtocolFoldersService;
        private readonly ISystemService _SystemService;
        private readonly IPortalParametersService _PortalParametersService;
        private readonly IDocumentReaderService _DocumentReadersService;

        public ReportController(IWorkflowTrackerService workflowTrackerService, IDocumentService documentService, IDepartmentService departmentService, ICompanyService companyService, IAccountService accountService, IProcessService processService, IEmplService emplService, IReportService reportService, IWorkScheduleService workScheduleService, IEmailService emailService, IWorkflowService workflowService, ICommentService commentService, IProtocolFoldersService protocolFoldersService, ISystemService systemService, IPortalParametersService portalParametersService, IDocumentReaderService documentReadersService)
            : base(companyService, accountService)
        {
            _WorkflowTrackerService = workflowTrackerService;
            _DocumentService = documentService;
            _DepartmentService = departmentService;
            _ProcessService = processService;
            _EmplService = emplService;
            _ReportService = reportService;
            _WorkScheduleService = workScheduleService;
            _EmailService = emailService;
            _WorkflowService = workflowService;
            _CommentService = commentService;
            _ProtocolFoldersService = protocolFoldersService;
            _SystemService = systemService;
            _PortalParametersService = portalParametersService;
            _DocumentReadersService = documentReadersService;
        }

        public ActionResult PerformanceDepartment()
        {
            ViewBag.DepartmentList = _DepartmentService.GetDropListDepartmentNull(null);
            return View();
        }

        public ActionResult DetailReport()
        {
            ViewBag.DepartmentList = _DepartmentService.GetDropListDepartmentNull(null);
            return View();
        }

        public ActionResult TaskReport()
        {
            return View();
        }

        public ActionResult IncomingCorrespondenceReportUSC()
        {
            return View();
        }

        public ActionResult ReportOfRoutes()
        {
            ViewBag.ProcessList = _ProcessService.GetDropListProcessNull(null);
            return View();
        }

        public ActionResult PdfReport(Guid id, Guid? processId, bool acquainted)
        {
            DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == id);
            ProcessTable process = _ProcessService.FirstOrDefault(x => x.Id == processId);
            var documentView = _DocumentService.GetDocumentView(docTable.RefDocumentId, process.TableName);
            EmplTable emplTable = _EmplService.GetEmployer(docTable.ApplicationUserCreatedId, process.CompanyTableId);
            List<EmplTable> emplList = new List<EmplTable>();
            string managerName = String.Empty;

            var trackersAssign = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == id && x.SystemName == "ORDCustomUserAssign" && x.TrackerType == TrackerType.Approved).ToList();
            trackersAssign.ForEach(item => emplList.Add(_EmplService.GetEmployer(item.SignUserId, process.CompanyTableId)));

            var trackerManager = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == id && x.SystemName == "ORDUserManager" && x.TrackerType == TrackerType.Approved);
            if (trackerManager != null)
            {
                managerName = _EmplService.GetEmployer(trackerManager.SignUserId, process.CompanyTableId).ShortFullNameType2;
            }

            List<FileTable> filesResult = new List<FileTable>();
            var files = _DocumentService.GetAllFilesDocument(docTable.FileId).ToList();
            foreach (var item in files)
            {
                if (!_DocumentService.FileReplaceContains(item.Id))
                {
                    filesResult.Add(item);
                }
            }

            ViewBag.ListFiles = filesResult;
            ViewBag.ListUsers = emplList;
            ViewBag.Department = emplTable.DepartmentName;
            ViewBag.DocState = docTable.DocumentState;
            ViewBag.ManagerName = managerName;
            ViewBag.TableName = docTable.ProcessTable.TableName;
            ViewBag.Acquainted = acquainted;
            ViewBag.CompanyAliasName = docTable.CompanyTable.AliasCompanyName;

            return new ViewAsPdf("PdfReport", documentView)
            {
                PageSize = Rotativa.Options.Size.A4,
                IsGrayScale = true,
                //PageMargins = { Left = 20 },
                FileName = String.Format("{0}.pdf", docTable.DocumentNum)
            };
        }

        public ActionResult PdfReportCZ(Guid id, Guid? processId)
        {
            List<ReportCZComments> commentsCZ = new List<ReportCZComments>();
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == id);
            ProcessTable process = _ProcessService.FirstOrDefault(x => x.Id == processId);
            var documentView = _DocumentService.GetDocumentView(docTable.RefDocumentId, process.TableName);
            EmplTable emplTable = _EmplService.GetEmployer(docTable.ApplicationUserCreatedId, process.CompanyTableId);
            var trackersAssign = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled), timeZoneInfo, docTable.DocType);
            var documentComments = _CommentService.GetPartial(x => x.DocumentTableId == id);

            foreach (var comment in documentComments)
            {
                EmplTable emplUserName = _EmplService.GetEmployer(comment.ApplicationUserCreatedId, process.CompanyTableId);
                commentsCZ.Add(new ReportCZComments { UserName = emplUserName.FullName, UserTitle = emplUserName.TitleName, Comment = comment.Comment, CreateDate = comment.CreatedDate });
            }
            List<FileTable> filesResult = new List<FileTable>();
            var files = _DocumentService.GetAllFilesDocument(docTable.FileId).ToList();
            foreach (var item in files)
            {
                if (!_DocumentService.FileReplaceContains(item.Id))
                {
                    filesResult.Add(item);
                }
            }

            ViewBag.ListFiles = filesResult;
            ViewBag.Tracker = trackersAssign;
            ViewBag.UserCreated = emplTable;
            ViewBag.CreatedDate = _SystemService.ConvertDateTimeToLocal(user, docTable.CreatedDate);
            ViewBag.DocState = docTable.DocumentState;
            ViewBag.TableName = docTable.ProcessTable.TableName;
            ViewBag.DocumentNum = docTable.DocumentNum;
            ViewBag.AliasCompanyName = docTable.AliasCompanyName;
            ViewBag.Id = docTable.Id;
            ViewBag.Process = process;
            ViewBag.CommentsCZ = commentsCZ;
            ViewBag.CompanyAliasName = docTable.CompanyTable.AliasCompanyName;

            return new ViewAsPdf("PdfReportCZ", documentView)
            {
                PageSize = Rotativa.Options.Size.A4,
                IsGrayScale = true,
                FileName = String.Format("{0}.pdf", docTable.DocumentNum)
            };
        }

        public ActionResult PdfReportProtocol(Guid id, Guid? processId)
        {
            List<RapidDoc.Models.DomainModels.PRT_QuestionList_Table> questionList = new List<PRT_QuestionList_Table>();

            DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == id);
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            ProcessTable process = _ProcessService.FirstOrDefault(x => x.Id == processId);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            var documentView = _DocumentService.GetDocumentView(docTable.RefDocumentId, process.TableName);
            if (documentView.Chairman != null)
            {
                string[] arrayTempStructrue = documentView.Chairman.Split(',');
                Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                string empId = arrayTempStructrue.FirstOrDefault(a => isGuid.IsMatch(a) == true).ToString();
                EmplTable emplUserChairman = _EmplService.FirstOrDefault(x => x.Id.ToString() == empId && x.CompanyTableId == docTable.CompanyTableId);
                ViewBag.Chairman = emplUserChairman.FullName;
                ViewBag.ChairmanTitle = emplUserChairman.TitleName;
            }
            EmplTable emplTable = _EmplService.GetEmployer(docTable.ApplicationUserCreatedId, process.CompanyTableId);

            var trackersAssign = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled), timeZoneInfo, docTable.DocType);

            if (documentView.Attended != null && documentView.Attended != "")
            {
                string[] arrayAttended = documentView.Attended.Split(',');
                List<string> listAttended = arrayAttended.ToList();
                ViewBag.ListAttended = listAttended;
            }
            if (documentView.Invited != null && documentView.Invited != "")
            {
                string[] arrayInvited = documentView.Invited.Split(',');
                List<string> listInvited = arrayInvited.ToList();
                ViewBag.ListInvited = listInvited;
            }
            if (documentView.Absent != null && documentView.Absent != "")
            {
                string[] arrayAbsent = documentView.Absent.Split(',');
                List<string> listAbsent = arrayAbsent.ToList();
                ViewBag.ListAbsent = listAbsent;
            }

            //if (documentView.QuestionList != null)
            //{
            //    foreach (var item in documentView.QuestionList)
            //    {
            //        questionList.Add(item);
            //    }

            //    foreach (var question in questionList)
            //    {
            //        question.Question = _SystemService.RemoveColorFromText(question.Question);
            //        foreach (var decision in question.DecisionList)
            //        {
            //            decision.Decision = _SystemService.RemoveColorFromText(decision.Decision);
            //        }
            //    }
            //    ViewBag.QuestionList = questionList;
            //}
            List<FileTable> filesResult = new List<FileTable>();
            var files = _DocumentService.GetAllFilesDocument(docTable.FileId).ToList();
            foreach (var item in files)
            {
                if (!_DocumentService.FileReplaceContains(item.Id))
                {
                    filesResult.Add(item);
                }
            }

            IEnumerable<SelectListItem> list = _ProtocolFoldersService.GetDropListProtocolFoldersFullPath(process.Id, documentView.ProtocolFoldersTableId);
            if (list.Count() > 0)
                ViewBag.FolderText = list.Where(x => x.Selected == true).FirstOrDefault().Text;
            ViewBag.DocumentNum = docTable.DocumentNum;
            ViewBag.AliasCompanyName = docTable.AliasCompanyName;
            ViewBag.Id = docTable.Id;
            ViewBag.Process = process;
            ViewBag.Tracker = trackersAssign;
            ViewBag.ListFiles = filesResult;
            ViewBag.UserCreateName = emplTable.FullName;
            ViewBag.UserCreateTitleName = emplTable.TitleName;
            ViewBag.CompanyAliasName = docTable.CompanyTable.AliasCompanyName;

            var taskModel = _DocumentService.GetDocumentRefTask(id);
            if (taskModel != null)
                taskModel.ForEach(y => y.DocumentText = _SystemService.DeleteAllTags(y.DocumentText));

            ViewBag.TaskModel = taskModel;

            return new ViewAsPdf("PdfReportProtocol", documentView)
            {
                IsGrayScale = true,
                PageSize = Rotativa.Options.Size.A4,
                FileName = String.Format("{0}.pdf", docTable.DocumentNum)
            };
        }

        [HttpPost]
        public FileContentResult GenerateTaskReport(ReportParametersBasicView model)
        {
            int i = 0, j = 0, templateSheets = 1;

            ReportExecutionType taskType;
            string subjectDoc = String.Empty;
            List<TaskReportModel> detailTasksList = new List<TaskReportModel>();
            List<TaskReportModel> detailTasksListPRTDir = new List<TaskReportModel>();
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = _AccountService.Find(currentUserId);
            bool otherCompany = currentUser.CompanyTable.AliasCompanyName != "ATK" ? true : false;

            EmailParameterTable emailParameter = _EmailService.FirstOrDefault(x => x.SmtpServer != String.Empty);
            WrapperImpersonationContext contextImpersonation = new WrapperImpersonationContext(emailParameter.ReportAdminDomain, emailParameter.ReportAdminUser, emailParameter.ReportAdminPassword);
            contextImpersonation.Enter();
            ApplicationDbContext context = new ApplicationDbContext();

            Excel.Application excelAppl;
            Excel.Workbook excelWorkbook;
            Excel.Worksheet excelWorksheet;
            int rowCount = 6;
            string reportText;
            excelAppl = new Excel.Application();
            excelAppl.Visible = false;
            excelAppl.DisplayAlerts = false;
            excelWorkbook = currentUser.CompanyTable.AliasCompanyName != "ATK" ? excelAppl.Workbooks.Add(@"C:\Template\TaskReportOther.xlsx") : excelAppl.Workbooks.Add(@"C:\Template\TaskReport.xlsx");
            Dictionary<string, int> blockDepartment = new Dictionary<string, int>();

            PortalParametersTable portalParameters = _PortalParametersService.GetAll().FirstOrDefault();
            List<string> departments = _SystemService.GuidsFromText(portalParameters.ReportDepartments).ToList();

            List<DepartmentTable> firstDepartment = new List<DepartmentTable>();
            if (otherCompany == true)
            {
                foreach (var department in _DepartmentService.GetAll())
                {
                    if (department != null)
                    {
                        j++;
                        firstDepartment.Add(department);
                        blockDepartment.Add(department.DepartmentName, j);
                    }
                }
            }
            else
            {
                foreach (var block in departments)
                {
                    Guid departmentId = new Guid(block);
                    var department = _DepartmentService.FirstOrDefault(x => x.Id == departmentId && x.CompanyTableId == currentUser.CompanyTableId);

                    if (department != null)
                    {
                        j++;
                        firstDepartment.Add(department);
                        blockDepartment.Add(department.DepartmentName, j);
                    }
                }
            }
            while (templateSheets <= excelAppl.Worksheets.Count)
            {
                if (otherCompany == true)
                    templateSheets = 4;

                var allTasksList = (from document in context.DocumentTable
                                    join detailDoc in context.USR_TAS_DailyTasks_Table
                                    on document.Id equals detailDoc.DocumentTableId
                                    join documentRef in context.DocumentTable
                                    on detailDoc.RefDocumentId equals documentRef.Id
                                    where document.DocType == DocumentType.Task &&
                                         document.CompanyTableId == currentUser.CompanyTableId &&
                                        detailDoc.RefDocumentId != null &&
                                        (((documentRef.DocType == DocumentType.Order && templateSheets == 1) ||
                                        (documentRef.DocType == DocumentType.IncomingDoc && templateSheets == 2) ||
                                        (documentRef.DocType == DocumentType.Protocol && templateSheets == 3)) ||
                                        otherCompany == true)
                                        && ((detailDoc.ExecutionDate >= model.StartDate && detailDoc.ExecutionDate <= model.EndDate && detailDoc.ProlongationDate == null) ||
                                               (detailDoc.ProlongationDate >= model.StartDate && detailDoc.ProlongationDate <= model.EndDate))
                                    select document).ToList();

                foreach (var item in allTasksList)
                {
                    reportText = String.Empty;
                    var docTracker = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == item.Id).OrderBy(y => y.CreatedDate);
                    DateTime? closeDate = item.DocumentState == DocumentState.Closed ? docTracker.FirstOrDefault(x => x.SignDate != null).SignDate : null;
                    USR_TAS_DailyTasks_Table taskDoc = context.USR_TAS_DailyTasks_Table.FirstOrDefault(x => x.DocumentTableId == item.Id);
                    if (item.DocumentState == DocumentState.Closed)
                        reportText = _SystemService.DeleteAllSpecialCharacters(_SystemService.DeleteAllTags(taskDoc.ReportText));
                    foreach (var tracker in docTracker)
                    {
                        i++;
                        string executor = "";
                        string delegation = "";
                        string department = "";
                        foreach (var user in tracker.Users)
                        {
                            EmplTable empl = _EmplService.GetEmployer(user.UserId, currentUser.CompanyTableId);
                            executor += empl.ShortFullNameType2 + "\n";
                            department += empl.DepartmentTableId.ToString();

                            WFTrackerTable delegationTracker = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == item.Id && x.ApplicationUserCreatedId == empl.ApplicationUserId && x.CreatedDate > tracker.CreatedDate).OrderBy(y => y.CreatedDate).FirstOrDefault();
                            if (delegationTracker != null)
                                delegationTracker.Users.ForEach(x => delegation += _EmplService.GetEmployer(x.UserId, currentUser.CompanyTableId).ShortFullNameType2 + "\n");

                        }

                        if (item.DocumentState != DocumentState.Closed)
                            taskType = ReportExecutionType.NoneDone;
                        else
                        {
                            DateTime performDate = taskDoc.ProlongationDate != null ? (DateTime)taskDoc.ProlongationDate : (DateTime)taskDoc.ExecutionDate;
                            //DateTime? performDate = _DocumentService.GetSLAPerformDate(signTrack.DocumentTableId, signTrack.StartDateSLA, signTrack.SLAOffset);
                            WFTrackerTable signTrack = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == item.Id && x.SignDate != null);
                            taskType = performDate.Date >= signTrack.SignDate.Value.Date ? ReportExecutionType.Done : ReportExecutionType.OverDate;

                        }

                        switch (templateSheets)
                        {
                            case 1:
                                var refORDDocView = (from detailDoc in context.USR_TAS_DailyTasks_Table
                                                     join refDocumentTable in context.DocumentTable
                                                     on detailDoc.RefDocumentId equals refDocumentTable.Id
                                                     join refDocView in context.USR_ORD_MainActivity_Table
                                                     on refDocumentTable.Id equals refDocView.DocumentTableId
                                                     where detailDoc.DocumentTableId == item.Id
                                                     select refDocView).FirstOrDefault();
                                if (refORDDocView != null)
                                    subjectDoc = refORDDocView.Subject;
                                else
                                    subjectDoc = String.Empty;
                                break;
                            case 2:
                                var refINCDocView = (from detailDoc in context.USR_TAS_DailyTasks_Table
                                                     join refDocumentTable in context.DocumentTable
                                                     on detailDoc.RefDocumentId equals refDocumentTable.Id
                                                     join refDocView in context.USR_IND_IncomingDocuments_Table
                                                     on refDocumentTable.Id equals refDocView.DocumentTableId
                                                     where detailDoc.DocumentTableId == item.Id
                                                     select refDocView).FirstOrDefault();

                                subjectDoc = refINCDocView.DocumentSubject;
                                break;
                            case 3:
                                var refPRTDocView = (from detailDoc in context.USR_TAS_DailyTasks_Table
                                                     join refDocumentTable in context.DocumentTable
                                                     on detailDoc.RefDocumentId equals refDocumentTable.Id
                                                     join refDocView in context.USR_PRT_ProtocolDocuments_Table
                                                     on refDocumentTable.Id equals refDocView.DocumentTableId
                                                     where detailDoc.DocumentTableId == item.Id
                                                     select refDocView).FirstOrDefault();
                                if (refPRTDocView != null)
                                {
                                    subjectDoc = taskDoc.MainField;

                                    detailTasksList.Add(new TaskReportModel
                                    {
                                        CardNumber = refPRTDocView.Subject,
                                        TaskDescription = subjectDoc,
                                        PlaneDate = taskDoc.ProlongationDate != null ? (DateTime)taskDoc.ProlongationDate : (DateTime)taskDoc.ExecutionDate,
                                        Factdate = item.DocumentState == DocumentState.Closed ? closeDate : null,
                                        Executor = executor,
                                        Delegation = delegation,
                                        Status = taskType,
                                        Text = item.DocumentState == DocumentState.Closed ? reportText : "",
                                        Department = department,
                                        DocType = DocumentType.Protocol,
                                        DocNum = item.DocumentNum,
                                        DocId = item.Id
                                    });
                                }
                                else
                                {
                                    subjectDoc = String.Empty;
                                    var refPRTDIRDocView = (from detailDoc in context.USR_TAS_DailyTasks_Table
                                                            join refDocumentTable in context.DocumentTable
                                                            on detailDoc.RefDocumentId equals refDocumentTable.Id
                                                            join refDocView in context.USR_PRT_DirectorateDocuments_Table
                                                            on refDocumentTable.Id equals refDocView.DocumentTableId
                                                            where detailDoc.DocumentTableId == item.Id
                                                            select refDocView).FirstOrDefault();
                                    if (refPRTDIRDocView != null)
                                    {
                                        subjectDoc = taskDoc.MainField;

                                        detailTasksListPRTDir.Add(new TaskReportModel
                                        {
                                            CardNumber = refPRTDIRDocView.Subject,
                                            TaskDescription = subjectDoc,
                                            PlaneDate = taskDoc.ProlongationDate != null ? (DateTime)taskDoc.ProlongationDate : (DateTime)taskDoc.ExecutionDate,
                                            Factdate = item.DocumentState == DocumentState.Closed ? closeDate : null,
                                            Executor = executor,
                                            Delegation = delegation,
                                            Status = taskType,
                                            Text = item.DocumentState == DocumentState.Closed ? reportText : "",
                                            Department = department,
                                            DocType = DocumentType.Protocol,
                                            DocNum = item.DocumentNum,
                                            DocId = item.Id
                                        });
                                    }
                                }
                                break;
                            default:
                                subjectDoc = taskDoc.MainField;
                                break;
                        }

                        if (!String.IsNullOrEmpty(subjectDoc) && templateSheets != excelAppl.Worksheets.Count)
                        {
                            detailTasksList.Add(new TaskReportModel
                            {
                                CardNumber = item.DocumentNum,
                                TaskDescription = subjectDoc,
                                PlaneDate = taskDoc.ProlongationDate != null ? (DateTime)taskDoc.ProlongationDate : (DateTime)taskDoc.ExecutionDate,
                                Factdate = item.DocumentState == DocumentState.Closed ? closeDate : null,
                                Executor = executor,
                                Delegation = delegation,
                                Status = taskType,
                                Text = item.DocumentState == DocumentState.Closed ? reportText : "",
                                Department = department,
                                DocType = item.DocType,
                                DocNum = item.DocumentNum,
                                DocId = item.Id
                            });
                        }
                        break;
                    }
                    i = 0;
                }
                excelWorksheet =
                    (Worksheet)excelAppl.Worksheets[otherCompany == true ? Convert.ToInt32(otherCompany) : templateSheets];

                if (templateSheets == 3)
                {
                    rowCount = _ReportService.GetDepartmentTaskReport(firstDepartment, blockDepartment, detailTasksList, excelWorksheet, rowCount, true, templateSheets, otherCompany);
                    _ReportService.GetDepartmentTaskReport(firstDepartment, blockDepartment, detailTasksListPRTDir, excelWorksheet, rowCount, true, templateSheets, otherCompany, "Поручения с протоколов директората");
                }
                else
                    _ReportService.GetDepartmentTaskReport(firstDepartment, blockDepartment, detailTasksList, excelWorksheet, rowCount, true, otherCompany == true ? Convert.ToInt32(otherCompany) : templateSheets, otherCompany);
                detailTasksList.Clear();
                templateSheets++;
                rowCount = 6;
            }
            object misValue = System.Reflection.Missing.Value;
            string path = @"C:\Template\Result\" + Guid.NewGuid().ToString() + ".xlsx";
            excelWorkbook.SaveAs(path, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue,
                misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue,
                misValue, misValue, misValue, misValue);
            excelWorkbook.Close(true, misValue, misValue);
            excelAppl.Quit();
            FileInfo file = new FileInfo(path);

            byte[] buff = null;
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file.FullName).Length;
            buff = br.ReadBytes((int)numBytes);

            contextImpersonation.Leave();

            return File(buff, "application/vnd.ms-excel", "TaskReport.xls");
        }

        [HttpPost]
        public FileContentResult GenerateCorrespondenceReport(ReportParametersBasicView model)
        {
            List<CorrespondenceReport> resultList = new List<CorrespondenceReport>();
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = _AccountService.Find(currentUserId);
            OutcomingTopicTypeKZHC[] topicOrder = { OutcomingTopicTypeKZHC.Element2, OutcomingTopicTypeKZHC.Element3, OutcomingTopicTypeKZHC.Element1 };
            ReportExecutionType taskType;
            Excel.Application excelAppl;
            Excel.Workbook excelWorkbook;
            Excel.Worksheet excelWorksheet;            
            const string informationTag = "#Статус";
            string chief = "", delegation = "", readers = "", signUserOutcoming = "";
            EmailParameterTable emailParameter = _EmailService.FirstOrDefault(x => x.SmtpServer != String.Empty);

            WrapperImpersonationContext contextImpersonation = new WrapperImpersonationContext(emailParameter.ReportAdminDomain, emailParameter.ReportAdminUser, emailParameter.ReportAdminPassword);
            contextImpersonation.Enter();

            excelAppl = new Excel.Application();
            excelAppl.Visible = false;
            excelAppl.DisplayAlerts = false;
            excelWorkbook = excelAppl.Workbooks.Add(@"C:\Template\CorrespondenceReportUSC.xlsx");

            ApplicationDbContext context = new ApplicationDbContext();
            var allCorrespondenceList = new List<CorrespondenceReportModel>();

            if (currentUser.CompanyTable.AliasCompanyName == "KZC")
            {
                allCorrespondenceList = (from document in context.DocumentTable
                                             join detailDoc in context.USC_IND_IncomingDocuments_Table
                                             on document.Id equals detailDoc.DocumentTableId
                                             where document.DocType == DocumentType.IncomingDoc &&
                                                  document.CompanyTableId == currentUser.CompanyTableId
                                                 && (document.CreatedDate >= model.StartDate && document.CreatedDate <= model.EndDate)
                                         select new CorrespondenceReportModel { document = document, detailDoc = detailDoc }).ToList();
            }
            else if (currentUser.CompanyTable.AliasCompanyName == "KZH")
            {
                allCorrespondenceList = (from document in context.DocumentTable
                                             join detailDoc in context.USK_IND_IncomingDocuments_Table
                                             on document.Id equals detailDoc.DocumentTableId
                                             where document.DocType == DocumentType.IncomingDoc &&
                                                  document.CompanyTableId == currentUser.CompanyTableId
                                                 && (document.CreatedDate >= model.StartDate && document.CreatedDate <= model.EndDate)
                                         select new CorrespondenceReportModel { document = document, detailDoc = detailDoc }).ToList();
            }

            excelWorksheet = (Worksheet)excelAppl.Worksheets[1];

            foreach (var item in allCorrespondenceList)
            {
                CorrespondenceReport resultDocument = new CorrespondenceReport();
                chief = ""; delegation = ""; readers = ""; signUserOutcoming = "";

                resultDocument.IncomingDocumentNum = item.document.DocumentNum;
                resultDocument.IncomingDocumentId = item.document.Id;
                resultDocument.IncomingDocumentTopic = item.detailDoc.TopicType;
                resultDocument.IncomingDateRegistration = item.document.CreatedDate.ToShortDateString();
                resultDocument.IncomingDocNum = item.detailDoc.IncomingDocNum;
                resultDocument.OrganizationName = item.detailDoc.OrganizationTable.OrgName;
                resultDocument.IncomingDocumentSubject = item.detailDoc.DocumentSubject;

                string[] receivers = _SystemService.GuidsFromText(item.detailDoc.Receiver);
                string executorTask = "";
                foreach (var receiver in receivers.ToList())
                {
                    Guid emplId = new Guid(receiver);
                    executorTask = _EmplService.GetPartialIntercompany(x => x.Id == emplId).FirstOrDefault().ApplicationUserId;
                    EmplTable empl = _EmplService.FindIntercompany(emplId);
                    chief += empl.ShortFullNameType2 + "\n";
                }

                resultDocument.IncomingChief = chief;

                var refTask = (from detailTaskDoc in context.USR_TAS_DailyTasks_Table
                               join refDocumentTask in context.DocumentTable
                               on detailTaskDoc.DocumentTableId equals refDocumentTask.Id
                               join wfDocumentTask in context.WFTrackerTable
                               on refDocumentTask.Id equals wfDocumentTask.DocumentTableId
                               where (detailTaskDoc.RefDocumentId == item.document.Id &&
                                wfDocumentTask.Users.Any(x => x.UserId == executorTask))
                               select new { detailTaskDoc, refDocumentTask }).FirstOrDefault();
                if (refTask != null)
                {
                    WFTrackerTable delegationTracker = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == refTask.refDocumentTask.Id && !x.Users.Any(y => y.UserId == executorTask)).OrderBy(y => y.CreatedDate).FirstOrDefault();

                    if (delegationTracker != null)
                    {
                        foreach (var user in delegationTracker.Users.ToList())
                        {
                            EmplTable empl = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId == user.UserId).FirstOrDefault();
                            delegation += empl.ShortFullNameType2 + "\n";
                        }

                        resultDocument.TaskDelegation = delegation;
                    }

                    List<DocumentReaderTable> readersTable = _DocumentReadersService.GetPartial(x => x.DocumentTableId == item.document.Id).ToList();
                    readersTable.Concat(_DocumentReadersService.GetPartial(x => x.DocumentTableId == refTask.refDocumentTask.Id).ToList());
                    foreach (var reader in readersTable.GroupBy(y => y.UserId).Distinct())
                    {
                        EmplTable empl = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId == reader.Key).FirstOrDefault();
                        readers += empl.ShortFullNameType2 + "\n";
                    }
                    resultDocument.DocumentReaders = readers;

                    DateTime planDate = refTask.detailTaskDoc.ProlongationDate != null ? (DateTime)refTask.detailTaskDoc.ProlongationDate : (DateTime)refTask.detailTaskDoc.ExecutionDate;
                    resultDocument.TaskPlaneDate = planDate.ToString() == "" ? "" : planDate.ToShortDateString();

                    DateTime? closeDate = refTask.refDocumentTask.DocumentState == DocumentState.Closed ? _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == refTask.refDocumentTask.Id && x.SignDate != null).SignDate : null;
                    if (closeDate != null)
                    {
                        DateTime factDate = (DateTime)closeDate;
                        resultDocument.TaskFactDate = factDate.ToShortDateString();
                    }

                    if (refTask.refDocumentTask.DocumentState != DocumentState.Closed)
                        taskType = ReportExecutionType.NoneDone;
                    else
                    {
                        WFTrackerTable signTrack = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == refTask.refDocumentTask.Id && x.SignDate != null);
                        taskType = planDate >= signTrack.SignDate.Value.Date ? ReportExecutionType.Done : ReportExecutionType.OverDate;
                        resultDocument.TaskReportText = _SystemService.DeleteAllTags(refTask.detailTaskDoc.ReportText);
                    }
                    CommentTable lastComment = _CommentService.GetPartial(x => x.DocumentTableId == refTask.detailTaskDoc.DocumentTableId && (x.Comment.Contains(informationTag) || x.Comment.Contains(informationTag.ToLower()))).OrderByDescending(y => y.CreatedDate).FirstOrDefault();
                    if (lastComment != null)
                        resultDocument.OutcomingInformation = lastComment.Comment.Replace(informationTag, "").Replace(informationTag.ToLower(), "");

                    resultDocument.ExecutionType = taskType;
                }
                var refOutcomingDocument = (from detailOutcomingDoc in context.USC_OND_OutcomingDocuments_Table
                                            join refOutcomingDoc in context.DocumentTable
                                            on detailOutcomingDoc.DocumentTableId equals refOutcomingDoc.Id
                                            where detailOutcomingDoc.IncomingNumberDocId == item.document.Id ||
                                            refOutcomingDoc.Id == item.detailDoc.OutcomingNumberDocId
                                            select new { detailOutcomingDoc, refOutcomingDoc }).FirstOrDefault();

                if (refOutcomingDocument != null)
                {
                    resultDocument.OutcomingDocumentNum = refOutcomingDocument.refOutcomingDoc.DocumentNum;
                    resultDocument.OutcomingDocumentId = refOutcomingDocument.refOutcomingDoc.Id;
                    resultDocument.OutcomingDocumentCreator = _EmplService.GetEmployer(refOutcomingDocument.refOutcomingDoc.ApplicationUserCreatedId, currentUser.CompanyTableId).ShortFullNameType2;

                    List<WFTrackerTable> signTrackOutcoming = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == refOutcomingDocument.refOutcomingDoc.Id && x.SignUserId != null).ToList();
                    foreach (var sign in signTrackOutcoming)
                    {
                        EmplTable empl = _EmplService.GetPartialIntercompany(x => x.ApplicationUserId == sign.SignUserId).FirstOrDefault();
                        signUserOutcoming += empl.ShortFullNameType2 + "\n";
                    }
                    resultDocument.OutcomingSignUsers = signUserOutcoming;

                    if (refOutcomingDocument.refOutcomingDoc.DocumentState == DocumentState.Closed)
                    {
                        WFTrackerTable signTrack = _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == refOutcomingDocument.refOutcomingDoc.Id && x.SignDate != null).OrderByDescending(y => y.SignDate).FirstOrDefault();
                        DateTime signDate = (DateTime)signTrack.SignDate;
                        resultDocument.OutcomingDateRegistration = signDate.ToShortDateString();
                    }
                }
                resultList.Add(resultDocument);
            }

            _ReportService.GenerateCorrespondenceReport(resultList, topicOrder, excelWorksheet);

            object misValue = System.Reflection.Missing.Value;
            string path = @"C:\Template\Result\" + Guid.NewGuid().ToString() + ".xlsx";
            excelWorkbook.SaveAs(path, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue,
                misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue,
                misValue, misValue, misValue, misValue);
            excelWorkbook.Close(true, misValue, misValue);
            excelAppl.Quit();
            FileInfo file = new FileInfo(path);

            byte[] buff = null;
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file.FullName).Length;
            buff = br.ReadBytes((int)numBytes);

            contextImpersonation.Leave();

            return File(buff, "application/vnd.ms-excel", "CorrespondenceReportUSC.xls");
        }

        [HttpPost]
        public FileContentResult GenerateDetail(ReportParametersBasicView model)
        {
            List<string> listdepartmentId = new List<string>();
            EmailParameterTable emailParameter = _EmailService.FirstOrDefault(x => x.SmtpServer != String.Empty);
            WrapperImpersonationContext contextImpersonation = new WrapperImpersonationContext(emailParameter.ReportAdminDomain, emailParameter.ReportAdminUser, emailParameter.ReportAdminPassword);
            contextImpersonation.Enter();

            List<DepartmentTable> departmentTableList = _DepartmentService.GetPartial(x => x.Id == model.DepartmentTableId).ToList();
            listdepartmentId = _ReportService.GetParentListDepartment(departmentTableList);

            ApplicationDbContext context = new ApplicationDbContext();

            Excel.Application excelAppl;
            Excel.Workbook excelWorkbook;
            Excel.Worksheet excelWorksheet;

            int rowCount = 3;
            int minutes;

            model.EndDate = model.EndDate.AddDays(1);

            var detailData = (from wfTracker in context.WFTrackerTable
                              join document in context.DocumentTable on wfTracker.DocumentTableId equals document.Id
                              join process in context.ProcessTable on document.ProcessTableId equals process.Id
                              let empl = context.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                              join title in context.TitleTable on empl.TitleTableId equals title.Id
                              join department in context.DepartmentTable.Where(x => listdepartmentId.Contains(x.DepartmentName) || listdepartmentId.Count == 0) on empl.DepartmentTableId equals department.Id
                              where wfTracker.CreatedDate >= model.StartDate && wfTracker.CreatedDate <= model.EndDate
                              select new DetailReportModel
                              {
                                  GroupProcessName = process.GroupProcessTable.GroupProcessName,
                                  ProcessName = process.ProcessName,
                                  DocumentNumber = document.DocumentNum,
                                  Author = document.ApplicationUserCreatedId,
                                  CreateDate = document.CreatedDate,
                                  TrackerType = wfTracker.TrackerType,
                                  ActivityName = wfTracker.ActivityName,
                                  UserExecuteName = wfTracker.SignUserId,
                                  SignDate = wfTracker.SignDate,
                                  SLAOffset = wfTracker.SLAOffset,
                                  DocumentId = wfTracker.DocumentTableId,
                                  Date = wfTracker.StartDateSLA,
                                  DepartmentName = department.DepartmentName,
                                  FullName = empl.SecondName + " " + empl.FirstName + " " + empl.MiddleName,
                                  TitleName = title.TitleName,
                                  DocumentText = document.DocumentText
                              }).ToList();

            excelAppl = new Excel.Application();
            excelAppl.Visible = false;
            excelAppl.DisplayAlerts = false;
            excelWorkbook = excelAppl.Workbooks.Add(@"C:\Template\DetailReport.xlsx");
            excelWorksheet = (Excel.Worksheet)excelWorkbook.ActiveSheet;

            Excel.Range range = excelWorksheet.get_Range("ReportDate");
            range.Value = model.StartDate.ToShortDateString() + " - " + model.EndDate.AddDays(-1).ToShortDateString();

            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            WorkScheduleTable workScheduleTable = _WorkScheduleService.FirstOrDefault(x => x.WorkEndTime != null && x.WorkStartTime != null);

            foreach (var item in detailData.Where(x => x.Date != null && x.SignDate != null))
            {
                minutes = 0; int cacheMinutes = 0;
                int year = item.Date.Value.Year;
                int c = item.Date.Value.DayOfYear;
                int s = item.SignDate.Value.DayOfYear;

                DateTime createDateWorkEndTime = new DateTime(item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day, workScheduleTable.WorkEndTime.Hours, workScheduleTable.WorkEndTime.Minutes, workScheduleTable.WorkEndTime.Seconds);
                DateTime signDateWorkStartTime = new DateTime(item.SignDate.Value.Year, item.SignDate.Value.Month, item.SignDate.Value.Day, workScheduleTable.WorkStartTime.Hours, workScheduleTable.WorkStartTime.Minutes, workScheduleTable.WorkStartTime.Seconds);

                if (_WorkScheduleService.CheckDayType(workScheduleTable.Id, signDateWorkStartTime) == false && signDateWorkStartTime < item.SignDate.Value)
                {
                    if (c < s)
                    {
                        cacheMinutes += Convert.ToInt32((item.SignDate.Value - signDateWorkStartTime).TotalMinutes);
                    }
                    else if (c == s)
                        cacheMinutes += Convert.ToInt32((item.SignDate.Value - item.Date.Value).TotalMinutes);
                }
                if (c < s)
                {
                    if (_WorkScheduleService.CheckDayType(workScheduleTable.Id, createDateWorkEndTime) == false && createDateWorkEndTime > item.Date.Value)
                    {
                        cacheMinutes += Convert.ToInt32((createDateWorkEndTime - item.Date.Value).TotalMinutes);

                    }

                    c++;
                }
                while (c < s)
                {
                    DateTime date = new DateTime(year, 1, 1).AddDays(c - 1);
                    if (_WorkScheduleService.CheckDayType(workScheduleTable.Id, new DateTime(year, 1, 1).AddDays(c - 1)) == false)
                    {
                        minutes += 480;
                    }
                    c++;
                }

                minutes += cacheMinutes;
                item.Minutes = minutes;

            }

            foreach (var line in detailData)
            {
                rowCount++;

                if (line.SLAOffset > 0)
                    line.PerformDate = _DocumentService.GetSLAPerformDate(line.DocumentId, line.Date, line.SLAOffset);

                //EmplTable emplAuthor = _EmplService.GetEmployer(line.Author, user.CompanyTableId);
                excelWorksheet.Cells[rowCount, 4] = line.FullName;
                excelWorksheet.Cells[rowCount, 5] = line.TitleName;
                excelWorksheet.Cells[rowCount, 6] = line.DepartmentName;

                if (line.UserExecuteName == String.Empty)
                    excelWorksheet.Cells[rowCount, 10] = "";
                else
                {
                    EmplTable emplExecutor = _EmplService.FirstOrDefault(x => x.ApplicationUserId == line.UserExecuteName);
                    excelWorksheet.Cells[rowCount, 10] = emplExecutor.SecondName + " " + emplExecutor.FirstName + " " + emplExecutor.MiddleName;
                }
                excelWorksheet.Cells[rowCount, 1] = line.GroupProcessName.ToString();
                excelWorksheet.Cells[rowCount, 2] = line.ProcessName.ToString();
                excelWorksheet.Cells[rowCount, 3] = line.DocumentNumber.ToString();
                excelWorksheet.Cells[rowCount, 7] = line.CreateDate.ToString() == "" ? "" : TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(line.CreateDate), timeZoneInfo).ToString();
                excelWorksheet.Cells[rowCount, 8] = line.TrackerType.ToString();
                excelWorksheet.Cells[rowCount, 9] = line.ActivityName.ToString();
                excelWorksheet.Cells[rowCount, 11] = line.Date.ToString() == "" ? "" : TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(line.Date), timeZoneInfo).ToString();
                excelWorksheet.Cells[rowCount, 12] = line.SignDate.ToString() == "" ? "" : TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(line.SignDate), timeZoneInfo).ToString();
                excelWorksheet.Cells[rowCount, 13] = line.PerformDate.ToString() == "" ? "" : TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(line.PerformDate), timeZoneInfo).ToString();
                excelWorksheet.Cells[rowCount, 14] = line.Minutes.ToString() == "" ? "" : line.Minutes.ToString();
                excelWorksheet.Cells[rowCount, 15] = line.DocumentText;
            }

            object misValue = System.Reflection.Missing.Value;
            string path = @"C:\Template\Result\" + Guid.NewGuid().ToString() + ".xlsx";
            excelWorkbook.SaveAs(path, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue,
                misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue,
                misValue, misValue, misValue, misValue);
            excelWorkbook.Close(true, misValue, misValue);
            excelAppl.Quit();
            FileInfo file = new FileInfo(path);

            byte[] buff = null;
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file.FullName).Length;
            buff = br.ReadBytes((int)numBytes);

            contextImpersonation.Leave();

            return File(buff, "application/vnd.ms-excel", "DetailReport.xls");
        }

        [HttpPost]
        public FileContentResult GenerateReport(ReportParametersBasicView model)
        {
            List<string> listdepartmentId = new List<string>();
            EmailParameterTable emailParameter = _EmailService.FirstOrDefault(x => x.SmtpServer != String.Empty);

            WrapperImpersonationContext contextImpersonation = new WrapperImpersonationContext(emailParameter.ReportAdminDomain, emailParameter.ReportAdminUser, emailParameter.ReportAdminPassword);
            contextImpersonation.Enter();

            List<DepartmentTable> departmentTableList = _DepartmentService.GetPartial(x => x.Id == model.DepartmentTableId).ToList();
            listdepartmentId = _ReportService.GetParentListDepartment(departmentTableList);

            ApplicationDbContext context = new ApplicationDbContext();

            Excel.Application excelAppl;
            Excel.Workbook excelWorkbook;
            Excel.Worksheet excelWorksheet;

            int rowCount = 3;

            model.EndDate = model.EndDate.AddDays(1);
            var flatData = (from wfTracker in context.WFTrackerTable
                            join user in context.Users on wfTracker.SignUserId equals user.Id
                            let empl = context.EmplTable.Where(p => p.ApplicationUserId == user.Id).OrderByDescending(p => p.Enable).FirstOrDefault()
                            join title in context.TitleTable on empl.TitleTableId equals title.Id
                            join department in context.DepartmentTable.Where(x => listdepartmentId.Contains(x.DepartmentName) || listdepartmentId.Count == 0) on empl.DepartmentTableId equals department.Id
                            join document in context.DocumentTable on wfTracker.DocumentTableId equals document.Id
                            join process in context.ProcessTable on document.ProcessTableId equals process.Id
                            where wfTracker.ExecutionStep == true && wfTracker.SignUserId != null
                            && (wfTracker.SignDate >= model.StartDate && wfTracker.SignDate <= model.EndDate)
                            && wfTracker.TrackerType == TrackerType.Approved
                            select new ReportPerformanceDepartmentModel
                            {
                                FullName = empl.SecondName + " " + empl.FirstName + " " + empl.MiddleName,
                                UserName = user.UserName,
                                TitleName = title.TitleName,
                                DepartmentName = department.DepartmentName,
                                ProcessName = process.ProcessName,
                                ActivityName = wfTracker.ActivityName,
                                SignDate = wfTracker.SignDate,
                                DocumentId = wfTracker.DocumentTableId,
                                Date = wfTracker.StartDateSLA,
                                SLAOffset = wfTracker.SLAOffset
                            }).ToList();

            foreach (var item in flatData.Where(x => x.SLAOffset > 0))
            {
                item.PerformDate = _DocumentService.GetSLAPerformDate(item.DocumentId, item.Date, item.SLAOffset);
            }

            var gridData = (from data in flatData.ToList()
                            group data by new
                            {
                                data.FullName,
                                data.UserName,
                                data.TitleName,
                                data.DepartmentName,
                                data.ProcessName
                            } into gflat
                            select new
                            {
                                FullName = gflat.Key.FullName,
                                UserName = gflat.Key.UserName,
                                TitleName = gflat.Key.TitleName,
                                DepartmentName = gflat.Key.DepartmentName,
                                ProcessName = gflat.Key.ProcessName,
                                Count = gflat.Count(),
                                CountError = gflat.Count(x => x.SignDate > x.PerformDate && x.PerformDate != null)
                            }).ToList();

            excelAppl = new Excel.Application();
            excelAppl.Visible = false;
            excelAppl.DisplayAlerts = false;
            excelWorkbook = excelAppl.Workbooks.Add(@"C:\Template\ReportDeparmentTemplate.xlsx");
            excelWorksheet = (Excel.Worksheet)excelWorkbook.ActiveSheet;

            Excel.Range range = excelWorksheet.get_Range("ReportDate");
            range.Value = model.StartDate.ToShortDateString() + " - " + model.EndDate.AddDays(-1).ToShortDateString();

            foreach (var line in gridData)
            {
                rowCount++;
                excelWorksheet.Cells[rowCount, 1] = line.FullName.ToString();
                excelWorksheet.Cells[rowCount, 2] = line.UserName.ToString();
                excelWorksheet.Cells[rowCount, 3] = line.TitleName.ToString();
                excelWorksheet.Cells[rowCount, 4] = line.DepartmentName.ToString();
                excelWorksheet.Cells[rowCount, 5] = line.ProcessName.ToString();
                excelWorksheet.Cells[rowCount, 6] = line.Count.ToString();
                excelWorksheet.Cells[rowCount, 7] = line.CountError.ToString();
            }

            object misValue = System.Reflection.Missing.Value;
            string path = @"C:\Template\Result\" + Guid.NewGuid().ToString() + ".xlsx";
            excelWorkbook.SaveAs(path, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue,
                misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue,
                misValue, misValue, misValue, misValue);
            excelWorkbook.Close(true, misValue, misValue);
            excelAppl.Quit();
            FileInfo file = new FileInfo(path);

            byte[] buff = null;
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file.FullName).Length;
            buff = br.ReadBytes((int)numBytes);

            contextImpersonation.Leave();

            return File(buff, "application/vnd.ms-excel", "ReportDepartment.xls");
        }

        public FileContentResult GetReportOfRoutes(Guid? processId)
        {
            List<ProcessTable> processList = new List<ProcessTable>();
            var rows = new List<ReportProcessesView>();
            string users;

            Dictionary<Type, int> typeDict = new Dictionary<Type, int>
            {
                {typeof(WFChooseUpManager),0},
                {typeof(WFChooseStaffStructure),1},
                {typeof(WFChooseSpecificUserFromService),2},
                {typeof(WFChooseSpecificUser),3},
                {typeof(WFChooseRoleUser),4},
                {typeof(WFChooseManualExecution),5},
                {typeof(WFChooseDocUsers),6},
                {typeof(WFChooseCreatedUser),7}
            };

            Excel.Application excelAppl;
            Excel.Workbook excelWorkbook;
            Excel.Worksheet excelWorksheet;

            int rowCount = 1;

            if (processId == null)
                processList = _ProcessService.GetPartial(x => x.isApproved == true).ToList();
            else
                processList.AddRange(_ProcessService.GetPartial(x => x.Id == processId).ToList());

            foreach (var process in processList)
            {
                rows = rows.Concat(_ReportService.GetActivityStages(typeDict, _ReportService.GetActivity(process), process)).ToList();
            }

            EmailParameterTable emailParameter = _EmailService.FirstOrDefault(x => x.SmtpServer != String.Empty);

            WrapperImpersonationContext contextImpersonation = new WrapperImpersonationContext(emailParameter.ReportAdminDomain, emailParameter.ReportAdminUser, emailParameter.ReportAdminPassword);
            contextImpersonation.Enter();

            excelAppl = new Excel.Application();
            excelAppl.Visible = false;
            excelAppl.DisplayAlerts = false;
            excelWorkbook = excelAppl.Workbooks.Add(@"C:\Template\ProcessReport.xlsx");
            excelWorksheet = (Excel.Worksheet)excelWorkbook.ActiveSheet;


            foreach (var line in rows)
            {
                rowCount++;
                excelWorksheet.Cells[rowCount, 1] = line.Process.ProcessName.ToString();
                excelWorksheet.Cells[rowCount, 2] = line.Process.TableName.ToString();
                excelWorksheet.Cells[rowCount, 3] = line.StageName.ToString();
                excelWorksheet.Cells[rowCount, 4] = line.FilterType.ToString();
                excelWorksheet.Cells[rowCount, 5] = line.FilterText.ToString();
                Range range = (Range)excelWorksheet.Cells[rowCount, 5];
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(line.Color);
                if (line.Names.Count != 0)
                {
                    if (line.Names.Count > 1)
                    {
                        users = String.Empty;
                        foreach (EmplTable user in line.Names)
                        {
                            users += user.FullName + ";";
                        }
                        excelWorksheet.Cells[rowCount, 6] = users.ToString();
                    }
                    else
                        excelWorksheet.Cells[rowCount, 6] = line.Names.FirstOrDefault().FullName;
                }
                else
                    excelWorksheet.Cells[rowCount, 6] = "";
            }


            object misValue = System.Reflection.Missing.Value;
            string path = @"C:\Template\Result\" + Guid.NewGuid().ToString() + ".xls";
            excelWorkbook.SaveAs(path, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue,
                misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue,
                misValue, misValue, misValue, misValue);
            excelWorkbook.Close(true, misValue, misValue);
            excelAppl.Quit();
            FileInfo file = new FileInfo(path);

            byte[] buff = null;
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file.FullName).Length;
            buff = br.ReadBytes((int)numBytes);

            contextImpersonation.Leave();

            return File(buff, "application/vnd.ms-excel", "ReportRoute.xls");
        }
    }

    public class ProcessReportModel
    {
        public string ProcessName { get; set; }
        public string TableName { get; set; }
        public string StageName { get; set; }
        public FilterType FilterType { get; set; }
        public string Filter { get; set; }
        public string Users { get; set; }
        public System.Drawing.Color Color { get; set; }
    }

    public class ReportPerformanceDepartmentModel
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string TitleName { get; set; }
        public string DepartmentName { get; set; }
        public string ProcessName { get; set; }
        public string ActivityName { get; set; }
        public Guid DocumentId { get; set; }
        public DateTime? Date { get; set; }
        public int SLAOffset { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime? PerformDate { get; set; }
        public string SignUserId { get; set; }
        public TrackerType TrackerType { get; set; }
        public Guid? WftId { get; set; }
    }

    public class CorrespondenceReport
    {
        public OutcomingTopicTypeKZHC IncomingDocumentTopic { get; set; }
        public string IncomingDocumentNum { get; set; }
        public Guid IncomingDocumentId { get; set; }
        public string IncomingDateRegistration { get; set; }
        public string IncomingDocNum { get; set; }
        public string OrganizationName { get; set; }
        public string IncomingDocumentSubject { get; set; }
        public string IncomingChief { get; set; }
        public string TaskDelegation { get; set; }
        public string DocumentReaders { get; set; }
        public string TaskPlaneDate { get; set; }
        public string TaskFactDate { get; set; }
        public ReportExecutionType ExecutionType { get; set; }
        public string TaskReportText { get; set; }
        public string OutcomingDocumentNum { get; set; }
        public Guid OutcomingDocumentId { get; set; }
        public string OutcomingDocumentCreator { get; set; }
        public string OutcomingSignUsers { get; set; }
        public string OutcomingDateRegistration { get; set; }
        public string OutcomingInformation { get; set; }
    }

    public class DetailReportModel
    {
        public string GroupProcessName { get; set; }
        public string ProcessName { get; set; }
        public string DocumentNumber { get; set; }
        public string Author { get; set; }
        public DateTime? CreateDate { get; set; }
        public TrackerType TrackerType { get; set; }
        public string ActivityName { get; set; }
        public string UserExecuteName { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime? PerformDate { get; set; }
        public int SLAOffset { get; set; }
        public Guid DocumentId { get; set; }
        public DateTime? Date { get; set; }
        public int? Minutes { get; set; }
        public string DepartmentName { get; set; }
        public string TitleName { get; set; }
        public string FullName { get; set; }
        public string DocumentText { get; set; }
    }

    public class TaskReportModel
    {
        public string CardNumber { get; set; }
        public string TaskDescription { get; set; }
        public DateTime PlaneDate { get; set; }
        public DateTime? Factdate { get; set; }
        public string Executor { get; set; }
        public string Delegation { get; set; }
        public ReportExecutionType Status { get; set; }
        public string Text { get; set; }
        public string Department { get; set; }
        public DocumentType DocType { get; set; }
        public string DocNum { get; set; }
        public Guid DocId { get; set; }
    }

    public class CorrespondenceReportModel
    {
        public DocumentTable document { get; set; }
        public BasicIncomingDocumentsKZHCTable detailDoc { get; set; }
    }
}