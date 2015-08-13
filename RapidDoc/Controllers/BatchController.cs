﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using System.Drawing;
using RapidDoc.Activities;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RapidDoc.Controllers
{
    public class BatchController : ApiController
    {
        protected readonly IEmplService _EmplService;
        protected readonly IWorkScheduleService _WorkScheduleService;
        protected readonly IEmailService _Emailservice;
        protected readonly IDocumentService _Documentservice;
        protected readonly IReviewDocLogService _ReviewDocLogService;
        protected readonly IAccountService _AccountService;
        protected readonly IProcessService _ProcessService;
        protected readonly IReportService _ReportService;
        protected readonly IDepartmentService _DepartmentService;
        protected readonly IWorkflowTrackerService _WorkflowTrackerService;
        private readonly IWorkflowService _WorkflowService;

        public BatchController(IEmplService emplService, IWorkScheduleService workScheduleService,
            IEmailService emailservice, IDocumentService documentservice, IReviewDocLogService reviewDocLogService,
            IAccountService accountService, IProcessService processService, IReportService reportService, IDepartmentService departmentService, IWorkflowTrackerService workflowtrackerService, IWorkflowService workflowService)
        {
            _EmplService = emplService;
            _WorkScheduleService = workScheduleService;
            _Emailservice = emailservice;
            _Documentservice = documentservice;
            _ReviewDocLogService = reviewDocLogService;
            _AccountService = accountService;
            _ProcessService = processService;
            _ReportService = reportService;
            _DepartmentService = departmentService;
            _WorkflowTrackerService = workflowtrackerService;
            _WorkflowService = workflowService;
        }

        // GET api/<controller>
        public void Get(int id, string companyId)
        {
            var allDocument = _Documentservice.GetPartial(x => x.CompanyTable.AliasCompanyName == companyId).ToList();

            if (allDocument == null)
                return;

            switch(id)
            {
                case 1:
                    if (_WorkScheduleService.CheckWorkTime(null, DateTime.UtcNow))
                    {
                        var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                        List<CheckSLAStatus> checkData = new List<CheckSLAStatus>();

                        foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Agreement
                        || x.DocumentState == Models.Repository.DocumentState.Execution).ToList())
                        {
                            var checkUser = _Documentservice.GetUsersSLAStatus(document, SLAStatusList.Warning).ToList();
                            checkData.Add(new CheckSLAStatus(document, checkUser));
                        }

                        foreach (var user in users)
                        {
                            var userDocuments = checkData.Where(x => x.TrackerUsers.Any(a => a.UserId == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();
                            if (userDocuments.Count() > 0)
                                _Emailservice.SendSLAWarningEmail(user.Id, userDocuments);
                        }
                    }
                    break;
                case 2:
                    if (_WorkScheduleService.CheckWorkTime(null, DateTime.UtcNow))
                    {
                        var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                        List<CheckSLAStatus> checkData = new List<CheckSLAStatus>();

                        foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Agreement
                        || x.DocumentState == Models.Repository.DocumentState.Execution).ToList())
                        {
                            var checkUser = _Documentservice.GetUsersSLAStatus(document, SLAStatusList.Disturbance).ToList();
                            checkData.Add(new CheckSLAStatus(document, checkUser));
                        }

                        foreach (var user in users)
                        {
                            var userDocuments = checkData.Where(x => x.TrackerUsers.Any(a => a.UserId == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();
                            if (userDocuments.Count() > 0)
                                _Emailservice.SendSLADisturbanceEmail(user.Id, userDocuments);
                        }
                    }
                    break;
                case 3:
                    foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Closed
                        || x.DocumentState == Models.Repository.DocumentState.Cancelled
                        || x.DocumentState == Models.Repository.DocumentState.Created).ToList())
                    {
                        IEnumerable<ReviewDocLogTable> reviewDocuments = _ReviewDocLogService.GetPartial(x => x.DocumentTableId == document.Id && x.isArchive == false).ToList();

                        if (reviewDocuments != null)
                        {
                            foreach (var reviewTable in reviewDocuments)
                            {
                                if (reviewTable.CreatedDate <= DateTime.UtcNow.AddDays(-10))
                                {
                                    reviewTable.isArchive = true;
                                    _ReviewDocLogService.SaveDomain(reviewTable, "Admin");
                                }
                            }
                        }
                    }
                    break;
                case 4:
                    if (!_WorkScheduleService.CheckDayType(_WorkScheduleService.FirstOrDefault(x => x.WorkScheduleName == "8x5").Id, DateTime.UtcNow.Date))
                    {
                        var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                        List<ReminderUsers> checkData = new List<ReminderUsers>();

                        foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Agreement
                        || x.DocumentState == Models.Repository.DocumentState.Execution).ToList())
                        {
                            var usersReminder = _Documentservice.GetSignUsersDirect(document).ToList();
                            checkData.Add(new ReminderUsers(document, usersReminder));
                        }

                        foreach (var user in users)
                        {
                            var userDocuments = checkData.Where(x => x.Users.Any(a => a.Id == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();
                            if (userDocuments.Count() > 0)
                                _Emailservice.SendReminderEmail(user, userDocuments);
                        }
                    }
                    break;
                case 5:
                    List<ReportProcessesView> listProcesses = new List<ReportProcessesView>();
                    Dictionary<Type, int> typeActivities = new Dictionary<Type, int>
                    {
                        {typeof(WFChooseStaffStructure),1},
                        {typeof(WFChooseSpecificUser),3},
                        {typeof(WFChooseRoleUser),4}
                    };


                    List<ProcessTable> processList = _ProcessService.GetPartialIntercompany(x => x.isApproved == true).ToList();

                    foreach (var process in processList)
                    {
                        listProcesses = listProcesses.Concat(_ReportService.GetActivityStages(typeActivities, _ReportService.GetActivity(process), process)).ToList();
                    }

                    if (listProcesses.Where(x => x.Color == Color.LightPink).Count() > 0)
                    {
                        _Emailservice.SendFailedRoutesAdministrator(listProcesses.Where(x => x.Color == Color.LightPink).ToList());
                    }
                    break;
                case 6:
                    if (_WorkScheduleService.CheckWorkTime(null, DateTime.UtcNow))
                    {
                        var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                        List<ReminderUsers> checkData = new List<ReminderUsers>();
                        ApplicationDbContext dbContext = new ApplicationDbContext();
                        List<ApplicationUser> usersReminder = new List<ApplicationUser>();

                        List<USR_TAS_DailyTasks_Table> listDocuments = dbContext.USR_TAS_DailyTasks_Table.Where(x => x.ReportText == null && 
                            ((x.ProlongationDate == null && 
                                (EntityFunctions.DiffDays(DateTime.UtcNow, x.ExecutionDate) == 7 || EntityFunctions.DiffDays(DateTime.UtcNow, x.ExecutionDate) < 4) || 
                                (x.ProlongationDate != null && (EntityFunctions.DiffDays(DateTime.UtcNow, x.ProlongationDate) == 7 || EntityFunctions.DiffDays(DateTime.UtcNow, x.ProlongationDate) < 4))))).ToList();
                         
                        foreach (USR_TAS_DailyTasks_Table item in listDocuments)
                        {
                            DocumentTable document = allDocument.FirstOrDefault(x => x.Id == item.DocumentTableId);
                            var usersTask = _Documentservice.GetSignUsersDirect(document).ToList();
                            checkData.Add(new ReminderUsers(document, usersTask));
                        }

                        foreach (var user in users)
                        {
                            var userDocuments = checkData.Where(x => x.Users.Any(a => a.Id == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();
                            if (userDocuments.Count() > 0)
                                _Emailservice.SendReminderTasksEmail(user, userDocuments, listDocuments);
                        }
                    }
                    break;
                case 7:
                    if (_WorkScheduleService.CheckWorkTime(null, DateTime.UtcNow))
                    {
                        var users = _EmplService.GetPartial(x => x.Enable == true).ToList();

                        UserManager<ApplicationUser>  userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                        RoleManager<ApplicationRole> roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));

                        foreach (var user in users)
                        {
                            DepartmentTable rolesDepartment = this.getParentDepartment(user.DepartmentTableId);

                            if (rolesDepartment != null)
                            {
                                string[] arrayTempStructrue = rolesDepartment.RequiredRoles.Split(',');
                                Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                                string[] arrayStructure = arrayTempStructrue.Where(a => isGuid.IsMatch(a) == true).ToArray();

                                foreach (var role in arrayStructure)
                                {
                                    string roleName = roleManager.FindById(role).Name;

                                    if (!userManager.IsInRole(user.ApplicationUserId, roleName))
                                    {
                                        userManager.AddToRole(user.ApplicationUserId, roleName);
                                    }
                                }
                            }

                        }
                    }
                    break;
                case 8:
                    Regex userGuid = new Regex(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}", RegexOptions.Compiled);
                    var workflowTracker = _WorkflowTrackerService.GetPartial(w => w.TrackerType == TrackerType.Waiting);
                    foreach (var document in allDocument.Where(x => x.DocType == DocumentType.Order && x.DocumentState == DocumentState.Agreement && workflowTracker.Any(wf => wf.DocumentTableId == x.Id && userGuid.IsMatch(wf.ActivityID) == true)).ToList())
                    {
                        var sourceDocumentData = _Documentservice.GetDocumentView(document.RefDocumentId, document.ProcessTable.TableName);

                        List<string> userList = _WorkflowService.GetUniqueUserList(document.Id, new Dictionary<string, object> { { "ListAgreement", sourceDocumentData.ListAgreement } }, "ListAgreement", true);
                        var trackers = _WorkflowTrackerService.GetCurrentStep(x => x.DocumentTableId == document.Id && x.TrackerType == TrackerType.Waiting && x.SLAOffset > 0);
                        var lastTracker = _WorkflowTrackerService.GetPartial(x => x.TrackerType == TrackerType.NonActive && x.DocumentTableId == document.Id && userList.Any(z => x.ActivityID.Contains(z))).ToList();

                        if (trackers.Any(x => userList.Any(z => x.ActivityID.Contains(z)) && DateTime.Now > _Documentservice.GetSLAPerformDate(document.Id, x.StartDateSLA, x.SLAOffset)) && lastTracker.Count() > 0)
                        {
                            _WorkflowService.ActiveWorkflowApprove(document.Id, document.ProcessTable.TableName, document.WWFInstanceId, document.ProcessTableId, new Dictionary<string, object>(), trackers.First().Users.First().UserId);
                        }
                        else
                            continue;
                    }
                    break;
            }      
        }

        public DepartmentTable getParentDepartment(Guid? id)
        {
            DepartmentTable childDepartment = _DepartmentService.FirstOrDefault(x => x.Id == id);
            if (childDepartment != null && childDepartment.RequiredRoles != null)
                return childDepartment;
            else
            {

                return childDepartment != null && childDepartment.ParentDepartmentId != null ? this.getParentDepartment(childDepartment.ParentDepartmentId) : null;
            }
        }
    }
    public class CheckSLAStatus
    {
        public CheckSLAStatus(DocumentTable documentTable, IEnumerable<WFTrackerUsersTable> trackerUsers)
        {
            DocumentTable = documentTable;
            TrackerUsers = trackerUsers;
        }

        public DocumentTable DocumentTable { get; set; }
        public IEnumerable<WFTrackerUsersTable> TrackerUsers { get; set; }
    }

    public class ReminderUsers
    {
        public ReminderUsers(DocumentTable documentTable, List<ApplicationUser> users)
        {
            DocumentTable = documentTable;
            Users = users;
        }

        public DocumentTable DocumentTable { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

}