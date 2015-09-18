using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Infrastructure;
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

        public ReportController(IWorkflowTrackerService workflowTrackerService, IDocumentService documentService, IDepartmentService departmentService, ICompanyService companyService, IAccountService accountService, IProcessService processService, IEmplService emplService, IReportService reportService, IWorkScheduleService workScheduleService, IEmailService emailService, IWorkflowService workflowService)
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

        public ActionResult ReportOfRoutes()
        {
            ViewBag.ProcessList = _ProcessService.GetDropListProcessNull(null);
            return View();
        }

        public ActionResult PdfReport(Guid id, Guid? processId)
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
            foreach(var item in files)
            {
                if(!_DocumentService.FileReplaceContains(item.Id))
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

            return new ViewAsPdf("PdfReport", documentView)
            {
                PageSize = Size.A4,
                FileName = String.Format("{0}.pdf", docTable.DocumentNum)
            };
        }

        public ActionResult PdfReportCZ(Guid id, Guid? processId)
        {
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DocumentTable docTable = _DocumentService.FirstOrDefault(x => x.Id == id);
            ProcessTable process = _ProcessService.FirstOrDefault(x => x.Id == processId);
            var documentView = _DocumentService.GetDocumentView(docTable.RefDocumentId, process.TableName);
            EmplTable emplTable = _EmplService.GetEmployer(docTable.ApplicationUserCreatedId, process.CompanyTableId);
            var trackersAssign = _WorkflowTrackerService.GetPartialView(x => x.DocumentTableId == id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled), timeZoneInfo, docTable.DocType);

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
            ViewBag.DocState = docTable.DocumentState;
            ViewBag.TableName = docTable.ProcessTable.TableName;
            ViewBag.DocumentNum = docTable.DocumentNum;
            ViewBag.AliasCompanyName = docTable.AliasCompanyName;
            ViewBag.Id = docTable.Id;
            ViewBag.Process = process;

            return new ViewAsPdf("PdfReportCZ", documentView)
            {
                PageSize = Size.A4,
                FileName = String.Format("{0}.pdf", docTable.DocumentNum)
            };
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
                              join empl in context.EmplTable on document.ApplicationUserCreatedId equals empl.ApplicationUserId
                              join title in context.TitleTable on empl.TitleTableId equals title.Id
                              join department in context.DepartmentTable.Where(x => listdepartmentId.Contains(x.DepartmentName)) on empl.DepartmentTableId equals department.Id
                              where wfTracker.CreatedDate >= model.StartDate && wfTracker.CreatedDate <= model.EndDate
                              //&& (document.DocumentNum == "RD0008280" || document.DocumentNum == "RDK000270")
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
                                  TitleName = title.TitleName
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
              
                if (_WorkScheduleService.CheckDayType(workScheduleTable.Id, signDateWorkStartTime) == false && signDateWorkStartTime < item.SignDate.Value )
                {
                    if (c < s)
                    {
                        cacheMinutes += Convert.ToInt32((item.SignDate.Value - signDateWorkStartTime).TotalMinutes);
                    }
                    else if(c == s)
                        cacheMinutes += Convert.ToInt32((item.SignDate.Value - item.Date.Value).TotalMinutes);
                }
                if(c < s)
                {
                    if (_WorkScheduleService.CheckDayType(workScheduleTable.Id, createDateWorkEndTime) == false && createDateWorkEndTime > item.Date.Value)
                    {
                        cacheMinutes += Convert.ToInt32((createDateWorkEndTime - item.Date.Value).TotalMinutes);
                        
                    }

                    c++;
                }
                 while (c < s)
                 {
                     DateTime date = new DateTime(year,1,1).AddDays(c -1);
                     if (_WorkScheduleService.CheckDayType(workScheduleTable.Id, new DateTime(year,1,1).AddDays(c -1)) == false)
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

                if(line.SLAOffset > 0)
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
                       join empl in context.EmplTable on user.Id equals empl.ApplicationUserId
                       join title in context.TitleTable on empl.TitleTableId equals title.Id
                       join department in context.DepartmentTable.Where(x => listdepartmentId.Contains(x.DepartmentName)) on empl.DepartmentTableId equals department.Id
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
    }
  /*  public class PdfReport
    {
        public string OrderNum { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Subject { get; set; }
        public string MainField { get; set; }
        public string MainFieldTranslate { get; set; }
        public string SignTitle { get; set; }
        public string SignName { get; set; }
    }*/
}