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
        [Required]
        public string RequestText { get; set; }

        [Required]
        public string Users { get; set; }
    }

    public abstract class BasicDocumantOfficeMemoTable : BasicDocumentTable
    {
        public Folder Folder { get; set; }

        [Required]
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

        [Required]
        public string FromWhom { get; set; }

        [Required]
        public string _DocumentTitle { get; set; }

        [Required]
        public string MainField { get; set; }
        public bool Parallel { get; set; }

        public string AdditionalText { get; set; } 
    }

    public abstract class BasicDailyTasksTable : BasicDocumentTable
    {
        [Required]
        public string MainField { get; set; }

        [Required]
        public string Users { get; set; }

        [Required]
        public DateTime ExecutionDate { get; set; }

        public DateTime? ProlongationDate { get; set; }

        public string ReportText { get; set; }
       
        public Guid? RefDocumentId { get; set; }
      
        public string RefDocNum { get; set; }

        public bool Separated { get; set; }
    }

    public abstract class BasicOrderTable : BasicDocumentTable
    {
        [Required]
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

        public string OrderNum { get; set; }
        public DateTime? OrderDate { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]       
        public string MainFieldTranslate { get; set; }
        public bool NeedTranslate { get; set; }
        public bool CancelOrder { get; set; }
        public Guid? CancelDocumentId { get; set; }

        public string MainField { get; set; }
        public bool Addition { get; set; }
        public bool Executed { get; set; }      

        [Required]
        public string ListAgreement { get; set; }
            
        public string ControlUsers { get; set; }
        public DateTime? ControlDate { get; set; }

        public string ListSubcription { get; set; } 

        [Required]
        public string Sign { get; set; }
        public string SignName { get; set; }
        public string SignTitle { get; set; }

        public Guid? NumberSeriesBookingTableId { get; set; }
        public virtual NumberSeriesBookingTable NumberSeriesBookingTable { get; set; }
    }
}