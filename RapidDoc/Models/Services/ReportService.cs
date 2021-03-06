﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Activities;
using System.Activities.XamlIntegration;
using System.IO;
using System.Xaml;
using System.Reflection;
using System.Linq.Expressions;
using System.Drawing;
using RapidDoc.Extensions;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Controllers;
using RapidDoc.Activities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Excel = Microsoft.Office.Interop.Excel;


namespace RapidDoc.Models.Services
{
    public interface IReportService
    {
        Activity GetActivity(ProcessTable processTable);
        List<ReportProcessesView> GetActivityStages(Dictionary<Type, int> codeActivitiesTypes, Activity activity,
            ProcessTable processTable);
        List<string> GetParentListDepartment(List<DepartmentTable> departmentList);
        int GetDepartmentTaskReport(List<DepartmentTable> departmentTable, Dictionary<string, int> blockDepartment,
            List<TaskReportModel> taskReportModel, Excel.Worksheet excelWorksheet, int rowCount, bool usesBlock, int worksheetNumber, bool otherCompany, string headerText = "");
        void GenerateCorrespondenceReport(List<CorrespondenceReport> resultList, OutcomingTopicTypeKZHC[] topicOrder, Excel.Worksheet excelWorksheet);
    }
    
    public class ReportService: IReportService
    {
        private IUnitOfWork _Uow;
        private readonly IDocumentService _DocumentService;
        private readonly IDepartmentService _DepartmentService;
        private readonly IEmplService _EmplService;
        private readonly IAccountService _AccountService;
        private readonly ISystemService _SystemService;

        public ReportService(IUnitOfWork uow, IDocumentService documentService, IDepartmentService departmentService, IEmplService emplService, IAccountService accountService, ISystemService systemService)
        {
            _Uow = uow;
            _DocumentService = documentService;
            _DepartmentService = departmentService;
            _EmplService = emplService;
            _AccountService = accountService;
            _SystemService = systemService; 
        }
        
        public Activity GetActivity(ProcessTable processTable)
        {
            Activity activity;

            FileTable wfXamlFile = _DocumentService.GetAllXAMLDocument(processTable.Id).OrderByDescending(x => Convert.ToInt32(x.Version)).FirstOrDefault();

            using (Stream stream = new MemoryStream(wfXamlFile.Data))
            {
                using(var xamlReader = new XamlXmlReader(stream, new XamlXmlReaderSettings{LocalAssembly = Assembly.GetExecutingAssembly()}))
	            {
		            activity = ActivityXamlServices.Load(xamlReader, new ActivityXamlServicesSettings { CompileExpressions = true }) as DynamicActivity;
	            }
            }

            return activity;
        }

        public List<string> GetParentListDepartment(List<DepartmentTable> departmentList)
        {
            List<string> listDepartmentId = new List<string>();
            List<string> listDepartmentBufId = new List<string>();

            foreach (DepartmentTable depId in departmentList)
            {
                listDepartmentId.Add(depId.DepartmentName);
                List<DepartmentTable> departmentTable = _DepartmentService.GetPartial(x => x.ParentDepartmentId == depId.Id).ToList();
                listDepartmentBufId = this.GetParentListDepartment(departmentTable);
                listDepartmentId = listDepartmentId.Concat(listDepartmentBufId).Distinct().OrderBy(x => x).ToList();
            }

            return listDepartmentId;
        }


        public List<ReportProcessesView> GetActivityStages(Dictionary<Type, int> codeActivitiesTypes, Activity activity, ProcessTable processTable)
        {
            List<ReportProcessesView> processesList = new List<ReportProcessesView>();
            List<EmplTable> namesList = new List<EmplTable>();
            string filterText = String.Empty, stageName = String.Empty;
            FilterType filterType = FilterType.Other;
            ApplicationDbContext contextDb = new ApplicationDbContext();
            Color color = Color.LightGreen;
            int item = 0;
            
            if (codeActivitiesTypes.TryGetValue(activity.GetType(), out item))
            {
                switch (item)
                {
                    case 0:
                    case 2:
                    case 5:
                    case 6:
                    case 7:
                        stageName = activity.DisplayName;
                        break;
                    case 1:
                        
                            var activityStaffStructure = activity as WFChooseStaffStructure;
                            var activityExpressionStaff = activityStaffStructure.inputPredicate.Expression as Microsoft.CSharp.Activities.CSharpValue<Expression<Func<EmplTable, bool>>>;

                            if (activityExpressionStaff != null)
                            {
                                stageName = activity.DisplayName;
                                filterText = activityExpressionStaff.ExpressionText;
                                filterType = FilterType.Predicate;
                                if (filterText.Contains("dynamic"))
                                {
                                    namesList.Add(new EmplTable  {FirstName = "Dynamic"});
                                }
                                else
                                {
                                    System.Linq.Expressions.Expression expressionTree = activityExpressionStaff.GetExpressionTree();
                                    dynamic dynamicExpression = expressionTree;
                                    Expression<Func<EmplTable, bool>> expressionEmpl = dynamicExpression.Body.Operand;
                                    if (expressionEmpl != null)
                                    {
                                        namesList = _EmplService.GetPartialIntercompany(expressionEmpl).Where(x => x.Enable == true).ToList();
                                        if (namesList.Count <= 0)
                                            color = Color.LightPink;
                                    }
                                    else
                                        color = Color.LightPink;
                                }
                            }                     
                        break;
                    case 3:
                        var activitySpecifyUser = activity as WFChooseSpecificUser;
                        var activityExpressionSpecific = activitySpecifyUser.inputUserName.Expression as System.Activities.Expressions.Literal<string>;

                        if (activityExpressionSpecific != null)
                        {
                            stageName = activity.DisplayName;
                            filterText = activityExpressionSpecific.Value;
                            filterType = FilterType.Login;

                            ApplicationUser userTable = _AccountService.FirstOrDefault(x => x.UserName == filterText && x.Enable == true);
                            if (userTable != null && filterText.Length > 0)
                            {
                                namesList.Add(_EmplService.FirstOrDefault(x => x.ApplicationUserId == userTable.Id));
                            }
                            else
                                color = Color.LightPink;
                        }

                        break;
                    case 4:
                        var activityRoleUser = activity as WFChooseRoleUser;
                        var activityExpressionRole = activityRoleUser.inputRoleName.Expression as System.Activities.Expressions.Literal<string>;

                        if (activityExpressionRole != null)
                        {
                            stageName = activity.DisplayName;
                            filterText = activityExpressionRole.Value;
                            filterType = FilterType.Role;


                            RoleManager<ApplicationRole> RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(contextDb));
                            if (RoleManager.RoleExists(filterText))
                            {
                                var names = RoleManager.FindByName(filterText).Users;
                                if (names != null && names.Count() > 0)
                                {
                                    foreach (IdentityUserRole name in names)
                                    {
                                        if (_EmplService.Contains(x => x.ApplicationUserId == name.UserId && x.Enable == true))
                                        {
                                            namesList.Add(_EmplService.FirstOrDefault(x => x.ApplicationUserId == name.UserId));
                                        }                                        
                                    }
                                }
                                else
                                    color = Color.LightPink;
                            }
                            else
                                color = Color.LightPink;
                        }

                        break;
                }

                processesList.Add(new ReportProcessesView()
                {
                    Process = processTable,
                    StageName = stageName,
                    FilterType = filterType,
                    FilterText = filterText,
                    Names = namesList,
                    Color = color
                });
            }

            IEnumerator<Activity> list = WorkflowInspectionServices.GetActivities(activity).GetEnumerator();

            while (list.MoveNext())
            {
                var allStepsBuf = processesList.Concat(GetActivityStages(codeActivitiesTypes, list.Current, processTable));
                processesList = allStepsBuf.ToList();
            }

            return processesList;
        }


        public int GetDepartmentTaskReport(List<DepartmentTable> departmentTable, Dictionary<string, int> blockDepartment, List<TaskReportModel> taskReportModel, Excel.Worksheet excelWorksheet, int rowCount, bool usesBlock, int worksheetNumber, bool otherCompany, string headerText = "")
        {
            int item, maxColumns = worksheetNumber == 3 ? 11 : 10, i = 1;
            bool isBlocks = usesBlock;
            List<DepartmentTable> childDepartment = new List<DepartmentTable>();
            if (!String.IsNullOrEmpty(headerText))
            {
                Excel.Range range = excelWorksheet.Range[excelWorksheet.Cells[rowCount, 2], excelWorksheet.Cells[rowCount + 2, maxColumns]];
                    this.AllBordersBlock(range.Borders);                
                range.Merge();
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.Constants.xlCenter;
                excelWorksheet.Cells[rowCount, 2] = headerText;
                rowCount += 2;
            }
            foreach (var department in departmentTable)
            {
                i = 1;
                if (blockDepartment.TryGetValue(department.DepartmentName, out item) && isBlocks == false)
                    continue;

                if (blockDepartment.TryGetValue(department.DepartmentName, out item) && isBlocks == true && ((otherCompany == false) || (otherCompany == true && taskReportModel.Where(x => x.Department.Contains(department.Id.ToString())).Count() > 0)))
                {
                    Excel.Range range = excelWorksheet.Range[excelWorksheet.Cells[rowCount, 2], excelWorksheet.Cells[rowCount, maxColumns]];
                    this.AllBordersBlock(range.Borders);
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(204, 255, 204));
                    range.Merge();
                    excelWorksheet.Cells[rowCount, 2] = department.DepartmentName;
                    rowCount++;
                }
                else if (taskReportModel.Where(x => x.Department.Contains(department.Id.ToString())).Count() > 0)
                {
                    Excel.Range range = excelWorksheet.Range[excelWorksheet.Cells[rowCount, 2], excelWorksheet.Cells[rowCount, maxColumns]];
                    this.AllBordersBlock(range.Borders);
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(253, 233, 217));
                    range.Merge();
                    excelWorksheet.Cells[rowCount, 2] = department.DepartmentName;
                    rowCount++;
                }

                if (taskReportModel.Where(x => x.Department.Contains(department.Id.ToString())).Count() > 0)
                {
                    int startRowBlock = rowCount;                 
                    foreach (var task in taskReportModel.Where(x => x.Department.Contains(department.Id.ToString())))
                    {
                        excelWorksheet.Cells[rowCount, 2] = i++;
                        excelWorksheet.Cells[rowCount, 3] = task.CardNumber;
                        excelWorksheet.Cells[rowCount, 4] = _SystemService.DeleteAllSpecialCharacters(_SystemService.DeleteAllTags(task.TaskDescription));
                        excelWorksheet.Cells[rowCount, 5] = task.PlaneDate.ToString() == "" ? "" : task.PlaneDate.ToShortDateString();

                        if (task.Factdate != null)
                        {
                            DateTime factDate =  (DateTime)task.Factdate;
                            excelWorksheet.Cells[rowCount, 6] = factDate.ToShortDateString();
                        }

                        excelWorksheet.Cells[rowCount, 7] = task.Executor;
                        excelWorksheet.Cells[rowCount, 8] = task.Delegation;
                        switch (task.Status)
                        {
                            case ReportExecutionType.Done:
                                excelWorksheet.Cells[rowCount, 9] = "исполнено";
                                break;
                            case ReportExecutionType.NoneDone:
                                excelWorksheet.Cells[rowCount, 9] = "не исполнено";
                                Excel.Range rangeColor = excelWorksheet.Range[excelWorksheet.Cells[rowCount, 2], excelWorksheet.Cells[rowCount, maxColumns]];
                                rangeColor.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                                break;
                            case ReportExecutionType.OverDate:
                                excelWorksheet.Cells[rowCount, 9] = "исполнено с нарушением сроков";                            
                                break;
                        }

                        excelWorksheet.Cells[rowCount, 10] = _SystemService.DeleteAllTags(task.Text);
                        if (task.DocType == DocumentType.Protocol) 
                            excelWorksheet.Cells[rowCount, 11] = task.DocNum;

                        Excel.Range rangeLink = task.DocType == DocumentType.Protocol ? excelWorksheet.Range[excelWorksheet.Cells[rowCount, 11], excelWorksheet.Cells[rowCount, 11]] : excelWorksheet.Range[excelWorksheet.Cells[rowCount, 3], excelWorksheet.Cells[rowCount, 3]];
                        excelWorksheet.Hyperlinks.Add(rangeLink, String.Format("https://df.altyntau.com/ATK/Document/ShowDocument/{0}", task.DocId), Type.Missing, "Перейти к документу");
                        rowCount++;
                    }
                    Excel.Range range = excelWorksheet.Range[excelWorksheet.Cells[startRowBlock, 2], excelWorksheet.Cells[rowCount - 1, maxColumns]];
                    this.AllBordersBlockTasks(range.Borders);
                    range.HorizontalAlignment = Excel.Constants.xlCenter;
                    range.VerticalAlignment = Excel.Constants.xlCenter;

                    
                }

                childDepartment = _DepartmentService.GetPartial(x => x.ParentDepartmentId == department.Id).ToList();
                if (childDepartment.Count() > 0)
                    rowCount = this.GetDepartmentTaskReport(childDepartment, blockDepartment, taskReportModel, excelWorksheet, rowCount, false, worksheetNumber, otherCompany);
            }
            return rowCount;
            
        }

        private void AllBordersBlock(Excel.Borders _borders)
        {
            _borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlMedium;
            _borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlMedium;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlMedium;
            _borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlMedium;
            //_borders.Color = Color.Black;
        }
        private void AllBordersBlockTasks(Excel.Borders _borders)
        {          
            _borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = Excel.XlBorderWeight.xlMedium;
            _borders[Excel.XlBordersIndex.xlEdgeRight].Weight = Excel.XlBorderWeight.xlMedium;
            _borders[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlMedium;
            _borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlMedium;

            _borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
            _borders[Excel.XlBordersIndex.xlInsideVertical].Weight = Excel.XlBorderWeight.xlThin;
            _borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = Excel.XlBorderWeight.xlThin;
           // _borders.Color = Color.Black;
        }


        public void GenerateCorrespondenceReport(List<CorrespondenceReport> resultList, OutcomingTopicTypeKZHC[] topicOrder, Excel.Worksheet excelWorksheet)
        {
            int rowCount = 2, maxColumns = 18;

            foreach (var topic in topicOrder.ToList())
            {
                int startRowBlock = rowCount + 1, counter = 1;
                var correspondenceDocs = resultList.Where(x => x.IncomingDocumentTopic == topic);

                Excel.Range range = excelWorksheet.Range[excelWorksheet.Cells[rowCount, 1], excelWorksheet.Cells[rowCount, maxColumns]];
                this.AllBordersBlock(range.Borders);
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 255, 0));
                range.Merge();
                range.Font.Bold = true;
                excelWorksheet.Cells[rowCount, 1] = topic.GetDisplayName();
                rowCount++;

                foreach (var item in correspondenceDocs)
                {
                    excelWorksheet.Cells[rowCount, 1] = counter++;
                    excelWorksheet.Cells[rowCount, 2] = item.IncomingDocumentNum;
                    excelWorksheet.Hyperlinks.Add(excelWorksheet.Range[excelWorksheet.Cells[rowCount, 2], excelWorksheet.Cells[rowCount, 2]], String.Format("https://df.altyntau.com/ATK/Document/ShowDocument/{0}", item.IncomingDocumentId), Type.Missing, "Перейти к документу");
                    excelWorksheet.Cells[rowCount, 3] = item.IncomingDateRegistration;
                    excelWorksheet.Cells[rowCount, 4] = item.IncomingDocNum;
                    excelWorksheet.Cells[rowCount, 5] = item.OrganizationName;
                    excelWorksheet.Cells[rowCount, 6] = item.IncomingDocumentSubject;

                    excelWorksheet.Cells[rowCount, 7] = item.IncomingChief;
                    excelWorksheet.Cells[rowCount, 8] = item.TaskDelegation;
                    excelWorksheet.Cells[rowCount, 9] = item.DocumentReaders;
                    excelWorksheet.Cells[rowCount, 10] = item.TaskPlaneDate != null ? item.TaskPlaneDate : "";
                    excelWorksheet.Cells[rowCount, 11] = item.TaskFactDate != null ? item.TaskFactDate : "";

                    switch (item.ExecutionType)
                    {
                        case ReportExecutionType.Done:
                            excelWorksheet.Cells[rowCount, 12] = "исполнено";
                            break;
                        case ReportExecutionType.NoneDone:
                            excelWorksheet.Cells[rowCount, 12] = "не исполнено";
                            Excel.Range rangeColor = excelWorksheet.Range[excelWorksheet.Cells[rowCount, 1], excelWorksheet.Cells[rowCount, maxColumns]];
                            rangeColor.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                            break;
                        case ReportExecutionType.OverDate:
                            excelWorksheet.Cells[rowCount, 12] = "исполнено с нарушением сроков";
                            break;
                    }

                    excelWorksheet.Cells[rowCount, 13] = item.TaskReportText != null ? item.TaskReportText : "";

                    if (!String.IsNullOrEmpty(item.OutcomingDocumentNum))
                    {
                        excelWorksheet.Cells[rowCount, 14] = item.OutcomingDocumentNum;
                        excelWorksheet.Hyperlinks.Add(excelWorksheet.Range[excelWorksheet.Cells[rowCount, 14], excelWorksheet.Cells[rowCount, 14]], String.Format("https://df.altyntau.com/ATK/Document/ShowDocument/{0}", item.OutcomingDocumentId), Type.Missing, "Перейти к документу");
                        excelWorksheet.Cells[rowCount, 15] = item.OutcomingDocumentCreator;
                        excelWorksheet.Cells[rowCount, 16] = item.OutcomingSignUsers != null ? item.OutcomingSignUsers : "";
                        excelWorksheet.Cells[rowCount, 17] = item.OutcomingDateRegistration != null ? item.OutcomingDateRegistration : "";                       
                    }
                    excelWorksheet.Cells[rowCount, 18] = String.IsNullOrEmpty(item.OutcomingInformation) ? "" : _SystemService.DeleteAllTags(item.OutcomingInformation);
                    rowCount++;
                    range = excelWorksheet.Range[excelWorksheet.Cells[startRowBlock, 1], excelWorksheet.Cells[rowCount - 1, maxColumns]];
                    this.AllBordersBlockTasks(range.Borders);
                    range.HorizontalAlignment = Excel.Constants.xlCenter;
                    range.VerticalAlignment = Excel.Constants.xlCenter;                  
                }
            }
        }
    }
}