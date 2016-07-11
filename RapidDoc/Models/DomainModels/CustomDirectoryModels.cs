using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using RapidDoc.Models.Repository;

namespace RapidDoc.Models.DomainModels
{
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
        public virtual ApplicationRole IdentityRole { get; set; }
    }

    public class TripSettingsTable : BasicCompanyNullTable
    {
        public EmplTripType EmplTripType { get; set; }
        public TripDirection TripDirection { get; set; }
        public string DayRate { get; set; }
        public string ResidenceRate { get; set; }
    }

    public class ItemCauseTable : BasicCompanyNullTable
    {
        public string CaseNumber { get; set; }
        public string CaseName { get; set; }

        public Guid? DepartmentTableId { get; set; }
        public virtual DepartmentTable DepartmentTable { get; set; }

        public  string Departmentname
        {
            get {
                    if (this.DepartmentTable != null)
                        return this.DepartmentTable.DepartmentName;

                    return String.Empty;
                }
        }

        public bool Enable { get; set; }
        
    }

    public class PortalParametersTable : BasicCompanyNullTable
    {
        public string ReportDepartments { get; set; }
        public int NumberUserMaxAlerts { get; set; }
        public int NumberUserMaxAlertsReaders { get; set; }
        public bool NotificationAllUserTask { get; set; }
    }

    public class CountryTable : BasicTable
    {
        public string CountryName { get; set; }
        public string CityName { get; set; }
    }

    public class OrganizationTable : BasicTable
    {
        public string OrgName { get; set; }
        public bool Enable { get; set; }
    }

    public class ProjectTable : BasicCompanyNullTable
    {
        public string ProjectName { get; set; }
    }

    public class ReasonRequestTable : BasicCompanyNullTable
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class QuestionRequestTable : BasicCompanyNullTable
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class TripMRPTable : BasicTable
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Amount { get; set; }
    }
   
    public class ProtocolFoldersTable : BasicCompanyNullTable
    {
        public string ProtocolFolderName { get; set; }

        public Guid? ProcessTableId { get; set; }
        public virtual ProcessTable ProcessTable { get; set; }
        public string ProcessName
        {
            get
            {
                if (this.ProcessTable != null)
                    return this.ProcessTable.ProcessName;

                return String.Empty;
            }
        }

        public bool Enable { get; set; }

        [ForeignKey("ProtocolFoldersTableParent")]
        public Guid? ProtocolFoldersParentId { get; set; }
        public virtual ProtocolFoldersTable ProtocolFoldersTableParent { get; set; }
        public string ParentProtocolFolderName
        {
            get
            {
                if (this.ProtocolFoldersTableParent != null)
                    return this.ProtocolFoldersTableParent.ProtocolFolderName;

                return string.Empty;
            }
        }
    }

    public class TaskScheduleTable : BasicCompanyNullTable
    {
        public string MainField { get; set; }
        public string Users { get; set; }       
        public TaskScheduleTypePeriod TypePeriod { get; set; }
        public int Periodicity { get; set; }
        public Guid fileId { get; set; }
        public DateTime? RefDate { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public bool Day1 { get; set; }
        public bool Day2 { get; set; }
        public bool Day3 { get; set; }
        public bool Day4 { get; set; }
        public bool Day5 { get; set; }
        public bool Day6 { get; set; }
        public bool Day7 { get; set; }
        public bool Day8 { get; set; }
        public bool Day9 { get; set; }
        public bool Day10 { get; set; }

        public bool Day11 { get; set; }
        public bool Day12 { get; set; }
        public bool Day13 { get; set; }
        public bool Day14 { get; set; }
        public bool Day15 { get; set; }
        public bool Day16 { get; set; }
        public bool Day17 { get; set; }
        public bool Day18 { get; set; }
        public bool Day19 { get; set; }
        public bool Day20 { get; set; }

        public bool Day21 { get; set; }
        public bool Day22 { get; set; }
        public bool Day23 { get; set; }
        public bool Day24 { get; set; }
        public bool Day25 { get; set; }
        public bool Day26 { get; set; }
        public bool Day27 { get; set; }
        public bool Day28 { get; set; }
        public bool Day29 { get; set; }
        public bool Day30 { get; set; }
        public bool Day31 { get; set; }
        public bool Last { get; set; }

    }

    public class TaskScheduleHistroyTable : BasicCompanyNullTable
    {
        public Guid TaskScheduleId { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNum { get; set; }
    }
}