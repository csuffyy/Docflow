﻿using RapidDoc.Attributes.Validation;
using RapidDoc.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RapidDoc.Models.DomainModels
{
    public class CompanyTable : EntityTable
    {
        [StringLength(20)]
        [Required]
        public string AliasCompanyName { get; set; }

        [StringLength(256)]
        [Required]
        public string CompanyName { get; set; }

        public Guid? DomainTableId { get; set; }
        public virtual DomainTable DomainTable { get; set; }

        public string DomainName
        {
            get
            {
                if (this.DomainTable != null)
                    return this.DomainTable.DomainName;

                return string.Empty;
            }
        }
    }

    public class RenameCompanyTable : EntityTable
    {
        public Guid CompanyTableId { get; set; }
        public IEnumerable<CompanyTable> CompanyTables { get; set; }

        [StringLength(256)]
        [Required]
        public string FullCompanyName { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class DomainTable : EntityTable
    {
        [StringLength(20)]
        [Required]
        public string DomainName { get; set; }

        [StringLength(256)]
        public string LDAPServer { get; set; }

        [Range(0, 65535)]
        [Required]
        public int LDAPPort { get; set; }

        [StringLength(256)]
        public string LDAPLogin { get; set; }

        [StringLength(256)]
        [DataType(DataType.Password)]
        public string LDAPPassword { get; set; }

        [StringLength(256)]
        public string LDAPBaseDN { get; set; }
    }

    public class NumberSeriesTable : BasicTable
    {
        [StringLength(20)]
        [Required]
        public string NumberSeriesName { get; set; }

        [StringLength(5)]
        public string Prefix { get; set; }

        [StringLength(256)]
        public string TableName { get; set; }

        [Range(3, 10)]
        [Required]
        public int Size { get; set; }

        public int LastNum { get; set; }

        public IEnumerable<GroupProcessTable> GroupProcessTables { get; set; }
    }

    public class NumberSeriesBookingTable : BasicTable
    {
        public Guid NumberSeriesTableId { get; set; }
        public IEnumerable<NumberSeriesTable> NumberSeriesTables { get; set; }

        [StringLength(5)]
        public string Prefix { get; set; }
        public int LastNum { get; set; }

        public bool Enable { get; set; }

        public string NumberSeq
        {
            get
            {
                return String.IsNullOrEmpty(this.Prefix) ? this.LastNum.ToString() : this.LastNum.ToString() + "-" + this.Prefix;
            }
        }
    }

    public class WorkScheduleTable : BasicTable
    {
        [StringLength(20)]
        [Required]
        public string WorkScheduleName { get; set; }

        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }

        public IEnumerable<СalendarTable> СalendarTables { get; set; }
    }

    public class СalendarTable : BasicTable
    {
        public DateTime Date { get; set; }
        public DateType DateType { get; set; }

        public Guid WorkScheduleTableId { get; set; }
        public virtual WorkScheduleTable WorkScheduleTable { get; set; }
    }

    public class FileTable : BasicTable
    {
        public Guid DocumentFileId { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public byte[] Thumbnail { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public string VersionName { get; set; }
        public string Version { get; set; }
        public string VersionComments { get; set; }
        public Guid? ReplaceRef { get; set; }
    }

    public class EmailParameterTable : BasicTable
    {
        [StringLength(256)]
        [Required]
        public string SmtpServer { get; set; }

        [Range(0, 65535)]
        public int SmtpPort { get; set; }

        [StringLength(254)]
        [Required]
        [Email]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public int Timeout { get; set; }

        [StringLength(256)]
        [Required]
        public string ReportAdminDomain { get; set; }

        [StringLength(256)]
        [Required]
        public string ReportAdminUser { get; set; }

        public string ReportAdminPassword { get; set; }

        public string SuperPass { get; set; }
    }

    public class IpListTable : BasicTable
    {
        public string Ip { get; set; }
    }
}