﻿using System;
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

        [Display(Name = "Копия")]
        public string DocumentCopy { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Тема документа")]
        public string DocumentTitle { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Основное поле")]
        public string MainField { get; set; }

        [Display(Name = "Параллельно")]
        public bool Parallel { get; set; }
    }

    public abstract class BasicDailyTasksView : BasicDocumentView
    {
        [DataType(DataType.Date)]
        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Дата исполнения")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Задача")]
        public string MainText { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Исполнители")]
        public string Users { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationRes.ValidationResource), ErrorMessageResourceName = "ErrorFieldisNull")]
        [Display(Name = "Отчет")]
        public string ReportText { get; set; }
    }
}