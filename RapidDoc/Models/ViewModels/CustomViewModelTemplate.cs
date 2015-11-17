using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RapidDoc.Models.Interfaces;
using RapidDoc.Models.Repository;

namespace RapidDoc.Models.ViewModels
{
    public abstract class BasicDocumentView : BasicCompanyNullView, IDocument
    {
        public Guid DocumentTableId { get; set; }
    }

    public abstract class BasicDocumentRequestView : BasicDocumentView
    {
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "RequestText", ResourceType = typeof(CustomRes.Custom))]
        public string RequestText { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Users", ResourceType = typeof(CustomRes.Custom))]
        public string Users { get; set; }
    }

    public abstract class BasicDocumantOfficeMemoView : BasicDocumentView
    {
        [Display(Name = "Папка")]
        public Folder Folder { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public Guid? ItemCauseTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumber { get; set; }

        [Display(Name = "Кому")]
        public string DocumentWhom { get; set; }

        [Display(Name = "Кому:")]
        public string Whom { get; set; }

        [Display(Name = "Копия:")]
        public string DocumentCopy { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "От кого:")]
        public string FromWhom { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Тема:")]
        public string _DocumentTitle { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Текст СЗ")]
        public string MainField { get; set; }

        [Display(Name = "Параллельно")]
        public bool Parallel { get; set; }

        [Display(Name = "Сопроводительный текст")]
        public string AdditionalText { get; set; } 
    }

    public abstract class  BasicDailyTasksView : BasicDocumentView
    {
        [DataType(DataType.Date)]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Дата исполнения")]
        public DateTime? ExecutionDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата продления")]
        public DateTime? ProlongationDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Задача")]
        public string MainField { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Исполнители")]
        public string Users { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Отчет")]
        public string ReportText { get; set; }        

        [Display(Name = "Ссылка на исходный документ")]
        public string RefDocNum { get; set; }

        public Guid? RefDocumentId { get; set; }

        [Display(Name = "Отдельная карточка каждому исполнителю")]
        public bool Separated { get; set; }
    }

    public abstract class BasicOrderView : BasicDocumentView
    {
        [Display(Name = "Номер приказа")]
        public string OrderNum { get; set; }

        [Display(Name = "Дата приказа")]
        public DateTime? OrderDate { get; set; }

        [Display(Name = "Текст приказа")]      
        public string MainField { get; set; }

        [Display(Name = "Текст приказа (перевод)")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        public string MainFieldTranslate { get; set; }

        [Display(Name = "Требуется перевод")]
        public bool NeedTranslate { get; set; }

        [Display(Name = "Аннулировать приказ")]
        public bool CancelOrder { get; set; }

        public Guid? CancelDocumentId { get; set; }

        [Display(Name = "Дополнение")]
        public bool Addition { get; set; }
        public Guid? AdditionDocumentId { get; set; }

        [Display(Name = "Текст дополнение")]
        public string AdditionText { get; set; }

        [Display(Name = "Исполнен")]
        public bool Executed { get; set; }

        [Display(Name = "Список согласования")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        public string ListAgreement { get; set; }

        [Display(Name = "Список рассылки")]      
        public string ListSubcription { get; set; }

        [Display(Name = "Срок исполнения")]
        public DateTime? ControlDate { get; set; }

        [Display(Name = "Поставить на контроль")]
        public string ControlUsers { get; set; }

        [Display(Name = "Подпись")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        public string Sign { get; set; }

        [Display(Name = "ФИО")]
        public string SignName { get; set; }

        [Display(Name = "Должность")]
        public string SignTitle { get; set; }

        [Display(Name = "Забронированные номера")]
        public Guid? NumberSeriesBookingTableId { get; set; }

        public int AdditionCount { get; set; }
    }

    public abstract class BasicOrderDefaultView : BasicOrderView
    {
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public Guid? ItemCauseTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumber { get; set; }

        [Display(Name = "Тема")]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        public string Subject { get; set; }
    }

    public abstract class BasicIncomingDocumentsView : BasicDocumentView
    {
        [Display(Name = "Казахский")]
        public bool Language_Kazakh { get; set; }
        [Display(Name = "Русский")]
        public bool Language_Russian { get; set; }
        [Display(Name = "Английский")]
        public bool Language_English { get; set; }
        [Display(Name = "Китайский")]
        public bool Language_Chinese { get; set; }
        [Display(Name = "Французкий")]
        public bool Language_French { get; set; }

        [Display(Name = "Ответ на исходящий")]
        public Guid? OutcomingNumberDocId { get; set; }
        public string OutcomingNumber { get; set; }

        [Display(Name = "Исходящий номер")]   
        public string OutgoingNumber { get; set; }
        [Display(Name = "Дата исходящего")]
        public DateTime? OutgoingDate { get; set; }

        [Display(Name = "Корреспондент")]
        public Guid? OrganizationTableId { get; set; }

        [Display(Name = "Забронированные номера")]
        public Guid? NumberSeriesBookingTableId { get; set; }

        [Display(Name = "Номер документа")]
        public string IncomingDocNum { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime? RegistrationDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Получатель")]
        public string Receiver { get; set; }

        [Display(Name = "Тип контроля")]
        public ControlType ControlType { get; set; }
        [Display(Name = "Приоритет")]
        public ServiceIncidientPriority Priority { get; set; }

        [Display(Name = "Срок исполнения")]
        public DateTime? ExecutionDate { get; set; }

        [Display(Name = "Характер вопроса")]
        public NatureIncomingQuestion NatureQuestionType { get; set; }
        [Display(Name = "Характер вопроса")]
        public string NatureQuestion { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Количество листов")]
        public string ListsCount { get; set; }
        
        [Display(Name = "Количество приложений")]
        public string ApplicationsCount { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]  
        [Display(Name = "Тема документа")]
        public string DocumentSubject { get; set; }

        [Display(Name = "Тип документа")]
        public IncomingDocumentType DocumentType { get; set; }
        [Display(Name = "Тип документа")]
        public string DocumentTypeName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public Guid? ItemCauseTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumber { get; set; }

        [Display(Name = "Исполнен")]
        public bool Executed { get; set; }
    }

    public abstract class BasicOutcomingDocumentsView : BasicDocumentView
    {
        [Display(Name = "Казахский")]
        public bool Language_Kazakh { get; set; }
        [Display(Name = "Русский")]
        public bool Language_Russian { get; set; }
        [Display(Name = "Английский")]
        public bool Language_English { get; set; }
        [Display(Name = "Китайский")]
        public bool Language_Chinese { get; set; }
        [Display(Name = "Французкий")]
        public bool Language_French { get; set; }

        [Display(Name = "Вид отправки")]
        public OutcomingDispatchType OutcomingDispatchType { get; set; }
        [Display(Name = "Вид отправки")]
        public string DispatchType { get; set; }

        [Display(Name = "Номер документа")]
        public string OutcomingDocNum { get; set; }

        [Display(Name = "Дата исходящего")]
        public DateTime? OutgoingDate { get; set; }

        [Display(Name = "Получатель")]
        public Guid? OrganizationTableId { get; set; }

        [Display(Name = "Забронированные номера")]
        public Guid? NumberSeriesBookingTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "За подписью")]
        public string Signer { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Список согласования")]
        public string ListAgreement { get; set; }

        [Display(Name = "Тип контроля")]
        public ControlType ControlType { get; set; }
        [Display(Name = "Приоритет")]
        public ServiceIncidientPriority Priority { get; set; }

        [Display(Name = "Номер бланка")]
        public string BlankNumber { get; set; }

        [Display(Name = "Ответ на входящий")]
        public Guid? IncomingNumberDocId { get; set; }
        public string IncomingNumber { get; set; }
        public DateTime? IncomingDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Количество листов")]
        public string ListsCount { get; set; }

        [Display(Name = "Количество приложений")]
        public string ApplicationsCount { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Тема документа")]
        public string DocumentSubject { get; set; }

        [Display(Name = "Тип документа")]
        public IncomingDocumentType DocumentType { get; set; }
        [Display(Name = "Тип документа")]
        public string DocumentTypeName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public Guid? ItemCauseTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumber { get; set; }

        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumberCode { get; set; }
    }

    public abstract class BasicAppealDocumentsView : BasicDocumentView
    {
        [Display(Name = "Казахский")]
        public bool Language_Kazakh { get; set; }
        [Display(Name = "Русский")]
        public bool Language_Russian { get; set; }
        [Display(Name = "Английский")]
        public bool Language_English { get; set; }
        [Display(Name = "Китайский")]
        public bool Language_Chinese { get; set; }
        [Display(Name = "Французкий")]
        public bool Language_French { get; set; }

        [Display(Name = "Категория лица")]
        public CategoryPerson CategoryPerson { get; set; }

        [Display(Name = "ИИН\\БИН")]
        public string IdentificatorNumber { get; set; }

        [Display(Name = "Поступило из")]
        public Guid? OrganizationTableId { get; set; }

        [Display(Name = "Статус обративщегося лица")]
        public StatusPerson StatusPerson { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "ФИО заявителя")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Адрес заявителя")]
        public string Address { get; set; }

        [Display(Name = "Регион")]
        public Guid? CountryTableId { get; set; }

        [Display(Name = "Регистрационный номер")]
        public string RegistrationNum { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Адресован")]
        public string Whom { get; set; }

        [Display(Name = "Форма обращения")]
        public FormAppeal FormAppeal { get; set; }

        [Display(Name = "Вид обращения")]
        public TypeAppeal TypeAppeal { get; set; }

        [Display(Name = "Вид обращения")]
        public string TypeAppealAddition { get; set; }

        [Display(Name = "Характер обращения")]
        public CharacterAppeal CharacterAppeal { get; set; }

        [Display(Name = "Характер вопроса")]
        public CharacterQuestion CharacterQuestion { get; set; }

        [Display(Name = "Характер вопроса")]
        public string CharacterQuestionAddition { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Количество листов")]
        public string ListsCount { get; set; }

        [Display(Name = "Количество приложений")]
        public string ApplicationsCount { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Тема документа")]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public Guid? ItemCauseTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumber { get; set; }

        [Display(Name = "Номенклатурное дело")]
        public string ItemCauseNumberCode { get; set; }

        [Display(Name = "Содержание")]
        public string Content { get; set; }

        [Display(Name = "Причина обращения")]
        public Guid? ReasonRequestTableId { get; set; }

        [Display(Name = "Вопрос обращения")]
        public Guid? QuestionRequestTableId { get; set; }

        [Display(Name = "Тип контроля")]
        public ControlType ControlType { get; set; }

        [Display(Name = "Приоритет")]
        public ServiceIncidientPriority Priority { get; set; }

        [Display(Name = "Срок исполнения")]
        public DateTime? ExecutionDate { get; set; }

        [Display(Name = "Исполнено")]
        public bool Executed { get; set; }
    }

    public abstract class BasicProtocolDocumentsView : BasicDocumentView
    {
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Display(Name = "Папка")]
        public Guid? ProtocolFoldersTableId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Тема документа")]
        public string Subject { get; set; }

        [Display(Name = "Место проведения")]
        public string Location { get; set; }

        [Display(Name = "Вступительное слово")]
        public string Introduction { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Повестка")]
        public string Agenda { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Присутствовали")]
        public string Attended { get; set; }

        [Display(Name = "Приглашенные")]
        public string Invited { get; set; }

        [Display(Name = "Отсутствовали")]
        public string Absent { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Председатель")]
        public string Chairman { get; set; }

        public List<RapidDoc.Models.DomainModels.PRT_QuestionList_Table> QuestionList { get; set; }
    }
}