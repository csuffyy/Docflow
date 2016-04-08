using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Services;
using RapidDoc.Models.DomainModels;
using Microsoft.AspNet.Identity;
using RapidDoc.Models.ViewModels;
using System.Globalization;

namespace RapidDoc.Controllers
{
    public class MonitoringController : BasicController
    {
        private readonly IDocumentService _DocumentService;
        private readonly IDepartmentService _DepartmentService;
        private readonly IEmplService       _EmplService;
        private readonly IWorkflowTrackerService _WorkflowTrackerService;
        private readonly ISystemService _SystemService;

        public MonitoringController(IDocumentService documentService, IDepartmentService departmentService, IEmplService emplService, ICompanyService companyService, IAccountService accountService, IWorkflowTrackerService workflowTrackerService, ISystemService systemService)
            : base(companyService, accountService)
        {
            _DocumentService = documentService;
            _DepartmentService = departmentService;
            _EmplService = emplService;
            _WorkflowTrackerService = workflowTrackerService;
            _SystemService = systemService;
        }

        public ActionResult Index()
        {
            ViewBag.DepartmentList = _DepartmentService.GetDropListDepartmentNull(null);
            return View();
        }

        public ActionResult IndexTask()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = _AccountService.Find(currentUserId);

            var allTasksList = (from document in context.DocumentTable
                                join detailDoc in context.USR_TAS_DailyTasks_Table
                                on document.Id equals detailDoc.DocumentTableId
                                join documentRef in context.DocumentTable
                                on detailDoc.RefDocumentId equals documentRef.Id
                                where document.DocType == DocumentType.Task &&
                                     document.CompanyTableId == currentUser.CompanyTableId &&
                                    detailDoc.RefDocumentId != null &&
                                    ((documentRef.DocType == DocumentType.Order && context.USR_ORD_MainActivity_Table.Any(x => x.DocumentTableId == documentRef.Id)) ||
                                    (documentRef.DocType == DocumentType.IncomingDoc && context.USR_IND_IncomingDocuments_Table.Any(x => x.DocumentTableId == documentRef.Id)) ||
                                    (documentRef.DocType == DocumentType.Protocol && context.USR_PRT_ProtocolDocuments_Table.Any(x => x.DocumentTableId == documentRef.Id)) ||
                                    (documentRef.DocType == DocumentType.Order && context.USK_ORD_MainActivity_Table.Any(x => x.DocumentTableId == documentRef.Id)) ||
                                    (documentRef.DocType == DocumentType.IncomingDoc && context.USK_IND_IncomingDocuments_Table.Any(x => x.DocumentTableId == documentRef.Id)) ||
                                    (documentRef.DocType == DocumentType.Protocol && context.USK_PRT_DirectorateDocuments_Table.Any(x => x.DocumentTableId == documentRef.Id)))
                                select new MonitoringTasksView
                                { 
                                    DocumentNumber = document.DocumentNum,
                                    DocumentState = document.DocumentState,
                                    DocumentRefType = documentRef.DocType,
                                    SignDate = document.ModifiedDate,
                                    CreateDate = document.CreatedDate,
                                    DocumentText = document.DocumentText,
                                    DocumentId = document.Id,
                                    RefDocId = documentRef.Id,
                                    ExecutionDate = detailDoc.ProlongationDate != null ? detailDoc.ProlongationDate.Value : detailDoc.ExecutionDate,
                                    Year = detailDoc.ProlongationDate != null ? detailDoc.ProlongationDate.Value.Year.ToString() : detailDoc.ExecutionDate.Value.Year.ToString()
                                }).ToList();

            foreach (var item in allTasksList)
            {
                item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.ExecutionDate.Value.Month);
                item.MonthNumber = item.ExecutionDate.Value.Month;
                if (item.DocumentState == DocumentState.Closed)
                {
                    WFTrackerTable signTrack = _WorkflowTrackerService.FirstOrDefault(x => x.DocumentTableId == item.DocumentId && x.SignDate != null);
                    item.SignDate = signTrack.SignDate;
                    item.TaskType = item.SignDate.Value.Date <= item.ExecutionDate ? ReportExecutionType.Done : ReportExecutionType.OverDate;
                    item.SignDateTime = _SystemService.ConvertDateTimeToLocal(currentUser, signTrack.SignDate.Value);
                }
                else
                {
                    item.TaskType = DateTime.UtcNow.Date <= item.ExecutionDate ? ReportExecutionType.NoneDone : ReportExecutionType.OverDate;
                }

                switch (item.DocumentRefType)
                {

                    case DocumentType.Order:
                        USR_ORD_MainActivity_Table order = context.USR_ORD_MainActivity_Table.FirstOrDefault(x => x.DocumentTableId == item.RefDocId);
                        item.DocumentText = String.Format(@"({0})/({1})", order.OrderNum, order.Subject);
                        break;
                    case DocumentType.IncomingDoc:
                        USR_IND_IncomingDocuments_Table incoming = context.USR_IND_IncomingDocuments_Table.FirstOrDefault(x => x.DocumentTableId == item.RefDocId);
                        item.DocumentText = String.Format(@"({0})/({1})", incoming.IncomingDocNum, incoming.DocumentSubject);
                        break;
                    case DocumentType.Protocol:
                        USR_PRT_ProtocolDocuments_Table protocol = context.USR_PRT_ProtocolDocuments_Table.FirstOrDefault(x => x.DocumentTableId == item.RefDocId);
                        item.DocumentText = protocol.Subject;
                        break;
                }

            }
            return View(allTasksList);
        }

        public List<string> GetParentListDepartment(List<DepartmentTable> departmentList)
        {
            List<string> listdepartmentId = new List<string>();
            List<string> listdepartmentBufId = new List<string>();

            foreach (DepartmentTable depId in departmentList)
            {
                listdepartmentId.Add(depId.DepartmentName);
                List<DepartmentTable> departmentTable = _DepartmentService.GetPartial(x => x.ParentDepartmentId == depId.Id).ToList();
                listdepartmentBufId = this.GetParentListDepartment(departmentTable);
                listdepartmentId = listdepartmentId.Concat(listdepartmentBufId).Distinct().OrderBy(x => x).ToList();
            }

            return listdepartmentId;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult PerformanceDepartment(String departmentString)
        {
            List<string> listdepartmentId = new List<string>();
          
            Guid? departmentId = new Guid();
            if (departmentString == "")
            {
                ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
                var empl = _EmplService.GetEmployer(user.Id, user.CompanyTableId);

                if (empl != null && empl.DepartmentTable != null)
                {
                    departmentId = empl.DepartmentTable.Id;
                }
                else
                {
                    departmentId = _DepartmentService.FirstOrDefault(x => x.Id != null).Id;
                }
            }
            else
            {
                departmentId = new Guid(departmentString);
            }

            List<DepartmentTable> departmentTableList = _DepartmentService.GetPartial(x => x.Id == departmentId).ToList();
            listdepartmentId = this.GetParentListDepartment(departmentTableList);

            ApplicationDbContext context = new ApplicationDbContext();
            DateTime startDate = DateTime.Now.AddDays(-30);
            DateTime endDate = DateTime.Now.AddDays(1);

            var flatData = (from wfTracker in context.WFTrackerTable
                            from user in context.Users.Where(x => x.Id == wfTracker.SignUserId).DefaultIfEmpty()
                where wfTracker.ExecutionStep == true
                && (wfTracker.CreatedDate >= startDate && wfTracker.CreatedDate <= endDate)/*&&  wfTracker.TrackerType == TrackerType.Approved*/
                select new ReportPerformanceDepartmentModel
                {
                    UserName = user.UserName,
                    ActivityName = wfTracker.ActivityName,
                    SignDate = wfTracker.SignDate,
                    DocumentId = wfTracker.DocumentTableId,
                    Date = wfTracker.StartDateSLA,
                    SLAOffset = wfTracker.SLAOffset,
                    TrackerType = wfTracker.TrackerType,
                    SignUserId = wfTracker != null ? wfTracker.SignUserId : null,
                    WftId = wfTracker.Id
                }).ToList();
         
            foreach (var item in flatData.Where(x => x.SLAOffset > 0))
            {
                item.PerformDate = _DocumentService.GetSLAPerformDate(item.DocumentId, item.Date, item.SLAOffset);
            }

            var barData = (from data in flatData.ToList()
                           join users in context.Users
                                on data.SignUserId equals users.Id
                           join epml in context.EmplTable
                               on users.Id equals epml.ApplicationUserId
                           join department in context.DepartmentTable.Where(x => listdepartmentId.Contains(x.DepartmentName))
                               on epml.DepartmentTable.Id equals department.Id
                            where data.SignUserId != null &&
                            data.TrackerType == TrackerType.Approved
                            group data by new
                            {
                                data.UserName
                            } into gflat
                            select new
                            {
                                UserName = gflat.Key.UserName,
                                Count = gflat.Count(),
                                CountError = gflat.Count(x => x.SignDate > x.PerformDate && x.PerformDate != null)
                            }).ToList();

            var pieData = (from piedata in flatData.ToList()
                           group piedata by new
                           {
                               piedata.DocumentId,
                               piedata.SignUserId,
                               piedata.SignDate,
                               piedata.PerformDate
                           } into piegflat
                           select new
                           {                          
                               piegflat.Key.DocumentId,
                               piegflat.Key.SignUserId,
                               piegflat.Key.SignDate,
                               piegflat.Key.PerformDate
                           }).ToList();

            string[] barEmplList = barData.Select(x => x.UserName).ToArray();
            int[] barCountList = barData.Select(x => x.Count).ToArray();
            int[] barCountErrorList = barData.Select(x => x.CountError).ToArray();

            int pieCountList = pieData.Where(x => x.SignDate < x.PerformDate && x.PerformDate != null && x.SignUserId != null).Count();
            int pieCountErrorList = pieData.Where(x => x.SignDate > x.PerformDate && x.PerformDate != null && x.SignUserId != null).Count();
            int pieOpenCountList = pieData.Where(x => DateTime.UtcNow < x.PerformDate && x.PerformDate != null && x.SignUserId == null).Count();
            int pieOpenCountErrorList = pieData.Where(x => DateTime.UtcNow > x.PerformDate && x.PerformDate != null && x.SignUserId == null).Count();

            return Json(new { barLabels = barEmplList, barDataCount = barCountList, barDataErrorCount = barCountErrorList,
                              pieCountListValue = pieCountList, pieCountErrorListValue = pieCountErrorList,
                              pieOpenCountListValue = pieOpenCountList, pieOpenCountErrorListValue = pieOpenCountErrorList}, JsonRequestBehavior.AllowGet);
        }
	}

}