﻿using RapidDoc.Attributes.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RapidDoc.Models.Repository;
using System.ComponentModel.DataAnnotations.Schema;
using RapidDoc.Models.Services;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RapidDoc.Models.DomainModels
{
    public class GroupProcessTable : BasicTable
    {
        [StringLength(40)] 
        [Required]
        public string GroupProcessName { get; set; }

        public Guid? NumberSeriesTableId { get; set; }
        public virtual NumberSeriesTable NumberSeriesTable { get; set; }

        [ForeignKey("GroupProcessTableParent")]
        public Guid? GroupProcessParentId { get; set; }
        public virtual GroupProcessTable GroupProcessTableParent { get; set; }

        public string NumberSeriesName
        {
            get
            {
                if (this.NumberSeriesTable != null)
                {
                    return this.NumberSeriesTable.NumberSeriesName;
                }

                return string.Empty;
            }
        }

        public IEnumerable<ProcessTable> ProcessTables { get; set; }
    }

    public class ProcessTable : BasicCompanyNullTable
    {
        [StringLength(70)]
        [Required]
        public string ProcessName { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        [StringLength(256)]
        [Required]
        public string TableName { get; set; }

        public bool isApproved { get; set; }

        public string RoleId { get; set; }
        public virtual IdentityRole IdentityRole { get; set; }

        public string GroupProcessName
        {
            get
            {
                if (this.GroupProcessTable != null)
                {
                    return this.GroupProcessTable.GroupProcessName;
                }

                return string.Empty;
            }
        }

        public string WorkScheduleName
        {
            get
            {
                if (this.WorkScheduleTable != null)
                {
                    return this.WorkScheduleTable.WorkScheduleName;
                }

                return string.Empty;
            }
        }

        public Guid? WorkScheduleTableId { get; set; }
        public virtual WorkScheduleTable WorkScheduleTable { get; set; }

        public Guid? GroupProcessTableId { get; set; }
        public virtual GroupProcessTable GroupProcessTable { get; set; }
    }

    public class DocumentTable : BasicCompanyNullTable
    {
        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "DocumentNum", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DocumentNum { get; set; }

        public Guid ProcessTableId { get; set; }
        public virtual ProcessTable ProcessTable { get; set; }

        public Guid EmplTableId { get; set; }
        public virtual EmplTable EmplTable { get; set; }

        [Display(Name = "DocumentText", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DocumentText { get; set; }

        public Guid RefDocumentId { get; set; }

        public Guid WWFInstanceId { get; set; }

        public Guid FileId { get; set; }

        [Display(Name = "DocumentState", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DocumentState DocumentState { get; set; }

        [Display(Name = "GroupProcesses", ResourceType = typeof(UIElementRes.UIElement))]
        public string GroupProcessName 
        {
            get
            {
                if (this.ProcessTable != null && this.ProcessTable.GroupProcessTable != null)
                {
                    return this.ProcessTable.GroupProcessTable.GroupProcessName;
                }

                return string.Empty;
            }
        }

        [Display(Name = "Processes", ResourceType = typeof(UIElementRes.UIElement))]
        public string ProcessName
        {
            get
            {
                if (this.ProcessTable != null)
                {
                    return this.ProcessTable.ProcessName;
                }

                return string.Empty;
            }
        }

        [Display(Name = "CurrentActivityName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ActivityName { get; set; }

        public bool isSign { get; set; }

        public bool isArchive { get; set; }

        public SLAStatusList SLAStatus { get; set; }

        public bool isNotReview { get; set; }

        public bool isShow { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string FullName { get; set; }

        [Display(Name = "TitleName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string TitleName { get; set; }

        [Display(Name = "DepartmentName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DepartmentName { get; set; }
    }

    public class CommentTable : BasicCompanyNullTable
    {
        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }

        public string Comment { get; set; }
    }

    public class WFTrackerTable : BasicTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LineNum { get; set; }

        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }

        [Required]
        public string ActivityName { get; set; }

        public string ActivityID { get; set; }

        public string ParallelID { get; set; }

        public string SignUserId { get; set; }

        public DateTime? SignDate { get; set; }

        public TrackerType TrackerType { get; set; }

        public bool ManualExecutor { get; set; }

        public DateTime? StartDateSLA { get; set; }

        public int SLAOffset { get; set; }

        public bool ExecutionStep { get; set; }

        public DateTime? PerformToDate()
        {
            if (SLAOffset > 0)
            {
                IDocumentService _service = DependencyResolver.Current.GetService<IDocumentService>();
                return _service.GetSLAPerformDate(DocumentTableId, StartDateSLA, SLAOffset);
            }

            return null;
        }

        public virtual List<WFTrackerUsersTable> Users { get; set; }
    }

    public class WFTrackerUsersTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Timestamp]
        public Byte[] TimeStamp { get; set; }

        public string InitiatorUserId { get; set; }

        [Required]
        public string UserId { get; set; }
    }

    public class ReviewDocLogTable : BasicTable
    {
        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }

        public bool isArchive { get; set; }
    }

    public class DocumentReaderTable : BasicTable
    {
        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }

        [Required]
        public string UserId { get; set; }
    }

    public class HistoryUserTable : BasicTable
    {
        public HistoryType HistoryType { get; set; }

        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }

        public string Description { get; set; }
    }

    public class SearchTable : BasicTable
    {
        public string DocumentText { get; set; }

        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }
    }

    public class ServiceIncidentTable : BasicCompanyNullTable
    {
        [StringLength(70)]
        [Required]
        public string ServiceName { get; set; }

        [StringLength(256)]
        [Required]
        public string Description { get; set; }

        public ServiceIncidientPriority ServiceIncidientPriority { get; set; }
        public ServiceIncidientLevel ServiceIncidientLevel { get; set; }
        public ServiceIncidientLocation ServiceIncidientLocation { get; set; }

        public int SLAIncident { get; set; }

        [Required]
        public string RoleTableId { get; set; }

        [ForeignKey("RoleTableId")]
        public virtual IdentityRole IdentityRole { get; set; }
    }
}