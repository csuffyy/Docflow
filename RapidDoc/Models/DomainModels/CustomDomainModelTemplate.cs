using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Interfaces;
using RapidDoc.Models.Repository;

namespace RapidDoc.Models.DomainModels
{
    public abstract class BasicDocumentTable : BasicTable, IDocument
    {
        public Guid DocumentTableId { get; set; }
        public virtual DocumentTable DocumentTable { get; set; }
    }

    public abstract class BasicDocumentRequestTable : BasicDocumentTable
    {
        public string RequestText { get; set; }
        public string Users { get; set; }
    }

    public abstract class BasicDocumantOfficeMemoTable : BasicDocumentTable
    {
        public Folder Folder { get; set; }

        public Guid? ItemCauseTableId { get; set; }
        public virtual ItemCauseTable ItemCauseTable { get; set; }
     
        public string ItemCauseNumber
        {
            get {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber + " - " + this.ItemCauseTable.CaseName;

                return String.Empty;
            }
        }

        public string DocumentWhom { get; set; }
        public string Whom { get; set; }
        public string DocumentCopy { get; set; }
        public string FromWhom { get; set; }
        public string _DocumentTitle { get; set; }
        public string MainField { get; set; }
        public bool Parallel { get; set; }
        public string AdditionalText { get; set; } 
    }

    public abstract class BasicDailyTasksTable : BasicDocumentTable
    {
        public string MainField { get; set; }
        public string Users { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public DateTime? ProlongationDate { get; set; }
        public string ReportText { get; set; }
        public Guid? RefDocumentId { get; set; }
        public string RefDocNum { get; set; }
        public bool Separated { get; set; }
    }

    public abstract class BasicOrderTable : BasicDocumentTable
    {
        public string OrderNum { get; set; }
        public DateTime? OrderDate { get; set; }   
        public string MainFieldTranslate { get; set; }
        public bool NeedTranslate { get; set; }
        public bool CancelOrder { get; set; }
        public Guid? CancelDocumentId { get; set; }

        public string MainField { get; set; }
        public bool Addition { get; set; }
        public Guid? AdditionDocumentId { get; set; }
        public string AdditionText { get; set; }
        public bool Executed { get; set; }      

        public string ListAgreement { get; set; }
            
        public string ControlUsers { get; set; }
        public DateTime? ControlDate { get; set; }

        public string ListSubcription { get; set; }
        public bool AddReaders { get; set; } 

        public string Sign { get; set; }
        public string SignName { get; set; }
        public string SignTitle { get; set; }
        public string ListFileName { get; set; }

        public Guid? NumberSeriesBookingTableId { get; set; }
        public virtual NumberSeriesBookingTable NumberSeriesBookingTable { get; set; }

        public int AdditionCount { get; set; }
    }

    public abstract class BasicOrderDefaultTable : BasicOrderTable
    {
        public Guid? ItemCauseTableId { get; set; }
        public virtual ItemCauseTable ItemCauseTable { get; set; }

        public string ItemCauseNumber
        {
            get
            {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber + " - " + this.ItemCauseTable.CaseName;

                return String.Empty;
            }
        }

        public string Subject { get; set; }
    }

    public abstract class BasicIncomingDocumentsTable : BasicDocumentTable
    {
        public bool Language_Kazakh { get; set; }
        public bool Language_Russian { get; set; }
        public bool Language_English { get; set; }
        public bool Language_Chinese { get; set; }
        public bool Language_French { get; set; }
        public bool Language_Other { get; set; }

        public Guid? OutcomingNumberDocId { get; set; }
        public string OutcomingNumber { get; set; }

        public string OutgoingNumber { get; set; }
        public DateTime? OutgoingDate { get; set; }

        public Guid? OrganizationTableId { get; set; }
        public virtual OrganizationTable OrganizationTable { get; set; }

        public Guid? NumberSeriesBookingTableId { get; set; }
        public virtual NumberSeriesBookingTable NumberSeriesBookingTable { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public string Receiver { get; set; }

        public ControlType ControlType { get; set; }
        public ServiceIncidientPriority Priority { get; set; }

        public DateTime? ExecutionDate { get; set; }

        public NatureIncomingQuestion NatureQuestionType { get; set; }
        public string NatureQuestion { get; set; }

        public string ListsCount { get; set; }

        public string ApplicationsCount { get; set; }
        public string IncomingDocNum { get; set; }
        public string DocumentSubject { get; set; }

        public IncomingDocumentType DocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public bool Executed { get; set; }

        public Guid? ItemCauseTableId { get; set; }
        public virtual ItemCauseTable ItemCauseTable { get; set; }

        public string ItemCauseNumber
        {
            get
            {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber + " - " + this.ItemCauseTable.CaseName;

                return String.Empty;
            }
        }
    }

    public abstract class BasicOutcomingDocumentsTable : BasicDocumentTable
    {
        public bool Language_Kazakh { get; set; }
        public bool Language_Russian { get; set; }
        public bool Language_English { get; set; }
        public bool Language_Chinese { get; set; }
        public bool Language_French { get; set; }
        public bool Language_Other { get; set; }

        public DateTime? OutgoingDate { get; set; }
        public OutcomingDispatchType OutcomingDispatchType { get; set; }
        public string DispatchType { get; set; }

        public Guid? OrganizationTableId { get; set; }
        public virtual OrganizationTable OrganizationTable { get; set; }

        public Guid? NumberSeriesBookingTableId { get; set; }
        public virtual NumberSeriesBookingTable NumberSeriesBookingTable { get; set; }

        public string Signer { get; set; }

        public ControlType ControlType { get; set; }
        public ServiceIncidientPriority Priority { get; set; }
        public string BlankNumber { get; set; }

        public Guid? IncomingNumberDocId { get; set; }
        public string IncomingNumber { get; set; }
        public DateTime? IncomingDate { get; set; }

        public string ListsCount { get; set; }
        public string ApplicationsCount { get; set; }
        public string OutcomingDocNum { get; set; }
        public string DocumentSubject { get; set; }

        public IncomingDocumentType DocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public string ListAgreement { get; set; }

        public Guid? ItemCauseTableId { get; set; }
        public virtual ItemCauseTable ItemCauseTable { get; set; }

        public bool NeedTranslate { get; set; }

        public string ItemCauseNumber
        {
            get
            {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber + " - " + this.ItemCauseTable.CaseName;

                return String.Empty;
            }
        }

        public string ItemCauseNumberCode
        {
            get
            {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber;

                return String.Empty;
            }
        }
    }

    public abstract class BasicAppealDocumentsTable : BasicDocumentTable
    {
        public bool Language_Kazakh { get; set; }
        public bool Language_Russian { get; set; }
        public bool Language_English { get; set; }
        public bool Language_Chinese { get; set; }
        public bool Language_French { get; set; }
        public bool Language_Other { get; set; }

        public CategoryPerson CategoryPerson { get; set; }

        public string IdentificatorNumber { get; set; }

        public Guid? OrganizationTableId { get; set; }
        public virtual OrganizationTable OrganizationTable { get; set; }

        public StatusPerson StatusPerson { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }

        public Guid? CountryTableId { get; set; }
        public virtual CountryTable CountryTable { get; set; }

        public string RegistrationNum { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public string Whom { get; set; }
        public FormAppeal FormAppeal { get; set; }
        public TypeAppeal TypeAppeal { get; set; }
        public string TypeAppealAddition { get; set; }
        public CharacterAppeal CharacterAppeal { get; set; }
        public CharacterQuestion CharacterQuestion { get; set; }
        public string CharacterQuestionAddition { get; set; }
        public string ListsCount { get; set; }
        public string ApplicationsCount { get; set; }
        public string Subject { get; set; }

        public Guid? ItemCauseTableId { get; set; }
        public virtual ItemCauseTable ItemCauseTable { get; set; }

        public string ItemCauseNumber
        {
            get
            {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber + " - " + this.ItemCauseTable.CaseName;

                return String.Empty;
            }
        }

        public string ItemCauseNumberCode
        {
            get
            {
                if (this.ItemCauseTable != null)
                    return this.ItemCauseTable.CaseNumber;

                return String.Empty;
            }
        }

        public string Content { get; set; }

        public Guid? ReasonRequestTableId { get; set; }
        public virtual ReasonRequestTable ReasonRequestTable { get; set; }

        public Guid? QuestionRequestTableId { get; set; }
        public virtual QuestionRequestTable QuestionRequestTable { get; set; }

        public ControlType ControlType { get; set; }
        public ServiceIncidientPriority Priority { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public bool Executed { get; set; }
    }

    public interface IBasicProtocol
    {
        Guid? ProtocolFoldersTableId { get; set; }
        string Subject { get; set; }
        string Location { get; set; }
        string Introduction { get; set; }
        string Agenda { get; set; }
        string Attended { get; set; }
        string Invited { get; set; }
        string Absent { get; set; }
        string Chairman { get; set; }
        string ListAgreement { get; set; }
    }

    public abstract class BasicProtocolDocumentsTable : BasicDocumentTable, IBasicProtocol
    {
        public Guid? ProtocolFoldersTableId { get; set; }
        public virtual ProtocolFoldersTable ProtocolFoldersTable { get; set; }

        public string Subject { get; set; }
        public string Location { get; set; }
        public string Introduction { get; set; }
        public string Agenda { get; set; }
        public string Attended { get; set; }
        public string Invited { get; set; }
        public string Absent { get; set; }
        public string Chairman { get; set; }
        public string ListAgreement { get; set; }
        public string Duration { get; set; }
        public virtual List<PRT_QuestionList_Table> QuestionList { get; set; }
    }
}