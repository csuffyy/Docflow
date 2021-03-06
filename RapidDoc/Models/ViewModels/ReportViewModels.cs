﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Repository;
using System.Drawing;

namespace RapidDoc.Models.ViewModels
{
    public class ReportParametersBasicView
    {
        [Display(Name = "StartDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime EndDate { get; set; }

        [Display(Name = "DepartmentName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public String DepartmentName { get; set; }
        public Guid? DepartmentTableId { get; set; }
    }

    public class ReportProcessesView
    {
        public ProcessTable Process { get; set; }

        public string StageName { get; set; }
        public FilterType FilterType { get; set; }
        public string FilterText { get; set; }
        public Color Color { get; set; }
        
        public List<EmplTable> Names { get; set; }

    }
    public class ReportCZComments
    {
        public string UserName { get; set; }
        public string UserTitle { get; set; }
        public DateTime CreateDate { get; set; }
        public string Comment { get; set; }
    }

    public class MonitoringTasksView
    {

        public string DocumentNumber { get; set; }
        public DocumentType DocumentRefType { get; set; }      
        public DocumentState DocumentState { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime CreateDate { get; set; }
        public string DocumentText { get; set; }
        public Guid DocumentId { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public string Month { get; set; }
        public int MonthNumber { get; set; }
        public string Year { get; set; }
        public ReportExecutionType TaskType { get; set; }
        public DateTime? SignDateTime { get; set; }
        public Guid RefDocId { get; set; }
    }

    public class GetUserWithSLA
    {
        public SLAStatusList Status { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime? Date { get; set; }
    }

    public class UserDocumentsWithLSLA
    {
        public SLAStatusList Status { get; set; }
        public DocumentTable Document { get; set; }
        public DateTime? Date { get; set; }
    }
}