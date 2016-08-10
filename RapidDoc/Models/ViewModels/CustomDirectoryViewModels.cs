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

    public class PortalParametersView : BasicCompanyNullView
    {
        [Display(Name = "Отчетные подразделения")]
        public string ReportDepartments { get; set; }

        [Display(Name = "Максимальное количество пользователей для уведомлений (активный этап)")]
        public int NumberUserMaxAlerts { get; set; }

        [Display(Name = "Максимальное количество пользователей для уведомлений (читатели)")]
        public int NumberUserMaxAlertsReaders { get; set; }

        [Display(Name = "Уведомление всех вышестоящих пользователей при закрытии поручений")]
        public bool NotificationAllUserTask { get; set; }
    }

    public class TaskDelegationView
    {
        public string DocumentNum { get; set; }
        public Guid DocumentId { get; set; }
        public string UserCreate { get; set; }
        public DateTime DateCreate { get; set; }
        public string DocumentText { get; set; }
        public string ReportText { get; set; }
        public DocumentState DocumentState { get; set; }
        public string Users { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public DateTime? ProlongationDate { get; set; }
        public DocumentType DocType { get; set; }
        public Guid FileId { get; set; }
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

    public class CountryView : BasicView
    {
        [Display(Name = "Страна")]
        public string CountryName { get; set; }

        [Display(Name = "Город")]
        public string CityName { get; set; }
    }

    public class AssetsView : BasicCompanyNullView
    {
        [Display(Name = "Инвентарный номер")]
        public string AssetNumber { get; set; }

        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Display(Name = "Местоположение")]
        public string Location { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }
    }

    public class OrganizationView : BasicView
    {
        [Display(Name = "Организация")]
        public string OrgName { get; set; }

        [Display(Name = "Включено")]
        public bool Enable { get; set; }
    }

    public class ProjectView : BasicCompanyNullView
    {
        [Display(Name="Наименование проекта")]
        public string ProjectName { get; set; }
    }

    public class TripMRPView : BasicView
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

    public class TaskScheduleView : BasicCompanyNullView
    {
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Текст задачи")]
        public string MainField { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Исполнители")]
        public string Users { get; set; }

        public Guid fileId { get; set; }
        public DateTime? RefDate { get; set; }

        [Display(Name = "Проект")]
        public Guid? ProjectTableId { get; set; }

        [Display(Name = "Период")]
        public TaskScheduleTypePeriod TypePeriod { get; set; }

        [Display(Name = "Периодичность")]
        public int Periodicity { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Начальная дата")]
        public DateTime? DateFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Конечная дата")]
        public DateTime? DateTo { get; set; }


        [Display(Name = "Понедельник")]
        public bool Monday { get; set; }
        [Display(Name = "Вторник")]
        public bool Tuesday { get; set; }
        [Display(Name = "Среда")]
        public bool Wednesday { get; set; }
        [Display(Name = "Четверг")]
        public bool Thursday { get; set; }
        [Display(Name = "Пятница")]
        public bool Friday { get; set; }
        [Display(Name = "Суббота")]
        public bool Saturday { get; set; }
        [Display(Name = "Воскресенье")]
        public bool Sunday { get; set; }

        [Display(Name = "1")]
        public bool Day1 { get; set; }
        [Display(Name = "2")]
        public bool Day2 { get; set; }
        [Display(Name = "3")]
        public bool Day3 { get; set; }
        [Display(Name = "4")]
        public bool Day4 { get; set; }
        [Display(Name = "5")]
        public bool Day5 { get; set; }
        [Display(Name = "6")]
        public bool Day6 { get; set; }
        [Display(Name = "7")]
        public bool Day7 { get; set; }
        [Display(Name = "8")]
        public bool Day8 { get; set; }
        [Display(Name = "9")]
        public bool Day9 { get; set; }
        [Display(Name = "10")]
        public bool Day10 { get; set; }

        [Display(Name = "11")]
        public bool Day11 { get; set; }
        [Display(Name = "12")]
        public bool Day12 { get; set; }
        [Display(Name = "13")]
        public bool Day13 { get; set; }
        [Display(Name = "14")]
        public bool Day14 { get; set; }
        [Display(Name = "15")]
        public bool Day15 { get; set; }
        [Display(Name = "16")]
        public bool Day16 { get; set; }
        [Display(Name = "17")]
        public bool Day17 { get; set; }
        [Display(Name = "18")]
        public bool Day18 { get; set; }
        [Display(Name = "19")]
        public bool Day19 { get; set; }
        [Display(Name = "20")]
        public bool Day20 { get; set; }

        [Display(Name = "21")]
        public bool Day21 { get; set; }
        [Display(Name = "22")]
        public bool Day22 { get; set; }
        [Display(Name = "23")]
        public bool Day23 { get; set; }
        [Display(Name = "24")]
        public bool Day24 { get; set; }
        [Display(Name = "25")]
        public bool Day25 { get; set; }
        [Display(Name = "26")]
        public bool Day26 { get; set; }
        [Display(Name = "27")]
        public bool Day27 { get; set; }
        [Display(Name = "28")]
        public bool Day28 { get; set; }
        [Display(Name = "29")]
        public bool Day29 { get; set; }
        [Display(Name = "30")]
        public bool Day30 { get; set; }
        [Display(Name = "31")]
        public bool Day31 { get; set; }
        [Display(Name = "Последний")]
        public bool Last { get; set; }

    }

    public class TaskScheduleHistroyView : BasicCompanyNullView
    {
        public Guid TaskScheduleId { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNum { get; set; }
    }
}