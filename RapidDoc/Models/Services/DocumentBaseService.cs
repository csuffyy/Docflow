using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.CSharp.RuntimeBinder;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Interfaces;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace RapidDoc.Models.Services
{
    public interface IDocumentBaseService
    {
        List<DocumentBaseView> GetAllViewUserDocument(DocumentType type, DateTime? startDate, DateTime? endDate);
        List<DocumentBaseView> GetAllViewUserDocumentWithExecutors(DocumentType type, DateTime? startDate, DateTime? endDate, Guid? proccesId = null);
        List<Guid> GetParentListFolders(Guid? protocolId);
        List<DocumentBaseProtocolFolderView> GetProtocolFoldersForDocumentBase(DocumentType documentType, Guid? processTableId, DateTime? startDate, DateTime? endDate);

    }

    public class DocumentBaseService : IDocumentBaseService
    {
        private IRepository<SearchTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<EmplTable> repoEmpl;
        private IUnitOfWork _uow;
        private readonly IDocumentService _DocumentService;
        private readonly ISystemService _SystemService;
        private readonly IGroupProcessService _GroupProcessService;
        private readonly ICompanyService _CompanyService;
        private readonly IItemCauseService _ItemCauseService;
        private readonly IProtocolFoldersService _ProtocolFoldersService;
        private readonly IOrganizationService _OrganizationService;  

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public DocumentBaseService(IUnitOfWork uow, IDocumentService documentService, ISystemService systemService, IItemCauseService itemCauseService, IProtocolFoldersService protocolFoldersService, IOrganizationService organizationService, IGroupProcessService groupProcessService, ICompanyService companyService)
        {
            _uow = uow;
            repo = uow.GetRepository<SearchTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoEmpl = uow.GetRepository<EmplTable>();
            _DocumentService = documentService;
            _SystemService = systemService;
            _ItemCauseService = itemCauseService;
            _ProtocolFoldersService = protocolFoldersService;
            _OrganizationService = organizationService;
            _GroupProcessService = groupProcessService;
            _CompanyService = companyService;

            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_uow.GetDbContext<ApplicationDbContext>()));
        }

        public List<DocumentBaseView> GetAllViewUserDocument(DocumentType type, DateTime? startDate, DateTime? endDate)
        {
            List<DocumentBaseView> items, editedItems = new List<DocumentBaseView>();

            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DateTime currentDate = DateTime.UtcNow;

            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            if (UserManager.IsInRole(user.Id, "Administrator") ||
                (UserManager.IsInRole(user.Id, "FullView_Request") && type == DocumentType.Request) ||
                (UserManager.IsInRole(user.Id, "FullView_Appeal") && type == DocumentType.AppealDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_Incoming") && type == DocumentType.IncomingDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_OfficeMemo") && type == DocumentType.OfficeMemo) ||
                (UserManager.IsInRole(user.Id, "FullView_Order") && type == DocumentType.Order) ||
                (UserManager.IsInRole(user.Id, "FullView_Outcoming") && type == DocumentType.OutcomingDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_Protocol") && type == DocumentType.Protocol) ||
                (UserManager.IsInRole(user.Id, "FullView_Task") && type == DocumentType.Task) ||
                (UserManager.IsInRole(user.Id, "FullView_Discussion") && type == DocumentType.Discussion))
            {
                items = (from document in contextQuery.DocumentTable
                         where document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && document.DocumentState != DocumentState.Created && user.CompanyTableId == document.CompanyTableId
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            join department in contextQuery.DepartmentTable on empl.DepartmentTableId equals department.Id
                            orderby document.ModifiedDate descending
                            select new DocumentBaseView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName,
                                DepartmentName = department.DepartmentName,
                                UserName = empl.SecondName + " " + empl.FirstName + " " + empl.MiddleName,
                                DocumentRefId = document.RefDocumentId,
                                ProcessTableName = process.TableName,
                                Cancel = document.Cancel,
                                Executed = document.Executed,
                                DocumentText = document.DocumentText
                            }).ToList();
            }
            else
            {
                var delegations = (from delegation in contextQuery.DelegationTable
                                   join emplTo in contextQuery.EmplTable on delegation.EmplTableToId equals emplTo.Id
                                   where delegation.DateFrom <= currentDate && delegation.DateTo >= currentDate && delegation.isArchive == false
                                   && delegation.CompanyTableId == user.CompanyTableId && emplTo.ApplicationUserId == user.Id
                                   select delegation).ToList();

                List<Guid> childGroup = new List<Guid>();

                foreach (var item in delegations.Where(x => x.GroupProcessTableId != null))
                {
                    childGroup.AddRange(_GroupProcessService.GetGroupChildren(item.GroupProcessTableId));
                    childGroup.Add((Guid)item.GroupProcessTableId);
                }

                var childGroupArray = childGroup.Distinct().ToArray();

                List<Guid> documentAccessList = new List<Guid>();
                documentAccessList.AddRange(GetAccessDocumentsList(contextQuery, user, type, startDate, endDate));

                if (DocumentType.IncomingDoc == type)
                {
                    var taskDocumentAccessList = GetAccessDocumentsList(contextQuery, user, DocumentType.Task, DateTime.MinValue, DateTime.MaxValue);
                    var documents = _DocumentService.GetPartial(x => x.DocType == type && x.DocumentState != DocumentState.Created && (x.CreatedDate >= startDate && x.CreatedDate <= endDate) && user.CompanyTableId == x.CompanyTableId).Select(x => x.Id).ToArray();
                    var taskList = contextQuery.USR_TAS_DailyTasks_Table.Where(x => documents.Contains(x.RefDocumentId.Value)).ToList();

                    foreach (var item in taskList)
                    {
                        if (taskDocumentAccessList.Contains(item.DocumentTableId))
                        {
                            documentAccessList.Add(item.RefDocumentId.Value);
                        }
                    }
                }

                if (delegations.Count() > 0)
                {
                    documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                                where (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                                && d.CompanyTableId == user.CompanyTableId
                                                && document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId
                                                && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                                                && (d.ProcessTableId == null || d.ProcessTableId == document.ProcessTableId)
                                                && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))))
                                                select document.Id);
                }

                documentAccessList.Distinct();
                var documentAccessListArray = documentAccessList.ToArray();

                items = (from document in contextQuery.DocumentTable
                         where documentAccessListArray.Contains(document.Id) && document.DocType == type
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            join department in contextQuery.DepartmentTable on empl.DepartmentTableId equals department.Id
                            orderby String.IsNullOrEmpty(document.ActivityName), document.ModifiedDate descending
                            select new DocumentBaseView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName,
                                DepartmentName = department.DepartmentName,
                                UserName = empl.SecondName + " " + empl.FirstName + " " + empl.MiddleName,
                                DocumentRefId = document.RefDocumentId,
                                ProcessTableName = process.TableName,
                                Cancel = document.Cancel,
                                Executed = document.Executed,
                                DocumentText = document.DocumentText
                            }).ToList();
            }

            List<ItemCauseTable> itemCaseList = new List<ItemCauseTable>();

            switch (type)
	        {
		        case DocumentType.Request:
                    foreach (var item in items)
                    {
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.CreatedDate.Month);
                        item.MonthNumber = item.CreatedDate.Month;
                        item.Year = item.CreatedDate.Year.ToString();
                    }
                    return items;           
                case DocumentType.OfficeMemo:
                        itemCaseList.AddRange(_ItemCauseService.GetAllIntercompany().ToList());

                        List<BasicDocumantOfficeMemoTable> documentFetchOfficeMemo = new List<BasicDocumantOfficeMemoTable>();
                        foreach (var process in items.GroupBy(x => x.ProcessTableName))
                            documentFetchOfficeMemo.AddRange((IEnumerable<BasicDocumantOfficeMemoTable>)_DocumentService.GetDocumentAll(process.Key));

                        foreach (var item in items)
                        {
                            var documentView = documentFetchOfficeMemo.FirstOrDefault(x => x.Id == item.DocumentRefId);
                            item.ItemCaseNumber = documentView.ItemCauseNumber;
                            if (documentView.ItemCauseTableId != Guid.Empty && documentView.ItemCauseTableId != null)
                                item.ItemCaseName = itemCaseList.FirstOrDefault(x => x.Id == (Guid)documentView.ItemCauseTableId).CaseName;
                            item.DocumentTitle = documentView._DocumentTitle;
                            item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                            item.ModCreatedDate = item.CreatedDate.ToShortDateString();
                            item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.CreatedDate.Month);
                            item.MonthNumber = item.CreatedDate.Month;
                            item.Year = item.CreatedDate.Year.ToString();
                            item.Folder = documentView.Folder;
                            editedItems.Add(item);
                        }

                        return editedItems;
                case DocumentType.Task:
                        foreach (var item in items)
                        {                         
                            item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                            item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.CreatedDate.Month);
                            item.MonthNumber = item.CreatedDate.Month;
                            item.Year = item.CreatedDate.Year.ToString();
                        }
                        return items;
                case DocumentType.Order:
                        List<BasicOrderTable> documentFetch = new List<BasicOrderTable>();
                        foreach (var process in items.GroupBy(x => x.ProcessTableName))
                            documentFetch.AddRange((IEnumerable<BasicOrderTable>)_DocumentService.GetDocumentAll(process.Key));

                        foreach (var item in items)
                        {
                            var documentView = documentFetch.FirstOrDefault(x => x.Id == item.DocumentRefId);
                            if (!String.IsNullOrEmpty(documentView.OrderNum))
                                item.OrderNumber = documentView.OrderNum;
                            item.DocumentTitle = documentView.Subject;
                            item.Addition = documentView.Addition;
                            item.OrderDate = documentView.OrderDate;
                            item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                            item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.OrderDate.Value.Month);
                            item.MonthNumber = item.OrderDate.Value.Month;
                            item.Year = item.OrderDate.Value.Year.ToString();
                            item.CompanyName = _CompanyService.GetCompanyName(_SystemService.GuidNull2Guid(item.CompanyTableId), item.OrderDate);
                            editedItems.Add(item);
                        }

                        return editedItems;
                case DocumentType.IncomingDoc:
                    itemCaseList.AddRange(_ItemCauseService.GetAllIntercompany().ToList());

                    List<BasicIncomingDocumentsTable> documentFetchIncoming = new List<BasicIncomingDocumentsTable>();
                        foreach (var process in items.GroupBy(x => x.ProcessTableName))
                            documentFetchIncoming.AddRange((IEnumerable<BasicIncomingDocumentsTable>)_DocumentService.GetDocumentAll(process.Key));

                    foreach (var item in items)
                    {
                        var documentView = documentFetchIncoming.FirstOrDefault(x => x.Id == item.DocumentRefId);
                        if (!String.IsNullOrEmpty(documentView.IncomingDocNum))
                            item.OrderNumber = documentView.IncomingDocNum;
                        item.OrderDate = documentView.RegistrationDate;
                        item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.OrderDate.Value.Month);
                        item.MonthNumber = item.OrderDate.Value.Month;
                        item.Year = item.OrderDate.Value.Year.ToString();
                        item.ItemCaseNumber = documentView.ItemCauseNumber;
                        if (documentView.ItemCauseTableId != Guid.Empty && documentView.ItemCauseTableId != null)
                            item.ItemCaseName = itemCaseList.FirstOrDefault(x => x.Id == (Guid)documentView.ItemCauseTableId).CaseName;
                        item.DocumentTitle = documentView.DocumentSubject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.CompanyName = _CompanyService.GetCompanyName(_SystemService.GuidNull2Guid(item.CompanyTableId), item.CreatedDate);
                        if (documentView.OrganizationTableId != null)
                        {
                            Guid organizationId = (Guid)documentView.OrganizationTableId;
                            item.InOutOrganization = _OrganizationService.FirstOrDefault(x => x.Id == organizationId).OrgName;
                        }
                        else
                            item.InOutOrganization = "Не указан";
                    }
                    return items;
                case DocumentType.OutcomingDoc:
                    itemCaseList.AddRange(_ItemCauseService.GetAllIntercompany().ToList());

                    foreach (var item in items)
                    {
                        var documentView = _DocumentService.GetDocument(item.DocumentRefId, item.ProcessTableName);
                        if (!String.IsNullOrEmpty(documentView.OutcomingDocNum))
                            item.OrderNumber = documentView.OutcomingDocNum;

                        if (documentView.DispatchType != null && !String.IsNullOrEmpty(documentView.DispatchType))
                        {
                            string dispatchType = documentView.DispatchType;
                            item.OutcomingDispatchType = dispatchType.ToString().Trim().ToUpper();
                        }
                        else
                            item.OutcomingDispatchType = (documentView.OutcomingDispatchType.GetType().GetMember(documentView.OutcomingDispatchType.ToString())[0].GetCustomAttributes(typeof(DisplayAttribute), false)[0].GetName()).ToString().ToUpper();

                        item.OrderDate = documentView.OutgoingDate;
                        item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.OrderDate.Value.Month);
                        item.MonthNumber = item.OrderDate.Value.Month;
                        item.Year = item.OrderDate.Value.Year.ToString();
                        item.ItemCaseNumber = documentView.ItemCauseNumber;
                        if (documentView.ItemCauseTableId != Guid.Empty && documentView.ItemCauseTableId != null)
                            item.ItemCaseName = itemCaseList.FirstOrDefault(x => x.Id == (Guid)documentView.ItemCauseTableId).CaseName;
                        item.DocumentTitle = documentView.DocumentSubject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.CompanyName = _CompanyService.GetCompanyName(_SystemService.GuidNull2Guid(item.CompanyTableId), item.CreatedDate);
                        if (documentView.OrganizationTableId != null)
                        {
                            Guid organizationId = (Guid)documentView.OrganizationTableId;
                            item.InOutOrganization = _OrganizationService.FirstOrDefault(x => x.Id == organizationId).OrgName;
                        }
                        else
                            item.InOutOrganization = "Не указан";
                    }
                    return items;
                case DocumentType.AppealDoc:
                    itemCaseList.AddRange(_ItemCauseService.GetAllIntercompany().ToList());

                    List<BasicAppealDocumentsTable> documentFetchAppeal = new List<BasicAppealDocumentsTable>();
                        foreach (var process in items.GroupBy(x => x.ProcessTableName))
                            documentFetchAppeal.AddRange((IEnumerable<BasicAppealDocumentsTable>)_DocumentService.GetDocumentAll(process.Key));

                    foreach (var item in items)
                    {
                        var documentView = documentFetchAppeal.FirstOrDefault(x => x.Id == item.DocumentRefId);
                        if (!String.IsNullOrEmpty(documentView.RegistrationNum))
                            item.OrderNumber = documentView.RegistrationNum;
                        item.OrderDate = documentView.RegistrationDate;
                        item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.OrderDate.Value.Month);
                        item.MonthNumber = item.OrderDate.Value.Month;
                        item.Year = item.OrderDate.Value.Year.ToString();
                        item.ItemCaseNumber = documentView.ItemCauseNumber;
                        if (documentView.ItemCauseTableId != Guid.Empty && documentView.ItemCauseTableId != null)
                            item.ItemCaseName = itemCaseList.FirstOrDefault(x => x.Id == (Guid)documentView.ItemCauseTableId).CaseName;
                        item.DocumentTitle = documentView.Subject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.CompanyName = _CompanyService.GetCompanyName(_SystemService.GuidNull2Guid(item.CompanyTableId), item.CreatedDate);

                        if (documentView.OrganizationTableId != null)
                        {
                            Guid organizationId = (Guid)documentView.OrganizationTableId;
                            item.InOutOrganization = _OrganizationService.FirstOrDefault(x => x.Id == organizationId).OrgName;
                        }
                        else
                            item.InOutOrganization = "Не указан";
                    }
                    return items;
                case DocumentType.Protocol:
                    List<BasicProtocolDocumentsTable> documentFetchProtocol = new List<BasicProtocolDocumentsTable>();
                        foreach (var process in items.GroupBy(x => x.ProcessTableName))
                            documentFetchProtocol.AddRange((IEnumerable<BasicProtocolDocumentsTable>)_DocumentService.GetDocumentAll(process.Key));

                    foreach (var item in items)
                    {
                        var documentView = documentFetchProtocol.FirstOrDefault(x => x.Id == item.DocumentRefId);
                        if (!String.IsNullOrEmpty(documentView.Subject))
                            item.OrderNumber = documentView.Subject;
                        item.DocumentTitle = documentView.Subject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.CreatedDate.Month);
                        item.MonthNumber = item.CreatedDate.Month;
                        item.Year = item.CreatedDate.Year.ToString();
                        item.ModCreatedDate = item.CreatedDate.ToShortDateString();
                        item.ProtocolFolderId = documentView.ProtocolFoldersTableId;
                        item.ProtocolCode = documentView.Subject;
                    }
                    return items;
                case DocumentType.Discussion:
                    List<BasicDailyDiscussionTable> documentFetchDiscussion = new List<BasicDailyDiscussionTable>();
                    foreach (var process in items.GroupBy(x => x.ProcessTableName))
                        documentFetchDiscussion.AddRange((IEnumerable<BasicDailyDiscussionTable>)_DocumentService.GetDocumentAll(process.Key));

                    foreach (var item in items)
                    {
                        var documentView = documentFetchDiscussion.FirstOrDefault(x => x.Id == item.DocumentRefId);
                        item.DocumentTitle = documentView.Subject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.CreatedDate.Month);
                        item.MonthNumber = item.CreatedDate.Month;
                        item.Year = item.CreatedDate.Year.ToString();
                        item.ModCreatedDate = item.CreatedDate.ToShortDateString();
                        item.ProtocolFolderId = documentView.ProtocolFoldersTableId;
                    }
                    return items; 
             }

             return null;
        }

        private List<Guid> GetAccessDocumentsList(ApplicationDbContext contextQuery, ApplicationUser user, DocumentType type, DateTime? startDate, DateTime? endDate)
        {
            List<Guid> documentAccessList = new List<Guid>();

            documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                        where (document.ApplicationUserCreatedId == user.Id || document.Share == true) && document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId
                                        select document.Id);

            documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                        join tracker in contextQuery.WFTrackerTable on document.Id equals tracker.DocumentTableId
                                        where (tracker.Users.Any(x => x.UserId == user.Id) || tracker.SignUserId == user.Id) && document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId
                                        select document.Id);

            documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                        join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                                        join role in contextQuery.Roles on process.StartReaderRoleId equals role.Id
                                        where (process.StartReaderRoleId != null && role.Users.Any(x => x.UserId == user.Id)) && document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId
                                        select document.Id);

            documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                        join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                                        join role in contextQuery.Roles on process.DocumentBaseRoleId equals role.Id
                                        where (process.DocumentBaseRoleId != null && role.Users.Any(x => x.UserId == user.Id)) && document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId
                                        select document.Id);

            documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                        join reader in contextQuery.DocumentReaderTable on document.Id equals reader.DocumentTableId
                                        where document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId && reader.UserId == user.Id
                                        select document.Id);

            documentAccessList.AddRange(from document in contextQuery.DocumentTable
                                        join reader in contextQuery.DocumentReaderTable on document.Id equals reader.DocumentTableId
                                        join role in contextQuery.Roles on reader.RoleId equals role.Id
                                        where document.DocumentState != DocumentState.Created && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && user.CompanyTableId == document.CompanyTableId && reader.RoleId != null && role.Users.Any(x => x.UserId == user.Id)
                                        select document.Id);

            return documentAccessList;
        }

        public List<DocumentBaseView> GetAllViewUserDocumentWithExecutors(DocumentType type, DateTime? startDate, DateTime? endDate, Guid? processId = null)
        {
            List<DocumentBaseView> items, editedItems = new List<DocumentBaseView>();

            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            DateTime currentDate = DateTime.UtcNow;

            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            if (UserManager.IsInRole(user.Id, "Administrator") ||
                (UserManager.IsInRole(user.Id, "FullView_Request") && type == DocumentType.Request) ||
                (UserManager.IsInRole(user.Id, "FullView_Appeal") && type == DocumentType.AppealDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_Incoming") && type == DocumentType.IncomingDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_OfficeMemo") && type == DocumentType.OfficeMemo) ||
                (UserManager.IsInRole(user.Id, "FullView_Order") && type == DocumentType.Order) ||
                (UserManager.IsInRole(user.Id, "FullView_Outcoming") && type == DocumentType.OutcomingDoc) ||
                (UserManager.IsInRole(user.Id, "FullView_Protocol") && type == DocumentType.Protocol) ||
                (UserManager.IsInRole(user.Id, "FullView_Task") && type == DocumentType.Task) ||
                (UserManager.IsInRole(user.Id, "FullView_Discussion") && type == DocumentType.Discussion))
            {
                items = (from document in contextQuery.DocumentTable
                         where document.DocumentState != DocumentState.Created && user.CompanyTableId == document.CompanyTableId &&
                         ((processId == null)||(processId != null && document.ProcessTableId == processId))
                         let trackerLast = contextQuery.WFTrackerTable.Where(p => p.DocumentTableId == document.Id).OrderByDescending(p => p.LineNum).FirstOrDefault()
                         join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                         join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                         join tracker in contextQuery.WFTrackerTable on document.Id equals tracker.DocumentTableId
                         where document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate)
                         orderby document.ModifiedDate descending
                         select new DocumentBaseView
                         {
                             ActivityName = document.ActivityName,
                             ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                             ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                             CompanyTableId = document.CompanyTableId,
                             CreatedDate = document.CreatedDate,
                             Id = document.Id,
                             DocumentNum = document.DocumentNum,
                             DocumentState = document.DocumentState,
                             ProcessName = process.ProcessName,
                             Users = tracker.Users,
                             WFTrackerType = tracker.TrackerType,
                             ModifiedDate = document.ModifiedDate,
                             ProcessTableId = document.ProcessTableId,
                             AliasCompanyName = company.AliasCompanyName,
                             DocumentRefId = document.RefDocumentId,
                             ProcessTableName = process.TableName,
                             Cancel = document.Cancel,
                             Executed = document.Executed,
                             DocumentText = document.DocumentText,
                             TrackerActivityName = tracker.ActivityName,
                             SignUser = tracker.SignUserId,
                             Delegation = tracker.LineNum != trackerLast.LineNum
                         }).ToList();

            }
            else
            {
                items = (from document in contextQuery.DocumentTable
                         let trackerLast = contextQuery.WFTrackerTable.Where(p => p.DocumentTableId == document.Id).OrderByDescending(p => p.LineNum).FirstOrDefault()
                         join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                         join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                         join tracker in contextQuery.WFTrackerTable on document.Id equals tracker.DocumentTableId
                         where
                             (document.ApplicationUserCreatedId == user.Id  || document.Share == true ||
                                 contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && (x.SignUserId == user.Id || x.Users.Any(b => b.UserId == user.Id)))
                                 || contextQuery.ProcessTable.Any(p => p.Id == document.ProcessTableId && contextQuery.Roles.Where(pr => pr.Id == p.StartReaderRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))
                                 || contextQuery.ProcessTable.Any(p => p.Id == document.ProcessTableId && contextQuery.Roles.Where(pr => pr.Id == p.DocumentBaseRoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id))) ||


                                 ((contextQuery.DocumentReaderTable.Any(r => r.DocumentTableId == document.Id && r.UserId == user.Id) || (

                                 contextQuery.DocumentReaderTable.Any(d => d.RoleId != null && d.DocumentTableId == document.Id && contextQuery.Roles.Where(r => r.Id == d.RoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))


                                     )) && document.DocumentState != DocumentState.Created) ||
                                 (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                 && d.CompanyTableId == user.CompanyTableId
                                 && (d.GroupProcessTableId == document.ProcessTable.Id || d.GroupProcessTableId == null)
                                 && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                                 && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                                 ))
                             ) && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && document.DocumentState != DocumentState.Created && user.CompanyTableId == document.CompanyTableId &&
                         ((processId == null) || (processId != null && document.ProcessTableId == processId))
                         orderby String.IsNullOrEmpty(document.ActivityName), document.ModifiedDate descending
                         select new DocumentBaseView
                         {
                             ActivityName = document.ActivityName,
                             ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                             ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                             CompanyTableId = document.CompanyTableId,
                             CreatedDate = document.CreatedDate,
                             Id = document.Id,
                             DocumentNum = document.DocumentNum,
                             DocumentState = document.DocumentState,
                             ProcessName = process.ProcessName,
                             Users = tracker.Users,
                             WFTrackerType = tracker.TrackerType,
                             ModifiedDate = document.ModifiedDate,
                             ProcessTableId = document.ProcessTableId,
                             AliasCompanyName = company.AliasCompanyName,
                             DocumentRefId = document.RefDocumentId,
                             ProcessTableName = process.TableName,
                             Cancel = document.Cancel,
                             Executed = document.Executed,
                             DocumentText = document.DocumentText,
                             TrackerActivityName = tracker.ActivityName,
                             SignUser = tracker.SignUserId,
                             Delegation = tracker.LineNum != trackerLast.LineNum
                         }).ToList();
            }

            List<EmplTable> emplTables = contextQuery.EmplTable.ToList();
            List<DepartmentTable> departmentTables = contextQuery.DepartmentTable.ToList();
            foreach(var item in items)
            {
                foreach(var itemUser in item.Users)
                {
                    EmplTable empl = emplTables.Where(p => p.ApplicationUserId == itemUser.UserId).OrderByDescending(p => p.Enable).FirstOrDefault();
                    DepartmentTable department = departmentTables.FirstOrDefault(x => x.Id == empl.DepartmentTableId);

                    editedItems.Add(new DocumentBaseView
                    {
                        ActivityName = item.ActivityName,
                        ApplicationUserCreatedId = item.ApplicationUserCreatedId,
                        ApplicationUserModifiedId = item.ApplicationUserModifiedId,
                        CompanyTableId = item.CompanyTableId,
                        CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo),
                        Id = item.Id,
                        DocumentNum = item.DocumentNum,
                        DocumentState = item.DocumentState,
                        ProcessName = item.ProcessName,
                        WFTrackerType = item.WFTrackerType,
                        ModifiedDate = item.ModifiedDate,
                        ProcessTableId = item.ProcessTableId,
                        AliasCompanyName = item.AliasCompanyName,
                        DocumentRefId = item.DocumentRefId,
                        ProcessTableName = item.ProcessTableName,
                        Cancel = item.Cancel,
                        Executed = item.Executed,
                        UserName = empl.FullName,
                        DocumentText = item.DocumentText,
                        DepartmentName = department != null ? department.DepartmentName : "<>",
                        TrackerActivityName = item.TrackerActivityName,
                        SignUser = item.SignUser,
                        Delegation = item.Delegation
                    });
                }
            }

            return editedItems;
        }

        public List<Guid> GetParentListFolders(Guid? protocolId)
        {
            List<Guid> listDocumentBaseProtocolFolder = new List<Guid>();
            List<Guid> listDocumentBaseProtocolFolderBuf = new List<Guid>();

            ProtocolFoldersTable documentBaseProtocolFolder = _ProtocolFoldersService.FirstOrDefault(x => x.Id == protocolId);

            if (documentBaseProtocolFolder.ProtocolFoldersParentId != null)
            {
                listDocumentBaseProtocolFolder.Add(documentBaseProtocolFolder.Id);
                listDocumentBaseProtocolFolderBuf = this.GetParentListFolders(documentBaseProtocolFolder.ProtocolFoldersParentId);
                listDocumentBaseProtocolFolder = listDocumentBaseProtocolFolder.Concat(listDocumentBaseProtocolFolderBuf).Distinct().OrderBy(x => x).ToList();
            }
            else
                listDocumentBaseProtocolFolder.Add(documentBaseProtocolFolder.Id);

            return listDocumentBaseProtocolFolder;
        }

        public List<DocumentBaseProtocolFolderView> GetProtocolFoldersForDocumentBase(DocumentType documentType, Guid? processTableId, DateTime? startDate, DateTime? endDate)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();
            List<DocumentBaseView> docBaseView;
            List<Guid> uniqueListProtocolFolders = new List<Guid>();

            List<ProtocolFoldersTable> protocolFolders = dbContext.ProtocolFoldersTable.Where(x => x.ProcessTableId == processTableId).ToList();
            docBaseView = GetAllViewUserDocument(documentType, startDate, endDate).Where(x => x.ProcessTableId == processTableId).ToList();
            List<DocumentBaseProtocolFolderView> documentBaseProtocolFolder = new List<DocumentBaseProtocolFolderView>();
            foreach (var item in docBaseView)
            {
                if (item.ProtocolFolderId != null)
                    uniqueListProtocolFolders = uniqueListProtocolFolders.Concat(GetParentListFolders(item.ProtocolFolderId)).Distinct().ToList();
            }

            foreach (var protocolId in uniqueListProtocolFolders)
            {
                ProtocolFoldersTable protocolFoldersTable = protocolFolders.FirstOrDefault(x => x.Id == protocolId);
                List<DocumentBaseView> protocolFolderDocumentBases = new List<DocumentBaseView>();
                foreach (var doc in docBaseView.Where(x => x.ProtocolFolderId == protocolId))
                {
                    protocolFolderDocumentBases.Add(doc);
                }
                documentBaseProtocolFolder.Add(new DocumentBaseProtocolFolderView { ProtocolFoldersId = protocolId, ProtocolFoldersParentId = protocolFoldersTable.ProtocolFoldersParentId, ProtocolFolderName = protocolFoldersTable.ProtocolFolderName, documentBaseList = protocolFolderDocumentBases });
            }

            return documentBaseProtocolFolder;
        }
    }
}
