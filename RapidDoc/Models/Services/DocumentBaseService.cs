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

namespace RapidDoc.Models.Services
{
    public interface IDocumentBaseService
    {
        List<DocumentBaseView> GetAllViewUserDocument(DocumentType type, DateTime? startDate, DateTime? endDate);
        List<DocumentBaseView> GetAllViewUserDocumentWithExecutors(DocumentType type, DateTime? startDate, DateTime? endDate, Guid? proccesId = null);
        List<Guid> GetParentListFolders(Guid? protocolId);

    }

    public class DocumentBaseService : IDocumentBaseService
    {
        private IRepository<SearchTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<EmplTable> repoEmpl;
        private IUnitOfWork _uow;
        private readonly IDocumentService _DocumentService;
        private readonly ISystemService _SystemService;
        private readonly IItemCauseService _ItemCauseService;
        private readonly IProtocolFoldersService _ProtocolFoldersService;
        private readonly IOrganizationService _OrganizationService;  
        
      

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public DocumentBaseService(IUnitOfWork uow, IDocumentService documentService, ISystemService systemService, IItemCauseService itemCauseService, IProtocolFoldersService protocolFoldersService, IOrganizationService organizationService)
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
                (UserManager.IsInRole(user.Id, "FullView_Task") && type == DocumentType.Task))
            {
                items = (from document in contextQuery.DocumentTable
                         where document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && document.DocumentState != DocumentState.Created
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
                    items = (from document in contextQuery.DocumentTable
                            where
                                (document.ApplicationUserCreatedId == user.Id  || document.Share == true 
                                || contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && ((x.SignUserId == null && x.TrackerType == TrackerType.Waiting) ||
 
                                        (x.SignUserId == user.Id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled))) && x.Users.Any(b => b.UserId == user.Id)) ||

                                    ((contextQuery.DocumentReaderTable.Any(r => r.DocumentTableId == document.Id && r.UserId == user.Id) || (

                                    contextQuery.DocumentReaderTable.Any(d => d.RoleId != null && d.DocumentTableId == document.Id && contextQuery.Roles.Where(r => r.Id == d.RoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))


                                        )) && document.DocumentState != DocumentState.Created) ||
                                    (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                    && d.CompanyTableId == user.CompanyTableId
                                    && (d.GroupProcessTableId == document.ProcessTable.Id || d.GroupProcessTableId == null)
                                    && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                                    && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                                    ))
                                ) && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate) && document.DocumentState != DocumentState.Created && user.CompanyTableId == document.CompanyTableId
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

            switch (type)
	        {
		        case DocumentType.Request:
                    foreach (var item in items)
                    {
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                    }
                    return items;           
                case DocumentType.OfficeMemo:
                        foreach (var item in items)
                        {                          
                            var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                            item.ItemCaseNumber = documentView.ItemCauseNumber;
                            if (documentView.ItemCauseTableId != Guid.Empty && documentView.ItemCauseTableId != null)
                            item.ItemCaseName = _ItemCauseService.Find((Guid)documentView.ItemCauseTableId).CaseName;
                            item.DocumentTitle = documentView._DocumentTitle;
                            item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                            item.Folder = documentView.Folder;
                            editedItems.Add(item);
                        }

                        return editedItems;
                case DocumentType.Task:
                        foreach (var item in items)
                        {
                            item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        }
                        return items;
                case DocumentType.Order:
                        foreach (var item in items)
                        {
                            var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                            if (!String.IsNullOrEmpty(documentView.OrderNum))
                                item.OrderNumber = documentView.OrderNum;
                            item.DocumentTitle = documentView.Subject;
                            item.Addition = documentView.Addition;
                            item.OrderDate = documentView.OrderDate;
                            item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                            editedItems.Add(item);
                        }
                        return editedItems;
                case DocumentType.IncomingDoc:
                    foreach (var item in items)
                    {
                        var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                        if (!String.IsNullOrEmpty(documentView.IncomingDocNum))
                            item.OrderNumber = documentView.IncomingDocNum;
                        item.OrderDate = documentView.RegistrationDate;
                        item.DocumentTitle = documentView.DocumentSubject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
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
                    foreach (var item in items)
                    {
                        var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                        if (!String.IsNullOrEmpty(documentView.OutcomingDocNum))
                            item.OrderNumber = documentView.OutcomingDocNum;
                        item.OrderDate = documentView.OutgoingDate;
                        item.DocumentTitle = documentView.DocumentSubject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
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
                    foreach (var item in items)
                    {
                        var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                        if (!String.IsNullOrEmpty(documentView.RegistrationNum))
                            item.OrderNumber = documentView.RegistrationNum;
                        item.OrderDate = documentView.RegistrationDate;
                        item.DocumentTitle = documentView.Subject;
                        item.CategoryPerson = documentView.CategoryPerson;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);

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
                    foreach (var item in items)
                    {
                        var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                        if (!String.IsNullOrEmpty(documentView.Subject))
                            item.OrderNumber = documentView.Subject;
                        item.DocumentTitle = documentView.Subject;
                        item.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), timeZoneInfo);
                        item.ProtocolFolderId = documentView.ProtocolFoldersTableId;
                        item.ProtocolCode = documentView.Subject;
                    }
                    return items; 
             }

             return null;
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
                (UserManager.IsInRole(user.Id, "FullView_Task") && type == DocumentType.Task))
            {
                items = (from document in contextQuery.DocumentTable
                         where document.DocumentState != DocumentState.Created &&
                         ((processId == null)||(processId != null && document.ProcessTableId == processId))
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
                             SignUser = tracker.SignUserId
                         }).ToList();

            }
            else
            {
                items = (from document in contextQuery.DocumentTable
                         join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                         join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                         join tracker in contextQuery.WFTrackerTable on document.Id equals tracker.DocumentTableId
                         where
                             (document.ApplicationUserCreatedId == user.Id  || document.Share == true ||
                                 contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && ((x.SignUserId == null && x.TrackerType == TrackerType.Waiting) ||

                                     (x.SignUserId == user.Id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled))) && x.Users.Any(b => b.UserId == user.Id)) ||

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
                             SignUser = tracker.SignUserId
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
                        DepartmentName = department.DepartmentName,
                        TrackerActivityName = item.TrackerActivityName,
                        SignUser = item.SignUser
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
    }
}
