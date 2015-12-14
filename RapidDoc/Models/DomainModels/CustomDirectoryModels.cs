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

    public class CountryTable : BasicCompanyNullTable
    {
        public string CountryName { get; set; }
        public string CityName { get; set; }
    }

    public class OrganizationTable : BasicCompanyNullTable
    {
        public string OrgName { get; set; }
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

    public class TripMRPTable : BasicCompanyNullTable
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
}