using System;
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
using RapidDoc.Extensions;
using System.Data.Entity.Core.Objects;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web;

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
        protected readonly IWorkflowService _WorkflowService;
        protected readonly ICompanyService _CompanyService;
        protected readonly ISystemService _SystemService;
        protected readonly IIpListService _IpListService;

        public BatchController(IEmplService emplService, IWorkScheduleService workScheduleService, ICompanyService companyService, ISystemService systemService,
            IEmailService emailservice, IDocumentService documentservice, IReviewDocLogService reviewDocLogService,
            IAccountService accountService, IProcessService processService, IReportService reportService, IDepartmentService departmentService, IWorkflowTrackerService workflowtrackerService, IWorkflowService workflowService, IIpListService ipListService)
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
            _CompanyService = companyService;
            _SystemService = systemService;
            _IpListService = ipListService;
        }

        // GET api/<controller>
        //http://sitename/api/batch/8/ATK
        public void Get(int id, string companyId)
        {
            HttpRequestMessage requestMessage = (HttpRequestMessage)HttpContext.Current.Items["MS_HttpRequestMessage"];
            var ipAddress = requestMessage.GetClientIpAddress();

            if (_IpListService.Contains(x => x.Ip == ipAddress))
            {
                CompanyTable company = _CompanyService.FirstOrDefault(x => x.AliasCompanyName == companyId);
                var allDocument = _Documentservice.GetPartial(x => x.CompanyTableId == company.Id).ToList();
                if (allDocument == null)
                    return;
                try
                {
                    switch (id)
                    {
                        case 1:
                            if (!_WorkScheduleService.CheckDayType(_WorkScheduleService.FirstOrDefault(x => x.WorkScheduleName == "8x5").Id, DateTime.UtcNow.Date))
                            {                           
                                var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                                List<CheckSLAStatus> checkData = new List<CheckSLAStatus>();

                                foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Agreement
                                || x.DocumentState == Models.Repository.DocumentState.Execution || x.DocumentState == Models.Repository.DocumentState.OnSign).ToList())
                                {
                                    var checkUser = _Documentservice.GetUsersSLAStatus(document, SLAStatusList.Warning).ToList();                             
                                    List<WFTrackerUsersTable> tmpTrackerUsers = new List<WFTrackerUsersTable>();
                                    foreach (var item in checkUser)
                                    {
                                    
                                        ApplicationUser user = users.FirstOrDefault(x => x.Id == item.UserId);
                                        if (user != null && (_ReviewDocLogService.isArchive(document.Id, "", user) == false || document.DocType == DocumentType.Task))
                                        {                                          
                                            tmpTrackerUsers.Add(item);                                   
                                        }
                                    }
                                    checkData.Add(new CheckSLAStatus(document, tmpTrackerUsers));
                                }

                                foreach (var user in users)
                                {
                                    var userDocuments = checkData.Where(x => x.TrackerUsers.Any(a => a.UserId == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();

                                    if (userDocuments.Count() > 0)
                                        _Emailservice.SendSLAWarningEmail(user.Id, userDocuments);
                                }
                            }                                                
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
                            break;
                        case 2:
                            if (!_WorkScheduleService.CheckDayType(_WorkScheduleService.FirstOrDefault(x => x.WorkScheduleName == "8x5").Id, DateTime.UtcNow.Date))
                            {
                                var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                                List<CheckSLAStatus> checkData = new List<CheckSLAStatus>();

                                foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Agreement
                                || x.DocumentState == Models.Repository.DocumentState.Execution || x.DocumentState == Models.Repository.DocumentState.OnSign).ToList())
                                {
                                    var checkUser = _Documentservice.GetUsersSLAStatus(document, SLAStatusList.Disturbance).ToList();

                                    List<WFTrackerUsersTable> tmpTrackerUsers = new List<WFTrackerUsersTable>();
                                    foreach (var item in checkUser)
                                    {
                                        ApplicationUser user = users.FirstOrDefault(x => x.Id == item.UserId);
                                        if (user != null && (_ReviewDocLogService.isArchive(document.Id, "", user) == false || document.DocType == DocumentType.Task))
                                        {
                                            tmpTrackerUsers.Add(item);
                                        }
                                    }

                                    checkData.Add(new CheckSLAStatus(document, tmpTrackerUsers));
                                }

                                foreach (var user in users)
                                {
                                    var userDocuments = checkData.Where(x => x.TrackerUsers.Any(a => a.UserId == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();
                                    if (userDocuments.Count() > 0)
                                        _Emailservice.SendSLADisturbanceEmail(user.Id, userDocuments);
                                }
                            }
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
                            break;
                        case 3:
                            foreach (var document in allDocument.Where(x => x.DocumentState == Models.Repository.DocumentState.Closed
                                || x.DocumentState == Models.Repository.DocumentState.Cancelled
                                || x.DocumentState == Models.Repository.DocumentState.Created
                                || (x.DocumentState == Models.Repository.DocumentState.OnSign && x.DocType != DocumentType.Task)).ToList())
                            {
                                IEnumerable<ReviewDocLogTable> reviewDocuments = _ReviewDocLogService.GetPartial(x => x.DocumentTableId == document.Id && x.isArchive == false && x.isFavorite == false).ToList();

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
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
                            break;
                        case 4:
                            if (!_WorkScheduleService.CheckDayType(_WorkScheduleService.FirstOrDefault(x => x.WorkScheduleName == "8x5").Id, DateTime.UtcNow.Date))
                            {
                                USR_TAS_DailyTasks_Table childTask = new USR_TAS_DailyTasks_Table();
                                ApplicationDbContext context = new ApplicationDbContext();
                                var users = _AccountService.GetPartial(x => x.Email != null && x.Enable == true).ToList();
                                List<ReminderUsers> checkData = new List<ReminderUsers>();

                                foreach (var document in allDocument.Where(x => (x.DocumentState == Models.Repository.DocumentState.Agreement
                                || x.DocumentState == Models.Repository.DocumentState.Execution
                                || x.DocumentState == Models.Repository.DocumentState.OnSign)).ToList())
                                {
                                    if (document.DocType == DocumentType.Task)
                                    {
                                        childTask = context.USR_TAS_DailyTasks_Table.Where(x => x.RefDocumentId == document.Id && x.ReportText == null).FirstOrDefault();

                                        WFTrackerTable checkHours = _WorkflowTrackerService.FirstOrDefault(w => w.DocumentTableId == document.Id);
                                        DateTime endDateTime = (DateTime)_Documentservice.GetSLAPerformDate(document.Id, checkHours.StartDateSLA, checkHours.SLAOffset);
                                        if (DateTime.UtcNow < endDateTime)
                                        {
                                            TimeSpan ts = (DateTime)_Documentservice.GetSLAPerformDate(document.Id, checkHours.StartDateSLA, checkHours.SLAOffset) - DateTime.UtcNow;
                                            if (ts.Days <= System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames.Length * 2)
                                            {
                                                WorkScheduleTable workScheduleTable = _WorkScheduleService.FirstOrDefault(x => x.WorkScheduleName == "8x5");
                                                int countDays = 0;
                                                DateTime bufDateTime = DateTime.UtcNow;
                                                while (bufDateTime.Date <= endDateTime.Date)
                                                {
                                                    if (!_WorkScheduleService.CheckDayType(workScheduleTable.Id, bufDateTime.Date))
                                                        countDays++;
                                                    bufDateTime = bufDateTime.AddDays(1);
                                                }

                                                if (countDays > 4 && countDays != System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames.Length)
                                                    continue;
                                            }
                                            else
                                                continue;
                                        }



                                        if (childTask != null)
                                            continue;
                                    }

                                    List<ApplicationUser> result = new List<ApplicationUser>();
                                    var usersReminder = _Documentservice.GetSignUsersDirect(document).ToList();

                                    foreach (var item in usersReminder)
                                    {
                                        if (_ReviewDocLogService.isArchive(document.Id, "", item) == false || document.DocType == DocumentType.Task)
                                        {
                                            if (document.DocType == DocumentType.Task)
                                            {
                                                var tracker = _WorkflowTrackerService.GetPartial(x => x.TrackerType == TrackerType.Waiting && x.DocumentTableId == document.Id).OrderByDescending(x => x.LineNum).FirstOrDefault();
                                                if (tracker != null && !tracker.Users.Any(x => x.UserId == item.Id))
                                                    continue;
                                            }

                                            result.Add(item);
                                        }
                                    }

                                    checkData.Add(new ReminderUsers(document, result));
                                }

                                foreach (var user in users)
                                {
                                    var userDocuments = checkData.Where(x => x.Users.Any(a => a.Id == user.Id)).GroupBy(b => b.DocumentTable).Select(group => group.Key).ToList();
                                    if (userDocuments.Count() > 0)
                                        _Emailservice.SendReminderEmail(user, userDocuments);
                                }
                            }
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
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
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
                            break;
                        /*case 6:
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
                            break;*/
                        case 7:
                            var userlist = _EmplService.GetPartialIntercompany(x => x.Enable == true && x.CompanyTableId == company.Id).ToList();

                            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                            RoleManager<ApplicationRole> roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));

                            foreach (var user in userlist)
                            {
                                DepartmentTable rolesDepartment = _DepartmentService.FirstOrDefault(x => x.Id == user.DepartmentTableId && x.CompanyTableId == company.Id);

                                if (rolesDepartment != null && !String.IsNullOrEmpty(rolesDepartment.RequiredRoles))
                                {
                                    string[] arrayStructure = _SystemService.GuidsFromText(rolesDepartment.RequiredRoles);

                                    foreach (var role in arrayStructure)
                                    {
                                        string roleName = roleManager.FindById(role).Name;

                                        if (user.ApplicationUserId != null && !userManager.IsInRole(user.ApplicationUserId, roleName))
                                        {
                                            userManager.AddToRole(user.ApplicationUserId, roleName);
                                        }
                                    }
                                }

                            }
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
                            break;
                        case 8:
                            foreach (var document in allDocument.Where(x => x.DocType == DocumentType.Order && x.DocumentState == DocumentState.Agreement).Join(_WorkflowTrackerService.GetPartial(w => w.TrackerType == TrackerType.Waiting && w.SystemName == "ORDCustomUserAssign"), x => x.Id, w => w.DocumentTableId, (x, w) => new { Doc = x, Tracker = w }).ToList())
                            {
                                if (DateTime.UtcNow > _Documentservice.GetSLAPerformDate(document.Doc.Id, document.Tracker.StartDateSLA, document.Tracker.SLAOffset) && _WorkflowTrackerService.GetPartial(x => x.DocumentTableId == document.Doc.Id && x.TrackerType == TrackerType.NonActive && x.SystemName == "ORDCustomUserAssign").ToList().Count() > 0)
                                {
                                    _WorkflowService.ActiveWorkflowApprove(document.Doc.Id, document.Doc.ProcessTable.TableName, document.Doc.WWFInstanceId, document.Doc.ProcessTableId, new Dictionary<string, object>(), document.Tracker.Users.First().UserId);
                                }
                            }
                            //_Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was completed in the {1} company.", id.ToString(), company.AliasCompanyName), false);
                            break;
                    }
                }
                catch (Exception e)
                {
                    _Emailservice.SendStatusExecutionBatch(String.Format("Procedure {0} was failed in the {1} company. Message is ({2}).", id.ToString(), company.AliasCompanyName, e.Message), true);
                }
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