﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Repository;
using Microsoft.AspNet.Identity.EntityFramework;


namespace RapidDoc.Models.ViewModels
{
    public class GroupProcessView : BasicCompanyNullView
    {
        [StringLength(40, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "GroupProcessName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string GroupProcessName { get; set; }

        [Display(Name = "NumberSeriesName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string NumberSeriesName { get; set; }
        public Guid? NumberSeriesTableId { get; set; }

        public Guid? GroupProcessParentId { get; set; }

        [Display(Name = "GroupProcessParentName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string GroupProcessParentName { get; set; }
    }

    public class ProcessView : BasicCompanyNullView
    {
        [StringLength(70, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "ProcessName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ProcessName { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Display(Name = "Description", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string Description { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "TableName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string TableName { get; set; }

        public string RoleId { get; set; }
        public string StartReaderRoleId { get; set; }
        public string AfterEndReaderRoleId { get; set; }
        public Guid? GroupProcessTableId { get; set; }
        public Guid? WorkScheduleTableId { get; set; }

        [Display(Name = "Approved", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public bool isApproved { get; set; }

        [Display(Name = "GroupProcessName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string GroupProcessName { get; set; }

        [Display(Name = "WorkScheduleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string WorkScheduleName { get; set; }

        [Display(Name = "RoleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string RoleName { get; set; }

        [Display(Name = "StartReaderRoleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string StartReaderRoleName { get; set; }

        [Display(Name = "AfterEndRoleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string AfterEndRoleName { get; set; }

        [Display(Name = "MandatoryNumberFiles", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public int MandatoryNumberFiles { get; set; }

        [Display(Name = "MandatoryFileTypes", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string MandatoryFileTypes { get; set; }

        [Display(Name = "StartWorkTime", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public TimeSpan StartWorkTime { get; set; }

        [Display(Name = "EndWorkTime", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public TimeSpan EndWorkTime { get; set; }

        [Display(Name = "DocSize", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public int DocSize { get; set; }

        [Display(Name = "MandatoryDocDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? MandatoryDocDate { get; set; }
        
        [Display(Name = "DocumentType", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DocumentType DocType { get; set; }
    }

    public class DocumentComposite
    {
        public ProcessView ProcessView { get; set; }
        public DocumentView DocumentView { get; set; }
        public IEnumerable<WFTrackerListView> WFTrackerItems { get; set; }
        public dynamic docData { get; set; }
        public Guid fileId { get; set; }
        public IEnumerable<FileTable> ProcessTemplates { get; set; }
    }

    public class DocumentView : BasicCompanyNullView
    {
        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "DocumentNum", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DocumentNum { get; set; }

        [Display(Name = "GroupProcesses", ResourceType = typeof(UIElementRes.UIElement))]
        public string GroupProcessName { get; set; }

        [Display(Name = "Processes", ResourceType = typeof(UIElementRes.UIElement))]
        public string ProcessName { get; set; }
        public Guid? ProcessTableId { get; set; }

        [Display(Name = "DocumentState", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DocumentState DocumentState { get; set; }

        [Display(Name = "CurrentActivityName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ActivityName { get; set; }

        public DocumentType DocType { get; set; }

        [Display(Name = "DocumentText", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DocumentText { get; set; }

        public Guid FileId { get; set; }

        public SLAStatusList SLAStatus { get; set; }

        public bool isNotReview { get; set; }

        public bool isArchive { get; set; }

        public bool isFavorite { get; set; }

        public bool isSign { get; set; }

        public bool isShow { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string FullName { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "TitleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string TitleName { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "DepartmentName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DepartmentName { get; set; }
        [Display(Name = "Notify", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public bool IsNotified { get; set; }
        [Display(Name = "Share", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public bool Share { get; set; }
        public bool Cancel { get; set; }
        public Guid? CancelDocumentId { get; set; }
        public bool Executed { get; set; }
    }

    public class DocumentTaskView : BasicCompanyNullView
    {
        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "DocumentNum", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DocumentNum { get; set; }

        [Display(Name = "GroupProcesses", ResourceType = typeof(UIElementRes.UIElement))]
        public string GroupProcessName { get; set; }

        [Display(Name = "Processes", ResourceType = typeof(UIElementRes.UIElement))]
        public string ProcessName { get; set; }
        public Guid? ProcessTableId { get; set; }

        [Display(Name = "DocumentState", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DocumentState DocumentState { get; set; }

        [Display(Name = "CurrentActivityName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ActivityName { get; set; }

        public DocumentType DocType { get; set; }

        [Display(Name = "DocumentText", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DocumentText { get; set; }

        public SLAStatusList SLAStatus { get; set; }

        public bool isNotReview { get; set; }

        public bool isArchive { get; set; }

        public bool isSign { get; set; }

        public bool isShow { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string FullName { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "TitleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string TitleName { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "DepartmentName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DepartmentName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата исполнения")]
        public DateTime? ExecutionDate { get; set; }
    }

    public class WFTrackerListView : BasicCompanyNullView
    {
        public int RowNum { get; set; }

        [Display(Name = "ActivityName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ActivityName { get; set; }

        public string ActivityID { get; set; }

        public string ParallelID { get; set; }

        [Display(Name = "Executor", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string Executors { get; set; }

        [Display(Name = "PerformToDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? PerformToDate { get; set; }

        public DateTime? StartDateSLA { get; set; }

        public int SLAOffset { get; set; }

        [Display(Name = "Date", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? SignDate { get; set; }

        public string SignUserId { get; set; }

        public bool ManualExecutor { get; set; }

        public TrackerType TrackerType { get; set; }

        public string Comments { get; set; }

        public string AdditionalText { get; set; }

        public DateTime? LastNotificationDate { get; set; }
    }

    public class CommentView : BasicCompanyNullView
    {
        [Display(Name = "FirstName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string EmplName { get; set; }

        [Display(Name = "TitleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string TitleName { get; set; }

        [Display(Name = "Comment", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string Comment { get; set; }
    }

    public class HistoryUserView : BasicView
    {
        public HistoryType HistoryType { get; set; }
        public string DocumentNum { get; set; }
        public string ProcessName { get; set; }
        public Guid DocumentTableId { get; set; }
        public string ApplicationCreatedUser { get; set; }
        public string Description { get; set; }
        public string CreatedEmplName { get; set; }
        public string CreatedEmplTitle { get; set; }
        public string CreatedEmplDepartment { get; set; }
    }

    public class SearchView : BasicView
    {
        public string DocumentText { get; set; }
        public Guid DocumentTableId { get; set; }
        public string DocumentNum { get; set; }
        public string ProcessName { get; set; }
        public bool isShow { get; set; }
        public string CreatedUserName { get; set; }
    }

    public class SearchFormView
    {
        public string SearchText { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }

        [Display(Name = "CreatedBy", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public Guid? CreatedEmplTableId { get; set; }

        [Display(Name = "CompanyName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public Guid? CompanyTableId { get; set; }

        [Display(Name = "ProcessName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public Guid? ProcessTableId { get; set; }
        public string HTMLString { get; set; }
        public bool NoMoreData { get; set; }
    }

    public class WFUserFunctionResult
    {
        public List<WFTrackerUsersTable> Users { get; set; }
        public bool Skip { get; set; }
    }

    public class RequestBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public RequestFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class OfficeMemoBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public OfficeMemoFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class TaskBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public TaskFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class OrderBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public OrderFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class IncomingBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public IncomingFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class OutcomingBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public OutcomingFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class AppealBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public AppealFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }
    }

    public class ProtocolBaseView
    {
        [Display(Name = "Grouping", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public ProtocolFilterType FilterType { get; set; }

        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime? EndDate { get; set; }

        [Display(Name = "ProcessName", ResourceType = typeof(FieldNameRes.FieldNameResource))]        
        public string ProcessName { get; set; }

        public Guid? ProcessTableId { get; set; }
    }

    public class DocumentBaseView : BasicCompanyNullView
    {
        public string DocumentNum { get; set; }
        public string GroupProcessName { get; set; }
        public string ProcessName { get; set; }
        public Guid? ProcessTableId { get; set; }
        public DocumentState DocumentState { get; set; }
        public string ActivityName { get; set; }
        public DocumentType DocType { get; set; }
        public bool isNotReview { get; set; }
        public bool isArchive { get; set; }
        public bool isSign { get; set; }
        public bool isShow { get; set; }
        public string FullName { get; set; }
        public string TitleName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsNotified { get; set; }
        public bool Cancel { get; set; }
        public bool Addition { get; set; }
        public bool Executed { get; set; }
        public string UserName { get; set; }
        public string ItemCaseNumber { get; set; }
        public string ItemCaseName { get; set; }
        public string DocumentTitle { get; set; }
        public Guid DocumentRefId { get; set; }
        public string ProcessTableName { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public List<WFTrackerUsersTable> Users { get; set; }
        public TrackerType WFTrackerType { get; set; }
        public string DocumentText { get; set; }
        public Folder Folder { get; set; }
        public CategoryPerson CategoryPerson { get; set; }
        public Guid? ProtocolFolderId  { get; set; }
        public string ProtocolCode { get; set; }
        public string TrackerActivityName { get; set; }
        public string SignUser { get; set; }
        public string InOutOrganization { get; set; }
        
    }

    public class DocumentBaseProtocolFolderView : BasicCompanyNullView
    {
        public List<DocumentBaseView> documentBaseList { get; set; }
        public Guid ProtocolFoldersId { get; set; }
        public string ProtocolFolderName { get; set; }
        public Guid? ProtocolFoldersParentId { get; set; }
    }

    public class DocumentBaseProtocolTasksView : BasicCompanyNullView
    {
        public string DocumentNum { get; set; }
        public string ProtocolNum { get; set; }
        public ProtocolTaskDocumentBaseStatus TaskStatus { get; set; }
        public string DepartmentName { get; set; }
        public string UserName { get; set; }
        public string CreateTaskDate { get; set; }
    }

    public class DocumentBaseProtocolProlongTasksView : BasicCompanyNullView
    {
        public string DocumentNum { get; set; }
        public ProtocolProlongTaskDocumentBaseStatus TaskStatus { get; set; }
        public string DepartmentName { get; set; }
        public string UserName { get; set; }
        public string CreateProlongTaskDate { get; set; }
    }

    public class DocumentSubscriptionListView : BasicView
    {
        [Display(Name = "Номер документа")]
        public string DocumentNum { get; set; }

        public Guid DocumentTableId { get; set; }       

        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; }

        [Display(Name = "Имя создавшего")]
        public string CreateUserName { get; set; }

        public string UserId { get; set; }

        public DateTime? LogDate { get; set; }
    }
}