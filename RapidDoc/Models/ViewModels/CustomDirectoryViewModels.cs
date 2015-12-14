using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RapidDoc.Models.Repository;

namespace RapidDoc.Models.ViewModels
{
    public class ServiceIncidentView : BasicCompanyNullView
    {
        [StringLength(70, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "ProcessName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ServiceName { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Display(Name = "Description", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string Description { get; set; }

        [Display(Name = "Приоритет")]
        public ServiceIncidientPriority ServiceIncidientPriority { get; set; }

        [Display(Name = "Уровень поддержки")]
        public ServiceIncidientLevel ServiceIncidientLevel { get; set; }

        [Display(Name = "Местоположение")]
        public ServiceIncidientLocation ServiceIncidientLocation { get; set; }

        [Display(Name = "SLA инцидентов")]
        public int SLAIncident { get; set; }

        public string RoleTableId { get; set; }

        [Display(Name = "Роль")]
        public string RoleName { get; set; }
    }

    public class TripSettingsView : BasicCompanyNullView
    {
        [Display(Name = "Категория сотрудника")]
        public EmplTripType EmplTripType { get; set; }

        [Display(Name = "Направления")]
        public TripDirection TripDirection { get; set; }

        [Display(Name = "Суточные норма")]
        [DisplayFormat(DataFormatString = "{0:#,##0.000#}", ApplyFormatInEditMode = true)]
        public string DayRate { get; set; }

        [Display(Name = "Проживание норма")]
        [DisplayFormat(DataFormatString = "{0:#,##0.000#}", ApplyFormatInEditMode = true)]
        public string ResidenceRate { get; set; }
    }

    public class ItemCauseView : BasicCompanyNullView
    {
        [Display(Name = "№ дела")]
        public string CaseNumber { get; set; }

        [Display(Name = "Название дела")]
        public string CaseName { get; set; }

        public Guid? DepartmentTableId { get; set; }

        [Display(Name = "DepartmentName", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string DepartmentName { get; set; }
        public bool IsCurrentUserDepartment { get; set; }
        
        [Display(Name = "Включено")]
        public bool Enable { get; set; }
    }

    public class TaskDelegationView
    {
        public string DocumentNum { get; set; }
        public Guid DocumentId { get; set; }
        public string UserCreate { get; set; }
        public DateTime DateCreate { get; set; }
        public string ReportText { get; set; }
        public DocumentState DocumentState { get; set; }
        public string Users { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public DateTime? ProlongationDate { get; set; }
        public DocumentType DocType { get; set; }
    }

    public class ModificationDocumentView
    {
        public string DocumentNum { get; set; }
        public Guid? DocumentId { get; set; }
        public Guid? ParentDocumentId { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public string NamesTo { get; set; }
    }

    public class CountryView : BasicCompanyNullView
    {
        [Display(Name = "Страна")]
        public string CountryName { get; set; }

        [Display(Name = "Город")]
        public string CityName { get; set; }
    }

    public class OrganizationView : BasicCompanyNullView
    {
        [Display(Name = "Организация")]
        public string OrgName { get; set; }
    }

    public class TripMRPView : BasicCompanyNullView
    {
        [Display(Name = "Начальная дата")]
        public DateTime? FromDate { get; set; }
        [Display(Name = "Конечная дата")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Значение")]
        [DisplayFormat(DataFormatString = "{0:#,##0.000#}", ApplyFormatInEditMode = true)]
        public string Amount { get; set; }
    }

    public class ReasonRequestView : BasicCompanyNullView
    {
        [StringLength(10, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }
    }

    public class QuestionRequestView : BasicCompanyNullView
    {
        [StringLength(10, ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisLong")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }
    }

    public class ProtocolFoldersView : BasicCompanyNullView
    {
        [Display(Name = "Наименование")]
        public string ProtocolFolderName { get; set; }

        public Guid? ProcessTableId { get; set; }

        [Display(Name = "Наименование процесса")]
        public string ProcessName { get; set; }

        public Guid? ProtocolFoldersParentId { get; set; }

        [Display(Name = "Ссылка на родительскую папку")]
        public string ParentProtocolFolderName{ get; set; }

        [Display(Name = "Включено")]
        public bool Enable { get; set; }
    }
}