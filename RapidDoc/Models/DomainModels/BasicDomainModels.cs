﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RapidDoc.Models.Interfaces;

namespace RapidDoc.Models.DomainModels
{
    public abstract class BasicTable : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ConcurrencyCheck]
        [Timestamp]
        public Byte[] TimeStamp { get; set; }

        [Display(Name = "CreatedDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "ModifiedDate", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public DateTime ModifiedDate { get; set; }

        public string ApplicationUserCreatedId { get; set; }
        public virtual ApplicationUser ApplicationUserCreated { get; set; }

        public string ApplicationUserModifiedId { get; set; }
        public virtual ApplicationUser ApplicationUserModified { get; set; }

        [Display(Name = "CreatedBy", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string CreatedBy
        {
            get
            {
                if (this.ApplicationUserCreated != null)
                {
                    return this.ApplicationUserCreated.UserName;
                }

                return string.Empty;
            }
        }

        [Display(Name = "ModifiedBy", ResourceType = typeof(FieldNameRes.FieldNameResource))]
        public string ModifiedBy
        {
            get
            {
                if (this.ApplicationUserModified != null)
                {
                    return this.ApplicationUserModified.UserName;
                }

                return string.Empty;
            }
        }
    }

    public abstract class BasicCompanyNullTable : BasicTable
    {
        public Guid? CompanyTableId { get; set; }
        public virtual CompanyTable CompanyTable { get; set; }
    }

    public abstract class BasicCompanyNotNullTable : BasicTable
    {
        public Guid CompanyTableId { get; set; }
        public virtual CompanyTable CompanyTable { get; set; }
    }

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
}